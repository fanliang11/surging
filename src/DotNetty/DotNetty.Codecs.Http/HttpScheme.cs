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

    public sealed class HttpScheme : IEquatable<HttpScheme>
    {
        // Scheme for non-secure HTTP connection.
        public static readonly HttpScheme Http = new HttpScheme(80, "http");

        // Scheme for secure HTTP connection.
        public static readonly HttpScheme Https = new HttpScheme(443, "https");

        readonly int port;
        readonly AsciiString name;

        HttpScheme(int port, string name)
        {
            this.port = port;
            this.name = AsciiString.Cached(name);
        }

        public AsciiString Name => this.name;

        public int Port => this.port;

        public override bool Equals(object obj)
        {
            if (obj is HttpScheme other)
            {
                return other.port == this.port && other.name.Equals(this.name);
            }

            return false;
        }

        public bool Equals(HttpScheme other)
        {
            if (ReferenceEquals(this, other)) { return true; }
            return other is object && other.port == this.port && other.name.Equals(this.name);
        }

        public override int GetHashCode() => this.port * 31 + this.name.GetHashCode();

        public override string ToString() => this.name.ToString();
    }
}
