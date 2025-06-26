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

    public class HttpResponseEncoder : HttpObjectEncoder<IHttpResponse>
    {
        /// <inheritdoc />
        public override bool AcceptOutboundMessage(object msg)
        {
            var result = base.AcceptOutboundMessage(msg);
            return result && !(msg is IHttpRequest);
        }

        /// <inheritdoc />
        protected internal override void EncodeInitialLine(IByteBuffer buf, IHttpResponse response)
        {
            response.ProtocolVersion.Encode(buf);
            _ = buf.WriteByte(HttpConstants.HorizontalSpace);
            response.Status.Encode(buf);
            _ = buf.WriteShort(HttpConstants.CrlfShort);
        }

        /// <inheritdoc />
        protected override void SanitizeHeadersBeforeEncode(IHttpResponse msg, bool isAlwaysEmpty)
        {
            if (isAlwaysEmpty)
            {
                HttpResponseStatus status = msg.Status;
                if (status.CodeClass == HttpStatusClass.Informational 
                    || status.Code == StatusCodes.Status204NoContent)
                {

                    // Stripping Content-Length:
                    // See https://tools.ietf.org/html/rfc7230#section-3.3.2
                    _ = msg.Headers.Remove(HttpHeaderNames.ContentLength);

                    // Stripping Transfer-Encoding:
                    // See https://tools.ietf.org/html/rfc7230#section-3.3.1
                    _ = msg.Headers.Remove(HttpHeaderNames.TransferEncoding);
                }
                else if (status.Code == StatusCodes.Status205ResetContent)
                {
                    // Stripping Transfer-Encoding:
                    _ = msg.Headers.Remove(HttpHeaderNames.TransferEncoding);

                    // Set Content-Length: 0
                    // https://httpstatuses.com/205
                    _ = msg.Headers.SetInt(HttpHeaderNames.ContentLength, 0);
                }
            }
        }

        /// <inheritdoc />
        protected override bool IsContentAlwaysEmpty(IHttpResponse msg)
        {
            // Correctly handle special cases as stated in:
            // https://tools.ietf.org/html/rfc7230#section-3.3.3
            HttpResponseStatus status = msg.Status;

            if (status.CodeClass == HttpStatusClass.Informational)
            {
                if (status.Code == StatusCodes.Status101SwitchingProtocols)
                {
                    // We need special handling for WebSockets version 00 as it will include an body.
                    // Fortunally this version should not really be used in the wild very often.
                    // See https://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-00#section-1.2
                    return msg.Headers.Contains(HttpHeaderNames.SecWebsocketVersion);
                }
                return true;
            }
            switch (status.Code)
            {
                case StatusCodes.Status204NoContent:
                case StatusCodes.Status304NotModified:
                case StatusCodes.Status205ResetContent:
                    return true;
                default:
                    return false;
            }
        }
    }
}
