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
    using System.Collections.Generic;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    public sealed class PooledByteBufferAllocatorMetric : IByteBufferAllocatorMetric
    {
        readonly PooledByteBufferAllocator _allocator;

        internal PooledByteBufferAllocatorMetric(PooledByteBufferAllocator allocator)
        {
            _allocator = allocator;
        }

        public IReadOnlyList<IPoolArenaMetric> HeapArenas() => _allocator.HeapArenas();

        public IReadOnlyList<IPoolArenaMetric> DirectArenas() => _allocator.DirectArenas();

        public int TinyCacheSize => _allocator.TinyCacheSize;

        public int SmallCacheSize => _allocator.SmallCacheSize;

        public int NormalCacheSize => _allocator.NormalCacheSize;

        public int ChunkSize => _allocator.ChunkSize;

        public long UsedHeapMemory => _allocator.UsedHeapMemory;

        public long UsedDirectMemory => _allocator.UsedDirectMemory;

        public int NumThreadLocalCaches()
        {
            int total = 0;
            var arenas = HeapArenas();
            if (arenas is object)
            {
                foreach (IPoolArenaMetric metric in arenas)
                {
                    total += metric.NumThreadCaches;
                }
            }

            arenas = DirectArenas();
            if (arenas is object)
            {
                foreach (IPoolArenaMetric metric in arenas)
                {
                    total += metric.NumThreadCaches;
                }
            }

            return total;
        }

        public override string ToString()
        {
            var sb = StringBuilderManager.Allocate(256);
            _ = sb.Append(StringUtil.SimpleClassName(this))
                .Append("(usedHeapMemory: ").Append(UsedHeapMemory)
                .Append("; usedDirectMemory: ").Append(UsedDirectMemory)
                .Append("; numHeapArenas: ").Append(HeapArenas().Count)
                .Append("; numDirectArenas: ").Append(DirectArenas().Count)
                .Append("; tinyCacheSize: ").Append(TinyCacheSize)
                .Append("; smallCacheSize: ").Append(SmallCacheSize)
                .Append("; normalCacheSize: ").Append(NormalCacheSize)
                .Append("; numThreadLocalCaches: ").Append(NumThreadLocalCaches())
                .Append("; chunkSize: ").Append(ChunkSize).Append(')');
            return StringBuilderManager.ReturnAndFree(sb);
        }
    }
}
