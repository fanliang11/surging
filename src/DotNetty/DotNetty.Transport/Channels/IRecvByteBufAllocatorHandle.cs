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
    using DotNetty.Buffers;

    public interface IRecvByteBufAllocatorHandle
    {
        /// <summary>
        ///     Creates a new receive buffer whose capacity is probably large enough to read all inbound data and small
        ///     enough not to waste its space.
        /// </summary>
        IByteBuffer Allocate(IByteBufferAllocator alloc);

        /// <summary>
        ///     Similar to <see cref="Allocate" /> except that it does not allocate anything but just tells the
        ///     capacity.
        /// </summary>
        int Guess();

        /// <summary>
        ///     Reset any counters that have accumulated and recommend how many messages/bytes should be read for the next
        ///     read loop.
        ///     <p>
        ///         This may be used by <see cref="ContinueReading" /> to determine if the read operation should complete.
        ///     </p>
        ///     This is only ever a hint and may be ignored by the implementation.
        /// </summary>
        /// <param name="config">The channel configuration which may impact this object's behavior.</param>
        void Reset(IChannelConfiguration config);

        /// <summary>Increment the number of messages that have been read for the current read loop.</summary>
        /// <param name="numMessages">The amount to increment by.</param>
        void IncMessagesRead(int numMessages);

        /// <summary>
        ///     Get or set the bytes that have been read for the last read operation.
        ///     This may be used to increment the number of bytes that have been read.
        /// </summary>
        /// <remarks>
        ///     Returned value may be negative if an read error
        ///     occurs. If a negative value is seen it is expected to be return on the next set to
        ///     <see cref="LastBytesRead" />. A negative value will signal a termination condition enforced externally
        ///     to this class and is not required to be enforced in <see cref="ContinueReading" />.
        /// </remarks>
        int LastBytesRead { get; set; }

        /// <summary>Get or set how many bytes the read operation will (or did) attempt to read.</summary>
        int AttemptedBytesRead { get; set; }

        /// <summary>Determine if the current read loop should continue.</summary>
        /// <returns><c>true</c> if the read loop should continue reading. <c>false</c> if the read loop is complete.</returns>
        bool ContinueReading();

        /// <summary>Signals read completion.</summary>
        void ReadComplete();
    }
}