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

using System.Threading;
using DotNetty.Common;
using DotNetty.Common.Internal;
using DotNetty.Common.Utilities;

namespace DotNetty.Buffers
{
    /// <summary>
    ///     Unpooled implementation of <see cref="IByteBufferAllocator" />.
    /// </summary>
    public sealed class ArrayPooledByteBufferAllocator : AbstractByteBufferAllocator, IByteBufferAllocatorMetricProvider
    {
        private readonly ArrayPooledByteBufferAllocatorMetric _metric = new ArrayPooledByteBufferAllocatorMetric();
        private readonly bool _disableLeakDetector;

        public static readonly ArrayPooledByteBufferAllocator Default =
            new ArrayPooledByteBufferAllocator(PlatformDependent.DirectBufferPreferred);

        public ArrayPooledByteBufferAllocator()
            : this(false, false)
        {
        }

        public unsafe ArrayPooledByteBufferAllocator(bool preferDirect)
            : this(preferDirect, false)
        {
        }

        public unsafe ArrayPooledByteBufferAllocator(bool preferDirect, bool disableLeakDetector)
            : base(preferDirect)
        {
            _disableLeakDetector = disableLeakDetector;
        }

        protected override IByteBuffer NewHeapBuffer(int initialCapacity, int maxCapacity)
            => InstrumentedArrayPooledHeapByteBuffer.Create(this, initialCapacity, maxCapacity);

        protected unsafe override IByteBuffer NewDirectBuffer(int initialCapacity, int maxCapacity)
            => InstrumentedArrayPooledUnsafeDirectByteBuffer.Create(this, initialCapacity, maxCapacity);

        public override CompositeByteBuffer CompositeHeapBuffer(int maxNumComponents)
        {
            var buf = new CompositeByteBuffer(this, false, maxNumComponents);
            return _disableLeakDetector ? buf : ToLeakAwareBuffer(buf);
        }

        public unsafe override CompositeByteBuffer CompositeDirectBuffer(int maxNumComponents)
        {
            var buf = new CompositeByteBuffer(this, true, maxNumComponents);
            return _disableLeakDetector ? buf : ToLeakAwareBuffer(buf);
        }

        public override bool IsDirectBufferPooled => true;

        public IByteBufferAllocatorMetric Metric => _metric;

        internal void IncrementDirect(int amount) => _metric.DirectCounter(amount);

        internal void DecrementDirect(int amount) => _metric.DirectCounter(-amount);

        internal void IncrementHeap(int amount) => _metric.HeapCounter(amount);

        internal void DecrementHeap(int amount) => _metric.HeapCounter(-amount);
    }

    sealed class InstrumentedArrayPooledHeapByteBuffer : ArrayPooledHeapByteBuffer
    {
        static readonly ThreadLocalPool<InstrumentedArrayPooledHeapByteBuffer> s_recycler = new ThreadLocalPool<InstrumentedArrayPooledHeapByteBuffer>(handle => new InstrumentedArrayPooledHeapByteBuffer(handle, 0));

        internal static InstrumentedArrayPooledHeapByteBuffer Create(ArrayPooledByteBufferAllocator allocator, int initialCapacity, int maxCapacity)
        {
            var buf = s_recycler.Take();
            buf.Reuse(allocator, ArrayPooled.DefaultArrayPool, initialCapacity, maxCapacity);
            return buf;
        }

        internal InstrumentedArrayPooledHeapByteBuffer(ThreadLocalPool.Handle recyclerHandle, int maxCapacity)
            : base(recyclerHandle, maxCapacity)
        {
        }

        protected override byte[] AllocateArray(int initialCapacity)
        {
            byte[] bytes = base.AllocateArray(initialCapacity);
            _allocator.IncrementHeap(bytes.Length);
            return bytes;
        }

        protected override void FreeArray(byte[] bytes)
        {
            int length = bytes.Length;
            base.FreeArray(bytes);
            _allocator.DecrementHeap(length);
        }
    }

    sealed class InstrumentedArrayPooledUnsafeDirectByteBuffer : ArrayPooledUnsafeDirectByteBuffer
    {
        static readonly ThreadLocalPool<InstrumentedArrayPooledUnsafeDirectByteBuffer> s_recycler = new ThreadLocalPool<InstrumentedArrayPooledUnsafeDirectByteBuffer>(handle => new InstrumentedArrayPooledUnsafeDirectByteBuffer(handle, 0));

        internal static InstrumentedArrayPooledUnsafeDirectByteBuffer Create(ArrayPooledByteBufferAllocator allocator, int initialCapacity, int maxCapacity)
        {
            var buf = s_recycler.Take();
            buf.Reuse(allocator, ArrayPooled.DefaultArrayPool, initialCapacity, maxCapacity);
            return buf;
        }

        internal InstrumentedArrayPooledUnsafeDirectByteBuffer(ThreadLocalPool.Handle recyclerHandle, int maxCapacity)
            : base(recyclerHandle, maxCapacity)
        {
        }

        protected override byte[] AllocateArray(int initialCapacity)
        {
            byte[] bytes = base.AllocateArray(initialCapacity);
            _allocator.IncrementDirect(bytes.Length);
            return bytes;
        }

        protected override void FreeArray(byte[] array)
        {
            int capacity = array.Length;
            base.FreeArray(array);
            _allocator.DecrementDirect(capacity);
        }
    }

    sealed class ArrayPooledByteBufferAllocatorMetric : IByteBufferAllocatorMetric
    {
        long usedHeapMemory;
        long userDirectMemory;

        public long UsedHeapMemory => Volatile.Read(ref usedHeapMemory);

        public long UsedDirectMemory => Volatile.Read(ref userDirectMemory);

        public void HeapCounter(int amount) => Interlocked.Add(ref usedHeapMemory, amount);

        public void DirectCounter(int amount) => Interlocked.Add(ref userDirectMemory, amount);

        public override string ToString() => $"{StringUtil.SimpleClassName(this)} (usedHeapMemory: {UsedHeapMemory}; usedDirectMemory: {UsedDirectMemory})";
    }
}