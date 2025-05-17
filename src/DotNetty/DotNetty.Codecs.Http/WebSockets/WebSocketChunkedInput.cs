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
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.WebSockets
{
    using DotNetty.Buffers;
    using DotNetty.Handlers.Streams;

    public sealed class WebSocketChunkedInput : IChunkedInput<WebSocketFrame>
    {
        readonly IChunkedInput<IByteBuffer> input;
        readonly int rsv;

        public WebSocketChunkedInput(IChunkedInput<IByteBuffer> input)
            : this(input, 0)
        {
        }

        public WebSocketChunkedInput(IChunkedInput<IByteBuffer> input, int rsv)
        {
            if (input is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.input); }

            this.input = input;
            this.rsv = rsv;
        }

        public bool IsEndOfInput => this.input.IsEndOfInput;

        public void Close() => this.input.Close();

        public WebSocketFrame ReadChunk(IByteBufferAllocator allocator)
        {
            IByteBuffer buf = this.input.ReadChunk(allocator);
            return buf is object ? new ContinuationWebSocketFrame(this.input.IsEndOfInput, this.rsv, buf) : null;
        }

        public long Length => this.input.Length;

        public long Progress => this.input.Progress;
    }
}
