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

    partial class CharUtil
    {
        [Obsolete("=> ICharSequenceExtensions::Contains")]
        public static bool Contains(IReadOnlyList<char> value, char c)
        {
            if (value is object)
            {
                int length = value.Count;
                for (int i = 0; i < length; i++)
                {
                    if (value[i] == c)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static int ParseInt(ICharSequence seq, int start, int end, int radix)
        {
            if (seq is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.seq); }
            if (radix < MinRadix || radix > MaxRadix) { ThrowHelper.ThrowIndexOutOfRangeException(); }

            if (start == end)
            {
                ThrowHelper.ThrowFormatException();
            }

            int i = start;
            bool negative = seq[i] == '-';
            if (negative && ++i == end)
            {
                ThrowHelper.ThrowFormatException(seq, start, end);
            }

            return ParseInt(seq, i, end, radix, negative);
        }

        public static int ParseInt(ICharSequence seq) => ParseInt(seq, 0, seq.Count, 10, false);

        public static int ParseInt(ICharSequence seq, int start, int end, int radix, bool negative)
        {
            if (seq is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.seq); }
            if (radix < MinRadix || radix > MaxRadix) { ThrowHelper.ThrowIndexOutOfRangeException(); }

            int max = int.MinValue / radix;
            int result = 0;
            int currOffset = start;
            while (currOffset < end)
            {
                int digit = Digit((char)(seq[currOffset++] & 0xFF), radix);
                if (digit == -1)
                {
                    ThrowHelper.ThrowFormatException(seq, start, end);
                }
                if (max > result)
                {
                    ThrowHelper.ThrowFormatException(seq, start, end);
                }
                int next = result * radix - digit;
                if (next > result)
                {
                    ThrowHelper.ThrowFormatException(seq, start, end);
                }
                result = next;
            }

            if (!negative)
            {
                result = -result;
                if (result < 0)
                {
                    ThrowHelper.ThrowFormatException(seq, start, end);
                }
            }

            return result;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static long ParseLong(ICharSequence str, int radix = 10)
        {
            if (str is AsciiString asciiString)
            {
                return asciiString.ParseLong(radix);
            }

            if (str is null
                || radix < MinRadix
                || radix > MaxRadix)
            {
                ThrowFormatException0(str);
            }

            // ReSharper disable once PossibleNullReferenceException
            int length = str.Count;
            int i = 0;
            if (0u >= (uint)length)
            {
                ThrowFormatException0(str);
            }
            bool negative = str[i] == '-';
            if (negative && ++i == length)
            {
                ThrowFormatException0(str);
            }

            return ParseLong(str, i, radix, negative);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static long ParseLong(ICharSequence str, int offset, int radix, bool negative)
        {
            long max = long.MinValue / radix;
            long result = 0, length = str.Count;
            while (offset < length)
            {
                int digit = Digit(str[offset++], radix);
                if (digit == -1)
                {
                    ThrowFormatException0(str);
                }
                if (max > result)
                {
                    ThrowFormatException0(str);
                }
                long next = result * radix - digit;
                if (next > result)
                {
                    ThrowFormatException0(str);
                }
                result = next;
            }

            if (!negative)
            {
                result = -result;
                if (result < 0)
                {
                    ThrowFormatException0(str);
                }
            }

            return result;
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static int Digit(char c, int radix)
        {
            if (radix >= MinRadix && radix <= MaxRadix)
            {
                if (c < 128)
                {
                    int result = -1;
                    if ('0' <= c && c <= '9')
                    {
                        result = c - '0';
                    }
                    else if ('a' <= c && c <= 'z')
                    {
                        result = c - ('a' - 10);
                    }
                    else if ('A' <= c && c <= 'Z')
                    {
                        result = c - ('A' - 10);
                    }

                    return result < radix ? result : -1;
                }

                int result1 = BinarySearchRange(DigitKeys, c);
                if (result1 >= 0 && c <= DigitValues[result1 * 2])
                {
                    int value = (char)(c - DigitValues[result1 * 2 + 1]);
                    if (value >= radix)
                    {
                        return -1;
                    }
                    return value;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFormatException0(ICharSequence str)
        {
            throw GetFormatException();
            FormatException GetFormatException()
            {
                return new FormatException(str.ToString());
            }
        }
    }
}
