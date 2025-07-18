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
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Common;
using DotNetty.Common.Internal;

namespace DotNetty.Buffers
{
    partial class ArrayPooledHeapByteBuffer : ArrayPooledByteBuffer
    {
        static readonly ThreadLocalPool<ArrayPooledHeapByteBuffer> Recycler = new ThreadLocalPool<ArrayPooledHeapByteBuffer>(handle => new ArrayPooledHeapByteBuffer(handle, 0));

        internal static ArrayPooledHeapByteBuffer NewInstance(ArrayPooledByteBufferAllocator allocator, ArrayPool<byte> arrayPool, byte[] buffer, int length, int maxCapacity)
        {
            var buf = Recycler.Take();
            buf.Reuse(allocator, arrayPool, buffer, length, maxCapacity);
            return buf;
        }

        internal ArrayPooledHeapByteBuffer(ThreadLocalPool.Handle recyclerHandle, int maxCapacity)
            : base(recyclerHandle, maxCapacity)
        {
        }

        public sealed override bool IsDirect => false;

        protected internal sealed override byte _GetByte(int index) => HeapByteBufferUtil.GetByte(Memory, index);

        protected internal sealed override short _GetShort(int index) => HeapByteBufferUtil.GetShort(Memory, index);

        protected internal sealed override short _GetShortLE(int index) => HeapByteBufferUtil.GetShortLE(Memory, index);

        protected internal sealed override int _GetUnsignedMedium(int index) => HeapByteBufferUtil.GetUnsignedMedium(Memory, index);

        protected internal sealed override int _GetUnsignedMediumLE(int index) => HeapByteBufferUtil.GetUnsignedMediumLE(Memory, index);

        protected internal sealed override int _GetInt(int index) => HeapByteBufferUtil.GetInt(Memory, index);

        protected internal sealed override int _GetIntLE(int index) => HeapByteBufferUtil.GetIntLE(Memory, index);

        protected internal sealed override long _GetLong(int index) => HeapByteBufferUtil.GetLong(Memory, index);

        protected internal sealed override long _GetLongLE(int index) => HeapByteBufferUtil.GetLongLE(Memory, index);

        public sealed override IByteBuffer GetBytes(int index, IByteBuffer dst, int dstIndex, int length)
        {
            if (dst is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst); }
            CheckDstIndex(index, length, dstIndex, dst.Capacity);
            if (dst.HasArray)
            {
                _ = GetBytes(index, dst.Array, dst.ArrayOffset + dstIndex, length);
            }
            else
            {
                _ = dst.SetBytes(dstIndex, Memory, index, length);
            }
            return this;
        }

        public sealed override IByteBuffer GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            if (dst is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst); }
            CheckDstIndex(index, length, dstIndex, dst.Length);
            PlatformDependent.CopyMemory(Memory, index, dst, dstIndex, length);
            return this;
        }

        public sealed override IByteBuffer GetBytes(int index, Stream destination, int length)
        {
            if (destination is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.destination); }
            CheckIndex(index, length);
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            destination.Write(new ReadOnlySpan<byte>(Memory, index, length));
#else
            destination.Write(Memory, index, length);
#endif
            return this;
        }

        protected internal sealed override void _SetByte(int index, int value) => HeapByteBufferUtil.SetByte(Memory, index, value);

        protected internal sealed override void _SetShort(int index, int value) => HeapByteBufferUtil.SetShort(Memory, index, value);

        protected internal sealed override void _SetShortLE(int index, int value) => HeapByteBufferUtil.SetShortLE(Memory, index, value);

        protected internal sealed override void _SetMedium(int index, int value) => HeapByteBufferUtil.SetMedium(Memory, index, value);

        protected internal sealed override void _SetMediumLE(int index, int value) => HeapByteBufferUtil.SetMediumLE(Memory, index, value);

        protected internal sealed override void _SetInt(int index, int value) => HeapByteBufferUtil.SetInt(Memory, index, value);

        protected internal sealed override void _SetIntLE(int index, int value) => HeapByteBufferUtil.SetIntLE(Memory, index, value);

        protected internal sealed override void _SetLong(int index, long value) => HeapByteBufferUtil.SetLong(Memory, index, value);

        protected internal sealed override void _SetLongLE(int index, long value) => HeapByteBufferUtil.SetLongLE(Memory, index, value);

        public sealed override IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }
            CheckSrcIndex(index, length, srcIndex, src.Capacity);
            if (src.HasArray)
            {
                _ = SetBytes(index, src.Array, src.ArrayOffset + srcIndex, length);
            }
            else
            {
                _ = src.GetBytes(srcIndex, Memory, index, length);
            }
            return this;
        }

        public sealed override Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }
            CheckIndex(index, length);

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

        public sealed override IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }
            CheckSrcIndex(index, length, srcIndex, src.Length);
            PlatformDependent.CopyMemory(src, srcIndex, Memory, index, length);
            return this;
        }

        public sealed override IByteBuffer Copy(int index, int length)
        {
            CheckIndex(index, length);
            IByteBuffer copy = Allocator.HeapBuffer(length, MaxCapacity);
            _ = copy.WriteBytes(Memory, index, length);
            return copy;
        }


        public sealed override IByteBuffer SetZero(int index, int length)
        {
            CheckIndex(index, length);
            PlatformDependent.Clear(Memory, index, length);
            return this;
        }

        public sealed override IByteBuffer WriteZero(int length)
        {
            if (0u >= (uint)length) { return this; }

            _ = EnsureWritable(length);
            int wIndex = WriterIndex;
            CheckIndex0(wIndex, length);
            PlatformDependent.Clear(Memory, wIndex, length);
            _ = SetWriterIndex(wIndex + length);

            return this;
        }
    }
}