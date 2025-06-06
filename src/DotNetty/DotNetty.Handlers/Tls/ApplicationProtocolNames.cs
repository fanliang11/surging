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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Handlers.Tls
{
    /// <summary>
    /// Provides a set of protocol names used in ALPN and NPN.
    /// @see <a href="https://tools.ietf.org/html/rfc7540#section-11.1">RFC7540 (HTTP/2)</a>
    /// @see <a href="https://tools.ietf.org/html/rfc7301#section-6">RFC7301 (TLS ALPN Extension)</a>
    /// @see <a href="https://tools.ietf.org/html/draft-agl-tls-nextprotoneg-04#section-7">TLS NPN Extension Draft</a>
    /// </summary>
    public static class ApplicationProtocolNames
    {
        /// <summary>
        /// <c>h2</c>: HTTP version 2
        /// </summary>
        public const string Http2 = "h2";

        /// <summary>
        /// <c>http/1.1</c>: HTTP version 1.1
        /// </summary>
        public const string Http11 = "http/1.1";

        /// <summary>
        /// <c>spdy/3.1</c>: SPDY version 3.1
        /// </summary>
        public const string Spdy31 = "spdy/3.1";

        /// <summary>
        /// <c>spdy/3</c>: SPDY version 3
        /// </summary>
        public const string Spdy3 = "spdy/3";

        /// <summary>
        /// <c>spdy/2</c>: SPDY version 2
        /// </summary>
        public const string Spdy2 = "spdy/2";

        /// <summary>
        /// <c>spdy/1</c>: SPDY version 1
        /// </summary>
        public const string Spdy1 = "spdy/1";
    }
}