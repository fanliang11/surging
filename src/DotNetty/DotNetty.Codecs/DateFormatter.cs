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
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
    using System.Collections;
    using System.Text;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// A formatter for HTTP header dates, such as "Expires" and "Date" headers, or "expires" field in "Set-Cookie".
    ///
    /// On the parsing side, it honors RFC6265 (so it supports RFC1123).
    /// Note that:
    /// <ul>
    ///     <li>Day of week is ignored and not validated</li>
    ///     <li>Timezone is ignored, as RFC6265 assumes UTC</li>
    /// </ul>
    /// If you're looking for a date format that validates day of week, or supports other timezones, consider using
    /// java.util.DateTimeFormatter.RFC_1123_DATE_TIME.
    ///
    /// On the formatting side, it uses a subset of RFC1123 (2 digit day-of-month and 4 digit year) as per RFC2616.
    /// This subset supports RFC6265.
    ///
    /// @see <a href="https://tools.ietf.org/html/rfc6265#section-5.1.1">RFC6265</a> for the parsing side
    /// @see <a href="https://tools.ietf.org/html/rfc1123#page-55">RFC1123</a> and
    /// <a href="https://tools.ietf.org/html/rfc2616#section-3.3.1">RFC2616</a> for the encoding side.
    /// </summary>
    public sealed class DateFormatter
    {
        static readonly BitArray Delimiters = GetDelimiters();

        static BitArray GetDelimiters()
        {
            var bitArray = new BitArray(128, false);
            bitArray[0x09] = true;
            for (int c = 0x20; c <= 0x2F; c++)
            {
                bitArray[c] = true;
            }

            for (int c = 0x3B; c <= 0x40; c++)
            {
                bitArray[c] = true;
            }

            for (int c = 0x5B; c <= 0x60; c++)
            {
                bitArray[c] = true;
            }

            for (int c = 0x7B; c <= 0x7E; c++)
            {
                bitArray[c] = true;
            }

            return bitArray;
        }

        // The order is the same as dateTime.DayOfWeek
        static readonly string[] DayOfWeekToShortName =
            { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        static readonly string[] CalendarMonthToShortName =
            { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        static readonly ThreadLocalCache Cache = new ThreadLocalCache();

        public static DateTime? ParseHttpDate(string txt) => ParseHttpDate(AsciiString.Cached(txt));

        public static DateTime? ParseHttpDate(ICharSequence txt) => ParseHttpDate(txt, 0, txt.Count);

        public static DateTime? ParseHttpDate(string txt, int start, int end) => ParseHttpDate(AsciiString.Cached(txt), start, end);

        public static DateTime? ParseHttpDate(ICharSequence txt, int start, int end)
        {
            if (txt is null) { CThrowHelper.ThrowArgumentNullException(CExceptionArgument.txt); }

            int length = end - start;
            if (0u >= (uint)length)
            {
                return null;
            }
            else if (length < 0)
            {
                CThrowHelper.ThrowArgumentException_CannotHaveEndStart();
            }
            else if (length > 64)
            {
                CThrowHelper.ThrowArgumentException_CannotParseMoreThan64Chars();
            }

            return Formatter().Parse0(txt, start, end);
        }

        public static string Format(DateTime dateTime) => Formatter().Format0(dateTime);

        public static StringBuilder Append(DateTime dateTime, StringBuilder sb) => Append0(dateTime, sb);

        static DateFormatter Formatter()
        {
            DateFormatter formatter = Cache.Value;
            formatter.Reset();
            return formatter;
        }

        // delimiter = %x09 / %x20-2F / %x3B-40 / %x5B-60 / %x7B-7E
        static bool IsDelim(char c) => Delimiters[c];

        static bool IsDigit(char c) => c >= 48 && c <= 57;

        static int GetNumericalValue(char c) => c - 48;

        readonly StringBuilder sb = new StringBuilder(29); // Sun, 27 Nov 2016 19:37:15 GMT
        bool timeFound;
        int hours;
        int minutes;
        int seconds;
        bool dayOfMonthFound;
        int dayOfMonth;
        bool monthFound;
        int month;
        bool yearFound;
        int year;

        DateFormatter()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.timeFound = false;
            this.hours = -1;
            this.minutes = -1;
            this.seconds = -1;
            this.dayOfMonthFound = false;
            this.dayOfMonth = -1;
            this.monthFound = false;
            this.month = -1;
            this.yearFound = false;
            this.year = -1;
            this.sb.Length = 0;
        }

        bool TryParseTime(ICharSequence txt, int tokenStart, int tokenEnd)
        {
            int len = tokenEnd - tokenStart;

            // h:m:s to hh:mm:ss
            if (len < 5 || len > 8)
            {
                return false;
            }

            int localHours = -1;
            int localMinutes = -1;
            int localSeconds = -1;
            int currentPartNumber = 0;
            int currentPartValue = 0;
            int numDigits = 0;

            for (int i = tokenStart; i < tokenEnd; i++)
            {
                char c = txt[i];
                if (IsDigit(c))
                {
                    currentPartValue = currentPartValue * 10 + GetNumericalValue(c);
                    if (++numDigits > 2)
                    {
                        return false; // too many digits in this part
                    }
                }
                else if (c == ':')
                {
                    if (numDigits == 0)
                    {
                        // no digits between separators
                        return false;
                    }
                    switch (currentPartNumber)
                    {
                        case 0:
                            // flushing hours
                            localHours = currentPartValue;
                            break;
                        case 1:
                            // flushing minutes
                            localMinutes = currentPartValue;
                            break;
                        default:
                            // invalid, too many :
                            return false;
                    }
                    currentPartValue = 0;
                    currentPartNumber++;
                    numDigits = 0;
                }
                else
                {
                    // invalid char
                    return false;
                }
            }

            if (numDigits > 0)
            {
                // pending seconds
                localSeconds = currentPartValue;
            }

            if (localHours >= 0 && localMinutes >= 0 && localSeconds >= 0)
            {
                this.hours = localHours;
                this.minutes = localMinutes;
                this.seconds = localSeconds;
                return true;
            }

            return false;
        }

        bool TryParseDayOfMonth(ICharSequence txt, int tokenStart, int tokenEnd)
        {
            int len = tokenEnd - tokenStart;

            if (len == 1)
            {
                char c0 = txt[tokenStart];
                if (IsDigit(c0))
                {
                    this.dayOfMonth = GetNumericalValue(c0);
                    return true;
                }

            }
            else if (len == 2)
            {
                char c0 = txt[tokenStart];
                char c1 = txt[tokenStart + 1];
                if (IsDigit(c0) && IsDigit(c1))
                {
                    this.dayOfMonth = GetNumericalValue(c0) * 10 + GetNumericalValue(c1);
                    return true;
                }
            }

            return false;
        }

        bool TryParseMonth(ICharSequence txt, int tokenStart, int tokenEnd)
        {
            int len = tokenEnd - tokenStart;

            if (len != 3) { return false; }

            char monthChar1 = AsciiString.ToLowerCase(txt[tokenStart]);
            char monthChar2 = AsciiString.ToLowerCase(txt[tokenStart + 1]);
            char monthChar3 = AsciiString.ToLowerCase(txt[tokenStart + 2]);

            if (monthChar1 == 'j' && monthChar2 == 'a' && monthChar3 == 'n')
            {
                this.month = 1;
            }
            else if (monthChar1 == 'f' && monthChar2 == 'e' && monthChar3 == 'b')
            {
                this.month = 2;
            }
            else if (monthChar1 == 'm' && monthChar2 == 'a' && monthChar3 == 'r')
            {
                this.month = 3;
            }
            else if (monthChar1 == 'a' && monthChar2 == 'p' && monthChar3 == 'r')
            {
                this.month = 4;
            }
            else if (monthChar1 == 'm' && monthChar2 == 'a' && monthChar3 == 'y')
            {
                this.month = 5;
            }
            else if (monthChar1 == 'j' && monthChar2 == 'u' && monthChar3 == 'n')
            {
                this.month = 6;
            }
            else if (monthChar1 == 'j' && monthChar2 == 'u' && monthChar3 == 'l')
            {
                this.month = 7;
            }
            else if (monthChar1 == 'a' && monthChar2 == 'u' && monthChar3 == 'g')
            {
                this.month = 8;
            }
            else if (monthChar1 == 's' && monthChar2 == 'e' && monthChar3 == 'p')
            {
                this.month = 9;
            }
            else if (monthChar1 == 'o' && monthChar2 == 'c' && monthChar3 == 't')
            {
                this.month = 10;
            }
            else if (monthChar1 == 'n' && monthChar2 == 'o' && monthChar3 == 'v')
            {
                this.month = 11;
            }
            else if (monthChar1 == 'd' && monthChar2 == 'e' && monthChar3 == 'c')
            {
                this.month = 12;
            }
            else
            {
                return false;
            }

            return true;
        }

        bool TryParseYear(ICharSequence txt, int tokenStart, int tokenEnd)
        {
            int len = tokenEnd - tokenStart;

            if (len == 2)
            {
                char c0 = txt[tokenStart];
                char c1 = txt[tokenStart + 1];
                if (IsDigit(c0) && IsDigit(c1))
                {
                    this.year = GetNumericalValue(c0) * 10 + GetNumericalValue(c1);
                    return true;
                }

            }
            else if (len == 4)
            {
                char c0 = txt[tokenStart];
                char c1 = txt[tokenStart + 1];
                char c2 = txt[tokenStart + 2];
                char c3 = txt[tokenStart + 3];
                if (IsDigit(c0) && IsDigit(c1) && IsDigit(c2) && IsDigit(c3))
                {
                    this.year = GetNumericalValue(c0) * 1000
                        + GetNumericalValue(c1) * 100
                        + GetNumericalValue(c2) * 10
                        + GetNumericalValue(c3);

                    return true;
                }
            }

            return false;
        }

        bool ParseToken(ICharSequence txt, int tokenStart, int tokenEnd)
        {
            // return true if all parts are found
            if (!this.timeFound)
            {
                this.timeFound = this.TryParseTime(txt, tokenStart, tokenEnd);
                if (this.timeFound)
                {
                    return this.dayOfMonthFound && this.monthFound && this.yearFound;
                }
            }

            if (!this.dayOfMonthFound)
            {
                this.dayOfMonthFound = this.TryParseDayOfMonth(txt, tokenStart, tokenEnd);
                if (this.dayOfMonthFound)
                {
                    return this.timeFound && this.monthFound && this.yearFound;
                }
            }

            if (!this.monthFound)
            {
                this.monthFound = this.TryParseMonth(txt, tokenStart, tokenEnd);
                if (this.monthFound)
                {
                    return this.timeFound && this.dayOfMonthFound && this.yearFound;
                }
            }

            if (!this.yearFound)
            {
                this.yearFound = this.TryParseYear(txt, tokenStart, tokenEnd);
            }

            return this.timeFound && this.dayOfMonthFound && this.monthFound && this.yearFound;
        }

        DateTime? Parse0(ICharSequence txt, int start, int end)
        {
            bool allPartsFound = this.Parse1(txt, start, end);
            return allPartsFound && this.NormalizeAndValidate() ? this.ComputeDate() : default(DateTime?);
        }

        bool Parse1(ICharSequence txt, int start, int end)
        {
            // return true if all parts are found
            int tokenStart = -1;

            for (int i = start; i < end; i++)
            {
                char c = txt[i];

                if (IsDelim(c))
                {
                    if (tokenStart != -1)
                    {
                        // terminate token
                        if (this.ParseToken(txt, tokenStart, i))
                        {
                            return true;
                        }
                        tokenStart = -1;
                    }
                }
                else if (tokenStart == -1)
                {
                    // start new token
                    tokenStart = i;
                }
            }

            // terminate trailing token
            return tokenStart != -1 && this.ParseToken(txt, tokenStart, txt.Count);
        }

        bool NormalizeAndValidate()
        {
            if (this.dayOfMonth < 1
                || this.dayOfMonth > 31
                || this.hours > 23
                || this.minutes > 59
                || this.seconds > 59)
            {
                return false;
            }

            if (this.year >= 70 && this.year <= 99)
            {
                this.year += 1900;
            }
            else if (this.year >= 0 && this.year < 70)
            {
                this.year += 2000;
            }
            else if (this.year < 1601)
            {
                // invalid value
                return false;
            }
            return true;
        }

        DateTime ComputeDate() => new DateTime(this.year, this.month, this.dayOfMonth, this.hours, this.minutes, this.seconds, DateTimeKind.Utc);

        string Format0(DateTime dateTime) => Append0(dateTime, this.sb).ToString();

        static StringBuilder Append0(DateTime dateTime, StringBuilder buffer)
        {
            _ = buffer.Append(DayOfWeekToShortName[(int)dateTime.DayOfWeek]).Append(", ");
            _ = AppendZeroLeftPadded(dateTime.Day, buffer).Append(' ');
            _ = buffer
                .Append(CalendarMonthToShortName[dateTime.Month - 1]).Append(' ')
                .Append(dateTime.Year).Append(' ');

            _ = AppendZeroLeftPadded(dateTime.Hour, buffer).Append(':');
            _ = AppendZeroLeftPadded(dateTime.Minute, buffer).Append(':');
            return AppendZeroLeftPadded(dateTime.Second, buffer).Append(" GMT");
        }

        static StringBuilder AppendZeroLeftPadded(int value, StringBuilder sb)
        {
            if (value < 10)
            {
                _ = sb.Append('0');
            }
            return sb.Append(value);
        }

        sealed class ThreadLocalCache : FastThreadLocal<DateFormatter>
        {
            protected override DateFormatter GetInitialValue() => new DateFormatter();
        }
    }
}
