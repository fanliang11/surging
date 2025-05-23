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
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    public class DefaultLastHttpContent : DefaultHttpContent, ILastHttpContent
    {
        readonly HttpHeaders trailingHeaders;
        readonly bool validateHeaders;

        public DefaultLastHttpContent() : this(ArrayPooled.Buffer(0), true)
        {
        }

        public DefaultLastHttpContent(IByteBuffer content) : this(content, true)
        {
        }

        public DefaultLastHttpContent(IByteBuffer content, bool validateHeaders)
            : base(content)
        {
            this.trailingHeaders = new TrailingHttpHeaders(validateHeaders);
            this.validateHeaders = validateHeaders;
        }

        public HttpHeaders TrailingHeaders => this.trailingHeaders;

        public override IByteBufferHolder Replace(IByteBuffer buffer)
        {
            var dup = new DefaultLastHttpContent(this.Content, this.validateHeaders);
            _ = dup.TrailingHeaders.Set(this.trailingHeaders);
            return dup;
        }

        public override string ToString()
        {
            var buf = StringBuilderManager.Allocate().Append(base.ToString());
            _ = buf.Append(StringUtil.Newline);
            this.AppendHeaders(buf);

            // Remove the last newline.
            buf.Length = buf.Length - StringUtil.Newline.Length;
            return StringBuilderManager.ReturnAndFree(buf);
        }

        void AppendHeaders(StringBuilder buf)
        {
            foreach (HeaderEntry<AsciiString, ICharSequence> e in this.trailingHeaders)
            {
                _ = buf.Append($"{e.Key}: {e.Value}{StringUtil.Newline}");
            }
        }

        sealed class TrailerNameValidator : INameValidator<ICharSequence>
        {
            public void ValidateName(ICharSequence name)
            {
                DefaultHttpHeaders.HttpNameValidator.ValidateName(name);
                if (HttpHeaderNames.ContentLength.ContentEqualsIgnoreCase(name)
                    || HttpHeaderNames.TransferEncoding.ContentEqualsIgnoreCase(name)
                    || HttpHeaderNames.Trailer.ContentEqualsIgnoreCase(name))
                {
                    ThrowHelper.ThrowArgumentException_TrailingHeaderName(name);
                }
            }
        }

        sealed class TrailingHttpHeaders : DefaultHttpHeaders
        {
            static readonly TrailerNameValidator TrailerNameValidator = new TrailerNameValidator();

            public TrailingHttpHeaders(bool validate) 
                : base(validate, validate ? TrailerNameValidator : NotNullValidator)
            {
            }
        }
    }
}
