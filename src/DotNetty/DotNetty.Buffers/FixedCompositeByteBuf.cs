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
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// <see cref="IByteBuffer"/> implementation which allows to wrap an array of <see cref="IByteBuffer"/> in a read-only mode.
    /// This is useful to write an array of <see cref="IByteBuffer"/>s.
    /// </summary>
    public sealed partial class FixedCompositeByteBuf : AbstractReferenceCountedByteBuffer
    {
        private static readonly IByteBuffer[] Empty = { Unpooled.Empty };

        private readonly int _nioBufferCount;
        private readonly int _capacity;
        private readonly IByteBufferAllocator _allocator;
        private readonly IByteBuffer[] _buffers;
        private readonly bool _direct;

        public FixedCompositeByteBuf(IByteBufferAllocator allocator, params IByteBuffer[] buffers)
            : base(AbstractByteBufferAllocator.DefaultMaxCapacity)
        {
            if (buffers is null || 0u >= (uint)buffers.Length)
            {
                _buffers = Empty;
                _nioBufferCount = 1;
                _capacity = 0;
                _direct = Unpooled.Empty.IsDirect;
            }
            else
            {
                var b = buffers[0];
                _buffers = buffers;
                var direct = true;
                int nioBufferCount = b.IoBufferCount;
                int capacity = b.ReadableBytes;
                for (int i = 1; i < buffers.Length; i++)
                {
                    b = buffers[i];
                    nioBufferCount += b.IoBufferCount;
                    capacity += b.ReadableBytes;
                    if (!b.IsDirect)
                    {
                        direct = false;
                    }
                }
                _nioBufferCount = nioBufferCount;
                _capacity = capacity;
                _direct = direct;
            }
            _ = SetIndex(0, _capacity);
            _allocator = allocator;
        }

        public override bool IsWritable() => false;

        public override bool IsWritable(int size) => false;

        public override int Capacity => _capacity;

        public override int MaxCapacity => _capacity;

        public override IByteBufferAllocator Allocator => _allocator;

        public override IByteBuffer Unwrap() => null;

        public override bool IsDirect => _direct;

        public override bool HasArray
        {
            get
            {
                switch (_buffers.Length)
                {
                    case 0:
                        return true;
                    case 1:
                        return Buffer(0).HasArray;
                    default:
                        return false;
                }
            }
        }

        public override byte[] Array
        {
            get
            {
                switch (_buffers.Length)
                {
                    case 0:
                        return ArrayExtensions.ZeroBytes;
                    case 1:
                        return Buffer(0).Array;
                    default:
                        throw ThrowHelper.GetNotSupportedException();
                }
            }
        }

        public override int ArrayOffset
        {
            get
            {
                switch (_buffers.Length)
                {
                    case 0:
                        return 0;
                    case 1:
                        return Buffer(0).ArrayOffset;
                    default:
                        throw ThrowHelper.GetNotSupportedException();
                }
            }
        }

        public override bool HasMemoryAddress
        {
            get
            {
                switch (_buffers.Length)
                {
                    case 1:
                        return Buffer(0).HasMemoryAddress;
                    default:
                        return false;
                }
            }
        }

        public override ref byte GetPinnableMemoryAddress()
        {
            switch (_buffers.Length)
            {
                case 1:
                    return ref Buffer(0).GetPinnableMemoryAddress();
                default:
                    throw ThrowHelper.GetNotSupportedException();
            }
        }

        public override IntPtr AddressOfPinnedMemory()
        {
            switch (_buffers.Length)
            {
                case 1:
                    return Buffer(0).AddressOfPinnedMemory();
                default:
                    throw ThrowHelper.GetNotSupportedException();
            }
        }

        public override bool IsSingleIoBuffer => 1u >= (uint)_nioBufferCount;

        public override int IoBufferCount => _nioBufferCount;

        ComponentEntry FindComponent(int index)
        {
            int readable = 0;
            for (int i = 0; i < _buffers.Length; i++)
            {
                var b = _buffers[i];
                var comp = b as ComponentEntry;
                if (comp is object)
                {
                    b = comp.Buf;
                }
                readable += b.ReadableBytes;
                if (index < readable)
                {
                    if (comp is null)
                    {
                        // Create a new component and store it in the array so it not create a new object
                        // on the next access.
                        comp = new ComponentEntry(i, readable - b.ReadableBytes, b);
                        _buffers[i] = comp;
                    }
                    return comp;
                }
            }

            throw ThrowHelper.GetInvalidOperationException_ShouldNotReachHere();
        }

        /// <summary>
        /// Return the <see cref="IByteBuffer"/> stored at the given index of the array.
        /// </summary>
        IByteBuffer Buffer(int idx)
        {
            var b = _buffers[idx];
            return b is ComponentEntry comp ? comp.Buf : b;
        }

        public override byte GetByte(int index)
        {
            return _GetByte(index);
        }

        protected internal override byte _GetByte(int index)
        {
            var c = FindComponent(index);
            return c.Buf.GetByte(index - c.Offset);
        }

        protected internal override short _GetShort(int index)
        {
            ComponentEntry c = FindComponent(index);
            if (index + 2 <= c.EndOffset)
            {
                return c.Buf.GetShort(index - c.Offset);
            }

            return (short)(_GetByte(index) << 8 | _GetByte(index + 1));
        }

        protected internal override short _GetShortLE(int index)
        {
            ComponentEntry c = FindComponent(index);
            if (index + 2 <= c.EndOffset)
            {
                return c.Buf.GetShortLE(index - c.Offset);
            }

            return (short)(_GetByte(index) << 8 | _GetByte(index + 1));
        }

        protected internal override int _GetUnsignedMedium(int index)
        {
            ComponentEntry c = FindComponent(index);
            if (index + 3 <= c.EndOffset)
            {
                return c.Buf.GetUnsignedMedium(index - c.Offset);
            }

            return (_GetShort(index) & 0xffff) << 8 | _GetByte(index + 2);
        }

        protected internal override int _GetUnsignedMediumLE(int index)
        {
            ComponentEntry c = FindComponent(index);
            if (index + 3 <= c.EndOffset)
            {
                return c.Buf.GetUnsignedMediumLE(index - c.Offset);
            }

            return (_GetShortLE(index) & 0xffff) << 8 | _GetByte(index + 2);
        }

        protected internal override int _GetInt(int index)
        {
            ComponentEntry c = FindComponent(index);
            if (index + 4 <= c.EndOffset)
            {
                return c.Buf.GetInt(index - c.Offset);
            }

            return _GetShort(index) << 16 | (ushort)_GetShort(index + 2);
        }

        protected internal override int _GetIntLE(int index)
        {
            ComponentEntry c = FindComponent(index);
            if (index + 4 <= c.EndOffset)
            {
                return c.Buf.GetIntLE(index - c.Offset);
            }

            return (_GetShortLE(index) << 16 | (ushort)_GetShortLE(index + 2));
        }

        protected internal override long _GetLong(int index)
        {
            ComponentEntry c = FindComponent(index);
            if (index + 8 <= c.EndOffset)
            {
                return c.Buf.GetLong(index - c.Offset);
            }

            return (long)_GetInt(index) << 32 | (uint)_GetInt(index + 4);
        }

        protected internal override long _GetLongLE(int index)
        {
            ComponentEntry c = FindComponent(index);
            if (index + 8 <= c.EndOffset)
            {
                return c.Buf.GetLongLE(index - c.Offset);
            }

            return (_GetIntLE(index) << 32 | _GetIntLE(index + 4));
        }

        public override IByteBuffer GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, dst.Length);
            if (0u >= (uint)length) { return this; }

            var c = FindComponent(index);
            int i = c.Index;
            int adjustment = c.Offset;
            var s = c.Buf;
            while (true)
            {
                int localLength = Math.Min(length, s.ReadableBytes - (index - adjustment));
                _ = s.GetBytes(index - adjustment, dst, dstIndex, localLength);
                index += localLength;
                dstIndex += localLength;
                length -= localLength;
                adjustment += s.ReadableBytes;
                if ((uint)(length - 1) > SharedConstants.TooBigOrNegative) // length <= 0
                {
                    break;
                }
                s = Buffer(++i);
            }
            return this;
        }

        public override IByteBuffer GetBytes(int index, IByteBuffer dst, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, dst.Capacity);
            if (0u >= (uint)length) { return this; }

            var c = FindComponent(index);
            int i = c.Index;
            int adjustment = c.Offset;
            var s = c.Buf;
            while (true)
            {
                int localLength = Math.Min(length, s.ReadableBytes - (index - adjustment));
                _ = s.GetBytes(index - adjustment, dst, dstIndex, localLength);
                index += localLength;
                dstIndex += localLength;
                length -= localLength;
                adjustment += s.ReadableBytes;
                if ((uint)(length - 1) > SharedConstants.TooBigOrNegative) // length <= 0
                {
                    break;
                }
                s = Buffer(++i);
            }
            return this;
        }

        public override IByteBuffer GetBytes(int index, Stream destination, int length)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length) { return this; }

            var c = FindComponent(index);
            int i = c.Index;
            int adjustment = c.Offset;
            var s = c.Buf;
            while (true)
            {
                int localLength = Math.Min(length, s.ReadableBytes - (index - adjustment));
                _ = s.GetBytes(index - adjustment, destination, localLength);
                index += localLength;
                length -= localLength;
                adjustment += s.ReadableBytes;
                if ((uint)(length - 1) > SharedConstants.TooBigOrNegative) // length <= 0
                {
                    break;
                }
                s = Buffer(++i);
            }
            return this;
        }

        public override IByteBuffer Copy(int index, int length)
        {
            CheckIndex(index, length);
            var release = true;
            var buf = _allocator.Buffer(length);
            try
            {
                _ = buf.WriteBytes(this, index, length);
                release = false;
                return buf;
            }
            finally
            {
                if (release) { _ = buf.Release(); }
            }
        }

        public override ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length) { return default; }

            if (_buffers.Length == 1)
            {
                var buf = Buffer(0);
                if (buf.IsSingleIoBuffer)
                {
                    return buf.GetIoBuffer(index, length);
                }
            }

            var buffers = GetSequence(index, length);
            if (buffers.IsSingleSegment && MemoryMarshal.TryGetArray(buffers.First, out var segment))
            {
                return segment;
            }
            var merged = buffers.ToArray();
            return new ArraySegment<byte>(merged);
        }

        public override ArraySegment<byte>[] GetIoBuffers(int index, int length)
        {
            CheckIndex(index, length);
            if (0u >= (uint)length) { return EmptyArray<ArraySegment<byte>>.Instance; }

            var array = ThreadLocalList<ArraySegment<byte>>.NewInstance(_nioBufferCount);
            try
            {
                var c = FindComponent(index);
                int i = c.Index;
                int adjustment = c.Offset;
                var s = c.Buf;
                for (; ; )
                {
                    int localLength = Math.Min(length, s.ReadableBytes - (index - adjustment));
                    switch (s.IoBufferCount)
                    {
                        case 0:
                            ThrowHelper.ThrowNotSupportedException();
                            break;
                        case 1:
                            array.Add(s.GetIoBuffer(index - adjustment, localLength));
                            break;
                        default:
                            array.AddRange(s.GetIoBuffers(index - adjustment, localLength));
                            break;
                    }

                    index += localLength;
                    length -= localLength;
                    adjustment += s.ReadableBytes;
                    if ((uint)(length - 1) > SharedConstants.TooBigOrNegative) // length <= 0
                    {
                        break;
                    }
                    s = Buffer(++i);
                }

                return array.ToArray();
            }
            finally
            {
                array.Return();
            }
        }

        protected internal override void Deallocate()
        {
            for (int i = 0; i < _buffers.Length; i++)
            {
                _ = Buffer(i).Release();
            }
        }

        public override IByteBuffer AdjustCapacity(int newCapacity) => throw ThrowHelper.GetReadOnlyBufferException();

        public override IByteBuffer EnsureWritable(int minWritableBytes) => throw ThrowHelper.GetReadOnlyBufferException();

        public override int EnsureWritable(int minWritableBytes, bool force) => 1;

        public override IByteBuffer DiscardReadBytes() => throw ThrowHelper.GetReadOnlyBufferException();

        public override IByteBuffer DiscardSomeReadBytes() => throw ThrowHelper.GetReadOnlyBufferException();

        public override IByteBuffer SetBytes(int index, byte[] src)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetBytes(int index, IByteBuffer src, int length)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetByte(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        protected internal override void _SetByte(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetInt(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        protected internal override void _SetInt(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetIntLE(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        protected internal override void _SetIntLE(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetLong(int index, long value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        protected internal override void _SetLong(int index, long value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetLongLE(int index, long value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        protected internal override void _SetLongLE(int index, long value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetMedium(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        protected internal override void _SetMedium(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetMediumLE(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        protected internal override void _SetMediumLE(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetShort(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        protected internal override void _SetShort(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetShortLE(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        protected internal override void _SetShortLE(int index, int value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetBoolean(int index, bool value)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override int SetCharSequence(int index, ICharSequence sequence, Encoding encoding)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override int SetString(int index, string value, Encoding encoding)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        public override IByteBuffer SetZero(int index, int length)
        {
            throw ThrowHelper.GetReadOnlyBufferException();
        }

        sealed class ComponentEntry : WrappedByteBuffer
        {
            internal readonly int Index;
            internal readonly int Offset;
            internal readonly int EndOffset;

            public ComponentEntry(int index, int offset, IByteBuffer buf)
                : base(buf)
            {
                Index = index;
                Offset = offset;
                EndOffset = offset + buf.ReadableBytes;
            }
        }
    }
}
