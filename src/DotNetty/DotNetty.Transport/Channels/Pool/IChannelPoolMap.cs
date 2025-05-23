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
    /// Allows the mapping of <see cref="IChannelPool"/> implementations to a specific key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TPool">The type of the <see cref="IChannelPool"/>.</typeparam>
    public interface IChannelPoolMap<TKey, TPool>
        where TPool : IChannelPool
    {
        /// <summary>
        /// Returns the <see cref="IChannelPool"/> for the <paramref name="key"/>. This will never return <c>null</c>,
        /// but create a new <see cref="IChannelPool"/> if non exists for they requested <paramref name="key"/>.
        /// Please note that <c>null</c> keys are not allowed.
        /// </summary>
        /// <param name="key">The key for the desired <see cref="IChannelPool"/></param>
        /// <returns>The <see cref="IChannelPool"/> for the specified <paramref name="key"/>.</returns>
        TPool Get(TKey key);

        /// <summary>
        /// Checks whether the <see cref="IChannelPoolMap{TKey,TPool}"/> contains an <see cref="IChannelPool"/> for the
        /// given <paramref name="key"/>. Please note that <c>null</c> keys are not allowed.
        /// </summary>
        /// <param name="key">The key to search the <see cref="IChannelPoolMap{TKey,TPool}"/> for.</param>
        /// <returns><c>true</c> if a <see cref="IChannelPool"/> exists for the given <paramref name="key"/>, otherwise <c>false</c>.</returns>
        bool Contains(TKey key);
    }
}
