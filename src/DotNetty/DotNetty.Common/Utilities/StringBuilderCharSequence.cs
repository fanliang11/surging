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
    using System.Text;
    using DotNetty.Common.Internal;

    public sealed partial class StringBuilderCharSequence : ICharSequence, IEquatable<StringBuilderCharSequence>
    {
        internal readonly StringBuilder builder;
        readonly int offset;
        int size;

        public StringBuilderCharSequence(int capacity = 0)
        {
            if ((uint)capacity > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(capacity, ExceptionArgument.capacity); }

            this.builder = new StringBuilder(capacity);
            this.offset = 0;
            this.size = 0;
        }

        public StringBuilderCharSequence(StringBuilder builder) : this(builder, 0, builder.Length)
        {
        }

        public StringBuilderCharSequence(StringBuilder builder, int offset, int count)
        {
            if (builder is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.builder); }
            if (MathUtil.IsOutOfBounds(offset, count, builder.Length))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Index(offset, count, builder.Length);
            }

            this.builder = builder;
            this.offset = offset;
            this.size = count;
        }

        public ICharSequence SubSequence(int start) => this.SubSequence(start, this.size);

        public ICharSequence SubSequence(int start, int end)
        {
            if ((uint)start > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_StartIndex(ExceptionArgument.start);
            }
            if (end < start)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_EndIndexLessThanStartIndex();
            }
            if (end > this.size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_IndexLargerThanLength(ExceptionArgument.end);
            }

            return end == start
                ? new StringBuilderCharSequence()
                : new StringBuilderCharSequence(this.builder, this.offset + start, end - start);
        }

        public int Count => this.size;

        public char this[int index]
        {
            get
            {
                var uIdx = (uint)index;
                if (uIdx > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(index, ExceptionArgument.index); }
                if (uIdx >= (uint)this.size) { ThrowHelper.ThrowArgumentOutOfRangeException_IndexLargerThanLength(ExceptionArgument.index); }
                return this.builder[this.offset + index];
            }
        }

        public void Append(string value)
        {
            _ = this.builder.Append(value);
            this.size += value.Length;
        }

        public void Append(string value, int index, int count)
        {
            _ = this.builder.Append(value, index, count);
            this.size += count;
        }

        public void Append(ICharSequence value)
        {
            if (value is null || 0u >= (uint)value.Count)
            {
                return;
            }

            _ = this.builder.Append(value);
            this.size += value.Count;
        }

        public void Append(ICharSequence value, int index, int count)
        {
            if (value is null || 0u >= (uint)count)
            {
                return;
            }

            this.Append(value.SubSequence(index, index + count));
        }

#if NETCOREAPP || NETSTANDARD_2_0_GREATER
        public void Append(in ReadOnlySpan<char> value)
        {
            _ = this.builder.Append(value);
            this.size += value.Length;
        }

        public void Append(in ReadOnlyMemory<char> value)
        {
            _ = this.builder.Append(value);
            this.size += value.Length;
        }
#endif

        public void Append(char value)
        {
            _ = this.builder.Append(value);
            this.size++;
        }

        public void Insert(int start, char value)
        {
            uint uStart = (uint)start;
            if (uStart > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(start, ExceptionArgument.start); }
            if (uStart >= (uint)this.size) { ThrowHelper.ThrowArgumentOutOfRangeException_IndexLargerThanLength(ExceptionArgument.start); }

            _ = this.builder.Insert(this.offset + start, value);
            this.size++;
        }

        public bool RegionMatches(int thisStart, ICharSequence seq, int start, int length) =>
            CharUtil.RegionMatches(this, this.offset + thisStart, seq, start, length);

        public bool RegionMatchesIgnoreCase(int thisStart, ICharSequence seq, int start, int length) =>
            CharUtil.RegionMatchesIgnoreCase(this, this.offset + thisStart, seq, start, length);

        public int IndexOf(char ch, int start = 0) => CharUtil.IndexOf(this, ch, start);

        public string ToString(int start)
        {
            var uStart = (uint)start;
            if (uStart > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(start, ExceptionArgument.start); }
            if (uStart >= (uint)this.size) { ThrowHelper.ThrowArgumentOutOfRangeException_IndexLargerThanLength(ExceptionArgument.start); }


            return this.builder.ToString(this.offset + start, this.size - start);
        }

        public override string ToString() => 0u >= (uint)this.size ? string.Empty : this.ToString(0);

        public bool Equals(StringBuilderCharSequence other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is object && this.size == other.size && string.Equals(this.builder.ToString(this.offset, this.size), other.builder.ToString(other.offset, this.size)
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                );
#else
                , StringComparison.Ordinal);
#endif
        }

        bool IEquatable<ICharSequence>.Equals(ICharSequence other)
        {
            if (ReferenceEquals(this, other)) { return true; }

            switch (other)
            {
                case StringBuilderCharSequence comparand:
                    return this.size == comparand.size && string.Equals(this.builder.ToString(this.offset, this.size), comparand.builder.ToString(comparand.offset, this.size)
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                        );
#else
                        , StringComparison.Ordinal);
#endif
                case IHasUtf16Span hasUtf16:
                    return this.Span.SequenceEqual(hasUtf16.Utf16Span);

                default:
                    return other is ICharSequence seq && this.ContentEquals(seq);
            }

        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            switch (obj)
            {
                case StringBuilderCharSequence other:
                    return this.size == other.size && string.Equals(this.builder.ToString(this.offset, this.size), other.builder.ToString(other.offset, this.size)
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                        );
#else
                        , StringComparison.Ordinal);
#endif
                case ICharSequence seq:
                    return this.ContentEquals(seq);
                default:
                    return false;
            }
        }

        public int HashCode(bool ignoreCase) => ignoreCase
            ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.ToString())
            : StringComparer.Ordinal.GetHashCode(this.ToString());

        public override int GetHashCode() => this.HashCode(true);

        public bool ContentEquals(ICharSequence other) => CharUtil.ContentEquals(this, other);

        public bool ContentEqualsIgnoreCase(ICharSequence other) => CharUtil.ContentEqualsIgnoreCase(this, other);

        public IEnumerator<char> GetEnumerator() => new CharSequenceEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public ReadOnlySpan<char> Span => this.builder.ToString(this.offset, this.size).AsSpan();
    }
}
