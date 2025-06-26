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

namespace DotNetty.Common.Concurrency
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Internal;

    abstract class ScheduledTask : IScheduledRunnable
    {
        private const int CancellationProhibited = 1;
        private const int CancellationRequested = 1 << 1;

        protected readonly IPromise Promise;
        protected readonly AbstractScheduledEventExecutor Executor;
        private int v_cancellationState;
        private int _queueIndex = DefaultPriorityQueue<IScheduledRunnable>.IndexNotInQueue;

        // set once when added to priority queue
        private long _id;

        private long _deadlineNanos;
        /* 0 - no repeat, >0 - repeat at fixed rate, <0 - repeat with fixed delay */
        private readonly long _periodNanos;

        protected ScheduledTask(AbstractScheduledEventExecutor executor, long deadlineNanos, IPromise promise)
        {
            Executor = executor;
            _deadlineNanos = deadlineNanos;
            Promise = promise;
            _periodNanos = 0L;
        }

        protected ScheduledTask(AbstractScheduledEventExecutor executor, long deadlineNanos, long periodNanos, IPromise promise)
        {
            if (0ul >= (ulong)periodNanos) { ThrowHelper.ThrowArgumentException_PeriodMustNotBeEquelToZero(); }

            Executor = executor;
            _deadlineNanos = deadlineNanos;
            _periodNanos = periodNanos;
            Promise = promise;
        }

        IScheduledRunnable IScheduledRunnable.SetId(long id)
        {
            _id = id;
            return this;
        }

        public long Id => _id;

        public long DelayNanos => PreciseTime.DeadlineToDelayNanos(_deadlineNanos);

        public long DeadlineNanos => _deadlineNanos;

        public PreciseTimeSpan Deadline => PreciseTimeSpan.FromTicks(_deadlineNanos);

        void IScheduledRunnable.SetConsumed()
        {
            // Optimization to avoid checking system clock again
            // after deadline has passed and task has been dequeued
            if (0ul >= (ulong)_periodNanos)
            {
                Debug.Assert(PreciseTime.NanoTime() >= _deadlineNanos);
                _deadlineNanos = 0L;
            }
        }

        public bool Cancel()
        {
            if (!AtomicCancellationStateUpdate(CancellationRequested, CancellationProhibited))
            {
                return false;
            }

            bool canceled = Promise.TrySetCanceled();
            if (canceled)
            {
                Executor.RemoveScheduled(this);
            }
            return canceled;
        }

        bool IScheduledRunnable.CancelWithoutRemove()
        {
            return Promise.TrySetCanceled();
        }

        public Task Completion => Promise.Task;

        public TaskAwaiter GetAwaiter() => Completion.GetAwaiter();

        int IComparable<IScheduledRunnable>.CompareTo(IScheduledRunnable other)
        {
            if (other is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other); }

            if (ReferenceEquals(this, other)) { return 0; }

            ulong diff = (ulong)(_deadlineNanos - other.DeadlineNanos);
            if (diff > 0ul)
            {
                if (SharedConstants.TooBigOrNegative64 >= diff) { return 1; }
                return -1;
            }

            if ((ulong)_id > (ulong)other.Id) { return 1; }

            Debug.Assert(_id != other.Id);
            return -1;
        }

        public virtual void Run()
        {
            Debug.Assert(Executor.InEventLoop);
            try
            {
                if ((ulong)DelayNanos > 0UL) // DelayNanos >= 0
                {
                    // Not yet expired, need to add or remove from queue
                    if (Promise.IsCanceled)
                    {
                        _ = Executor._scheduledTaskQueue.TryRemove(this);
                    }
                    else
                    {
                        Executor.ScheduleFromEventLoop(this);
                    }
                    return;
                }
                if (0ul >= (ulong)_periodNanos)
                {
                    if (TrySetUncancelable())
                    {
                        Execute();
                        _ = Promise.TryComplete();
                    }
                }
                else
                {
                    // check if is done as it may was cancelled
                    if (!Promise.IsCanceled)
                    {
                        Execute();
                        if (!Executor.IsShutdown)
                        {
                            if (_periodNanos > 0)
                            {
                                _deadlineNanos += _periodNanos;
                            }
                            else
                            {
                                _deadlineNanos = PreciseTime.NanoTime() - _periodNanos;
                            }
                            if (!Promise.IsCanceled)
                            {
                                _ = Executor._scheduledTaskQueue.TryEnqueue(this);
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                _ = Promise.TrySetException(exc);
            }
        }

        protected abstract void Execute();

        private bool TrySetUncancelable() => AtomicCancellationStateUpdate(CancellationProhibited, CancellationRequested);

        private bool AtomicCancellationStateUpdate(int newBits, int illegalBits)
        {
            int cancellationState = Volatile.Read(ref v_cancellationState);
            int oldCancellationState;
            do
            {
                oldCancellationState = cancellationState;
                if ((cancellationState & illegalBits) != 0)
                {
                    return false;
                }
                cancellationState = Interlocked.CompareExchange(ref v_cancellationState, cancellationState | newBits, cancellationState);
            }
            while (cancellationState != oldCancellationState);

            return true;
        }

        public int GetPriorityQueueIndex(IPriorityQueue<IScheduledRunnable> queue) => _queueIndex;

        public void SetPriorityQueueIndex(IPriorityQueue<IScheduledRunnable> queue, int i) => _queueIndex = i;
    }
}