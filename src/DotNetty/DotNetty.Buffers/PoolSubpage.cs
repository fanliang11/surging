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
    using System.Diagnostics;
    using DotNetty.Common.Utilities;

    sealed class PoolSubpage<T> : IPoolSubpageMetric
    {
        internal readonly PoolChunk<T> Chunk;
        private readonly int _memoryMapIdx;
        private readonly int _runOffset;
        private readonly int _pageSize;
        private readonly long[] _bitmap;

        internal PoolSubpage<T> Prev;
        internal PoolSubpage<T> Next;

        internal bool DoNotDestroy;
        internal int ElemSize;
        private int _maxNumElems;
        private int _bitmapLength;
        private int _nextAvail;
        private int _numAvail;

        // TODO: Test if adding padding helps under contention
        //private long pad0, pad1, pad2, pad3, pad4, pad5, pad6, pad7;

        /** Special constructor that creates a linked list head */

        public PoolSubpage(int pageSize)
        {
            Chunk = null;
            _memoryMapIdx = -1;
            _runOffset = -1;
            ElemSize = -1;
            _pageSize = pageSize;
            _bitmap = null;
        }

        public PoolSubpage(PoolSubpage<T> head, PoolChunk<T> chunk, int memoryMapIdx, int runOffset, int pageSize, int elemSize)
        {
            Chunk = chunk;
            _memoryMapIdx = memoryMapIdx;
            _runOffset = runOffset;
            _pageSize = pageSize;
            _bitmap = new long[pageSize.RightUShift(10)]; // pageSize / 16 / 64
            Init(head, elemSize);
        }

        public void Init(PoolSubpage<T> head, int elemSize)
        {
            DoNotDestroy = true;
            ElemSize = elemSize;
            if (elemSize != 0)
            {
                _maxNumElems = _numAvail = _pageSize / elemSize;
                _nextAvail = 0;
                _bitmapLength = _maxNumElems.RightUShift(6);
                if ((_maxNumElems & 63) != 0)
                {
                    _bitmapLength++;
                }

                for (int i = 0; i < _bitmapLength; i++)
                {
                    _bitmap[i] = 0;
                }
            }

            AddToPool(head);
        }

        /**
         * Returns the bitmap index of the subpage allocation.
         */

        internal long Allocate()
        {
            if (0u >= (uint)ElemSize)
            {
                return ToHandle(0);
            }

            if (0u >= (uint)_numAvail || !DoNotDestroy)
            {
                return -1;
            }

            int bitmapIdx = GetNextAvail();
            int q = bitmapIdx.RightUShift(6);
            int r = bitmapIdx & 63;
            Debug.Assert((_bitmap[q].RightUShift(r) & 1) == 0);
            _bitmap[q] |= 1L << r;

            if (0u >= (uint)(--_numAvail))
            {
                RemoveFromPool();
            }

            return ToHandle(bitmapIdx);
        }

        /**
         * @return <c>true</c> if this subpage is in use.
         *         <c>false</c> if this subpage is not used by its chunk and thus it's OK to be released.
         */

        internal bool Free(PoolSubpage<T> head, int bitmapIdx)
        {
            if (0u >= (uint)ElemSize)
            {
                return true;
            }

            int q = bitmapIdx.RightUShift(6);
            int r = bitmapIdx & 63;
            Debug.Assert((_bitmap[q].RightUShift(r) & 1) != 0);
            _bitmap[q] ^= 1L << r;

            SetNextAvail(bitmapIdx);

            if (0u >= (uint)_numAvail++)
            {
                AddToPool(head);
                return true;
            }

            if (_numAvail != _maxNumElems)
            {
                return true;
            }
            else
            {
                // Subpage not in use (numAvail == maxNumElems)
                if (Prev == Next)
                {
                    // Do not remove if this subpage is the only one left in the pool.
                    return true;
                }

                // Remove this subpage from the pool if there are other subpages left in the pool.
                DoNotDestroy = false;
                RemoveFromPool();
                return false;
            }
        }

        void AddToPool(PoolSubpage<T> head)
        {
            Debug.Assert(Prev is null && Next is null);

            Prev = head;
            Next = head.Next;
            Next.Prev = this;
            head.Next = this;
        }

        void RemoveFromPool()
        {
            Debug.Assert(Prev is object && Next is object);

            Prev.Next = Next;
            Next.Prev = Prev;
            Next = null;
            Prev = null;
        }

        void SetNextAvail(int bitmapIdx) => _nextAvail = bitmapIdx;

        int GetNextAvail()
        {
            int nextAvail = _nextAvail;
            if (nextAvail >= 0)
            {
                _nextAvail = -1;
                return nextAvail;
            }
            return FindNextAvail();
        }

        int FindNextAvail()
        {
            long[] bitmap = _bitmap;
            int bitmapLength = _bitmapLength;
            for (int i = 0; i < bitmapLength; i++)
            {
                long bits = bitmap[i];
                if (~bits != 0)
                {
                    return FindNextAvail0(i, bits);
                }
            }
            return -1;
        }

        int FindNextAvail0(int i, long bits)
        {
            int maxNumElems = _maxNumElems;
            int baseVal = i << 6;

            for (int j = 0; j < 64; j++)
            {
                if (0u >= (uint)(bits & 1))
                {
                    int val = baseVal | j;
                    if (val < maxNumElems)
                    {
                        return val;
                    }
                    else
                    {
                        break;
                    }
                }
                bits = bits.RightUShift(1);
            }
            return -1;
        }

        long ToHandle(int bitmapIdx) => 0x4000000000000000L | (long)bitmapIdx << 32 | (uint)_memoryMapIdx;

        public override string ToString()
        {
            bool doNotDestroy;
            int maxNumElems;
            int numAvail;
            int elemSize;

            var thisChunk = Chunk;
            if (thisChunk is null)
            {
                // This is the head so there is no need to synchronize at all as these never change.
                doNotDestroy = true;
                maxNumElems = 0;
                numAvail = 0;
                elemSize = -1;
            }
            else
            {
                lock (thisChunk.Arena)
                {
                    if (!DoNotDestroy)
                    {
                        doNotDestroy = false;
                        // Not used for creating the String.
                        maxNumElems = numAvail = elemSize = -1;
                    }
                    else
                    {
                        doNotDestroy = true;
                        maxNumElems = _maxNumElems;
                        numAvail = _numAvail;
                        elemSize = ElemSize;
                    }
                }
            }

            if (!doNotDestroy)
            {
                return "(" + _memoryMapIdx + ": not in use)";
            }

            return "(" + _memoryMapIdx + ": " + (maxNumElems - numAvail) + "/" + maxNumElems +
                ", offset: " + _runOffset + ", length: " + _pageSize + ", elemSize: " + elemSize + ")";
        }

        public int MaxNumElements
        {
            get
            {
                var thisChunk = Chunk;
                if (thisChunk is null)
                {
                    // It's the head.
                    return 0;
                }

                lock (thisChunk.Arena)
                {
                    return _maxNumElems;
                }
            }
        }

        public int NumAvailable
        {
            get
            {
                var thisChunk = Chunk;
                if (thisChunk is null)
                {
                    // It's the head.
                    return 0;
                }

                lock (thisChunk.Arena)
                {
                    return _numAvail;
                }
            }
        }

        public int ElementSize
        {
            get
            {
                var thisChunk = Chunk;
                if (thisChunk is null)
                {
                    // It's the head.
                    return -1;
                }

                lock (thisChunk.Arena)
                {
                    return ElemSize;
                }
            }
        }

        public int PageSize => _pageSize;

        internal void Destroy() => Chunk?.Destroy();
    }
}