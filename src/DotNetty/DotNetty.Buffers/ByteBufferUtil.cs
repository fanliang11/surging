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
    using System.Runtime.CompilerServices;
    using System.Text;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;

    public static partial class ByteBufferUtil
    {
        const int IndexNotFound = -1;
        const char WriteUtfUnknown = '?';

        static readonly IInternalLogger Logger;

        public static readonly IByteBufferAllocator DefaultAllocator;

        static ByteBufferUtil()
        {
            Logger = InternalLoggerFactory.GetInstance(typeof(ByteBufferUtil));

            string allocType = SystemPropertyUtil.Get("io.netty.allocator.type", "pooled");
            allocType = allocType.Trim();

            IByteBufferAllocator alloc;
            if ("unpooled".Equals(allocType, StringComparison.OrdinalIgnoreCase))
            {
                alloc = UnpooledByteBufferAllocator.Default;
                Logger.Debug("-Dio.netty.allocator.type: {}", allocType);
            }
            else if ("pooled".Equals(allocType, StringComparison.OrdinalIgnoreCase))
            {
                alloc = PooledByteBufferAllocator.Default;
                Logger.Debug("-Dio.netty.allocator.type: {}", allocType);
            }
            else if ("arraypooled".Equals(allocType, StringComparison.OrdinalIgnoreCase))
            {
                alloc = ArrayPooledByteBufferAllocator.Default;
                Logger.Debug("-Dio.netty.allocator.type: {}", allocType);
            }
            else
            {
                alloc = PooledByteBufferAllocator.Default;
                Logger.Debug("-Dio.netty.allocator.type: pooled (unknown: {})", allocType);
            }

            DefaultAllocator = alloc;
            MaxBytesPerCharUtf8 = Encoding.UTF8.GetMaxByteCount(1);
            AsciiByteProcessor = new FindNonAscii();
        }

        public static bool EnsureWritableSuccess(int ensureWritableResult)
        {
            var nresult = (uint)ensureWritableResult;
            return 0u >= nresult || 2u == nresult;
        }

        /// <summary>
        ///     Read the given amount of bytes into a new <see cref="IByteBuffer"/> that is allocated from the <see cref="IByteBufferAllocator"/>.
        /// </summary>
        public static IByteBuffer ReadBytes(IByteBufferAllocator alloc, IByteBuffer buffer, int length)
        {
            bool release = true;
            IByteBuffer dst = alloc.Buffer(length);
            try
            {
                _ = buffer.ReadBytes(dst);
                release = false;
                return dst;
            }
            finally
            {
                if (release)
                {
                    _ = dst.Release();
                }
            }
        }

        /// <summary>
        /// Create a copy of the underlying storage from <paramref name="buf"/> into a byte array.
        /// The copy will start at <see cref="IByteBuffer.ReaderIndex"/> and copy <see cref="IByteBuffer.ReadableBytes"/> bytes.
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static byte[] GetBytes(IByteBuffer buf)
        {
            return GetBytes(buf, buf.ReaderIndex, buf.ReadableBytes, true);
        }

        /// <summary>
        /// Create a copy of the underlying storage from <paramref name="buf"/> into a byte array.
        /// The copy will start at <paramref name="start"/> and copy <paramref name="length"/> bytes.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GetBytes(IByteBuffer buf, int start, int length)
        {
            return GetBytes(buf, start, length, true);
        }

        /// <summary>
        /// Return an array of the underlying storage from <paramref name="buf"/> into a byte array.
        /// The copy will start at {@code start} and copy {@code length} bytes.
        /// If <paramref name="copy"/> is true a copy will be made of the memory.
        /// If <paramref name="copy"/> is false the underlying storage will be shared, if possible.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="copy"></param>
        /// <returns></returns>
        public static byte[] GetBytes(IByteBuffer buf, int start, int length, bool copy)
        {
            var capacity = buf.Capacity;
            if (MathUtil.IsOutOfBounds(start, length, capacity))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Expected(start, length, capacity);
            }

            if (buf.HasArray)
            {
                if (copy || start != 0 || length != capacity)
                {
                    int baseOffset = buf.ArrayOffset + start;
                    var bytes = new byte[length];
                    PlatformDependent.CopyMemory(buf.Array, baseOffset, bytes, 0, length);
                    return bytes;
                }
                else
                {
                    return buf.Array;
                }
            }

            byte[] v = new byte[length];
            _ = buf.GetBytes(start, v);
            return v;
        }

        public static void Copy(AsciiString src, IByteBuffer dst) => Copy(src, 0, dst, src.Count);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static void Copy(AsciiString src, int srcIdx, IByteBuffer dst, int dstIdx, int length)
        {
            if (MathUtil.IsOutOfBounds(srcIdx, length, src.Count))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Src(srcIdx, length, src.Count);
            }
            if (dst is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst);
            }
            // ReSharper disable once PossibleNullReferenceException
            _ = dst.SetBytes(dstIdx, src.Array, srcIdx + src.Offset, length);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static void Copy(AsciiString src, int srcIdx, IByteBuffer dst, int length)
        {
            if (MathUtil.IsOutOfBounds(srcIdx, length, src.Count))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_Src(srcIdx, length, src.Count);
            }
            if (dst is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst);
            }
            // ReSharper disable once PossibleNullReferenceException
            _ = dst.WriteBytes(src.Array, srcIdx + src.Offset, length);
        }

        public static unsafe int SingleToInt32Bits(float value)
        {
            return *(int*)(&value);
        }

        public static unsafe float Int32BitsToSingle(int value)
        {
            return *(float*)(&value);
        }

        /// <summary>
        ///     Toggles the endianness of the specified 64-bit long integer.
        /// </summary>
        public static long SwapLong(long value)
#if !NETFRAMEWORK
            => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
#else
            => ((SwapInt((int)value) & 0xFFFFFFFF) << 32)
                | (SwapInt((int)(value >> 32)) & 0xFFFFFFFF);
#endif

        /// <summary>
        ///     Toggles the endianness of the specified 32-bit integer.
        /// </summary>
        public static int SwapInt(int value)
#if !NETFRAMEWORK
            => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
#else
            => ((SwapShort((short)value) & 0xFFFF) << 16)
                | (SwapShort((short)(value >> 16)) & 0xFFFF);
#endif

        /// <summary>
        ///     Toggles the endianness of the specified 16-bit integer.
        /// </summary>
        public static short SwapShort(short value)
#if !NETFRAMEWORK
            => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
#else
            => (short)(((value & 0xFF) << 8) | (value >> 8) & 0xFF);
#endif

    }
}
