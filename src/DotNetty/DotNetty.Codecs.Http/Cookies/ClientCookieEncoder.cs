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
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using static CookieUtil;

    public sealed class ClientCookieEncoder : CookieEncoder
    {
        // Strict encoder that validates that name and value chars are in the valid scope and (for methods that accept
        // multiple cookies) sorts cookies into order of decreasing path length, as specified in RFC6265.
        public static readonly ClientCookieEncoder StrictEncoder = new ClientCookieEncoder(true);

        // Lax instance that doesn't validate name and value, and (for methods that accept multiple cookies) keeps
        // cookies in the order in which they were given.
        public static readonly ClientCookieEncoder LaxEncoder = new ClientCookieEncoder(false);

        internal static readonly IComparer<ICookie> Comparer = new CookieComparer();

        ClientCookieEncoder(bool strict) : base(strict)
        {
        }

        public string Encode(string name, string value) => this.Encode(new DefaultCookie(name, value));

        public string Encode(ICookie cookie)
        {
            if (cookie is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.cookie); }

            StringBuilder buf = StringBuilder();
            this.Encode(buf, cookie);
            return StripTrailingSeparator(buf);
        }

        sealed class CookieComparer : IComparer<ICookie>
        {
            public int Compare(ICookie c1, ICookie c2)
            {
                Debug.Assert(c1 is object && c2 is object);

                string path1 = c1.Path;
                string path2 = c2.Path;
                // Cookies with unspecified path default to the path of the request. We don't
                // know the request path here, but we assume that the length of an unspecified
                // path is longer than any specified path (i.e. pathless cookies come first),
                // because setting cookies with a path longer than the request path is of
                // limited use.
                int len1 = path1?.Length ?? int.MaxValue;
                int len2 = path2?.Length ?? int.MaxValue;

                // Rely on Arrays.sort's stability to retain creation order in cases where
                // cookies have same path length.
                return len2 - len1;
            }
        }

        public string Encode(params ICookie[] cookies)
        {
            if (cookies is null || 0u >= (uint)cookies.Length)
            {
                return null;
            }

            StringBuilder buf = StringBuilder();
            if (this.Strict)
            {
                if (cookies.Length == 1)
                {
                    this.Encode(buf, cookies[0]);
                }
                else
                {
                    var cookiesSorted = new ICookie[cookies.Length];
                    Array.Copy(cookies, cookiesSorted, cookies.Length);
                    Array.Sort(cookiesSorted, Comparer);
                    foreach(ICookie c in cookiesSorted)
                    {
                        this.Encode(buf, c);
                    }
                }
            }
            else
            {
                foreach (ICookie c in cookies)
                {
                    this.Encode(buf, c);
                }
            }
            return StripTrailingSeparatorOrNull(buf);
        }

        public string Encode(IEnumerable<ICookie> cookies)
        {
            if (cookies is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.cookies); }

            StringBuilder buf = StringBuilder();
            if (this.Strict)
            {
                var cookiesList = cookies.ToList();
                cookiesList.Sort(Comparer);
                foreach (ICookie c in cookiesList)
                {
                    this.Encode(buf, c);
                }
            }
            else
            {
                foreach (ICookie cookie in cookies)
                {
                    this.Encode(buf, cookie);
                }
            }
            return StripTrailingSeparatorOrNull(buf);
        }

        void Encode(StringBuilder buf, ICookie c)
        {
            string name = c.Name;
            string value = c.Value?? "";

            this.ValidateCookie(name, value);

            if (c.Wrap)
            {
                AddQuoted(buf, name, value);
            }
            else
            {
                Add(buf, name, value);
            }
        }
    }
}
