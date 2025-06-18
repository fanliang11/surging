// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using System.Diagnostics;
    using System.Threading;

    public class WorkStealingQueue<T> : IDeque<T>
        where T : class
    {
        private const int c_initialSize = 32;
        private const int c_startIndex = 0;

        private volatile T[] _array = new T[c_initialSize];
        private volatile int v_mask = c_initialSize - 1;

        private volatile int v_headIndex = c_startIndex;
        private volatile int v_tailIndex = c_startIndex;

        private readonly SpinLock _foreignLock = new SpinLock(false);

        public int Count => v_tailIndex - v_headIndex;

        public bool IsEmpty => v_headIndex >= v_tailIndex;

        public bool NonEmpty => v_tailIndex > v_headIndex;

        public bool TryEnqueue(T item)
        {
            int tail = v_tailIndex;

            // We're going to increment the tail; if we'll overflow, then we need to reset our counts
            if (tail == int.MaxValue)
            {
                bool lockTaken = false;
                try
                {
                    _foreignLock.Enter(ref lockTaken);

                    if (v_tailIndex == int.MaxValue)
                    {
                        //
                        // Rather than resetting to zero, we'll just mask off the bits we don't care about.
                        // This way we don't need to rearrange the items already in the queue; they'll be found
                        // correctly exactly where they are.  One subtlety here is that we need to make sure that
                        // if head is currently < tail, it remains that way.  This happens to just fall out from
                        // the bit-masking, because we only do this if tail == int.MaxValue, meaning that all
                        // bits are set, so all of the bits we're keeping will also be set.  Thus it's impossible
                        // for the head to end up > than the tail, since you can't set any more bits than all of 
                        // them.
                        //
                        _ = Interlocked.Exchange(ref v_headIndex, v_headIndex & v_mask);
                        tail = v_tailIndex & v_mask;
                        _ = Interlocked.Exchange(ref v_tailIndex, tail);
                        Debug.Assert(v_headIndex <= v_tailIndex);
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        _foreignLock.Exit(true);
                    }
                }
            }

            // When there are at least 2 elements' worth of space, we can take the fast path.
            if (tail < v_headIndex + v_mask)
            {
                _ = Interlocked.Exchange(ref _array[tail & v_mask], item);
                _ = Interlocked.Exchange(ref v_tailIndex, tail + 1);
            }
            else
            {
                // We need to contend with foreign pops, so we lock.
                bool lockTaken = false;
                try
                {
                    _foreignLock.Enter(ref lockTaken);

                    int head = v_headIndex;
                    int count = v_tailIndex - v_headIndex;

                    // If there is still space (one left), just add the element.
                    if (count >= v_mask)
                    {
                        // We're full; expand the queue by doubling its size.
                        var newArray = new T[_array.Length << 1];
                        for (int i = 0; i < _array.Length; i++)
                        {
                            newArray[i] = _array[(i + head) & v_mask];
                        }

                        // Reset the field values, incl. the mask.
                        _ = Interlocked.Exchange(ref _array, newArray);
                        _ = Interlocked.Exchange(ref v_headIndex, 0);
                        _ = Interlocked.Exchange(ref v_tailIndex, tail = count);
                        _ = Interlocked.Exchange(ref v_mask, (v_mask << 1) | 1);
                    }

                    _ = Interlocked.Exchange(ref _array[tail & v_mask], item);
                    _ = Interlocked.Exchange(ref v_tailIndex, tail + 1);
                }
                finally
                {
                    if (lockTaken)
                    {
                        _foreignLock.Exit(false);
                    }
                }
            }

            return true;
        }

        public bool TryPeek(out T item)
        {
            while (true)
            {
                int tail = v_tailIndex;
                if (v_headIndex >= tail)
                {
                    item = null;
                    return false;
                }
                else
                {
                    int idx = tail & v_mask;
                    item = Volatile.Read(ref _array[idx]);

                    // Check for nulls in the array.
                    if (item is null)
                    {
                        continue;
                    }

                    return true;
                }
            }
        }

        public bool TryDequeue(out T item)
        {
            while (true)
            {
                // Decrement the tail using a fence to ensure subsequent read doesn't come before.
                int tail = v_tailIndex;
                if (v_headIndex >= tail)
                {
                    item = null;
                    return false;
                }

                tail -= 1;
                _ = Interlocked.Exchange(ref v_tailIndex, tail);

                // If there is no interaction with a take, we can head down the fast path.
                if (v_headIndex <= tail)
                {
                    int idx = tail & v_mask;
                    item = Volatile.Read(ref _array[idx]);

                    // Check for nulls in the array.
                    if (item is null)
                    {
                        continue;
                    }

                    _ = Interlocked.Exchange(ref _array[idx], null);
                    return true;
                }
                else
                {
                    // Interaction with takes: 0 or 1 elements left.
                    bool lockTaken = false;
                    try
                    {
                        _foreignLock.Enter(ref lockTaken);

                        if (v_headIndex <= tail)
                        {
                            // Element still available. Take it.
                            int idx = tail & v_mask;
                            item = Volatile.Read(ref _array[idx]);

                            // Check for nulls in the array.
                            if (item is null)
                            {
                                continue;
                            }

                            _ = Interlocked.Exchange(ref _array[idx], null);
                            return true;
                        }
                        else
                        {
                            // We lost the ----, element was stolen, restore the tail.
                            _ = Interlocked.Exchange(ref v_tailIndex, tail + 1);
                            item = null;
                            return false;
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            _foreignLock.Exit(false);
                        }
                    }
                }
            }
        }

        public bool TryDequeueLast(out T item)
        {
            item = null;

            while (true)
            {
                if (v_headIndex >= v_tailIndex)
                {
                    return false;
                }

                bool taken = false;
                try
                {
                    _foreignLock.TryEnter(0, ref taken);
                    if (taken)
                    {
                        // Increment head, and ensure read of tail doesn't move before it (fence).
                        int head = v_headIndex;
                        _ = Interlocked.Exchange(ref v_headIndex, head + 1);

                        if (head < v_tailIndex)
                        {
                            int idx = head & v_mask;
                            item = Volatile.Read(ref _array[idx]);

                            // Check for nulls in the array.
                            if (item is null)
                            {
                                continue;
                            }

                            _ = Interlocked.Exchange(ref _array[idx], null);
                            return true;
                        }
                        else
                        {
                            // Failed, restore head.
                            _ = Interlocked.Exchange(ref v_headIndex, head);
                            item = null;
                        }
                    }
                }
                finally
                {
                    if (taken)
                    {
                        _foreignLock.Exit(false);
                    }
                }

                return false;
            }
        }

        public void Clear()
        {
            _ = Interlocked.Exchange(ref v_headIndex, c_startIndex);
            _ = Interlocked.Exchange(ref v_tailIndex, c_startIndex);
        }
    }
}