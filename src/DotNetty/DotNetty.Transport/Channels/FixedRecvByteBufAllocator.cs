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
    /// <summary>
    ///     The <see cref="IRecvByteBufAllocator" /> that always yields the same buffer
    ///     size prediction. This predictor ignores the feedback from the I/O thread.
    /// </summary>
    public sealed class FixedRecvByteBufAllocator : DefaultMaxMessagesRecvByteBufAllocator
    {
        public static readonly FixedRecvByteBufAllocator Default = new FixedRecvByteBufAllocator(4 * 1024);

        sealed class HandleImpl : MaxMessageHandle<FixedRecvByteBufAllocator>
        {
            readonly int _bufferSize;

            public HandleImpl(FixedRecvByteBufAllocator owner, int bufferSize)
                : base(owner)
            {
                _bufferSize = bufferSize;
            }

            public override int Guess() => _bufferSize;
        }

        readonly int _bufferSize;

        /// <summary>
        ///     Creates a new predictor that always returns the same prediction of
        ///     the specified buffer size.
        /// </summary>
        public FixedRecvByteBufAllocator(int bufferSize)
        {
            if ((uint)(bufferSize - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                ThrowHelper.ThrowArgumentException_Positive(bufferSize, ExceptionArgument.bufferSize);
            }

            _bufferSize = bufferSize;
        }

        public override IRecvByteBufAllocatorHandle NewHandle() => new HandleImpl(this, _bufferSize);
    }
}