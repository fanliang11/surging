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

namespace DotNetty.Transport.Channels.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IChannelGroup : ICollection<IChannel>, IComparable<IChannelGroup>
    {
        /// <summary>
        ///     Returns the name of this group.  A group name is purely for helping
        ///     you to distinguish one group from others.
        /// </summary>
        string Name { get; }

        IChannel Find(IChannelId id);

        Task WriteAsync(object message);

        Task WriteAsync(object message, IChannelMatcher matcher);

        Task WriteAsync(object message, IChannelMatcher matcher, bool voidPromise);

        IChannelGroup Flush();

        IChannelGroup Flush(IChannelMatcher matcher);

        Task WriteAndFlushAsync(object message);

        Task WriteAndFlushAsync(object message, IChannelMatcher matcher);

        Task WriteAndFlushAsync(object message, IChannelMatcher matcher, bool voidPromise);

        Task DisconnectAsync();

        Task DisconnectAsync(IChannelMatcher matcher);

        Task CloseAsync();

        Task CloseAsync(IChannelMatcher matcher);

        Task DeregisterAsync();

        Task DeregisterAsync(IChannelMatcher matcher);

        Task NewCloseFuture();

        Task NewCloseFuture(IChannelMatcher matcher);
    }
}