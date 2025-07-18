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
    using DotNetty.Codecs.Compression;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Embedded;

    public class HttpContentCompressor : HttpContentEncoder
    {
        static readonly AsciiString GZipString = AsciiString.Cached("gzip");
        static readonly AsciiString DeflateString = AsciiString.Cached("deflate");

        readonly int compressionLevel;
        readonly int windowBits;
        readonly int memLevel;
        readonly int contentSizeThreshold;
        IChannelHandlerContext handlerContext;

        public HttpContentCompressor() : this(6) { }

        public HttpContentCompressor(int compressionLevel) : this(compressionLevel, 15, 8, 0) { }

        public HttpContentCompressor(int compressionLevel, int windowBits, int memLevel)
            : this(compressionLevel, windowBits, memLevel, 0) { }

        public HttpContentCompressor(int compressionLevel, int windowBits, int memLevel, int contentSizeThreshold)
        {
            if (compressionLevel < 0 || compressionLevel > 9) { ThrowHelper.ThrowArgumentException_CompressionLevel(compressionLevel); }
            if (windowBits < 9 || windowBits > 15) { ThrowHelper.ThrowArgumentException_WindowBits(windowBits); }
            if (memLevel < 1 || memLevel > 9) { ThrowHelper.ThrowArgumentException_MemLevel(memLevel); }
            if (contentSizeThreshold < 0) { ThrowHelper.ThrowArgumentException_PositiveOrZero(contentSizeThreshold, ExceptionArgument.contentSizeThreshold); }

            this.compressionLevel = compressionLevel;
            this.windowBits = windowBits;
            this.memLevel = memLevel;
            this.contentSizeThreshold = contentSizeThreshold;
        }

        public override void HandlerAdded(IChannelHandlerContext context) => this.handlerContext = context;

        protected override Result BeginEncode(IHttpResponse httpResponse, ICharSequence acceptEncoding)
        {
            if (this.contentSizeThreshold > 0)
            {
                if (httpResponse is IHttpContent httpContent &&
                    httpContent.Content.ReadableBytes < this.contentSizeThreshold)
                {
                    return null;
                }
            }

            if (httpResponse.Headers.Contains(HttpHeaderNames.ContentEncoding))
            {
                // Content-Encoding was set, either as something specific or as the IDENTITY encoding
                // Therefore, we should NOT encode here
                return null;
            }

            ZlibWrapper? wrapper = this.DetermineWrapper(acceptEncoding);
            if (wrapper is null)
            {
                return null;
            }

            ICharSequence targetContentEncoding = null;
            switch (wrapper.Value)
            {
                case ZlibWrapper.Gzip:
                    targetContentEncoding = GZipString;
                    break;
                case ZlibWrapper.Zlib:
                    targetContentEncoding = DeflateString;
                    break;
                default:
                    ThrowHelper.ThrowCodecException_InvalidCompression(wrapper.Value); break;
            }

            return new Result(targetContentEncoding,
              new EmbeddedChannel(
                  this.handlerContext.Channel.Id,
                  this.handlerContext.Channel.Metadata.HasDisconnect,
                  this.handlerContext.Channel.Configuration,
                  ZlibCodecFactory.NewZlibEncoder(
                      wrapper.Value, this.compressionLevel, this.windowBits, this.memLevel)));
        }

        protected internal ZlibWrapper? DetermineWrapper(ICharSequence acceptEncoding)
        {
            float starQ = -1.0f;
            float gzipQ = -1.0f;
            float deflateQ = -1.0f;
            ICharSequence[] parts = CharUtil.Split(acceptEncoding, ',');
            foreach (ICharSequence encoding in parts)
            {
                float q = 1.0f;
                int equalsPos = encoding.IndexOf('=');
                if (equalsPos != -1)
                {
                    try
                    {
                        var ddd = encoding.ToString(equalsPos + 1);
                        q = float.Parse(encoding.ToString(equalsPos + 1));
                    }
                    catch (FormatException)
                    {
                        // Ignore encoding
                        q = 0.0f;
                    }
                }

                if (encoding.Contains('*'))
                {
                    starQ = q;
                }
                else if (AsciiString.Contains(encoding, GZipString) && q > gzipQ)
                {
                    gzipQ = q;
                }
                else if (AsciiString.Contains(encoding, DeflateString) && q > deflateQ)
                {
                    deflateQ = q;
                }
            }
            if (gzipQ > 0.0f || deflateQ > 0.0f)
            {
                return gzipQ >= deflateQ ? ZlibWrapper.Gzip : ZlibWrapper.Zlib;
            }
            if (starQ > 0.0f)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (gzipQ == -1.0f)
                {
                    return ZlibWrapper.Gzip;
                }
                if (deflateQ == -1.0f)
                {
                    return ZlibWrapper.Zlib;
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }
            return null;
        }
    }
}
