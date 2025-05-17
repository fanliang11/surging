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
    using System.Collections.Generic;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal;
    using DotNetty.Transport.Channels;

    public class HttpServerCodec : CombinedChannelDuplexHandler<HttpRequestDecoder, HttpResponseEncoder>,
        HttpServerUpgradeHandler.ISourceCodec
    {
        /// <summary>
        /// A queue that is used for correlating a request and a response.
        /// </summary>
        private readonly Deque<HttpMethod> _queue = new Deque<HttpMethod>();

        /// <summary>
        /// Creates a new instance with the default decoder options
        /// ({@code maxInitialLineLength (4096}}, {@code maxHeaderSize (8192)}, and
        /// {@code maxChunkSize (8192)}).
        /// </summary>
        public HttpServerCodec()
            : this(HttpObjectDecoder.DefaultMaxInitialLineLength, HttpObjectDecoder.DefaultMaxHeaderSize, HttpObjectDecoder.DefaultMaxChunkSize)
        {
        }

        /// <summary>
        /// Creates a new instance with the specified decoder options.
        /// </summary>
        /// <param name="maxInitialLineLength"></param>
        /// <param name="maxHeaderSize"></param>
        /// <param name="maxChunkSize"></param>
        public HttpServerCodec(int maxInitialLineLength, int maxHeaderSize, int maxChunkSize)
        {
            Init(new HttpServerRequestDecoder(this, maxInitialLineLength, maxHeaderSize, maxChunkSize),
                new HttpServerResponseEncoder(this));
        }

        /// <summary>
        /// Creates a new instance with the specified decoder options.
        /// </summary>
        /// <param name="maxInitialLineLength"></param>
        /// <param name="maxHeaderSize"></param>
        /// <param name="maxChunkSize"></param>
        /// <param name="validateHeaders"></param>
        public HttpServerCodec(int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders)
        {
            Init(new HttpServerRequestDecoder(this, maxInitialLineLength, maxHeaderSize, maxChunkSize, validateHeaders),
                new HttpServerResponseEncoder(this));
        }

        /// <summary>
        /// Creates a new instance with the specified decoder options.
        /// </summary>
        /// <param name="maxInitialLineLength"></param>
        /// <param name="maxHeaderSize"></param>
        /// <param name="maxChunkSize"></param>
        /// <param name="validateHeaders"></param>
        /// <param name="initialBufferSize"></param>
        public HttpServerCodec(int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders, int initialBufferSize)
        {
            Init(new HttpServerRequestDecoder(this, maxInitialLineLength, maxHeaderSize, maxChunkSize, validateHeaders, initialBufferSize),
                new HttpServerResponseEncoder(this));
        }

        public HttpServerCodec(int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders,
                               int initialBufferSize, bool allowDuplicateContentLengths)
        {
            Init(new HttpServerRequestDecoder(this, maxInitialLineLength, maxHeaderSize, maxChunkSize, validateHeaders,
                                              initialBufferSize, allowDuplicateContentLengths),
                 new HttpServerResponseEncoder(this));
        }

        /// <summary>
        /// Upgrades to another protocol from HTTP. Removes the <see cref="HttpRequestDecoder"/> and
        /// <see cref="HttpResponseEncoder"/> from the pipeline.
        /// </summary>
        /// <param name="ctx"></param>
        public void UpgradeFrom(IChannelHandlerContext ctx) => ctx.Pipeline.Remove(this);

        sealed class HttpServerRequestDecoder : HttpRequestDecoder
        {
            private readonly HttpServerCodec _serverCodec;

            public HttpServerRequestDecoder(HttpServerCodec serverCodec, int maxInitialLineLength, int maxHeaderSize, int maxChunkSize)
                : base(maxInitialLineLength, maxHeaderSize, maxChunkSize)
            {
                _serverCodec = serverCodec;
            }

            public HttpServerRequestDecoder(HttpServerCodec serverCodec, int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders)
                : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, validateHeaders)
            {
                _serverCodec = serverCodec;
            }

            public HttpServerRequestDecoder(HttpServerCodec serverCodec,
                int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders, int initialBufferSize)
                : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, validateHeaders, initialBufferSize)
            {
                _serverCodec = serverCodec;
            }

            public HttpServerRequestDecoder(HttpServerCodec serverCodec,
                int maxInitialLineLength, int maxHeaderSize, int maxChunkSize,
                bool validateHeaders, int initialBufferSize, bool allowDuplicateContentLengths)
                : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, validateHeaders, initialBufferSize, allowDuplicateContentLengths)
            {
                _serverCodec = serverCodec;
            }

            protected override void Decode(IChannelHandlerContext context, IByteBuffer buffer, List<object> output)
            {
                int oldSize = output.Count;
                base.Decode(context, buffer, output);
                int size = output.Count;
                for (int i = oldSize; i < size; i++)
                {
                    if (output[i] is IHttpRequest request)
                    {
                        _serverCodec._queue.AddLast​(request.Method);
                    }
                }
            }
        }

        sealed class HttpServerResponseEncoder : HttpResponseEncoder
        {
            private readonly HttpServerCodec _serverCodec;
            private HttpMethod _method;

            public HttpServerResponseEncoder(HttpServerCodec serverCodec)
            {
                _serverCodec = serverCodec;
            }

            protected override void SanitizeHeadersBeforeEncode(IHttpResponse msg, bool isAlwaysEmpty)
            {
                if (!isAlwaysEmpty && ReferenceEquals(_method, HttpMethod.Connect) &&
                    msg.Status.CodeClass == HttpStatusClass.Success)
                {
                    // Stripping Transfer-Encoding:
                    // See https://tools.ietf.org/html/rfc7230#section-3.3.1
                    _ = msg.Headers.Remove(HttpHeaderNames.TransferEncoding);
                    return;
                }

                base.SanitizeHeadersBeforeEncode(msg, isAlwaysEmpty);
            }

            protected override bool IsContentAlwaysEmpty(IHttpResponse msg)
            {
                _ = _serverCodec._queue.TryRemoveFirst(out _method);
                return HttpMethod.Head.Equals(_method) || base.IsContentAlwaysEmpty(msg);
            }
        }
    }
}
