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
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using DotNetty.Common.Internal;

    partial class ByteBufferUtil
    {
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsText(IByteBuffer buf, Encoding charset) => IsText(buf, buf.ReaderIndex, buf.ReadableBytes, charset);

        public static bool IsText(IByteBuffer buf, int index, int length, Encoding encoding)
        {
            if (buf is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buf); }
            if (encoding is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.encoding); }

            int maxIndex = buf.ReaderIndex + buf.ReadableBytes;
            if (index < 0 || length < 0 || index > maxIndex - length)
            {
                ThrowHelper.ThrowIndexOutOfRangeException_IsText(index, length);
            }
            switch (encoding.CodePage)
            {
                case TextEncodings.UTF8CodePage:
                    return IsUtf8(buf, index, length);

                case TextEncodings.ASCIICodePage:
                    return IsAscii(buf, index, length);

                default:
                    try
                    {
                        if (buf.IsSingleIoBuffer)
                        {
                            ArraySegment<byte> segment = buf.GetIoBuffer();
                            _ = encoding.GetChars(segment.Array, segment.Offset, segment.Count);
                        }
                        else
                        {
                            IByteBuffer heapBuffer = buf.Allocator.HeapBuffer(length);
                            try
                            {
                                _ = heapBuffer.WriteBytes(buf, index, length);
                                ArraySegment<byte> segment = heapBuffer.GetIoBuffer();
                                _ = encoding.GetChars(segment.Array, segment.Offset, segment.Count);
                            }
                            finally
                            {
                                _ = heapBuffer.Release();
                            }
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
            }
        }

        /// <summary>
        ///     Encode the given <see cref="string" /> using the given <see cref="Encoding" /> into a new
        ///     <see cref="IByteBuffer" /> which
        ///     is allocated via the <see cref="IByteBufferAllocator" />.
        /// </summary>
        /// <param name="alloc">The <see cref="IByteBufferAllocator" /> to allocate {@link IByteBuffer}.</param>
        /// <param name="src">src The <see cref="string" /> to encode.</param>
        /// <param name="encoding">charset The specified <see cref="Encoding" /></param>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static IByteBuffer EncodeString(IByteBufferAllocator alloc, string src, Encoding encoding) => EncodeString0(alloc, false, src, encoding, 0);

        /// <summary>
        ///     Encode the given <see cref="string" /> using the given <see cref="Encoding" /> into a new
        ///     <see cref="IByteBuffer" /> which
        ///     is allocated via the <see cref="IByteBufferAllocator" />.
        /// </summary>
        /// <param name="alloc">The <see cref="IByteBufferAllocator" /> to allocate {@link IByteBuffer}.</param>
        /// <param name="src">src The <see cref="string" /> to encode.</param>
        /// <param name="encoding">charset The specified <see cref="Encoding" /></param>
        /// <param name="extraCapacity">the extra capacity to alloc except the space for decoding.</param>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static IByteBuffer EncodeString(IByteBufferAllocator alloc, string src, Encoding encoding, int extraCapacity) => EncodeString0(alloc, false, src, encoding, extraCapacity);

        internal static IByteBuffer EncodeString0(IByteBufferAllocator alloc, bool enforceHeap, string src, Encoding encoding, int extraCapacity)
        {
            int length = encoding.GetMaxByteCount(src.Length) + extraCapacity;
            bool release = true;

            IByteBuffer dst = enforceHeap ? alloc.HeapBuffer(length) : alloc.Buffer(length);
            Debug.Assert(dst.HasArray, "Operation expects allocator to operate array-based buffers.");

            try
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                int written = encoding.GetBytes(src.AsSpan(), dst.FreeSpan);
#else
                int written = encoding.GetBytes(src, 0, src.Length, dst.Array, dst.ArrayOffset + dst.WriterIndex);
#endif
                dst.SetWriterIndex(dst.WriterIndex + written);
                release = false;

                return dst;
            }
            finally
            {
                if (release) { dst.Release(); }
            }
        }

        public static string DecodeString(IByteBuffer src, int readerIndex, int len, Encoding encoding)
        {
            if (0u >= (uint)len) { return string.Empty; }

#if NET451
            if (src.IsSingleIoBuffer)
            {
                ArraySegment<byte> ioBuf = src.GetIoBuffer(readerIndex, len);
                return encoding.GetString(ioBuf.Array, ioBuf.Offset, ioBuf.Count);
            }
            else
            {
                int maxLength = encoding.GetMaxCharCount(len);
                IByteBuffer buffer = src.Allocator.HeapBuffer(maxLength);
                try
                {
                    buffer.WriteBytes(src, readerIndex, len);
                    ArraySegment<byte> ioBuf = buffer.GetIoBuffer();
                    return encoding.GetString(ioBuf.Array, ioBuf.Offset, ioBuf.Count);
                }
                finally
                {
                    // Release the temporary buffer again.
                    buffer.Release();
                }
            }
#else
            var source = src.GetReadableSpan(readerIndex, len);
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            return encoding.GetString(source);
#else
            unsafe
            {
                fixed (byte* bytes = &MemoryMarshal.GetReference(source))
                {
                    return encoding.GetString(bytes, source.Length);
                }
            }
#endif
#endif
        }
    }
}
