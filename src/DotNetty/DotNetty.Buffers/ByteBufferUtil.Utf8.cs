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
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    partial class ByteBufferUtil
    {
        static readonly int MaxBytesPerCharUtf8;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static int Utf8MaxBytes(ICharSequence seq) => seq.Count * MaxBytesPerCharUtf8;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static int Utf8MaxBytes(string seq) => seq.Length * MaxBytesPerCharUtf8;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static int Utf8MaxBytes(int seqLength) => seqLength * MaxBytesPerCharUtf8;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static int Utf8Bytes(string seq)
        {
            return seq is object ? TextEncodings.Utf8.GetByteCount(seq.AsSpan()) : 0;
        }


        public static IByteBuffer WriteUtf8(IByteBufferAllocator alloc, ICharSequence seq)
        {
            // UTF-8 uses max. 3 bytes per char, so calculate the worst case.
            var maxByteCount = Utf8MaxBytes(seq);
            IByteBuffer buf = alloc.Buffer(maxByteCount);
            _ = ReserveAndWriteUtf8(buf, seq, maxByteCount);
            return buf;
        }

        public static int WriteUtf8(IByteBuffer buf, ICharSequence seq)
        {
            var seqCount = seq.Count;
            return ReserveAndWriteUtf8Seq(buf, seq, 0, seqCount, Utf8MaxBytes(seqCount));
        }

        public static int WriteUtf8(IByteBuffer buf, ICharSequence seq, int start, int end)
        {
            var seqCount = seq.Count;
            var length = end - start;
            if (MathUtil.IsOutOfBounds(start, length, seqCount))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Expected_Seq(start, seqCount, end);
            }
            return ReserveAndWriteUtf8Seq(buf, seq, start, end, Utf8MaxBytes(length));
        }

        public static int ReserveAndWriteUtf8(IByteBuffer buf, ICharSequence seq, int reserveBytes)
            => ReserveAndWriteUtf8Seq(buf, seq, 0, seq.Count, reserveBytes);

        public static int ReserveAndWriteUtf8(IByteBuffer buf, ICharSequence seq, int start, int end, int reserveBytes)
        {
            var seqCount = seq.Count;
            if (MathUtil.IsOutOfBounds(start, end - start, seqCount))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Expected_Seq(start, seqCount, end);
            }
            return ReserveAndWriteUtf8Seq(buf, seq, start, end, reserveBytes);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private static int ReserveAndWriteUtf8Seq(IByteBuffer buf, ICharSequence seq, int start, int end, int reserveBytes)
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
                        byteBuf.EnsureWritable0(reserveBytes);
                        int written = WriteUtf8(byteBuf, byteBuf.WriterIndex, seq, start, end);
                        byteBuf.SetWriterIndex(byteBuf.WriterIndex + written);
                        return written;

                    case WrappedByteBuffer _:
                        // Unwrap as the wrapped buffer may be an AbstractByteBuf and so we can use fast-path.
                        buf = buf.Unwrap();
                        break;

                    default:
                        if (seq is IHasUtf16Span hasUtf16)
                        {
#if NETCOREAPP_2_X_GREATER || NETSTANDARD_2_0_GREATER
                            var utf16Span = hasUtf16.Utf16Span[start..end];
#else
                            var utf16Span = hasUtf16.Utf16Span.Slice(start, end - start);
#endif
                            byte[] tempArray = null;
                            try
                            {
                                Span<byte> utf8Bytes = (uint)reserveBytes <= SharedConstants.uStackallocThreshold ?
                                    stackalloc byte[reserveBytes] :
                                    (tempArray = ArrayPool<byte>.Shared.Rent(reserveBytes));
                                var result = TextEncodings.Utf16.ToUtf8(utf16Span, utf8Bytes, out _, out int bytesWritten);
                                Debug.Assert(result == OperationStatus.Done);
                                buf.WriteBytes(utf8Bytes.Slice(0, bytesWritten));
                                return bytesWritten;
                            }
                            finally
                            {
                                if (tempArray is object) { ArrayPool<byte>.Shared.Return(tempArray); }
                            }
                        }
                        else
                        {
                            byte[] bytes = TextEncodings.UTF8NoBOM.GetBytes(seq.SubSequence(start, end).ToString());
                            buf.WriteBytes(bytes);
                            return bytes.Length;
                        }
                }
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static ICharSequence CheckCharSequenceBounds(ICharSequence seq, int start, int end)
        {
            var seqCount = seq.Count;
            if (MathUtil.IsOutOfBounds(start, end - start, seqCount))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Expected_Seq(start, seqCount, end);
            }
            return seq;
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        internal static int WriteUtf8(AbstractByteBuffer buffer, int writerIndex, ICharSequence value)
            => WriteUtf8(buffer, writerIndex, value, 0, value.Count);

        // Fast-Path implementation
        internal static int WriteUtf8(AbstractByteBuffer buffer, int writerIndex, ICharSequence value, int start, int end)
        {
            if (value is IHasUtf16Span hasUtf16)
            {
#if NETCOREAPP_2_X_GREATER || NETSTANDARD_2_0_GREATER
                var utf16Span = hasUtf16.Utf16Span[start..end];
#else
                var utf16Span = hasUtf16.Utf16Span.Slice(start, end - start);
#endif
                if (buffer.IsSingleIoBuffer)
                {
                    var bufSpan = buffer.GetSpan(writerIndex, buffer.Capacity - writerIndex);
                    var status = TextEncodings.Utf16.ToUtf8(utf16Span, bufSpan, out _, out var written);
                    if (status == OperationStatus.Done) { return written; }
                }
                else
                {
                    if (TryWriteUtf8Composite(buffer, writerIndex, utf16Span, out var written)) { return written; }
                }
                return WriteUtf80(buffer, writerIndex, utf16Span);
            }
            return WriteUtf80(buffer, writerIndex, value, start, end);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TryWriteUtf8Composite(AbstractByteBuffer buffer, int writerIndex, in ReadOnlySpan<char> utf16Span, out int written)
        {
            var tempArray = ArrayPool<byte>.Shared.Rent(buffer.Capacity);
            try
            {
                var status = TextEncodings.Utf16.ToUtf8(utf16Span, tempArray.AsSpan(), out _, out written);
                if (status == OperationStatus.Done)
                {
                    _ = buffer.SetBytes(writerIndex, tempArray, 0, written);
                    return true;
                }
                return false;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(tempArray);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int WriteUtf80(AbstractByteBuffer buffer, int writerIndex, ICharSequence value, int start, int end)
        {
            int oldWriterIndex = writerIndex;

            // We can use the _set methods as these not need to do any index checks and reference checks.
            // This is possible as we called ensureWritable(...) before.
            for (int i = start; i < end; i++)
            {
                char c = value[i];
                if (c < 0x80)
                {
                    buffer._SetByte(writerIndex++, (byte)c);
                }
                else if (c < 0x800)
                {
                    buffer._SetByte(writerIndex++, (byte)(0xc0 | (c >> 6)));
                    buffer._SetByte(writerIndex++, (byte)(0x80 | (c & 0x3f)));
                }
                else if (char.IsSurrogate(c))
                {
                    if (!char.IsHighSurrogate(c))
                    {
                        buffer._SetByte(writerIndex++, WriteUtfUnknown);
                        continue;
                    }
                    // Surrogate Pair consumes 2 characters.
                    if (++i == end)
                    {
                        buffer._SetByte(writerIndex++, WriteUtfUnknown);
                        break;
                    }
                    // Extra method to allow inlining the rest of writeUtf8 which is the most likely code path.
                    writerIndex = WriteUtf8Surrogate(buffer, writerIndex, c, value[i]);
                }
                else
                {
                    buffer._SetByte(writerIndex++, (byte)(0xe0 | (c >> 12)));
                    buffer._SetByte(writerIndex++, (byte)(0x80 | ((c >> 6) & 0x3f)));
                    buffer._SetByte(writerIndex++, (byte)(0x80 | (c & 0x3f)));
                }
            }

            return writerIndex - oldWriterIndex;
        }


        public static IByteBuffer WriteUtf8(IByteBufferAllocator alloc, string value)
        {
            // UTF-8 uses max. 3 bytes per char, so calculate the worst case.
            var maxByteCount = Utf8MaxBytes(value);
            IByteBuffer buf = alloc.Buffer(maxByteCount);
            _ = ReserveAndWriteUtf8(buf, value.AsSpan(), maxByteCount);
            return buf;
        }


        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static int WriteUtf8(IByteBuffer buf, string seq) => ReserveAndWriteUtf8(buf, seq.AsSpan(), Utf8MaxBytes(seq));

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static int WriteUtf8(IByteBuffer buf, in ReadOnlySpan<char> chars) => ReserveAndWriteUtf8(buf, chars, Utf8MaxBytes(chars.Length));

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static int ReserveAndWriteUtf8(IByteBuffer buf, string value, int reserveBytes) => ReserveAndWriteUtf8(buf, value.AsSpan(), reserveBytes);

        ///<summary>
        /// Encode a string in http://en.wikipedia.org/wiki/UTF-8 and write it into reserveBytes of 
        /// a byte buffer. The reserveBytes must be computed (ie eagerly using {@link #utf8MaxBytes(string)}
        /// or exactly with #utf8Bytes(string)}) to ensure this method not to not: for performance reasons
        /// the index checks will be performed using just reserveBytes.
        /// </summary>
        /// <returns> This method returns the actual number of bytes written.</returns>
        public static int ReserveAndWriteUtf8(IByteBuffer buf, in ReadOnlySpan<char> chars, int reserveBytes)
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
                        byteBuf.EnsureWritable0(reserveBytes);
                        int written = WriteUtf8(byteBuf, byteBuf.WriterIndex, chars);
                        _ = byteBuf.SetWriterIndex(byteBuf.WriterIndex + written);
                        return written;

                    case WrappedByteBuffer _:
                        // Unwrap as the wrapped buffer may be an AbstractByteBuf and so we can use fast-path.
                        buf = buf.Unwrap();
                        break;

                    default:
                        byte[] tempArray = null;
                        try
                        {
                            Span<byte> utf8Bytes = (uint)reserveBytes <= SharedConstants.uStackallocThreshold ?
                                stackalloc byte[reserveBytes] :
                                (tempArray = ArrayPool<byte>.Shared.Rent(reserveBytes));
                            var result = TextEncodings.Utf16.ToUtf8(chars, utf8Bytes, out _, out int bytesWritten);
                            Debug.Assert(result == OperationStatus.Done);
                            _ = buf.WriteBytes(utf8Bytes.Slice(0, bytesWritten));
                            return bytesWritten;
                        }
                        finally
                        {
                            if (tempArray is object) { ArrayPool<byte>.Shared.Return(tempArray); }
                        }
                }
            }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        internal static int WriteUtf8(AbstractByteBuffer buffer, int writerIndex, string value) => WriteUtf8(buffer, writerIndex, value.AsSpan());

        // Fast-Path implementation
        internal static int WriteUtf8(AbstractByteBuffer buffer, int writerIndex, in ReadOnlySpan<char> chars)
        {
            if (buffer.IsSingleIoBuffer)
            {
                var bufSpan = buffer.GetSpan(writerIndex, buffer.Capacity - writerIndex);
                var status = TextEncodings.Utf16.ToUtf8(chars, bufSpan, out _, out var written);
                if (status == OperationStatus.Done) { return written; }
            }
            else
            {
                if (TryWriteUtf8Composite(buffer, writerIndex, chars, out var written)) { return written; }
            }
            return WriteUtf80(buffer, writerIndex, chars);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int WriteUtf80(AbstractByteBuffer buffer, int writerIndex, in ReadOnlySpan<char> chars)
        {
            int oldWriterIndex = writerIndex;
            var len = chars.Length;

            // We can use the _set methods as these not need to do any index checks and reference checks.
            // This is possible as we called ensureWritable(...) before.
            for (int i = 0; i < len; i++)
            {
                char c = chars[i];
                if (c < 0x80)
                {
                    buffer._SetByte(writerIndex++, (byte)c);
                }
                else if (c < 0x800)
                {
                    buffer._SetByte(writerIndex++, (byte)(0xc0 | (c >> 6)));
                    buffer._SetByte(writerIndex++, (byte)(0x80 | (c & 0x3f)));
                }
                else if (char.IsSurrogate(c))
                {
                    if (!char.IsHighSurrogate(c))
                    {
                        buffer._SetByte(writerIndex++, WriteUtfUnknown);
                        continue;
                    }
                    // Surrogate Pair consumes 2 characters.
                    if (++i == len)
                    {
                        buffer._SetByte(writerIndex++, WriteUtfUnknown);
                        break;
                    }
                    // Extra method to allow inlining the rest of writeUtf8 which is the most likely code path.
                    writerIndex = WriteUtf8Surrogate(buffer, writerIndex, c, chars[i]);
                }
                else
                {
                    buffer._SetByte(writerIndex++, (byte)(0xe0 | (c >> 12)));
                    buffer._SetByte(writerIndex++, (byte)(0x80 | ((c >> 6) & 0x3f)));
                    buffer._SetByte(writerIndex++, (byte)(0x80 | (c & 0x3f)));
                }
            }

            return writerIndex - oldWriterIndex;
        }

        static int WriteUtf8Surrogate(AbstractByteBuffer buffer, int writerIndex, char c, char c2)
        {
            if (!char.IsLowSurrogate(c2))
            {
                buffer._SetByte(writerIndex++, WriteUtfUnknown);
                buffer._SetByte(writerIndex++, char.IsHighSurrogate(c2) ? WriteUtfUnknown : c2);
                return writerIndex;
            }
            int codePoint = CharUtil.ToCodePoint(c, c2);
            // See http://www.unicode.org/versions/Unicode7.0.0/ch03.pdf#G2630.
            buffer._SetByte(writerIndex++, (byte)(0xf0 | (codePoint >> 18)));
            buffer._SetByte(writerIndex++, (byte)(0x80 | ((codePoint >> 12) & 0x3f)));
            buffer._SetByte(writerIndex++, (byte)(0x80 | ((codePoint >> 6) & 0x3f)));
            buffer._SetByte(writerIndex++, (byte)(0x80 | (codePoint & 0x3f)));
            return writerIndex;
        }

        static bool IsUtf8(IByteBuffer buf, int index, int length)
        {
            if (!buf.IsSingleIoBuffer) { return IsUtf8Slow(buf, index, length); }

            var utf8Span = buf.GetReadableSpan(index, length);
            ref byte utf8Source = ref MemoryMarshal.GetReference(utf8Span);

            nint offset = 0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            uint uLength = (uint)length;

            while ((uint)index < uLength)
            {
                byte b1 = Unsafe.AddByteOffset(ref utf8Source, offset);
                byte b2, b3;
                if (0u >= (uint)(b1 & 0x80))
                {
                    // 1 byte
                    index++;
                    offset += 1;
                    continue;
                }
                if ((b1 & 0xE0) == 0xC0)
                {
                    // 2 bytes
                    //
                    // Bit/Byte pattern
                    // 110xxxxx    10xxxxxx
                    // C2..DF      80..BF
                    if ((uint)(index + 1) >= uLength)
                    { // no enough bytes
                        return false;
                    }
                    b2 = Unsafe.AddByteOffset(ref utf8Source, offset + 1);
                    if ((b2 & 0xC0) != 0x80)
                    { // 2nd byte not starts with 10
                        return false;
                    }
                    if ((b1 & 0xFF) < 0xC2)
                    { // out of lower bound
                        return false;
                    }
                    index += 2;
                    offset += 2;
                }
                else if ((b1 & 0xF0) == 0xE0)
                {
                    // 3 bytes
                    //
                    // Bit/Byte pattern
                    // 1110xxxx    10xxxxxx    10xxxxxx
                    // E0          A0..BF      80..BF
                    // E1..EC      80..BF      80..BF
                    // ED          80..9F      80..BF
                    // E1..EF      80..BF      80..BF
                    if ((uint)(index + 2) >= uLength)
                    { // no enough bytes
                        return false;
                    }
                    b2 = Unsafe.AddByteOffset(ref utf8Source, offset + 1);
                    b3 = Unsafe.AddByteOffset(ref utf8Source, offset + 2);
                    if ((b2 & 0xC0) != 0x80 || (b3 & 0xC0) != 0x80)
                    { // 2nd or 3rd bytes not start with 10
                        return false;
                    }
                    if ((b1 & 0x0F) == 0x00 && (b2 & 0xFF) < 0xA0)
                    { // out of lower bound
                        return false;
                    }
                    if ((b1 & 0x0F) == 0x0D && (b2 & 0xFF) > 0x9F)
                    { // out of upper bound
                        return false;
                    }
                    index += 3;
                    offset += 3;
                }
                else if ((b1 & 0xF8) == 0xF0)
                {
                    // 4 bytes
                    //
                    // Bit/Byte pattern
                    // 11110xxx    10xxxxxx    10xxxxxx    10xxxxxx
                    // F0          90..BF      80..BF      80..BF
                    // F1..F3      80..BF      80..BF      80..BF
                    // F4          80..8F      80..BF      80..BF
                    if ((uint)(index + 3) >= uLength)
                    { // no enough bytes
                        return false;
                    }
                    b2 = Unsafe.AddByteOffset(ref utf8Source, offset + 1);
                    b3 = Unsafe.AddByteOffset(ref utf8Source, offset + 2);
                    byte b4 = Unsafe.AddByteOffset(ref utf8Source, offset + 3);
                    if ((b2 & 0xC0) != 0x80 || (b3 & 0xC0) != 0x80 || (b4 & 0xC0) != 0x80)
                    {
                        // 2nd, 3rd or 4th bytes not start with 10
                        return false;
                    }
                    if ((b1 & 0xFF) > 0xF4 // b1 invalid
                        || (b1 & 0xFF) == 0xF0 && (b2 & 0xFF) < 0x90    // b2 out of lower bound
                        || (b1 & 0xFF) == 0xF4 && (b2 & 0xFF) > 0x8F)
                    { // b2 out of upper bound
                        return false;
                    }
                    index += 4;
                    offset += 4;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        static bool IsUtf8Slow(IByteBuffer buf, int index, int length)
        {
            int endIndex = index + length;
            while (index < endIndex)
            {
                byte b1 = buf.GetByte(index++);
                byte b2, b3;
                if ((b1 & 0x80) == 0)
                {
                    // 1 byte
                    continue;
                }
                if ((b1 & 0xE0) == 0xC0)
                {
                    // 2 bytes
                    //
                    // Bit/Byte pattern
                    // 110xxxxx    10xxxxxx
                    // C2..DF      80..BF
                    if (index >= endIndex)
                    { // no enough bytes
                        return false;
                    }
                    b2 = buf.GetByte(index++);
                    if ((b2 & 0xC0) != 0x80)
                    { // 2nd byte not starts with 10
                        return false;
                    }
                    if ((b1 & 0xFF) < 0xC2)
                    { // out of lower bound
                        return false;
                    }
                }
                else if ((b1 & 0xF0) == 0xE0)
                {
                    // 3 bytes
                    //
                    // Bit/Byte pattern
                    // 1110xxxx    10xxxxxx    10xxxxxx
                    // E0          A0..BF      80..BF
                    // E1..EC      80..BF      80..BF
                    // ED          80..9F      80..BF
                    // E1..EF      80..BF      80..BF
                    if (index > endIndex - 2)
                    { // no enough bytes
                        return false;
                    }
                    b2 = buf.GetByte(index++);
                    b3 = buf.GetByte(index++);
                    if ((b2 & 0xC0) != 0x80 || (b3 & 0xC0) != 0x80)
                    { // 2nd or 3rd bytes not start with 10
                        return false;
                    }
                    if ((b1 & 0x0F) == 0x00 && (b2 & 0xFF) < 0xA0)
                    { // out of lower bound
                        return false;
                    }
                    if ((b1 & 0x0F) == 0x0D && (b2 & 0xFF) > 0x9F)
                    { // out of upper bound
                        return false;
                    }
                }
                else if ((b1 & 0xF8) == 0xF0)
                {
                    // 4 bytes
                    //
                    // Bit/Byte pattern
                    // 11110xxx    10xxxxxx    10xxxxxx    10xxxxxx
                    // F0          90..BF      80..BF      80..BF
                    // F1..F3      80..BF      80..BF      80..BF
                    // F4          80..8F      80..BF      80..BF
                    if (index > endIndex - 3)
                    { // no enough bytes
                        return false;
                    }
                    b2 = buf.GetByte(index++);
                    b3 = buf.GetByte(index++);
                    byte b4 = buf.GetByte(index++);
                    if ((b2 & 0xC0) != 0x80 || (b3 & 0xC0) != 0x80 || (b4 & 0xC0) != 0x80)
                    {
                        // 2nd, 3rd or 4th bytes not start with 10
                        return false;
                    }
                    if ((b1 & 0xFF) > 0xF4 // b1 invalid
                        || (b1 & 0xFF) == 0xF0 && (b2 & 0xFF) < 0x90    // b2 out of lower bound
                        || (b1 & 0xFF) == 0xF4 && (b2 & 0xFF) > 0x8F)
                    { // b2 out of upper bound
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
