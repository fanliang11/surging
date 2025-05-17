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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Transport.Channels.Sockets
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;

    partial class AbstractSocketChannel<TChannel, TUnsafe>
    {
        internal interface ISocketChannelUnsafe : IChannelUnsafe
        {
            /// <summary>
            ///     Finish connect
            /// </summary>
            void FinishConnect(SocketChannelAsyncOperation<TChannel, TUnsafe> operation);

            /// <summary>
            ///     Read from underlying {@link SelectableChannel}
            /// </summary>
            void FinishRead(SocketChannelAsyncOperation<TChannel, TUnsafe> operation);

            void FinishWrite(SocketChannelAsyncOperation<TChannel, TUnsafe> operation);
        }

        public abstract class AbstractSocketUnsafe : AbstractUnsafe, ISocketChannelUnsafe
        {
            protected AbstractSocketUnsafe()
                : base()
            {
            }

            public sealed override Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
            {
                // todo: handle cancellation
                var ch = _channel;
                if (!ch.IsOpen)
                {
                    return CreateClosedChannelExceptionTask();
                }

                try
                {
                    if (ch._connectPromise is object)
                    {
                        ThrowHelper.ThrowInvalidOperationException_ConnAttemptAlreadyMade();
                    }

                    bool wasActive = _channel.IsActive;
                    if (ch.DoConnect(remoteAddress, localAddress))
                    {
                        FulfillConnectPromise(ch._connectPromise, wasActive);
                        return TaskUtil.Completed;
                    }
                    else
                    {
                        ch._connectPromise = ch.NewPromise(remoteAddress);

                        // Schedule connect timeout.
                        TimeSpan connectTimeout = ch.Configuration.ConnectTimeout;
                        if (connectTimeout > TimeSpan.Zero)
                        {
                            ch._connectCancellationTask = ch.EventLoop.Schedule(
                                ConnectTimeoutAction, _channel,
                                remoteAddress, connectTimeout);
                        }

                        _ = ch._connectPromise.Task.ContinueWith(CloseSafeOnCompleteAction, ch,
                            TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously);

                        return ch._connectPromise.Task;
                    }
                }
                catch (Exception ex)
                {
                    CloseIfClosed();
                    return TaskUtil.FromException(AnnotateConnectException(ex, remoteAddress));
                }
            }

            void FulfillConnectPromise(IPromise promise, bool wasActive)
            {
                if (promise is null)
                {
                    // Closed via cancellation and the promise has been notified already.
                    return;
                }

                var ch = _channel;

                // Get the state as trySuccess() may trigger an ChannelFutureListener that will close the Channel.
                // We still need to ensure we call fireChannelActive() in this case.
                bool active = ch.IsActive;

                // trySuccess() will return false if a user cancelled the connection attempt.
                bool promiseSet = promise.TryComplete();

                // Regardless if the connection attempt was cancelled, channelActive() event should be triggered,
                // because what happened is what happened.
                if (!wasActive && active)
                {
                    _ = ch.Pipeline.FireChannelActive();
                }

                // If a user cancelled the connection attempt, close the channel, which is followed by channelInactive().
                if (!promiseSet)
                {
                    Close(VoidPromise());
                }
            }

            void FulfillConnectPromise(IPromise promise, Exception cause)
            {
                if (promise is null)
                {
                    // Closed via cancellation and the promise has been notified already.
                    return;
                }

                // Use tryFailure() instead of setFailure() to avoid the race against cancel().
                _ = promise.TrySetException(cause);
                CloseIfClosed();
            }

            public void FinishConnect(SocketChannelAsyncOperation<TChannel, TUnsafe> operation)
            {
                var ch = _channel;
                Debug.Assert(ch.EventLoop.InEventLoop);

                try
                {
                    bool wasActive = ch.IsActive;
                    ch.DoFinishConnect(operation);
                    FulfillConnectPromise(ch._connectPromise, wasActive);
                }
                catch (Exception ex)
                {
                    var promise = ch._connectPromise;
                    var remoteAddress = (EndPoint)promise?.Task.AsyncState;
                    FulfillConnectPromise(ch._connectPromise, AnnotateConnectException(ex, remoteAddress));
                }
                finally
                {
                    // Check for null as the connectTimeoutFuture is only created if a connectTimeoutMillis > 0 is used
                    // See https://github.com/netty/netty/issues/1770
                    _ = (ch._connectCancellationTask?.Cancel());
                    ch._connectPromise = null;
                }
            }

            public abstract void FinishRead(SocketChannelAsyncOperation<TChannel, TUnsafe> operation);

            protected sealed override void Flush0()
            {
                // Flush immediately only when there's no pending flush.
                // If there's a pending flush operation, event loop will call FinishWrite() later,
                // and thus there's no need to call it now.
                if (!IsFlushPending()) { base.Flush0(); }
            }

            public void FinishWrite(SocketChannelAsyncOperation<TChannel, TUnsafe> operation)
            {
                var ch = _channel;
                bool resetWritePending = ch.TryResetState(StateFlags.WriteScheduled);

                Debug.Assert(resetWritePending);

                ChannelOutboundBuffer input = OutboundBuffer;
                try
                {
                    operation.Validate();
                    int sent = operation.BytesTransferred;
                    ch.ResetWriteOperation();
                    if (sent > 0)
                    {
                        input.RemoveBytes(sent);
                    }
                }
                catch (Exception ex)
                {
                    _ = ch.Pipeline.FireExceptionCaught(ex);
                    Close(VoidPromise(), ThrowHelper.GetClosedChannelException_FailedToWrite(ex), WriteClosedChannelException, false);
                }

                // Double check if there's no pending flush
                // See https://github.com/Azure/DotNetty/issues/218
                Flush0(); // todo: does it make sense now that we've actually written out everything that was flushed previously? concurrent flush handling?
            }

            bool IsFlushPending() => _channel.IsInState(StateFlags.WriteScheduled);
        }
    }
}