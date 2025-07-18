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
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    /// <summary>
    ///     Description of algorithm for PageRun/PoolSubpage allocation from PoolChunk
    ///     Notation: The following terms are important to understand the code
    ///     > page  - a page is the smallest unit of memory chunk that can be allocated
    ///     > chunk - a chunk is a collection of pages
    ///     > in this code chunkSize = 2^{maxOrder} /// pageSize
    ///     To begin we allocate a byte array of size = chunkSize
    ///     Whenever a ByteBuf of given size needs to be created we search for the first position
    ///     in the byte array that has enough empty space to accommodate the requested size and
    ///     return a (long) handle that encodes this offset information, (this memory segment is then
    ///     marked as reserved so it is always used by exactly one ByteBuf and no more)
    ///     For simplicity all sizes are normalized according to PoolArena#normalizeCapacity method
    ///     This ensures that when we request for memory segments of size >= pageSize the normalizedCapacity
    ///     equals the next nearest power of 2
    ///     To search for the first offset in chunk that has at least requested size available we construct a
    ///     complete balanced binary tree and store it in an array (just like heaps) - memoryMap
    ///     The tree looks like this (the size of each node being mentioned in the parenthesis)
    ///     depth=0        1 node (chunkSize)
    ///     depth=1        2 nodes (chunkSize/2)
    ///     ..
    ///     ..
    ///     depth=d        2^d nodes (chunkSize/2^d)
    ///     ..
    ///     depth=maxOrder 2^maxOrder nodes (chunkSize/2^{maxOrder} = pageSize)
    ///     depth=maxOrder is the last level and the leafs consist of pages
    ///     With this tree available searching in chunkArray translates like this:
    ///     To allocate a memory segment of size chunkSize/2^k we search for the first node (from left) at height k
    ///     which is unused
    ///     Algorithm:
    ///     ----------
    ///     Encode the tree in memoryMap with the notation
    ///     memoryMap[id] = x => in the subtree rooted at id, the first node that is free to be allocated
    ///     is at depth x (counted from depth=0) i.e., at depths [depth_of_id, x), there is no node that is free
    ///     As we allocate and free nodes, we update values stored in memoryMap so that the property is maintained
    ///     Initialization -
    ///     In the beginning we construct the memoryMap array by storing the depth of a node at each node
    ///     i.e., memoryMap[id] = depth_of_id
    ///     Observations:
    ///     -------------
    ///     1) memoryMap[id] = depth_of_id  => it is free / unallocated
    ///     2) memoryMap[id] > depth_of_id  => at least one of its child nodes is allocated, so we cannot allocate it, but
    ///     some of its children can still be allocated based on their availability
    ///     3) memoryMap[id] = maxOrder + 1 => the node is fully allocated and thus none of its children can be allocated, it
    ///     is thus marked as unusable
    ///     Algorithm: [allocateNode(d) => we want to find the first node (from left) at height h that can be allocated]
    ///     ----------
    ///     1) start at root (i.e., depth = 0 or id = 1)
    ///     2) if memoryMap[1] > d => cannot be allocated from this chunk
    ///     3) if left node value &lt;= h; we can allocate from left subtree so move to left and repeat until found
    ///     4) else try in right subtree
    ///     Algorithm: [allocateRun(size)]
    ///     ----------
    ///     1) Compute d = log_2(chunkSize/size)
    ///     2) Return allocateNode(d)
    ///     Algorithm: [allocateSubpage(size)]
    ///     ----------
    ///     1) use allocateNode(maxOrder) to find an empty (i.e., unused) leaf (i.e., page)
    ///     2) use this handle to construct the PoolSubpage object or if it already exists just call init(normCapacity)
    ///     note that this PoolSubpage object is added to subpagesPool in the PoolArena when we init() it
    ///     Note:
    ///     -----
    ///     In the implementation for improving cache coherence,
    ///     we store 2 pieces of information depth_of_id and x as two byte values in memoryMap and depthMap respectively
    ///
    ///     memoryMap[id] = depth_of_id is defined above
    ///     depthMap[id] = x  indicates that the first node which is free to be allocated is at depth x(from root)
    /// </summary>
    sealed class PoolChunk<T> : IPoolChunkMetric
    {
        const int IntegerSizeMinusOne = IntegerExtensions.SizeInBits - 1;

        internal readonly PoolArena<T> Arena;
        internal readonly T Memory;
        internal readonly bool Unpooled;
        internal readonly int Offset;
        internal readonly IntPtr NativePointer;

        private readonly sbyte[] _memoryMap;
        private readonly sbyte[] _depthMap;
        private readonly PoolSubpage<T>[] _subpages;
        /** Used to determine if the requested capacity is equal to or greater than pageSize. */
        private readonly int _subpageOverflowMask;
        private readonly int _pageSize;
        private readonly int _pageShifts;
        private readonly int _maxOrder;
        private readonly int _chunkSize;
        private readonly int _log2ChunkSize;
        private readonly int _maxSubpageAllocs;
        /** Used to mark memory as unusable */
        private readonly sbyte _unusable;

        internal int _freeBytes;

        internal PoolChunkList<T> Parent;
        internal PoolChunk<T> Prev;
        internal PoolChunk<T> Next;

        // TODO: Test if adding padding helps under contention
        //private long pad0, pad1, pad2, pad3, pad4, pad5, pad6, pad7;

        internal PoolChunk(PoolArena<T> arena, T memory, int pageSize, int maxOrder, int pageShifts, int chunkSize, int offset, IntPtr pointer)
        {
            if (maxOrder >= 30) { ThrowHelper.ThrowArgumentException_CheckMaxOrder30(maxOrder); }

            Unpooled = false;
            Arena = arena;
            Memory = memory;
            _pageSize = pageSize;
            _pageShifts = pageShifts;
            _maxOrder = maxOrder;
            _chunkSize = chunkSize;
            Offset = offset;
            NativePointer = pointer;
            _unusable = (sbyte)(maxOrder + 1);
            _log2ChunkSize = Log2(chunkSize);
            _subpageOverflowMask = ~(pageSize - 1);
            _freeBytes = chunkSize;

            Debug.Assert(maxOrder < 30, "maxOrder should be < 30, but is: " + maxOrder);
            _maxSubpageAllocs = 1 << maxOrder;

            // Generate the memory map.
            _memoryMap = new sbyte[_maxSubpageAllocs << 1];
            _depthMap = new sbyte[_memoryMap.Length];
            int memoryMapIndex = 1;
            for (int d = 0; d <= maxOrder; ++d)
            {
                // move down the tree one level at a time
                int depth = 1 << d;
                for (int p = 0; p < depth; ++p)
                {
                    // in each level traverse left to right and set value to the depth of subtree
                    _memoryMap[memoryMapIndex] = (sbyte)d;
                    _depthMap[memoryMapIndex] = (sbyte)d;
                    memoryMapIndex++;
                }
            }

            _subpages = NewSubpageArray(_maxSubpageAllocs);
        }

        /** Creates a special chunk that is not pooled. */

        internal PoolChunk(PoolArena<T> arena, T memory, int size, int offset, IntPtr pointer)
        {
            Unpooled = true;
            Arena = arena;
            Memory = memory;
            Offset = offset;
            NativePointer = pointer;
            _memoryMap = null;
            _depthMap = null;
            _subpages = null;
            _subpageOverflowMask = 0;
            _pageSize = 0;
            _pageShifts = 0;
            _maxOrder = 0;
            _unusable = (sbyte)(_maxOrder + 1);
            _chunkSize = size;
            _log2ChunkSize = Log2(_chunkSize);
            _maxSubpageAllocs = 0;
        }

        PoolSubpage<T>[] NewSubpageArray(int size) => new PoolSubpage<T>[size];

        public int Usage
        {
            get
            {
                int freeBytes;
                lock (Arena)
                {
                    freeBytes = _freeBytes;
                }

                return GetUsage(freeBytes);
            }
        }

        int GetUsage(int freeBytes)
        {
            if (0u >= (uint)freeBytes)
            {
                return 100;
            }

            int freePercentage = (int)(freeBytes * 100L / ChunkSize);
            if (0u >= (uint)freePercentage)
            {
                return 99;
            }

            return 100 - freePercentage;
        }


        internal bool Allocate(PooledByteBuffer<T> buf, int reqCapacity, int normCapacity, PoolThreadCache<T> threadCache)
        {
            long handle;
            if ((normCapacity & _subpageOverflowMask) != 0)
            {
                // >= pageSize
                handle = AllocateRun(normCapacity);
            }
            else
            {
                handle = AllocateSubpage(normCapacity);
            }
            if (handle < 0) { return false; }

            InitBuf(buf, handle, reqCapacity, threadCache);

            return true;
        }

        /**
         * Update method used by allocate
         * This is triggered only when a successor is allocated and all its predecessors
         * need to update their state
         * The minimal depth at which subtree rooted at id has some free space
         *
         * @param id id
         */

        void UpdateParentsAlloc(int id)
        {
            while (id > 1)
            {
                int parentId = id.RightUShift(1);
                sbyte val1 = Value(id);
                sbyte val2 = Value(id ^ 1);
                sbyte val = val1 < val2 ? val1 : val2;
                SetValue(parentId, val);
                id = parentId;
            }
        }

        /**
         * Update method used by free
         * This needs to handle the special case when both children are completely free
         * in which case parent be directly allocated on request of size = child-size * 2
         *
         * @param id id
         */

        void UpdateParentsFree(int id)
        {
            int logChild = Depth(id) + 1;
            while (id > 1)
            {
                int parentId = id.RightUShift(1);
                sbyte val1 = Value(id);
                sbyte val2 = Value(id ^ 1);
                logChild -= 1; // in first iteration equals log, subsequently reduce 1 from logChild as we traverse up

                if (val1 == logChild && val2 == logChild)
                {
                    SetValue(parentId, (sbyte)(logChild - 1));
                }
                else
                {
                    sbyte val = val1 < val2 ? val1 : val2;
                    SetValue(parentId, val);
                }

                id = parentId;
            }
        }

        /**
         * Algorithm to allocate an index in memoryMap when we query for a free node
         * at depth d
         *
         * @param d depth
         * @return index in memoryMap
         */

        int AllocateNode(int d)
        {
            int id = 1;
            int initial = -(1 << d); // has last d bits = 0 and rest all = 1
            sbyte val = Value(id);
            if (val > d)
            {
                // unusable
                return -1;
            }
            while (val < d || 0u >= (uint)(id & initial))
            {
                // id & initial == 1 << d for all ids at depth d, for < d it is 0
                id <<= 1;
                val = Value(id);
                if (val > d)
                {
                    id ^= 1;
                    val = Value(id);
                }
            }
            sbyte value = Value(id);
            Debug.Assert(value == d && (id & initial) == 1 << d, $"val = {value}, id & initial = {id & initial}, d = {d}");
            SetValue(id, _unusable); // mark as unusable
            UpdateParentsAlloc(id);
            return id;
        }

        /**
         * Allocate a run of pages (>=1)
         *
         * @param normCapacity normalized capacity
         * @return index in memoryMap
         */

        long AllocateRun(int normCapacity)
        {
            int d = _maxOrder - (Log2(normCapacity) - _pageShifts);
            int id = AllocateNode(d);
            if (id < 0)
            {
                return id;
            }
            _freeBytes -= RunLength(id);
            return id;
        }

        /**
         * Create/ initialize a new PoolSubpage of normCapacity
         * Any PoolSubpage created/ initialized here is added to subpage pool in the PoolArena that owns this PoolChunk
         *
         * @param normCapacity normalized capacity
         * @return index in memoryMap
         */

        long AllocateSubpage(int normCapacity)
        {
            // Obtain the head of the PoolSubPage pool that is owned by the PoolArena and synchronize on it.
            // This is need as we may add it back and so alter the linked-list structure.
            PoolSubpage<T> head = Arena.FindSubpagePoolHead(normCapacity);
            lock (head)
            {
                int d = _maxOrder; // subpages are only be allocated from pages i.e., leaves
                int id = AllocateNode(d);
                if (id < 0)
                {
                    return id;
                }

                PoolSubpage<T>[] subpages = _subpages;
                int pageSize = _pageSize;

                _freeBytes -= pageSize;

                int subpageIdx = SubpageIdx(id);
                PoolSubpage<T> subpage = subpages[subpageIdx];
                if (subpage is null)
                {
                    subpage = new PoolSubpage<T>(head, this, id, RunOffset(id), pageSize, normCapacity);
                    subpages[subpageIdx] = subpage;
                }
                else
                {
                    subpage.Init(head, normCapacity);
                }

                return subpage.Allocate();
            }
        }

        /**
         * Free a subpage or a run of pages
         * When a subpage is freed from PoolSubpage, it might be added back to subpage pool of the owning PoolArena
         * If the subpage pool in PoolArena has at least one other PoolSubpage of given elemSize, we can
         * completely free the owning Page so it is available for subsequent allocations
         *
         * @param handle handle to free
         */

        internal void Free(long handle)
        {
            int memoryMapIdx = MemoryMapIdx(handle);
            int bitmapIdx = BitmapIdx(handle);

            if (bitmapIdx != 0)
            {
                // free a subpage
                PoolSubpage<T> subpage = _subpages[SubpageIdx(memoryMapIdx)];
                Debug.Assert(subpage is object && subpage.DoNotDestroy);

                // Obtain the head of the PoolSubPage pool that is owned by the PoolArena and synchronize on it.
                // This is need as we may add it back and so alter the linked-list structure.
                PoolSubpage<T> head = Arena.FindSubpagePoolHead(subpage.ElemSize);
                lock (head)
                {
                    if (subpage.Free(head, bitmapIdx & 0x3FFFFFFF))
                    {
                        return;
                    }
                }
            }
            _freeBytes += RunLength(memoryMapIdx);
            SetValue(memoryMapIdx, Depth(memoryMapIdx));
            UpdateParentsFree(memoryMapIdx);
        }

        internal void InitBuf(PooledByteBuffer<T> buf, long handle, int reqCapacity, PoolThreadCache<T> threadCache)
        {
            int memoryMapIdx = MemoryMapIdx(handle);
            int bitmapIdx = BitmapIdx(handle);
            if (0u >= (uint)bitmapIdx)
            {
                sbyte val = Value(memoryMapIdx);
                Debug.Assert(val == _unusable, val.ToString());
                buf.Init(this, handle, RunOffset(memoryMapIdx) + Offset, reqCapacity, RunLength(memoryMapIdx), threadCache);
            }
            else
            {
                InitBufWithSubpage(buf, handle, bitmapIdx, reqCapacity, threadCache);
            }
        }

        internal void InitBufWithSubpage(PooledByteBuffer<T> buf, long handle, int reqCapacity, PoolThreadCache<T> threadCache) =>
            InitBufWithSubpage(buf, handle, BitmapIdx(handle), reqCapacity, threadCache);

        void InitBufWithSubpage(PooledByteBuffer<T> buf, long handle, int bitmapIdx, int reqCapacity, PoolThreadCache<T> threadCache)
        {
            Debug.Assert(bitmapIdx != 0);

            int memoryMapIdx = MemoryMapIdx(handle);

            PoolSubpage<T> subpage = _subpages[SubpageIdx(memoryMapIdx)];
            Debug.Assert(subpage.DoNotDestroy);
            Debug.Assert(reqCapacity <= subpage.ElemSize);

            buf.Init(
                this, handle,
                RunOffset(memoryMapIdx) + (bitmapIdx & 0x3FFFFFFF) * subpage.ElemSize + Offset,
                reqCapacity, subpage.ElemSize, threadCache);
        }

        sbyte Value(int id) => _memoryMap[id];

        void SetValue(int id, sbyte val) => _memoryMap[id] = val;

        sbyte Depth(int id) => _depthMap[id];

        // compute the (0-based, with lsb = 0) position of highest set bit i.e, log2
        static int Log2(int val) => IntegerSizeMinusOne - val.NumberOfLeadingZeros();

        /// represents the size in #bytes supported by node 'id' in the tree
        int RunLength(int id) => 1 << _log2ChunkSize - Depth(id);

        int RunOffset(int id)
        {
            // represents the 0-based offset in #bytes from start of the byte-array chunk
            int shift = id ^ 1 << Depth(id);
            return shift * RunLength(id);
        }

        int SubpageIdx(int memoryMapIdx) => memoryMapIdx ^ _maxSubpageAllocs; // remove highest set bit, to get offset

        static int MemoryMapIdx(long handle) => (int)handle;

        static int BitmapIdx(long handle) => (int)handle.RightUShift(IntegerExtensions.SizeInBits);

        public int ChunkSize => _chunkSize;

        public int FreeBytes
        {
            get
            {
                lock (Arena)
                {
                    return _freeBytes;
                }
            }
        }

        public override string ToString()
        {
            var freeBytes = FreeBytes;
            var sb = StringBuilderManager.Allocate()
                .Append("Chunk(")
                .Append(RuntimeHelpers.GetHashCode(this).ToString("X"))
                .Append(": ")
                .Append(Usage)
                .Append("%, ")
                .Append(_chunkSize - freeBytes)
                .Append('/')
                .Append(_chunkSize)
                .Append(')');
            return StringBuilderManager.ReturnAndFree(sb);
        }

        internal void Destroy() => Arena.DestroyChunk(this);
    }
}