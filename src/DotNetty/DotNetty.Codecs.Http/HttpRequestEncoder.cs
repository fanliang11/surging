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
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    using static HttpConstants;

    public class HttpRequestEncoder : HttpObjectEncoder<IHttpRequest>
    {
        const char Slash = '/';
        const char QuestionMark = '?';
        const int SlashAndSpaceShort = (Slash << 8) | HorizontalSpace;
        const int SpaceSlashAndSpaceMedium = (HorizontalSpace << 16) | SlashAndSpaceShort;

        /// <inheritdoc />
        public override bool AcceptOutboundMessage(object msg)
        {
            var result = base.AcceptOutboundMessage(msg);
            return result && !(msg is IHttpResponse);
        }

        /// <inheritdoc />
        protected internal override void EncodeInitialLine(IByteBuffer buf, IHttpRequest request)
        {
            ByteBufferUtil.Copy(request.Method.AsciiName, buf);

            string uri = request.Uri;

            if (string.IsNullOrEmpty(uri))
            {
                // Add / as absolute path if no is present.
                // See http://tools.ietf.org/html/rfc2616#section-5.1.2
                _ = buf.WriteMedium(SpaceSlashAndSpaceMedium);
            }
            else
            {
                var uriCharSequence = new StringBuilderCharSequence();
                uriCharSequence.Append(uri);

                bool needSlash = false;
                int start = uri.IndexOf("://", StringComparison.Ordinal);
                if (start != -1 && uri[0] != Slash)
                {
                    start += 3;
                    // Correctly handle query params.
                    // See https://github.com/netty/netty/issues/2732
                    int index = uri.IndexOf(QuestionMark, start);
                    if (index == -1)
                    {
                        if (uri.LastIndexOf(Slash) < start)
                        {
                            needSlash = true;
                        }
                    }
                    else
                    {
                        if (uri.LastIndexOf(Slash, index) < start)
                        {
                            uriCharSequence.Insert(index, Slash);
                        }
                    }
                }

                _ = buf.WriteByte(HorizontalSpace).WriteCharSequence(uriCharSequence, Encoding.UTF8);
                if (needSlash)
                {
                    // write "/ " after uri
                    _ = buf.WriteShort(SlashAndSpaceShort);
                }
                else
                {
                    _ = buf.WriteByte(HorizontalSpace);
                }
            }

            request.ProtocolVersion.Encode(buf);
            _ = buf.WriteShort(CrlfShort);
        }
    }
}
