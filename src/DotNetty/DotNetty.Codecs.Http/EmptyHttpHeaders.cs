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
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using DotNetty.Common.Utilities;

    public class EmptyHttpHeaders : HttpHeaders
    {
        static readonly IEnumerator<HeaderEntry<AsciiString, ICharSequence>> EmptryEnumerator = 
            Enumerable.Empty<HeaderEntry<AsciiString, ICharSequence>>().GetEnumerator();

        public static readonly EmptyHttpHeaders Default = new EmptyHttpHeaders();

        protected EmptyHttpHeaders()
        {
        }

        public override bool TryGet(AsciiString name, out ICharSequence value)
        {
            value = default;
            return false;
        }

        public override bool TryGetInt(AsciiString name, out int value)
        {
            value = default;
            return false;
        }

        public override int GetInt(AsciiString name, int defaultValue) => defaultValue;

        public override bool TryGetShort(AsciiString name, out short value)
        {
            value = default;
            return false;
        }

        public override short GetShort(AsciiString name, short defaultValue) => defaultValue;

        public override bool TryGetTimeMillis(AsciiString name, out long value)
        {
            value = default;
            return false;
        }

        public override long GetTimeMillis(AsciiString name, long defaultValue) => defaultValue;

        public override IList<ICharSequence> GetAll(AsciiString name) => ImmutableList<ICharSequence>.Empty;

        public override IList<HeaderEntry<AsciiString, ICharSequence>> Entries() => ImmutableList<HeaderEntry<AsciiString, ICharSequence>>.Empty;

        public override bool Contains(AsciiString name) => false;

        public override bool IsEmpty => true;

        public override int Size => 0;

        public override ISet<AsciiString> Names() => ImmutableHashSet<AsciiString>.Empty;

        public override HttpHeaders AddInt(AsciiString name, int value) => throw new NotSupportedException("read only");

        public override HttpHeaders AddShort(AsciiString name, short value) => throw new NotSupportedException("read only");

        public override HttpHeaders Set(AsciiString name, object value) => throw new NotSupportedException("read only");

        public override HttpHeaders Set(AsciiString name, IEnumerable<object> values) =>  throw new NotSupportedException("read only");

        public override HttpHeaders SetInt(AsciiString name, int value) => throw new NotSupportedException("read only");

        public override HttpHeaders SetShort(AsciiString name, short value) => throw new NotSupportedException("read only");

        public override HttpHeaders Remove(AsciiString name) => throw new NotSupportedException("read only");

        public override HttpHeaders Clear() => throw new NotSupportedException("read only");

        public override HttpHeaders Add(AsciiString name, object value) => throw new NotSupportedException("read only");

        public override IEnumerator<HeaderEntry<AsciiString, ICharSequence>> GetEnumerator() => EmptryEnumerator;
    }
}
