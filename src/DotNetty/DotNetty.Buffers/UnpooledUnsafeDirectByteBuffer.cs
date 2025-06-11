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
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Internal;
#if NET
    using System.Runtime.InteropServices;
#endif

    unsafe partial class UnpooledUnsafeDirectByteBuffer : AbstractReferenceCountedByteBuffer
    {
        private readonly IByteBufferAllocator _allocator;

        private int _capacity;
        private bool _doNotFree;
        private byte[] _buffer;

        public UnpooledUnsafeDirectByteBuffer(IByteBufferAllocator alloc, int initialCapacity, int maxCapacity)
            : base(maxCapacity)
        {
            if (alloc is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.alloc); }
            if ((uint)initialCapacity > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(initialCapacity, ExceptionArgument.initialCapacity); }
            //if ((uint)maxCapacity > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(maxCapacity, ExceptionArgument.maxCapacity); }

            if ((uint)initialCapacity > (uint)maxCapacity)
            {
                ThrowHelper.ThrowArgumentException_InitialCapacity(initialCapacity, maxCapacity);
            }

            _allocator = alloc;
            SetByteBuffer(NewArray(initialCapacity), false);
        }

        protected UnpooledUnsafeDirectByteBuffer(IByteBufferAllocator alloc, byte[] initialBuffer, int maxCapacity, bool doFree)
            : base(maxCapacity)
        {
            if (alloc is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.alloc); }
            if (initialBuffer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.initialBuffer); }

            int initialCapacity = initialBuffer.Length;
            if ((uint)initialCapacity > (uint)maxCapacity)
            {
                ThrowHelper.ThrowArgumentException_InitialCapacity(initialCapacity, maxCapacity);
            }

            _allocator = alloc;
            _doNotFree = !doFree;
            SetByteBuffer(initialBuffer, false);
        }

        protected virtual byte[] AllocateDirect(int initialCapacity) => NewArray(initialCapacity);

        protected byte[] NewArray(int initialCapacity) => new byte[initialCapacity];

        protected virtual void FreeDirect(byte[] array)
        {
            // NOOP rely on GC.
        }

        void SetByteBuffer(byte[] array, bool tryFree)
        {
            if (tryFree)
            {
                byte[] oldBuffer = _buffer;
                if (oldBuffer is object)
                {
                    if (_doNotFree)
                    {
                        _doNotFree = false;
                    }
                    else
                    {
                        FreeDirect(oldBuffer);
                    }
                }
            }
            _buffer = array;
            _capacity = array.Length;
        }

        public sealed override bool IsDirect => true;

        public sealed override int Capacity => _capacity;

        public sealed override IByteBuffer AdjustCapacity(int newCapacity)
        {
            CheckNewCapacity(newCapacity);

            uint unewCapacity = (uint)newCapacity;
            uint oldCapacity = (uint)_capacity;
            if (oldCapacity == unewCapacity)
            {
                return this;
            }
            int bytesToCopy;
            if (unewCapacity > oldCapacity)
            {
                bytesToCopy = _capacity;
            }
            else
            {
                TrimIndicesToCapacity(newCapacity);
                bytesToCopy = newCapacity;
            }
            byte[] oldBuffer = _buffer;
            byte[] newBuffer = AllocateDirect(newCapacity);
            PlatformDependent.CopyMemory(oldBuffer, 0, newBuffer, 0, bytesToCopy);
            SetByteBuffer(newBuffer, true);
            return this;
        }

        public sealed override IByteBufferAllocator Allocator => _allocator;

        public sealed override bool HasArray => true;

        public sealed override byte[] Array
        {
            get
            {
                EnsureAccessible();
                return _buffer;
            }
        }

        public sealed override int ArrayOffset => 0;

        public sealed override bool IsSingleIoBuffer => true;

        public sealed override int IoBufferCount => 1;

        public sealed override ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            CheckIndex(index, length);
            return new ArraySegment<byte>(_buffer, index, length);
        }

        public sealed override ArraySegment<byte>[] GetIoBuffers(int index, int length) => new[] { GetIoBuffer(index, length) };

        protected internal sealed override void Deallocate()
        {
            byte[] buf = _buffer;
            if (buf is null)
            {
                return;
            }

            _buffer = null;

            if (!_doNotFree)
            {
                FreeDirect(buf);
            }
        }

        public sealed override IByteBuffer Unwrap() => null;

        protected internal sealed override byte _GetByte(int index) => _buffer[index];

        protected internal sealed override void _SetByte(int index, int value) => _buffer[index] = unchecked((byte)value);

        public sealed override IntPtr AddressOfPinnedMemory() => IntPtr.Zero;

        public sealed override bool HasMemoryAddress => true;

        public sealed override ref byte GetPinnableMemoryAddress()
        {
            EnsureAccessible();
#if NET
            return ref MemoryMarshal.GetArrayDataReference(_buffer);
#else
            return ref _buffer[0];
#endif
        }

        public sealed override bool IsContiguous => true;

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
            output.Write(new ReadOnlySpan<byte>(_buffer, index, length));
#else
            output.Write(_buffer, index, length);
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

            //    // See https://github.com/Azure/DotNetty/issues/436
            //    return Task.FromResult(read);
            //}
            int readTotal = 0;
            int read;
            do
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                read = src.Read(new Span<byte>(_buffer, index + readTotal, length - readTotal));
#else
                read = src.Read(_buffer, index + readTotal, length - readTotal);
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
            return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), index);
#else
            return ref _buffer[index];
#endif
        }

        public sealed override IByteBuffer SetZero(int index, int length)
        {
            CheckIndex(index, length);
            fixed (byte* addr = &Addr(index))
                UnsafeByteBufferUtil.SetZero(addr, length);
            return this;
        }

        public sealed override IByteBuffer WriteZero(int length)
        {
            if (0u >= (uint)length) { return this; }

            _ = EnsureWritable(length);
            int wIndex = WriterIndex;
            CheckIndex0(wIndex, length);
            fixed (byte* addr = &Addr(wIndex))
                UnsafeByteBufferUtil.SetZero(addr, length);
            _ = SetWriterIndex(wIndex + length);

            return this;
        }
    }
}