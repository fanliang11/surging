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
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Internal;
#if NET
    using System.Runtime.InteropServices;
#endif

    partial class UnpooledHeapByteBuffer : AbstractReferenceCountedByteBuffer
    {
        private readonly IByteBufferAllocator _allocator;
        private byte[] _array;

        protected internal UnpooledHeapByteBuffer(IByteBufferAllocator alloc, int initialCapacity, int maxCapacity)
            : base(maxCapacity)
        {
            if (alloc is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.alloc); }
            if (initialCapacity > maxCapacity) { ThrowHelper.ThrowArgumentException_InitialCapacityMaxCapacity(initialCapacity, maxCapacity); }

            _allocator = alloc;
            SetArray(NewArray(initialCapacity));
            SetIndex0(0, 0);
        }

        protected internal UnpooledHeapByteBuffer(IByteBufferAllocator alloc, byte[] initialArray, int maxCapacity)
            : base(maxCapacity)
        {
            if (alloc is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.alloc); }
            if (initialArray is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.initialArray); }

            if (initialArray.Length > maxCapacity)
            {
                ThrowHelper.ThrowArgumentException_InitialCapacity(initialArray.Length, maxCapacity);
            }

            _allocator = alloc;
            SetArray(initialArray);
            SetIndex0(0, initialArray.Length);
        }

        protected virtual byte[] AllocateArray(int initialCapacity) => NewArray(initialCapacity);

        protected byte[] NewArray(int initialCapacity) => new byte[initialCapacity];

        protected virtual void FreeArray(byte[] bytes)
        {
            // NOOP
        }

        protected void SetArray(byte[] initialArray) => _array = initialArray;

        public sealed override IByteBufferAllocator Allocator => _allocator;

        public sealed override bool IsDirect => false;

        public sealed override int Capacity
        {
            [System.Runtime.CompilerServices.MethodImpl(InlineMethod.AggressiveInlining)]
            get
            {
                return _array.Length;
            }
        }

        public sealed override IByteBuffer AdjustCapacity(int newCapacity)
        {
            CheckNewCapacity(newCapacity);

            uint unewCapacity = (uint)newCapacity;
            byte[] oldArray = _array;
            uint oldCapacity = (uint)oldArray.Length;
            if (oldCapacity == unewCapacity)
            {
                return this;
            }

            int bytesToCopy;
            if (unewCapacity > oldCapacity)
            {
                bytesToCopy = oldArray.Length;
            }
            else
            {
                TrimIndicesToCapacity(newCapacity);
                bytesToCopy = newCapacity;
            }
            byte[] newArray = AllocateArray(newCapacity);
            PlatformDependent.CopyMemory(oldArray, 0, newArray, 0, bytesToCopy);
            SetArray(newArray);
            FreeArray(oldArray);
            return this;
        }

        public sealed override bool HasArray => true;

        public sealed override byte[] Array
        {
            get
            {
                EnsureAccessible();
                return _array;
            }
        }

        public sealed override int ArrayOffset => 0;

        public sealed override bool HasMemoryAddress => true;

        public sealed override ref byte GetPinnableMemoryAddress()
        {
            EnsureAccessible();
#if NET
            return ref MemoryMarshal.GetArrayDataReference(_array);
#else
            return ref _array[0];
#endif
        }

        public sealed override IntPtr AddressOfPinnedMemory() => IntPtr.Zero;

        public sealed override bool IsContiguous => true;

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
                _ = dst.SetBytes(dstIndex, _array, index, length);
            }

            return this;
        }

        public sealed override IByteBuffer GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            if (dst is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst); }
            CheckDstIndex(index, length, dstIndex, dst.Length);
            PlatformDependent.CopyMemory(_array, index, dst, dstIndex, length);
            return this;
        }

        public sealed override IByteBuffer GetBytes(int index, Stream destination, int length)
        {
            if (destination is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.destination); }
            CheckIndex(index, length);
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            destination.Write(new ReadOnlySpan<byte>(_array, index, length));
#else
            destination.Write(_array, index, length);
#endif
            return this;
        }

        public override IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }
            CheckSrcIndex(index, length, srcIndex, src.Capacity);
            if (src.HasArray)
            {
                _ = SetBytes(index, src.Array, src.ArrayOffset + srcIndex, length);
            }
            else
            {
                _ = src.GetBytes(srcIndex, _array, index, length);
            }
            return this;
        }

        public sealed override IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }
            CheckSrcIndex(index, length, srcIndex, src.Length);
            PlatformDependent.CopyMemory(src, srcIndex, _array, index, length);
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
                read = src.Read(new Span<byte>(_array, index + readTotal, length - readTotal));
#else
                read = src.Read(_array, index + readTotal, length - readTotal);
#endif
                readTotal += read;
            }
            while (read > 0 && readTotal < length);

            return Task.FromResult(readTotal);
        }

        public sealed override bool IsSingleIoBuffer => true;

        public sealed override int IoBufferCount => 1;

        public sealed override ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            CheckIndex(index, length);
            return new ArraySegment<byte>(_array, index, length);
        }

        public sealed override ArraySegment<byte>[] GetIoBuffers(int index, int length) => new[] { GetIoBuffer(index, length) };

        protected internal sealed override byte _GetByte(int index) => HeapByteBufferUtil.GetByte(_array, index);

        public sealed override IByteBuffer SetZero(int index, int length)
        {
            CheckIndex(index, length);
            PlatformDependent.Clear(_array, index, length);
            return this;
        }

        protected internal sealed override short _GetShort(int index) => HeapByteBufferUtil.GetShort(_array, index);

        protected internal sealed override short _GetShortLE(int index) => HeapByteBufferUtil.GetShortLE(_array, index);

        protected internal sealed override int _GetUnsignedMedium(int index) => HeapByteBufferUtil.GetUnsignedMedium(_array, index);

        protected internal sealed override int _GetUnsignedMediumLE(int index) => HeapByteBufferUtil.GetUnsignedMediumLE(_array, index);

        protected internal sealed override int _GetInt(int index) => HeapByteBufferUtil.GetInt(_array, index);

        protected internal sealed override int _GetIntLE(int index) => HeapByteBufferUtil.GetIntLE(_array, index);

        protected internal sealed override long _GetLong(int index) => HeapByteBufferUtil.GetLong(_array, index);

        protected internal sealed override long _GetLongLE(int index) => HeapByteBufferUtil.GetLongLE(_array, index);

        protected internal sealed override void _SetByte(int index, int value) => HeapByteBufferUtil.SetByte(_array, index, value);

        protected internal sealed override void _SetShort(int index, int value) => HeapByteBufferUtil.SetShort(_array, index, value);

        protected internal sealed override void _SetShortLE(int index, int value) => HeapByteBufferUtil.SetShortLE(_array, index, value);

        protected internal sealed override void _SetMedium(int index, int value) => HeapByteBufferUtil.SetMedium(_array, index, value);

        protected internal sealed override void _SetMediumLE(int index, int value) => HeapByteBufferUtil.SetMediumLE(_array, index, value);

        protected internal sealed override void _SetInt(int index, int value) => HeapByteBufferUtil.SetInt(_array, index, value);

        protected internal sealed override void _SetIntLE(int index, int value) => HeapByteBufferUtil.SetIntLE(_array, index, value);

        protected internal sealed override void _SetLong(int index, long value) => HeapByteBufferUtil.SetLong(_array, index, value);

        protected internal sealed override void _SetLongLE(int index, long value) => HeapByteBufferUtil.SetLongLE(_array, index, value);

        public sealed override IByteBuffer Copy(int index, int length)
        {
            CheckIndex(index, length);
            return _allocator.HeapBuffer(length, MaxCapacity).WriteBytes(_array, index, length);
        }

        protected internal sealed override void Deallocate()
        {
            FreeArray(_array);
            _array = EmptyArrays.EmptyBytes;
        }

        public sealed override IByteBuffer Unwrap() => null;

        public sealed override IByteBuffer WriteZero(int length)
        {
            if (0u >= (uint)length) { return this; }

            _ = EnsureWritable(length);
            int wIndex = WriterIndex;
            CheckIndex0(wIndex, length);
            PlatformDependent.Clear(_array, wIndex, length);
            _ = SetWriterIndex(wIndex + length);

            return this;
        }
    }
}