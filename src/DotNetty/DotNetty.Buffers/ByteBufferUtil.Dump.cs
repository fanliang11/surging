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

namespace DotNetty.Buffers
{
    using System;
    using System.Text;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    partial class ByteBufferUtil
    {
        /// <summary>
        ///     Returns a <a href="http://en.wikipedia.org/wiki/Hex_dump">hex dump</a>
        ///     of the specified buffer's sub-region.
        /// </summary>
        public static string HexDump(IByteBuffer buffer) => HexDump(buffer, buffer.ReaderIndex, buffer.ReadableBytes);

        /// <summary>
        ///     Returns a <a href="http://en.wikipedia.org/wiki/Hex_dump">hex dump</a>
        ///     of the specified buffer's sub-region.
        /// </summary>
        public static string HexDump(IByteBuffer buffer, int fromIndex, int length) => HexUtil.DoHexDump(buffer, fromIndex, length);

        /// <summary>
        ///     Returns a <a href="http://en.wikipedia.org/wiki/Hex_dump">hex dump</a>
        ///     of the specified buffer's sub-region.
        /// </summary>
        public static string HexDump(byte[] array) => HexDump(array, 0, array.Length);

        /// <summary>
        ///     Returns a <a href="http://en.wikipedia.org/wiki/Hex_dump">hex dump</a>
        ///     of the specified buffer's sub-region.
        /// </summary>
        public static string HexDump(byte[] array, int fromIndex, int length) => HexUtil.DoHexDump(array, fromIndex, length);

        /// <summary>
        ///     Returns a multi-line hexadecimal dump of the specified {@link ByteBuf} that is easy to read by humans.
        /// </summary>
        public static string PrettyHexDump(IByteBuffer buffer) => PrettyHexDump(buffer, buffer.ReaderIndex, buffer.ReadableBytes);

        /// <summary>
        ///     Returns a multi-line hexadecimal dump of the specified {@link ByteBuf} that is easy to read by humans,
        ///     starting at the given {@code offset} using the given {@code length}.
        /// </summary>
        public static string PrettyHexDump(IByteBuffer buffer, int offset, int length) => HexUtil.DoPrettyHexDump(buffer, offset, length);

        /// <summary>
        ///     Appends the prettified multi-line hexadecimal dump of the specified {@link ByteBuf} to the specified
        ///     {@link StringBuilder} that is easy to read by humans.
        /// </summary>
        public static void AppendPrettyHexDump(StringBuilder dump, IByteBuffer buf) => AppendPrettyHexDump(dump, buf, buf.ReaderIndex, buf.ReadableBytes);

        /// <summary>
        ///     Appends the prettified multi-line hexadecimal dump of the specified {@link ByteBuf} to the specified
        ///     {@link StringBuilder} that is easy to read by humans, starting at the given {@code offset} using
        ///     the given {@code length}.
        /// </summary>
        public static void AppendPrettyHexDump(StringBuilder dump, IByteBuffer buf, int offset, int length) => HexUtil.DoAppendPrettyHexDump(dump, buf, offset, length);

        static class HexUtil
        {
            static readonly char[] HexdumpTable;
            static readonly string Newline;
            static readonly string[] Byte2Hex;
            static readonly string[] HexPadding;
            static readonly string[] BytePadding;
            static readonly char[] Byte2Char;
            static readonly string[] HexDumpRowPrefixes;

            static HexUtil()
            {
                HexdumpTable = new char[256 * 4];
                Newline = StringUtil.Newline;
                Byte2Hex = new string[256];
                HexPadding = new string[16];
                BytePadding = new string[16];
                Byte2Char = new char[256];
                HexDumpRowPrefixes = new string[65536.RightUShift(4)];

                char[] digits = "0123456789abcdef".ToCharArray();
                for (int i = 0; i < 256; i++)
                {
                    HexdumpTable[i << 1] = digits[i.RightUShift(4) & 0x0F];
                    HexdumpTable[(i << 1) + 1] = digits[i & 0x0F];
                }

                // Generate the lookup table for byte-to-hex-dump conversion
                for (int i = 0; i < Byte2Hex.Length; i++)
                {
                    Byte2Hex[i] = ' ' + StringUtil.ByteToHexStringPadded(i);
                }

                // Generate the lookup table for hex dump paddings
                for (int i = 0; i < HexPadding.Length; i++)
                {
                    int padding = HexPadding.Length - i;
                    var buf = StringBuilderManager.Allocate(padding * 3);
                    for (int j = 0; j < padding; j++)
                    {
                        _ = buf.Append("   ");
                    }
                    HexPadding[i] = StringBuilderManager.ReturnAndFree(buf);
                }

                // Generate the lookup table for byte dump paddings
                for (int i = 0; i < BytePadding.Length; i++)
                {
                    int padding = BytePadding.Length - i;
                    var buf = StringBuilderManager.Allocate(padding);
                    for (int j = 0; j < padding; j++)
                    {
                        _ = buf.Append(' ');
                    }
                    BytePadding[i] = StringBuilderManager.ReturnAndFree(buf);
                }

                // Generate the lookup table for byte-to-char conversion
                for (int i = 0; i < Byte2Char.Length; i++)
                {
                    if (i <= 0x1f || i >= 0x7f)
                    {
                        Byte2Char[i] = '.';
                    }
                    else
                    {
                        Byte2Char[i] = (char)i;
                    }
                }

                // Generate the lookup table for the start-offset header in each row (up to 64KiB).
                for (int i = 0; i < HexDumpRowPrefixes.Length; i++)
                {
                    var buf = StringBuilderManager.Allocate(); // new StringBuilder(12);
                    _ = buf.Append(Environment.NewLine);
                    _ = buf.Append((i << 4 & 0xFFFFFFFFL | 0x100000000L).ToString("X2"));
                    _ = buf.Insert(buf.Length - 9, '|');
                    _ = buf.Append('|');
                    HexDumpRowPrefixes[i] = StringBuilderManager.ReturnAndFree(buf);
                }
            }

            public static string DoHexDump(IByteBuffer buffer, int fromIndex, int length)
            {
                uint uLength = (uint)length;
                if (uLength > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(length, ExceptionArgument.length); }
                if (0u >= uLength) { return string.Empty; }

                int endIndex = fromIndex + length;
                var buf = new char[length << 1];

                int srcIdx = fromIndex;
                int dstIdx = 0;
                for (; srcIdx < endIndex; srcIdx++, dstIdx += 2)
                {
                    Array.Copy(
                        HexdumpTable, buffer.GetByte(srcIdx) << 1,
                        buf, dstIdx, 2);
                }

                return new string(buf);
            }

            public static string DoHexDump(byte[] array, int fromIndex, int length)
            {
                uint uLength = (uint)length;
                if (uLength > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(length, ExceptionArgument.length); }

                if (0u >= uLength) { return string.Empty; }

                int endIndex = fromIndex + length;
                var buf = new char[length << 1];

                int srcIdx = fromIndex;
                int dstIdx = 0;
                for (; srcIdx < endIndex; srcIdx++, dstIdx += 2)
                {
                    Array.Copy(HexdumpTable, (array[srcIdx] & 0xFF) << 1, buf, dstIdx, 2);
                }

                return new string(buf);
            }

            public static string DoPrettyHexDump(IByteBuffer buffer, int offset, int length)
            {
                if (0u >= (uint)length)
                {
                    return string.Empty;
                }
                else
                {
                    int rows = length / 16 + (0u >= (uint)(length % 15) ? 0 : 1) + 4;
                    var buf = StringBuilderManager.Allocate(rows * 80);
                    AppendPrettyHexDump(buf, buffer, offset, length);
                    return StringBuilderManager.ReturnAndFree(buf);
                }
            }

            public static void DoAppendPrettyHexDump(StringBuilder dump, IByteBuffer buf, int offset, int length)
            {
                if (MathUtil.IsOutOfBounds(offset, length, buf.Capacity))
                {
                    ThrowHelper.ThrowIndexOutOfRangeException_Expected(offset, length, buf.Capacity);
                }
                if (0u >= (uint)length)
                {
                    return;
                }
                _ = dump.Append(
                    "         +-------------------------------------------------+" +
                    Newline + "         |  0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f |" +
                    Newline + "+--------+-------------------------------------------------+----------------+");

                int startIndex = offset;
                int fullRows = length.RightUShift(4);
                int remainder = length & 0xF;

                // Dump the rows which have 16 bytes.
                for (int row = 0; row < fullRows; row++)
                {
                    int rowStartIndex = (row << 4) + startIndex;

                    // Per-row prefix.
                    AppendHexDumpRowPrefix(dump, row, rowStartIndex);

                    // Hex dump
                    int rowEndIndex = rowStartIndex + 16;
                    for (int j = rowStartIndex; j < rowEndIndex; j++)
                    {
                        _ = dump.Append(Byte2Hex[buf.GetByte(j)]);
                    }
                    _ = dump.Append(" |");

                    // ASCII dump
                    for (int j = rowStartIndex; j < rowEndIndex; j++)
                    {
                        _ = dump.Append(Byte2Char[buf.GetByte(j)]);
                    }
                    _ = dump.Append('|');
                }

                // Dump the last row which has less than 16 bytes.
                if (remainder != 0)
                {
                    int rowStartIndex = (fullRows << 4) + startIndex;
                    AppendHexDumpRowPrefix(dump, fullRows, rowStartIndex);

                    // Hex dump
                    int rowEndIndex = rowStartIndex + remainder;
                    for (int j = rowStartIndex; j < rowEndIndex; j++)
                    {
                        _ = dump.Append(Byte2Hex[buf.GetByte(j)]);
                    }
                    _ = dump.Append(HexPadding[remainder]);
                    _ = dump.Append(" |");

                    // Ascii dump
                    for (int j = rowStartIndex; j < rowEndIndex; j++)
                    {
                        _ = dump.Append(Byte2Char[buf.GetByte(j)]);
                    }
                    _ = dump.Append(BytePadding[remainder]);
                    _ = dump.Append('|');
                }

                _ = dump.Append(Newline + "+--------+-------------------------------------------------+----------------+");
            }

            static void AppendHexDumpRowPrefix(StringBuilder dump, int row, int rowStartIndex)
            {
                if (row < HexDumpRowPrefixes.Length)
                {
                    _ = dump.Append(HexDumpRowPrefixes[row]);
                }
                else
                {
                    _ = dump.Append(Environment.NewLine);
                    _ = dump.Append((rowStartIndex & 0xFFFFFFFFL | 0x100000000L).ToString("X2"));
                    _ = dump.Insert(dump.Length - 9, '|');
                    _ = dump.Append('|');
                }
            }
        }
    }
}
