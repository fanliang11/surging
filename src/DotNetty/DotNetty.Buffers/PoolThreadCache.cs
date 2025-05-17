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
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using Thread = DotNetty.Common.Concurrency.XThread;

    /// <summary>
    /// Acts a Thread cache for allocations. This implementation is moduled after
    /// <a href="http://people.freebsd.org/~jasone/jemalloc/bsdcan2006/jemalloc.pdf">jemalloc</a> and the descripted
    /// technics of
    /// <a href="https://www.facebook.com/notes/facebook-engineering/scalable-memory-allocation-using-jemalloc/480222803919">
    /// Scalable memory allocation using jemalloc</a>.
    /// </summary>
    sealed class PoolThreadCache<T>
    {
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<PoolThreadCache<T>>();
        private static readonly int s_integerSizeMinusOne = IntegerExtensions.SizeInBits - 1;

        internal readonly PoolArena<T> HeapArena;
        internal readonly PoolArena<T> DirectArena;

        // Hold the caches for the different size classes, which are tiny, small and normal.
        private readonly MemoryRegionCache[] tinySubPageHeapCaches;
        private readonly MemoryRegionCache[] smallSubPageHeapCaches;
        private readonly MemoryRegionCache[] tinySubPageDirectCaches;
        private readonly MemoryRegionCache[] smallSubPageDirectCaches;
        private readonly MemoryRegionCache[] normalHeapCaches;
        private readonly MemoryRegionCache[] normalDirectCaches;

        // Used for bitshifting when calculate the index of normal caches later
        private readonly int _numShiftsNormalDirect;
        private readonly int _numShiftsNormalHeap;
        private readonly int _freeSweepAllocationThreshold;

        //int freed = SharedConstants.False; // TODO
        private int _allocations;

        private readonly Thread _deathWatchThread;
        private readonly Action _freeTask;

        // TODO: Test if adding padding helps under contention
        //private long pad0, pad1, pad2, pad3, pad4, pad5, pad6, pad7;

        internal PoolThreadCache(PoolArena<T> heapArena, PoolArena<T> directArena,
            int tinyCacheSize, int smallCacheSize, int normalCacheSize,
            int maxCachedBufferCapacity, int freeSweepAllocationThreshold)
        {
            if ((uint)maxCachedBufferCapacity > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(maxCachedBufferCapacity, ExceptionArgument.maxCachedBufferCapacity); }

            _freeSweepAllocationThreshold = freeSweepAllocationThreshold;
            HeapArena = heapArena;
            DirectArena = directArena;
            if (directArena is object)
            {
                tinySubPageDirectCaches = CreateSubPageCaches(
                    tinyCacheSize, PoolArena<T>.NumTinySubpagePools, SizeClass.Tiny);
                smallSubPageDirectCaches = CreateSubPageCaches(
                    smallCacheSize, directArena.NumSmallSubpagePools, SizeClass.Small);

                _numShiftsNormalDirect = Log2(directArena.PageSize);
                normalDirectCaches = CreateNormalCaches(
                    normalCacheSize, maxCachedBufferCapacity, directArena);

                directArena.IncrementNumThreadCaches();
            }
            else
            {
                // No directArea is configured so just null out all caches
                tinySubPageDirectCaches = null;
                smallSubPageDirectCaches = null;
                normalDirectCaches = null;
                _numShiftsNormalDirect = -1;
            }
            if (heapArena is object)
            {
                // Create the caches for the heap allocations
                tinySubPageHeapCaches = CreateSubPageCaches(
                    tinyCacheSize, PoolArena<T>.NumTinySubpagePools, SizeClass.Tiny);
                smallSubPageHeapCaches = CreateSubPageCaches(
                    smallCacheSize, heapArena.NumSmallSubpagePools, SizeClass.Small);

                _numShiftsNormalHeap = Log2(heapArena.PageSize);
                normalHeapCaches = CreateNormalCaches(
                    normalCacheSize, maxCachedBufferCapacity, heapArena);

                heapArena.IncrementNumThreadCaches();
            }
            else
            {
                // No heapArea is configured so just null out all caches
                tinySubPageHeapCaches = null;
                smallSubPageHeapCaches = null;
                normalHeapCaches = null;
                _numShiftsNormalHeap = -1;
            }

            // We only need to watch the thread when any cache is used.
            if (tinySubPageDirectCaches is object || smallSubPageDirectCaches is object || normalDirectCaches is object
                || tinySubPageHeapCaches is object || smallSubPageHeapCaches is object || normalHeapCaches is object)
            {
                if (freeSweepAllocationThreshold < 1) { ThrowHelper.ThrowArgumentException_Positive(freeSweepAllocationThreshold, ExceptionArgument.freeSweepAllocationThreshold); }
                _freeTask = Free0;
                _deathWatchThread = Thread.CurrentThread;

                // The thread-local cache will keep a list of pooled buffers which must be returned to
                // the pool when the thread is not alive anymore.
                ThreadDeathWatcher.Watch(_deathWatchThread, _freeTask);
            }
            else
            {
                _freeTask = null;
                _deathWatchThread = null;
            }
        }

        static MemoryRegionCache[] CreateSubPageCaches(
            int cacheSize, int numCaches, SizeClass sizeClass)
        {
            if (cacheSize > 0 && numCaches > 0)
            {
                var cache = new MemoryRegionCache[numCaches];
                for (int i = 0; i < cache.Length; i++)
                {
                    // TODO: maybe use cacheSize / cache.length
                    cache[i] = new SubPageMemoryRegionCache(cacheSize, sizeClass);
                }
                return cache;
            }
            else
            {
                return null;
            }
        }

        static MemoryRegionCache[] CreateNormalCaches(
            int cacheSize, int maxCachedBufferCapacity, PoolArena<T> area)
        {
            if (cacheSize > 0 && maxCachedBufferCapacity > 0)
            {
                int max = Math.Min(area.ChunkSize, maxCachedBufferCapacity);
                int arraySize = Math.Max(1, Log2(max / area.PageSize) + 1);

                var cache = new MemoryRegionCache[arraySize];
                for (int i = 0; i < cache.Length; i++)
                {
                    cache[i] = new NormalMemoryRegionCache(cacheSize);
                }
                return cache;
            }
            else
            {
                return null;
            }
        }

        // val > 0
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private static int Log2(int val)
        {
            return s_integerSizeMinusOne - IntegerExtensions.NumberOfLeadingZeros(val);
        }

        /// <summary>
        /// Try to allocate a tiny buffer out of the cache.
        /// </summary>
        /// <returns><c>true</c> if successful <c>false</c> otherwise</returns>
        internal bool AllocateTiny(PoolArena<T> area, PooledByteBuffer<T> buf, int reqCapacity, int normCapacity) =>
            Allocate(CacheForTiny(area, normCapacity), buf, reqCapacity);

        /// <summary>
        /// Try to allocate a small buffer out of the cache.
        /// </summary>
        /// <returns><c>true</c> if successful <c>false</c> otherwise</returns>
        internal bool AllocateSmall(PoolArena<T> area, PooledByteBuffer<T> buf, int reqCapacity, int normCapacity) =>
            Allocate(CacheForSmall(area, normCapacity), buf, reqCapacity);

        /// <summary>
        /// Try to allocate a small buffer out of the cache
        /// </summary>
        /// <returns><c>true</c> if successful <c>false</c> otherwise</returns>
        internal bool AllocateNormal(PoolArena<T> area, PooledByteBuffer<T> buf, int reqCapacity, int normCapacity) =>
            Allocate(CacheForNormal(area, normCapacity), buf, reqCapacity);

        bool Allocate(MemoryRegionCache cache, PooledByteBuffer<T> buf, int reqCapacity)
        {
            if (cache is null)
            {
                // no cache found so just return false here
                return false;
            }
            bool allocated = cache.Allocate(buf, reqCapacity, this);
            if (++_allocations >= _freeSweepAllocationThreshold)
            {
                _allocations = 0;
                Trim();
            }
            return allocated;
        }

        /// <summary>
        /// Add <see cref="PoolChunk{T}"/> and <paramref name="handle"/> to the cache if there is enough room.
        /// </summary>
        /// <returns><c>true</c> if it fit into the cache <c>false</c> otherwise.</returns>
        internal bool Add(PoolArena<T> area, PoolChunk<T> chunk, long handle, int normCapacity, SizeClass sizeClass)
        {
            MemoryRegionCache cache = Cache(area, normCapacity, sizeClass);
            if (cache is null)
            {
                return false;
            }
            return cache.Add(chunk, handle);
        }

        MemoryRegionCache Cache(PoolArena<T> area, int normCapacity, SizeClass sizeClass)
        {
            switch (sizeClass)
            {
                case SizeClass.Normal:
                    return CacheForNormal(area, normCapacity);
                case SizeClass.Small:
                    return CacheForSmall(area, normCapacity);
                case SizeClass.Tiny:
                    return CacheForTiny(area, normCapacity);
                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(); return default;
            }
        }

        /// <summary>
        /// Should be called if the Thread that uses this cache is about to exist to release resources out of the cache
        /// </summary>
        internal void Free()
        {
            if (_freeTask is object)
            {
                Debug.Assert(_deathWatchThread is object);
                ThreadDeathWatcher.Unwatch(_deathWatchThread, _freeTask);
            }

            Free0();
        }

        void Free0()
        {
            int numFreed = Free(tinySubPageDirectCaches) +
                Free(smallSubPageDirectCaches) +
                Free(normalDirectCaches) +
                Free(tinySubPageHeapCaches) +
                Free(smallSubPageHeapCaches) +
                Free(normalHeapCaches);

            if (numFreed > 0 && Logger.DebugEnabled)
            {
                Logger.FreedThreadLocalBufferFromThread(numFreed, _deathWatchThread);
            }

            DirectArena?.DecrementNumThreadCaches();
            HeapArena?.DecrementNumThreadCaches();
        }

        static int Free(MemoryRegionCache[] caches)
        {
            if (caches is null)
            {
                return 0;
            }

            int numFreed = 0;
            foreach (MemoryRegionCache c in caches)
            {
                numFreed += Free(c);
            }
            return numFreed;
        }

        static int Free(MemoryRegionCache cache)
        {
            if (cache is null)
            {
                return 0;
            }
            return cache.Free();
        }

        internal void Trim()
        {
            Trim(tinySubPageDirectCaches);
            Trim(smallSubPageDirectCaches);
            Trim(normalDirectCaches);
            Trim(tinySubPageHeapCaches);
            Trim(smallSubPageHeapCaches);
            Trim(normalHeapCaches);
        }

        static void Trim(MemoryRegionCache[] caches)
        {
            if (caches is null)
            {
                return;
            }
            foreach (MemoryRegionCache c in caches)
            {
                Trim(c);
            }
        }

        static void Trim(MemoryRegionCache cache) => cache?.Trim();

        MemoryRegionCache CacheForTiny(PoolArena<T> area, int normCapacity)
        {
            int idx = PoolArena<T>.TinyIdx(normCapacity);
            return Cache(area.IsDirect ? tinySubPageDirectCaches : tinySubPageHeapCaches, idx);
        }

        MemoryRegionCache CacheForSmall(PoolArena<T> area, int normCapacity)
        {
            int idx = PoolArena<T>.SmallIdx(normCapacity);
            return Cache(area.IsDirect ? smallSubPageDirectCaches : smallSubPageHeapCaches, idx);
        }

        MemoryRegionCache CacheForNormal(PoolArena<T> area, int normCapacity)
        {
            if (area.IsDirect)
            {
                int idx = Log2(normCapacity >> _numShiftsNormalDirect);
                return Cache(normalDirectCaches, idx);
            }
            int idx1 = Log2(normCapacity >> _numShiftsNormalHeap);
            return Cache(normalHeapCaches, idx1);
        }

        static MemoryRegionCache Cache(MemoryRegionCache[] cache, int idx)
        {
            if (cache is null || idx > cache.Length - 1)
            {
                return null;
            }
            return cache[idx];
        }

        /// <summary>
        /// Cache used for buffers which are backed by TINY or SMALL size.
        /// </summary>
        sealed class SubPageMemoryRegionCache : MemoryRegionCache
        {
            internal SubPageMemoryRegionCache(int size, SizeClass sizeClass)
                : base(size, sizeClass)
            {
            }

            protected override void InitBuf(
                PoolChunk<T> chunk, long handle, PooledByteBuffer<T> buf, int reqCapacity, PoolThreadCache<T> threadCache) =>
                chunk.InitBufWithSubpage(buf, handle, reqCapacity, threadCache);
        }

        /// <summary>
        /// Cache used for buffers which are backed by NORMAL size.
        /// </summary>
        sealed class NormalMemoryRegionCache : MemoryRegionCache
        {
            internal NormalMemoryRegionCache(int size)
                : base(size, SizeClass.Normal)
            {
            }

            protected override void InitBuf(
                PoolChunk<T> chunk, long handle, PooledByteBuffer<T> buf, int reqCapacity, PoolThreadCache<T> threadCache) =>
                chunk.InitBuf(buf, handle, reqCapacity, threadCache);
        }

        abstract class MemoryRegionCache
        {
            readonly int _size;
            readonly IQueue<Entry> _queue;
            readonly SizeClass _sizeClass;
            int _allocations;

            protected MemoryRegionCache(int size, SizeClass sizeClass)
            {
                _size = MathUtil.SafeFindNextPositivePowerOfTwo(size);
                _queue = PlatformDependent.NewFixedMpscQueue<Entry>(_size);
                _sizeClass = sizeClass;
            }

            /// <summary>
            /// Init the <see cref="PooledByteBuffer{T}"/> using the provided chunk and handle with the capacity restrictions.
            /// </summary>
            protected abstract void InitBuf(PoolChunk<T> chunk, long handle,
                PooledByteBuffer<T> buf, int reqCapacity, PoolThreadCache<T> threadCache);

            /// <summary>
            /// Add to cache if not already full.
            /// </summary>
            public bool Add(PoolChunk<T> chunk, long handle)
            {
                Entry entry = NewEntry(chunk, handle);
                bool queued = _queue.TryEnqueue(entry);
                if (!queued)
                {
                    // If it was not possible to cache the chunk, immediately recycle the entry
                    entry.Recycle();
                }

                return queued;
            }

            /// <summary>
            /// Allocate something out of the cache if possible and remove the entry from the cache.
            /// </summary>
            public bool Allocate(PooledByteBuffer<T> buf, int reqCapacity, PoolThreadCache<T> threadCache)
            {
                if (!_queue.TryDequeue(out Entry entry))
                {
                    return false;
                }
                InitBuf(entry.Chunk, entry.Handle, buf, reqCapacity, threadCache);
                entry.Recycle();

                // allocations is not thread-safe which is fine as this is only called from the same thread all time.
                ++_allocations;
                return true;
            }

            /// <summary>
            /// Clear out this cache and free up all previous cached <see cref="PoolChunk{T}"/>s and {@code handle}s.
            /// </summary>
            public int Free() => Free(int.MaxValue);

            int Free(int max)
            {
                int numFreed = 0;
                for (; numFreed < max; numFreed++)
                {
                    if (_queue.TryDequeue(out Entry entry))
                    {
                        FreeEntry(entry);
                    }
                    else
                    {
                        // all cleared
                        return numFreed;
                    }
                }
                return numFreed;
            }

            /// <summary>
            /// Free up cached <see cref="PoolChunk{T}"/>s if not allocated frequently enough.
            /// </summary>
            public void Trim()
            {
                int toFree = _size - _allocations;
                _allocations = 0;

                // We not even allocated all the number that are
                if (toFree > 0)
                {
                    _ = Free(toFree);
                }
            }

            void FreeEntry(Entry entry)
            {
                PoolChunk<T> chunk = entry.Chunk;
                long handle = entry.Handle;

                // recycle now so PoolChunk can be GC'ed.
                entry.Recycle();

                chunk.Arena.FreeChunk(chunk, handle, _sizeClass, false);
            }

            sealed class Entry
            {
                readonly ThreadLocalPool.Handle _recyclerHandle;
                public PoolChunk<T> Chunk;
                public long Handle = -1;

                public Entry(ThreadLocalPool.Handle recyclerHandle)
                {
                    _recyclerHandle = recyclerHandle;
                }

                internal void Recycle()
                {
                    Chunk = null;
                    Handle = -1;
                    _recyclerHandle.Release(this);
                }
            }

            static Entry NewEntry(PoolChunk<T> chunk, long handle)
            {
                Entry entry = Recycler.Take();
                entry.Chunk = chunk;
                entry.Handle = handle;
                return entry;
            }

            static readonly ThreadLocalPool<Entry> Recycler = new ThreadLocalPool<Entry>(handle => new Entry(handle));
        }
    }
}