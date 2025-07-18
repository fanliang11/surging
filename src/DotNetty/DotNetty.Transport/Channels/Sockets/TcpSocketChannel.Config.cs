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

namespace DotNetty.Transport.Channels.Sockets
{
    using System.Net.Sockets;
    using System.Threading;

    partial class TcpSocketChannel<TChannel>
    {
        sealed class TcpSocketChannelConfig : DefaultSocketChannelConfiguration
        {
            private int v_maxBytesPerGatheringWrite = int.MaxValue;

            public TcpSocketChannelConfig(TChannel channel, Socket javaSocket)
                : base(channel, javaSocket)
            {
                CalculateMaxBytesPerGatheringWrite();
            }

            public int GetMaxBytesPerGatheringWrite() => Volatile.Read(ref v_maxBytesPerGatheringWrite);

            public override int SendBufferSize
            {
                get => base.SendBufferSize;
                set
                {
                    base.SendBufferSize = value;
                    CalculateMaxBytesPerGatheringWrite();
                }
            }

            void CalculateMaxBytesPerGatheringWrite()
            {
                // Multiply by 2 to give some extra space in case the OS can process write data faster than we can provide.
                int newSendBufferSize = SendBufferSize << 1;
                if (newSendBufferSize > 0)
                {
                    Interlocked.Exchange(ref v_maxBytesPerGatheringWrite, newSendBufferSize);
                }
            }

            protected override void AutoReadCleared() => ((TChannel)Channel).ClearReadPending();
        }
    }
}