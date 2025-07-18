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
    using System.Text;
    using DotNetty.Common.Internal;
#if !NET
    using System.Runtime.InteropServices;
#endif

    /// <summary>
    ///     String utility class.
    /// </summary>
    public static class StringUtil
    {
        public static readonly string EmptyString;
        public static readonly string Newline;

        public const char DoubleQuote = '\"';
        public const char Comma = ',';
        public const char LineFeed = '\n';
        public const char CarriageReturn = '\r';
        public const char Tab = '\t';
        public const char Space = '\x20';
        public const byte UpperCaseToLowerCaseAsciiOffset = 'a' - 'A';
        private const uint uTab = Tab;
        private const uint uSpace = Space;
        private static readonly string[] Byte2HexPad;
        private static readonly string[] Byte2HexNopad;
        private static ReadOnlySpan<sbyte> Hex2B => new sbyte[128]
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, -1, -1, -1, -1, -1, -1,
            -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
        };

        /**
         * 2 - Quote character at beginning and end.
         * 5 - Extra allowance for anticipated escape characters that may be added.
        */
        const int CsvNumberEscapeCharacters = 2 + 5;

        static StringUtil()
        {
            EmptyString = string.Empty;
            Newline = SystemPropertyUtil.Get("line.separator", Environment.NewLine);

            Byte2HexPad = new string[256];
            Byte2HexNopad = new string[256];
            // Generate the lookup table that converts a byte into a 2-digit hexadecimal integer.
            int i;
            for (i = 0; i < 10; i++)
            {
                var buf = new StringBuilder(2);
                _ = buf.Append('0');
                _ = buf.Append(i);
                Byte2HexPad[i] = buf.ToString();
                Byte2HexNopad[i] = (i).ToString();
            }
            for (; i < 16; i++)
            {
                var buf = new StringBuilder(2);
                char c = (char)('A' + i - 10);
                _ = buf.Append('0');
                _ = buf.Append(c);
                Byte2HexPad[i] = buf.ToString();
                Byte2HexNopad[i] = c.ToString(); /* String.valueOf(c);*/
            }
            for (; i < Byte2HexPad.Length; i++)
            {
                var buf = new StringBuilder(2);
                _ = buf.Append(i.ToString("X") /*Integer.toHexString(i)*/);
                string str = buf.ToString();
                Byte2HexPad[i] = str;
                Byte2HexNopad[i] = str;
            }
        }

        public static string SubstringAfter(string value, char delim)
        {
            int pos = value.IndexOf(delim);
            return SharedConstants.TooBigOrNegative >= (uint)pos ? value.Substring(pos + 1) : null;
        }

        public static bool CommonSuffixOfLength(string s, string p, int len)
            => s is object && p is object && SharedConstants.TooBigOrNegative >= (uint)len && RegionMatches(s, s.Length - len, p, p.Length - len, len);

        static bool RegionMatches(string value, int thisStart, string other, int start, int length)
        {
            if ((uint)start > (uint)other.Length || (uint)length > (uint)(other.Length - start))
            {
                return false;
            }

            if ((uint)thisStart > (uint)value.Length || (uint)length > (uint)(value.Length - thisStart))
            {
                return false;
            }

            if (0u >= (uint)length) { return true; }

#if NET
            return value.AsSpan().Slice(thisStart, length).SequenceEqual(other.AsSpan().Slice(start, length));
#else
            ref char valueStart = ref MemoryMarshal.GetReference(value.AsSpan());
            ref char otherStart = ref MemoryMarshal.GetReference(other.AsSpan());
            return SpanHelpers.SequenceEqual(ref Unsafe.Add(ref valueStart, thisStart), ref Unsafe.Add(ref otherStart, start), length);
#endif
        }

        /// <summary>
        ///     Converts the specified byte value into a 2-digit hexadecimal integer.
        /// </summary>
        public static string ByteToHexStringPadded(int value) => Byte2HexPad[value & 0xff];

        // 
        // Converts the specified byte value into a 2-digit hexadecimal integer and appends it to the specified buffer.
        // 
        public static T ByteToHexStringPadded<T>(T buf, int value) where T : IAppendable
        {
            _ = buf.Append(new StringCharSequence(ByteToHexStringPadded(value)));
            return buf;
        }

        /// <summary>
        ///     Converts the specified byte array into a hexadecimal value.
        /// </summary>
        public static string ToHexStringPadded(byte[] src) => ToHexStringPadded(src, 0, src.Length);

        /// <summary>
        ///     Converts the specified byte array into a hexadecimal value.
        /// </summary>
        public static string ToHexStringPadded(byte[] src, int offset, int length)
        {
            int end = offset + length;
            var sb = new StringBuilder(length << 1);
            for (int i = offset; i < end; i++)
            {
                _ = sb.Append(ByteToHexStringPadded(src[i]));
            }
            return sb.ToString();
        }

        public static T ToHexStringPadded<T>(T dst, byte[] src) where T : IAppendable => ToHexStringPadded(dst, src, 0, src.Length);

        public static T ToHexStringPadded<T>(T dst, byte[] src, int offset, int length) where T : IAppendable
        {
            int end = offset + length;
            for (int i = offset; i < end; i++)
            {
                _ = ByteToHexStringPadded(dst, src[i]);
            }
            return dst;
        }

        /// <summary>
        ///     Converts the specified byte value into a hexadecimal integer.
        /// </summary>
        public static string ByteToHexString(byte value) => Byte2HexNopad[value & 0xff];

        public static T ByteToHexString<T>(T buf, byte value) where T : IAppendable
        {
            _ = buf.Append(new StringCharSequence(ByteToHexString(value)));
            return buf;
        }

        public static string ToHexString(byte[] src) => ToHexString(src, 0, src.Length);

        public static string ToHexString(byte[] src, int offset, int length) => ToHexString(new AppendableCharSequence(length << 1), src, offset, length).ToString();

        public static T ToHexString<T>(T dst, byte[] src) where T : IAppendable => ToHexString(dst, src, 0, src.Length);

        public static T ToHexString<T>(T dst, byte[] src, int offset, int length) where T : IAppendable
        {
            uint uLen = (uint)length;
            if (uLen > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(length, ExceptionArgument.length); }

            if (0u >= uLen)
            {
                return dst;
            }

            int end = offset + length;
            int endMinusOne = end - 1;
            int i;

            // Skip preceding zeroes.
            for (i = offset; i < endMinusOne; i++)
            {
                if (src[i] != 0)
                {
                    break;
                }
            }

            _ = ByteToHexString(dst, src[i++]);
            int remaining = end - i;
            _ = ToHexStringPadded(dst, src, i, remaining);

            return dst;
        }

        public static int DecodeHexNibble(char c)
        {
            const uint uHex2BLen = 128u;
            // Character.digit() is not used here, as it addresses a larger
            // set of characters (both ASCII and full-width latin letters).
            int index = c;
            if (uHex2BLen > (uint)index) { return Hex2B[index]; }
            return -1;
        }

        // Decode a 2-digit hex byte from within a string.
        public static byte DecodeHexByte(string s, int pos)
        {
            const uint MaxHex2B = 15U;
            int hi = DecodeHexNibble(s[pos]);
            int lo = DecodeHexNibble(s[pos + 1]);
            if (MaxHex2B >= (uint)hi && MaxHex2B >= (uint)lo)
            {
                return (byte)((hi << 4) + lo);
            }
            throw ThrowHelper.GetArgumentException_DecodeHexByte(s, pos);
        }

        //Decodes part of a string with <a href="http://en.wikipedia.org/wiki/Hex_dump">hex dump</a>
        public static byte[] DecodeHexDump(string hexDump, int fromIndex, int length)
        {
            if (length < 0 || (length & 1) != 0)
            {
                ThrowHelper.ThrowArgumentException_InvalidLen(length);
            }
            if (0u >= (uint)length)
            {
                return EmptyArrays.EmptyBytes;
            }
            var bytes = new byte[length.RightUShift(1)];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i.RightUShift(1)] = DecodeHexByte(hexDump, fromIndex + i);
            }
            return bytes;
        }

        // Decodes a <a href="http://en.wikipedia.org/wiki/Hex_dump">hex dump</a>
        public static byte[] DecodeHexDump(string hexDump) => DecodeHexDump(hexDump, 0, hexDump.Length);

        /// <summary>
        ///     The shortcut to <see cref="SimpleClassName(Type)">SimpleClassName(o.GetType())</see>.
        /// </summary>
        public static string SimpleClassName(object o) => o?.GetType().Name ?? "null_object";

        /// <summary>
        ///     The shortcut to <see cref="SimpleClassName(Type)">SimpleClassName(o.GetType())</see>.
        /// </summary>
        public static string SimpleClassName<T>() => typeof(T).Name;

        /// <summary>
        ///     Generates a simplified name from a <see cref="Type" />.  Similar to {@link Class#getSimpleName()}, but it works
        ///     fine
        ///     with anonymous classes.
        /// </summary>
        public static string SimpleClassName(Type type) => type.Name;

        /// <summary>
        ///     Escapes the specified value, if necessary according to
        ///     <a href="https://tools.ietf.org/html/rfc4180#section-2">RFC-4180</a>.
        /// </summary>
        /// <param name="value">
        ///     The value which will be escaped according to
        ///     <a href="https://tools.ietf.org/html/rfc4180#section-2">RFC-4180</a>
        /// </param>
        /// <param name="trimWhiteSpace">
        ///     The value will first be trimmed of its optional white-space characters, according to 
        ///     <a href= "https://tools.ietf.org/html/rfc7230#section-7" >RFC-7230</a>
        /// </param>
        /// <returns>the escaped value if necessary, or the value unchanged</returns>
        public static ICharSequence EscapeCsv(ICharSequence value, bool trimWhiteSpace = false)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            int length = value.Count;
            if (0u >= (uint)length)
            {
                return value;
            }

            int start;
            int last;
            if (trimWhiteSpace)
            {
                start = IndexOfFirstNonOwsChar(value, (uint)length);
                last = IndexOfLastNonOwsChar(value, start, length);
            }
            else
            {
                start = 0;
                last = length - 1;
            }
            if (start > last)
            {
                return StringCharSequence.Empty;
            }

            int firstUnescapedSpecial = -1;
            bool quoted = false;
            if (IsDoubleQuote(value[start]))
            {
                quoted = IsDoubleQuote(value[last]) && last > start;
                if (quoted)
                {
                    start++;
                    last--;
                }
                else
                {
                    firstUnescapedSpecial = start;
                }
            }

            if (firstUnescapedSpecial < 0)
            {
                if (quoted)
                {
                    for (int i = start; i <= last; i++)
                    {
                        if (IsDoubleQuote(value[i]))
                        {
                            if (i == last || !IsDoubleQuote(value[i + 1]))
                            {
                                firstUnescapedSpecial = i;
                                break;
                            }
                            i++;
                        }
                    }
                }
                else
                {
                    for (int i = start; i <= last; i++)
                    {
                        char c = value[i];
                        if (c == LineFeed || c == CarriageReturn || c == Comma)
                        {
                            firstUnescapedSpecial = i;
                            break;
                        }
                        if (IsDoubleQuote(c))
                        {
                            if (i == last || !IsDoubleQuote(value[i + 1]))
                            {
                                firstUnescapedSpecial = i;
                                break;
                            }
                            i++;
                        }
                    }
                }
                if (firstUnescapedSpecial < 0)
                {
                    // Special characters is not found or all of them already escaped.
                    // In the most cases returns a same string. New string will be instantiated (via StringBuilder)
                    // only if it really needed. It's important to prevent GC extra load.
                    return quoted ? value.SubSequence(start - 1, last + 2) : value.SubSequence(start, last + 1);
                }
            }

            var result = new StringBuilderCharSequence(last - start + 1 + CsvNumberEscapeCharacters);
            result.Append(DoubleQuote);
            result.Append(value, start, firstUnescapedSpecial - start);
            for (int i = firstUnescapedSpecial; i <= last; i++)
            {
                char c = value[i];
                if (IsDoubleQuote(c))
                {
                    result.Append(DoubleQuote);
                    if (i < last && IsDoubleQuote(value[i + 1]))
                    {
                        i++;
                    }
                }
                result.Append(c);
            }

            result.Append(DoubleQuote);
            return result;
        }

        public static ICharSequence UnescapeCsv(ICharSequence value)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
            int length = value.Count;
            if (0u >= (uint)length)
            {
                return value;
            }
            int last = length - 1;
            bool quoted = IsDoubleQuote(value[0]) && IsDoubleQuote(value[last]) && length != 1;
            if (!quoted)
            {
                ValidateCsvFormat(value);
                return value;
            }
            StringBuilder unescaped = InternalThreadLocalMap.Get().StringBuilder;
            for (int i = 1; i < last; i++)
            {
                char current = value[i];
                if (current == DoubleQuote)
                {
                    if (IsDoubleQuote(value[i + 1]) && (i + 1) != last)
                    {
                        // Followed by a double-quote but not the last character
                        // Just skip the next double-quote
                        i++;
                    }
                    else
                    {
                        // Not followed by a double-quote or the following double-quote is the last character
                        _ = ThrowInvalidEscapedCsvFieldException(value, i);
                    }
                }
                _ = unescaped.Append(current);
            }

            return new StringCharSequence(unescaped.ToString());
        }

        public static IList<ICharSequence> UnescapeCsvFields(ICharSequence value)
        {
            var unescaped = new List<ICharSequence>(2);
            StringBuilder current = InternalThreadLocalMap.Get().StringBuilder;
            bool quoted = false;
            int last = value.Count - 1;
            for (int i = 0; i <= last; i++)
            {
                char c = value[i];
                if (quoted)
                {
                    switch (c)
                    {
                        case DoubleQuote:
                            if (i == last)
                            {
                                // Add the last field and return
                                unescaped.Add((StringCharSequence)current.ToString());
                                return unescaped;
                            }
                            char next = value[++i];
                            if (next == DoubleQuote)
                            {
                                // 2 double-quotes should be unescaped to one
                                _ = current.Append(DoubleQuote);
                            }
                            else if (next == Comma)
                            {
                                // This is the end of a field. Let's start to parse the next field.
                                quoted = false;
                                unescaped.Add((StringCharSequence)current.ToString());
                                current.Length = 0;
                            }
                            else
                            {
                                // double-quote followed by other character is invalid
                                _ = ThrowInvalidEscapedCsvFieldException(value, i - 1);
                            }
                            break;
                        default:
                            _ = current.Append(c);
                            break;
                    }
                }
                else
                {
                    switch (c)
                    {
                        case Comma:
                            // Start to parse the next field
                            unescaped.Add((StringCharSequence)current.ToString());
                            current.Length = 0;
                            break;
                        case DoubleQuote:
                            if (0u >= (uint)current.Length)
                            {
                                quoted = true;
                            }
                            else
                            {
                                // double-quote appears without being enclosed with double-quotes
                                _ = current.Append(c);
                            }
                            break;
                        case LineFeed:
                        case CarriageReturn:
                            // special characters appears without being enclosed with double-quotes
                            _ = ThrowInvalidEscapedCsvFieldException(value, i); break;
                        default:
                            _ = current.Append(c);
                            break;
                    }
                }
            }
            if (quoted)
            {
                _ = ThrowInvalidEscapedCsvFieldException(value, last);
            }

            unescaped.Add((StringCharSequence)current.ToString());
            return unescaped;
        }

        static void ValidateCsvFormat(ICharSequence value)
        {
            int length = value.Count;
            for (int i = 0; i < length; i++)
            {
                switch (value[i])
                {
                    case DoubleQuote:
                    case LineFeed:
                    case CarriageReturn:
                    case Comma:
                        // If value contains any special character, it should be enclosed with double-quotes
                        _ = ThrowInvalidEscapedCsvFieldException(value, i); break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static ArgumentException NewInvalidEscapedCsvFieldException(ICharSequence value, int index) => new ArgumentException($"invalid escaped CSV field: {value} index: {index}");

        public static int Length(string s) => s?.Length ?? 0;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsEmpty(string s) => 0u >= (uint)s.Length;

        public static int IndexOfNonWhiteSpace(IReadOnlyList<char> seq, int offset)
        {
            if (seq is IHasUtf16Span hasUtf16Span)
            {
                var utf16Span = hasUtf16Span.Utf16Span;
                for (; offset < utf16Span.Length; ++offset)
                {
                    if (!char.IsWhiteSpace(utf16Span[offset]))
                    {
                        return offset;
                    }
                }

                return AsciiString.IndexNotFound;
            }
            return IndexOfNonWhiteSpaceSlow(seq, offset);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int IndexOfNonWhiteSpaceSlow(IReadOnlyList<char> seq, int offset)
        {
            for (; offset < seq.Count; ++offset)
            {
                if (!char.IsWhiteSpace(seq[offset]))
                {
                    return offset;
                }
            }

            return AsciiString.IndexNotFound;
        }

        public static int IndexOfWhiteSpace(IReadOnlyList<char> seq, int offset)
        {
            if (seq is IHasUtf16Span hasUtf16Span)
            {
                var utf16Span = hasUtf16Span.Utf16Span;
                for (; offset < utf16Span.Length; ++offset)
                {
                    if (char.IsWhiteSpace(utf16Span[offset]))
                    {
                        return offset;
                    }
                }

                return AsciiString.IndexNotFound;
            }
            return IndexOfWhiteSpaceSlow(seq, offset);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int IndexOfWhiteSpaceSlow(IReadOnlyList<char> seq, int offset)
        {
            for (; offset < seq.Count; ++offset)
            {
                if (char.IsWhiteSpace(seq[offset]))
                {
                    return offset;
                }
            }

            return AsciiString.IndexNotFound;
        }

        public static bool IsSurrogate(char c) => UnicodeUtility.IsInRangeInclusive(c, '\uD800', '\uDFFF');

        static bool IsDoubleQuote(char c) => c == DoubleQuote;

        public static bool EndsWith(IReadOnlyList<char> s, char c)
        {
            int len = s.Count;
            return (uint)len > 0u && s[len - 1] == c;
        }

        public static ICharSequence TrimOws(ICharSequence value)
        {
            int length = value.Count;
            uint ulength = (uint)length;
            if (0u >= ulength) { return value; }

            int start, end;
            if (value is IHasUtf16Span hasUtf16Span)
            {
                var utf16Span = hasUtf16Span.Utf16Span;
                start = IndexOfFirstNonOwsChar(utf16Span, ulength);
                end = IndexOfLastNonOwsChar(utf16Span, start, length);
            }
            else
            {
                start = IndexOfFirstNonOwsChar(value, ulength);
                end = IndexOfLastNonOwsChar(value, start, length);
            }
            return 0u >= (uint)start && end == length - 1 ? value : value.SubSequence(start, end + 1);
        }

        public static ICharSequence Join(string separator, IList<ICharSequence> elements)
        {
            if (separator is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.separator); }
            if (elements is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.elements); }

            var count = elements.Count;
            uint uCount = (uint)count;
            if (0u >= uCount) { return StringCharSequence.Empty; }

            var firstElement = elements[0];
            if (1u >= uCount) { return firstElement; }

            var builder = StringBuilderManager.Allocate().Append(firstElement.ToString());
            for (var idx = 1; idx < count; idx++)
            {
                _ = builder.Append(separator).Append(elements[idx].ToString());
            }
            return new StringCharSequence(StringBuilderManager.ReturnAndFree(builder));
        }

        static int IndexOfFirstNonOwsChar(in ReadOnlySpan<char> value, uint length)
        {
            int i = 0;
            while ((uint)i < length && IsOws(value[i]))
            {
                i++;
            }

            return i;
        }

        static int IndexOfLastNonOwsChar(in ReadOnlySpan<char> value, int start, int length)
        {
            int i = length - 1;
            while (i > start && IsOws(value[i]))
            {
                i--;
            }

            return i;
        }

        static int IndexOfFirstNonOwsChar(IReadOnlyList<char> value, uint length)
        {
            int i = 0;
            while ((uint)i < length && IsOws(value[i]))
            {
                i++;
            }

            return i;
        }

        static int IndexOfLastNonOwsChar(IReadOnlyList<char> value, int start, int length)
        {
            int i = length - 1;
            while (i > start && IsOws(value[i]))
            {
                i--;
            }

            return i;
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        static bool IsOws(uint c) => c == uSpace || c == uTab;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int ThrowInvalidEscapedCsvFieldException(ICharSequence value, int index)
        {
            throw NewInvalidEscapedCsvFieldException(value, index);
        }
    }
}