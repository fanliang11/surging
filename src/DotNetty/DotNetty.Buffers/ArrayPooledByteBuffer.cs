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
using DotNetty.Common;
using DotNetty.Common.Internal;
#if NET
using System.Runtime.InteropServices;
#endif

namespace DotNetty.Buffers
{
    abstract partial class ArrayPooledByteBuffer : AbstractReferenceCountedByteBuffer
    {
        private  ThreadLocalPool.Handle _recyclerHandle;

        protected internal byte[] Memory;
        protected ArrayPooledByteBufferAllocator _allocator;
        private ArrayPool<byte> _arrayPool;
        private int _capacity;

        protected ArrayPooledByteBuffer(ThreadLocalPool.Handle recyclerHandle, int maxCapacity)
            : base(maxCapacity)
        {
            _recyclerHandle = recyclerHandle;
        }

        /// <summary>Method must be called before reuse this {@link ArrayPooledByteBufAllocator}.</summary>
        /// <param name="allocator"></param>
        /// <param name="initialCapacity"></param>
        /// <param name="maxCapacity"></param>
        /// <param name="arrayPool"></param>
        internal void Reuse(ArrayPooledByteBufferAllocator allocator, ArrayPool<byte> arrayPool, int initialCapacity, int maxCapacity)
        {
            _allocator = allocator;
            _arrayPool = arrayPool;

            SetMaxCapacity(maxCapacity);
            SetArray(AllocateArray(initialCapacity), maxCapacity);

            SetReferenceCount(1);
            SetIndex0(0, 0);
            DiscardMarks();
        }

        internal void Reuse(ArrayPooledByteBufferAllocator allocator, ArrayPool<byte> arrayPool, byte[] buffer, int length, int maxCapacity)
        {
            _allocator = allocator;
            _arrayPool = arrayPool;

            SetMaxCapacity(maxCapacity);
            SetArray(buffer, maxCapacity);

            SetReferenceCount(1);
            SetIndex0(0, length);
            DiscardMarks();
        }

        public override int Capacity => _capacity;

        protected virtual byte[] AllocateArray(int initialCapacity) => _arrayPool.Rent(initialCapacity);

        protected virtual void FreeArray(byte[] bytes)
        {
#if DEBUG
            // for unit testing
            try
            {
                _arrayPool.Return(bytes);
            }
            catch { } // 防止回收非 BufferMannager 的 byte array 抛异常
#else
            _arrayPool.Return(bytes);
#endif
        }

        protected void SetArray(byte[] initialArray, int maxCapacity)
        {
            Memory = initialArray;
            _capacity = Math.Min(initialArray.Length, maxCapacity);
        }

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
            byte[] oldArray = Memory;
            byte[] newArray = AllocateArray(newCapacity);
            PlatformDependent.CopyMemory(oldArray, 0, newArray, 0, bytesToCopy);

            SetArray(newArray, MaxCapacity);
            FreeArray(oldArray);
            FreeArray(newArray);//fanly update
            return this;
        }

        public sealed override IByteBufferAllocator Allocator => _allocator;

        public sealed override IByteBuffer Unwrap() => null;

        public sealed override IByteBuffer RetainedDuplicate() => ArrayPooledDuplicatedByteBuffer.NewInstance(this, this, ReaderIndex, WriterIndex);

        public sealed override IByteBuffer RetainedSlice()
        {
            int index = ReaderIndex;
            return RetainedSlice(index, WriterIndex - index);
        }

        public sealed override IByteBuffer RetainedSlice(int index, int length) => ArrayPooledSlicedByteBuffer.NewInstance(this, this, index, length);

        protected internal sealed override void Deallocate()
        {
            var buffer = Memory;
            if (_arrayPool is object & buffer is object)
            {
                FreeArray(buffer);
                FreeArray(Memory);
                _arrayPool = null;
                Memory = null;
               // _recyclerHandle = null;//fanly update
               Recycle();
            }
        }

        void Recycle() => _recyclerHandle.Release(this);

        public sealed override bool IsSingleIoBuffer => true;

        public sealed override int IoBufferCount => 1;

        public sealed override ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            CheckIndex(index, length);
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

        public sealed override int ArrayOffset => 0;

        public sealed override bool HasMemoryAddress => true;

        public sealed override ref byte GetPinnableMemoryAddress()
        {
            EnsureAccessible();
#if NET
            return ref MemoryMarshal.GetArrayDataReference(Memory);
#else
            return ref Memory[0];
#endif
        }

        public sealed override IntPtr AddressOfPinnedMemory() => IntPtr.Zero;

        public sealed override bool IsContiguous => true;
    }
}