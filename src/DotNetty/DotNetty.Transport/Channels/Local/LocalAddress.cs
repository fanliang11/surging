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

namespace DotNetty.Transport.Channels.Local
{
    using System;
    using System.Net;
    using DotNetty.Common.Internal;

    public class LocalAddress : EndPoint, IComparable<LocalAddress>, IEquatable<LocalAddress>
    {
        public static readonly LocalAddress Any = new LocalAddress("ANY");

        private readonly string _id;
        private readonly string _strVal;

        internal LocalAddress(IChannel channel)
        {
            var buf = StringBuilderCache.Acquire(); // new StringBuilder(16);
            buf.Append("local:E");
            buf.Append((channel.GetHashCode() & 0xFFFFFFFFL | 0x100000000L).ToString("X"));
            buf[7] = ':';

            _strVal = StringBuilderCache.GetStringAndRelease(buf);
            _id = _strVal.Substring(6);
        }

        public LocalAddress(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.id);
            }
            _id = id.Trim().ToLowerInvariant();
            _strVal = $"local: {_id}";
        }

        public string Id => _id;

        public override bool Equals(object obj)
        {
            return Equals(obj as LocalAddress);
        }

        public bool Equals(LocalAddress other)
        {
            if (ReferenceEquals(this, other)) { return true; }
            return other is object && string.Equals(_strVal, other._strVal
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                        );
#else
                        , StringComparison.Ordinal);
#endif
        }

        public override int GetHashCode() => _id.GetHashCode();

        public override string ToString() => _strVal;

        public int CompareTo(LocalAddress other)
        {
            if (ReferenceEquals(this, other))
                return 0;

            if (other is null)
                return 1;

            return string.Compare(_id, other._id, StringComparison.Ordinal);
        }
    }
}
