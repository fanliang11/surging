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
    using DotNetty.Codecs.Compression;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels.Embedded;

    public class HttpContentDecompressor : HttpContentDecoder
    {
        readonly bool strict;

        public HttpContentDecompressor() : this(false)
        {
        }

        public HttpContentDecompressor(bool strict)
        {
            this.strict = strict;
        }

        protected override EmbeddedChannel NewContentDecoder(ICharSequence contentEncoding)
        {
            if (HttpHeaderValues.Gzip.ContentEqualsIgnoreCase(contentEncoding) 
                || HttpHeaderValues.XGzip.ContentEqualsIgnoreCase(contentEncoding))
            {
                return new EmbeddedChannel(
                    this.HandlerContext.Channel.Id, 
                    this.HandlerContext.Channel.Metadata.HasDisconnect, 
                    this.HandlerContext.Channel.Configuration, 
                    ZlibCodecFactory.NewZlibDecoder(ZlibWrapper.Gzip));
            }

            if (HttpHeaderValues.Deflate.ContentEqualsIgnoreCase(contentEncoding) 
                || HttpHeaderValues.XDeflate.ContentEqualsIgnoreCase(contentEncoding))
            {
                ZlibWrapper wrapper = this.strict ? ZlibWrapper.Zlib : ZlibWrapper.ZlibOrNone;
                return new EmbeddedChannel(
                    this.HandlerContext.Channel.Id, 
                    this.HandlerContext.Channel.Metadata.HasDisconnect, 
                    this.HandlerContext.Channel.Configuration,
                    ZlibCodecFactory.NewZlibDecoder(wrapper));
            }

            // 'identity' or unsupported
            return null;
        }
    }
}
