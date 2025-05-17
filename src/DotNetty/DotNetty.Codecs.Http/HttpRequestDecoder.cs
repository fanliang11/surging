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

    public class HttpRequestDecoder : HttpObjectDecoder
    {
        public HttpRequestDecoder()
        {
        }

        public HttpRequestDecoder(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize)
            : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, DefaultChunkedSupported)
        {
        }

        public HttpRequestDecoder(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders)
            : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, DefaultChunkedSupported, validateHeaders)
        {
        }

        public HttpRequestDecoder(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders,
            int initialBufferSize)
            : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, DefaultChunkedSupported, validateHeaders,
              initialBufferSize)
        {
        }

        public HttpRequestDecoder(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders,
            int initialBufferSize, bool allowDuplicateContentLengths)
            : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, DefaultChunkedSupported, validateHeaders,
                  initialBufferSize, allowDuplicateContentLengths)
        {
        }

        protected sealed override IHttpMessage CreateMessage(AsciiString[] initialLine) =>
            new DefaultHttpRequest(
                HttpVersion.ValueOf(initialLine[2]),
                HttpMethod.ValueOf(initialLine[0]), initialLine[1].ToString(), this.ValidateHeaders);

        protected override IHttpMessage CreateInvalidMessage() => new DefaultFullHttpRequest(HttpVersion.Http10, HttpMethod.Get, "/bad-request", this.ValidateHeaders);

        protected override bool IsDecodingRequest() => true;
    }
}
