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
    using System.Collections.Generic;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;

    public class PooledByteBufferAllocator : AbstractByteBufferAllocator, IByteBufferAllocatorMetricProvider
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<PooledByteBufferAllocator>();

        public static readonly int DefaultNumHeapArena;
        public static readonly int DefaultNumDirectArena;

        public static readonly int DefaultPageSize;
        public static readonly int DefaultMaxOrder; // 8192 << 11 = 16 MiB per chunk
        public static readonly int DefaultTinyCacheSize;
        public static readonly int DefaultSmallCacheSize;
        public static readonly int DefaultNormalCacheSize;

        static readonly int DefaultMaxCachedBufferCapacity;
        static readonly int DefaultCacheTrimInterval; 
        private static readonly long DefaultCacheTrimIntervalMillis;
        private static readonly bool DefaultUseCacheForAllThreads;
        const int MinPageSize = 4096;
        const int MaxChunkSize = (int)(((long)int.MaxValue + 1) / 2);  

        static PooledByteBufferAllocator()
        {
            int defaultPageSize = SystemPropertyUtil.GetInt("io.netty.allocator.pageSize", 8192);
            Exception pageSizeFallbackCause = null;
            try
            {
                _ = ValidateAndCalculatePageShifts(defaultPageSize);
            }
            catch (Exception t)
            {
                pageSizeFallbackCause = t;
                defaultPageSize = 8192;
            }
            DefaultPageSize = defaultPageSize;

            int defaultMaxOrder = SystemPropertyUtil.GetInt("io.netty.allocator.maxOrder", 11);
            Exception maxOrderFallbackCause = null;
            try
            {
                _ = ValidateAndCalculateChunkSize(DefaultPageSize, defaultMaxOrder);
            }
            catch (Exception t)
            {
                maxOrderFallbackCause = t;
                defaultMaxOrder = 11;
            }
            DefaultMaxOrder = defaultMaxOrder;

            // todo: Determine reasonable default for heapArenaCount
            // Assuming each arena has 3 chunks, the pool should not consume more than 50% of max memory.

            /*
             * We use 2 * available processors by default to reduce contention as we use 2 * available processors for the
             * number of EventLoops in NIO and EPOLL as well. If we choose a smaller number we will run into hot spots as
             * allocation and de-allocation needs to be synchronized on the PoolArena.
             *
             * See https://github.com/netty/netty/issues/3888.
             */
            int defaultMinNumArena = Environment.ProcessorCount * 2;
            DefaultNumHeapArena = Math.Max(0, SystemPropertyUtil.GetInt("io.netty.allocator.numHeapArenas", defaultMinNumArena));
            DefaultNumDirectArena = Math.Max(0, SystemPropertyUtil.GetInt("io.netty.allocator.numDirectArenas", defaultMinNumArena));

            // cache sizes
            DefaultTinyCacheSize = SystemPropertyUtil.GetInt("io.netty.allocator.tinyCacheSize", 512);
            DefaultSmallCacheSize = SystemPropertyUtil.GetInt("io.netty.allocator.smallCacheSize", 256);
            DefaultNormalCacheSize = SystemPropertyUtil.GetInt("io.netty.allocator.normalCacheSize", 64);
            if (SystemPropertyUtil.Contains("io.netty.allocation.cacheTrimIntervalMillis"))
            {
                Logger.Warn("-Dio.netty.allocation.cacheTrimIntervalMillis is deprecated," +
                        " use -Dio.netty.allocator.cacheTrimIntervalMillis");

                if (SystemPropertyUtil.Contains("io.netty.allocator.cacheTrimIntervalMillis"))
                {
                    // Both system properties are specified. Use the non-deprecated one.
                    DefaultCacheTrimIntervalMillis = SystemPropertyUtil.GetLong(
                            "io.netty.allocator.cacheTrimIntervalMillis", 0);
                }
                else
                {
                    DefaultCacheTrimIntervalMillis = SystemPropertyUtil.GetLong(
                            "io.netty.allocation.cacheTrimIntervalMillis", 0);
                }
            }
            else
            {
                DefaultCacheTrimIntervalMillis = SystemPropertyUtil.GetLong(
                        "io.netty.allocator.cacheTrimIntervalMillis", 0);
            }
            DefaultUseCacheForAllThreads = SystemPropertyUtil.GetBoolean(
        "io.netty.allocator.useCacheForAllThreads", false);
            // 32 kb is the default maximum capacity of the cached buffer. Similar to what is explained in
            // 'Scalable memory allocation using jemalloc'
            DefaultMaxCachedBufferCapacity = SystemPropertyUtil.GetInt("io.netty.allocator.maxCachedBufferCapacity", 32 * 1024);

            // the number of threshold of allocations when cached entries will be freed up if not frequently used
            DefaultCacheTrimInterval = SystemPropertyUtil.GetInt(
                "io.netty.allocator.cacheTrimInterval", 8192);

            if (Logger.DebugEnabled)
            {
                Logger.Debug("-Dio.netty.allocator.numHeapArenas: {}", DefaultNumHeapArena);
                Logger.Debug("-Dio.netty.allocator.numDirectArenas: {}", DefaultNumDirectArena);
                if (pageSizeFallbackCause is null)
                {
                    Logger.Debug("-Dio.netty.allocator.pageSize: {}", DefaultPageSize);
                }
                else
                {
                    Logger.Debug("-Dio.netty.allocator.pageSize: {}", DefaultPageSize, pageSizeFallbackCause);
                }
                if (maxOrderFallbackCause is null)
                {
                    Logger.Debug("-Dio.netty.allocator.maxOrder: {}", DefaultMaxOrder);
                }
                else
                {
                    Logger.Debug("-Dio.netty.allocator.maxOrder: {}", DefaultMaxOrder, maxOrderFallbackCause);
                }
                Logger.Debug("-Dio.netty.allocator.chunkSize: {}", DefaultPageSize << DefaultMaxOrder);
                Logger.Debug("-Dio.netty.allocator.tinyCacheSize: {}", DefaultTinyCacheSize);
                Logger.Debug("-Dio.netty.allocator.smallCacheSize: {}", DefaultSmallCacheSize);
                Logger.Debug("-Dio.netty.allocator.cacheTrimIntervalMillis: {}", DefaultCacheTrimIntervalMillis);
                Logger.Debug("-Dio.netty.allocator.useCacheForAllThreads: {}", DefaultUseCacheForAllThreads);
                Logger.Debug("-Dio.netty.allocator.normalCacheSize: {}", DefaultNormalCacheSize);
                Logger.Debug("-Dio.netty.allocator.maxCachedBufferCapacity: {}", DefaultMaxCachedBufferCapacity);
                Logger.Debug("-Dio.netty.allocator.cacheTrimInterval: {}", DefaultCacheTrimInterval);
            }

            Default = new PooledByteBufferAllocator(PlatformDependent.DirectBufferPreferred);
        }

        public static readonly PooledByteBufferAllocator Default;

        private readonly PoolArena<byte[]>[] _heapArenas;
        private readonly PoolArena<byte[]>[] _directArenas;
        private readonly int _tinyCacheSize;
        private readonly int _smallCacheSize;
        private readonly int _normalCacheSize;
        private readonly bool _useCacheForAllThreads;
        private readonly IRunnable trimTask;
        private readonly IReadOnlyList<IPoolArenaMetric> _heapArenaMetrics;
        private readonly IReadOnlyList<IPoolArenaMetric> _directArenaMetrics;
        private readonly PoolThreadLocalCache _threadCache;
        private readonly int _chunkSize;
        private readonly PooledByteBufferAllocatorMetric _metric;

        public PooledByteBufferAllocator() : this(false)
        {
        }

        public unsafe PooledByteBufferAllocator(bool preferDirect)
            : this(preferDirect, DefaultNumHeapArena, DefaultNumDirectArena, DefaultPageSize, DefaultMaxOrder, DefaultUseCacheForAllThreads)
        {
        }

        public PooledByteBufferAllocator(int nHeapArena, int nDirectArena, int pageSize, int maxOrder, bool useCacheForAllThreads)
            : this(false, nHeapArena, nDirectArena, pageSize, maxOrder,useCacheForAllThreads)
        {
        }

        public unsafe PooledByteBufferAllocator(bool preferDirect, int nHeapArena, int nDirectArena, int pageSize, int maxOrder, bool useCacheForAllThreads)
            : this(preferDirect, nHeapArena, nDirectArena, pageSize, maxOrder,
                DefaultTinyCacheSize, DefaultSmallCacheSize, DefaultNormalCacheSize, useCacheForAllThreads, DefaultCacheTrimIntervalMillis)
        {
        }

        public PooledByteBufferAllocator(int nHeapArena, int nDirectArena, int pageSize, int maxOrder,
            int tinyCacheSize, int smallCacheSize, int normalCacheSize, bool useCacheForAllThreads)
            : this(false, nHeapArena, nDirectArena, pageSize, maxOrder, tinyCacheSize, smallCacheSize, normalCacheSize, useCacheForAllThreads, DefaultCacheTrimIntervalMillis)
        { }

        public PooledByteBufferAllocator(int nHeapArena, int nDirectArena, int pageSize, int maxOrder,
    int tinyCacheSize, int smallCacheSize, int normalCacheSize)
    : this(false, nHeapArena, nDirectArena, pageSize, maxOrder, tinyCacheSize, smallCacheSize, normalCacheSize, DefaultUseCacheForAllThreads, DefaultCacheTrimIntervalMillis)
        { }

        public unsafe PooledByteBufferAllocator(bool preferDirect, int nHeapArena, int nDirectArena, int pageSize, int maxOrder,
            int tinyCacheSize, int smallCacheSize, int normalCacheSize, bool defaultUseCacheForAllThreads, long defaultCacheTrimIntervalMillis)
            : base(preferDirect)
        {
            if ((uint)nHeapArena > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(nHeapArena, ExceptionArgument.nHeapArena); }
            if ((uint)nDirectArena > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(nHeapArena, ExceptionArgument.nDirectArena); }

            _threadCache = new PoolThreadLocalCache(this);
            _useCacheForAllThreads = defaultUseCacheForAllThreads;
            _tinyCacheSize = tinyCacheSize;
            trimTask = new ActionTrimTask(() => this.TrimCurrentThreadCache());
            _smallCacheSize = smallCacheSize;
            _normalCacheSize = normalCacheSize;
            _chunkSize = ValidateAndCalculateChunkSize(pageSize, maxOrder);

            int pageShifts = ValidateAndCalculatePageShifts(pageSize);

            if (0u >= (uint)nHeapArena)
            {
                _heapArenas = null;
                _heapArenaMetrics = new IPoolArenaMetric[0];
            }
            else
            {
                _heapArenas = NewArenaArray<byte[]>(nHeapArena);
                var metrics = new List<IPoolArenaMetric>(_heapArenas.Length);
                for (int i = 0; i < _heapArenas.Length; i++)
                {
                    var arena = new HeapArena(this, pageSize, maxOrder, pageShifts, _chunkSize);
                    _heapArenas[i] = arena;
                    metrics.Add(arena);
                }
                _heapArenaMetrics = metrics.AsReadOnly();
            }

            if (0u >= (uint)nDirectArena)
            {
                _directArenas = null;
                _directArenaMetrics = new IPoolArenaMetric[0];
            }
            else
            {
                _directArenas = NewArenaArray<byte[]>(nDirectArena);
                var metrics = new List<IPoolArenaMetric>(_directArenas.Length);
                for (int i = 0; i < _directArenas.Length; i++)
                {
                    var arena = new DirectArena(this, pageSize, maxOrder, pageShifts, _chunkSize);
                    _directArenas[i] = arena;
                    metrics.Add(arena);
                }
                _directArenaMetrics = metrics.AsReadOnly();
            }

            _metric = new PooledByteBufferAllocatorMetric(this);
        }

        static PoolArena<T>[] NewArenaArray<T>(int size) => new PoolArena<T>[size];

        public bool TrimCurrentThreadCache()
        {
            var cache = _threadCache.Value;
            if (cache != null)
            {
                cache.Trim();
                return true;
            }
            return false;
        }

        static int ValidateAndCalculatePageShifts(int pageSize)
        {
            if (pageSize < MinPageSize) { ThrowHelper.ThrowArgumentOutOfRangeException(); }
            if ((pageSize & pageSize - 1) != 0) { ThrowHelper.ThrowArgumentException_ExpectedPowerOf2(); }

            // Logarithm base 2. At this point we know that pageSize is a power of two.
            return (sizeof(int) * 8 - 1) - pageSize.NumberOfLeadingZeros();
        }

        static int ValidateAndCalculateChunkSize(int pageSize, int maxOrder)
        {
            if (maxOrder > 14) { ThrowHelper.ThrowArgumentException_CheckMaxOrder14(maxOrder); }

            // Ensure the resulting chunkSize does not overflow.
            int chunkSize = pageSize;
            for (int i = maxOrder; i > 0; i--)
            {
                if (chunkSize > MaxChunkSize >> 1)
                {
                    ThrowHelper.ThrowArgumentException_PageSize(pageSize, maxOrder, MaxChunkSize);
                }
                chunkSize <<= 1;
            }
            return chunkSize;
        }

        protected override IByteBuffer NewHeapBuffer(int initialCapacity, int maxCapacity)
        {
            PoolThreadCache<byte[]> cache = _threadCache.Value;
            PoolArena<byte[]> heapArena = cache.HeapArena;

            IByteBuffer buf;
            if (heapArena is object)
            {
                buf = heapArena.Allocate(cache, initialCapacity, maxCapacity);
            }
            else
            {
                buf = new UnpooledHeapByteBuffer(this, initialCapacity, maxCapacity);
            }

            return ToLeakAwareBuffer(buf);
        }

        protected unsafe override IByteBuffer NewDirectBuffer(int initialCapacity, int maxCapacity)
        {
            PoolThreadCache<byte[]> cache = _threadCache.Value;
            PoolArena<byte[]> directArena = cache.DirectArena;

            IByteBuffer buf;
            if (directArena is object)
            {
                buf = directArena.Allocate(cache, initialCapacity, maxCapacity);
            }
            else
            {
                buf = UnsafeByteBufferUtil.NewUnsafeDirectByteBuffer(this, initialCapacity, maxCapacity);
            }

            return ToLeakAwareBuffer(buf);
        }

        public static bool DefaultPreferDirect => PlatformDependent.DirectBufferPreferred;

        public override bool IsDirectBufferPooled => _directArenas is object;


        sealed class ActionTrimTask : IRunnable
        {
            readonly Action _action; 
            public ActionTrimTask(Action action)
            {
                _action = action;
            }
            public void Run() => _action();
        }
    sealed class PoolThreadLocalCache : FastThreadLocal<PoolThreadCache<byte[]>>
        {
            readonly PooledByteBufferAllocator _owner;

            public PoolThreadLocalCache(PooledByteBufferAllocator owner)
            {
                _owner = owner;
            }

            protected override PoolThreadCache<byte[]> GetInitialValue()
            {
                lock (this)
                {
                    PoolArena<byte[]> heapArena = LeastUsedArena(_owner._heapArenas);
                    PoolArena<byte[]> directArena = LeastUsedArena(_owner._directArenas);
                    ExecutionEnvironment.TryGetCurrentExecutor(out IEventExecutor eventExecutor);
                    PoolThreadCache<byte[]> cache = null;
                    if (_owner._useCacheForAllThreads || 
                      // The Thread is used by an EventExecutor, let's use the cache as the chances are good that we
                      // will allocate a lot!
                      eventExecutor != null)
                    {
                        cache = new PoolThreadCache<byte[]>(
                            heapArena, directArena,
                            _owner._tinyCacheSize, _owner._smallCacheSize, _owner._normalCacheSize,
                            DefaultMaxCachedBufferCapacity, DefaultCacheTrimInterval);

                        if (DefaultCacheTrimIntervalMillis > 0)
                        {
                            if (eventExecutor != null)
                            {
                                eventExecutor.ScheduleAtFixedRateAsync(_owner.trimTask,TimeSpan.FromMilliseconds( DefaultCacheTrimIntervalMillis),
                                       TimeSpan.FromMilliseconds(DefaultCacheTrimIntervalMillis));
                            }
                        }
                    }
                    return cache;
                }
            }

            protected override void OnRemoval(PoolThreadCache<byte[]> threadCache) => threadCache.Free();

            PoolArena<T> LeastUsedArena<T>(PoolArena<T>[] arenas)
            {
                if (arenas is null || 0u >= (uint)arenas.Length)
                {
                    return null;
                }

                PoolArena<T> minArena = arenas[0];
                for (int i = 1; i < arenas.Length; i++)
                {
                    PoolArena<T> arena = arenas[i];
                    if (arena.NumThreadCaches < minArena.NumThreadCaches)
                    {
                        minArena = arena;
                    }
                }

                return minArena;
            }
        }

        internal IReadOnlyList<IPoolArenaMetric> HeapArenas() => _heapArenaMetrics;

        internal IReadOnlyList<IPoolArenaMetric> DirectArenas() => _directArenaMetrics;

        internal int TinyCacheSize => _tinyCacheSize;

        internal int SmallCacheSize => _smallCacheSize;

        internal int NormalCacheSize => _normalCacheSize;

        internal int ChunkSize => _chunkSize;

        public PooledByteBufferAllocatorMetric Metric => _metric;

        IByteBufferAllocatorMetric IByteBufferAllocatorMetricProvider.Metric => Metric;

        internal long UsedHeapMemory => UsedMemory(_heapArenas);

        internal long UsedDirectMemory => UsedMemory(_directArenas);

        static long UsedMemory(PoolArena<byte[]>[] arenas)
        {
            if (arenas is null)
            {
                return -1;
            }
            long used = 0;
            foreach (PoolArena<byte[]> arena in arenas)
            {
                used += arena.NumActiveBytes;
                if (used < 0)
                {
                    return long.MaxValue;
                }
            }

            return used;
        }

        internal PoolThreadCache<T> ThreadCache<T>() => (PoolThreadCache<T>)(object)_threadCache.Value;

        /// Returns the status of the allocator (which contains all metrics) as string. Be aware this may be expensive
        /// and so should not called too frequently.
        public string DumpStats()
        {
            int heapArenasLen = _heapArenas?.Length ?? 0;
            var buf = StringBuilderManager.Allocate(512)
                    .Append(heapArenasLen)
                    .Append(" heap arena(s):")
                    .Append(StringUtil.Newline);
            if (heapArenasLen > 0)
            {
                // ReSharper disable once PossibleNullReferenceException
                foreach (PoolArena<byte[]> a in _heapArenas)
                {
                    _ = buf.Append(a);
                }
            }

            int directArenasLen = _directArenas?.Length ?? 0;
            _ = buf.Append(directArenasLen)
                .Append(" direct arena(s):")
                .Append(StringUtil.Newline);
            if (directArenasLen > 0)
            {
                // ReSharper disable once PossibleNullReferenceException
                foreach (PoolArena<byte[]> a in _directArenas)
                {
                    _ = buf.Append(a);
                }
            }

            return StringBuilderManager.ReturnAndFree(buf);
        }
    }
}