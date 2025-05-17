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

namespace DotNetty.Transport.Libuv
{
    using System.Net;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Libuv.Native;

    public sealed class TcpChannel : TcpChannel<TcpChannel>
    {
        public TcpChannel() : base() { }

        internal TcpChannel(IChannel parent, Tcp tcp) : base(parent, tcp) { }
    }

    public partial class TcpChannel<TChannel> : NativeChannel<TChannel, TcpChannel<TChannel>.TcpChannelUnsafe>
        where TChannel : TcpChannel<TChannel>
    {
        private static readonly ChannelMetadata TcpMetadata = new ChannelMetadata(false);

        private readonly TcpChannelConfig _config;
        private Tcp _tcp;
        private bool _isBound;

        public TcpChannel() : this(null, null)
        {
        }

        internal TcpChannel(IChannel parent, Tcp tcp) : base(parent)
        {
            _config = new TcpChannelConfig(this);
            SetState(StateFlags.Open);
            _tcp = tcp;
        }

        public override IChannelConfiguration Configuration => _config;

        public override ChannelMetadata Metadata => TcpMetadata;

        protected override EndPoint LocalAddressInternal => _tcp?.GetLocalEndPoint();

        protected override EndPoint RemoteAddressInternal => _tcp?.GetPeerEndPoint();

        protected override void DoRegister()
        {
            if (_tcp is null)
            {
                var loopExecutor = (LoopExecutor)EventLoop;
                _tcp = new Tcp(loopExecutor.UnsafeLoop);
            }
            else
            {
                OnConnected();
            }
        }

        internal override NativeHandle GetHandle()
        {
            if (_tcp is null)
            {
                ThrowHelper.ThrowInvalidOperationException_TcpHandle();
            }
            return _tcp;
        }

        protected override void DoBind(EndPoint localAddress)
        {
            _tcp.Bind((IPEndPoint)localAddress);
            _config.Apply();
            _isBound = true;
            _ = CacheLocalAddress();
        }

        internal override bool IsBound => _isBound;

        protected override void OnConnected()
        {
            if (!_isBound)
            {
                // Either channel is created by tcp server channel
                // or connect to remote without bind first
                _config.Apply();
                _isBound = true;
            }

            base.OnConnected();
        }

        protected override void DoDisconnect() => DoClose();

        protected override void DoClose()
        {
            try
            {
                if (TryResetState(StateFlags.Open | StateFlags.Active))
                {
                    if (_tcp is object)
                    {
                        _tcp.ReadStop();
                        _tcp.CloseHandle();
                    }
                    _tcp = null;
                }
            }
            finally
            {
                base.DoClose();
            }
        }

        protected override void DoBeginRead()
        {
            if (!IsOpen)
            {
                return;
            }

            ReadPending = true;
            if (!IsInState(StateFlags.ReadScheduled))
            {
                SetState(StateFlags.ReadScheduled);
                _tcp.ReadStart(Unsafe);
            }
        }

        protected override void DoStopRead()
        {
            if (!IsOpen)
            {
                return;
            }

            if (IsInState(StateFlags.ReadScheduled))
            {
                _ = ResetState(StateFlags.ReadScheduled);
                _tcp.ReadStop();
            }
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            if (input.Size > 0)
            {
                SetState(StateFlags.WriteScheduled);
                var loopExecutor = (LoopExecutor)EventLoop;
                WriteRequest request = loopExecutor.WriteRequestPool.Take();
                request.DoWrite(Unsafe, input);
            }
        }

    }
}
