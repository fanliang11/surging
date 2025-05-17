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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Handlers.IPFilter
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// This class allows one to ensure that at all times for every IP address there is at most one
    /// <see cref="IChannel"/>  connected to the server.
    /// </summary>
    public class UniqueIPFilter : AbstractRemoteAddressFilter<IPEndPoint>
    {
        const byte Filler = 0;
        //using dictionary as set. value always equals Filler.
        readonly ConcurrentDictionary<IPAddress, byte> connected = new ConcurrentDictionary<IPAddress, byte>();

        protected override bool Accept(IChannelHandlerContext ctx, IPEndPoint remoteAddress)
        {
            IPAddress remoteIp = remoteAddress.Address;
            if (!this.connected.TryAdd(remoteIp, Filler))
            {
                return false;
            }
            else
            {
                ctx.Channel.CloseCompletion.ContinueWith(s_removeIpAddrAfterCloseAction, (this.connected, remoteIp), TaskContinuationOptions.ExecuteSynchronously);
            }
            return true;
        }

        static readonly Action<Task, object> s_removeIpAddrAfterCloseAction = (t, s) => RemoveIpAddrAfterCloseAction(t, s);
        static void RemoveIpAddrAfterCloseAction(Task t, object s)
        {
            var wrapped = ((ConcurrentDictionary<IPAddress, byte>, IPAddress))s;
            wrapped.Item1.TryRemove(wrapped.Item2, out _);
        }

        public override bool IsSharable => true;
    }
}