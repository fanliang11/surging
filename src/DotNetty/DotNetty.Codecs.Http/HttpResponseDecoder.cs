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
    using DotNetty.Common.Utilities;

    public class HttpResponseDecoder : HttpObjectDecoder
    {
        static readonly HttpResponseStatus UnknownStatus = new HttpResponseStatus(999, new AsciiString("Unknown"));

        public HttpResponseDecoder()
        {
        }

        public HttpResponseDecoder(int maxInitialLineLength, int maxHeaderSize, int maxChunkSize)
            : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, DefaultChunkedSupported)
        {
        }

        public HttpResponseDecoder(int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders)
            : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, DefaultChunkedSupported, validateHeaders)
        {
        }

        public HttpResponseDecoder(int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders, int initialBufferSize)
            : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, DefaultChunkedSupported, validateHeaders, initialBufferSize)
        {
        }

        public HttpResponseDecoder(
                int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders,
                int initialBufferSize, bool allowDuplicateContentLengths)
            : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, DefaultChunkedSupported, validateHeaders,
                  initialBufferSize, allowDuplicateContentLengths)
        {
        }

        protected sealed override IHttpMessage CreateMessage(AsciiString[] initialLine) =>
             new DefaultHttpResponse(
                HttpVersion.ValueOf(initialLine[0]),
                HttpResponseStatus.ValueOf(initialLine[1].ParseInt(), initialLine[2]), this.ValidateHeaders);

        protected override IHttpMessage CreateInvalidMessage() => new DefaultFullHttpResponse(HttpVersion.Http10, UnknownStatus, this.ValidateHeaders);

        protected override bool IsDecodingRequest() => false;
    }
}
