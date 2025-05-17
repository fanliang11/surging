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
    using DotNetty.Common.Utilities;

    public class DefaultHttpContent : DefaultHttpObject, IHttpContent
    {
        readonly IByteBuffer content;

        public DefaultHttpContent(IByteBuffer content)
        {
            if (content is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.content); }

            this.content = content;
        }

        public IByteBuffer Content => this.content;

        public IByteBufferHolder Copy() => this.Replace(this.content.Copy());

        public IByteBufferHolder Duplicate() => this.Replace(this.content.Duplicate());

        public IByteBufferHolder RetainedDuplicate() => this.Replace(this.content.RetainedDuplicate());

        public virtual IByteBufferHolder Replace(IByteBuffer buffer) => new DefaultHttpContent(buffer);

        public int ReferenceCount => this.content.ReferenceCount;

        public IReferenceCounted Retain()
        {
            _ = this.content.Retain();
            return this;
        }

        public IReferenceCounted Retain(int increment)
        {
            _ = this.content.Retain(increment);
            return this;
        }

        public IReferenceCounted Touch()
        {
            _ = this.content.Touch();
            return this;
        }

        public IReferenceCounted Touch(object hint)
        {
            _ = this.content.Touch(hint);
            return this;
        }

        public bool Release() => this.content.Release();

        public bool Release(int decrement) => this.content.Release(decrement);

        public override string ToString() => $"{StringUtil.SimpleClassName(this)} (data: {this.content}, decoderResult: {this.Result})";
    }
}
