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
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.Cookies
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    // http://tools.ietf.org/html/rfc6265 
    // compliant cookie decoder to be used server side.
    //
    // http://tools.ietf.org/html/rfc2965 
    // cookies are still supported,old fields will simply be ignored.
    public sealed class ServerCookieDecoder : CookieDecoder
    {
        static readonly AsciiString RFC2965Version = new AsciiString("$Version");
        static readonly AsciiString RFC2965Path = new AsciiString($"${CookieHeaderNames.Path}");
        static readonly AsciiString RFC2965Domain = new AsciiString($"${CookieHeaderNames.Domain}");
        static readonly AsciiString RFC2965Port = new AsciiString("$Port");

        //
        // Strict encoder that validates that name and value chars are in the valid scope
        // defined in RFC6265
        //
        public static readonly ServerCookieDecoder StrictDecoder = new ServerCookieDecoder(true);

        //
        // Lax instance that doesn't validate name and value
        //
        public static readonly ServerCookieDecoder LaxDecoder = new ServerCookieDecoder(false);

        ServerCookieDecoder(bool strict) : base(strict)
        {
        }

        /// <summary>
        /// Decodes the specified Set-Cookie HTTP header value into a <see cref="ICookie"/>.  Unlike <see cref="Decode(string)"/>, this
        /// includes all cookie values present, even if they have the same name.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public IReadOnlyList<ICookie> DecodeAll(string header)
        {
            if (header is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.header); }
            if (0u >= (uint)header.Length) { return EmptyArray<ICookie>.Instance; }

            var cookies = new List<ICookie>();
            Decode(header, cookies);
            return cookies;
        }

        /// <summary>
        /// Decodes the specified Set-Cookie HTTP header value into a <see cref="ICookie"/>.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public ISet<ICookie> Decode(string header)
        {
            if (header is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.header); }
            if (0u >= (uint)header.Length) { return ImmutableHashSet<ICookie>.Empty; }

            var cookies = new SortedSet<ICookie>();
            Decode(header, cookies);
            return cookies;
        }

        private void Decode(string header, ICollection<ICookie> cookies)
        {
            int headerLen = header.Length;
            int i = 0;

            bool rfc2965Style = false;
            if (CharUtil.RegionMatchesIgnoreCase(header, 0, RFC2965Version, 0, RFC2965Version.Count))
            {
                // RFC 2965 style cookie, move to after version value
                i = header.IndexOf(';') + 1;
                rfc2965Style = true;
            }

            // loop
            while (true)
            {
                // Skip spaces and separators.
                while (true)
                {
                    if (i == headerLen)
                    {
                        goto loop;
                    }
                    char c = header[i];
                    if (IsSpace(c))
                    {
                        i++;
                        continue;
                    }
                    break;
                }

                int nameBegin = i;
                int nameEnd;
                int valueBegin;
                int valueEnd;

                while (true)
                {
                    char curChar = header[i];
                    switch (curChar)
                    {
                        case HttpConstants.SemicolonChar:
                            // NAME; (no value till ';')
                            nameEnd = i;
                            valueBegin = valueEnd = -1;
                            goto loop0;

                        case HttpConstants.EqualsSignChar:
                            // NAME=VALUE
                            nameEnd = i;
                            i++;
                            if (i == headerLen)
                            {
                                // NAME= (empty value, i.e. nothing after '=')
                                valueBegin = valueEnd = 0;
                                goto loop0;
                            }

                            valueBegin = i;
                            // NAME=VALUE;
                            int semiPos = header.IndexOf(';', i);
                            valueEnd = i = semiPos > 0 ? semiPos : headerLen;
                            goto loop0;

                        default:
                            i++;
                            break;
                    }

                    if (i == headerLen)
                    {
                        // NAME (no value till the end of string)
                        nameEnd = headerLen;
                        valueBegin = valueEnd = -1;
                        break;
                    }
                }
            loop0:
                if (rfc2965Style && (CharUtil.RegionMatches(header, nameBegin, RFC2965Path, 0, RFC2965Path.Count)
                        || CharUtil.RegionMatches(header, nameBegin, RFC2965Domain, 0, RFC2965Domain.Count)
                        || CharUtil.RegionMatches(header, nameBegin, RFC2965Port, 0, RFC2965Port.Count)))
                {
                    // skip obsolete RFC2965 fields
                    continue;
                }

                DefaultCookie cookie = InitCookie(header, nameBegin, nameEnd, valueBegin, valueEnd);
                if (cookie is object)
                {
                    cookies.Add(cookie);
                }
            }

        loop:
            return;
        }

        static bool IsSpace(char c)
        {
            switch (c)
            {
                case '\t':
                case '\n':
                case (char)0x0b:
                case '\f':
                case '\r':
                case ' ':
                case ',':
                case ';':
                    return true;
                default:
                    return false;
            }
        }
    }
}
