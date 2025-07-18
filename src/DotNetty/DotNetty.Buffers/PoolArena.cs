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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    enum SizeClass
    {
        Tiny,
        Small,
        Normal
    }

    abstract class PoolArena<T> : IPoolArenaMetric
    {
        internal const int NumTinySubpagePools = 512 >> 4;

        internal readonly PooledByteBufferAllocator Parent;

        private readonly int _maxOrder;
        internal readonly int PageSize;
        internal readonly int PageShifts;
        internal readonly int ChunkSize;
        internal readonly int SubpageOverflowMask;
        internal readonly int NumSmallSubpagePools;
        private readonly PoolSubpage<T>[] _tinySubpagePools;
        private readonly PoolSubpage<T>[] _smallSubpagePools;

        private readonly PoolChunkList<T> _q050;
        private readonly PoolChunkList<T> _q025;
        private readonly PoolChunkList<T> _q000;
        private readonly PoolChunkList<T> _qInit;
        private readonly PoolChunkList<T> _q075;
        private readonly PoolChunkList<T> _q100;

        private readonly IReadOnlyList<IPoolChunkListMetric> _chunkListMetrics;

        // Metrics for allocations and deallocations
        private long _allocationsNormal;

        // We need to use the LongCounter here as this is not guarded via synchronized block.
        private long _allocationsTiny;

        private long _allocationsSmall;
        private long _allocationsHuge;
        private long _activeBytesHuge;

        private long _deallocationsTiny;
        private long _deallocationsSmall;
        private long _deallocationsNormal;

        // We need to use the LongCounter here as this is not guarded via synchronized block.
        private long _deallocationsHuge;

        // Number of thread caches backed by this arena.
        private int _numThreadCaches;

        // TODO: Test if adding padding helps under contention
        //private long pad0, pad1, pad2, pad3, pad4, pad5, pad6, pad7;

        protected PoolArena(
            PooledByteBufferAllocator parent,
            int pageSize,
            int maxOrder,
            int pageShifts,
            int chunkSize)
        {
            Parent = parent;
            PageSize = pageSize;
            _maxOrder = maxOrder;
            PageShifts = pageShifts;
            ChunkSize = chunkSize;
            SubpageOverflowMask = ~(pageSize - 1);
            _tinySubpagePools = NewSubpagePoolArray(NumTinySubpagePools);
            for (int i = 0; i < _tinySubpagePools.Length; i++)
            {
                _tinySubpagePools[i] = NewSubpagePoolHead(pageSize);
            }

            NumSmallSubpagePools = pageShifts - 9;
            _smallSubpagePools = NewSubpagePoolArray(NumSmallSubpagePools);
            for (int i = 0; i < _smallSubpagePools.Length; i++)
            {
                _smallSubpagePools[i] = NewSubpagePoolHead(pageSize);
            }

            _q100 = new PoolChunkList<T>(this, null, 100, int.MaxValue, chunkSize);
            _q075 = new PoolChunkList<T>(this, _q100, 75, 100, chunkSize);
            _q050 = new PoolChunkList<T>(this, _q075, 50, 100, chunkSize);
            _q025 = new PoolChunkList<T>(this, _q050, 25, 75, chunkSize);
            _q000 = new PoolChunkList<T>(this, _q025, 1, 50, chunkSize);
            _qInit = new PoolChunkList<T>(this, _q000, int.MinValue, 25, chunkSize);

            _q100.PrevList(_q075);
            _q075.PrevList(_q050);
            _q050.PrevList(_q025);
            _q025.PrevList(_q000);
            _q000.PrevList(null);
            _qInit.PrevList(_qInit);

            var metrics = new List<IPoolChunkListMetric>(6);
            metrics.Add(_qInit);
            metrics.Add(_q000);
            metrics.Add(_q025);
            metrics.Add(_q050);
            metrics.Add(_q075);
            metrics.Add(_q100);
            _chunkListMetrics = metrics;
        }

        PoolSubpage<T> NewSubpagePoolHead(int pageSize)
        {
            var head = new PoolSubpage<T>(pageSize);
            head.Prev = head;
            head.Next = head;
            return head;
        }

        PoolSubpage<T>[] NewSubpagePoolArray(int size) => new PoolSubpage<T>[size];

        internal abstract bool IsDirect { get; }

        internal PooledByteBuffer<T> Allocate(PoolThreadCache<T> cache, int reqCapacity, int maxCapacity)
        {
            PooledByteBuffer<T> buf = NewByteBuf(maxCapacity);
            Allocate(cache, buf, reqCapacity);
            return buf;
        }

        internal static int TinyIdx(int normCapacity) => normCapacity.RightUShift(4);

        internal static int SmallIdx(int normCapacity)
        {
            int tableIdx = 0;
            int i = normCapacity.RightUShift(10);
            while (i != 0)
            {
                i = i.RightUShift(1);
                tableIdx++;
            }
            return tableIdx;
        }

        // capacity < pageSize
        internal bool IsTinyOrSmall(int normCapacity) => 0u >= (uint)(normCapacity & SubpageOverflowMask);

        // normCapacity < 512
        internal static bool IsTiny(int normCapacity) => 0u >= (uint)(normCapacity & 0xFFFFFE00);

        void Allocate(PoolThreadCache<T> cache, PooledByteBuffer<T> buf, int reqCapacity)
        {
            int normCapacity = NormalizeCapacity(reqCapacity);
            if (IsTinyOrSmall(normCapacity))
            {
                // capacity < pageSize
                int tableIdx;
                PoolSubpage<T>[] table;
                bool tiny = IsTiny(normCapacity);
                if (tiny)
                {
                    // < 512
                    if (cache.AllocateTiny(this, buf, reqCapacity, normCapacity))
                    {
                        // was able to allocate out of the cache so move on
                        return;
                    }
                    tableIdx = TinyIdx(normCapacity);
                    table = _tinySubpagePools;
                }
                else
                {
                    if (cache.AllocateSmall(this, buf, reqCapacity, normCapacity))
                    {
                        // was able to allocate out of the cache so move on
                        return;
                    }
                    tableIdx = SmallIdx(normCapacity);
                    table = _smallSubpagePools;
                }

                PoolSubpage<T> head = table[tableIdx];

                //
                //  Synchronize on the head. This is needed as {@link PoolSubpage#allocate()} and
                // {@link PoolSubpage#free(int)} may modify the doubly linked list as well.
                // 
                lock (head)
                {
                    PoolSubpage<T> s = head.Next;
                    if (s != head)
                    {
                        Debug.Assert(s.DoNotDestroy && s.ElemSize == normCapacity);
                        long handle = s.Allocate();
                        Debug.Assert(handle >= 0);
                        s.Chunk.InitBufWithSubpage(buf, handle, reqCapacity, cache);
                        IncTinySmallAllocation(tiny);
                        return;
                    }
                }

                lock (this)
                {
                    AllocateNormal(buf, reqCapacity, normCapacity, cache);
                }

                IncTinySmallAllocation(tiny);
                return;
            }
            if (normCapacity <= ChunkSize)
            {
                if (cache.AllocateNormal(this, buf, reqCapacity, normCapacity))
                {
                    // was able to allocate out of the cache so move on
                    return;
                }

                lock (this)
                {
                    AllocateNormal(buf, reqCapacity, normCapacity, cache);
                    _allocationsNormal++;
                }
            }
            else
            {
                // Huge allocations are never served via the cache so just call allocateHuge
                AllocateHuge(buf, reqCapacity);
            }
        }

        void AllocateNormal(PooledByteBuffer<T> buf, int reqCapacity, int normCapacity, PoolThreadCache<T> threadCache)
        {
            if (_q050.Allocate(buf, reqCapacity, normCapacity, threadCache) ||
                _q025.Allocate(buf, reqCapacity, normCapacity, threadCache) ||
                _q000.Allocate(buf, reqCapacity, normCapacity, threadCache) ||
                _qInit.Allocate(buf, reqCapacity, normCapacity, threadCache) ||
                _q075.Allocate(buf, reqCapacity, normCapacity, threadCache))
            {
                return;
            }

            // Add a new chunk.
            PoolChunk<T> c = NewChunk(PageSize, _maxOrder, PageShifts, ChunkSize);
            bool success = c.Allocate(buf, reqCapacity, normCapacity, threadCache);
            Debug.Assert(success);
            _qInit.Add(c);
        }

        void IncTinySmallAllocation(bool tiny)
        {
            if (tiny)
            {
                _ = Interlocked.Increment(ref _allocationsTiny);
            }
            else
            {
                _ = Interlocked.Increment(ref _allocationsSmall);
            }
        }

        void AllocateHuge(PooledByteBuffer<T> buf, int reqCapacity)
        {
            PoolChunk<T> chunk = NewUnpooledChunk(reqCapacity);
            _ = Interlocked.Add(ref _activeBytesHuge, chunk.ChunkSize);
            buf.InitUnpooled(chunk, reqCapacity);
            _ = Interlocked.Increment(ref _allocationsHuge);
        }

        internal void Free(PoolChunk<T> chunk, long handle, int normCapacity, PoolThreadCache<T> cache)
        {
            if (chunk.Unpooled)
            {
                int size = chunk.ChunkSize;
                DestroyChunk(chunk);
                _ = Interlocked.Add(ref _activeBytesHuge, -size);
                _ = Interlocked.Increment(ref _deallocationsHuge);
            }
            else
            {
                SizeClass sizeClass = SizeClass(normCapacity);
                if (cache is object && cache.Add(this, chunk, handle, normCapacity, sizeClass))
                {
                    // cached so not free it.
                    return;
                }

                FreeChunk(chunk, handle, sizeClass, false);
            }
        }

        SizeClass SizeClass(int normCapacity)
        {
            if (!IsTinyOrSmall(normCapacity))
            {
                return Buffers.SizeClass.Normal;
            }

            return IsTiny(normCapacity) ? Buffers.SizeClass.Tiny : Buffers.SizeClass.Small;
        }

        internal void FreeChunk(PoolChunk<T> chunk, long handle, SizeClass sizeClass, bool finalizer)
        {
            bool destroyChunk;
            lock (this)
            {
                if (!finalizer)
                {
                    switch (sizeClass)
                    {
                        case Buffers.SizeClass.Normal:
                            ++_deallocationsNormal;
                            break;
                        case Buffers.SizeClass.Small:
                            ++_deallocationsSmall;
                            break;
                        case Buffers.SizeClass.Tiny:
                            ++_deallocationsTiny;
                            break;
                        default:
                            ThrowHelper.ThrowArgumentOutOfRangeException(); break;
                    }
                }
                destroyChunk = !chunk.Parent.Free(chunk, handle);
            }
            if (destroyChunk)
            {
                // destroyChunk not need to be called while holding the synchronized lock.
                DestroyChunk(chunk);
            }
        }

        internal PoolSubpage<T> FindSubpagePoolHead(int elemSize)
        {
            int tableIdx;
            PoolSubpage<T>[] table;
            if (IsTiny(elemSize))
            {
                // < 512
                tableIdx = TinyIdx(elemSize);
                table = _tinySubpagePools;
            }
            else
            {
                tableIdx = SmallIdx(elemSize);
                table = _smallSubpagePools;
            }

            return table[tableIdx];
        }

        internal int NormalizeCapacity(int reqCapacity)
        {
            uint ureqCapacity = (uint)reqCapacity;
            if (ureqCapacity > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(reqCapacity, ExceptionArgument.reqCapacity); }

            if (ureqCapacity >= (uint)ChunkSize)
            {
                return reqCapacity;
            }

            if (!IsTiny(reqCapacity))
            {
                // >= 512
                // Doubled

                int normalizedCapacity = reqCapacity;
                normalizedCapacity--;
                normalizedCapacity |= normalizedCapacity.RightUShift(1);
                normalizedCapacity |= normalizedCapacity.RightUShift(2);
                normalizedCapacity |= normalizedCapacity.RightUShift(4);
                normalizedCapacity |= normalizedCapacity.RightUShift(8);
                normalizedCapacity |= normalizedCapacity.RightUShift(16);
                normalizedCapacity++;

                if (normalizedCapacity < 0)
                {
                    normalizedCapacity = normalizedCapacity.RightUShift(1);
                }

                return normalizedCapacity;
            }

            // Quantum-spaced
            if (0u >= (uint)(reqCapacity & 15))
            {
                return reqCapacity;
            }

            return (reqCapacity & ~15) + 16;
        }

        internal void Reallocate(PooledByteBuffer<T> buf, int newCapacity, bool freeOldMemory)
        {
            if (/*newCapacity < 0 || */(uint)newCapacity > (uint)buf.MaxCapacity) { ThrowHelper.ThrowIndexOutOfRangeException(); }

            int oldCapacity = buf.Length;
            if (oldCapacity == newCapacity)
            {
                return;
            }

            PoolChunk<T> oldChunk = buf.Chunk;
            long oldHandle = buf.Handle;
            T oldMemory = buf.Memory;
            int oldOffset = buf.Offset;
            int oldMaxLength = buf.MaxLength;

            // This does not touch buf's reader/writer indices
            Allocate(Parent.ThreadCache<T>(), buf, newCapacity);
            int bytesToCopy;
            if (newCapacity > oldCapacity)
            {
                bytesToCopy = oldCapacity;
            }
            else
            {
                buf.TrimIndicesToCapacity(newCapacity);
                bytesToCopy = newCapacity;
            }
            MemoryCopy(oldMemory, oldOffset, buf.Memory, buf.Offset, bytesToCopy);

            if (freeOldMemory)
            {
                Free(oldChunk, oldHandle, oldMaxLength, buf.Cache);
            }
        }

        internal void IncrementNumThreadCaches() => Interlocked.Increment(ref _numThreadCaches);

        internal void DecrementNumThreadCaches() => Interlocked.Decrement(ref _numThreadCaches);

        public int NumThreadCaches => Volatile.Read(ref _numThreadCaches);

        public int NumTinySubpages => _tinySubpagePools.Length;

        public int NumSmallSubpages => _smallSubpagePools.Length;

        public int NumChunkLists => _chunkListMetrics.Count;

        public IReadOnlyList<IPoolSubpageMetric> TinySubpages => SubPageMetricList(_tinySubpagePools);

        public IReadOnlyList<IPoolSubpageMetric> SmallSubpages => SubPageMetricList(_smallSubpagePools);

        public IReadOnlyList<IPoolChunkListMetric> ChunkLists => _chunkListMetrics;

        static List<IPoolSubpageMetric> SubPageMetricList(PoolSubpage<T>[] pages)
        {
            var metrics = new List<IPoolSubpageMetric>();
            foreach (PoolSubpage<T> head in pages)
            {
                if (head.Next == head)
                {
                    continue;
                }
                PoolSubpage<T> s = head.Next;
                while (true)
                {
                    metrics.Add(s);
                    s = s.Next;
                    if (s == head)
                    {
                        break;
                    }
                }
            }
            return metrics;
        }

        public long NumAllocations
        {
            get
            {
                long allocsNormal;
                lock (this)
                {
                    allocsNormal = _allocationsNormal;
                }

                return NumTinyAllocations + NumSmallAllocations + allocsNormal + NumHugeAllocations;
            }
        }

        public long NumTinyAllocations => Volatile.Read(ref _allocationsTiny);

        public long NumSmallAllocations => Volatile.Read(ref _allocationsSmall);

        public long NumNormalAllocations
        {
            get
            {
                lock (this)
                {
                    return _allocationsNormal;
                }
            }
        }

        public long NumDeallocations
        {
            get
            {
                long deallocs;
                lock (this)
                {
                    deallocs = _deallocationsTiny + _deallocationsSmall + _deallocationsNormal;
                }

                return deallocs + NumHugeDeallocations;
            }
        }

        public long NumTinyDeallocations
        {
            get
            {
                lock (this)
                {
                    return _deallocationsTiny;
                }
            }
        }

        public long NumSmallDeallocations
        {
            get
            {
                lock (this)
                {
                    return _deallocationsSmall;
                }
            }
        }

        public long NumNormalDeallocations
        {
            get
            {
                lock (this)
                {
                    return _deallocationsNormal;
                }
            }
        }

        public long NumHugeAllocations => Volatile.Read(ref _allocationsHuge);

        public long NumHugeDeallocations => Volatile.Read(ref _deallocationsHuge);

        public long NumActiveAllocations
        {
            get
            {
                long val = NumTinyAllocations + NumSmallAllocations + NumHugeAllocations
                    - NumHugeDeallocations;
                lock (this)
                {
                    val += _allocationsNormal - (_deallocationsTiny + _deallocationsSmall + _deallocationsNormal);
                }
                return Math.Max(val, 0);
            }
        }

        public long NumActiveTinyAllocations => Math.Max(NumTinyAllocations - NumTinyDeallocations, 0);

        public long NumActiveSmallAllocations => Math.Max(NumSmallAllocations - NumSmallDeallocations, 0);

        public long NumActiveNormalAllocations
        {
            get
            {
                long val;
                lock (this)
                {
                    val = _allocationsNormal - _deallocationsNormal;
                }
                return Math.Max(val, 0);
            }
        }

        public long NumActiveHugeAllocations => Math.Max(NumHugeAllocations - NumHugeDeallocations, 0);

        public long NumActiveBytes
        {
            get
            {
                long val = Volatile.Read(ref _activeBytesHuge);
                lock (this)
                {
                    for (int i = 0; i < _chunkListMetrics.Count; i++)
                    {
                        var metrics = _chunkListMetrics[i];
                        foreach (IPoolChunkMetric m in metrics)
                        {
                            val += m.ChunkSize;
                        }
                    }
                }
                return Math.Max(0, val);
            }
        }

        protected abstract PoolChunk<T> NewChunk(int pageSize, int maxOrder, int pageShifts, int chunkSize);

        protected abstract PoolChunk<T> NewUnpooledChunk(int capacity);

        protected abstract PooledByteBuffer<T> NewByteBuf(int maxCapacity);

        protected abstract void MemoryCopy(T src, int srcOffset, T dst, int dstOffset, int length);

        protected internal abstract void DestroyChunk(PoolChunk<T> chunk);

        public override string ToString()
        {
            lock (this)
            {
                var buf = StringBuilderManager.Allocate()
                    .Append("Chunk(s) at 0~25%:")
                    .Append(StringUtil.Newline)
                    .Append(_qInit)
                    .Append(StringUtil.Newline)
                    .Append("Chunk(s) at 0~50%:")
                    .Append(StringUtil.Newline)
                    .Append(_q000)
                    .Append(StringUtil.Newline)
                    .Append("Chunk(s) at 25~75%:")
                    .Append(StringUtil.Newline)
                    .Append(_q025)
                    .Append(StringUtil.Newline)
                    .Append("Chunk(s) at 50~100%:")
                    .Append(StringUtil.Newline)
                    .Append(_q050)
                    .Append(StringUtil.Newline)
                    .Append("Chunk(s) at 75~100%:")
                    .Append(StringUtil.Newline)
                    .Append(_q075)
                    .Append(StringUtil.Newline)
                    .Append("Chunk(s) at 100%:")
                    .Append(StringUtil.Newline)
                    .Append(_q100)
                    .Append(StringUtil.Newline)
                    .Append("tiny subpages:");
                AppendPoolSubPages(buf, _tinySubpagePools);
                _ = buf.Append(StringUtil.Newline)
                    .Append("small subpages:");
                AppendPoolSubPages(buf, _smallSubpagePools);
                _ = buf.Append(StringUtil.Newline);

                return StringBuilderManager.ReturnAndFree(buf);
            }
        }

        static void AppendPoolSubPages(StringBuilder buf, PoolSubpage<T>[] subpages)
        {
            for (int i = 0; i < subpages.Length; i++)
            {
                PoolSubpage<T> head = subpages[i];
                if (head.Next == head)
                {
                    continue;
                }

                _ = buf.Append(StringUtil.Newline)
                    .Append(i)
                    .Append(": ");
                PoolSubpage<T> s = head.Next;
                while (true)
                {
                    _ = buf.Append(s);
                    s = s.Next;
                    if (s == head)
                    {
                        break;
                    }
                }
            }
        }

        ~PoolArena()
        {
            DestroyPoolSubPages(_smallSubpagePools);
            DestroyPoolSubPages(_tinySubpagePools);
            DestroyPoolChunkLists(_qInit, _q000, _q025, _q050, _q075, _q100);
        }

        static void DestroyPoolSubPages(PoolSubpage<T>[] pages)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].Destroy();
            }
        }

        void DestroyPoolChunkLists(params PoolChunkList<T>[] chunkLists)
        {
            for (int i = 0; i < chunkLists.Length; i++)
            {
                chunkLists[i].Destroy(this);
            }
        }
    }

    sealed class HeapArena : PoolArena<byte[]>
    {
        public HeapArena(PooledByteBufferAllocator parent, int pageSize, int maxOrder, int pageShifts, int chunkSize)
            : base(parent, pageSize, maxOrder, pageShifts, chunkSize)
        {
        }

        static byte[] NewByteArray(int size) => new byte[size];

        internal override bool IsDirect => false;

        protected override PoolChunk<byte[]> NewChunk(int pageSize, int maxOrder, int pageShifts, int chunkSize) =>
            new PoolChunk<byte[]>(this, NewByteArray(chunkSize), pageSize, maxOrder, pageShifts, chunkSize, 0, IntPtr.Zero);

        protected override PoolChunk<byte[]> NewUnpooledChunk(int capacity) =>
            new PoolChunk<byte[]>(this, NewByteArray(capacity), capacity, 0, IntPtr.Zero);

        protected internal override void DestroyChunk(PoolChunk<byte[]> chunk)
        {
            // Rely on GC.
        }

        protected override PooledByteBuffer<byte[]> NewByteBuf(int maxCapacity) =>
            PooledHeapByteBuffer.NewInstance(maxCapacity);

        protected override void MemoryCopy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int length)
        {
            PlatformDependent.CopyMemory(src, srcOffset, dst, dstOffset, length);
        }
    }

    // TODO: Maybe use Memory or OwnedMemory as direct arena/byte buffer type parameter in NETStandard 2.0
    // 鉴于几个方面考虑还是暂时不需要包装 IMemoryOwner<byte> 或 MemoryManager<byte>
    // 1、IByteBuffer直接操作数组性能更高，参考 System.IO.Pipelines 和 System.Buffers 的内部实现
    // 2、IByetBuffer实现 IReferenceCounted 接口，IMemoryOwner的管理会更加混乱
    // 3、现在 IByteBuffer 已经实现了 IBufferWriter<byte> 接口
    sealed class DirectArena : PoolArena<byte[]>
    {
        private readonly List<MemoryChunk> _memoryChunks;

        public DirectArena(PooledByteBufferAllocator parent, int pageSize, int maxOrder, int pageShifts, int chunkSize)
            : base(parent, pageSize, maxOrder, pageShifts, chunkSize)
        {
            _memoryChunks = new List<MemoryChunk>();
        }

        static MemoryChunk NewMemoryChunk(int size) => new MemoryChunk(size);

        internal override bool IsDirect => true;

        protected override PoolChunk<byte[]> NewChunk(int pageSize, int maxOrder, int pageShifts, int chunkSize)
        {
            MemoryChunk memoryChunk = NewMemoryChunk(chunkSize);
            _memoryChunks.Add(memoryChunk);
            var chunk = new PoolChunk<byte[]>(this, memoryChunk.Bytes, pageSize, maxOrder, pageShifts, chunkSize, 0, memoryChunk.NativePointer);
            return chunk;
        }

        protected override PoolChunk<byte[]> NewUnpooledChunk(int capacity)
        {
            MemoryChunk memoryChunk = NewMemoryChunk(capacity);
            _memoryChunks.Add(memoryChunk);
            var chunk = new PoolChunk<byte[]>(this, memoryChunk.Bytes, capacity, 0, memoryChunk.NativePointer);
            return chunk;
        }

        protected override PooledByteBuffer<byte[]> NewByteBuf(int maxCapacity) =>
            PooledUnsafeDirectByteBuffer.NewInstance(maxCapacity);

        protected override unsafe void MemoryCopy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int length) =>
            PlatformDependent.CopyMemory((byte*)Unsafe.AsPointer(ref src[srcOffset]), (byte*)Unsafe.AsPointer(ref dst[dstOffset]), length);

        protected internal override void DestroyChunk(PoolChunk<byte[]> chunk)
        {
            for (int i = 0; i < _memoryChunks.Count; i++)
            {
                MemoryChunk memoryChunk = _memoryChunks[i];
                if (ReferenceEquals(chunk.Memory, memoryChunk.Bytes))
                {
                    _ = _memoryChunks.Remove(memoryChunk);
                    memoryChunk.Dispose();
                    break;
                }
            }
        }

        sealed class MemoryChunk : IDisposable
        {
            internal byte[] Bytes;
            private GCHandle _handle;
            internal IntPtr NativePointer;

            internal MemoryChunk(int size)
            {
                Bytes = new byte[size];
                _handle = GCHandle.Alloc(Bytes, GCHandleType.Pinned);
                NativePointer = _handle.AddrOfPinnedObject();
            }

            void Release()
            {
                if (_handle.IsAllocated)
                {
                    try
                    {
                        _handle.Free();
                    }
                    catch (InvalidOperationException)
                    {
                        // Free is not thread safe
                    }
                }
                NativePointer = IntPtr.Zero;
                Bytes = null;
            }

            public void Dispose()
            {
                Release();
                GC.SuppressFinalize(this);
            }

            ~MemoryChunk()
            {
                Release();
            }
        }

        sealed class OwnedPinnedBlock : MemoryManager<byte>, IPoolMemoryOwner<byte>
        {
            private byte[] _array;
            private IntPtr _origin;
            private readonly int _offset;
            private readonly int _length;

            private volatile int _disposed;

            public unsafe OwnedPinnedBlock(byte[] array, void* origin, int offset, int length)
            {
                _array = array;
                _origin = new IntPtr(Unsafe.Add<byte>(origin, offset));
                _offset = offset;
                _length = length;
            }

            public IntPtr Origin => _origin;

            public byte[] Array => _array;

            public int Offset => _offset;

            public int Length => _length;

            protected override bool TryGetArray(out ArraySegment<byte> segment)
            {
                segment = new ArraySegment<byte>(_array, _offset, _length);
                return true;
            }

            public unsafe override Span<byte> GetSpan()
            {
                if (IsDisposed) { ThrowObjectDisposedException(); }
                return new Span<byte>(_origin.ToPointer(), _length);
            }

            public unsafe override MemoryHandle Pin(int elementIndex = 0)
            {
                if (IsDisposed) { ThrowObjectDisposedException(); }
                if (elementIndex != 0 && ((uint)elementIndex - 1) >= (uint)_length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.elementIndex);
                }
                return new MemoryHandle(Unsafe.Add<byte>(_origin.ToPointer(), elementIndex), default, this);
            }

            public override void Unpin()
            {
                // no-op
            }

            public bool IsDisposed => SharedConstants.False < (uint)_disposed;

            protected override void Dispose(bool disposing)
            {
                if (SharedConstants.False < (uint)Interlocked.Exchange(ref _disposed, SharedConstants.True)) { return; }

                _array = null;
                _origin = IntPtr.Zero;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowObjectDisposedException()
            {
                throw GetObjectDisposedException();

                static ObjectDisposedException GetObjectDisposedException()
                {
                    return new ObjectDisposedException(nameof(OwnedPinnedBlock));
                }
            }
        }
    }
}