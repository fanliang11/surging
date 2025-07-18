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

namespace DotNetty.Common.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Internal;

    public sealed partial class StringCharSequence : ICharSequence, IEquatable<StringCharSequence>
    {
        public static readonly StringCharSequence Empty = new StringCharSequence(string.Empty);

        readonly string value;
        readonly int offset;
        readonly int count;

        public StringCharSequence(string value)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            this.value = value;
            this.offset = 0;
            this.count = this.value.Length;
        }

        public StringCharSequence(string value, int offset, int count)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
            if (MathUtil.IsOutOfBounds(offset, count, value.Length))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Index(offset, count, value.Length);
            }

            this.value = value;
            this.offset = offset;
            this.count = count;
        }

        public int Count => this.count;

        public static explicit operator string(StringCharSequence charSequence)
        {
            if (charSequence is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.charSequence); }
            return charSequence.ToString();
        }

        public static explicit operator StringCharSequence(string value)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            return (uint)value.Length > 0u ? new StringCharSequence(value) : Empty;
        }

        public ICharSequence SubSequence(int start) => this.SubSequence(start, this.count);

        public ICharSequence SubSequence(int start, int end)
        {
            if ((uint)end > (uint)this.count || (uint)start > (uint)end) { ThrowHelper.ThrowIndexOutOfRangeException(); }

            return end == start
                ? Empty
                : new StringCharSequence(this.value, this.offset + start, end - start);
        }

        public char this[int index]
        {
            get
            {
                if ((uint)index >= (uint)this.count) { ThrowHelper.ThrowIndexOutOfRangeException(); }
                return this.value[this.offset + index];
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public bool RegionMatches(int thisStart, ICharSequence seq, int start, int length)
            => CharUtil.RegionMatches(this, thisStart, seq, start, length);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public bool RegionMatchesIgnoreCase(int thisStart, ICharSequence seq, int start, int length)
            => CharUtil.RegionMatchesIgnoreCase(this, thisStart, seq, start, length);

        public int IndexOf(char ch, int start = 0)
        {
            var uCount = (uint)this.count;
            if (0u >= uCount) { return AsciiString.IndexNotFound; }

            if ((uint)start >= uCount)
            {
                ThrowHelper.ThrowIndexOutOfRangeException();
            }

            int index = this.value.IndexOf(ch, this.offset + start);
            return index < 0 ? index : index - this.offset;
        }

        public int IndexOf(string target, int start = 0) => this.value.IndexOf(target, StringComparison.Ordinal);

        public string ToString(int start)
        {
            var uCount = (uint)this.count;
            if (0u >= uCount) { return string.Empty; }
            if ((uint)start >= uCount) { ThrowHelper.ThrowIndexOutOfRangeException(); }

            return this.value.Substring(this.offset + start, this.count - start);
        }

        public override string ToString() => 0u >= (uint)this.count ? string.Empty : this.ToString(0);

        public bool Equals(StringCharSequence other)
        {
            if (ReferenceEquals(this, other)) { return true; }

            return other is object
                && this.count == other.count
                && 0u >= (uint)string.Compare(this.value, this.offset, other.value, other.offset, this.count, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) { return true; }

            if (obj is StringCharSequence other)
            {
                return this.count == other.count
                    && 0u >= (uint)string.Compare(this.value, this.offset, other.value, other.offset, this.count, StringComparison.Ordinal);
            }
            if (obj is ICharSequence seq)
            {
                return this.ContentEquals(seq);
            }

            return false;
        }

        public bool Equals(ICharSequence other)
        {
            if (ReferenceEquals(this, other)) { return true; }

            if (other is StringCharSequence comparand)
            {
                return this.count == comparand.count
                    && 0u >= (uint)string.Compare(this.value, this.offset, comparand.value, comparand.offset, this.count, StringComparison.Ordinal);
            }

            return other is object && this.ContentEquals(other);
        }

        public int HashCode(bool ignoreCase) => ignoreCase
            ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.ToString())
            : StringComparer.Ordinal.GetHashCode(this.ToString());

        public override int GetHashCode() => this.HashCode(false);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public bool ContentEquals(ICharSequence other) => CharUtil.ContentEquals(this, other);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public bool ContentEqualsIgnoreCase(ICharSequence other) => CharUtil.ContentEqualsIgnoreCase(this, other);

        public IEnumerator<char> GetEnumerator() => new CharSequenceEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
