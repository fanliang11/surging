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
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    internal interface IPooledByteBuffer
    {
        long Handle { get; }
        int MaxLength { get; }
    }

    abstract class PooledByteBuffer<T> : AbstractReferenceCountedByteBuffer, IPooledByteBuffer
    {
        private readonly ThreadLocalPool.Handle _recyclerHandle;

        protected internal PoolChunk<T> Chunk;
        protected internal long Handle;
        protected internal T Memory;
        protected internal int Offset;
        protected internal int Length;
        protected internal IntPtr Origin;
        internal int MaxLength;
        internal PoolThreadCache<T> Cache;
        private PooledByteBufferAllocator _allocator;

        protected PooledByteBuffer(ThreadLocalPool.Handle recyclerHandle, int maxCapacity)
            : base(maxCapacity)
        {
            _recyclerHandle = recyclerHandle;
        }

        internal virtual void Init(PoolChunk<T> chunk, long handle, int offset, int length, int maxLength, PoolThreadCache<T> cache) =>
            Init0(chunk, handle, offset, length, maxLength, cache);

        internal virtual void InitUnpooled(PoolChunk<T> chunk, int length) => Init0(chunk, 0, 0, length, length, null);

        unsafe void Init0(PoolChunk<T> chunk, long handle, int offset, int length, int maxLength, PoolThreadCache<T> cache)
        {
            Debug.Assert(handle >= 0);
            Debug.Assert(chunk is object);

            Chunk = chunk;
            Memory = chunk.Memory;
            _allocator = chunk.Arena.Parent;
            Origin = chunk.NativePointer;
            Cache = cache;
            Handle = handle;
            Offset = offset;
            Length = length;
            MaxLength = maxLength;
        }

        long IPooledByteBuffer.Handle => Handle;
        int IPooledByteBuffer.MaxLength => MaxLength;

        /**
          * Method must be called before reuse this {@link PooledByteBufAllocator}
          */
        internal void Reuse(int maxCapacity)
        {
            SetMaxCapacity(maxCapacity);
            ResetReferenceCount();
            SetIndex0(0, 0);
            DiscardMarks();
        }

        public override int Capacity
        {
            [MethodImpl(InlineMethod.AggressiveInlining)]
            get => Length;
        }

        public override int MaxFastWritableBytes => Math.Min(MaxLength, MaxCapacity) - WriterIndex;

        public sealed override IByteBuffer AdjustCapacity(int newCapacity)
        {
            uint uLength = (uint)Length;
            uint unewCapacity = (uint)newCapacity;
            if (unewCapacity == uLength)
            {
                EnsureAccessible();
                return this;
            }

            CheckNewCapacity(newCapacity);

            if (!Chunk.Unpooled)
            {
                uint uMaxLength = (uint)MaxLength;
                // If the request capacity does not require reallocation, just update the length of the memory.
                if (unewCapacity > uLength)
                {
                    if (unewCapacity <= uMaxLength)
                    {
                        Length = newCapacity;
                        return this;
                    }
                }
                else if (unewCapacity > MaxLength.RightShift2U(1)
                    && (uMaxLength > 512u || unewCapacity > uMaxLength - 16u))
                {
                    // here newCapacity < length
                    Length = newCapacity;
                    TrimIndicesToCapacity(newCapacity);
                    return this;
                }
            }

            // Reallocation required.
            Chunk.Arena.Reallocate(this, newCapacity, true);
            return this;
        }

        public sealed override bool IsContiguous => true;

        public sealed override IByteBufferAllocator Allocator => _allocator;

        public sealed override IByteBuffer Unwrap() => null;

        public sealed override IByteBuffer RetainedDuplicate() => PooledDuplicatedByteBuffer.NewInstance(this, this, ReaderIndex, WriterIndex);

        public sealed override IByteBuffer RetainedSlice()
        {
            int index = ReaderIndex;
            return RetainedSlice(index, WriterIndex - index);
        }

        public sealed override IByteBuffer RetainedSlice(int index, int length) => PooledSlicedByteBuffer.NewInstance(this, this, index, length);

        protected internal sealed override void Deallocate()
        {
            if (Handle >= 0)
            {
                long handle = Handle;
                Handle = -1;
                Origin = IntPtr.Zero;
                Memory = default;
                Chunk.Arena.Free(Chunk, handle, MaxLength, Cache);
                Chunk = null;
                Recycle();
            }
        }

        void Recycle() => _recyclerHandle.Release(this);

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected int Idx(int index) => Offset + index;
    }
}