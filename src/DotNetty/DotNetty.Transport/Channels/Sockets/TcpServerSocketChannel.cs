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

namespace DotNetty.Transport.Channels.Sockets
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;

    public sealed class TcpServerSocketChannel : TcpServerSocketChannel<TcpServerSocketChannel, TcpSocketChannelFactory>
    {
        public TcpServerSocketChannel() : base() { }

        /// <summary>Create a new instance</summary>
        public TcpServerSocketChannel(AddressFamily addressFamily) : base(addressFamily) { }

        /// <summary>Create a new instance using the given <see cref="Socket"/>.</summary>
        public TcpServerSocketChannel(Socket socket) : base(socket) { }
    }

    /// <summary>
    ///     A <see cref="IServerSocketChannel" /> implementation which uses Socket-based implementation to accept new
    ///     connections.
    /// </summary>
    public partial class TcpServerSocketChannel<TServerChannel, TChannelFactory> : AbstractSocketChannel<TServerChannel, TcpServerSocketChannel<TServerChannel, TChannelFactory>.TcpServerSocketChannelUnsafe>, IServerSocketChannel
        where TServerChannel : TcpServerSocketChannel<TServerChannel, TChannelFactory>
        where TChannelFactory : ITcpSocketChannelFactory, new()
    {
        private static readonly ChannelMetadata METADATA = new ChannelMetadata(false);

        private static readonly Action<object, object> ReadCompletedSyncCallback = (u, p) => OnReadCompletedSync(u, p);

        private readonly TChannelFactory _channelFactory;

        private readonly IServerSocketChannelConfiguration _config;

        private SocketChannelAsyncOperation<TServerChannel, TcpServerSocketChannelUnsafe> _acceptOperation;

        /// <summary>
        ///     Create a new instance
        /// </summary>
        public TcpServerSocketChannel()
          : this(SocketEx.CreateSocket())
        {
        }

        /// <summary>
        ///     Create a new instance
        /// </summary>
        public TcpServerSocketChannel(AddressFamily addressFamily)
            : this(SocketEx.CreateSocket(addressFamily))
        {
        }

        /// <summary>
        ///     Create a new instance using the given <see cref="Socket"/>.
        /// </summary>
        public TcpServerSocketChannel(Socket socket)
            : base(null, socket)
        {
            _config = new TcpServerSocketChannelConfig((TServerChannel)this, socket);
            _channelFactory = new TChannelFactory();
        }

        IServerSocketChannelConfiguration IServerSocketChannel.Configuration => _config;
        public override IChannelConfiguration Configuration => _config;

        public override bool IsActive
        {
            // As IsBound will continue to return true even after the channel was closed
            // we will also need to check if it is open.
            get => IsOpen && Socket.IsBound;
        }

        public override ChannelMetadata Metadata => METADATA;

        protected override EndPoint RemoteAddressInternal => null;

        protected override EndPoint LocalAddressInternal => Socket.LocalEndPoint;

        private SocketChannelAsyncOperation<TServerChannel, TcpServerSocketChannelUnsafe> AcceptOperation
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => _acceptOperation ?? EnsureAcceptOperationCreated();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private SocketChannelAsyncOperation<TServerChannel, TcpServerSocketChannelUnsafe> EnsureAcceptOperationCreated()
        {
            lock (this)
            {
                if (_acceptOperation is null)
                {
                    _acceptOperation = new SocketChannelAsyncOperation<TServerChannel, TcpServerSocketChannelUnsafe>((TServerChannel)this, false);
                }
            }
            return _acceptOperation;
        }

        protected override void DoBind(EndPoint localAddress)
        {
            Socket.Bind(localAddress);
            Socket.Listen(_config.Backlog);
            SetState(StateFlags.Active);

            _ = CacheLocalAddress();
        }

        protected override void DoClose()
        {
            if (TryResetState(StateFlags.Open | StateFlags.Active))
            {
                Socket.SafeClose();
            }
        }

        protected override void ScheduleSocketRead()
        {
            var closed = false;
            var aborted = false;
            var operation = AcceptOperation;
            while (!closed)
            {
                try
                {
                    bool pending = Socket.AcceptAsync(operation);
                    if (!pending)
                    {
                        EventLoop.Execute(ReadCompletedSyncCallback, Unsafe, operation);
                    }
                    return;
                }
                catch (SocketException ex) when (ex.SocketErrorCode.IsSocketAbortError())
                {
                    Socket.SafeClose(); // Unbind......
                    _ = Pipeline.FireExceptionCaught(ex);
                    aborted = true;
                }
                catch (SocketException ex)
                {
                    // socket exceptions here are internal to channel's operation and should not go through the pipeline
                    // especially as they have no effect on overall channel's operation
                    if (Logger.InfoEnabled) Logger.ExceptionOnAccept(ex);
                }
                catch (ObjectDisposedException)
                {
                    closed = true;
                }
                catch (Exception ex)
                {
                    _ = Pipeline.FireExceptionCaught(ex);
                    closed = true;
                }
            }
            if (IsOpen)
            {
                if (closed) { Unsafe.Close(Unsafe.VoidPromise()); }
                else if (aborted) { this.CloseSafe(); }
            }
        }

        static void OnReadCompletedSync(object u, object p) => ((ISocketChannelUnsafe)u).FinishRead((SocketChannelAsyncOperation<TServerChannel, TcpServerSocketChannelUnsafe>)p);

        protected override bool DoConnect(EndPoint remoteAddress, EndPoint localAddress)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        protected override void DoFinishConnect(SocketChannelAsyncOperation<TServerChannel, TcpServerSocketChannelUnsafe> operation)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        protected override void DoDisconnect()
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        protected sealed override object FilterOutboundMessage(object msg)
        {
            throw ThrowHelper.GetNotSupportedException();
        }
    }
}