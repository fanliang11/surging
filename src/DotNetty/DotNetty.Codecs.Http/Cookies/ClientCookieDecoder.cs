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
    using System;
    using DotNetty.Common.Utilities;

    public sealed class ClientCookieDecoder : CookieDecoder
    {
        // Strict encoder that validates that name and value chars are in the valid scope
        // defined in RFC6265
        public static readonly ClientCookieDecoder StrictDecoder = new ClientCookieDecoder(true);

        // Lax instance that doesn't validate name and value
        public static readonly ClientCookieDecoder LaxDecoder = new ClientCookieDecoder(false);

        ClientCookieDecoder(bool strict) : base(strict)
        {
        }

        public ICookie Decode(string header)
        {
            if (header is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.header); }

            int headerLen = header.Length;
            if (0u >= (uint)headerLen)
            {
                return null;
            }

            CookieBuilder cookieBuilder = null;
            //loop:
            for (int i = 0; ;)
            {

                // Skip spaces and separators.
                while (true)
                {
                    if (i == headerLen)
                    {
                        goto loop;
                    }
                    char c = header[i];
                    if (c == HttpConstants.CommaChar)
                    {
                        // Having multiple cookies in a single Set-Cookie header is
                        // deprecated, modern browsers only parse the first one
                        goto loop;

                    }
                    else if (IsSpace(c))
                    {
                        i++;
                        continue;
                    }
                    break;
                }

                int nameBegin = i;
                int nameEnd = 0;
                int valueBegin = 0;
                int valueEnd = 0;

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
                if (valueEnd > 0 && header[valueEnd - 1] == HttpConstants.CommaChar)
                {
                    // old multiple cookies separator, skipping it
                    valueEnd--;
                }

                if (cookieBuilder is null)
                {
                    // cookie name-value pair
                    DefaultCookie cookie = InitCookie(header, nameBegin, nameEnd, valueBegin, valueEnd);

                    if (cookie is null)
                    {
                        return null;
                    }

                    cookieBuilder = new CookieBuilder(cookie, header);
                }
                else
                {
                    // cookie attribute
                    cookieBuilder.AppendAttribute(nameBegin, nameEnd, valueBegin, valueEnd);
                }
            }

        loop:
            return cookieBuilder?.Cookie();
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
                case ';':
                    return true;
                default:
                    return false;
            }
        }

        sealed class CookieBuilder
        {
            private readonly string _header;
            private readonly DefaultCookie _cookie;
            private string _domain;
            private string _path;
            private long _maxAge = long.MinValue;
            private int _expiresStart;
            private int _expiresEnd;
            private bool _secure;
            private bool _httpOnly;
            private SameSite? _sameSite;

            internal CookieBuilder(DefaultCookie cookie, string header)
            {
                _cookie = cookie;
                _header = header;
            }

            long MergeMaxAgeAndExpires()
            {
                // max age has precedence over expires
                if (_maxAge != long.MinValue)
                {
                    return _maxAge;
                }
                else if (IsValueDefined(_expiresStart, _expiresEnd))
                {
                    DateTime? expiresDate = DateFormatter.ParseHttpDate(_header, _expiresStart, _expiresEnd);
                    if (expiresDate is object)
                    {
                        return (expiresDate.Value.Ticks - DateTime.UtcNow.Ticks) / TimeSpan.TicksPerSecond;
                    }
                }
                return long.MinValue;
            }

            internal ICookie Cookie()
            {
                _cookie.Domain = _domain;
                _cookie.Path = _path;
                _cookie.MaxAge = MergeMaxAgeAndExpires();
                _cookie.IsSecure = _secure;
                _cookie.IsHttpOnly = _httpOnly;
                _cookie.SameSite = _sameSite;
                return _cookie;
            }

            public void AppendAttribute(int keyStart, int keyEnd, int valueStart, int valueEnd)
            {
                int length = keyEnd - keyStart;

                switch (length)
                {
                    case 4:
                        Parse4(keyStart, valueStart, valueEnd);
                        break;

                    case 6:
                        Parse6(keyStart, valueStart, valueEnd);
                        break;

                    case 7:
                        Parse7(keyStart, valueStart, valueEnd);
                        break;

                    case 8:
                        Parse8(keyStart, valueStart, valueEnd);
                        break;
                }
            }

            void Parse4(int nameStart, int valueStart, int valueEnd)
            {
                if (CharUtil.RegionMatchesIgnoreCase(_header, nameStart, CookieHeaderNames.Path, 0, 4))
                {
                    _path = ComputeValue(valueStart, valueEnd);
                }
            }

            void Parse6(int nameStart, int valueStart, int valueEnd)
            {
                if (CharUtil.RegionMatchesIgnoreCase(_header, nameStart, CookieHeaderNames.Domain, 0, 5))
                {
                    _domain = ComputeValue(valueStart, valueEnd);
                }
                else if (CharUtil.RegionMatchesIgnoreCase(_header, nameStart, CookieHeaderNames.Secure, 0, 5))
                {
                    _secure = true;
                }
            }

            void SetMaxAge(string value)
            {
                if (long.TryParse(value, out long v))
                {
                    _maxAge = Math.Max(v, 0);
                }
            }

            void Parse7(int nameStart, int valueStart, int valueEnd)
            {
                if (CharUtil.RegionMatchesIgnoreCase(_header, nameStart, CookieHeaderNames.Expires, 0, 7))
                {
                    _expiresStart = valueStart;
                    _expiresEnd = valueEnd;
                }
                else if (CharUtil.RegionMatchesIgnoreCase(_header, nameStart, CookieHeaderNames.MaxAge, 0, 7))
                {
                    SetMaxAge(ComputeValue(valueStart, valueEnd));
                }
            }

            void Parse8(int nameStart, int valueStart, int valueEnd)
            {
                if (CharUtil.RegionMatchesIgnoreCase(_header, nameStart, CookieHeaderNames.HttpOnly, 0, 8))
                {
                    _httpOnly = true;
                }
                else if (CharUtil.RegionMatchesIgnoreCase(_header, nameStart, CookieHeaderNames.SameSite, 0, 8))
                {
                    _sameSite = (SameSite)Enum.Parse(typeof(SameSite), ComputeValue(valueStart, valueEnd), true);
                }
            }

            static bool IsValueDefined(int valueStart, int valueEnd) => valueStart != -1 && valueStart != valueEnd;

            string ComputeValue(int valueStart, int valueEnd) => IsValueDefined(valueStart, valueEnd)
                ? _header.Substring(valueStart, valueEnd - valueStart)
                : null;
        }
    }
}
