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
    /// <summary>Represents the properties of a <see cref="IChannel" /> implementation.</summary>
    public sealed class ChannelMetadata
    {
        /// <summary>Create a new instance</summary>
        /// <param name="hasDisconnect">
        ///     <c>true</c> if and only if the channel has the <c>DisconnectAsync()</c> operation
        ///     that allows a user to disconnect and then call <see cref="IChannel.ConnectAsync(System.Net.EndPoint)" />
        ///     again, such as UDP/IP.
        /// </param>
        public ChannelMetadata(bool hasDisconnect)
            : this(hasDisconnect, 1)
        {
        }

        /// <summary>Create a new instance</summary>
        /// <param name="hasDisconnect">
        ///     <c>true</c> if and only if the channel has the <c>DisconnectAsync</c> operation
        ///     that allows a user to disconnect and then call <see cref="IChannel.ConnectAsync(System.Net.EndPoint)" />
        ///     again, such as UDP/IP.
        /// </param>
        /// <param name="defaultMaxMessagesPerRead">
        ///     If a <see cref="IMaxMessagesRecvByteBufAllocator" /> is in use, then this value will be
        ///     set for <see cref="IMaxMessagesRecvByteBufAllocator.MaxMessagesPerRead" />. Must be <c> &gt; 0</c>.
        /// </param>
        public ChannelMetadata(bool hasDisconnect, int defaultMaxMessagesPerRead)
        {
            if ((uint)(defaultMaxMessagesPerRead - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                ThrowHelper.ThrowArgumentException_Positive(defaultMaxMessagesPerRead, ExceptionArgument.defaultMaxMessagesPerRead);
            }
            this.HasDisconnect = hasDisconnect;
            this.DefaultMaxMessagesPerRead = defaultMaxMessagesPerRead;
        }

        /// <summary>
        ///     Returns <c>true</c> if and only if the channel has the <c>DisconnectAsync()</c> operation
        ///     that allows a user to disconnect and then call <see cref="IChannel.ConnectAsync(System.Net.EndPoint)" /> again,
        ///     such as UDP/IP.
        /// </summary>
        public bool HasDisconnect { get; }

        /// <summary>
        ///     If a <see cref="IMaxMessagesRecvByteBufAllocator" /> is in use, then this is the default value for
        ///     <see cref="IMaxMessagesRecvByteBufAllocator.MaxMessagesPerRead" />.
        /// </summary>
        public int DefaultMaxMessagesPerRead { get; }
    }
}