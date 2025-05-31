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
    using System;
    using DotNetty.Buffers;
    using DotNetty.Common;

    public sealed class EmptyLastHttpContent : ILastHttpContent
    {
        public static readonly EmptyLastHttpContent Default = new EmptyLastHttpContent();

        EmptyLastHttpContent()
        {
            this.Content = Unpooled.Empty;
        }

        public DecoderResult Result
        {
            get => DecoderResult.Success;
            set => throw new NotSupportedException("read only");
        }

        public int ReferenceCount => 1;

        public IReferenceCounted Retain() => this;

        public IReferenceCounted Retain(int increment) => this;

        public IReferenceCounted Touch() => this;

        public IReferenceCounted Touch(object hint) => this;

        public bool Release() => false;

        public bool Release(int decrement) => false;

        public IByteBuffer Content { get; }

        public IByteBufferHolder Copy() => this;

        public IByteBufferHolder Duplicate() => this;

        public IByteBufferHolder RetainedDuplicate() => this;

        public IByteBufferHolder Replace(IByteBuffer content) => new DefaultLastHttpContent(content);

        public HttpHeaders TrailingHeaders => EmptyHttpHeaders.Default;

        public override string ToString() => nameof(EmptyLastHttpContent);
    }
}
