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
    using DotNetty.Common.Utilities;

    static class HttpMessageUtil
    {
        internal static StringBuilder AppendRequest(StringBuilder buf, IHttpRequest req)
        {
            AppendCommon(buf, req);
            AppendInitialLine(buf, req);
            AppendHeaders(buf, req.Headers);
            RemoveLastNewLine(buf);
            return buf;
        }

        internal static StringBuilder AppendResponse(StringBuilder buf, IHttpResponse res)
        {
            AppendCommon(buf, res);
            AppendInitialLine(buf, res);
            AppendHeaders(buf, res.Headers);
            RemoveLastNewLine(buf);
            return buf;
        }

        static void AppendCommon(StringBuilder buf, IHttpMessage msg)
        {
            _ = buf.Append($"{StringUtil.SimpleClassName(msg)}");
            _ = buf.Append("(decodeResult: ");
            _ = buf.Append(msg.Result);
            _ = buf.Append(", version: ");
            _ = buf.Append(msg.ProtocolVersion);
            _ = buf.Append($"){StringUtil.Newline}");
        }

        internal static StringBuilder AppendFullRequest(StringBuilder buf, IFullHttpRequest req)
        {
            AppendFullCommon(buf, req);
            AppendInitialLine(buf, req);
            AppendHeaders(buf, req.Headers);
            AppendHeaders(buf, req.TrailingHeaders);
            RemoveLastNewLine(buf);
            return buf;
        }

        internal static StringBuilder AppendFullResponse(StringBuilder buf, IFullHttpResponse res)
        {
            AppendFullCommon(buf, res);
            AppendInitialLine(buf, res);
            AppendHeaders(buf, res.Headers);
            AppendHeaders(buf, res.TrailingHeaders);
            RemoveLastNewLine(buf);
            return buf;
        }

        static void AppendFullCommon(StringBuilder buf, IFullHttpMessage msg)
        {
            _ = buf.Append(StringUtil.SimpleClassName(msg));
            _ = buf.Append("(decodeResult: ");
            _ = buf.Append(msg.Result);
            _ = buf.Append(", version: ");
            _ = buf.Append(msg.ProtocolVersion);
            _ = buf.Append(", content: ");
            _ = buf.Append(msg.Content);
            _ = buf.Append(')');
            _ = buf.Append(StringUtil.Newline);
        }

        static void AppendInitialLine(StringBuilder buf, IHttpRequest req) => 
            buf.Append($"{req.Method} {req.Uri} {req.ProtocolVersion}{StringUtil.Newline}");

        static void AppendInitialLine(StringBuilder buf, IHttpResponse res) => 
            buf.Append($"{res.ProtocolVersion} {res.Status}{StringUtil.Newline}");

        static void AppendHeaders(StringBuilder buf, HttpHeaders headers)
        {
            foreach(HeaderEntry<AsciiString, ICharSequence> e in headers)
            {
                _ = buf.Append($"{e.Key}:{e.Value}{StringUtil.Newline}");
            }
        }

        static void RemoveLastNewLine(StringBuilder buf) => buf.Length = buf.Length - StringUtil.Newline.Length;
    }
}
