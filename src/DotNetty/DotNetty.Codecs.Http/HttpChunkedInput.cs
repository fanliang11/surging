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

namespace DotNetty.Codecs.Http
{
    using DotNetty.Buffers;
    using DotNetty.Handlers.Streams;

    public class HttpChunkedInput : IChunkedInput<IHttpContent>
    {
        readonly IChunkedInput<IByteBuffer> input;
        readonly ILastHttpContent lastHttpContent;
        bool sentLastChunk;

        public HttpChunkedInput(IChunkedInput<IByteBuffer> input)
        {
            this.input = input;
            this.lastHttpContent = EmptyLastHttpContent.Default;
        }

        public HttpChunkedInput(IChunkedInput<IByteBuffer> input, ILastHttpContent lastHttpContent)
        {
            this.input = input;
            this.lastHttpContent = lastHttpContent;
        }

        public bool IsEndOfInput => this.input.IsEndOfInput && this.sentLastChunk;

        public void Close() => this.input.Close();

        public IHttpContent ReadChunk(IByteBufferAllocator allocator)
        {
            if (this.input.IsEndOfInput)
            {
                if (this.sentLastChunk)
                {
                    return null;
                }
                // Send last chunk for this input
                this.sentLastChunk = true;
                return this.lastHttpContent;
            }
            else
            {
                IByteBuffer buf = this.input.ReadChunk(allocator);
                return buf is null ? null : new DefaultHttpContent(buf);
            }
        }

        public long Length => this.input.Length;

        public long Progress => this.input.Progress;
    }
}
