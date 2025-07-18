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
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    abstract partial class AbstractUnpooledSlicedByteBuffer : AbstractDerivedByteBuffer
    {
        private readonly AbstractByteBuffer _buffer;
        private readonly int _adjustment;

        protected AbstractUnpooledSlicedByteBuffer(IByteBuffer buffer, int index, int length)
            : base(length)
        {
            CheckSliceOutOfBounds(index, length, buffer);

            switch (buffer)
            {
                case AbstractUnpooledSlicedByteBuffer byteBuffer:
                    _buffer = byteBuffer._buffer;
                    _adjustment = byteBuffer._adjustment + index;
                    break;

                case UnpooledDuplicatedByteBuffer _:
                    _buffer = (AbstractByteBuffer)buffer.Unwrap();
                    _adjustment = index;
                    break;

                default:
                    _buffer = (AbstractByteBuffer)buffer;
                    _adjustment = index;
                    break;
            }

            SetWriterIndex0(length);
        }

        internal int Length => Capacity;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public override IByteBuffer Unwrap() => _buffer;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        protected AbstractByteBuffer UnwrapCore() => _buffer;

        public override IByteBufferAllocator Allocator => Unwrap().Allocator;

        public override bool IsDirect => Unwrap().IsDirect;

        public override IByteBuffer AdjustCapacity(int newCapacity) => throw new NotSupportedException("sliced buffer");

        public override bool HasArray => Unwrap().HasArray;

        public override byte[] Array => Unwrap().Array;

        public override int ArrayOffset => Idx(Unwrap().ArrayOffset);

        public override bool HasMemoryAddress => Unwrap().HasMemoryAddress;

        public override ref byte GetPinnableMemoryAddress() => ref Unsafe.Add(ref Unwrap().GetPinnableMemoryAddress(), _adjustment);

        public override IntPtr AddressOfPinnedMemory()
        {
            IntPtr ptr = Unwrap().AddressOfPinnedMemory();
            if (ptr == IntPtr.Zero)
            {
                return ptr;
            }
            return ptr + _adjustment;
        }

        public override byte GetByte(int index)
        {
            CheckIndex0(index, 1);
            return Unwrap().GetByte(Idx(index));
        }

        protected internal override byte _GetByte(int index) => Unwrap().GetByte(Idx(index));

        public override short GetShort(int index)
        {
            CheckIndex0(index, 2);
            return Unwrap().GetShort(Idx(index));
        }

        protected internal override short _GetShort(int index) => Unwrap().GetShort(Idx(index));

        public override short GetShortLE(int index)
        {
            CheckIndex0(index, 2);
            return Unwrap().GetShortLE(Idx(index));
        }

        protected internal override short _GetShortLE(int index) => Unwrap().GetShortLE(Idx(index));

        public override int GetUnsignedMedium(int index)
        {
            CheckIndex0(index, 3);
            return Unwrap().GetUnsignedMedium(Idx(index));
        }

        protected internal override int _GetUnsignedMedium(int index) => Unwrap().GetUnsignedMedium(Idx(index));

        public override int GetUnsignedMediumLE(int index)
        {
            CheckIndex0(index, 3);
            return Unwrap().GetUnsignedMediumLE(Idx(index));
        }

        protected internal override int _GetUnsignedMediumLE(int index) => Unwrap().GetUnsignedMediumLE(Idx(index));

        public override int GetInt(int index)
        {
            CheckIndex0(index, 4);
            return Unwrap().GetInt(Idx(index));
        }

        protected internal override int _GetInt(int index) => Unwrap().GetInt(Idx(index));

        public override int GetIntLE(int index)
        {
            CheckIndex0(index, 4);
            return Unwrap().GetIntLE(Idx(index));
        }

        protected internal override int _GetIntLE(int index) => Unwrap().GetIntLE(Idx(index));

        public override long GetLong(int index)
        {
            CheckIndex0(index, 8);
            return Unwrap().GetLong(Idx(index));
        }

        protected internal override long _GetLong(int index) => Unwrap().GetLong(Idx(index));

        public override long GetLongLE(int index)
        {
            CheckIndex0(index, 8);
            return Unwrap().GetLongLE(Idx(index));
        }

        protected internal override long _GetLongLE(int index) => Unwrap().GetLongLE(Idx(index));

        public override IByteBuffer Duplicate() => Unwrap().Duplicate().SetIndex(Idx(ReaderIndex), Idx(WriterIndex));

        public override IByteBuffer Copy(int index, int length)
        {
            CheckIndex0(index, length);
            return Unwrap().Copy(Idx(index), length);
        }

        public override IByteBuffer Slice(int index, int length)
        {
            CheckIndex0(index, length);
            return Unwrap().Slice(Idx(index), length);
        }

        public override IByteBuffer GetBytes(int index, IByteBuffer dst, int dstIndex, int length)
        {
            CheckIndex0(index, length);
            _ = Unwrap().GetBytes(Idx(index), dst, dstIndex, length);
            return this;
        }

        public override IByteBuffer GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            CheckIndex0(index, length);
            _ = Unwrap().GetBytes(Idx(index), dst, dstIndex, length);
            return this;
        }

        public override IByteBuffer GetBytes(int index, Stream destination, int length)
        {
            CheckIndex0(index, length);
            _ = Unwrap().GetBytes(Idx(index), destination, length);
            return this;
        }

        public override IByteBuffer SetByte(int index, int value)
        {
            CheckIndex0(index, 1);
            _ = Unwrap().SetByte(Idx(index), value);
            return this;
        }

        public override ICharSequence GetCharSequence(int index, int length, Encoding encoding)
        {
            CheckIndex0(index, length);
            return Unwrap().GetCharSequence(Idx(index), length, encoding);
        }

        protected internal override void _SetByte(int index, int value) => Unwrap().SetByte(Idx(index), value);

        public override IByteBuffer SetShort(int index, int value)
        {
            CheckIndex0(index, 2);
            _ = Unwrap().SetShort(Idx(index), value);
            return this;
        }

        protected internal override void _SetShort(int index, int value) => Unwrap().SetShort(Idx(index), value);

        public override IByteBuffer SetShortLE(int index, int value)
        {
            CheckIndex0(index, 2);
            _ = Unwrap().SetShortLE(Idx(index), value);
            return this;
        }

        protected internal override void _SetShortLE(int index, int value) => Unwrap().SetShortLE(Idx(index), value);

        public override IByteBuffer SetMedium(int index, int value)
        {
            CheckIndex0(index, 3);
            _ = Unwrap().SetMedium(Idx(index), value);
            return this;
        }

        protected internal override void _SetMedium(int index, int value) => Unwrap().SetMedium(Idx(index), value);

        public override IByteBuffer SetMediumLE(int index, int value)
        {
            CheckIndex0(index, 3);
            _ = Unwrap().SetMediumLE(Idx(index), value);
            return this;
        }

        protected internal override void _SetMediumLE(int index, int value) => Unwrap().SetMediumLE(Idx(index), value);

        public override IByteBuffer SetInt(int index, int value)
        {
            CheckIndex0(index, 4);
            _ = Unwrap().SetInt(Idx(index), value);
            return this;
        }

        protected internal override void _SetInt(int index, int value) => Unwrap().SetInt(Idx(index), value);

        public override IByteBuffer SetIntLE(int index, int value)
        {
            CheckIndex0(index, 4);
            _ = Unwrap().SetIntLE(Idx(index), value);
            return this;
        }

        protected internal override void _SetIntLE(int index, int value) => Unwrap().SetIntLE(Idx(index), value);

        public override IByteBuffer SetLong(int index, long value)
        {
            CheckIndex0(index, 8);
            _ = Unwrap().SetLong(Idx(index), value);
            return this;
        }

        protected internal override void _SetLong(int index, long value) => Unwrap().SetLong(Idx(index), value);

        public override IByteBuffer SetLongLE(int index, long value)
        {
            CheckIndex0(index, 8);
            _ = Unwrap().SetLongLE(Idx(index), value);
            return this;
        }

        protected internal override void _SetLongLE(int index, long value) => Unwrap().SetLongLE(Idx(index), value);

        public override IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            CheckIndex0(index, length);
            _ = Unwrap().SetBytes(Idx(index), src, srcIndex, length);
            return this;
        }

        public override IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length)
        {
            CheckIndex0(index, length);
            _ = Unwrap().SetBytes(Idx(index), src, srcIndex, length);
            return this;
        }

        public override Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken)
        {
            CheckIndex0(index, length);
            return Unwrap().SetBytesAsync(Idx(index), src, length, cancellationToken);
        }

        public sealed override bool IsSingleIoBuffer => Unwrap().IsSingleIoBuffer;

        public sealed override int IoBufferCount => Unwrap().IoBufferCount;

        public override ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            CheckIndex0(index, length);
            return Unwrap().GetIoBuffer(Idx(index), length);
        }

        public override ArraySegment<byte>[] GetIoBuffers(int index, int length)
        {
            CheckIndex0(index, length);
            return Unwrap().GetIoBuffers(Idx(index), length);
        }

        public override int ForEachByte(int index, int length, IByteProcessor processor)
        {
            CheckIndex0(index, length);
            int ret = Unwrap().ForEachByte(Idx(index), length, processor);
            if (ret >= _adjustment)
            {
                return ret - _adjustment;
            }
            else
            {
                return IndexNotFound;
            }
        }

        public override int ForEachByteDesc(int index, int length, IByteProcessor processor)
        {
            CheckIndex0(index, length);
            int ret = Unwrap().ForEachByteDesc(Idx(index), length, processor);
            if (ret >= _adjustment)
            {
                return ret - _adjustment;
            }
            else
            {
                return IndexNotFound;
            }
        }

        // Returns the index with the needed adjustment.
        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal protected int Idx(int index) => index + _adjustment;

        internal static void CheckSliceOutOfBounds(int index, int length, IByteBuffer buffer)
        {
            if (MathUtil.IsOutOfBounds(index, length, buffer.Capacity))
            {
                ThrowHelper.ThrowIndexOutOfRangeException_CheckSliceOutOfBounds(index, length, buffer);
            }
        }
    }
}
