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
    using System.ComponentModel;
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Concurrency;

    public interface IChannelUnsafe
    {
        /// <summary>
        /// Gets the assigned <see cref="IRecvByteBufAllocatorHandle"/> which will be used to allocate <see cref="IByteBuffer"/>'s when
        /// receiving data.
        /// </summary>
        IRecvByteBufAllocatorHandle RecvBufAllocHandle { get; }

        /// <summary>
        /// Register the <see cref="IChannel"/> and notify
        /// the <see cref="Task"/> once the registration was complete.
        /// </summary>
        /// <param name="eventLoop"></param>
        /// <returns></returns>
        Task RegisterAsync(IEventLoop eventLoop);

        void Deregister(IPromise promise);

        Task BindAsync(EndPoint localAddress);

        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

        void Disconnect(IPromise promise);

        void Close(IPromise promise);

        void CloseForcibly();

        void BeginRead();

        void Write(object message, IPromise promise);

        void Flush();

        ChannelOutboundBuffer OutboundBuffer { get; }

        IPromise VoidPromise();

        [EditorBrowsable(EditorBrowsableState.Never)]
        void Initialize(IChannel channel);
    }
}