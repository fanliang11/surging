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
    using DotNetty.Common.Utilities;

    public static class CookieHeaderNames
    {
        public static readonly AsciiString Path = AsciiString.Cached("Path");

        public static readonly AsciiString Expires = AsciiString.Cached("Expires");

        public static readonly AsciiString MaxAge = AsciiString.Cached("Max-Age");

        public static readonly AsciiString Domain = AsciiString.Cached("Domain");

        public static readonly AsciiString Secure = AsciiString.Cached("Secure");

        public static readonly AsciiString HttpOnly = AsciiString.Cached("HTTPOnly");

        public static readonly AsciiString SameSite = AsciiString.Cached("SameSite");
    }

    /// <summary>
    /// Possible values for the SameSite attribute.
    /// See <a href="https://tools.ietf.org/html/draft-ietf-httpbis-rfc6265bis-05">changes to RFC6265bis</a>
    /// </summary>
    public enum SameSite
    {
        None,
        Lax,
        Strict,
    }
}
