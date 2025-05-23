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

    partial class AsciiString
    {
        public bool ParseBoolean() => this.length >= 1 && this.value[this.offset] != 0;

        public char ParseChar() => this.ParseChar(0);

        public char ParseChar(int start)
        {
            if (start + 1 >= this.length)
            {
                ThrowHelper.ThrowIndexOutOfRangeException_ParseChar(start);
            }

            int startWithOffset = start + this.offset;

            return (char)((ByteToChar(this.value[startWithOffset]) << 8)
                | ByteToChar(this.value[startWithOffset + 1]));
        }

        public short ParseShort() => this.ParseShort(0, this.length, 10);

        public short ParseShort(int radix) => this.ParseShort(0, this.length, radix);

        public short ParseShort(int start, int end) => this.ParseShort(start, end, 10);

        public short ParseShort(int start, int end, int radix)
        {
            int intValue = this.ParseInt(start, end, radix);
            short result = (short)intValue;
            if (result != intValue)
            {
                ThrowHelper.ThrowFormatException(this, start, end);
            }

            return result;
        }

        public int ParseInt() => this.ParseInt(0, this.length, 10);

        public int ParseInt(int radix) => this.ParseInt(0, this.length, radix);

        public int ParseInt(int start, int end) => this.ParseInt(start, end, 10);

        public int ParseInt(int start, int end, int radix)
        {
            if (radix < CharUtil.MinRadix || radix > CharUtil.MaxRadix)
            {
                ThrowHelper.ThrowFormatException_Radix();
            }
            if (start == end)
            {
                ThrowHelper.ThrowFormatException(start, end);
            }

            int i = start;
            bool negative = this.ByteAt(i) == '-';
            if (negative && ++i == end)
            {
                ThrowHelper.ThrowFormatException(this, start, end);
            }

            return this.ParseInt(i, end, radix, negative);
        }

        int ParseInt(int start, int end, int radix, bool negative)
        {
            int max = int.MinValue / radix;
            int result = 0;
            int currOffset = start;
            while (currOffset < end)
            {
                int digit = CharUtil.Digit((char)(this.value[currOffset++ + this.offset]), radix);
                if (digit == -1)
                {
                    ThrowHelper.ThrowFormatException(this, start, end);
                }
                if (max > result)
                {
                    ThrowHelper.ThrowFormatException(this, start, end);
                }
                int next = result * radix - digit;
                if (next > result)
                {
                    ThrowHelper.ThrowFormatException(this, start, end);
                }
                result = next;
            }

            if (!negative)
            {
                result = -result;
                if (result < 0)
                {
                    ThrowHelper.ThrowFormatException(this, start, end);
                }
            }

            return result;
        }

        public long ParseLong() => this.ParseLong(0, this.length, 10);

        public long ParseLong(int radix) => this.ParseLong(0, this.length, radix);

        public long ParseLong(int start, int end) => this.ParseLong(start, end, 10);

        public long ParseLong(int start, int end, int radix)
        {
            if (radix < CharUtil.MinRadix || radix > CharUtil.MaxRadix)
            {
                ThrowHelper.ThrowFormatException_Radix();
            }

            if (start == end)
            {
                ThrowHelper.ThrowFormatException(start, end);
            }

            int i = start;
            bool negative = this.ByteAt(i) == '-';
            if (negative && ++i == end)
            {
                ThrowHelper.ThrowFormatException(this, start, end);
            }

            return this.ParseLong(i, end, radix, negative);
        }

        long ParseLong(int start, int end, int radix, bool negative)
        {
            long max = long.MinValue / radix;
            long result = 0;
            int currOffset = start;
            while (currOffset < end)
            {
                int digit = CharUtil.Digit((char)(this.value[currOffset++ + this.offset]), radix);
                if (digit == -1)
                {
                    ThrowHelper.ThrowFormatException(this, start, end);
                }
                if (max > result)
                {
                    ThrowHelper.ThrowFormatException(this, start, end);
                }
                long next = result * radix - digit;
                if (next > result)
                {
                    ThrowHelper.ThrowFormatException(this, start, end);
                }
                result = next;
            }

            if (!negative)
            {
                result = -result;
                if (result < 0)
                {
                    ThrowHelper.ThrowFormatException(this, start, end);
                }
            }

            return result;
        }

        public float ParseFloat() => this.ParseFloat(0, this.length);

        public float ParseFloat(int start, int end) => Convert.ToSingle(this.ToString(start, end));

        public double ParseDouble() => this.ParseDouble(0, this.length);

        public double ParseDouble(int start, int end) => Convert.ToDouble(this.ToString(start, end));
    }
}
