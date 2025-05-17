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

namespace DotNetty.Transport.Channels.Pool
{
    /// <summary>
    /// Handler which is called for various actions done by the <see cref="IChannelPool"/>.
    /// </summary>
    public interface IChannelPoolHandler
    {
        /// <summary>
        /// Called once a <see cref="IChannel"/> was released by calling <see cref="IChannelPool.ReleaseAsync"/>.
        /// This method will be called by the <see cref="IEventLoop"/> of the <see cref="IChannel"/>.
        /// </summary>
        /// <param name="channel">The <see cref="IChannel"/> instance which was released.</param>
        void ChannelReleased(IChannel channel);

        /// <summary>
        /// Called once a <see cref="IChannel"/> was acquired by calling <see cref="IChannelPool.AcquireAsync"/>.
        /// </summary>
        /// <param name="channel">The <see cref="IChannel"/> instance which was aquired.</param>
        void ChannelAcquired(IChannel channel);

        /// <summary>
        /// Called once a new <see cref="IChannel"/> is created in the <see cref="IChannelPool"/>.
        /// </summary>
        /// <param name="channel">The <see cref="IChannel"/> instance which was aquired.</param>
        void ChannelCreated(IChannel channel);
    }
}
