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
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Allows the acquisition and release of <see cref="IChannel"/> instances, and so act as a pool of these.
    /// </summary>
    public interface IChannelPool
    {
        /// <summary>
        /// Acquires an <see cref="IChannel"/> from this <see cref="IChannelPool"/>.
        /// <para>
        /// It is important that an acquired <see cref="IChannel"/> is always released to the pool again via the
        /// <see cref="ReleaseAsync"/> method, even if the <see cref="IChannel"/> is explicitly closed.
        /// </para>
        /// </summary>
        /// <returns>The aquired <see cref="IChannel"/>.</returns>
        ValueTask<IChannel> AcquireAsync();

        /// <summary>
        /// Releases a previously aquired <see cref="IChannel"/> from this <see cref="IChannelPool"/>, allowing it to
        /// be aquired again by another caller.
        /// </summary>
        /// <param name="channel">The <see cref="IChannel"/> instance to be released.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="IChannel"/> was successfully released, otherwise <c>false</c>.
        /// </returns>
        Task<bool> ReleaseAsync(IChannel channel);

        void Close();

        Task CloseAsync();
    }
}
