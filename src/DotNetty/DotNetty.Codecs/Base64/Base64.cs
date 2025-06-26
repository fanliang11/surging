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

namespace DotNetty.Codecs.Base64
{
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using DotNetty.Buffers;
    using DotNetty.Buffers.Internal;

    public static class Base64
    {
        const int MAX_LINE_LENGTH = 76;
        const byte EQUALS_SIGN = (byte)'='; //pad
        const byte NEW_LINE = (byte)'\n';
        const sbyte WHITE_SPACE_ENC = -5; // Indicates white space in encoding
        const sbyte EQUALS_SIGN_ENC = -1; // Indicates equals sign in encoding

        public static IByteBuffer Encode(IByteBuffer src) => Encode(src, Base64Dialect.Standard);
        
        public static IByteBuffer Encode(IByteBuffer src, bool breakLines) => Encode(src, breakLines, Base64Dialect.Standard);

        public static IByteBuffer Encode(IByteBuffer src, IBase64Dialect dialect) => Encode(src, src.ReaderIndex, src.ReadableBytes, dialect.BreakLinesByDefault, dialect);

        public static IByteBuffer Encode(IByteBuffer src, bool breakLines, IBase64Dialect dialect) => Encode(src, src.ReaderIndex, src.ReadableBytes, breakLines, dialect);

        public static IByteBuffer Encode(IByteBuffer src, int offset, int length, bool breakLines, IBase64Dialect dialect) => Encode(src, offset, length, breakLines, dialect, src.Allocator);

        static unsafe int EncodeUsingPointer(byte* alphabet, IByteBuffer src, IByteBuffer dest, int offset, int length, bool breakLines)
        {
            //avoid unnecessary range checking
            fixed (byte* srcArray = src.Array, d = dest.Array)
            {
                byte* destArray = d + dest.ArrayOffset + dest.WriterIndex;
                int j = 0;
                int charCount = 0;
                //the real offset of the array, is ArrayOfffset + offset
                int i = src.ArrayOffset + offset;
                int remainderLength = length % 3;
                int calcLength = src.ArrayOffset + offset + length - remainderLength;
                for (; i < calcLength; i += 3)
                {
                    if (breakLines)
                    {
                        if (0u >= (uint)(charCount - MAX_LINE_LENGTH))
                        {
                            destArray[j++] = NEW_LINE;
                            charCount = 0;
                        }
                        charCount += 4;
                    }

                    destArray[j + 0] = alphabet[(srcArray[i] & 0xfc) >> 2];
                    destArray[j + 1] = alphabet[((srcArray[i] & 0x03) << 4) | ((srcArray[i + 1] & 0xf0) >> 4)];
                    destArray[j + 2] = alphabet[((srcArray[i + 1] & 0x0f) << 2) | ((srcArray[i + 2] & 0xc0) >> 6)];
                    destArray[j + 3] = alphabet[(srcArray[i + 2] & 0x3f)];
                    j += 4;
                }

                i = calcLength;

                if (breakLines && ((uint)remainderLength > 0u) && (0u >= (uint)(charCount - MAX_LINE_LENGTH)))
                {
                    destArray[j++] = NEW_LINE;
                }
                switch (remainderLength)
                {
                    case 2:
                        destArray[j + 0] = alphabet[(srcArray[i] & 0xfc) >> 2];
                        destArray[j + 1] = alphabet[((srcArray[i] & 0x03) << 4) | ((srcArray[i + 1] & 0xf0) >> 4)];
                        destArray[j + 2] = alphabet[(srcArray[i + 1] & 0x0f) << 2];
                        destArray[j + 3] = EQUALS_SIGN;
                        j += 4;
                        break;
                    case 1:
                        destArray[j + 0] = alphabet[(srcArray[i] & 0xfc) >> 2];
                        destArray[j + 1] = alphabet[(srcArray[i] & 0x03) << 4];
                        destArray[j + 2] = EQUALS_SIGN;
                        destArray[j + 3] = EQUALS_SIGN;
                        j += 4;
                        break;
                }
                //remove last byte if it's NewLine
                int destLength = 0u >= (uint)(destArray[j - 1] - NEW_LINE) ? j - 1 : j;
                return destLength;
            }
        }

        static unsafe int EncodeUsingGetSet(byte* alphabet, IByteBuffer src, IByteBuffer dest, int offset, int length, bool breakLines)
        {
            int i = 0;
            int j = 0;
            int charCount = 0;
            int remainderLength = length % 3;
            byte b0 = 0, b1 = 0, b2 = 0;
            int calcLength = offset + length - remainderLength;
            for (i = offset; i < calcLength; i += 3)
            {
                if (breakLines)
                {
                    if (0u >= (uint)(charCount - MAX_LINE_LENGTH))
                    {
                        _ = dest.SetByte(j++, NEW_LINE);
                        charCount = 0;
                    }
                    charCount += 4;
                }
                b0 = src.GetByte(i);
                b1 = src.GetByte(i + 1);
                b2 = src.GetByte(i + 2);

                _ = dest.SetByte(j + 0, alphabet[(b0 & 0xfc) >> 2]);
                _ = dest.SetByte(j + 1, alphabet[((b0 & 0x03) << 4) | ((b1 & 0xf0) >> 4)]);
                _ = dest.SetByte(j + 2, alphabet[((b1 & 0x0f) << 2) | ((b2 & 0xc0) >> 6)]);
                _ = dest.SetByte(j + 3, alphabet[(b2 & 0x3f)]);
                j += 4;
            }

            i = calcLength;

            if (breakLines && ((uint)remainderLength > 0u) && (0u >= (uint)(charCount - MAX_LINE_LENGTH)))
            {
                _ = dest.SetByte(j++, NEW_LINE);
            }
            switch (remainderLength)
            {
                case 2:
                    b0 = src.GetByte(i);
                    b1 = src.GetByte(i + 1);
                    _ = dest.SetByte(j + 0, alphabet[(b0 & 0xfc) >> 2]);
                    _ = dest.SetByte(j + 1, alphabet[((b0 & 0x03) << 4) | ((b1 & 0xf0) >> 4)]);
                    _ = dest.SetByte(j + 2, alphabet[(b1 & 0x0f) << 2]);
                    _ = dest.SetByte(j + 3, EQUALS_SIGN);
                    j += 4;
                    break;
                case 1:
                    b0 = src.GetByte(i);
                    _ = dest.SetByte(j + 0, alphabet[(b0 & 0xfc) >> 2]);
                    _ = dest.SetByte(j + 1, alphabet[(b0 & 0x03) << 4]);
                    _ = dest.SetByte(j + 2, EQUALS_SIGN);
                    _ = dest.SetByte(j + 3, EQUALS_SIGN);
                    j += 4;
                    break;
            }
            //remove last byte if it's NewLine
            int destLength = 0u >= (uint)(dest.GetByte(j - 1) - NEW_LINE) ? j - 1 : j;
            return destLength;
        }

        public static unsafe IByteBuffer Encode(IByteBuffer src, int offset, int length, bool breakLines, IBase64Dialect dialect, IByteBufferAllocator allocator)
        {
            if (src is null)
            {
                CThrowHelper.ThrowArgumentNullException(CExceptionArgument.src);
            }
            //if (dialect.alphabet is null)
            //{
            //    CThrowHelper.ThrowArgumentNullException(CExceptionArgument.dialect_alphabet);
            //}
            Debug.Assert(dialect.Alphabet.Length == 64, "alphabet.Length must be 64!");
            if ((offset < src.ReaderIndex) || (offset + length > src.WriterIndex/*src.ReaderIndex + src.ReadableBytes*/))
            {
                CThrowHelper.ThrowArgumentOutOfRangeException(CExceptionArgument.offset);
            }
            if ((uint)(length - 1) > SharedConstants.TooBigOrNegative)
            {
                return Unpooled.Empty;
            }

            int remainderLength = length % 3;
            int outLength = length / 3 * 4 + (remainderLength > 0 ? 4 : 0);
            outLength += breakLines ? outLength / MAX_LINE_LENGTH : 0;
            IByteBuffer dest = allocator.Buffer(outLength);
            int destLength = 0;
            int destIndex = dest.WriterIndex;

            fixed (byte* alphabet = &MemoryMarshal.GetReference(dialect.Alphabet))
            {
                if (src.IsSingleIoBuffer && dest.IsSingleIoBuffer)
                {
                    destLength = EncodeUsingPointer(alphabet, src, dest, offset, length, breakLines);
                }
                else
                {
                    destLength = EncodeUsingGetSet(alphabet, src, dest, offset, length, breakLines);
                }
            }
            return dest.SetIndex(destIndex, destIndex + destLength);
        }

        public static IByteBuffer Decode(IByteBuffer src) => Decode(src, Base64Dialect.Standard);

        public static IByteBuffer Decode(IByteBuffer src, IBase64Dialect dialect) => Decode(src, src.ReaderIndex, src.ReadableBytes, dialect);

        public static IByteBuffer Decode(IByteBuffer src, int offset, int length, IBase64Dialect dialect) => Decode(src, offset, length, dialect, src.Allocator);

        static unsafe int DecodeUsingPointer(IByteBuffer src, IByteBuffer dest, sbyte* decodabet, int offset, int length)
        {
            int charCount = 0;
            fixed (byte* srcArray = src.Array, d = dest.Array)
            {
                byte* destArray = d + dest.ArrayOffset + dest.WriterIndex;
                byte* b4 = stackalloc byte[4];
                int b4Count = 0;
                int i = src.ArrayOffset + offset;
                int calcLength = src.ArrayOffset + offset + length;
                for (; i < calcLength; ++i)
                {
                    var value = srcArray[i];
                    var sbiDecode = decodabet[value];
                    if (sbiDecode < WHITE_SPACE_ENC)
                    {
                        CThrowHelper.ThrowArgumentException_InvalidBase64InputChar(i, value);
                    }
                    if (sbiDecode >= EQUALS_SIGN_ENC)
                    {
                        b4[b4Count++] = value;
                        if (b4Count <= 3) { continue; }

                        if (0u >= (uint)(b4[2] - EQUALS_SIGN))
                        {
                            int output = ((decodabet[b4[0]] & 0xFF) << 18) |
                                ((decodabet[b4[1]] & 0xFF) << 12);
                            destArray[charCount++] = (byte)((uint)output >> 16);
                        }
                        else if (0u >= (uint)(b4[3] - EQUALS_SIGN))
                        {
                            int output = ((decodabet[b4[0]] & 0xFF) << 18) |
                                ((decodabet[b4[1]] & 0xFF) << 12) |
                                ((decodabet[b4[2]] & 0xFF) << 6);
                            destArray[charCount++] = (byte)((uint)output >> 16);
                            destArray[charCount++] = (byte)((uint)output >> 8);
                        }
                        else
                        {
                            int output = ((decodabet[b4[0]] & 0xFF) << 18) |
                                ((decodabet[b4[1]] & 0xFF) << 12) |
                                ((decodabet[b4[2]] & 0xFF) << 6) |
                                ((decodabet[b4[3]] & 0xFF) << 0);
                            destArray[charCount++] = (byte)((uint)output >> 16);
                            destArray[charCount++] = (byte)((uint)output >> 8);
                            destArray[charCount++] = (byte)((uint)output >> 0);
                        }

                        b4Count = 0;
                        if (0u >= (uint)(value - EQUALS_SIGN))
                        {
                            break;
                        }
                    }
                }
            }
            return charCount;
        }

        static unsafe int DecodeUsingGetSet(IByteBuffer src, IByteBuffer dest, sbyte* decodabet, int offset, int length)
        {
            int charCount = 0;

            byte* b4 = stackalloc byte[4];
            int b4Count = 0;
            int i = 0;

            for (i = offset; i < offset + length; ++i)
            {
                var value = src.GetByte(i);
                var sbiDecode = decodabet[value];
                if (sbiDecode < WHITE_SPACE_ENC)
                {
                    CThrowHelper.ThrowArgumentException_InvalidBase64InputChar(i, value);
                }
                if (sbiDecode >= EQUALS_SIGN_ENC)
                {
                    b4[b4Count++] = value;
                    if (b4Count <= 3) { continue; }

                    if (0u >= (uint)(b4[2] - EQUALS_SIGN))
                    {
                        int output = ((decodabet[b4[0]] & 0xFF) << 18) |
                            ((decodabet[b4[1]] & 0xFF) << 12);
                        _ = dest.SetByte(charCount++, (int)((uint)output >> 16));
                    }
                    else if (0u >= (uint)(b4[3] - EQUALS_SIGN))
                    {
                        int output = ((decodabet[b4[0]] & 0xFF) << 18) |
                            ((decodabet[b4[1]] & 0xFF) << 12) |
                            ((decodabet[b4[2]] & 0xFF) << 6);
                        _ = dest.SetByte(charCount++, (int)((uint)output >> 16));
                        _ = dest.SetByte(charCount++, (int)((uint)output >> 8));
                    }
                    else
                    {
                        int output = ((decodabet[b4[0]] & 0xFF) << 18) |
                            ((decodabet[b4[1]] & 0xFF) << 12) |
                            ((decodabet[b4[2]] & 0xFF) << 6) |
                            ((decodabet[b4[3]] & 0xFF) << 0);
                        _ = dest.SetByte(charCount++, (int)((uint)output >> 16));
                        _ = dest.SetByte(charCount++, (int)((uint)output >> 8));
                        _ = dest.SetByte(charCount++, (int)((uint)output >> 0));
                    }

                    b4Count = 0;
                    if (0u >= (uint)(value - EQUALS_SIGN))
                    {
                        break;
                    }
                }
            }
            return charCount;
        }

        public static unsafe IByteBuffer Decode(IByteBuffer src, int offset, int length, IBase64Dialect dialect, IByteBufferAllocator allocator)
        {
            if (src is null)
            {
                CThrowHelper.ThrowArgumentNullException(CExceptionArgument.src);
            }
            //if (dialect.Decodabet is null)
            //{
            //    CThrowHelper.ThrowArgumentNullException(CExceptionArgument.dialect_decodabet);
            //}
            if ((offset < src.ReaderIndex) || (offset + length > src.WriterIndex/*src.ReaderIndex + src.ReadableBytes*/))
            {
                CThrowHelper.ThrowArgumentOutOfRangeException(CExceptionArgument.offset);
            }
            Debug.Assert(dialect.Decodabet.Length == 256, "decodabet.Length must be 256!");
            if ((uint)(length - 1) > SharedConstants.TooBigOrNegative)
            {
                return Unpooled.Empty;
            }

            int outLength = length * 3 / 4;
            IByteBuffer dest = allocator.Buffer(outLength);
            int charCount = 0;
            int destIndex = dest.WriterIndex;

            fixed (sbyte* decodabet = &MemoryMarshal.GetReference(dialect.Decodabet))
            {
                if (src.IsSingleIoBuffer && dest.IsSingleIoBuffer)
                {
                    charCount = DecodeUsingPointer(src, dest, decodabet, offset, length);
                }
                else
                {
                    charCount = DecodeUsingGetSet(src, dest, decodabet, offset, length);
                }
            }

            return dest.SetIndex(destIndex, destIndex + charCount);
        }
    }
}