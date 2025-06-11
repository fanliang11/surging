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
    using System.Net;
    using System.Threading;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;

    /// <summary>
    /// A <see cref="IServerChannel"/> for the local transport which allows in VM communication.
    /// </summary>
    public class LocalServerChannel : AbstractServerChannel<LocalServerChannel, LocalServerChannel.LocalServerUnsafe>
    {
        private readonly IQueue<object> _inboundBuffer;
        private readonly Action _shutdownHook;

        private int v_state; // 0 - open, 1 - active, 2 - closed
        private LocalAddress v_localAddress;
        private int v_acceptInProgress;

        public LocalServerChannel()
        {
            _inboundBuffer = PlatformDependent.NewMpscQueue<object>();
            _shutdownHook = () => Unsafe.Close(Unsafe.VoidPromise());

            var config = new DefaultChannelConfiguration(this);
            config.Allocator = new PreferHeapByteBufAllocator(config.Allocator);
            Configuration = config;
        }

        public override IChannelConfiguration Configuration { get; }

        public override bool IsOpen => (uint)Volatile.Read(ref v_state) < 2u;

        public override bool IsActive => Volatile.Read(ref v_state) == 1;

        protected override EndPoint LocalAddressInternal => Volatile.Read(ref v_localAddress);

        protected override bool IsCompatible(IEventLoop eventLoop) => eventLoop is SingleThreadEventLoopBase;

        public new LocalAddress LocalAddress => (LocalAddress)base.LocalAddress;

        public new LocalAddress RemoteAddress => (LocalAddress)base.RemoteAddress;

        protected override void DoRegister() =>
            ((SingleThreadEventExecutor)EventLoop).AddShutdownHook(_shutdownHook);

        protected override void DoBind(EndPoint localAddress)
        {
            _ = Interlocked.Exchange(ref v_localAddress, LocalChannelRegistry.Register(this, Volatile.Read(ref v_localAddress), localAddress));
            _ = Interlocked.Exchange(ref v_state, 1);
        }

        protected override void DoClose()
        {
            if (Volatile.Read(ref v_state) <= 1)
            {
                // Update all internal state before the closeFuture is notified.
                var thisLocalAddr = Volatile.Read(ref v_localAddress);
                if (thisLocalAddr is object)
                {
                    LocalChannelRegistry.Unregister(thisLocalAddr);
                    _ = Interlocked.Exchange(ref v_localAddress, null);
                }
                _ = Interlocked.Exchange(ref v_state, 2);
            }
        }

        protected override void DoDeregister()
            => ((SingleThreadEventExecutor)EventLoop).RemoveShutdownHook(_shutdownHook);

        protected override void DoBeginRead()
        {
            if (SharedConstants.False < (uint)Volatile.Read(ref v_acceptInProgress))
            {
                return;
            }

            if (_inboundBuffer.IsEmpty)
            {
                _ = Interlocked.Exchange(ref v_acceptInProgress, SharedConstants.True);
                return;
            }

            ReadInbound();
        }

        public LocalChannel Serve(LocalChannel peer)
        {
            LocalChannel child = NewLocalChannel(peer);
            if (EventLoop.InEventLoop)
            {
                Serve0(child);
            }
            else
            {
                EventLoop.Execute(() => Serve0(child));
            }
            return child;
        }

        private void ReadInbound()
        {
            var handle = Unsafe.RecvBufAllocHandle;
            handle.Reset(Configuration);

            var pipeline = Pipeline;
            var inboundBuffer = _inboundBuffer;
            do
            {
                if (!inboundBuffer.TryDequeue(out object m)) { break; }
                _ = pipeline.FireChannelRead(m);
            } while (handle.ContinueReading());

            _ = pipeline.FireChannelReadComplete();
        }

        /// <summary>
        /// A factory method for <see cref="LocalChannel"/>s. Users may override it to create custom instances of <see cref="LocalChannel"/>s.
        /// </summary>
        /// <param name="peer">An existing <see cref="LocalChannel"/> that will act as a peer for the new channel.</param>
        /// <returns>The newly created <see cref="LocalChannel"/> instance.</returns>
        protected LocalChannel NewLocalChannel(LocalChannel peer) => new LocalChannel(this, peer);

        void Serve0(LocalChannel child)
        {
            _ = _inboundBuffer.TryEnqueue(child);

            if (SharedConstants.False < (uint)Volatile.Read(ref v_acceptInProgress))
            {
                _ = Interlocked.Exchange(ref v_acceptInProgress, SharedConstants.False);
                ReadInbound();
            }
        }

        public class LocalServerUnsafe : DefaultServerUnsafe { }
    }
}
