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

namespace DotNetty.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;

    public sealed class HashedWheelTimer : ITimer
    {
        private static readonly IInternalLogger Logger =
            InternalLoggerFactory.GetInstance<HashedWheelTimer>();

        private static int v_instanceCounter;
        private static int v_warnedTooManyInstances;

        private const int InstanceCountLimit = 64;

        private readonly Worker _worker;
        private readonly XThread _workerThread;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private const int WorkerStateInit = 0;
        private const int WorkerStateStarted = 1;
        private const int WorkerStateShutdown = 2;
        private int v_workerState = WorkerStateInit; // 0 - init, 1 - started, 2 - shut down

        private readonly long _tickDuration;
        private readonly HashedWheelBucket[] _wheel;
        private readonly int _mask;
        private readonly CountdownEvent _startTimeInitialized = new CountdownEvent(1);
        private readonly IQueue<HashedWheelTimeout> _timeouts = PlatformDependent.NewMpscQueue<HashedWheelTimeout>();
        private readonly IQueue<HashedWheelTimeout> _cancelledTimeouts = PlatformDependent.NewMpscQueue<HashedWheelTimeout>();
        private readonly long _maxPendingTimeouts;
        private long v_pendingTimeouts;
        private long v_startTime;

        /// <summary>Creates a new timer.</summary>
        public HashedWheelTimer()
            : this(TimeSpan.FromMilliseconds(100), 512, -1)
        {
        }

        /// <summary>Creates a new timer.</summary>
        /// <param name="tickInterval">the interval between two consecutive ticks</param>
        /// <param name="ticksPerWheel">the size of the wheel</param>
        /// <param name="maxPendingTimeouts">The maximum number of pending timeouts after which call to
        /// <c>newTimeout</c> will result in <see cref="RejectedExecutionException"/> being thrown.
        /// No maximum pending timeouts limit is assumed if this value is 0 or negative.</param>
        /// <exception cref="ArgumentException">if either of <c>tickInterval</c> and <c>ticksPerWheel</c> is &lt;= 0</exception>
        public HashedWheelTimer(TimeSpan tickInterval, int ticksPerWheel, long maxPendingTimeouts)
        {
            if (tickInterval <= TimeSpan.Zero)
            {
                ThrowHelper.ThrowArgumentException_MustBeGreaterThanZero(tickInterval);
            }
            if (Math.Ceiling(tickInterval.TotalMilliseconds) > int.MaxValue)
            {
                ThrowHelper.ThrowArgumentException_MustBeLessThanOrEqualTo();
            }
            if (ticksPerWheel <= 0)
            {
                ThrowHelper.ThrowArgumentException_MustBeGreaterThanZero(ticksPerWheel);
            }
            if (ticksPerWheel > int.MaxValue / 2 + 1)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_MustBeGreaterThan(ticksPerWheel);
            }

            // Normalize ticksPerWheel to power of two and initialize the wheel.
            _wheel = CreateWheel(ticksPerWheel);
            _worker = new Worker(this);
            _mask = _wheel.Length - 1;

            _tickDuration = tickInterval.Ticks;

            // Prevent overflow
            if (_tickDuration >= long.MaxValue / _wheel.Length)
            {
                throw new ArgumentException(
                    string.Format(
                        "tickInterval: {0} (expected: 0 < tickInterval in nanos < {1}",
                        tickInterval,
                        long.MaxValue / _wheel.Length));
            }
            _workerThread = new XThread(st => _worker.Run());

            _maxPendingTimeouts = maxPendingTimeouts;

            if (Interlocked.Increment(ref v_instanceCounter) > InstanceCountLimit &&
                0u >= (uint)Interlocked.CompareExchange(ref v_warnedTooManyInstances, 1, 0))
            {
                ReportTooManyInstances();
            }
        }

        ~HashedWheelTimer()
        {
            // This object is going to be GCed and it is assumed the ship has sailed to do a proper shutdown. If
            // we have not yet shutdown then we want to make sure we decrement the active instance count.
            if (Interlocked.Exchange(ref v_workerState, WorkerStateShutdown) != WorkerStateShutdown)
            {
                _ = Interlocked.Decrement(ref v_instanceCounter);
            }
        }

        internal long PendingTimeouts => Volatile.Read(ref v_pendingTimeouts);

        internal CancellationToken CancellationToken => _cancellationTokenSource.Token;

        PreciseTimeSpan StartTime
        {
            get => PreciseTimeSpan.FromTicks(Volatile.Read(ref v_startTime));
            set => Interlocked.Exchange(ref v_startTime, value.Ticks);
        }

        int WorkerState => Volatile.Read(ref v_workerState);

        static HashedWheelBucket[] CreateWheel(int ticksPerWheel)
        {
            ticksPerWheel = NormalizeTicksPerWheel(ticksPerWheel);
            var wheel = new HashedWheelBucket[ticksPerWheel];
            for (int i = 0; i < wheel.Length; i++)
            {
                wheel[i] = new HashedWheelBucket();
            }
            return wheel;
        }

        static int NormalizeTicksPerWheel(int ticksPerWheel)
        {
            int normalizedTicksPerWheel = 1;
            while (normalizedTicksPerWheel < ticksPerWheel)
            {
                normalizedTicksPerWheel <<= 1;
            }
            return normalizedTicksPerWheel;
        }

        /// <summary>
        /// Starts the background thread explicitly. The background thread will
        /// start automatically on demand even if you did not call this method.
        /// </summary>
        /// <exception cref="InvalidOperationException">if this timer has been
        /// stopped already.</exception>
        public void Start()
        {
            switch (WorkerState)
            {
                case WorkerStateInit:
                    if (Interlocked.CompareExchange(ref v_workerState, WorkerStateStarted, WorkerStateInit) == WorkerStateInit)
                    {
                        _workerThread.Start();
                    }
                    break;
                case WorkerStateStarted:
                    break;
                case WorkerStateShutdown:
                    ThrowHelper.ThrowInvalidOperationException_CannotBeStartedOnceStopped(); break;
                default:
                    ThrowHelper.ThrowInvalidOperationException_InvalidWorkerState(); break;
            }

            // Wait until the startTime is initialized by the worker.
            if (StartTime == PreciseTimeSpan.Zero)
            {
                _startTimeInitialized.Wait(CancellationToken);
            }
        }

        public async Task<ISet<ITimeout>> StopAsync()
        {
            GC.SuppressFinalize(this);

            if (XThread.CurrentThread == _workerThread)
            {
                ThrowHelper.ThrowInvalidOperationException_CannotBeCalledFromTimerTask();
            }

            if (Interlocked.CompareExchange(ref v_workerState, WorkerStateShutdown, WorkerStateStarted) != WorkerStateStarted)
            {
                // workerState can be 0 or 2 at this moment - let it always be 2.
                if (Interlocked.Exchange(ref v_workerState, WorkerStateShutdown) != WorkerStateShutdown)
                {
                    _cancellationTokenSource.Cancel();
                    _ = Interlocked.Decrement(ref v_instanceCounter);
                }

                return new HashSet<ITimeout>();
            }

            try
            {
                _cancellationTokenSource.Cancel();
            }
            finally
            {
                _ = Interlocked.Decrement(ref v_instanceCounter);
            }
            await _worker.ClosedFuture;
            return _worker.UnprocessedTimeouts;
        }

        public ITimeout NewTimeout(ITimerTask task, TimeSpan delay)
        {
            if (task is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.task);
            }
            if (WorkerState == WorkerStateShutdown)
            {
                _ = ThrowHelper.ThrowRejectedExecutionException_TimerStopped();
            }
            long pendingTimeoutsCount = Interlocked.Increment(ref v_pendingTimeouts);
            long maxPendingTimeouts = _maxPendingTimeouts;
            if (maxPendingTimeouts > 0L && pendingTimeoutsCount > maxPendingTimeouts)
            {
                _ = Interlocked.Decrement(ref v_pendingTimeouts);
                _ = ThrowHelper.ThrowRejectedExecutionException_NumOfPendingTimeouts(pendingTimeoutsCount, maxPendingTimeouts);
            }

            Start();

            // Add the timeout to the timeout queue which will be processed on the next tick.
            // During processing all the queued HashedWheelTimeouts will be added to the correct HashedWheelBucket.
            TimeSpan deadline = CeilTimeSpanToMilliseconds((PreciseTimeSpan.Deadline(delay) - StartTime).ToTimeSpan());
            var timeout = new HashedWheelTimeout(this, task, deadline);
            _ = _timeouts.TryEnqueue(timeout);
            return timeout;
        }

        void ScheduleCancellation(HashedWheelTimeout timeout)
        {
            if (WorkerState != WorkerStateShutdown)
            {
                _ = _cancelledTimeouts.TryEnqueue(timeout);
            }
        }

        static void ReportTooManyInstances() =>
            Logger.Error($"You are creating too many {nameof(HashedWheelTimer)} instances. {nameof(HashedWheelTimer)} is a shared resource that must be reused across the process,so that only a few instances are created.");

        static TimeSpan CeilTimeSpanToMilliseconds(TimeSpan time)
        {
            long remainder = time.Ticks % TimeSpan.TicksPerMillisecond;
            return 0ul >= (ulong)remainder ? time : new TimeSpan(time.Ticks - remainder + TimeSpan.TicksPerMillisecond);
        }

        sealed class Worker : IRunnable
        {
            readonly HashedWheelTimer _owner;

            long _tick;
            readonly DefaultPromise _closedPromise;

            public Worker(HashedWheelTimer owner)
            {
                _owner = owner;
                _closedPromise = new DefaultPromise();
            }

            public Task ClosedFuture => _closedPromise.Task;

            public void Run()
            {
                try
                {
                    // Initialize the startTime.
                    _owner.StartTime = PreciseTimeSpan.FromStart;
                    if (_owner.StartTime == PreciseTimeSpan.Zero)
                    {
                        // We use 0 as an indicator for the uninitialized value here, so make sure it's not 0 when initialized.
                        _owner.StartTime = PreciseTimeSpan.FromTicks(1);
                    }

                    // Notify the other threads waiting for the initialization at start().
                    _ = _owner._startTimeInitialized.Signal();

                    while (true)
                    {
                        TimeSpan deadline = WaitForNextTick();
                        if (Volatile.Read(ref _owner.v_workerState) != WorkerStateStarted)
                        {
                            break;
                        }
                        if (deadline > TimeSpan.Zero)
                        {
                            int idx = (int)(_tick & _owner._mask);
                            ProcessCancelledTasks();
                            HashedWheelBucket bucket = _owner._wheel[idx];
                            TransferTimeoutsToBuckets();
                            bucket.ExpireTimeouts(deadline);
                            _tick++;
                        }
                    }

                    // Fill the unprocessedTimeouts so we can return them from stop() method.
                    foreach (HashedWheelBucket bucket in _owner._wheel)
                    {
                        bucket.ClearTimeouts(UnprocessedTimeouts);
                    }
                    while (_owner._timeouts.TryDequeue(out var timeout))
                    {
                        if (!timeout.Canceled)
                        {
                            _ = UnprocessedTimeouts.Add(timeout);
                        }
                    }
                    ProcessCancelledTasks();
                }
                catch (Exception ex)
                {
                    Logger.TimeoutProcessingFailed(ex);
                }
                finally
                {
                    _ = _closedPromise.TryComplete();
                }
            }

            void TransferTimeoutsToBuckets()
            {
                // transfer only max. 100000 timeouts per tick to prevent a thread to stall the workerThread when it just
                // adds new timeouts in a loop.
                for (int i = 0; i < 100000; i++)
                {
                    if (!_owner._timeouts.TryDequeue(out HashedWheelTimeout timeout))
                    {
                        // all processed
                        break;
                    }
                    if (timeout.State == HashedWheelTimeout.StCanceled)
                    {
                        // Was cancelled in the meantime.
                        continue;
                    }

                    long calculated = timeout._Deadline.Ticks / _owner._tickDuration;
                    timeout.RemainingRounds = (calculated - _tick) / _owner._wheel.Length;

                    long ticks = Math.Max(calculated, _tick); // Ensure we don't schedule for past.
                    int stopIndex = (int)(ticks & _owner._mask);

                    HashedWheelBucket bucket = _owner._wheel[stopIndex];
                    bucket.AddTimeout(timeout);
                }
            }

            void ProcessCancelledTasks()
            {
                while (true)
                {
                    if (!_owner._cancelledTimeouts.TryDequeue(out HashedWheelTimeout timeout))
                    {
                        // all processed
                        break;
                    }
                    try
                    {
                        timeout.Remove();
                    }
                    catch (Exception ex)
                    {
                        if (Logger.WarnEnabled)
                        {
                            Logger.AnExcWasThrownWhileProcessingACancellationTask(ex);
                        }
                    }
                }
            }

            /// <summary>
            /// calculate timer firing time from startTime and current tick number,
            /// then wait until that goal has been reached.
            /// </summary>
            /// <returns>long.MinValue if received a shutdown request,
            /// current time otherwise (with long.MinValue changed by +1)
            /// </returns>
            TimeSpan WaitForNextTick()
            {
                long deadline = _owner._tickDuration * (_tick + 1);

                while (true)
                {
                    TimeSpan currentTime = (PreciseTimeSpan.FromStart - _owner.StartTime).ToTimeSpan();
                    TimeSpan sleepTime = CeilTimeSpanToMilliseconds(TimeSpan.FromTicks(deadline - currentTime.Ticks));

                    if (sleepTime <= TimeSpan.Zero)
                    {
                        return currentTime.Ticks == long.MinValue
                            ? TimeSpan.FromTicks(-long.MaxValue)
                            : currentTime;
                    }

                    try
                    {
                        long sleepTimeMs = sleepTime.Ticks / TimeSpan.TicksPerMillisecond; // we've already rounded so no worries about the remainder > 0 here
                        Debug.Assert(sleepTimeMs <= int.MaxValue);
                        XThread.Sleep((int)sleepTimeMs, _owner.CancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        if (Volatile.Read(ref _owner.v_workerState) == WorkerStateShutdown)
                        {
                            return TimeSpan.FromTicks(long.MinValue);
                        }
                    }
                }
            }

            internal ISet<ITimeout> UnprocessedTimeouts { get; } = new HashSet<ITimeout>();
        }

        sealed class HashedWheelTimeout : ITimeout
        {
            const int StInit = 0;
            internal const int StCanceled = 1;
            const int StExpired = 2;

            internal readonly HashedWheelTimer _timer;
            internal readonly TimeSpan _Deadline;

            int v_state = StInit;

            // remainingRounds will be calculated and set by Worker.transferTimeoutsToBuckets() before the
            // HashedWheelTimeout will be added to the correct HashedWheelBucket.
            internal long RemainingRounds;

            // This will be used to chain timeouts in HashedWheelTimerBucket via a double-linked-list.
            // As only the workerThread will act on it there is no need for synchronization / volatile.
            internal HashedWheelTimeout Next;

            internal HashedWheelTimeout Prev;

            // The bucket to which the timeout was added
            internal HashedWheelBucket Bucket;

            internal HashedWheelTimeout(HashedWheelTimer timer, ITimerTask task, TimeSpan deadline)
            {
                _timer = timer;
                Task = task;
                _Deadline = deadline;
            }

            public ITimer Timer => _timer;

            public ITimerTask Task { get; }

            public bool Cancel()
            {
                // only update the state it will be removed from HashedWheelBucket on next tick.
                if (!CompareAndSetState(StInit, StCanceled))
                {
                    return false;
                }
                // If a task should be canceled we put this to another queue which will be processed on each tick.
                // So this means that we will have a GC latency of max. 1 tick duration which is good enough. This way
                // we can make again use of our MpscLinkedQueue and so minimize the locking / overhead as much as possible.
                _timer.ScheduleCancellation(this);
                return true;
            }

            internal void Remove()
            {
                HashedWheelBucket bucket = Bucket;
                if (bucket is object)
                {
                    // timeout got canceled before it was added to the bucket
                    _ = bucket.Remove(this);
                }
                else
                {
                    _ = Interlocked.Decrement(ref _timer.v_pendingTimeouts);
                }
            }

            bool CompareAndSetState(int expected, int state)
            {
                return Interlocked.CompareExchange(ref v_state, state, expected) == expected;
            }

            internal int State => Volatile.Read(ref v_state);

            public bool Canceled => State == StCanceled;

            public bool Expired => State == StExpired;

            internal void Expire()
            {
                if (!CompareAndSetState(StInit, StExpired))
                {
                    return;
                }

                try
                {
                    Task.Run(this);
                }
                catch (Exception t)
                {
                    if (Logger.WarnEnabled)
                    {
                        Logger.AnExceptionWasThrownBy(Task, t);
                    }
                }
            }

            public override string ToString()
            {
                PreciseTimeSpan currentTime = PreciseTimeSpan.FromStart - _timer.StartTime;
                TimeSpan remaining = _Deadline - currentTime.ToTimeSpan();

                var buf = StringBuilderCache.Acquire() // new StringBuilder(192)
                    .Append(GetType().Name)
                    .Append('(')
                    .Append("deadline: ");
                if (remaining > TimeSpan.Zero)
                {
                    _ = buf.Append(remaining)
                        .Append(" later");
                }
                else if (remaining < TimeSpan.Zero)
                {
                    _ = buf.Append(-remaining)
                        .Append(" ago");
                }
                else
                {
                    _ = buf.Append("now");
                }

                if (Canceled)
                {
                    _ = buf.Append(", cancelled");
                }

                var result = buf.Append(", task: ")
                    .Append(Task)
                    .Append(')')
                    .ToString();
                StringBuilderCache.Release(buf);
                return result;
            }
        }

        /// <summary>
        /// Bucket that stores HashedWheelTimeouts. These are stored in a linked-list like datastructure to allow easy
        /// removal of HashedWheelTimeouts in the middle. Also the HashedWheelTimeout act as nodes themself and so no
        /// extra object creation is needed.
        /// </summary>
        sealed class HashedWheelBucket
        {
            // Used for the linked-list datastructure
            HashedWheelTimeout _head;
            HashedWheelTimeout _tail;

            /// <summary>
            /// Add a <see cref="HashedWheelTimeout"/> to this bucket.
            /// </summary>
            public void AddTimeout(HashedWheelTimeout timeout)
            {
                Debug.Assert(timeout.Bucket is null);
                timeout.Bucket = this;
                if (_head is null)
                {
                    _head = _tail = timeout;
                }
                else
                {
                    _tail.Next = timeout;
                    timeout.Prev = _tail;
                    _tail = timeout;
                }
            }

            /// <summary>
            /// Expire all <see cref="HashedWheelTimeout"/>s for the given <c>deadline</c>.
            /// </summary>
            public void ExpireTimeouts(TimeSpan deadline)
            {
                HashedWheelTimeout timeout = _head;

                // process all timeouts
                while (timeout is object)
                {
                    HashedWheelTimeout next = timeout.Next;
                    if (timeout.RemainingRounds <= 0)
                    {
                        next = Remove(timeout);
                        if (timeout._Deadline <= deadline)
                        {
                            timeout.Expire();
                        }
                        else
                        {
                            // The timeout was placed into a wrong slot. This should never happen.
                            ThrowHelper.ThrowInvalidOperationException_Deadline(timeout._Deadline, deadline);
                        }
                    }
                    else
                    {
                        timeout.RemainingRounds--;
                    }
                    timeout = next;
                }
            }

            public HashedWheelTimeout Remove(HashedWheelTimeout timeout)
            {
                HashedWheelTimeout next = timeout.Next;
                // remove timeout that was either processed or cancelled by updating the linked-list
                if (timeout.Prev is object)
                {
                    timeout.Prev.Next = next;
                }
                if (timeout.Next is object)
                {
                    timeout.Next.Prev = timeout.Prev;
                }

                if (timeout == _head)
                {
                    // if timeout is also the tail we need to adjust the entry too
                    if (timeout == _tail)
                    {
                        _tail = null;
                        _head = null;
                    }
                    else
                    {
                        _head = next;
                    }
                }
                else if (timeout == _tail)
                {
                    // if the timeout is the tail modify the tail to be the prev node.
                    _tail = timeout.Prev;
                }
                // null out prev, next and bucket to allow for GC.
                timeout.Prev = null;
                timeout.Next = null;
                timeout.Bucket = null;
                _ = Interlocked.Decrement(ref timeout._timer.v_pendingTimeouts);
                return next;
            }

            /// <summary>
            /// Clear this bucket and return all not expired / cancelled <see cref="ITimeout"/>s.
            /// </summary>
            public void ClearTimeouts(ISet<ITimeout> set)
            {
                while (true)
                {
                    HashedWheelTimeout timeout = PollTimeout();
                    if (timeout is null)
                    {
                        return;
                    }
                    if (timeout.Expired || timeout.Canceled)
                    {
                        continue;
                    }
                    _ = set.Add(timeout);
                }
            }

            HashedWheelTimeout PollTimeout()
            {
                HashedWheelTimeout head = _head;
                if (head is null)
                {
                    return null;
                }
                HashedWheelTimeout next = head.Next;
                if (next is null)
                {
                    _tail = _head = null;
                }
                else
                {
                    _head = next;
                    next.Prev = null;
                }

                // null out prev and next to allow for GC.
                head.Next = null;
                head.Prev = null;
                head.Bucket = null;
                return head;
            }
        }
    }
}