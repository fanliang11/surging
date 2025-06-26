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

using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Common;
#if NET
using System.Runtime.InteropServices;
#endif

namespace DotNetty.Buffers
{
    unsafe partial class ArrayPooledUnsafeDirectByteBuffer : ArrayPooledByteBuffer
    {
        static readonly ThreadLocalPool<ArrayPooledUnsafeDirectByteBuffer> Recycler = new ThreadLocalPool<ArrayPooledUnsafeDirectByteBuffer>(handle => new ArrayPooledUnsafeDirectByteBuffer(handle, 0));

        internal static ArrayPooledUnsafeDirectByteBuffer NewInstance(ArrayPooledByteBufferAllocator allocator, ArrayPool<byte> arrayPool, byte[] buffer, int length, int maxCapacity)
        {
            var buf = Recycler.Take();
            buf.Reuse(allocator, arrayPool, buffer, length, maxCapacity);
            return buf;
        }

        internal ArrayPooledUnsafeDirectByteBuffer(ThreadLocalPool.Handle recyclerHandle, int maxCapacity)
            : base(recyclerHandle, maxCapacity)
        {
        }

        public sealed override bool IsDirect => true;

        protected internal sealed override byte _GetByte(int index) => Memory[index];

        protected internal sealed override void _SetByte(int index, int value) => Memory[index] = unchecked((byte)value);

        protected internal sealed override short _GetShort(int index)
        {
            fixed (byte* addr = &Addr(index))
                return UnsafeByteBufferUtil.GetShort(addr);
        }

        protected internal sealed override short _GetShortLE(int index)
        {
            fixed (byte* addr = &Addr(index))
                return UnsafeByteBufferUtil.GetShortLE(addr);
        }

        protected internal sealed override int _GetUnsignedMedium(int index)
        {
            fixed (byte* addr = &Addr(index))
                return UnsafeByteBufferUtil.GetUnsignedMedium(addr);
        }

        protected internal sealed override int _GetUnsignedMediumLE(int index)
        {
            fixed (byte* addr = &Addr(index))
                return UnsafeByteBufferUtil.GetUnsignedMediumLE(addr);
        }

        protected internal sealed override int _GetInt(int index)
        {
            fixed (byte* addr = &Addr(index))
                return UnsafeByteBufferUtil.GetInt(addr);
        }

        protected internal sealed override int _GetIntLE(int index)
        {
            fixed (byte* addr = &Addr(index))
                return UnsafeByteBufferUtil.GetIntLE(addr);
        }

        protected internal sealed override long _GetLong(int index)
        {
            fixed (byte* addr = &Addr(index))
                return UnsafeByteBufferUtil.GetLong(addr);
        }

        protected internal sealed override long _GetLongLE(int index)
        {
            fixed (byte* addr = &Addr(index))
                return UnsafeByteBufferUtil.GetLongLE(addr);
        }

        public sealed override IByteBuffer GetBytes(int index, IByteBuffer dst, int dstIndex, int length)
        {
            if (dst is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst); }
            CheckDstIndex(index, length, dstIndex, dst.Capacity);
            fixed (byte* addr = &Addr(index))
            {
                UnsafeByteBufferUtil.GetBytes(this, addr, index, dst, dstIndex, length);
                return this;
            }
        }

        public sealed override IByteBuffer GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            if (dst is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst); }
            CheckDstIndex(index, length, dstIndex, dst.Length);
            fixed (byte* addr = &Addr(index))
            {
                UnsafeByteBufferUtil.GetBytes(addr, dst, dstIndex, length);
                return this;
            }
        }

        protected internal sealed override void _SetShort(int index, int value)
        {
            fixed (byte* addr = &Addr(index))
                UnsafeByteBufferUtil.SetShort(addr, value);
        }

        protected internal sealed override void _SetShortLE(int index, int value)
        {
            fixed (byte* addr = &Addr(index))
                UnsafeByteBufferUtil.SetShortLE(addr, value);
        }

        protected internal sealed override void _SetMedium(int index, int value)
        {
            fixed (byte* addr = &Addr(index))
                UnsafeByteBufferUtil.SetMedium(addr, value);
        }

        protected internal sealed override void _SetMediumLE(int index, int value)
        {
            fixed (byte* addr = &Addr(index))
                UnsafeByteBufferUtil.SetMediumLE(addr, value);
        }

        protected internal sealed override void _SetInt(int index, int value)
        {
            fixed (byte* addr = &Addr(index))
                UnsafeByteBufferUtil.SetInt(addr, value);
        }

        protected internal sealed override void _SetIntLE(int index, int value)
        {
            fixed (byte* addr = &Addr(index))
                UnsafeByteBufferUtil.SetIntLE(addr, value);
        }

        protected internal sealed override void _SetLong(int index, long value)
        {
            fixed (byte* addr = &Addr(index))
                UnsafeByteBufferUtil.SetLong(addr, value);
        }

        protected internal sealed override void _SetLongLE(int index, long value)
        {
            fixed (byte* addr = &Addr(index))
                UnsafeByteBufferUtil.SetLongLE(addr, value);
        }

        public sealed override IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }
            CheckSrcIndex(index, length, srcIndex, src.Capacity);
            if (0u >= (uint)length) { return this; }

            fixed (byte* addr = &Addr(index))
            {
                UnsafeByteBufferUtil.SetBytes(this, addr, index, src, srcIndex, length);
                return this;
            }
        }

        public sealed override IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }
            CheckSrcIndex(index, length, srcIndex, src.Length);
            if (0u >= (uint)length) { return this; }

            fixed (byte* addr = &Addr(index))
            {
                UnsafeByteBufferUtil.SetBytes(addr, src, srcIndex, length);
                return this;
            }
        }

        public sealed override IByteBuffer GetBytes(int index, Stream output, int length)
        {
            if (output is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.output); }
            CheckIndex(index, length);
            //fixed (byte* addr = &Addr(index))
            //{
            //    UnsafeByteBufferUtil.GetBytes(this, addr, index, output, length);
            //    return this;
            //}
            // UnsafeByteBufferUtil.GetBytes 多一遍内存拷贝，最终还是调用 stream.write，没啥必要
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            output.Write(new ReadOnlySpan<byte>(Memory, index, length));
#else
            output.Write(Memory, index, length);
#endif
            return this;
        }

        public sealed override Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }
            CheckIndex(index, length);
            //int read;
            //fixed (byte* addr = &Addr(index))
            //{
            //    read = UnsafeByteBufferUtil.SetBytes(this, addr, index, src, length);
            //    return Task.FromResult(read);
            //}
            int readTotal = 0;
            int read;
            do
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                read = src.Read(new Span<byte>(Memory, index + readTotal, length - readTotal));
#else
                read = src.Read(Memory, index + readTotal, length - readTotal);
#endif
                readTotal += read;
            }
            while (read > 0 && readTotal < length);

            return Task.FromResult(readTotal);
        }

        public sealed override IByteBuffer Copy(int index, int length)
        {
            CheckIndex(index, length);
            fixed (byte* addr = &Addr(index))
                return UnsafeByteBufferUtil.Copy(this, addr, index, length);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        ref byte Addr(int index)
        {
#if NET
            return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(Memory), index);
#else
            return ref Memory[index];
#endif
        }

        public sealed override IByteBuffer SetZero(int index, int length)
        {
            CheckIndex(index, length);
            fixed (byte* addr = &Addr(index))
            {
                UnsafeByteBufferUtil.SetZero(addr, length);
                return this;
            }
        }

        public sealed override IByteBuffer WriteZero(int length)
        {
            if (0u >= (uint)length) { return this; }

            _ = EnsureWritable(length);
            int wIndex = WriterIndex;
            CheckIndex0(wIndex, length);
            fixed (byte* addr = &Addr(wIndex))
            {
                UnsafeByteBufferUtil.SetZero(addr, length);
            }
            _ = SetWriterIndex(wIndex + length);

            return this;
        }
    }
}
