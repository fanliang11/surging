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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs
{
    using System;
    using System.Globalization;
    using DotNetty.Common.Utilities;

    public class CharSequenceValueConverter : IValueConverter<ICharSequence>
    {
        public static readonly CharSequenceValueConverter Default = new CharSequenceValueConverter();
        static readonly AsciiString TrueAscii = new AsciiString("true");

        public virtual ICharSequence ConvertObject(object value)
        {
            if (value is ICharSequence sequence)
            {
                return sequence;
            }
            return new StringCharSequence(value.ToString());
        }

        public ICharSequence ConvertInt(int value) => new StringCharSequence(value.ToString());

        public ICharSequence ConvertLong(long value) => new StringCharSequence(value.ToString());

        public ICharSequence ConvertDouble(double value) => new StringCharSequence(value.ToString(CultureInfo.InvariantCulture));

        public ICharSequence ConvertChar(char value) => new StringCharSequence(value.ToString());

        public ICharSequence ConvertBoolean(bool value) => new StringCharSequence(value.ToString());

        public ICharSequence ConvertFloat(float value) => new StringCharSequence(value.ToString(CultureInfo.InvariantCulture));

        public bool ConvertToBoolean(ICharSequence value) => AsciiString.ContentEqualsIgnoreCase(value, TrueAscii);

        public ICharSequence ConvertByte(byte value) => new StringCharSequence(value.ToString());

        public byte ConvertToByte(ICharSequence value)
        {
            if (value is AsciiString asciiString && value.Count == 1)
            {
                return asciiString.ByteAt(0);
            }
            return byte.Parse(value.ToString());
        }

        public char ConvertToChar(ICharSequence value) => value[0];

        public ICharSequence ConvertShort(short value) => new StringCharSequence(value.ToString());

        public short ConvertToShort(ICharSequence value)
        {
            if (value is AsciiString asciiString)
            {
                return asciiString.ParseShort();
            }
            return short.Parse(value.ToString());
        }

        public int ConvertToInt(ICharSequence value)
        {
            if (value is AsciiString asciiString)
            {
                return asciiString.ParseInt();
            }
            return int.Parse(value.ToString());
        }

        public long ConvertToLong(ICharSequence value)
        {
            if (value is AsciiString asciiString)
            {
                return asciiString.ParseLong();
            }
            return long.Parse(value.ToString());
        }

        public ICharSequence ConvertTimeMillis(long value) => new StringCharSequence(DateFormatter.Format(new DateTime(value * TimeSpan.TicksPerMillisecond)));

        public long ConvertToTimeMillis(ICharSequence value)
        {
            DateTime? dateTime = DateFormatter.ParseHttpDate(value);
            if (dateTime is null)
            {
                CThrowHelper.ThrowFormatException(value);
            }
            return dateTime.Value.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public float ConvertToFloat(ICharSequence value)
        {
            if (value is AsciiString asciiString)
            {
                return asciiString.ParseFloat();
            }
            return float.Parse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
        }


        public double ConvertToDouble(ICharSequence value)
        {
            if (value is AsciiString asciiString)
            {
                return asciiString.ParseDouble();
            }
            return double.Parse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
        }
    }
}
