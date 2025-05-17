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
    using System.Buffers;
    using System.Runtime.CompilerServices;
    using System.Text;
    using DotNetty.Common.Utilities;
#if !(NETCOREAPP || NETSTANDARD_2_0_GREATER)
    using System.Runtime.InteropServices;
#endif

    partial class ByteBufferUtil
    {
        public static IByteBuffer WriteAscii(IByteBufferAllocator alloc, ICharSequence seq)
        {
            // ASCII uses 1 byte per char
            IByteBuffer buf = alloc.Buffer(seq.Count);
            _ = WriteAscii(buf, seq);
            return buf;
        }

        public static int WriteAscii(IByteBuffer buf, ICharSequence seq)
        {
            // ASCII uses 1 byte per char
            int len = seq.Count;
            if (seq is AsciiString asciiString)
            {
                _ = buf.WriteBytes(asciiString.Array, asciiString.Offset, len);
            }
            else
            {
                while (true)
                {
                    switch (buf)
                    {
                        case WrappedCompositeByteBuffer _:
                            // WrappedCompositeByteBuf is a sub-class of AbstractByteBuf so it needs special handling.
                            buf = buf.Unwrap();
                            break;

                        case AbstractByteBuffer byteBuf:
                            byteBuf.EnsureWritable0(len);
                            int written = WriteAscii(byteBuf, byteBuf.WriterIndex, seq);
                            _ = byteBuf.SetWriterIndex(byteBuf.WriterIndex + written);
                            return written;

                        case WrappedByteBuffer _:
                            // Unwrap as the wrapped buffer may be an AbstractByteBuf and so we can use fast-path.
                            buf = buf.Unwrap();
                            break;

                        default:
                            byte[] bytes = Encoding.ASCII.GetBytes(seq.ToString());
                            _ = buf.WriteBytes(bytes);
                            return bytes.Length;
                    }
                }
            }
            return len;
        }

        public static IByteBuffer WriteAscii(IByteBufferAllocator alloc, string value)
        {
            // ASCII uses 1 byte per char
            IByteBuffer buf = alloc.Buffer(value.Length);
            _ = WriteAscii(buf, value);
            return buf;
        }

        public static int WriteAscii(IByteBuffer buf, string value)
        {
            // ASCII uses 1 byte per char
            int len = value.Length;
            while (true)
            {
                switch (buf)
                {
                    case WrappedCompositeByteBuffer _:
                        // WrappedCompositeByteBuf is a sub-class of AbstractByteBuf so it needs special handling.
                        buf = buf.Unwrap();
                        break;

                    case AbstractByteBuffer byteBuf:
                        byteBuf.EnsureWritable0(len);
                        int written = WriteAscii(byteBuf, byteBuf.WriterIndex, value);
                        _ = byteBuf.SetWriterIndex(byteBuf.WriterIndex + written);
                        return written;

                    case WrappedByteBuffer _:
                        // Unwrap as the wrapped buffer may be an AbstractByteBuf and so we can use fast-path.
                        buf = buf.Unwrap();
                        break;

                    default:
                        byte[] bytes = Encoding.ASCII.GetBytes(value);
                        _ = buf.WriteBytes(bytes);
                        return bytes.Length;
                }
            }
        }

        // Fast-Path implementation
        internal static int WriteAscii(AbstractByteBuffer buffer, int writerIndex, ICharSequence seq)
        {
            if (seq is IHasAsciiSpan hasAscii)
            {
                _ = buffer.SetBytes(writerIndex, hasAscii.AsciiSpan);
                return seq.Count;
            }
            if (seq is IHasUtf16Span hasUtf16)
            {
                return WriteAscii0(buffer, writerIndex, hasUtf16.Utf16Span);
            }

            return WriteAscii0(buffer, writerIndex, seq);
        }

        internal static int WriteAscii(AbstractByteBuffer buffer, int writerIndex, string value)
        {
            return WriteAscii0(buffer, writerIndex, value.AsSpan());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int WriteAscii0(AbstractByteBuffer buffer, int writerIndex, ICharSequence seq)
        {
            var len = seq.Count;
            // We can use the _set methods as these not need to do any index checks and reference checks.
            // This is possible as we called ensureWritable(...) before.
            for (int i = 0; i < len; i++)
            {
                buffer._SetByte(writerIndex++, AsciiString.CharToByte(seq[i]));
            }
            return len;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int WriteAscii0(AbstractByteBuffer buffer, int writerIndex, in ReadOnlySpan<char> utf16Source)
        {
            var charCount = utf16Source.Length;
            if (buffer.IsSingleIoBuffer)
            {
                var asciiDestination = buffer.GetSpan(writerIndex, buffer.Capacity - writerIndex);
                WriteAscii0(utf16Source, asciiDestination, charCount);
            }
            else
            {
                WriteAsciiComposite(buffer, writerIndex, utf16Source, charCount);
            }
            return charCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteAsciiComposite(AbstractByteBuffer buffer, int writerIndex, in ReadOnlySpan<char> utf16Source, int length)
        {
            var memory = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                WriteAscii0(utf16Source, memory.AsSpan(), length);
                _ = buffer.SetBytes(writerIndex, memory, 0, length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(memory);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteAscii0(in ReadOnlySpan<char> utf16Source, Span<byte> asciiDestination, int length)
        {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            Encoding.ASCII.GetBytes(utf16Source, asciiDestination);
#else
            unsafe
            {
                fixed (char* chars = &MemoryMarshal.GetReference(utf16Source))
                {
                    fixed (byte* bytes = &MemoryMarshal.GetReference(asciiDestination))
                    {
                        Encoding.ASCII.GetBytes(chars, length, bytes, length);
                    }
                }
            }
#endif
        }

        static readonly FindNonAscii AsciiByteProcessor;

        sealed class FindNonAscii : IByteProcessor
        {
            public bool Process(byte value) => value < 0x80;
        }

        static bool IsAscii(IByteBuffer buf, int index, int length) => buf.ForEachByte(index, length, AsciiByteProcessor) == -1;
    }
}
