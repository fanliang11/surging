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
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Internal;

    partial class AsciiString
    {
        public static readonly AsciiString Empty = Cached(string.Empty);
        private const int MaxCharValue = 255;
        internal const uint uMaxCharValue = 255u;
        const byte Replacement = (byte)'?';
        const uint uSpace = ' ';
        public const int IndexNotFound = -1;
        internal const uint NIndexNotFound = unchecked((uint)IndexNotFound);

        public static readonly IHashingStrategy<ICharSequence> CaseInsensitiveHasher = new CaseInsensitiveHashingStrategy();
        public static readonly IHashingStrategy<ICharSequence> CaseSensitiveHasher = new CaseSensitiveHashingStrategy();

        static readonly ICharEqualityComparator DefaultCharComparator = new DefaultCharEqualityComparator();
        static readonly ICharEqualityComparator GeneralCaseInsensitiveComparator = new GeneralCaseInsensitiveCharEqualityComparator();
        static readonly ICharEqualityComparator AsciiCaseInsensitiveCharComparator = new AsciiCaseInsensitiveCharEqualityComparator();

        sealed class CaseInsensitiveHashingStrategy : IHashingStrategy<ICharSequence>
        {
            public int HashCode(ICharSequence obj) => AsciiString.GetHashCode(obj);

            int IEqualityComparer<ICharSequence>.GetHashCode(ICharSequence obj) => this.HashCode(obj);

            public bool Equals(ICharSequence a, ICharSequence b) => ContentEqualsIgnoreCase(a, b);
        }

        sealed class CaseSensitiveHashingStrategy : IHashingStrategy<ICharSequence>
        {
            public int HashCode(ICharSequence obj) => AsciiString.GetHashCode(obj);

            int IEqualityComparer<ICharSequence>.GetHashCode(ICharSequence obj) => this.HashCode(obj);

            public bool Equals(ICharSequence a, ICharSequence b) => ContentEquals(a, b);
        }

        public static ICharSequence Trim(ICharSequence c)
        {
            if (c is AsciiString asciiString)
            {
                return asciiString.Trim();
            }

            if (0u >= (uint)c.Count) { return c; }

            int start = 0;
            int last = c.Count - 1;
            int end = last;
            uint uEnd = (uint)end;
            while ((uint)start <= uEnd && c[start] <= uSpace)
            {
                start++;
            }
            while (end >= start && c[end] <= uSpace)
            {
                end--;
            }
            if (0u >= (uint)start && end == last)
            {
                return c;
            }
            return c.SubSequence(start, end + 1);
        }

        public static AsciiString Of(string value) => new AsciiString(value);

        public static AsciiString Of(ICharSequence charSequence) => charSequence is AsciiString s ? s : new AsciiString(charSequence);

        public static AsciiString Cached(string value)
        {
            return new AsciiString(value)
            {
                stringValue = value
            };
        }

        public static int GetHashCode(ICharSequence value)
        {
            switch (value)
            {
                case null:
                    return 0;

                case AsciiString _:
                    return value.GetHashCode();

                default:
                    return PlatformDependent.HashCodeAscii(value);
            }
        }

        public static bool Contains(ICharSequence a, ICharSequence b) => Contains(a, b, DefaultCharComparator);

        public static bool ContainsIgnoreCase(ICharSequence a, ICharSequence b) => Contains(a, b, AsciiCaseInsensitiveCharComparator);

        public static bool ContentEqualsIgnoreCase(ICharSequence a, ICharSequence b)
        {
            if (ReferenceEquals(a, b)) { return true; }
            if (a is null || b is null) { return false; }

            if (a is AsciiString stringA)
            {
                return stringA.ContentEqualsIgnoreCase(b);
            }
            if (b is AsciiString stringB)
            {
                return stringB.ContentEqualsIgnoreCase(a);
            }

            if (a.Count != b.Count)
            {
                return false;
            }
            for (int i = 0; i < a.Count; ++i)
            {
                if (!EqualsIgnoreCase(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ContainsContentEqualsIgnoreCase(IReadOnlyList<ICharSequence> collection, ICharSequence value)
        {
            for (int idx = 0; idx < collection.Count; idx++)
            {
                if (ContentEqualsIgnoreCase(value, collection[idx]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsAllContentEqualsIgnoreCase(IReadOnlyList<ICharSequence> a, IReadOnlyList<AsciiString> b)
        {
            for (int idx = 0; idx < b.Count; idx++)
            {
                if (!ContainsContentEqualsIgnoreCase(a, b[idx]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ContentEquals(ICharSequence a, ICharSequence b)
        {
            if (a is null || b is null)
            {
                return ReferenceEquals(a, b);
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            if (a is AsciiString stringA)
            {
                return stringA.ContentEquals(b);
            }
            if (b is AsciiString stringB)
            {
                return stringB.ContentEquals(a);
            }

            for (int i = 0; i < a.Count; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        static bool Contains(ICharSequence a, ICharSequence b, ICharEqualityComparator comparator)
        {
            if (a is null || b is null || (uint)a.Count < (uint)b.Count)
            {
                return false;
            }
            if (0u >= (uint)b.Count)
            {
                return true;
            }

            int bStart = 0;
            for (int i = 0; i < a.Count; ++i)
            {
                if (comparator.CharEquals(b[bStart], a[i]))
                {
                    // If b is consumed then true.
                    if (++bStart == b.Count)
                    {
                        return true;
                    }
                }
                else if (a.Count - i < b.Count)
                {
                    // If there are not enough characters left in a for b to be contained, then false.
                    return false;
                }
                else
                {
                    bStart = 0;
                }
            }

            return false;
        }

        static bool RegionMatchesCharSequences(ICharSequence cs, int csStart,
            ICharSequence seq, int start, int length, ICharEqualityComparator charEqualityComparator)
        {
            //general purpose implementation for CharSequences
            if (csStart < 0 || length > cs.Count - csStart)
            {
                return false;
            }
            if (start < 0 || length > seq.Count - start)
            {
                return false;
            }

            int csIndex = csStart;
            int csEnd = csIndex + length;
            int stringIndex = start;

            while (csIndex < csEnd)
            {
                char c1 = cs[csIndex++];
                char c2 = seq[stringIndex++];

                if (!charEqualityComparator.CharEquals(c1, c2))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool RegionMatches(ICharSequence cs, bool ignoreCase, int csStart, ICharSequence seq, int start, int length)
        {
            if (cs is null || seq is null)
            {
                return false;
            }
            switch (cs)
            {
                case StringCharSequence stringCharSequence when seq is StringCharSequence:
                    return ignoreCase
                        ? stringCharSequence.RegionMatchesIgnoreCase(csStart, seq, start, length)
                        : stringCharSequence.RegionMatches(csStart, seq, start, length);

                case AsciiString asciiString:
                    return ignoreCase
                        ? asciiString.RegionMatchesIgnoreCase(csStart, seq, start, length)
                        : asciiString.RegionMatches(csStart, seq, start, length);

                default:
                    return RegionMatchesCharSequences(cs, csStart, seq, start, length,
                        ignoreCase ? GeneralCaseInsensitiveComparator : DefaultCharComparator);
            }
        }

        public static bool RegionMatchesAscii(ICharSequence cs, bool ignoreCase, int csStart, ICharSequence seq, int start, int length)
        {
            if (cs is null || seq is null)
            {
                return false;
            }

            switch (cs)
            {
                case StringCharSequence _ when !ignoreCase && seq is StringCharSequence:
                    //we don't call regionMatches from String for ignoreCase==true. It's a general purpose method,
                    //which make complex comparison in case of ignoreCase==true, which is useless for ASCII-only strings.
                    //To avoid applying this complex ignore-case comparison, we will use regionMatchesCharSequences
                    return cs.RegionMatches(csStart, seq, start, length);

                case AsciiString asciiString:
                    return ignoreCase
                        ? asciiString.RegionMatchesIgnoreCase(csStart, seq, start, length)
                        : asciiString.RegionMatches(csStart, seq, start, length);

                default:
                    return RegionMatchesCharSequences(cs, csStart, seq, start, length,
                        ignoreCase ? AsciiCaseInsensitiveCharComparator : DefaultCharComparator);
            }
        }

        public static int IndexOfIgnoreCase(ICharSequence str, ICharSequence searchStr, int startPos)
        {
            if (str is null || searchStr is null)
            {
                return IndexNotFound;
            }

            if (startPos < 0)
            {
                startPos = 0;
            }
            int searchStrLen = searchStr.Count;
            int endLimit = str.Count - searchStrLen + 1;
            if (startPos > endLimit)
            {
                return IndexNotFound;
            }
            if (0u >= (uint)searchStrLen)
            {
                return startPos;
            }
            for (int i = startPos; i < endLimit; i++)
            {
                if (RegionMatches(str, true, i, searchStr, 0, searchStrLen))
                {
                    return i;
                }
            }

            return IndexNotFound;
        }

        public static int IndexOfIgnoreCaseAscii(ICharSequence str, ICharSequence searchStr, int startPos)
        {
            if (str is null || searchStr is null)
            {
                return IndexNotFound;
            }

            if (startPos < 0)
            {
                startPos = 0;
            }
            int searchStrLen = searchStr.Count;
            int endLimit = str.Count - searchStrLen + 1;
            if (startPos > endLimit)
            {
                return IndexNotFound;
            }
            if (0u >= (uint)searchStrLen)
            {
                return startPos;
            }
            for (int i = startPos; i < endLimit; i++)
            {
                if (RegionMatchesAscii(str, true, i, searchStr, 0, searchStrLen))
                {
                    return i;
                }
            }

            return IndexNotFound;
        }

        public static int IndexOf(ICharSequence cs, char searchChar, int start)
        {
            switch (cs)
            {
                case StringCharSequence stringCharSequence:
                    return stringCharSequence.IndexOf(searchChar, start);

                case AsciiString asciiString:
                    return asciiString.IndexOf(searchChar, start);

                case null:
                    return IndexNotFound;
            }
            int sz = cs.Count;
            for (int i = start < 0 ? 0 : start; i < sz; i++)
            {
                if (cs[i] == searchChar)
                {
                    return i;
                }
            }
            return IndexNotFound;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static bool EqualsIgnoreCase(byte a, byte b)
        {
            var ua = (uint)a;
            var ub = (uint)b;
            return ua == ub || ToLowerCase0(ua) == ToLowerCase0(ub);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static bool EqualsIgnoreCase(char a, char b)
        {
            var ua = (uint)a;
            var ub = (uint)b;
            return ua == ub || ToLowerCase0(ua) == ToLowerCase0(ub);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static byte ToLowerCase(byte b) => unchecked((byte)ToLowerCase0(b));

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static byte ToLowerCase(uint b) => unchecked((byte)ToLowerCase0(b));
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static char ToLowerCase(char c) => unchecked((char)ToLowerCase0(c));
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static uint ToLowerCase0(uint b) => IsUpperCase(b) ? (b + 32u) : b;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static byte ToUpperCase(byte b) => unchecked((byte)ToUpperCase0(b));
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static byte ToUpperCase(uint b) => unchecked((byte)ToUpperCase0(b));
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static char ToUpperCase(char c) => unchecked((char)ToUpperCase0(c));
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static uint ToUpperCase0(uint b) => IsLowerCase(b) ? (b - 32u) : b;

        const uint DigitDiff = '9' - '0';
        const uint HexCharDiff = 'F' - 'A';
        const uint AsciiCharDiff = 'Z' - 'A';
        const uint Ascii0 = '0';
        const uint AsciiA = 'A';
        const uint Asciia = 'a';
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsLowerCase(byte value) => value - Asciia <= AsciiCharDiff;
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsLowerCase(uint value) => value - Asciia <= AsciiCharDiff;
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsLowerCase(char value) => value - Asciia <= AsciiCharDiff;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsUpperCase(byte value) => value - AsciiA <= AsciiCharDiff;
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsUpperCase(uint value) => value - AsciiA <= AsciiCharDiff;
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsUpperCase(char value) => value - AsciiA <= AsciiCharDiff;

        /// <summary>
        /// A hex digit is valid if it is in the range: [0..9] | [A..F] | [a..f]
        /// Otherwise, return false.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static bool IsHexDigit(byte value) => IsHexDigit((uint)value);
        public static bool IsHexDigit(char value) => IsHexDigit((uint)value);
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static bool IsHexDigit(uint value) =>
            (value - Ascii0) <= DigitDiff ||
            (value - AsciiA) <= HexCharDiff ||
            (value - Asciia) <= HexCharDiff;

        /// <summary>
        /// Returns <see langword="true"/> iff <paramref name="value"/> is in the range [0..9].
        /// Otherwise, returns <see langword="false"/>.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static bool IsDigit(byte value) => value - Ascii0 <= DigitDiff;
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static bool IsDigit(char value) => value - Ascii0 <= DigitDiff;
        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static bool IsDigit(uint value) => value - Ascii0 <= DigitDiff;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static byte CharToByte(char c) => c > uMaxCharValue ? Replacement : unchecked((byte)c);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static char ByteToChar(byte b) => (char)b;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowIndexOutOfRangeException_Start(int start, int length, int count)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("expected: 0 <= start({0}) <= start + length({1}) <= value.length({2})", start, length, count));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowIndexOutOfRangeException_StartEnd(int start, int end, int length)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("expected: 0 <= start({0}) <= end ({1}) <= length({2})", start, end, length));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowIndexOutOfRangeException_SrcIndex(int start, int count, int length)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("expected: 0 <= start({0}) <= srcIdx + length({1}) <= srcLen({2})", start, count, length));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowIndexOutOfRangeException_Index(int index, int length, int count)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("expected: 0 <= index({0} <= start + length({1}) <= length({2})", index, length, count));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowIndexOutOfRangeException_Index(int index, int length)
        {
            throw GetIndexOutOfRangeException();

            IndexOutOfRangeException GetIndexOutOfRangeException()
            {
                return new IndexOutOfRangeException(string.Format("index: {0} must be in the range [0,{1})", index, length));
            }
        }

        interface ICharEqualityComparator
        {
            bool CharEquals(char a, char b);
        }

        sealed class DefaultCharEqualityComparator : ICharEqualityComparator
        {
            public bool CharEquals(char a, char b) => a == b;
        }

        sealed class GeneralCaseInsensitiveCharEqualityComparator : ICharEqualityComparator
        {
            public bool CharEquals(char a, char b) =>
                char.ToUpper(a, CultureInfo.InvariantCulture) == char.ToUpper(b, CultureInfo.InvariantCulture) || char.ToLower(a, CultureInfo.InvariantCulture) == char.ToLower(b, CultureInfo.InvariantCulture);
        }

        sealed class AsciiCaseInsensitiveCharEqualityComparator : ICharEqualityComparator
        {
            public bool CharEquals(char a, char b) => EqualsIgnoreCase(a, b);
        }
    }
}
