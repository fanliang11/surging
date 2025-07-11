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
    using System.Threading.Tasks;

    /// <summary>
    /// Called before an <see cref="IChannel"/> will be returned via <see cref="IChannelPool.AcquireAsync"/>.
    /// </summary>
    public interface IChannelHealthChecker
    {
        /// <summary>
        /// Checks if the given channel is healthy (which means it can be used). This method will be called by the
        /// <see cref="IEventLoop"/> of the given <see cref="IChannel"/>
        /// </summary>
        /// <param name="channel">The <see cref="IChannel"/> to check for healthiness.</param>
        /// <returns><c>true</c> if the given <see cref="IChannel"/> is healthy, otherwise <c>false</c>.</returns>
        ValueTask<bool> IsHealthyAsync(IChannel channel);
    }
}
