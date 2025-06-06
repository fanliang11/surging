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

namespace DotNetty.Handlers.Streams
{
    using System;
    using System.IO;
    using System.Threading;
    using DotNetty.Buffers;

    public class ChunkedStream : IChunkedInput<IByteBuffer>
    {
        public static readonly int DefaultChunkSize = 8192;

        readonly Stream input;
        readonly int chunkSize;
        long offset; bool closed;

        public ChunkedStream(Stream input) : this(input, DefaultChunkSize)
        {
        }

        public ChunkedStream(Stream input, int chunkSize)
        {
            if (input is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.input); }
            if ((uint)(chunkSize - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                ThrowHelper.ThrowArgumentException_Positive(chunkSize, ExceptionArgument.chunkSize);
            }

            this.input = input;
            this.chunkSize = chunkSize;
        }

        public long TransferredBytes => this.offset;

        public bool IsEndOfInput => this.closed || (this.input.Position == this.input.Length);

        public void Close()
        {
            this.closed = true;
            this.input.Dispose();
        }

        public IByteBuffer ReadChunk(IByteBufferAllocator allocator)
        {
            if (this.IsEndOfInput)
            {
                return null;
            }

            long availableBytes = this.input.Length - this.input.Position;
            int readChunkSize = availableBytes <= 0L
                ? this.chunkSize
                : (int)Math.Min(this.chunkSize, availableBytes);

            bool release = true;
            IByteBuffer buffer = allocator.Buffer(readChunkSize);
            try
            {
                // transfer to buffer
                int count = buffer.SetBytesAsync(buffer.WriterIndex, this.input, readChunkSize, CancellationToken.None).Result;
                buffer.SetWriterIndex(buffer.WriterIndex + count);
                this.offset += count;

                release = false;
            }
            finally
            {
                if (release)
                {
                    buffer.Release();
                }
            }

            return buffer;
        }

        public long Length => -1;

        public long Progress => this.offset;
    }
}
