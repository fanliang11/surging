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
    using DotNetty.Common;

    sealed unsafe partial class PooledUnsafeDirectByteBuffer : PooledByteBuffer<byte[]>
    {
        static readonly ThreadLocalPool<PooledUnsafeDirectByteBuffer> Recycler = new ThreadLocalPool<PooledUnsafeDirectByteBuffer>(handle => new PooledUnsafeDirectByteBuffer(handle, 0));

        byte* _memoryAddress;

        internal static PooledUnsafeDirectByteBuffer NewInstance(int maxCapacity)
        {
            PooledUnsafeDirectByteBuffer buf = Recycler.Take();
            buf.Reuse(maxCapacity);
            return buf;
        }

        PooledUnsafeDirectByteBuffer(ThreadLocalPool.Handle recyclerHandle, int maxCapacity)
            : base(recyclerHandle, maxCapacity)
        {
        }

        internal sealed override void Init(PoolChunk<byte[]> chunk, long handle, int offset, int length, int maxLength,
            PoolThreadCache<byte[]> cache)
        {
            base.Init(chunk, handle, offset, length, maxLength, cache);
            InitMemoryAddress();
        }

        internal sealed override void InitUnpooled(PoolChunk<byte[]> chunk, int length)
        {
            base.InitUnpooled(chunk, length);
            InitMemoryAddress();
        }

        void InitMemoryAddress()
        {
            _memoryAddress = (byte*)Unsafe.Add<byte>(Origin.ToPointer(), Offset);
        }

        public sealed override bool IsDirect => true;

        protected internal sealed override byte _GetByte(int index) => *(_memoryAddress + index);

        protected internal sealed override short _GetShort(int index) => UnsafeByteBufferUtil.GetShort(Addr(index));

        protected internal sealed override short _GetShortLE(int index) => UnsafeByteBufferUtil.GetShortLE(Addr(index));

        protected internal sealed override int _GetUnsignedMedium(int index) => UnsafeByteBufferUtil.GetUnsignedMedium(Addr(index));

        protected internal sealed override int _GetUnsignedMediumLE(int index) => UnsafeByteBufferUtil.GetUnsignedMediumLE(Addr(index));

        protected internal sealed override int _GetInt(int index) => UnsafeByteBufferUtil.GetInt(Addr(index));

        protected internal sealed override int _GetIntLE(int index) => UnsafeByteBufferUtil.GetIntLE(Addr(index));

        protected internal sealed override long _GetLong(int index) => UnsafeByteBufferUtil.GetLong(Addr(index));

        protected internal sealed override long _GetLongLE(int index) => UnsafeByteBufferUtil.GetLongLE(Addr(index));

        public sealed override IByteBuffer GetBytes(int index, IByteBuffer dst, int dstIndex, int length)
        {
            if (dst is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst); }
            CheckDstIndex(index, length, dstIndex, dst.Capacity);
            UnsafeByteBufferUtil.GetBytes(this, Addr(index), index, dst, dstIndex, length);
            return this;
        }

        public sealed override IByteBuffer GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            if (dst is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dst); }
            CheckDstIndex(index, length, dstIndex, dst.Length);
            UnsafeByteBufferUtil.GetBytes(Addr(index), dst, dstIndex, length);
            return this;
        }

        public sealed override IByteBuffer GetBytes(int index, Stream output, int length)
        {
            if (output is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.output); }
            CheckIndex(index, length);
            //UnsafeByteBufferUtil.GetBytes(this, Addr(index), index, output, length);
            // UnsafeByteBufferUtil.GetBytes 多一遍内存拷贝，最终还是调用 stream.write，没啥必要
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            output.Write(_GetReadableSpan(index, length));
#else
            output.Write(Memory, Idx(index), length);
#endif
            return this;
        }

        protected internal sealed override void _SetByte(int index, int value) => *(_memoryAddress + index) = unchecked((byte)value);

        protected internal sealed override void _SetShort(int index, int value) => UnsafeByteBufferUtil.SetShort(Addr(index), value);

        protected internal sealed override void _SetShortLE(int index, int value) => UnsafeByteBufferUtil.SetShortLE(Addr(index), value);

        protected internal sealed override void _SetMedium(int index, int value) => UnsafeByteBufferUtil.SetMedium(Addr(index), value);

        protected internal sealed override void _SetMediumLE(int index, int value) => UnsafeByteBufferUtil.SetMediumLE(Addr(index), value);

        protected internal sealed override void _SetInt(int index, int value) => UnsafeByteBufferUtil.SetInt(Addr(index), value);

        protected internal sealed override void _SetIntLE(int index, int value) => UnsafeByteBufferUtil.SetIntLE(Addr(index), value);

        protected internal sealed override void _SetLong(int index, long value) => UnsafeByteBufferUtil.SetLong(Addr(index), value);

        protected internal sealed override void _SetLongLE(int index, long value) => UnsafeByteBufferUtil.SetLongLE(Addr(index), value);

        public sealed override IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }
            CheckSrcIndex(index, length, srcIndex, src.Capacity);
            UnsafeByteBufferUtil.SetBytes(this, Addr(index), index, src, srcIndex, length);
            return this;
        }

        public sealed override IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }
            CheckSrcIndex(index, length, srcIndex, src.Length);
            UnsafeByteBufferUtil.SetBytes(Addr(index), src, srcIndex, length);
            return this;
        }

        public sealed override Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken)
        {
            if (src is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.src); }
            CheckIndex(index, length);
            //int read = UnsafeByteBufferUtil.SetBytes(this, Addr(index), index, src, length);
            //return Task.FromResult(read);
            int readTotal = 0;
            int read;
#if !(NETCOREAPP || NETSTANDARD_2_0_GREATER)
            int offset = Idx(index);
#endif
            do
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                read = src.Read(_GetSpan(index + readTotal, length - readTotal));
#else
                read = src.Read(Memory, offset + readTotal, length - readTotal);
#endif
                readTotal += read;
            }
            while (read > 0 && readTotal < length);

            return Task.FromResult(readTotal);
        }

        public sealed override IByteBuffer Copy(int index, int length)
        {
            CheckIndex(index, length);
            return UnsafeByteBufferUtil.Copy(this, Addr(index), index, length);
        }

        public sealed override bool IsSingleIoBuffer => true;

        public sealed override int IoBufferCount => 1;

        public sealed override ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            CheckIndex(index, length);
            index = Idx(index);
            return new ArraySegment<byte>(Memory, index, length);
        }

        public sealed override ArraySegment<byte>[] GetIoBuffers(int index, int length) => new[] { GetIoBuffer(index, length) };

        public sealed override bool HasArray => true;

        public sealed override byte[] Array
        {
            get
            {
                EnsureAccessible();
                return Memory;
            }
        }

        public sealed override int ArrayOffset => Offset;

        public sealed override bool HasMemoryAddress => true;

        public sealed override ref byte GetPinnableMemoryAddress()
        {
            EnsureAccessible();
            return ref Unsafe.AsRef<byte>(_memoryAddress);
        }

        public sealed override IntPtr AddressOfPinnedMemory() => (IntPtr)_memoryAddress;

        [MethodImpl(InlineMethod.AggressiveInlining)]
        byte* Addr(int index) => _memoryAddress + index;

        public sealed override IByteBuffer SetZero(int index, int length)
        {
            CheckIndex(index, length);
            UnsafeByteBufferUtil.SetZero(Addr(index), length);
            return this;
        }

        public sealed override IByteBuffer WriteZero(int length)
        {
            if (0u >= (uint)length) { return this; }

            _ = EnsureWritable(length);
            int wIndex = WriterIndex;
            CheckIndex0(wIndex, length);
            UnsafeByteBufferUtil.SetZero(Addr(wIndex), length);
            _ = SetWriterIndex(wIndex + length);

            return this;
        }
    }
}
