// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Security;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotNetty.Common.Internal
{
    /// <summary>A simple synchronized pool would simply lock a stack and push/pop on return/take.</summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This implementation tries to reduce locking by exploiting the case where an item
    /// is taken and returned by the same thread, which turns out to be common in our 
    /// scenarios.  
    ///
    /// Initially, all the quota is allocated to a global (non-thread-specific) pool, 
    /// which takes locks.  As different threads take and return values, we record their IDs, 
    /// and if we detect that a thread is taking and returning "enough" on the same thread, 
    /// then we decide to "promote" the thread.  When a thread is promoted, we decrease the 
    /// quota of the global pool by one, and allocate a thread-specific entry for the thread 
    /// to store it's value.  Once this entry is allocated, the thread can take and return 
    /// it's value from that entry without taking any locks.  Not only does this avoid 
    /// locks, but it affinitizes pooled items to a particular thread.
    ///
    /// There are a couple of additional things worth noting:
    /// 
    /// It is possible for a thread that we have reserved an entry for to exit.  This means
    /// we will still have a entry allocated for it, but the pooled item stored there 
    /// will never be used.  After a while, we could end up with a number of these, and 
    /// as a result we would begin to exhaust the quota of the overall pool.  To mitigate this
    /// case, we throw away the entire per-thread pool, and return all the quota back to 
    /// the global pool if we are unable to promote a thread (due to lack of space).  Then 
    /// the set of active threads will be re-promoted as they take and return items.
    /// 
    /// You may notice that the code does not immediately promote a thread, and does not
    /// immediately throw away the entire per-thread pool when it is unable to promote a 
    /// thread.  Instead, it uses counters (based on the number of calls to the pool) 
    /// and a threshold to figure out when to do these operations.  In the case where the
    /// pool to misconfigured to have too few items for the workload, this avoids constant 
    /// promoting and rebuilding of the per thread entries.
    ///
    /// You may also notice that we do not use interlocked methods when adjusting statistics.
    /// Since the statistics are a heuristic as to how often something is happening, they 
    /// do not need to be perfect.
    /// </remarks>
    internal sealed class SynchronizedPool<T> where T : class
    {
        private const int maxPendingEntries = 128;
        private const int maxPromotionFailures = 64;
        private const int maxReturnsBeforePromotion = 64;
        private const int maxThreadItemsPerProcessor = 16;
        private const int zeroThreadID = 0;

        private Entry[] _entries;
        private GlobalPool _globalPool;
        private int _maxCount;
        private PendingEntry[] _pending;
        private int _promotionFailures;

        internal SynchronizedPool(int maxCount)
        {
            var threadCount = maxCount;
            int maxThreadCount = maxThreadItemsPerProcessor + SynchronizedPoolHelper.ProcessorCount;
            if (threadCount > maxThreadCount) { threadCount = maxThreadCount; }

            _maxCount = maxCount;
            _entries = new Entry[threadCount];
            _pending = new PendingEntry[4];
            _globalPool = new GlobalPool(maxCount);
        }

        private object ThisLock => this;

        public void Clear()
        {
            Entry[] entries = _entries;

            for (int i = 0; i < entries.Length; i++)
            {
                entries[i].value = null;
            }

            _globalPool.Clear();
        }

        private void HandlePromotionFailure(int thisThreadID)
        {
            int newPromotionFailures = _promotionFailures + 1;

            if (newPromotionFailures >= maxPromotionFailures)
            {
                lock (ThisLock)
                {
                    _entries = new Entry[_entries.Length];

                    _globalPool.MaxCount = _maxCount;
                }

                _ = PromoteThread(thisThreadID);
            }
            else
            {
                _promotionFailures = newPromotionFailures;
            }
        }

        private bool PromoteThread(int thisThreadID)
        {
            lock (ThisLock)
            {
                for (int i = 0; i < _entries.Length; i++)
                {
                    int threadID = _entries[i].threadID;

                    if (threadID == thisThreadID)
                    {
                        return true;
                    }
                    else if (zeroThreadID == threadID)
                    {
                        _globalPool.DecrementMaxCount();
                        _entries[i].threadID = thisThreadID;
                        return true;
                    }
                }
            }

            return false;
        }

        private void RecordReturnToGlobalPool(int thisThreadID)
        {
            PendingEntry[] localPending = _pending;

            for (int i = 0; i < localPending.Length; i++)
            {
                int threadID = localPending[i].threadID;

                if (threadID == thisThreadID)
                {
                    int newReturnCount = localPending[i].returnCount + 1;

                    if (newReturnCount >= maxReturnsBeforePromotion)
                    {
                        localPending[i].returnCount = 0;

                        if (!PromoteThread(thisThreadID))
                        {
                            HandlePromotionFailure(thisThreadID);
                        }
                    }
                    else
                    {
                        localPending[i].returnCount = newReturnCount;
                    }
                    break;
                }
                else if (zeroThreadID == threadID)
                {
                    break;
                }
            }
        }

        private void RecordTakeFromGlobalPool(int thisThreadID)
        {
            PendingEntry[] localPending = _pending;

            for (int i = 0; i < localPending.Length; i++)
            {
                int threadID = localPending[i].threadID;

                if (threadID == thisThreadID)
                {
                    return;
                }
                else if (zeroThreadID == threadID)
                {
                    lock (localPending)
                    {
                        if (zeroThreadID == localPending[i].threadID)
                        {
                            localPending[i].threadID = thisThreadID;
                            return;
                        }
                    }
                }
            }

            if (localPending.Length >= maxPendingEntries)
            {
                _pending = new PendingEntry[localPending.Length];
            }
            else
            {
                PendingEntry[] newPending = new PendingEntry[localPending.Length * 2];
                Array.Copy(localPending, newPending, localPending.Length);
                _pending = newPending;
            }
        }

        public bool Return(T value)
        {
            int thisThreadID = Thread.CurrentThread.ManagedThreadId;

            if (zeroThreadID == thisThreadID) { return false; }

            if (ReturnToPerThreadPool(thisThreadID, value)) { return true; }

            return ReturnToGlobalPool(thisThreadID, value);
        }

        private bool ReturnToPerThreadPool(int thisThreadID, T value)
        {
            Entry[] entries = _entries;

            for (int i = 0; i < entries.Length; i++)
            {
                int threadID = entries[i].threadID;

                if (threadID == thisThreadID)
                {
                    if (entries[i].value == null)
                    {
                        entries[i].value = value;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (zeroThreadID == threadID)
                {
                    break;
                }
            }

            return false;
        }

        private bool ReturnToGlobalPool(int thisThreadID, T value)
        {
            RecordReturnToGlobalPool(thisThreadID);

            return _globalPool.Return(value);
        }

        public T Take()
        {
            int thisThreadID = Thread.CurrentThread.ManagedThreadId;

            if (zeroThreadID == thisThreadID) { return null; }

            T value = TakeFromPerThreadPool(thisThreadID);

            if (value != null) { return value; }

            return TakeFromGlobalPool(thisThreadID);
        }

        private T TakeFromPerThreadPool(int thisThreadID)
        {
            Entry[] entries = _entries;

            for (int i = 0; i < entries.Length; i++)
            {
                int threadID = entries[i].threadID;

                if (threadID == thisThreadID)
                {
                    T value = entries[i].value;

                    if (value != null)
                    {
                        entries[i].value = null;
                        return value;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (zeroThreadID == threadID)
                {
                    break;
                }
            }

            return null;
        }

        private T TakeFromGlobalPool(int thisThreadID)
        {
            RecordTakeFromGlobalPool(thisThreadID);

            return _globalPool.Take();
        }

        private struct Entry
        {
            public int threadID;
            public T value;
        }

        private struct PendingEntry
        {
            public int returnCount;
            public int threadID;
        }

        internal static class SynchronizedPoolHelper
        {
            public static readonly int ProcessorCount = GetProcessorCount();

            [SecuritySafeCritical]
            private static int GetProcessorCount() => Environment.ProcessorCount;
        }

        internal class GlobalPool
        {
            private Stack<T> _items;

            private int _maxCount;

            internal GlobalPool(int maxCount)
            {
                _items = new Stack<T>();
                _maxCount = maxCount;
            }

            internal int MaxCount
            {
                [MethodImpl(InlineMethod.AggressiveOptimization)]
                get => _maxCount;
                set
                {
                    lock (ThisLock)
                    {
                        while (_items.Count > value)
                        {
                            _ = _items.Pop();
                        }
                        _maxCount = value;
                    }
                }
            }

            private object ThisLock => this;

            internal void DecrementMaxCount()
            {
                lock (ThisLock)
                {
                    if (_items.Count == _maxCount)
                    {
                        _ = _items.Pop();
                    }
                    _maxCount--;
                }
            }

            internal T Take()
            {
                if (_items.Count > 0)
                {
                    lock (ThisLock)
                    {
                        if (_items.Count > 0)
                        {
                            return _items.Pop();
                        }
                    }
                }
                return null;
            }

            internal bool Return(T value)
            {
                if (_items.Count < this.MaxCount)
                {
                    lock (ThisLock)
                    {
                        if (_items.Count < this.MaxCount)
                        {
                            _items.Push(value);
                            return true;
                        }
                    }
                }
                return false;
            }

            internal void Clear()
            {
                lock (ThisLock)
                {
                    _items.Clear();
                }
            }
        }
    }
}
