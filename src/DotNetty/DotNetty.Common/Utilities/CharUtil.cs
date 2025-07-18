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
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
#if !NET
    using System.Runtime.InteropServices;
    using DotNetty.Common.Internal;
#endif

    public static partial class CharUtil
    {
        public static readonly string Digits = "0123456789ABCDEF";

        public static readonly int MinRadix = 2;
        public static readonly int MaxRadix = 36;

        const string DigitKeys = "0Aa\u0660\u06f0\u0966\u09e6\u0a66\u0ae6\u0b66\u0be7\u0c66\u0ce6\u0d66\u0e50\u0ed0\u0f20\u1040\u1369\u17e0\u1810\uff10\uff21\uff41";
        static readonly char[] DigitValues = "90Z7zW\u0669\u0660\u06f9\u06f0\u096f\u0966\u09ef\u09e6\u0a6f\u0a66\u0aef\u0ae6\u0b6f\u0b66\u0bef\u0be6\u0c6f\u0c66\u0cef\u0ce6\u0d6f\u0d66\u0e59\u0e50\u0ed9\u0ed0\u0f29\u0f20\u1049\u1040\u1371\u1368\u17e9\u17e0\u1819\u1810\uff19\uff10\uff3a\uff17\uff5a\uff37".ToCharArray();

        public static int BinarySearchRange(string data, char c)
        {
            char value = '\u0000';
            int low = 0, mid = -1, high = data.Length - 1;
            while (low <= high)
            {
                mid = (low + high) >> 1;
                value = data[mid];
                if (c > value)
                    low = mid + 1;
                else if (c == value)
                    return mid;
                else
                    high = mid - 1;
            }

            return mid - (c < value ? 1 : 0);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsNullOrEmpty(ICharSequence sequence) => (sequence is null || 0u >= (uint)sequence.Count) ? true : false;

        public static ICharSequence[] Split(ICharSequence sequence, params char[] delimiters) => Split(sequence, 0, delimiters);

        internal static bool ContentEquals(ICharSequence left, ICharSequence right)
        {
            if (left is null || right is null) { return ReferenceEquals(left, right); }
            if (ReferenceEquals(left, right)) { return true; }
            if (left.Count != right.Count) { return false; }

            if (left is IHasAsciiSpan thisHasAscii && right is IHasAsciiSpan otherHasAscii)
            {
#if NET
                return thisHasAscii.AsciiSpan.SequenceEqual(otherHasAscii.AsciiSpan);
#else
                return SpanHelpers.SequenceEqual(
                    ref MemoryMarshal.GetReference(thisHasAscii.AsciiSpan),
                    ref MemoryMarshal.GetReference(otherHasAscii.AsciiSpan),
                    left.Count);
#endif
            }
            else if (left is IHasUtf16Span thisHasUtf16 && right is IHasUtf16Span otherHasUtf16)
            {
#if NET
                return thisHasUtf16.Utf16Span.SequenceEqual(otherHasUtf16.Utf16Span);
#else
                return SpanHelpers.SequenceEqual(
                    ref MemoryMarshal.GetReference(thisHasUtf16.Utf16Span),
                    ref MemoryMarshal.GetReference(otherHasUtf16.Utf16Span),
                    left.Count);
#endif
            }

            for (int i = 0; i < left.Count; i++)
            {
                char c1 = left[i];
                char c2 = right[i];
                if (c1 != c2
                    && char.ToUpper(c1).CompareTo(char.ToUpper(c2)) != 0
                    && char.ToLower(c1).CompareTo(char.ToLower(c2)) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool ContentEqualsIgnoreCase(ICharSequence left, ICharSequence right)
        {
            if (left is null || right is null) { return ReferenceEquals(left, right); }
            if (ReferenceEquals(left, right)) { return true; }

            if (left is IHasUtf16Span thisHasUtf16 && right is IHasUtf16Span otherHasUtf16)
            {
                return thisHasUtf16.Utf16Span.Equals(otherHasUtf16.Utf16Span, StringComparison.OrdinalIgnoreCase);
            }

            if (left.Count != right.Count) { return false; }

            for (int i = 0; i < left.Count; i++)
            {
                char c1 = left[i];
                char c2 = right[i];
                if (char.ToLower(c1).CompareTo(char.ToLower(c2)) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool RegionMatches(string value, int thisStart, ICharSequence other, int start, int length)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
            if (other is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other); }

            uint uLength = (uint)length;
            if (0u >= uLength) { return true; }
            if ((uint)thisStart > SharedConstants.TooBigOrNegative || uLength > (uint)(value.Length - thisStart)) { return false; }
            if ((uint)start > SharedConstants.TooBigOrNegative || uLength > (uint)(other.Count - start)) { return false; }

            if (other is IHasUtf16Span hasUtf16)
            {
#if NET
                return value.AsSpan().Slice(thisStart, length).SequenceEqual(hasUtf16.Utf16Span.Slice(start, length));
#else
                return SpanHelpers.SequenceEqual(
                    ref Unsafe.Add(ref MemoryMarshal.GetReference(value.AsSpan()), thisStart),
                    ref Unsafe.Add(ref MemoryMarshal.GetReference(hasUtf16.Utf16Span), start),
                    length);
#endif
            }
            int o1 = thisStart;
            int o2 = start;
            for (int i = 0; i < length; ++i)
            {
                if (value[o1 + i] != other[o2 + i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool RegionMatchesIgnoreCase(string value, int thisStart, ICharSequence other, int start, int length)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
            if (other is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other); }

            uint uLength = (uint)length;
            if (0u >= uLength) { return true; }
            if ((uint)thisStart > SharedConstants.TooBigOrNegative || uLength > (uint)(value.Length - thisStart)) { return false; }
            if ((uint)start > SharedConstants.TooBigOrNegative || uLength > (uint)(other.Count - start)) { return false; }

            if (other is IHasUtf16Span hasUtf16)
            {
                return value.AsSpan(thisStart, length).Equals(hasUtf16.Utf16Span.Slice(start, length), StringComparison.OrdinalIgnoreCase);
            }

            int end = thisStart + length;
            while (thisStart < end)
            {
                char c1 = value[thisStart++];
                char c2 = other[start++];
                if (c1 != c2
                    && char.ToUpper(c1).CompareTo(char.ToUpper(c2)) != 0
                    && char.ToLower(c1).CompareTo(char.ToLower(c2)) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool RegionMatches(ICharSequence value, int thisStart, ICharSequence other, int start, int length)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
            if (other is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other); }

            uint uLength = (uint)length;
            if (0u >= uLength) { return true; }
            if ((uint)thisStart > SharedConstants.TooBigOrNegative || uLength > (uint)(value.Count - thisStart)) { return false; }
            if ((uint)start > SharedConstants.TooBigOrNegative || uLength > (uint)(other.Count - start)) { return false; }

            if (value is IHasAsciiSpan thisHasAscii && other is IHasAsciiSpan otherHasAscii)
            {
#if NET
                return thisHasAscii.AsciiSpan.Slice(thisStart, length).SequenceEqual(otherHasAscii.AsciiSpan.Slice(start, length));
#else
                return SpanHelpers.SequenceEqual(
                    ref Unsafe.Add(ref MemoryMarshal.GetReference(thisHasAscii.AsciiSpan), thisStart),
                    ref Unsafe.Add(ref MemoryMarshal.GetReference(otherHasAscii.AsciiSpan), start),
                    length);
#endif
            }
            else if (value is IHasUtf16Span thisHasUtf16 && other is IHasUtf16Span otherHasUtf16)
            {
#if NET
                return thisHasUtf16.Utf16Span.Slice(thisStart, length).SequenceEqual(otherHasUtf16.Utf16Span.Slice(start, length));
#else
                return SpanHelpers.SequenceEqual(
                    ref Unsafe.Add(ref MemoryMarshal.GetReference(thisHasUtf16.Utf16Span), thisStart),
                    ref Unsafe.Add(ref MemoryMarshal.GetReference(otherHasUtf16.Utf16Span), start),
                    length);
#endif
            }

            int o1 = thisStart;
            int o2 = start;
            for (int i = 0; i < length; ++i)
            {
                if (value[o1 + i] != other[o2 + i])
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool RegionMatchesIgnoreCase(ICharSequence value, int thisStart, ICharSequence other, int start, int length)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
            if (other is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other); }

            uint uLength = (uint)length;
            if (0u >= uLength) { return true; }
            if ((uint)thisStart > SharedConstants.TooBigOrNegative || uLength > (uint)(value.Count - thisStart)) { return false; }
            if ((uint)start > SharedConstants.TooBigOrNegative || uLength > (uint)(other.Count - start)) { return false; }

            if (value is IHasUtf16Span thisHasUtf16 && other is IHasUtf16Span otherHasUtf16)
            {
                return thisHasUtf16.Utf16Span.Slice(thisStart, length).Equals(otherHasUtf16.Utf16Span.Slice(start, length), StringComparison.OrdinalIgnoreCase);
            }

            int end = thisStart + length;
            while (thisStart < end)
            {
                char c1 = value[thisStart++];
                char c2 = other[start++];
                if (c1 != c2
                    && char.ToUpper(c1).CompareTo(char.ToUpper(c2)) != 0
                    && char.ToLower(c1).CompareTo(char.ToLower(c2)) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static ICharSequence SubstringAfter(this ICharSequence value, char delim)
        {
            int pos = value.IndexOf(delim);
            return pos >= 0 ? value.SubSequence(pos + 1, value.Count) : null;
        }

        public static ICharSequence Trim(ICharSequence sequence)
        {
            if (sequence is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.sequence); }

            int length = sequence.Count;
            int start = IndexOfFirstNonWhiteSpace(sequence);
            if (start == length)
            {
                return StringCharSequence.Empty;
            }

            int last = IndexOfLastNonWhiteSpaceChar(sequence, start);

            length = last - start + 1;
            return length == sequence.Count
                ? sequence
                : sequence.SubSequence(start, last + 1);
        }

        static int IndexOfFirstNonWhiteSpace(IReadOnlyList<char> value)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            int i = 0;
            while (i < value.Count && char.IsWhiteSpace(value[i]))
            {
                i++;
            }

            return i;
        }

        static int IndexOfLastNonWhiteSpaceChar(IReadOnlyList<char> value, int start)
        {
            int i = value.Count - 1;
            while (i > start && char.IsWhiteSpace(value[i]))
            {
                i--;
            }

            return i;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int Digit(byte b)
        {
            const byte First = (byte)'0';
            const byte Last = (byte)'9';

            if (b < First || b > Last)
            {
                return -1;
            }

            return b - First;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static bool IsISOControl(int c) => (c >= 0 && c <= 0x1f) || (c >= 0x7f && c <= 0x9f);

        public static int IndexOf(this ICharSequence cs, char searchChar, int start)
        {
            switch (cs)
            {
                case null:
                    return AsciiString.IndexNotFound;

                case StringCharSequence sequence:
                    return sequence.IndexOf(searchChar, start);

                case AsciiString s:
                    return s.IndexOf(searchChar, start);

                default:
                    int sz = cs.Count;
                    if (start < 0)
                    {
                        start = 0;
                    }
                    for (int i = start; i < sz; i++)
                    {
                        if (cs[i] == searchChar)
                        {
                            return i;
                        }
                    }

                    return AsciiString.IndexNotFound;
            }
        }

        public static int CodePointAt(IReadOnlyList<char> seq, int index)
        {
            if (seq is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.seq); }
            if (/*index < 0 ||*/ (uint)index >= (uint)seq.Count) { ThrowHelper.ThrowIndexOutOfRangeException(); }

            char high = seq[index++];
            if (index >= seq.Count)
            {
                return high;
            }

            char low = seq[index];

            return IsSurrogatePair(high, low) ? ToCodePoint(high, low) : high;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int ToCodePoint(char high, char low)
        {
            // See RFC 2781, Section 2.2
            // http://www.faqs.org/rfcs/rfc2781.html
            int h = (high & 0x3FF) << 10;
            int l = low & 0x3FF;
            return (h | l) + 0x10000;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static bool IsSurrogatePair(char high, char low) => char.IsHighSurrogate(high) && char.IsLowSurrogate(low);

        internal static int IndexOf(IReadOnlyList<char> value, char ch, int start)
        {
            char upper = char.ToUpper(ch);
            char lower = char.ToLower(ch);
            int i = start;
            while (i < value.Count)
            {
                char c1 = value[i];
                if (c1 == ch
                    && char.ToUpper(c1).CompareTo(upper) != 0
                    && char.ToLower(c1).CompareTo(lower) != 0)
                {
                    return i;
                }

                i++;
            }

            return AsciiString.IndexNotFound;
        }
    }
}
