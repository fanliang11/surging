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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    sealed class PoolChunkList<T> : IPoolChunkListMetric
    {
        private readonly PoolArena<T> _arena;
        private readonly PoolChunkList<T> _nextList;
        private readonly int _minUsage;
        private readonly int _maxUsage;
        private readonly int _maxCapacity;
        private PoolChunk<T> _head;
        private readonly int _freeMinThreshold;
        private readonly int _freeMaxThreshold;

        // This is only update once when create the linked like list of PoolChunkList in PoolArena constructor.
        private PoolChunkList<T> _prevList;

        // TODO: Test if adding padding helps under contention
        //private long pad0, pad1, pad2, pad3, pad4, pad5, pad6, pad7;

        public PoolChunkList(PoolArena<T> arena, PoolChunkList<T> nextList, int minUsage, int maxUsage, int chunkSize)
        {
            Debug.Assert(minUsage <= maxUsage);
            _arena = arena;
            _nextList = nextList;
            _minUsage = minUsage;
            _maxUsage = maxUsage;
            _maxCapacity = CalculateMaxCapacity(minUsage, chunkSize);

            // the thresholds are aligned with PoolChunk.usage() logic:
            // 1) basic logic: usage() = 100 - freeBytes * 100L / chunkSize
            //    so, for example: (usage() >= maxUsage) condition can be transformed in the following way:
            //      100 - freeBytes * 100L / chunkSize >= maxUsage
            //      freeBytes <= chunkSize * (100 - maxUsage) / 100
            //      let freeMinThreshold = chunkSize * (100 - maxUsage) / 100, then freeBytes <= freeMinThreshold
            //
            //  2) usage() returns an int value and has a floor rounding during a calculation,
            //     to be aligned absolute thresholds should be shifted for "the rounding step":
            //       freeBytes * 100 / chunkSize < 1
            //       the condition can be converted to: freeBytes < 1 * chunkSize / 100
            //     this is why we have + 0.99999999 shifts. A example why just +1 shift cannot be used:
            //       freeBytes = 16777216 == freeMaxThreshold: 16777216, usage = 0 < minUsage: 1, chunkSize: 16777216
            //     At the same time we want to have zero thresholds in case of (maxUsage == 100) and (minUsage == 100).
            //
            _freeMinThreshold = CalculateThresholdWithOverflow(chunkSize, maxUsage);
            _freeMaxThreshold = CalculateThresholdWithOverflow(chunkSize, minUsage);
        }

        // https://github.com/cuteant/SpanNetty/issues/29
        private static int CalculateThresholdWithOverflow(int chunkSize, int usage)
        {
            int freeThreshold;
            if (usage == 100)
            {
                freeThreshold = 0;
            }
            else
            {
                var tmp = chunkSize * (100.0d - usage + 0.99999999d) / 100L;
                if (tmp <= int.MinValue)
                {
                    freeThreshold = int.MinValue;
                }
                else if (tmp >= int.MaxValue)
                {
                    freeThreshold = int.MaxValue;
                }
                else
                {
                    freeThreshold = (int)tmp;
                }
            }

            return freeThreshold;
        }

        /// Calculates the maximum capacity of a buffer that will ever be possible to allocate out of the {@link PoolChunk}s
        /// that belong to the {@link PoolChunkList} with the given {@code minUsage} and {@code maxUsage} settings.
        static int CalculateMaxCapacity(int minUsage, int chunkSize)
        {
            minUsage = MinUsage0(minUsage);

            if (minUsage == 100)
            {
                // If the minUsage is 100 we can not allocate anything out of this list.
                return 0;
            }

            // Calculate the maximum amount of bytes that can be allocated from a PoolChunk in this PoolChunkList.
            //
            // As an example:
            // - If a PoolChunkList has minUsage == 25 we are allowed to allocate at most 75% of the chunkSize because
            //   this is the maximum amount available in any PoolChunk in this PoolChunkList.
            return (int)(chunkSize * (100L - minUsage) / 100L);
        }

        internal void PrevList(PoolChunkList<T> list)
        {
            Debug.Assert(_prevList is null);
            _prevList = list;
        }

        internal bool Allocate(PooledByteBuffer<T> buf, int reqCapacity, int normCapacity, PoolThreadCache<T> threadCache)
        {
            if (_head is null || normCapacity > _maxCapacity)
            {
                // Either this PoolChunkList is empty or the requested capacity is larger then the capacity which can
                // be handled by the PoolChunks that are contained in this PoolChunkList.
                return false;
            }

            for (PoolChunk<T> cur = _head; cur is object; cur = cur.Next)
            {
                if (cur.Allocate(buf, reqCapacity, normCapacity, threadCache))
                {
                    if (cur._freeBytes <= _freeMinThreshold)
                    {
                        Remove(cur);
                        _nextList.Add(cur);
                    }
                    return true;
                }
            }
            return false;
        }

        internal bool Free(PoolChunk<T> chunk, long handle)
        {
            chunk.Free(handle);
            if (chunk._freeBytes > _freeMaxThreshold)
            {
                Remove(chunk);
                // Move the PoolChunk down the PoolChunkList linked-list.
                return Move0(chunk);
            }
            return true;
        }

        bool Move(PoolChunk<T> chunk)
        {
            Debug.Assert(chunk.Usage < _maxUsage);

            if (chunk._freeBytes > _freeMaxThreshold)
            {
                // Move the PoolChunk down the PoolChunkList linked-list.
                return Move0(chunk);
            }

            // PoolChunk fits into this PoolChunkList, adding it here.
            Add0(chunk);
            return true;
        }

        /// Moves the {@link PoolChunk} down the {@link PoolChunkList} linked-list so it will end up in the right
        /// {@link PoolChunkList} that has the correct minUsage / maxUsage in respect to {@link PoolChunk#usage()}.
        bool Move0(PoolChunk<T> chunk)
        {
            if (_prevList is null)
            {
                // There is no previous PoolChunkList so return false which result in having the PoolChunk destroyed and
                // all memory associated with the PoolChunk will be released.
                Debug.Assert(chunk.Usage == 0);
                return false;
            }
            return _prevList.Move(chunk);
        }

        internal void Add(PoolChunk<T> chunk)
        {
            if (chunk._freeBytes <= _freeMinThreshold)
            {
                _nextList.Add(chunk);
                return;
            }
            Add0(chunk);
        }

        /// Adds the {@link PoolChunk} to this {@link PoolChunkList}.
        void Add0(PoolChunk<T> chunk)
        {
            chunk.Parent = this;
            if (_head is null)
            {
                _head = chunk;
                chunk.Prev = null;
                chunk.Next = null;
            }
            else
            {
                chunk.Prev = null;
                chunk.Next = _head;
                _head.Prev = chunk;
                _head = chunk;
            }
        }

        void Remove(PoolChunk<T> cur)
        {
            if (cur == _head)
            {
                _head = cur.Next;
                if (_head is object)
                {
                    _head.Prev = null;
                }
            }
            else
            {
                PoolChunk<T> next = cur.Next;
                cur.Prev.Next = next;
                if (next is object)
                {
                    next.Prev = cur.Prev;
                }
            }
        }

        public int MinUsage => MinUsage0(_minUsage);

        public int MaxUsage => Math.Min(_maxUsage, 100);

        static int MinUsage0(int value) => Math.Max(1, value);

        public IEnumerator<IPoolChunkMetric> GetEnumerator()
        {
            lock (_arena)
            {
                if (_head is null)
                {
                    return Enumerable.Empty<IPoolChunkMetric>().GetEnumerator();
                }
                var metrics = new List<IPoolChunkMetric>();
                for (PoolChunk<T> cur = _head; cur is object;)
                {
                    metrics.Add(cur);
                    cur = cur.Next;
                }
                return metrics.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            var buf = StringBuilderManager.Allocate();
            lock (_arena)
            {
                if (_head is null)
                {
                    StringBuilderManager.Free(buf);
                    return "none";
                }

                for (PoolChunk<T> cur = _head; ;)
                {
                    _ = buf.Append(cur);
                    cur = cur.Next;
                    if (cur is null)
                    {
                        break;
                    }
                    _ = buf.Append(StringUtil.Newline);
                }
            }

            return StringBuilderManager.ReturnAndFree(buf);
        }

        internal void Destroy(PoolArena<T> poolArena)
        {
            PoolChunk<T> chunk = _head;
            while (chunk is object)
            {
                poolArena.DestroyChunk(chunk);
                chunk = chunk.Next;
            }

            _head = null;
        }
    }
}