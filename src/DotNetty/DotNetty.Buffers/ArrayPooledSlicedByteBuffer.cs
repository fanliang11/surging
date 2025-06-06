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

namespace DotNetty.Buffers
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    sealed partial class ArrayPooledSlicedByteBuffer : AbstractArrayPooledDerivedByteBuffer
    {
        static readonly ThreadLocalPool<ArrayPooledSlicedByteBuffer> Recycler = new ThreadLocalPool<ArrayPooledSlicedByteBuffer>(handle => new ArrayPooledSlicedByteBuffer(handle));

        internal static ArrayPooledSlicedByteBuffer NewInstance(AbstractByteBuffer unwrapped, IByteBuffer wrapped, int index, int length)
        {
            AbstractUnpooledSlicedByteBuffer.CheckSliceOutOfBounds(index, length, unwrapped);
            return NewInstance0(unwrapped, wrapped, index, length);
        }

        static ArrayPooledSlicedByteBuffer NewInstance0(AbstractByteBuffer unwrapped, IByteBuffer wrapped, int adjustment, int length)
        {
            ArrayPooledSlicedByteBuffer slice = Recycler.Take();
            _ = slice.Init<ArrayPooledSlicedByteBuffer>(unwrapped, wrapped, 0, length, length);
            slice.DiscardMarks();
            slice.adjustment = adjustment;

            return slice;
        }

        internal int adjustment;

        ArrayPooledSlicedByteBuffer(ThreadLocalPool.Handle handle)
            : base(handle)
        {
        }

        public sealed override int Capacity => MaxCapacity;

        public sealed override IByteBuffer AdjustCapacity(int newCapacity) => throw new NotSupportedException("sliced buffer");

        public sealed override int ArrayOffset => Idx(Unwrap().ArrayOffset);

        public sealed override ref byte GetPinnableMemoryAddress() => ref Unsafe.Add(ref Unwrap().GetPinnableMemoryAddress(), adjustment);

        public sealed override IntPtr AddressOfPinnedMemory()
        {
            IntPtr ptr = Unwrap().AddressOfPinnedMemory();
            if (ptr == IntPtr.Zero)
            {
                return ptr;
            }
            return ptr + adjustment;
        }

        public sealed override ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            CheckIndex0(index, length);
            return Unwrap().GetIoBuffer(Idx(index), length);
        }

        public sealed override ArraySegment<byte>[] GetIoBuffers(int index, int length)
        {
            CheckIndex0(index, length);
            return Unwrap().GetIoBuffers(Idx(index), length);
        }

        public sealed override IByteBuffer Copy(int index, int length)
        {
            CheckIndex0(index, length);
            return Unwrap().Copy(Idx(index), length);
        }

        public sealed override IByteBuffer Slice(int index, int length)
        {
            CheckIndex0(index, length);
            return base.Slice(Idx(index), length);
        }

        public sealed override IByteBuffer RetainedSlice(int index, int length)
        {
            CheckIndex0(index, length);
            return NewInstance0(UnwrapCore(), this, Idx(index), length);
        }

        public sealed override IByteBuffer Duplicate() => Duplicate0().SetIndex(Idx(ReaderIndex), Idx(WriterIndex));

        public sealed override IByteBuffer RetainedDuplicate() => ArrayPooledDuplicatedByteBuffer.NewInstance(UnwrapCore(), this, Idx(ReaderIndex), Idx(WriterIndex));

        public sealed override byte GetByte(int index)
        {
            CheckIndex0(index, 1);
            return Unwrap().GetByte(Idx(index));
        }

        protected internal sealed override byte _GetByte(int index) => UnwrapCore()._GetByte(Idx(index));

        public sealed override short GetShort(int index)
        {
            CheckIndex0(index, 2);
            return Unwrap().GetShort(Idx(index));
        }

        protected internal sealed override short _GetShort(int index) => UnwrapCore()._GetShort(Idx(index));

        public sealed override short GetShortLE(int index)
        {
            CheckIndex0(index, 2);
            return Unwrap().GetShortLE(Idx(index));
        }

        protected internal sealed override short _GetShortLE(int index) => UnwrapCore()._GetShortLE(Idx(index));

        public sealed override int GetUnsignedMedium(int index)
        {
            CheckIndex0(index, 3);
            return Unwrap().GetUnsignedMedium(Idx(index));
        }

        protected internal sealed override int _GetUnsignedMedium(int index) => UnwrapCore()._GetUnsignedMedium(Idx(index));

        public sealed override int GetUnsignedMediumLE(int index)
        {
            CheckIndex0(index, 3);
            return Unwrap().GetUnsignedMediumLE(Idx(index));
        }

        protected internal sealed override int _GetUnsignedMediumLE(int index) => UnwrapCore()._GetUnsignedMediumLE(Idx(index));

        public sealed override int GetInt(int index)
        {
            CheckIndex0(index, 4);
            return Unwrap().GetInt(Idx(index));
        }

        protected internal sealed override int _GetInt(int index) => UnwrapCore()._GetInt(Idx(index));

        public sealed override int GetIntLE(int index)
        {
            CheckIndex0(index, 4);
            return Unwrap().GetIntLE(Idx(index));
        }

        protected internal sealed override int _GetIntLE(int index) => UnwrapCore()._GetIntLE(Idx(index));

        public sealed override long GetLong(int index)
        {
            CheckIndex0(index, 8);
            return Unwrap().GetLong(Idx(index));
        }

        protected internal sealed override long _GetLong(int index) => UnwrapCore()._GetLong(Idx(index));

        public sealed override long GetLongLE(int index)
        {
            CheckIndex0(index, 8);
            return Unwrap().GetLongLE(Idx(index));
        }

        protected internal sealed override long _GetLongLE(int index) => UnwrapCore()._GetLongLE(Idx(index));

        public sealed override IByteBuffer GetBytes(int index, IByteBuffer dst, int dstIndex, int length)
        {
            CheckIndex0(index, length);
            _ = Unwrap().GetBytes(Idx(index), dst, dstIndex, length);
            return this;
        }

        public sealed override IByteBuffer GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            CheckIndex0(index, length);
            _ = Unwrap().GetBytes(Idx(index), dst, dstIndex, length);
            return this;
        }

        public sealed override IByteBuffer SetByte(int index, int value)
        {
            CheckIndex0(index, 1);
            _ = Unwrap().SetByte(Idx(index), value);
            return this;
        }

        protected internal sealed override void _SetByte(int index, int value) => UnwrapCore()._SetByte(Idx(index), value);

        public sealed override IByteBuffer SetShort(int index, int value)
        {
            CheckIndex0(index, 2);
            _ = Unwrap().SetShort(Idx(index), value);
            return this;
        }

        protected internal sealed override void _SetShort(int index, int value) => UnwrapCore()._SetShort(Idx(index), value);

        public sealed override IByteBuffer SetShortLE(int index, int value)
        {
            CheckIndex0(index, 2);
            _ = Unwrap().SetShortLE(Idx(index), value);
            return this;
        }

        protected internal sealed override void _SetShortLE(int index, int value) => UnwrapCore()._SetShortLE(Idx(index), value);

        public sealed override IByteBuffer SetMedium(int index, int value)
        {
            CheckIndex0(index, 3);
            _ = Unwrap().SetMedium(Idx(index), value);
            return this;
        }

        protected internal sealed override void _SetMedium(int index, int value) => UnwrapCore()._SetMedium(Idx(index), value);

        public sealed override IByteBuffer SetMediumLE(int index, int value)
        {
            CheckIndex0(index, 3);
            _ = Unwrap().SetMediumLE(Idx(index), value);
            return this;
        }

        protected internal sealed override void _SetMediumLE(int index, int value) => UnwrapCore()._SetMediumLE(Idx(index), value);

        public sealed override IByteBuffer SetInt(int index, int value)
        {
            CheckIndex0(index, 4);
            _ = Unwrap().SetInt(Idx(index), value);
            return this;
        }

        protected internal sealed override void _SetInt(int index, int value) => UnwrapCore()._SetInt(Idx(index), value);

        public sealed override IByteBuffer SetIntLE(int index, int value)
        {
            CheckIndex0(index, 4);
            _ = Unwrap().SetIntLE(Idx(index), value);
            return this;
        }

        protected internal sealed override void _SetIntLE(int index, int value) => UnwrapCore()._SetIntLE(Idx(index), value);

        public sealed override IByteBuffer SetLong(int index, long value)
        {
            CheckIndex0(index, 8);
            _ = Unwrap().SetLong(Idx(index), value);
            return this;
        }

        protected internal sealed override void _SetLong(int index, long value) => UnwrapCore()._SetLong(Idx(index), value);

        public sealed override IByteBuffer SetLongLE(int index, long value)
        {
            CheckIndex0(index, 8);
            _ = Unwrap().SetLongLE(Idx(index), value);
            return this;
        }

        protected internal sealed override void _SetLongLE(int index, long value) => UnwrapCore()._SetLongLE(Idx(index), value);

        public sealed override IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            CheckIndex0(index, length);
            _ = Unwrap().SetBytes(Idx(index), src, srcIndex, length);
            return this;
        }

        public sealed override IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length)
        {
            CheckIndex0(index, length);
            _ = Unwrap().SetBytes(Idx(index), src, srcIndex, length);
            return this;
        }

        public sealed override IByteBuffer GetBytes(int index, Stream destination, int length)
        {
            CheckIndex0(index, length);
            return Unwrap().GetBytes(Idx(index), destination, length);
        }

        public sealed override Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken)
        {
            CheckIndex0(index, length);
            return Unwrap().SetBytesAsync(Idx(index), src, length, cancellationToken);
        }

        public sealed override int ForEachByte(int index, int length, IByteProcessor processor)
        {
            CheckIndex0(index, length);
            int ret = Unwrap().ForEachByte(Idx(index), length, processor);
            if (ret < adjustment)
            {
                return IndexNotFound;
            }
            return ret - adjustment;
        }

        public sealed override int ForEachByteDesc(int index, int length, IByteProcessor processor)
        {
            CheckIndex0(index, length);
            int ret = Unwrap().ForEachByteDesc(Idx(index), length, processor);
            if (ret < adjustment)
            {
                return IndexNotFound;
            }
            return ret - adjustment;
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        int Idx(int index) => index + adjustment;
    }
}
