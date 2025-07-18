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
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using DotNetty.Common.Internal;

    static unsafe class UnsafeByteBufferUtil
    {
        const byte Zero = 0;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static short GetShort(byte* bytes) =>
            unchecked((short)(((*bytes) << 8) | *(bytes + 1)));

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static short GetShortLE(byte* bytes) =>
            unchecked((short)((*bytes) | (*(bytes + 1) << 8)));

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static int GetUnsignedMedium(byte* bytes) =>
            *bytes << 16 |
            *(bytes + 1) << 8 |
            *(bytes + 2);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static int GetUnsignedMediumLE(byte* bytes) =>
            *bytes |
            *(bytes + 1) << 8 |
            *(bytes + 2) << 16;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static int GetInt(byte* bytes) =>
            (*bytes << 24) |
            (*(bytes + 1) << 16) |
            (*(bytes + 2) << 8) |
            (*(bytes + 3));

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static int GetIntLE(byte* bytes) =>
            *bytes |
            (*(bytes + 1) << 8) |
            (*(bytes + 2) << 16) |
            (*(bytes + 3) << 24);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static long GetLong(byte* bytes)
        {
            unchecked
            {
                int i1 = (*bytes << 24) | (*(bytes + 1) << 16) | (*(bytes + 2) << 8) | (*(bytes + 3));
                int i2 = (*(bytes + 4) << 24) | (*(bytes + 5) << 16) | (*(bytes + 6) << 8) | *(bytes + 7);
                return (uint)i2 | ((long)i1 << 32);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static long GetLongLE(byte* bytes)
        {
            unchecked
            {
                int i1 = *bytes | (*(bytes + 1) << 8) | (*(bytes + 2) << 16) | (*(bytes + 3) << 24);
                int i2 = *(bytes + 4) | (*(bytes + 5) << 8) | (*(bytes + 6) << 16) | (*(bytes + 7) << 24);
                return (uint)i1 | ((long)i2 << 32);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetShort(byte* bytes, int value)
        {
            unchecked
            {
                *bytes = (byte)((ushort)value >> 8);
                *(bytes + 1) = (byte)value;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetShortLE(byte* bytes, int value)
        {
            unchecked
            {
                *bytes = (byte)value;
                *(bytes + 1) = (byte)((ushort)value >> 8);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetMedium(byte* bytes, int value)
        {
            unchecked
            {
                uint unsignedValue = (uint)value;
                *bytes = (byte)(unsignedValue >> 16);
                *(bytes + 1) = (byte)(unsignedValue >> 8);
                *(bytes + 2) = (byte)unsignedValue;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetMediumLE(byte* bytes, int value)
        {
            unchecked
            {
                uint unsignedValue = (uint)value;
                *bytes = (byte)unsignedValue;
                *(bytes + 1) = (byte)(unsignedValue >> 8);
                *(bytes + 2) = (byte)(unsignedValue >> 16);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetInt(byte* bytes, int value)
        {
            unchecked
            {
                uint unsignedValue = (uint)value;
                *bytes = (byte)(unsignedValue >> 24);
                *(bytes + 1) = (byte)(unsignedValue >> 16);
                *(bytes + 2) = (byte)(unsignedValue >> 8);
                *(bytes + 3) = (byte)unsignedValue;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetIntLE(byte* bytes, int value)
        {
            unchecked
            {
                uint unsignedValue = (uint)value;
                *bytes = (byte)unsignedValue;
                *(bytes + 1) = (byte)(unsignedValue >> 8);
                *(bytes + 2) = (byte)(unsignedValue >> 16);
                *(bytes + 3) = (byte)(unsignedValue >> 24);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetLong(byte* bytes, long value)
        {
            unchecked
            {
                ulong unsignedValue = (ulong)value;
                *bytes = (byte)(unsignedValue >> 56);
                *(bytes + 1) = (byte)(unsignedValue >> 48);
                *(bytes + 2) = (byte)(unsignedValue >> 40);
                *(bytes + 3) = (byte)(unsignedValue >> 32);
                *(bytes + 4) = (byte)(unsignedValue >> 24);
                *(bytes + 5) = (byte)(unsignedValue >> 16);
                *(bytes + 6) = (byte)(unsignedValue >> 8);
                *(bytes + 7) = (byte)unsignedValue;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetLongLE(byte* bytes, long value)
        {
            unchecked
            {
                ulong unsignedValue = (ulong)value;
                *bytes = (byte)unsignedValue;
                *(bytes + 1) = (byte)(unsignedValue >> 8);
                *(bytes + 2) = (byte)(unsignedValue >> 16);
                *(bytes + 3) = (byte)(unsignedValue >> 24);
                *(bytes + 4) = (byte)(unsignedValue >> 32);
                *(bytes + 5) = (byte)(unsignedValue >> 40);
                *(bytes + 6) = (byte)(unsignedValue >> 48);
                *(bytes + 7) = (byte)(unsignedValue >> 56);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetZero(byte[] array, int index, int length)
        {
            //if (0u >= (uint)length)
            //{
            //    return;
            //}
            PlatformDependent.SetMemory(array, index, length, Zero);
        }

        internal static IByteBuffer Copy(AbstractByteBuffer buf, byte* addr, int index, int length)
        {
            IByteBuffer copy = buf.Allocator.DirectBuffer(length, buf.MaxCapacity);
            if (0u >= (uint)length) { return copy; }

            if (copy.HasMemoryAddress)
            {
                IntPtr ptr = copy.AddressOfPinnedMemory();
                if (ptr != IntPtr.Zero)
                {
                    PlatformDependent.CopyMemory(addr, (byte*)ptr, length);
                }
                else
                {
                    fixed (byte* dst = &copy.GetPinnableMemoryAddress())
                    {
                        PlatformDependent.CopyMemory(addr, dst, length);
                    }
                }
                _ = copy.SetIndex(0, length);
            }
            else
            {
                _ = copy.WriteBytes(buf, index, length);
            }
            return copy;
        }

        //internal static int SetBytes(AbstractByteBuffer buf, byte* addr, int index, Stream input, int length)
        //{
        //    IByteBuffer tmpBuf = buf.Allocator.HeapBuffer(length);
        //    try
        //    {
        //        int readTotal = 0;
        //        int readBytes;
        //        byte[] tmp = tmpBuf.Array;
        //        int offset = tmpBuf.ArrayOffset;
        //        do
        //        {
        //            readBytes = input.Read(tmp, offset + readTotal, length - readTotal);
        //            readTotal += readBytes;
        //        }
        //        while (readBytes > 0 && readTotal < length);

        //        //if (readTotal > 0)
        //        //{
        //        PlatformDependent.CopyMemory(tmp, offset, addr, readTotal);
        //        //}

        //        return readTotal;
        //    }
        //    finally
        //    {
        //        tmpBuf.Release();
        //    }
        //}

        internal static void GetBytes(AbstractByteBuffer buf, byte* addr, int index, IByteBuffer dst, int dstIndex, int length)
        {
            //if (dst is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst); }

            //if (MathUtil.IsOutOfBounds(dstIndex, length, dst.Capacity))
            //{
            //    ThrowHelper.ThrowIndexOutOfRangeException_DstIndex(dstIndex);
            //}
            if (0u >= (uint)length) { return; }

            if (dst.HasMemoryAddress)
            {
                IntPtr ptr = dst.AddressOfPinnedMemory();
                if (ptr != IntPtr.Zero)
                {
                    PlatformDependent.CopyMemory(addr, (byte*)(ptr + dstIndex), length);
                }
                else
                {
                    fixed (byte* destination = &dst.GetPinnableMemoryAddress())
                    {
                        PlatformDependent.CopyMemory(addr, destination + dstIndex, length);
                    }
                }
                return;
            }

            GetBytes0(buf, addr, index, dst, dstIndex, length);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void GetBytes0(AbstractByteBuffer buf, byte* addr, int index, IByteBuffer dst, int dstIndex, int length)
        {
            if (dst.HasArray)
            {
                PlatformDependent.CopyMemory(addr, dst.Array, dst.ArrayOffset + dstIndex, length);
            }
            else
            {
                _ = dst.SetBytes(dstIndex, buf, index, length);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void GetBytes(byte* addr, byte[] dst, int dstIndex, int length)
        {
            //if (dst is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst); }

            //if (MathUtil.IsOutOfBounds(dstIndex, length, dst.Length))
            //{
            //    ThrowHelper.ThrowIndexOutOfRangeException_DstIndex(dstIndex);
            //}
            //if (length != 0)
            //{
            PlatformDependent.CopyMemory(addr, dst, dstIndex, length);
            //}
        }

        internal static void SetBytes(AbstractByteBuffer buf, byte* addr, int index, IByteBuffer src, int srcIndex, int length)
        {
            //if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }

            //if (MathUtil.IsOutOfBounds(srcIndex, length, src.Capacity))
            //{
            //    ThrowHelper.ThrowIndexOutOfRangeException_SrcIndex(srcIndex);
            //}
            if (0u >= (uint)length) { return; }

            if (src.HasMemoryAddress)
            {
                IntPtr ptr = src.AddressOfPinnedMemory();
                if (ptr != IntPtr.Zero)
                {
                    PlatformDependent.CopyMemory((byte*)(ptr + srcIndex), addr, length);
                }
                else
                {
                    fixed (byte* source = &src.GetPinnableMemoryAddress())
                    {
                        PlatformDependent.CopyMemory(source + srcIndex, addr, length);
                    }
                }
                return;
            }

            SetBytes0(buf, addr, index, src, srcIndex, length);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void SetBytes0(AbstractByteBuffer buf, byte* addr, int index, IByteBuffer src, int srcIndex, int length)
        {
            if (src.HasArray)
            {
                PlatformDependent.CopyMemory(src.Array, src.ArrayOffset + srcIndex, addr, length);
            }
            else
            {
                _ = src.GetBytes(srcIndex, buf, index, length);
            }
        }

        // No need to check length zero, the calling method already done it
        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetBytes(byte* addr, byte[] src, int srcIndex, int length) =>
                PlatformDependent.CopyMemory(src, srcIndex, addr, length);

        internal static void GetBytes(AbstractByteBuffer buf, byte* addr, int index, Stream output, int length)
        {
            if (length != 0)
            {
                IByteBuffer tmpBuf = buf.Allocator.HeapBuffer(length);
                try
                {
                    byte[] tmp = tmpBuf.Array;
                    int offset = tmpBuf.ArrayOffset;
                    PlatformDependent.CopyMemory(addr, tmp, offset, length);
                    output.Write(tmp, offset, length);
                }
                finally
                {
                    _ = tmpBuf.Release();
                }
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static void SetZero(byte* addr, int length)
        {
            //if (0u >= (uint)length)
            //{
            //    return;
            //}
            PlatformDependent.SetMemory(addr, length, Zero);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static string GetString(byte* src, int length, Encoding encoding)
        {
#if NET451
            int charCount = encoding.GetCharCount(src, length);
            char* chars = stackalloc char[charCount];
            encoding.GetChars(src, length, chars, charCount);
            return new string(chars, 0, charCount);
#else
            return encoding.GetString(src, length);
#endif
        }

        internal static UnpooledUnsafeDirectByteBuffer NewUnsafeDirectByteBuffer(IByteBufferAllocator alloc, int initialCapacity, int maxCapacity) =>
            new UnpooledUnsafeDirectByteBuffer(alloc, initialCapacity, maxCapacity);
    }
}