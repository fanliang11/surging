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

namespace DotNetty.Transport.Channels.Local
{
    using System.Collections.Concurrent;
    using System.Net;

    public static class LocalChannelRegistry
    {
        static readonly ConcurrentDictionary<LocalAddress, IChannel> BoundChannels = new ConcurrentDictionary<LocalAddress, IChannel>(LocalAddressComparer.Default);

        internal static LocalAddress Register(IChannel channel, LocalAddress oldLocalAddress, EndPoint localAddress) 
        {
            if (oldLocalAddress is object) 
            {
                ThrowHelper.ThrowChannelException_AlreadyBound();
            }

            var addr = localAddress as LocalAddress;
            if (addr is null) 
            {
                ThrowHelper.ThrowChannelException_UnsupportedAddrType(localAddress);
            }

            if (LocalAddress.Any.Equals(addr)) 
            {
                addr = new LocalAddress(channel);
            }

            var result = BoundChannels.GetOrAdd(addr, channel);
            if (!ReferenceEquals(result, channel))
            {
                ThrowHelper.ThrowChannelException_AddrAlreadyInUseBy(result);
            }
            
            return addr;
        }

        internal static IChannel Get(EndPoint localAddress) 
            => localAddress is LocalAddress key && BoundChannels.TryGetValue(key, out var ch) ? ch : null;

        internal static void Unregister(LocalAddress localAddress) 
            => BoundChannels.TryRemove(localAddress, out _);
    }
}
