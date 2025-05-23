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
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// A derived buffer which forbids any write requests to its parent.  It is
    /// recommended to use <see cref="Unpooled.UnmodifiableBuffer(IByteBuffer)"/>
    /// instead of calling the constructor explicitly.
    /// </summary>
    public partial class ReadOnlyByteBuffer : AbstractDerivedByteBuffer
    {
        private readonly IByteBuffer _buffer;

        public ReadOnlyByteBuffer(IByteBuffer buffer)
            : base(buffer is object ? buffer.MaxCapacity : AbstractByteBufferAllocator.DefaultMaxCapacity)
        {
            if (buffer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.buffer); }

            switch (buffer)
            {
                case ReadOnlyByteBuffer _:
                case UnpooledDuplicatedByteBuffer _:
                    _buffer = buffer.Unwrap();
                    break;

                default:
                    _buffer = buffer;
                    break;
            }
            _ = SetIndex(buffer.ReaderIndex, buffer.WriterIndex);
        }

        public override bool IsReadOnly => true;

        public override IByteBuffer AsReadOnly() => this;

        public override bool IsWritable() => false;

        public override bool IsWritable(int size) => false;

        public override int Capacity => Unwrap().Capacity;

        public override IByteBuffer AdjustCapacity(int newCapacity) => throw ThrowHelper.GetReadOnlyBufferException();

        public override IByteBuffer EnsureWritable(int minWritableBytes) => throw ThrowHelper.GetReadOnlyBufferException();

        public override int EnsureWritable(int minWritableBytes, bool force) => 1;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public override IByteBuffer Unwrap() => _buffer;

        public override IByteBufferAllocator Allocator => Unwrap().Allocator;

        public override bool IsDirect => Unwrap().IsDirect;

        public override bool HasArray => false;

        public override byte[] Array => throw ThrowHelper.GetReadOnlyBufferException();

        public override int ArrayOffset => throw ThrowHelper.GetReadOnlyBufferException();

        public override bool HasMemoryAddress => Unwrap().HasMemoryAddress;

        public override ref byte GetPinnableMemoryAddress() => ref Unwrap().GetPinnableMemoryAddress();

        public override IntPtr AddressOfPinnedMemory() => Unwrap().AddressOfPinnedMemory();

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

        public override IByteBuffer GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            return Unwrap().GetBytes(index, destination, dstIndex, length);
        }

        public override IByteBuffer GetBytes(int index, IByteBuffer destination, int dstIndex, int length)
        {
            return Unwrap().GetBytes(index, destination, dstIndex, length);
        }

        public override IByteBuffer GetBytes(int index, Stream destination, int length)
        {
            return Unwrap().GetBytes(index, destination, length);
        }

        public override byte GetByte(int index)
        {
            return Unwrap().GetByte(index);
        }

        protected internal override byte _GetByte(int index)
        {
            return Unwrap().GetByte(index);
        }

        public override int GetInt(int index)
        {
            return Unwrap().GetInt(index);
        }

        protected internal override int _GetInt(int index)
        {
            return Unwrap().GetInt(index);
        }

        public override int GetIntLE(int index)
        {
            return Unwrap().GetIntLE(index);
        }

        protected internal override int _GetIntLE(int index)
        {
            return Unwrap().GetIntLE(index);
        }

        public override long GetLong(int index)
        {
            return Unwrap().GetLong(index);
        }

        protected internal override long _GetLong(int index)
        {
            return Unwrap().GetLong(index);
        }

        public override long GetLongLE(int index)
        {
            return Unwrap().GetLongLE(index);
        }

        protected internal override long _GetLongLE(int index)
        {
            return Unwrap().GetLongLE(index);
        }

        public override short GetShort(int index)
        {
            return Unwrap().GetShort(index);
        }

        protected internal override short _GetShort(int index)
        {
            return Unwrap().GetShort(index);
        }

        public override short GetShortLE(int index)
        {
            return Unwrap().GetShortLE(index);
        }

        protected internal override short _GetShortLE(int index)
        {
            return Unwrap().GetShortLE(index);
        }

        public override int GetUnsignedMedium(int index)
        {
            return Unwrap().GetUnsignedMedium(index);
        }

        protected internal override int _GetUnsignedMedium(int index)
        {
            return Unwrap().GetUnsignedMedium(index);
        }

        public override int GetUnsignedMediumLE(int index)
        {
            return Unwrap().GetUnsignedMediumLE(index);
        }

        protected internal override int _GetUnsignedMediumLE(int index)
        {
            return Unwrap().GetUnsignedMediumLE(index);
        }

        public override string GetString(int index, int length, Encoding encoding)
        {
            return Unwrap().GetString(index, length, encoding);
        }

        public override bool IsSingleIoBuffer => Unwrap().IsSingleIoBuffer;

        public override int IoBufferCount => Unwrap().IoBufferCount;

        public override IByteBuffer Copy(int index, int length)
        {
            return Unwrap().Copy(index, length);
        }

        public override IByteBuffer Duplicate()
        {
            return new ReadOnlyByteBuffer(this);
        }

        public override IByteBuffer Slice(int index, int length)
        {
            return new ReadOnlyByteBuffer(Unwrap().Slice(index, length));
        }

        public override int ForEachByte(int index, int length, IByteProcessor processor)
        {
            return Unwrap().ForEachByte(index, length, processor);
        }

        public override int ForEachByteDesc(int index, int length, IByteProcessor processor)
        {
            return Unwrap().ForEachByteDesc(index, length, processor);
        }
    }
}
