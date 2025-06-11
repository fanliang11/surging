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
    using System.Threading;

    public sealed class CountingChannelPoolHandler : IChannelPoolHandler
    {
        private int _channelCount;
        private int _acquiredCount;
        private int _releasedCount;
        
        public int ChannelCount => Volatile.Read(ref _channelCount);

        public int AcquiredCount => Volatile.Read(ref _acquiredCount);

        public int ReleasedCount => Volatile.Read(ref _releasedCount);

        public void ChannelCreated(IChannel ch) => Interlocked.Increment(ref _channelCount);

        public void ChannelReleased(IChannel ch) => Interlocked.Increment(ref _releasedCount);

        public void ChannelAcquired(IChannel ch) => Interlocked.Increment(ref _acquiredCount);
    }
}
