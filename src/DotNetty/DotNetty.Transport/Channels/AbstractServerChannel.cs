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
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// A skeletal server-side <see cref="IChannel"/> implementation. A server-side <see cref="IChannel"/> does not
    /// allow the following operations: <see cref="IChannel.ConnectAsync(EndPoint)"/>,
    /// <see cref="IChannel.DisconnectAsync()"/>, <see cref="IChannel.WriteAsync(object)"/>,
    /// <see cref="IChannel.Flush()"/>.
    /// </summary>
    public abstract class AbstractServerChannel<TChannel, TUnsafe> : AbstractChannel<TChannel, TUnsafe>, IServerChannel
        where TChannel : AbstractServerChannel<TChannel, TUnsafe>
        where TUnsafe : AbstractServerChannel<TChannel, TUnsafe>.DefaultServerUnsafe, new()
    {
        static readonly ChannelMetadata METADATA = new ChannelMetadata(false, 16);

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        protected AbstractServerChannel()
            : base(null)
        {
        }

        public override ChannelMetadata Metadata => METADATA;

        protected override EndPoint RemoteAddressInternal => null;

        protected override void DoDisconnect() => throw ThrowHelper.GetNotSupportedException();

        //protected override IChannelUnsafe NewUnsafe() => new DefaultServerUnsafe(this);

        protected override void DoWrite(ChannelOutboundBuffer buf) => throw ThrowHelper.GetNotSupportedException();

        protected override object FilterOutboundMessage(object msg) => throw ThrowHelper.GetNotSupportedException();

        public class DefaultServerUnsafe : AbstractUnsafe
        {
            private Task _err;

            public override void Initialize(IChannel channel)
            {
                base.Initialize(channel);
                _err = TaskUtil.FromException(new NotSupportedException());
            }

            public override Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress) => _err;
        }
    }
}