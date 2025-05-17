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
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Compression
{
    using System;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    public abstract class ZlibDecoder : ByteToMessageDecoder
    {
        /// <summary>
        /// Maximum allowed size of the decompression buffer.
        /// </summary>
        protected readonly int _maxAllocation;

        /// <summary>Construct a new ZlibDecoder.</summary>
        public ZlibDecoder() : this(0) { }

        /// <summary>Construct a new ZlibDecoder.</summary>
        /// <param name="maxAllocation">Maximum size of the decompression buffer. Must be &gt;= 0.
        /// If zero, maximum size is decided by the <see cref="IByteBufferAllocator"/>.</param>
        public ZlibDecoder(int maxAllocation)
        {
            if ((uint)maxAllocation > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentException_PositiveOrZero(maxAllocation, ExceptionArgument.maxAllocation);
            }
            _maxAllocation = maxAllocation;
        }

        public abstract bool IsClosed { get; }

        /// <summary>
        /// Allocate or expand the decompression buffer, without exceeding the maximum allocation.
        /// Calls <see cref="DecompressionBufferExhausted(IByteBuffer)"/> if the buffer is full and cannot be expanded further.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="buffer"></param>
        /// <param name="preferredSize"></param>
        /// <returns></returns>
        protected IByteBuffer PrepareDecompressBuffer(IChannelHandlerContext ctx, IByteBuffer buffer, int preferredSize)
        {
            if (buffer is null)
            {
                if (0u >= (uint)_maxAllocation)
                {
                    return ctx.Allocator.HeapBuffer(preferredSize);
                }

                return ctx.Allocator.HeapBuffer(Math.Min(preferredSize, _maxAllocation), _maxAllocation);
            }

            // this always expands the buffer if possible, even if the expansion is less than preferredSize
            // we throw the exception only if the buffer could not be expanded at all
            // this means that one final attempt to deserialize will always be made with the buffer at maxAllocation
            if (buffer.EnsureWritable(preferredSize, true) == 1)
            {
                // buffer must be consumed so subclasses don't add it to output
                // we therefore duplicate it when calling decompressionBufferExhausted() to guarantee non-interference
                // but wait until after to consume it so the subclass can tell how much output is really in the buffer
                DecompressionBufferExhausted(buffer.Duplicate());
                _ = buffer.SkipBytes(buffer.ReadableBytes);
                CThrowHelper.ThrowDecompressionException_Decompression_buffer_has_reached_maximum_size(buffer.MaxCapacity);
            }

            return buffer;
        }

        /// <summary>
        /// Called when the decompression buffer cannot be expanded further.
        /// Default implementation is a no-op, but subclasses can override in case they want to
        /// do something before the <see cref="DecompressionException"/> is thrown, such as log the
        /// data that was decompressed so far.
        /// </summary>
        /// <param name="buffer"></param>
        protected virtual void DecompressionBufferExhausted(IByteBuffer buffer)
        {
        }
    }
}
