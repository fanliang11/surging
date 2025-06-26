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
    using DotNetty.Common;

    public sealed class ComposedLastHttpContent : ILastHttpContent
    {
        readonly HttpHeaders trailingHeaders;
        DecoderResult result;

        internal ComposedLastHttpContent(HttpHeaders trailingHeaders)
        {
            this.trailingHeaders = trailingHeaders;
        }

        internal ComposedLastHttpContent(HttpHeaders trailingHeaders, DecoderResult result)
            : this(trailingHeaders)
        {
            this.result = result;
        }

        public HttpHeaders TrailingHeaders => this.trailingHeaders;

        public IByteBufferHolder Copy()
        {
            var content = new DefaultLastHttpContent(Unpooled.Empty);
            _ = content.TrailingHeaders.Set(this.trailingHeaders);
            return content;
        }

        public IByteBufferHolder Duplicate() => this.Copy();

        public IByteBufferHolder RetainedDuplicate() => this.Copy();

        public IByteBufferHolder Replace(IByteBuffer content)
        {
            var dup = new DefaultLastHttpContent(content);
            _ = dup.TrailingHeaders.SetAll(this.trailingHeaders);
            return dup;
        }

        public IReferenceCounted Retain() => this;

        public IReferenceCounted Retain(int increment) => this;

        public IReferenceCounted Touch() => this;

        public IReferenceCounted Touch(object hint) => this;

        public IByteBuffer Content => Unpooled.Empty;

        public DecoderResult Result
        {
            get => this.result;
            set => this.result = value;
        }

        public int ReferenceCount => 1;

        public bool Release() => false;

        public bool Release(int decrement) => false;
    }
}
