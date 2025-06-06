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
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    sealed partial class PooledDuplicatedByteBuffer : AbstractPooledDerivedByteBuffer
    {
        static readonly ThreadLocalPool<PooledDuplicatedByteBuffer> Recycler = new ThreadLocalPool<PooledDuplicatedByteBuffer>(handle => new PooledDuplicatedByteBuffer(handle));

        internal static PooledDuplicatedByteBuffer NewInstance(AbstractByteBuffer unwrapped, IByteBuffer wrapped, int readerIndex, int writerIndex)
        {
            PooledDuplicatedByteBuffer duplicate = Recycler.Take();
            _ = duplicate.Init<PooledDuplicatedByteBuffer>(unwrapped, wrapped, readerIndex, writerIndex, unwrapped.MaxCapacity);
            _ = duplicate.MarkReaderIndex();
            _ = duplicate.MarkWriterIndex();

            return duplicate;
        }

        public PooledDuplicatedByteBuffer(ThreadLocalPool.Handle recyclerHandle)
            : base(recyclerHandle)
        {
        }

        public sealed override int Capacity => Unwrap().Capacity;

        public sealed override IByteBuffer AdjustCapacity(int newCapacity)
        {
            _ = Unwrap().AdjustCapacity(newCapacity);
            return this;
        }

        public sealed override int ArrayOffset => Unwrap().ArrayOffset;

        public sealed override ref byte GetPinnableMemoryAddress() => ref Unwrap().GetPinnableMemoryAddress();

        public sealed override IntPtr AddressOfPinnedMemory() => Unwrap().AddressOfPinnedMemory();

        public sealed override ArraySegment<byte> GetIoBuffer(int index, int length) => Unwrap().GetIoBuffer(index, length);

        public sealed override ArraySegment<byte>[] GetIoBuffers(int index, int length) => Unwrap().GetIoBuffers(index, length);

        public sealed override IByteBuffer Copy(int index, int length) => Unwrap().Copy(index, length);

        public sealed override IByteBuffer RetainedSlice(int index, int length) => PooledSlicedByteBuffer.NewInstance(UnwrapCore(), this, index, length);

        public sealed override IByteBuffer Duplicate() => Duplicate0().SetIndex(ReaderIndex, WriterIndex);

        public sealed override IByteBuffer RetainedDuplicate() => NewInstance(UnwrapCore(), this, ReaderIndex, WriterIndex);

        protected internal sealed override byte _GetByte(int index) => UnwrapCore()._GetByte(index);

        protected internal sealed override short _GetShort(int index) => UnwrapCore()._GetShort(index);

        protected internal sealed override short _GetShortLE(int index) => UnwrapCore()._GetShortLE(index);

        protected internal sealed override int _GetUnsignedMedium(int index) => UnwrapCore()._GetUnsignedMedium(index);

        protected internal sealed override int _GetUnsignedMediumLE(int index) => UnwrapCore()._GetUnsignedMediumLE(index);

        protected internal sealed override int _GetInt(int index) => UnwrapCore()._GetInt(index);

        protected internal sealed override int _GetIntLE(int index) => UnwrapCore()._GetIntLE(index);

        protected internal sealed override long _GetLong(int index) => UnwrapCore()._GetLong(index);

        protected internal sealed override long _GetLongLE(int index) => UnwrapCore()._GetLongLE(index);

        public sealed override IByteBuffer GetBytes(int index, IByteBuffer destination, int dstIndex, int length) { _ = Unwrap().GetBytes(index, destination, dstIndex, length); return this; }

        public sealed override IByteBuffer GetBytes(int index, byte[] destination, int dstIndex, int length) { _ = Unwrap().GetBytes(index, destination, dstIndex, length); return this; }

        public sealed override IByteBuffer GetBytes(int index, Stream destination, int length) { _ = Unwrap().GetBytes(index, destination, length); return this; }

        protected internal sealed override void _SetByte(int index, int value) => UnwrapCore()._SetByte(index, value);

        protected internal sealed override void _SetShort(int index, int value) => UnwrapCore()._SetShort(index, value);

        protected internal sealed override void _SetShortLE(int index, int value) => UnwrapCore()._SetShortLE(index, value);

        protected internal sealed override void _SetMedium(int index, int value) => UnwrapCore()._SetMedium(index, value);

        protected internal sealed override void _SetMediumLE(int index, int value) => UnwrapCore()._SetMediumLE(index, value);

        public sealed override IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length) { _ = Unwrap().SetBytes(index, src, srcIndex, length); return this; }

        public sealed override Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken) => Unwrap().SetBytesAsync(index, src, length, cancellationToken);

        public sealed override IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length) { _ = Unwrap().SetBytes(index, src, srcIndex, length); return this; }

        protected internal sealed override void _SetInt(int index, int value) => UnwrapCore()._SetInt(index, value);

        protected internal sealed override void _SetIntLE(int index, int value) => UnwrapCore()._SetIntLE(index, value);

        protected internal sealed override void _SetLong(int index, long value) => UnwrapCore()._SetLong(index, value);

        protected internal sealed override void _SetLongLE(int index, long value) => UnwrapCore()._SetLongLE(index, value);

        public sealed override int ForEachByte(int index, int length, IByteProcessor processor) => Unwrap().ForEachByte(index, length, processor);

        public sealed override int ForEachByteDesc(int index, int length, IByteProcessor processor) => Unwrap().ForEachByteDesc(index, length, processor);
    }
}