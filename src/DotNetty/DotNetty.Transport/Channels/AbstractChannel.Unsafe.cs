/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Transport.Channels
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels.Sockets;

    partial class AbstractChannel<TChannel, TUnsafe>
    {
        /// <summary>
        /// <see cref="IChannelUnsafe" /> implementation which sub-classes must extend and use.
        /// </summary>
        public abstract partial class AbstractUnsafe : IChannelUnsafe
        {
            private static readonly Action<object, object> RegisterAction = (u, p) => OnRegister(u, p);

            protected TChannel _channel;
            private ChannelOutboundBuffer v_outboundBuffer;
            private IRecvByteBufAllocatorHandle _recvHandle;
            private bool _inFlush0;

            /// <summary> true if the channel has never been registered, false otherwise /// </summary>
            private bool _neverRegistered = true;

            public IRecvByteBufAllocatorHandle RecvBufAllocHandle
                => _recvHandle ??= _channel.Configuration.RecvByteBufAllocator.NewHandle();

            //public ChannelHandlerInvoker invoker() {
            //    // return the unwrapped invoker.
            //    return ((PausableChannelEventExecutor) eventLoop().asInvoker()).unwrapInvoker();
            //}
            public AbstractUnsafe() { }
            public virtual void Initialize(IChannel channel)
            {
                _channel = (TChannel)channel;
                _ = Interlocked.Exchange(ref v_outboundBuffer, new ChannelOutboundBuffer(channel));
            }

            public TChannel Channel => _channel;

            public ChannelOutboundBuffer OutboundBuffer => Volatile.Read(ref v_outboundBuffer);

            [Conditional("DEBUG")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void AssertEventLoop() => Debug.Assert(SharedConstants.False >= (uint)Volatile.Read(ref _channel.v_registered) || Volatile.Read(ref _channel.v_eventLoop).InEventLoop);

            public Task RegisterAsync(IEventLoop eventLoop)
            {
                if (eventLoop is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.eventLoop); }

                var ch = _channel;
                if (ch.IsRegistered)
                {
                    return ThrowHelper.FromInvalidOperationException_RegisteredToEventLoopAlready();
                }

                if (!ch.IsCompatible(eventLoop))
                {
                    return ThrowHelper.FromInvalidOperationException_IncompatibleEventLoopType(eventLoop);
                }

                _ = Interlocked.Exchange(ref ch.v_eventLoop, eventLoop);

                var promise = ch.NewPromise();

                if (eventLoop.InEventLoop)
                {
                    Register0(promise);
                }
                else
                {
                    try
                    {
                        eventLoop.Execute(RegisterAction, this, promise);
                    }
                    catch (Exception ex)
                    {
                        if (Logger.WarnEnabled) Logger.ForceClosingAChannel(ch, ex);
                        CloseForcibly();
                        ch._closeFuture.Complete();
                        Util.SafeSetFailure(promise, ex, Logger);
                    }
                }

                return promise.Task;
            }

            private static void OnRegister(object u, object p)
            {
                ((AbstractUnsafe)u).Register0((IPromise)p);
            }

            void Register0(IPromise promise)
            {
                var ch = _channel;
                try
                {
                    // check if the channel is still open as it could be closed input the mean time when the register
                    // call was outside of the eventLoop
                    if (!promise.SetUncancellable() || !EnsureOpen(promise))
                    {
                        Util.SafeSetFailure(promise, ThrowHelper.GetClosedChannelException(), Logger);
                        return;
                    }
                    bool firstRegistration = _neverRegistered;
                    ch.DoRegister();
                    _neverRegistered = false;
                    _ = Interlocked.Exchange(ref ch.v_registered, SharedConstants.True);

                    // Ensure we call handlerAdded(...) before we actually notify the promise. This is needed as the
                    // user may already fire events through the pipeline in the ChannelFutureListener.
                    ch._pipeline.InvokeHandlerAddedIfNeeded();

                    Util.SafeSetSuccess(promise, Logger);
                    _ = ch._pipeline.FireChannelRegistered();
                    // Only fire a channelActive if the channel has never been registered. This prevents firing
                    // multiple channel actives if the channel is deregistered and re-registered.
                    if (ch.IsActive)
                    {
                        if (firstRegistration)
                        {
                            _ = ch._pipeline.FireChannelActive();
                        }
                        else if (ch.Configuration.IsAutoRead)
                        {
                            // This channel was registered before and autoRead() is set. This means we need to begin read
                            // again so that we process inbound data.
                            //
                            // See https://github.com/netty/netty/issues/4805
                            BeginRead();
                        }
                    }
                }
                catch (Exception t)
                {
                    // Close the channel directly to avoid FD leak.
                    CloseForcibly();
                    ch._closeFuture.Complete();
                    Util.SafeSetFailure(promise, t, Logger);
                }
            }

            public Task BindAsync(EndPoint localAddress)
            {
                AssertEventLoop();

                var ch = _channel;
                if ( /*!promise.setUncancellable() || */!ch.IsOpen)
                {
                    return CreateClosedChannelExceptionTask();
                }

                //// See: https://github.com/netty/netty/issues/576
                //if (bool.TrueString.Equals(this.channel.Configuration.getOption(ChannelOption.SO_BROADCAST)) &&
                //    localAddress is IPEndPoint &&
                //    !((IPEndPoint)localAddress).Address.getAddress().isAnyLocalAddress() &&
                //    !Environment.OSVersion.Platform == PlatformID.Win32NT && !Environment.isRoot())
                //{
                //    // Warn a user about the fact that a non-root user can't receive a
                //    // broadcast packet on *nix if the socket is bound on non-wildcard address.
                //    logger.Warn(
                //        "A non-root user can't receive a broadcast packet if the socket " +
                //            "is not bound to a wildcard address; binding to a non-wildcard " +
                //            "address (" + localAddress + ") anyway as requested.");
                //}

                bool wasActive = ch.IsActive;
                try
                {
                    ch.DoBind(localAddress);
                }
                catch (Exception t)
                {
                    CloseIfClosed();
                    return TaskUtil.FromException(t);
                }

                if (!wasActive && ch.IsActive)
                {
                    InvokeLater(() => ch._pipeline.FireChannelActive());
                }

                return TaskUtil.Completed;
            }

            public abstract Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

            public void Disconnect(IPromise promise)
            {
                AssertEventLoop();

                if (!promise.SetUncancellable()) { return; }

                var ch = _channel;
                bool wasActive = ch.IsActive;
                try
                {
                    ch.DoDisconnect();
                    // Reset remoteAddress and localAddress
                    _ = Interlocked.Exchange(ref ch.v_remoteAddress, null);
                    _ = Interlocked.Exchange(ref ch.v_localAddress, null);
                }
                catch (Exception t)
                {
                    Util.SafeSetFailure(promise, t, Logger);
                    CloseIfClosed();
                    return;
                }

                if (wasActive && !ch.IsActive)
                {
                    InvokeLater(() => ch._pipeline.FireChannelInactive());
                }

                Util.SafeSetSuccess(promise, Logger);

                CloseIfClosed(); // doDisconnect() might have closed the channel
            }

            public void Close(IPromise promise) /*CancellationToken cancellationToken) */
            {
                AssertEventLoop();

                Close(promise, CloseClosedChannelException, CloseClosedChannelException, false);
            }

            /// <summary>
            /// Shutdown the output portion of the corresponding <see cref="IChannel"/>.
            /// For example this will clean up the <see cref="ChannelOutboundBuffer"/> and not allow any more writes.
            /// </summary>
            /// <param name="promise"></param>
            public void ShutdownOutput(IPromise promise)
            {
                AssertEventLoop();
                ShutdownOutput(promise, null);
            }

            /// <summary>
            /// Shutdown the output portion of the corresponding <see cref="IChannel"/>.
            /// For example this will clean up the <see cref="ChannelOutboundBuffer"/> and not allow any more writes.
            /// </summary>
            /// <param name="cause">The cause which may provide rational for the shutdown.</param>
            /// <param name="promise"></param>
            public void ShutdownOutput(IPromise promise, Exception cause)
            {
                if (!promise.SetUncancellable()) { return; }

                var outboundBuffer = Interlocked.Exchange(ref v_outboundBuffer, null); // Disallow adding any messages and flushes to outboundBuffer.
                if (outboundBuffer is null)
                {
                    _ = promise.TrySetException(CloseClosedChannelException);
                    return;
                }

                var shutdownCause = ThrowHelper.GetChannelOutputShutdownException(cause);
                var closeExecutor = PrepareToClose();
                if (closeExecutor is object)
                {
                    closeExecutor.Execute(() =>
                    {
                        try
                        {
                            // Execute the shutdown.
                            _channel.DoShutdownOutput();
                            _ = promise.TryComplete();
                        }
                        catch (Exception err)
                        {
                            _ = promise.TrySetException(err);
                        }
                        finally
                        {
                            // Dispatch to the EventLoop
                            _channel.EventLoop.Execute(() => CloseOutboundBufferForShutdown(_channel._pipeline, outboundBuffer, shutdownCause));
                        }
                    });
                }
                else
                {
                    try
                    {
                        // Execute the shutdown.
                        _channel.DoShutdownOutput();
                        _ = promise.TryComplete();
                    }
                    catch (Exception err)
                    {
                        _ = promise.TrySetException(err);
                    }
                    finally
                    {
                        CloseOutboundBufferForShutdown(_channel._pipeline, outboundBuffer, shutdownCause);
                    }
                }
            }

            private void CloseOutboundBufferForShutdown(IChannelPipeline pipeline, ChannelOutboundBuffer buffer, Exception cause)
            {
                buffer.FailFlushed(cause, false);
                buffer.Close(cause, true);
                _ = pipeline.FireUserEventTriggered(ChannelOutputShutdownEvent.Instance);
            }

            protected void Close(IPromise promise, Exception cause, ClosedChannelException closeCause, bool notify)
            {
                if (!promise.SetUncancellable()) { return; }

                var ch = _channel;
                if (SharedConstants.False < (uint)Interlocked.Exchange(ref ch.v_closeInitiated, SharedConstants.True))
                {
                    var closeCompletion = ch.CloseCompletion;
                    if (closeCompletion.IsCompleted)
                    {
                        // Closed already.
                        Util.SafeSetSuccess(promise, Logger);
                    }
                    else if (!promise.IsVoid) // Only needed if no VoidChannelPromise.
                    {
                        // This means close() was called before so we just register a listener and return
                        closeCompletion.LinkOutcome(promise);
                    }
                    return;
                }

                bool wasActive = ch.IsActive;
                var outboundBuffer = Interlocked.Exchange(ref v_outboundBuffer, null); // Disallow adding any messages and flushes to outboundBuffer.
                IEventExecutor closeExecutor = PrepareToClose();
                if (closeExecutor is object)
                {
                    closeExecutor.Execute(() =>
                    {
                        try
                        {
                            // Execute the close.
                            DoClose0(promise);
                        }
                        finally
                        {
                            // Call invokeLater so closeAndDeregister is executed input the EventLoop again!
                            InvokeLater(() =>
                            {
                                if (outboundBuffer is object)
                                {
                                    // Fail all the queued messages
                                    outboundBuffer.FailFlushed(cause, notify);
                                    outboundBuffer.Close(closeCause);
                                }
                                FireChannelInactiveAndDeregister(wasActive);
                            });
                        }
                    });
                }
                else
                {
                    try
                    {
                        // Close the channel and fail the queued messages input all cases.
                        DoClose0(promise);
                    }
                    finally
                    {
                        if (outboundBuffer is object)
                        {
                            // Fail all the queued messages.
                            outboundBuffer.FailFlushed(cause, notify);
                            outboundBuffer.Close(closeCause);
                        }
                    }
                    if (_inFlush0)
                    {
                        InvokeLater(() => FireChannelInactiveAndDeregister(wasActive));
                    }
                    else
                    {
                        FireChannelInactiveAndDeregister(wasActive);
                    }
                }
            }

            void DoClose0(IPromise promise)
            {
                var ch = _channel;
                try
                {
                    ch.DoClose();
                    ch._closeFuture.Complete();
                    Util.SafeSetSuccess(promise, Logger);
                }
                catch (Exception t)
                {
                    ch._closeFuture.Complete();
                    Util.SafeSetFailure(promise, t, Logger);
                }
            }

            void FireChannelInactiveAndDeregister(bool wasActive) => Deregister(VoidPromise(), wasActive && !_channel.IsActive);

            public void CloseForcibly()
            {
                AssertEventLoop();

                try
                {
                    _channel.DoClose();
                }
                catch (Exception e)
                {
                    if (Logger.WarnEnabled) Logger.FailedToCloseAChannel(e);
                }
            }

            /// <summary>
            /// This method must NEVER be called directly, but be executed as an
            /// extra task with a clean call stack instead. The reason for this
            /// is that this method calls <see cref="IChannelPipeline.FireChannelUnregistered"/>
            /// directly, which might lead to an unfortunate nesting of independent inbound/outbound
            /// events. See the comments input <see cref="InvokeLater"/> for more details.
            /// </summary>
            public void Deregister(IPromise promise)
            {
                AssertEventLoop();

                Deregister(promise, false);
            }

            void Deregister(IPromise promise, bool fireChannelInactive)
            {
                if (!promise.SetUncancellable())
                {
                    return;
                }

                var ch = _channel;
                if (SharedConstants.False >= (uint)Volatile.Read(ref ch.v_registered))
                {
                    Util.SafeSetSuccess(promise, Logger);
                    return;
                }

                // As a user may call deregister() from within any method while doing processing in the ChannelPipeline,
                // we need to ensure we do the actual deregister operation later. This is needed as for example,
                // we may be in the ByteToMessageDecoder.callDecode(...) method and so still try to do processing in
                // the old EventLoop while the user already registered the Channel to a new EventLoop. Without delay,
                // the deregister operation this could lead to have a handler invoked by different EventLoop and so
                // threads.
                //
                // See:
                // https://github.com/netty/netty/issues/4435
                InvokeLater(() =>
                {
                    try
                    {
                        ch.DoDeregister();
                    }
                    catch (Exception t)
                    {
                        if (Logger.WarnEnabled) Logger.UnexpectedExceptionOccurredWhileDeregisteringChannel(t);
                    }
                    finally
                    {
                        if (fireChannelInactive)
                        {
                            _ = ch._pipeline.FireChannelInactive();
                        }
                        // Some transports like local and AIO does not allow the deregistration of
                        // an open channel.  Their doDeregister() calls close(). Consequently,
                        // close() calls deregister() again - no need to fire channelUnregistered, so check
                        // if it was registered.
                        if (SharedConstants.False < (uint)Volatile.Read(ref ch.v_registered))
                        {
                            _ = Interlocked.Exchange(ref ch.v_registered, SharedConstants.False);
                            _ = ch._pipeline.FireChannelUnregistered();
                        }
                        Util.SafeSetSuccess(promise, Logger);
                    }
                });
            }

            public void BeginRead()
            {
                AssertEventLoop();

                var ch = _channel;
                if (!ch.IsActive)
                {
                    return;
                }

                try
                {
                    ch.DoBeginRead();
                }
                catch (Exception e)
                {
                    InvokeLater(() => ch._pipeline.FireExceptionCaught(e));
                    Close(VoidPromise());
                }
            }

            public void Write(object msg, IPromise promise)
            {
                AssertEventLoop();

                ChannelOutboundBuffer outboundBuffer = Volatile.Read(ref v_outboundBuffer);
                if (outboundBuffer is null)
                {
                    // If the outboundBuffer is null we know the channel was closed and so
                    // need to fail the future right away. If it is not null the handling of the rest
                    // will be done input flush0()
                    // See https://github.com/netty/netty/issues/2362
                    Util.SafeSetFailure(promise, WriteClosedChannelException, Logger);
                    // release message now to prevent resource-leak
                    _ = ReferenceCountUtil.Release(msg);
                    return;
                }

                int size;
                try
                {
                    var ch = _channel;
                    msg = ch.FilterOutboundMessage(msg);
                    size = ch._pipeline.EstimatorHandle.Size(msg);
                    if ((uint)size > SharedConstants.TooBigOrNegative) // < 0
                    {
                        size = 0;
                    }
                }
                catch (Exception t)
                {
                    Util.SafeSetFailure(promise, t, Logger);
                    _ = ReferenceCountUtil.Release(msg);
                    return;
                }

                outboundBuffer.AddMessage(msg, size, promise);
            }

            public void Flush()
            {
                AssertEventLoop();

                ChannelOutboundBuffer outboundBuffer = Volatile.Read(ref v_outboundBuffer);
                if (outboundBuffer is null)
                {
                    return;
                }

                outboundBuffer.AddFlush();
                Flush0();
            }

            protected virtual void Flush0()
            {
                if (_inFlush0)
                {
                    // Avoid re-entrance
                    return;
                }

                ChannelOutboundBuffer outboundBuffer = Volatile.Read(ref v_outboundBuffer);
                if (outboundBuffer is null || outboundBuffer.IsEmpty)
                {
                    return;
                }

                _inFlush0 = true;

                var ch = _channel;
                // Mark all pending write requests as failure if the channel is inactive.
                if (!CanWrite)
                {
                    try
                    {
                        if (ch.IsOpen)
                        {
                            outboundBuffer.FailFlushed(Flush0NotYetConnectedException, true);
                        }
                        else
                        {
                            // Do not trigger channelWritabilityChanged because the channel is closed already.
                            outboundBuffer.FailFlushed(Flush0ClosedChannelException, false);
                        }
                    }
                    finally
                    {
                        _inFlush0 = false;
                    }
                    return;
                }

                try
                {
                    ch.DoWrite(outboundBuffer);
                }
                catch (Exception ex)
                {
                    if ((ex is SocketException || ex is IOException) && ch.Configuration.IsAutoClose)
                    {
                        /*
                         * Just call {@link #close(ChannelPromise, Throwable, boolean)} here which will take care of
                         * failing all flushed messages and also ensure the actual close of the underlying transport
                         * will happen before the promises are notified.
                         *
                         * This is needed as otherwise {@link #isActive()} , {@link #isOpen()} and {@link #isWritable()}
                         * may still return <c>true</c> even if the channel should be closed as result of the exception.
                         */
                        Close(VoidPromise(), ex, Flush0ClosedChannelException, false);
                    }
                    else
                    {
                        try
                        {
                            ShutdownOutput(VoidPromise(), ex);
                        }
                        catch (Exception ex2)
                        {
                            Close(VoidPromise(), ex2, Flush0ClosedChannelException, false);
                        }
                    }
                }
                finally
                {
                    _inFlush0 = false;
                }
            }

            protected virtual bool CanWrite => _channel.IsActive;

            public IPromise VoidPromise()
            {
                AssertEventLoop();
                return _channel._unsafeVoidPromise;
            }

            protected bool EnsureOpen() => _channel.IsOpen;

            protected bool EnsureOpen(IPromise promise)
            {
                if (_channel.IsOpen)
                {
                    return true;
                }

                Util.SafeSetFailure(promise, EnsureOpenClosedChannelException, Logger);
                return false;
            }

            protected Task CreateClosedChannelExceptionTask() => ThrowHelper.FromClosedChannelException();

            protected void CloseIfClosed()
            {
                if (_channel.IsOpen)
                {
                    return;
                }
                Close(VoidPromise());
            }

            void InvokeLater(Action task)
            {
                try
                {
                    // This method is used by outbound operation implementations to trigger an inbound event later.
                    // They do not trigger an inbound event immediately because an outbound operation might have been
                    // triggered by another inbound event handler method.  If fired immediately, the call stack
                    // will look like this for example:
                    //
                    //   handlerA.inboundBufferUpdated() - (1) an inbound handler method closes a connection.
                    //   -> handlerA.ctx.close()
                    //      -> channel.unsafe.close()
                    //         -> handlerA.channelInactive() - (2) another inbound handler method called while input (1) yet
                    //
                    // which means the execution of two inbound handler methods of the same handler overlap undesirably.
                    _channel.EventLoop.Execute(task);
                }
                catch (RejectedExecutionException e)
                {
                    if (Logger.WarnEnabled) Logger.CannotInvokeTaskLaterAsEventLoopRejectedIt(e);
                }
            }

            protected Exception AnnotateConnectException(Exception exception, EndPoint remoteAddress)
            {
                if (exception is SocketException)
                {
                    return new ConnectException("LogError connecting to " + remoteAddress, exception);
                }

                return exception;
            }

            /// <summary>
            /// Prepares to close the <see cref="IChannel"/>. If this method returns an <see cref="IEventExecutor"/>, the
            /// caller must call the <see cref="IExecutor.Execute(DotNetty.Common.Concurrency.IRunnable)"/> method with a task that calls
            /// <see cref="AbstractChannel{TChannel, TUnsafe}.DoClose"/> on the returned <see cref="IEventExecutor"/>. If this method returns <c>null</c>,
            /// <see cref="AbstractChannel{TChannel, TUnsafe}.DoClose"/> must be called from the caller thread. (i.e. <see cref="IEventLoop"/>)
            /// </summary>
            protected virtual IEventExecutor PrepareToClose() => null;
        }
    }
}