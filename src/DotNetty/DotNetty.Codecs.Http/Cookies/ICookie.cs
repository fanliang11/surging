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

    // http://en.wikipedia.org/wiki/HTTP_cookie
    public interface ICookie : IEquatable<ICookie>, IComparable<ICookie>, IComparable
    {
        string Name { get; }

        string Value { get; set; }

        /// <summary>
        /// Returns true if the raw value of this <see cref="ICookie"/>,
        /// was wrapped with double quotes in original Set-Cookie header.
        /// </summary>
        bool Wrap { get; set; }

        string Domain { get; set; }

        string Path { get; set; }

        long MaxAge { get; set; }

        bool IsSecure { get; set; }

        ///<summary>
        /// Checks to see if this Cookie can only be accessed via HTTP.
        /// If this returns true, the Cookie cannot be accessed through
        /// client side script - But only if the browser supports it.
        /// For more information, please look "http://www.owasp.org/index.php/HTTPOnly".
        ///</summary>
        bool IsHttpOnly { get; set; }
    }
}
