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

namespace DotNetty.Transport.Channels.Local
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// A <see cref="IChannel"/> for the local transport.
    /// </summary>
    public class LocalChannel : AbstractChannel<LocalChannel, LocalChannel.LocalUnsafe>
    {
        static class State
        {
            public const int Open = 0;
            public const int Bound = 1;
            public const int Connected = 2;
            public const int Closed = 3;
        }

        private const int MAX_READER_STACK_DEPTH = 8;
        private static readonly ChannelMetadata METADATA;
        internal static readonly ClosedChannelException DoWriteClosedChannelException;
        private static readonly ClosedChannelException DoCloseClosedChannelException;
        private static readonly Action<object> InternalReadAction;

        static LocalChannel()
        {
            METADATA = new ChannelMetadata(false);
            DoWriteClosedChannelException = new ClosedChannelException();
            DoCloseClosedChannelException = new ClosedChannelException();
            InternalReadAction = o => InternalRead(o);
        }

        // To further optimize this we could write our own SPSC queue.
        internal readonly IQueue<object> _inboundBuffer;

        private int v_state;
        private LocalChannel v_peer;
        private LocalAddress v_localAddress;
        private LocalAddress v_remoteAddress;
        private DefaultPromise v_connectPromise;
        private int v_readInProgress;
        private int v_writeInProgress;
        private Task v_finishReadFuture;

        private readonly Action shutdownHook;

        public LocalChannel()
            : this(null, null)
        {
        }

        internal LocalChannel(LocalServerChannel parent, LocalChannel peer)
            : base(parent)
        {
            _inboundBuffer = PlatformDependent.NewMpscQueue<object>();

            v_peer = peer;
            if (parent is object)
            {
                v_localAddress = parent.LocalAddress;
            }
            if (peer is object)
            {
                v_remoteAddress = peer.LocalAddress;
            }

            var config = new DefaultChannelConfiguration(this);
            config.Allocator = new PreferHeapByteBufAllocator(config.Allocator);
            Configuration = config;
            shutdownHook = () => Unsafe.Close(Unsafe.VoidPromise());
        }

        public override ChannelMetadata Metadata => METADATA;

        public override IChannelConfiguration Configuration { get; }

        public new LocalServerChannel Parent => (LocalServerChannel)base.Parent;

        public new LocalAddress LocalAddress => (LocalAddress)base.LocalAddress;

        public new LocalAddress RemoteAddress => (LocalAddress)base.RemoteAddress;

        public override bool IsOpen => Volatile.Read(ref v_state) != State.Closed;

        public override bool IsActive => Volatile.Read(ref v_state) == State.Connected;

        //protected override IChannelUnsafe NewUnsafe() => new LocalUnsafe(this);

        protected override bool IsCompatible(IEventLoop loop) => loop is SingleThreadEventLoopBase;

        protected override EndPoint LocalAddressInternal => Volatile.Read(ref v_localAddress);

        protected override EndPoint RemoteAddressInternal => Volatile.Read(ref v_remoteAddress);

        static void InternalRead(object c)
        {
            var self = (LocalChannel)c;
            // Ensure the inboundBuffer is not empty as readInbound() will always call fireChannelReadComplete()
            if (self._inboundBuffer.NonEmpty)
            {
                self.ReadInbound();
            }
        }

        protected override void DoRegister()
        {
            // Check if both peer and parent are non-null because this channel was created by a LocalServerChannel.
            // This is needed as a peer may not be null also if a LocalChannel was connected before and
            // deregistered / registered later again.
            //
            // See https://github.com/netty/netty/issues/2400
            if (Volatile.Read(ref v_peer) is object && Parent is object)
            {
                // Store the peer in a local variable as it may be set to null if doClose() is called.
                // See https://github.com/netty/netty/issues/2144
                var peer = Volatile.Read(ref v_peer);
                _ = Interlocked.Exchange(ref v_state, State.Connected);

                _ = Interlocked.Exchange(ref peer.v_remoteAddress, Parent?.LocalAddress);
                _ = Interlocked.Exchange(ref peer.v_state, State.Connected);

                // Always call peer.eventLoop().execute() even if peer.eventLoop().InEventLoop is true.
                // This ensures that if both channels are on the same event loop, the peer's channelActive
                // event is triggered *after* this channel's channelRegistered event, so that this channel's
                // pipeline is fully initialized by ChannelInitializer before any channelRead events.
                peer.EventLoop.Execute(
                    () =>
                    {
                        var promise = Volatile.Read(ref peer.v_connectPromise);

                        // Only trigger fireChannelActive() if the promise was not null and was not completed yet.
                        // connectPromise may be set to null if doClose() was called in the meantime.
                        if (promise is object && promise.TryComplete())
                        {
                            _ = peer.Pipeline.FireChannelActive();
                        }
                    }
                );
            }
            ((SingleThreadEventExecutor)EventLoop).AddShutdownHook(shutdownHook);
        }

        protected override void DoBind(EndPoint localAddress)
        {
            _ = Interlocked.Exchange(ref v_localAddress, LocalChannelRegistry.Register(this, Volatile.Read(ref v_localAddress), localAddress));
            _ = Interlocked.Exchange(ref v_state, State.Bound);
        }

        protected override void DoDisconnect() => DoClose();

        protected override void DoClose()
        {
            var peer = Volatile.Read(ref v_peer);
            var oldState = Volatile.Read(ref v_state);

            try
            {
                if (oldState != State.Closed)
                {
                    // Update all internal state before the closeFuture is notified.
                    var thisLocalAddr = Volatile.Read(ref v_localAddress);
                    if (thisLocalAddr is object)
                    {
                        if (Parent is null)
                        {
                            LocalChannelRegistry.Unregister(thisLocalAddr);
                        }
                        _ = Interlocked.Exchange(ref v_localAddress, null);
                    }

                    // State change must happen before finishPeerRead to ensure writes are released either in doWrite or
                    // channelRead.
                    _ = Interlocked.Exchange(ref v_state, State.Closed);

                    // Preserve order of event and force a read operation now before the close operation is processed.
                    if (SharedConstants.False < (uint)Volatile.Read(ref v_writeInProgress) && peer is object) { FinishPeerRead(peer); }

                    var promise = Volatile.Read(ref v_connectPromise);
                    if (promise is object)
                    {
                        // Use tryFailure() instead of setFailure() to avoid the race against cancel().
                        _ = promise.TrySetException(DoCloseClosedChannelException);
                        _ = Interlocked.Exchange(ref v_connectPromise, null);
                    }
                }

                if (peer is object)
                {
                    _ = Interlocked.Exchange(ref v_peer, null);
                    // Always call peer.eventLoop().execute() even if peer.eventLoop().inEventLoop() is true.
                    // This ensures that if both channels are on the same event loop, the peer's channelInActive
                    // event is triggered *after* this peer's channelInActive event
                    IEventLoop peerEventLoop = peer.EventLoop;
                    bool peerIsActive = peer.IsActive;
                    try
                    {
                        peerEventLoop.Execute(() => peer.TryClose(peerIsActive));
                    }
                    catch (Exception cause)
                    {
                        Logger.Warn("Releasing Inbound Queues for channels {}-{} because exception occurred!", this, peer, cause);

                        if (peerEventLoop.InEventLoop)
                        {
                            peer.ReleaseInboundBuffers();
                        }
                        else
                        {
                            // inboundBuffers is a SPSC so we may leak if the event loop is shutdown prematurely or
                            // rejects the close Runnable but give a best effort.
                            _ = peer.CloseAsync();
                        }
                        throw;
                    }
                }
            }
            finally
            {
                // Release all buffers if the Channel was already registered in the past and if it was not closed before.
                if (oldState != State.Closed)
                {
                    // We need to release all the buffers that may be put into our inbound queue since we closed the Channel
                    // to ensure we not leak any memory. This is fine as it basically gives the same guarantees as TCP which
                    // means even if the promise was notified before its not really guaranteed that the "remote peer" will
                    // see the buffer at all.
                    ReleaseInboundBuffers();
                }
            }
        }

        void TryClose(bool isActive)
        {
            if (isActive)
            {
                Unsafe.Close(Unsafe.VoidPromise());
            }
            else
            {
                ReleaseInboundBuffers();
            }
        }

        protected override void DoDeregister() => ((SingleThreadEventExecutor)EventLoop).RemoveShutdownHook(shutdownHook);

        private void ReadInbound()
        {
            var handle = Unsafe.RecvBufAllocHandle;
            handle.Reset(Configuration);

            var pipeline = Pipeline;
            var inboundBuffer = _inboundBuffer;
            do
            {
                if (!inboundBuffer.TryDequeue(out object received)) { break; }
                _ = pipeline.FireChannelRead(received);
            } while (handle.ContinueReading());

            _ = pipeline.FireChannelReadComplete();
        }

        protected override void DoBeginRead()
        {
            if (SharedConstants.False < (uint)Volatile.Read(ref v_readInProgress))
            {
                return;
            }

            if (_inboundBuffer.IsEmpty)
            {
                _ = Interlocked.Exchange(ref v_readInProgress, SharedConstants.True);
                return;
            }

            InternalThreadLocalMap threadLocals = InternalThreadLocalMap.Get();
            int stackDepth = threadLocals.LocalChannelReaderStackDepth;
            if (stackDepth < MAX_READER_STACK_DEPTH)
            {
                threadLocals.LocalChannelReaderStackDepth = stackDepth + 1;

                try
                {
                    ReadInbound();
                }
                finally
                {
                    threadLocals.LocalChannelReaderStackDepth = stackDepth;
                }
            }
            else
            {
                try
                {
                    EventLoop.Execute(InternalReadAction, this);
                }
                catch (Exception ex)
                {
                    Logger.Warn("Closing Local channels {}-{} because exception occurred!", this, Volatile.Read(ref v_peer), ex);
                    _ = CloseAsync();
                    _ = Volatile.Read(ref v_peer).CloseAsync();
                    throw;
                }
            }
        }

        protected override void DoWrite(ChannelOutboundBuffer buffer)
        {
            switch (Volatile.Read(ref v_state))
            {
                case State.Open:
                case State.Bound:
                    ThrowHelper.ThrowNotYetConnectedException(); break;
                case State.Closed:
                    ThrowHelper.ThrowDoWriteClosedChannelException(); break;
                case State.Connected:
                    break;
            }

            var peer = Volatile.Read(ref v_peer);

            _ = Interlocked.Exchange(ref v_writeInProgress, SharedConstants.True);
            try
            {
                while (true)
                {
                    object msg = buffer.Current;
                    if (msg is null)
                    {
                        break;
                    }

                    try
                    {
                        // It is possible the peer could have closed while we are writing, and in this case we should
                        // simulate real socket behavior and ensure the write operation is failed.
                        if (Volatile.Read(ref peer.v_state) == State.Connected)
                        {
                            _ = peer._inboundBuffer.TryEnqueue(ReferenceCountUtil.Retain(msg));
                            _ = buffer.Remove();
                        }
                        else
                        {
                            _ = buffer.Remove(DoWriteClosedChannelException);
                        }
                    }
                    catch (Exception cause)
                    {
                        _ = buffer.Remove(cause);
                    }
                }
            }
            finally
            {
                // The following situation may cause trouble:
                // 1. Write (with promise X)
                // 2. promise X is completed when in.remove() is called, and a listener on this promise calls close()
                // 3. Then the close event will be executed for the peer before the write events, when the write events
                // actually happened before the close event.
                _ = Interlocked.Exchange(ref v_writeInProgress, SharedConstants.False);
            }

            FinishPeerRead(peer);
        }

        void FinishPeerRead(LocalChannel peer)
        {
            // If the peer is also writing, then we must schedule the event on the event loop to preserve read order.
            if (peer.EventLoop == EventLoop && SharedConstants.False >= (uint)Volatile.Read(ref peer.v_writeInProgress))
            {
                FinishPeerRead0(peer);
            }
            else
            {
                RunFinishPeerReadTask(peer);
            }
        }

        void RunFinishPeerReadTask(LocalChannel peer)
        {
            // If the peer is writing, we must wait until after reads are completed for that peer before we can read. So
            // we keep track of the task, and coordinate later that our read can't happen until the peer is done.
            try
            {
                if (SharedConstants.False < (uint)Volatile.Read(ref peer.v_writeInProgress))
                {
                    _ = Interlocked.Exchange(ref peer.v_finishReadFuture, peer.EventLoop.SubmitAsync(
                        () =>
                        {
                            FinishPeerRead0(peer);
                            return (object)null;
                        }));
                }
                else
                {
                    peer.EventLoop.Execute(() => FinishPeerRead0(peer));
                }
            }
            catch (Exception cause)
            {
                Logger.Warn("Closing Local channels {}-{} because exception occurred!", this, peer, cause);
                _ = CloseAsync();
                _ = peer.CloseAsync();
                throw;
            }
        }

        void ReleaseInboundBuffers()
        {
            //Debug.Assert(EventLoop is null || EventLoop.InEventLoop);
            _ = Interlocked.Exchange(ref v_readInProgress, SharedConstants.False);
            var inboundBuffer = _inboundBuffer;
            while (inboundBuffer.TryDequeue(out object msg))
            {
                _ = ReferenceCountUtil.Release(msg);
            }
        }

        void FinishPeerRead0(LocalChannel peer)
        {
            Task peerFinishReadFuture = Volatile.Read(ref peer.v_finishReadFuture);
            if (peerFinishReadFuture is object)
            {
                if (!peerFinishReadFuture.IsCompleted)
                {
                    RunFinishPeerReadTask(peer);
                    return;
                }
                else
                {
                    // Lazy unset to make sure we don't prematurely unset it while scheduling a new task.
                    _ = Interlocked.CompareExchange(ref peer.v_finishReadFuture, null, peerFinishReadFuture);
                }
            }

            // We should only set readInProgress to false if there is any data that was read as otherwise we may miss to
            // forward data later on.
            IChannelPipeline peerPipeline = peer.Pipeline;
            if (SharedConstants.False < (uint)Volatile.Read(ref peer.v_readInProgress) && peer._inboundBuffer.NonEmpty)
            {
                _ = Interlocked.Exchange(ref peer.v_readInProgress, SharedConstants.False);
                peer.ReadInbound();
            }
        }

        public sealed class LocalUnsafe : AbstractUnsafe
        {
            public override Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
            {
                var promise = new DefaultPromise();

                if (Volatile.Read(ref _channel.v_state) == State.Connected)
                {
                    var cause = new AlreadyConnectedException();
                    Util.SafeSetFailure(promise, cause, Logger);
                    _ = _channel.Pipeline.FireExceptionCaught(cause);
                    return promise.Task;
                }

                if (Volatile.Read(ref _channel.v_connectPromise) is object)
                {
                    return ThrowHelper.FromConnectionPendingException();
                }

                _ = Interlocked.Exchange(ref _channel.v_connectPromise, promise);

                if (Volatile.Read(ref _channel.v_state) != State.Bound)
                {
                    // Not bound yet and no localAddress specified - get one.
                    if (localAddress is null)
                    {
                        localAddress = new LocalAddress(_channel);
                    }
                }

                if (localAddress is object)
                {
                    try
                    {
                        _channel.DoBind(localAddress);
                    }
                    catch (Exception ex)
                    {
                        Util.SafeSetFailure(promise, ex, Logger);
                        _ = _channel.CloseAsync();
                        return promise.Task;
                    }
                }

                IChannel boundChannel = LocalChannelRegistry.Get(remoteAddress);
                if (!(boundChannel is LocalServerChannel serverChannel))
                {
                    Exception cause = new ConnectException($"connection refused: {remoteAddress}", null);
                    Util.SafeSetFailure(promise, cause, Logger);
                    _ = _channel.CloseAsync();
                    return promise.Task;
                }

                _ = Interlocked.Exchange(ref _channel.v_peer, serverChannel.Serve(_channel));
                return promise.Task;
            }
        }
    }
}
