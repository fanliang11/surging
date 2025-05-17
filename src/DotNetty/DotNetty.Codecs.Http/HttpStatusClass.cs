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
    using DotNetty.Common.Utilities;

    public sealed class HttpStatusClass : IEquatable<HttpStatusClass>
    {
        public static readonly HttpStatusClass Informational = new HttpStatusClass(100, 200, "Informational");

        public static readonly HttpStatusClass Success = new HttpStatusClass(200, 300, "Success");

        public static readonly HttpStatusClass Redirection = new HttpStatusClass(300, 400, "Redirection");

        public static readonly HttpStatusClass ClientError = new HttpStatusClass(400, 500, "Client Error");

        public static readonly HttpStatusClass ServerError = new HttpStatusClass(500, 600, "Server Error");

        public static readonly HttpStatusClass Unknown = new HttpStatusClass(0, 0, "Unknown Status");

        public static HttpStatusClass ValueOf(int code)
        {
            if (Contains(Informational, code))
            {
                return Informational;
            }
            if (Contains(Success, code))
            {
                return Success;
            }
            if (Contains(Redirection, code))
            {
                return Redirection;
            }
            if (Contains(ClientError, code))
            {
                return ClientError;
            }
            if (Contains(ServerError, code))
            {
                return ServerError;
            }
            return Unknown;
        }

        public static HttpStatusClass ValueOf(ICharSequence code)
        {
            if (code is object && code.Count == 3)
            {
                char c0 = code[0];
                return IsDigit(c0) && IsDigit(code[1]) && IsDigit(code[2])
                    ? ValueOf(Digit(c0) * 100)
                    : Unknown;
            }

            return Unknown;
        }

        static int Digit(char c) => c - '0';

        static bool IsDigit(char c) => c >= '0' && c <= '9';

        readonly int min;
        readonly int max;
        readonly AsciiString defaultReasonPhrase;

        HttpStatusClass(int min, int max, string defaultReasonPhrase)
        {
            this.min = min;
            this.max = max;
            this.defaultReasonPhrase = AsciiString.Cached(defaultReasonPhrase);
        }

        public bool Contains(int code) => Contains(this, code);

        public static bool Contains(HttpStatusClass httpStatusClass, int code)
        {
            if (0u >= (uint)(httpStatusClass.min & httpStatusClass.max))
            {
                return code < 100 || code >= 600;
            }

            return code >= httpStatusClass.min && code < httpStatusClass.max;
        }

        public AsciiString DefaultReasonPhrase => this.defaultReasonPhrase;

        public bool Equals(HttpStatusClass other)
        {
            if (ReferenceEquals(this, other)) { return true; }
            return other is object && this.min == other.min && this.max == other.max;
        }

        public override bool Equals(object obj) => obj is HttpStatusClass httpStatusClass && this.Equals(httpStatusClass);

        public override int GetHashCode() => this.min.GetHashCode() ^ this.max.GetHashCode();

        public static bool operator !=(HttpStatusClass left, HttpStatusClass right) => !(left == right);

        public static bool operator ==(HttpStatusClass left, HttpStatusClass right) => left.Equals(right);
    }
}
