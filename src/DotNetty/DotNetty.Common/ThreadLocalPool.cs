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
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Common
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using Thread = DotNetty.Common.Concurrency.XThread;

    public class ThreadLocalPool
    {
        public abstract class Handle
        {
            public abstract void Release<T>(T value)
                where T : class;
        }

        protected sealed class NoopHandle : Handle
        {
            public static readonly NoopHandle Instance = new NoopHandle();

            NoopHandle()
            {
            }

            public override void Release<T>(T value)
            {
            }
        }

        protected sealed class DefaultHandle : Handle
        {
            internal int _lastRecycledId;
            internal int _recycleId;

            internal bool _hasBeenRecycled;

            internal object Value;
            internal Stack Stack;

            internal DefaultHandle(Stack stack)
            {
                Stack = stack;
            }

            public override void Release<T>(T value)
            {
                if (value != Value) { ThrowHelper.ThrowArgumentException_ValueDiffers(); }

                Stack stack = Stack;
                if (_lastRecycledId != _recycleId || stack is null)
                {
                    ThrowHelper.ThrowInvalidOperationException_RecycledAlready();
                }
                stack.Push(this);
            }
        }

        // a queue that makes only moderate guarantees about visibility: items are seen in the correct order,
        // but we aren't absolutely guaranteed to ever see anything at all, thereby keeping the queue cheap to maintain
        protected sealed class WeakOrderQueue
        {
            internal static readonly WeakOrderQueue Dummy = new WeakOrderQueue();

            // Let Link extend AtomicInteger for intrinsics. The Link itself will be used as writerIndex.
            sealed class Link
            {
                private int v_writeIndex;

                internal readonly DefaultHandle[] _elements = new DefaultHandle[LinkCapacity];
                internal Link _next;

                internal int ReadIndex { get; set; }

                internal int WriteIndex
                {
                    get => Volatile.Read(ref v_writeIndex);
                    set => Volatile.Write(ref v_writeIndex, value);
                }

                internal void LazySetWriteIndex(int value) => v_writeIndex = value;
            }

            // Its important this does not hold any reference to either Stack or WeakOrderQueue.
            sealed class Head
            {
                readonly StrongBox<int> _availableSharedCapacity;
                readonly StrongBox<int> _weakTableCounter;

                internal Link _link;

                internal Head(StrongBox<int> availableSharedCapacity, StrongBox<int> weakTableCounter)
                {
                    _availableSharedCapacity = availableSharedCapacity;
                    _weakTableCounter = weakTableCounter;
                    if (weakTableCounter is object) { _ = Interlocked.Increment(ref weakTableCounter.Value); }
                }

                /// <summary>
                /// Reclaim all used space and also unlink the nodes to prevent GC nepotism.
                /// </summary>
                internal void ReclaimAllSpaceAndUnlink()
                {
                    if (_weakTableCounter is object)
                    {
                        _ = Interlocked.Decrement(ref _weakTableCounter.Value);
                    }
                    if (_availableSharedCapacity is null)
                    {
                        return;
                    }

                    Link head = _link;
                    _link = null;
                    int reclaimSpace = 0;
                    while (head != null)
                    {
                        reclaimSpace += LinkCapacity;
                        Link next = head._next;
                        // Unlink to help GC and guard against GC nepotism.
                        head._next = null;
                        head = next;
                    }
                    if (reclaimSpace > 0)
                    {
                        ReclaimSpace(reclaimSpace);
                    }
                }

                private void ReclaimSpace(int space)
                {
                    _ = Interlocked.Add(ref _availableSharedCapacity.Value, space);
                }

                internal void Relink(Link link)
                {
                    ReclaimSpace(LinkCapacity);
                    _link = link;
                }

                /// <summary>
                /// Creates a new <see cref="Link"/> and returns it if we can reserve enough space for it, otherwise it
                /// returns <c>null</c>.
                /// </summary>
                internal Link NewLink()
                {
                    return ReserveSpaceForLink(_availableSharedCapacity) ? new Link() : null;
                }

                internal static bool ReserveSpaceForLink(StrongBox<int> availableSharedCapacity)
                {
                    for (; ; )
                    {
                        int available = Volatile.Read(ref availableSharedCapacity.Value);
                        if (available < LinkCapacity)
                        {
                            return false;
                        }
                        if (Interlocked.CompareExchange(ref availableSharedCapacity.Value, available - LinkCapacity, available) == available)
                        {
                            return true;
                        }
                    }
                }
            }

            // chain of data items
            private readonly Head _head;
            private Link _tail;
            // pointer to another queue of delayed items for the same stack
            internal WeakOrderQueue _next;
            internal readonly WeakReference<Thread> _owner;
            private readonly int _id = Interlocked.Increment(ref s_idSource);
            private readonly int _interval;
            private int _handleRecycleCount;

            WeakOrderQueue()
            {
                _owner = null;
                _head = new Head(null, null);
                _interval = 0;
            }

            WeakOrderQueue(Stack stack, Thread thread, DelayedThreadLocal.CountedWeakTable countedWeakTable)
            {
                _tail = new Link();

                // Its important that we not store the Stack itself in the WeakOrderQueue as the Stack also is used in
                // the WeakHashMap as key. So just store the enclosed AtomicInteger which should allow to have the
                // Stack itself GCed.
                _head = new Head(stack._availableSharedCapacity, countedWeakTable.Counter);
                _head._link = _tail;
                _owner = new WeakReference<Thread>(thread);
                _interval = stack._delayedQueueInterval;
                _handleRecycleCount = _interval; // Start at interval so the first one will be recycled.
            }

            internal static WeakOrderQueue NewQueue(Stack stack, Thread thread, DelayedThreadLocal.CountedWeakTable countedWeakTable)
            {
                // We allocated a Link so reserve the space
                if (!Head.ReserveSpaceForLink(stack._availableSharedCapacity))
                {
                    return null;
                }
                WeakOrderQueue queue = new WeakOrderQueue(stack, thread, countedWeakTable);
                // Done outside of the constructor to ensure WeakOrderQueue.this does not escape the constructor and so
                // may be accessed while its still constructed.
                stack.Head = queue;

                return queue;
            }

            internal WeakOrderQueue Next
            {
                get => _next;
                set
                {
                    Debug.Assert(value != this);
                    _next = value;
                }
            }

            internal void ReclaimAllSpaceAndUnlink()
            {
                _head.ReclaimAllSpaceAndUnlink();
                _next = null;
            }

            internal void Add(DefaultHandle handle)
            {
                handle._lastRecycledId = _id;

                // While we also enforce the recycling ratio when we transfer objects from the WeakOrderQueue to the Stack
                // we better should enforce it as well early. Missing to do so may let the WeakOrderQueue grow very fast
                // without control
                if (_handleRecycleCount < _interval)
                {
                    _handleRecycleCount++;
                    // Drop the item to prevent recycling to aggressive.
                    return;
                }
                _handleRecycleCount = 0;

                Link tail = _tail;
                int writeIndex = tail.WriteIndex;
                if (writeIndex == LinkCapacity)
                {
                    Link link = _head.NewLink();
                    if (link is null)
                    {
                        // Drop it.
                        return;
                    }
                    // We allocate a Link so reserve the space
                    _tail = tail = tail._next = link;
                    writeIndex = tail.WriteIndex;
                }
                tail._elements[writeIndex] = handle;
                handle.Stack = null;
                // we lazy set to ensure that setting stack to null appears before we unnull it in the owning thread;
                // this also means we guarantee visibility of an element in the queue if we see the index updated
                tail.LazySetWriteIndex(writeIndex + 1);
            }

            internal bool HasFinalData => _tail.ReadIndex != _tail.WriteIndex;

            // transfer as many items as we can from this queue to the stack, returning true if any were transferred
            internal bool Transfer(Stack dst)
            {
                Link head = _head._link;
                if (head is null)
                {
                    return false;
                }

                if (head.ReadIndex == LinkCapacity)
                {
                    if (head._next is null)
                    {
                        return false;
                    }
                    head = head._next;
                    _head.Relink(head);
                }

                int srcStart = head.ReadIndex;
                int srcEnd = head.WriteIndex;
                int srcSize = srcEnd - srcStart;
                if (0u >= (uint)srcSize)
                {
                    return false;
                }

                int dstSize = dst._size;
                int expectedCapacity = dstSize + srcSize;

                if (expectedCapacity > dst._elements.Length)
                {
                    int actualCapacity = dst.IncreaseCapacity(expectedCapacity);
                    srcEnd = Math.Min(srcStart + actualCapacity - dstSize, srcEnd);
                }

                if (srcStart != srcEnd)
                {
                    DefaultHandle[] srcElems = head._elements;
                    DefaultHandle[] dstElems = dst._elements;
                    int newDstSize = dstSize;
                    for (int i = srcStart; i < srcEnd; i++)
                    {
                        DefaultHandle element = srcElems[i];
                        if (0u >= (uint)element._recycleId)
                        {
                            element._recycleId = element._lastRecycledId;
                        }
                        else if (element._recycleId != element._lastRecycledId)
                        {
                            ThrowInvalidOperationException_recycled_already();
                        }
                        srcElems[i] = null;

                        if (dst.DropHandle(element))
                        {
                            // Drop the object.
                            continue;
                        }
                        element.Stack = dst;
                        dstElems[newDstSize++] = element;
                    }

                    if (srcEnd == LinkCapacity && head._next is object)
                    {
                        // Add capacity back as the Link is GCed.
                        _head.Relink(head._next);
                    }

                    head.ReadIndex = srcEnd;
                    if (dst._size == newDstSize)
                    {
                        return false;
                    }
                    dst._size = newDstSize;
                    return true;
                }
                else
                {
                    // The destination stack is full already.
                    return false;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowInvalidOperationException_recycled_already()
            {
                throw GetException();
                static InvalidOperationException GetException()
                {
                    return new InvalidOperationException("recycled already");
                }
            }
        }

        protected sealed class Stack
        {
            // we keep a queue of per-thread queues, which is appended to once only, each time a new thread other
            // than the stack owner recycles: when we run out of items in our stack we iterate this collection
            // to scavenge those that can be reused. this permits us to incur minimal thread synchronisation whilst
            // still recycling all items.
            internal readonly ThreadLocalPool _parent;

            // We store the Thread in a WeakReference as otherwise we may be the only ones that still hold a strong
            // Reference to the Thread itself after it died because DefaultHandle will hold a reference to the Stack.
            //
            // The biggest issue is if we do not use a WeakReference the Thread may not be able to be collected at all if
            // the user will store a reference to the DefaultHandle somewhere and never clear this reference (or not clear
            // it in a timely manner).
            internal readonly WeakReference<Thread> _threadRef;
            internal readonly StrongBox<int> _availableSharedCapacity;
            internal readonly int _maxDelayedQueues;

            private readonly int _maxCapacity;
            internal readonly int _interval;
            internal readonly int _delayedQueueInterval;
            internal DefaultHandle[] _elements;
            internal int _size;
            private int _handleRecycleCount;
            private WeakOrderQueue _cursorQueue, _prevQueue;
            private volatile WeakOrderQueue _headQueue; 
            internal Stack(ThreadLocalPool parent, Thread thread, int maxCapacity, int maxSharedCapacityFactor,
                int interval, int maxDelayedQueues, int delayedQueueInterval)
            {
                _parent = parent;
                _threadRef = new WeakReference<Thread>(thread);
                _maxCapacity = maxCapacity;
                _availableSharedCapacity = new StrongBox<int>(Math.Max(maxCapacity / maxSharedCapacityFactor, LinkCapacity));
                _elements = new DefaultHandle[Math.Min(DefaultInitialCapacity, maxCapacity)];
                _interval = interval;
                _delayedQueueInterval = delayedQueueInterval;
                _handleRecycleCount = interval; // Start at interval so the first one will be recycled.
                _maxDelayedQueues = maxDelayedQueues;
            }

            internal WeakOrderQueue Head
            {
                set
                {
                    lock (this)
                    {
                        value._next = _headQueue;
                        _headQueue = value;
                    }
                }
            }

            internal int IncreaseCapacity(int expectedCapacity)
            {
                int newCapacity = _elements.Length;
                int maxCapacity = _maxCapacity;
                do
                {
                    newCapacity <<= 1;
                }
                while (newCapacity < expectedCapacity && newCapacity < maxCapacity);

                newCapacity = Math.Min(newCapacity, maxCapacity);
                if (newCapacity != _elements.Length)
                {
                    Array.Resize(ref _elements, newCapacity);
                }

                return newCapacity;
            }

            internal void Push(DefaultHandle item)
            {
                Thread currentThread = Thread.CurrentThread;
                if (_threadRef.TryGetTarget(out Thread thread) && thread == currentThread)
                {
                    // The current Thread is the thread that belongs to the Stack, we can try to push the object now.
                    PushNow(item);
                }
                else
                {
                    // The current Thread is not the one that belongs to the Stack
                    // (or the Thread that belonged to the Stack was collected already), we need to signal that the push
                    // happens later.
                    PushLater(item, currentThread);
                }
            }

            void PushNow(DefaultHandle item)
            {
                if ((item._recycleId | item._lastRecycledId) != 0)
                {
                    ThrowHelper.ThrowInvalidOperationException_ReleasedAlready();
                }
                item._recycleId = item._lastRecycledId = s_ownThreadId;

                int size = _size;
                if (size >= _maxCapacity || DropHandle(item))
                {
                    // Hit the maximum capacity - drop the possibly youngest object.
                    return;
                }
                if (size == _elements.Length)
                {
                    Array.Resize(ref _elements, Math.Min(size << 1, _maxCapacity));
                }

                _elements[size] = item;
                _size = size + 1;
            }

            void PushLater(DefaultHandle item, Thread thread)
            {
                if (0u >= (uint)_maxDelayedQueues)
                {
                    // We don't support recycling across threads and should just drop the item on the floor.
                    return;
                }

                // we don't want to have a ref to the queue as the value in our weak map
                // so we null it out; to ensure there are no races with restoring it later
                // we impose a memory ordering here (no-op on x86)
                DelayedThreadLocal.CountedWeakTable countedWeakTable = DelayedPool.Value;
                ConditionalWeakTable<Stack, WeakOrderQueue> delayedRecycled = countedWeakTable.WeakTable;
                _ = delayedRecycled.TryGetValue(this, out WeakOrderQueue queue);
                if (queue is null)
                {
                    if (Volatile.Read(ref countedWeakTable.Counter.Value) >= _maxDelayedQueues)
                    {
                        // Add a dummy queue so we know we should drop the object
                        delayedRecycled.Add(this, WeakOrderQueue.Dummy);
                        return;
                    }
                    // Check if we already reached the maximum number of delayed queues and if we can allocate at all.
                    if ((queue = NewWeakOrderQueue(thread, countedWeakTable)) is null)
                    {
                        // drop object
                        return;
                    }
                    delayedRecycled.Add(this, queue);
                }
                else if (queue == WeakOrderQueue.Dummy)
                {
                    // drop object
                    return;
                }

                queue.Add(item);
            }

            /// <summary>
            /// Allocate a new <see cref="WeakOrderQueue"/> or return <c>null</c> if not possible.
            /// </summary>
            private WeakOrderQueue NewWeakOrderQueue(Thread thread, DelayedThreadLocal.CountedWeakTable countedWeakTable)
            {
                return WeakOrderQueue.NewQueue(this, thread, countedWeakTable);
            }

            internal bool DropHandle(DefaultHandle handle)
            {
                if (!handle._hasBeenRecycled)
                {
                    if (_handleRecycleCount < _interval)
                    {
                        _handleRecycleCount++;
                        // Drop the object.
                        return true;
                    }
                    _handleRecycleCount = 0;
                    handle._hasBeenRecycled = true;
                }
                return false;
            }

            internal DefaultHandle NewHandle() => new DefaultHandle(this);

            internal bool TryPop(out DefaultHandle item)
            {

                int size = _size;
                if (0u >= (uint)size)
                {
                    if (!Scavenge())
                    {
                        goto Failed;
                    }
                    size = _size;
                    if ((uint)(size - 1) > SharedConstants.TooBigOrNegative)
                    {
                        // double check, avoid races
                        goto Failed;
                    }
                }
                size--;
                DefaultHandle ret = _elements[size];
                _elements[size] = null;
                // As we already set the element[size] to null we also need to store the updated size before we do
                // any validation. Otherwise we may see a null value when later try to pop again without a new element
                // added before.
                _size = size;

                if (ret._lastRecycledId != ret._recycleId)
                {
                    ThrowHelper.ThrowInvalidOperationException_RecycledMultiTimes();
                }
                ret._recycleId = 0;
                ret._lastRecycledId = 0;

                item = ret;
                return true;
                Failed:
                item = null;
                return false;
            }

            private bool Scavenge()
            {
                // continue an existing scavenge, if any
                if (ScavengeSome())
                {
                    return true;
                }

                // reset our scavenge cursor
                _prevQueue = null;
                _cursorQueue = _headQueue;
                return false;

            }

            private bool ScavengeSome()
            {
                WeakOrderQueue prev;
                WeakOrderQueue cursor = _cursorQueue;
                if (cursor is null)
                {
                    prev = null;
                    cursor = _headQueue;
                    if (cursor is null)
                    {
                        return false;
                    }
                }
                else
                {
                    prev = _prevQueue;
                }

                bool success = false;
                do
                {
                    if (cursor.Transfer(this))
                    {
                        success = true;
                        break;
                    }

                    WeakOrderQueue next = cursor._next;
                    if (!cursor._owner.TryGetTarget(out _))
                    {
                        // If the thread associated with the queue is gone, unlink it, after
                        // performing a volatile read to confirm there is no data left to collect.
                        // We never unlink the first queue, as we don't want to synchronize on updating the head.
                        if (cursor.HasFinalData)
                        {
                            for (; ; )
                            {
                                if (cursor.Transfer(this))
                                {
                                    success = true;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        if (prev is object)
                        {
                            // Ensure we reclaim all space before dropping the WeakOrderQueue to be GC'ed.
                            cursor.ReclaimAllSpaceAndUnlink();
                            prev.Next = next;
                        }
                    }
                    else
                    {
                        prev = cursor;
                    }

                    cursor = next;
                }
                while (cursor is object && !success);

                _prevQueue = prev;
                _cursorQueue = cursor;
                return success;
            }
        }

        private const int DefaultInitialMaxCapacityPerThread = 4 * 1024; // Use 4k instances as default.
        internal protected static readonly int DefaultMaxCapacityPerThread;
        protected static readonly int DefaultInitialCapacity;
        protected static readonly int DefaultMaxSharedCapacityFactor;
        protected static readonly int DefaultMaxDelayedQueuesPerThread;
        protected static readonly int LinkCapacity;
        protected static readonly int DefaultRatio;
        protected static readonly int DelayedQueueRatio;
        private static int s_idSource = int.MinValue;
        private static readonly int s_ownThreadId = Interlocked.Increment(ref s_idSource);

        protected static readonly DelayedThreadLocal DelayedPool = new DelayedThreadLocal();

        protected sealed class DelayedThreadLocal : FastThreadLocal<DelayedThreadLocal.CountedWeakTable>
        {
            public sealed class CountedWeakTable
            {
                internal readonly ConditionalWeakTable<Stack, WeakOrderQueue> WeakTable = new ConditionalWeakTable<Stack, WeakOrderQueue>();

                internal readonly StrongBox<int> Counter = new StrongBox<int>();
            }
            protected override CountedWeakTable GetInitialValue() => new CountedWeakTable();
        }

        static ThreadLocalPool()
        {
            // In the future, we might have different maxCapacity for different object types.
            // e.g. io.netty.recycler.maxCapacity.writeTask
            //      io.netty.recycler.maxCapacity.outboundBuffer
            int maxCapacityPerThread = SystemPropertyUtil.GetInt("io.netty.recycler.maxCapacityPerThread",
                    SystemPropertyUtil.GetInt("io.netty.recycler.maxCapacity", DefaultInitialMaxCapacityPerThread));
            if (maxCapacityPerThread < 0)
            {
                maxCapacityPerThread = DefaultInitialMaxCapacityPerThread;
            }

            DefaultMaxCapacityPerThread = maxCapacityPerThread;

            DefaultMaxSharedCapacityFactor = Math.Max(2,
                    SystemPropertyUtil.GetInt("io.netty.recycler.maxSharedCapacityFactor",
                            2));

            DefaultMaxDelayedQueuesPerThread = Math.Max(0,
                    SystemPropertyUtil.GetInt("io.netty.recycler.maxDelayedQueuesPerThread",
                            // We use the same value as default EventLoop number
                            Environment.ProcessorCount * 2));

            LinkCapacity = MathUtil.SafeFindNextPositivePowerOfTwo(
                    Math.Max(SystemPropertyUtil.GetInt("io.netty.recycler.linkCapacity", 16), 16));

            // By default we allow one push to a Recycler for each 8th try on handles that were never recycled before.
            // This should help to slowly increase the capacity of the recycler while not be too sensitive to allocation
            // bursts.
            DefaultRatio = Math.Max(0, SystemPropertyUtil.GetInt("io.netty.recycler.ratio", 8));
            DelayedQueueRatio = Math.Max(0, SystemPropertyUtil.GetInt("io.netty.recycler.delayedQueue.ratio", DefaultRatio));

            IInternalLogger logger = InternalLoggerFactory.GetInstance(typeof(ThreadLocalPool));
            if (logger.DebugEnabled)
            {
                if (0u >= (uint)DefaultMaxCapacityPerThread)
                {
                    logger.Debug("-Dio.netty.recycler.maxCapacityPerThread: disabled");
                    logger.Debug("-Dio.netty.recycler.maxSharedCapacityFactor: disabled");
                    logger.Debug("-Dio.netty.recycler.maxDelayedQueuesPerThread: disabled");
                    logger.Debug("-Dio.netty.recycler.linkCapacity: disabled");
                    logger.Debug("-Dio.netty.recycler.ratio: disabled");
                    logger.Debug("-Dio.netty.recycler.delayedQueue.ratio: disabled");
                }
                else
                {
                    logger.Debug("-Dio.netty.recycler.maxCapacityPerThread: {}", DefaultMaxCapacityPerThread);
                    logger.Debug("-Dio.netty.recycler.maxSharedCapacityFactor: {}", DefaultMaxSharedCapacityFactor);
                    logger.Debug("-Dio.netty.recycler.maxDelayedQueuesPerThread: {}", DefaultMaxDelayedQueuesPerThread);
                    logger.Debug("-Dio.netty.recycler.linkCapacity: {}", LinkCapacity);
                    logger.Debug("-Dio.netty.recycler.ratio: {}", DefaultRatio);
                    logger.Debug("-Dio.netty.recycler.delayedQueue.ratio: {}", DelayedQueueRatio);
                }
            }

            DefaultInitialCapacity = Math.Min(DefaultMaxCapacityPerThread, 256);
        }

        public ThreadLocalPool(int maxCapacityPerThread)
            : this(maxCapacityPerThread, DefaultMaxSharedCapacityFactor, DefaultRatio, DefaultMaxDelayedQueuesPerThread)
        {
        }
        public ThreadLocalPool(int maxCapacityPerThread, int maxSharedCapacityFactor, int ratio, int maxDelayedQueuesPerThread)
            : this(maxCapacityPerThread, maxSharedCapacityFactor, ratio, maxDelayedQueuesPerThread, DelayedQueueRatio)
        {

        }

        public ThreadLocalPool(int maxCapacityPerThread, int maxSharedCapacityFactor,
            int ratio, int maxDelayedQueuesPerThread, int delayedQueueRatio)
        {
            _interval = Math.Max(0, ratio);
            _delayedQueueInterval = Math.Max(0, delayedQueueRatio);
            if ((uint)(maxCapacityPerThread - 1) > SharedConstants.TooBigOrNegative) // <= 0
            {
                _maxCapacityPerThread = 0;
                _maxSharedCapacityFactor = 1;
                _maxDelayedQueuesPerThread = 0;
            }
            else
            {
                _maxCapacityPerThread = maxCapacityPerThread;
                _maxSharedCapacityFactor = Math.Max(1, maxSharedCapacityFactor);
                _maxDelayedQueuesPerThread = Math.Max(0, maxDelayedQueuesPerThread);
            }
        }

        protected readonly int _maxCapacityPerThread;
        protected readonly int _interval;
        protected readonly int _maxSharedCapacityFactor;
        protected readonly int _maxDelayedQueuesPerThread;
        protected readonly int _delayedQueueInterval;
    }
}