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
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;

    using static CookieUtil;

    // http://tools.ietf.org/html/rfc6265 compliant cookie encoder to be used server side,
    // so some fields are sent (Version is typically ignored).
    // 
    // As Netty's Cookie merges Expires and MaxAge into one single field, only Max-Age field is sent.
    // Note that multiple cookies must be sent as separate "Set-Cookie" headers.
    public sealed class ServerCookieEncoder : CookieEncoder
    {
        // 
        // Strict encoder that validates that name and value chars are in the valid scope
        // defined in RFC6265, and(for methods that accept multiple cookies) that only
        // one cookie is encoded with any given name. (If multiple cookies have the same
        // name, the last one is the one that is encoded.)
        // 
        public static readonly ServerCookieEncoder StrictEncoder = new ServerCookieEncoder(true);

        // 
        // Lax instance that doesn't validate name and value, and that allows multiple
        // cookies with the same name.
        // 
        public static readonly ServerCookieEncoder LaxEncoder = new ServerCookieEncoder(false);

        ServerCookieEncoder(bool strict) : base(strict)
        {
        }

        public string Encode(string name, string value) => this.Encode(new DefaultCookie(name, value));

        public string Encode(ICookie cookie)
        {
            if (cookie is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.cookie); }

            string name = cookie.Name ?? nameof(cookie);
            string value = cookie.Value ?? "";

            this.ValidateCookie(name, value);

            StringBuilder buf = StringBuilder();

            if (cookie.Wrap)
            {
                AddQuoted(buf, name, value);
            }
            else
            {
                Add(buf, name, value);
            }

            if (cookie.MaxAge != long.MinValue)
            {
                Add(buf, (string)CookieHeaderNames.MaxAge, cookie.MaxAge);
                DateTime expires = DateTime.UtcNow.AddMilliseconds(cookie.MaxAge * 1000);
                _ = buf
                    .Append(CookieHeaderNames.Expires)
                    .Append(HttpConstants.EqualsSignChar);
                _ = DateFormatter.Append(expires, buf);
                _ = buf
                    .Append(HttpConstants.SemicolonChar)
                    .Append(HttpConstants.HorizontalSpaceChar);
            }

            if (cookie.Path is object)
            {
                Add(buf, (string)CookieHeaderNames.Path, cookie.Path);
            }

            if (cookie.Domain is object)
            {
                Add(buf, (string)CookieHeaderNames.Domain, cookie.Domain);
            }

            if (cookie.IsSecure)
            {
                Add(buf, (string)CookieHeaderNames.Secure);
            }

            if (cookie.IsHttpOnly)
            {
                Add(buf, (string)CookieHeaderNames.HttpOnly);
            }

            if(cookie is DefaultCookie defaultCookie && defaultCookie.SameSite.HasValue)
            {
                Add(buf, (string)CookieHeaderNames.SameSite, defaultCookie.SameSite.Value.ToString());
            }

            return StripTrailingSeparator(buf);
        }

        static List<string> Dedup(IReadOnlyList<string> encoded, IDictionary<string, int> nameToLastIndex)
        {
            var isLastInstance = new int[encoded.Count/32];
            foreach (int idx in nameToLastIndex.Values)
            {
                //isLastInstance[idx] = true;
                isLastInstance[idx >> 5] |= 1 << (idx & 0x1F);
            }

            var dedupd = new List<string>(nameToLastIndex.Count);
            for (int i = 0, n = encoded.Count; i < n; i++)
            {
                if ((isLastInstance[i >> 5] & (1 << (i & 0x1F))) != 0)
                {
                    dedupd.Add(encoded[i]);
                }
            }
            return dedupd;
        }

        public IList<string> Encode(params ICookie[] cookies)
        {
            if (cookies is null || 0u >= (uint)cookies.Length)
            {
                return ImmutableList<string>.Empty;
            }

            var encoded = new List<string>(cookies.Length);
            Dictionary<string, int> nameToIndex = this.Strict && (uint)cookies.Length > 1u ? new Dictionary<string, int>(StringComparer.Ordinal) : null;
            bool hasDupdName = false;
            for (int i = 0; i < cookies.Length; i++)
            {
                ICookie c = cookies[i];
                encoded.Add(this.Encode(c));
                if (nameToIndex is object)
                {
                    if (nameToIndex.ContainsKey(c.Name))
                    {
                        nameToIndex[c.Name] = i;
                        hasDupdName = true;
                    }
                    else
                    {
                        nameToIndex.Add(c.Name, i);
                    }
                }
            }
            return hasDupdName ? Dedup(encoded, nameToIndex) : encoded;
        }

        public IList<string> Encode(ICollection<ICookie> cookies)
        {
            if (cookies is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.cookies); }
            if (0u >= (uint)cookies.Count)
            {
                return ImmutableList<string>.Empty;
            }

            var encoded = new List<string>();
            var nameToIndex = new Dictionary<string, int>(StringComparer.Ordinal);
            bool hasDupdName = false;
            int i = 0;
            foreach (ICookie c in cookies)
            {
                encoded.Add(this.Encode(c));
                if (nameToIndex.ContainsKey(c.Name))
                {
                    nameToIndex[c.Name] = i;
                    hasDupdName = true;
                }
                else
                {
                    nameToIndex.Add(c.Name, i);
                }
                i++;
            }
            return hasDupdName ? Dedup(encoded, nameToIndex) : encoded;
        }
    }
}
