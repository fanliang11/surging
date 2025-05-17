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
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// Abstract base class for <see cref="IEventExecutor" />s that need to support scheduling.
    /// </summary>
    public abstract class AbstractScheduledEventExecutor : AbstractEventExecutor
    {
        protected static readonly IRunnable WakeupTask;

        static AbstractScheduledEventExecutor()
        {
            WakeupTask = new NoOpRunnable();
        }

        internal readonly IPriorityQueue<IScheduledRunnable> _scheduledTaskQueue;
        private long _nextTaskId;

        protected AbstractScheduledEventExecutor()
            : this(null)
        {
        }

        protected AbstractScheduledEventExecutor(IEventExecutorGroup parent)
            : base(parent)
        {
            _scheduledTaskQueue = new DefaultPriorityQueue<IScheduledRunnable>();
        }

        /// <summary>TBD</summary>
        protected abstract bool HasTasks { get; }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected static PreciseTimeSpan GetNanos() => PreciseTimeSpan.FromStart;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected static long NanoTime() => PreciseTime.NanoTime();

        /// <summary>
        /// Given an arbitrary deadline <paramref name="deadlineNanos"/>, calculate the number of nano seconds from now
        /// <paramref name="deadlineNanos"/> would expire.
        /// </summary>
        /// <param name="deadlineNanos">An arbitrary deadline in nano seconds.</param>
        /// <returns>the number of nano seconds from now <paramref name="deadlineNanos"/> would expire.</returns>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected static long DeadlineToDelayNanos(long deadlineNanos) => PreciseTime.DeadlineToDelayNanos(deadlineNanos);

        /// <summary>The initial value used for delay and computations based upon a monatomic time source.</summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected static long InitialNanoTime() => PreciseTime.StartTime;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected static bool IsNullOrEmpty<T>(IPriorityQueue<T> taskQueue)
            where T : class, IPriorityQueueNode<T>
        {
            return taskQueue is null || 0u >= (uint)taskQueue.Count;
        }

        [Obsolete("Please use IPriorityQueue{T}.IsEmpty instead.")]
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected static bool IsEmpty(IPriorityQueue<IScheduledRunnable> taskQueue)
        {
            return 0u >= (uint)taskQueue.Count;
        }

        /// <summary>
        /// Cancel all scheduled tasks
        /// This method MUST be called only when <see cref="IEventExecutor.InEventLoop" /> is <c>true</c>.
        /// </summary>
        protected virtual void CancelScheduledTasks()
        {
            Debug.Assert(InEventLoop);

            IPriorityQueue<IScheduledRunnable> scheduledTaskQueue = _scheduledTaskQueue;
            if (scheduledTaskQueue.IsEmpty) { return; }

            IScheduledRunnable[] tasks = scheduledTaskQueue.ToArray();
            for (int i = 0; i < tasks.Length; i++)
            {
                _ = tasks[i].CancelWithoutRemove();
            }

            _scheduledTaskQueue.ClearIgnoringIndexes();
        }

        internal protected IScheduledRunnable PollScheduledTask() => PollScheduledTask(NanoTime());

        /// <summary>
        /// Return the <see cref="IScheduledRunnable"/> which is ready to be executed with the given <paramref name="nanoTime"/>.
        /// You should use <see cref="NanoTime()"/> to retrieve the correct <paramref name="nanoTime"/>.
        /// </summary>
        /// <param name="nanoTime"></param>
        protected IScheduledRunnable PollScheduledTask(long nanoTime)
        {
            Debug.Assert(InEventLoop);

            if (_scheduledTaskQueue.TryPeek(out IScheduledRunnable scheduledTask) &&
                scheduledTask.DeadlineNanos <= nanoTime)
            {
                _ = _scheduledTaskQueue.TryDequeue(out _);
                scheduledTask.SetConsumed();
                return scheduledTask;
            }

            return null;
        }

        /// <summary>
        /// Return the nanoseconds until the next scheduled task is ready to be run or <c>-1</c> if no task is scheduled.
        /// </summary>
        protected long NextScheduledTaskNanos()
        {
            if (_scheduledTaskQueue.TryPeek(out IScheduledRunnable nextScheduledRunnable))
            {
                return nextScheduledRunnable.DelayNanos;
            }
            return PreciseTime.MinusOne;
        }

        /// <summary>
        /// Return the deadline (in nanoseconds) when the next scheduled task is ready to be run or <c>-1</c>
        /// if no task is scheduled.
        /// </summary>
        protected long NextScheduledTaskDeadlineNanos()
        {
            if (_scheduledTaskQueue.TryPeek(out IScheduledRunnable nextScheduledRunnable))
            {
                return nextScheduledRunnable.DeadlineNanos;
            }
            return PreciseTime.MinusOne;
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected IScheduledRunnable PeekScheduledTask()
        {
            return _scheduledTaskQueue.TryPeek(out IScheduledRunnable task) ? task : null;
        }

        protected bool TryPeekScheduledTask(out IScheduledRunnable task)
        {
            return _scheduledTaskQueue.TryPeek(out task);
        }

        /// <summary>
        /// Returns <c>true</c> if a scheduled task is ready for processing.
        /// </summary>
        protected bool HasScheduledTasks()
        {
            return _scheduledTaskQueue.TryPeek(out IScheduledRunnable scheduledTask) && scheduledTask.DeadlineNanos <= PreciseTime.NanoTime();
        }

        public override IScheduledTask Schedule(IRunnable action, TimeSpan delay)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }

            return Schedule(new RunnableScheduledTask(this, action, PreciseTime.DeadlineNanos(delay)));
        }

        public override IScheduledTask Schedule(Action action, TimeSpan delay)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }

            return Schedule(new ActionScheduledTask(this, action, PreciseTime.DeadlineNanos(delay)));
        }

        public override IScheduledTask Schedule(Action<object> action, object state, TimeSpan delay)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }

            return Schedule(new StateActionScheduledTask(this, action, state, PreciseTime.DeadlineNanos(delay)));
        }

        public override IScheduledTask Schedule(Action<object, object> action, object context, object state, TimeSpan delay)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }

            return Schedule(new StateActionWithContextScheduledTask(this, action, context, state, PreciseTime.DeadlineNanos(delay)));
        }

        public override IScheduledTask ScheduleAtFixedRate(IRunnable action, TimeSpan initialDelay, TimeSpan period)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }
            if (period <= TimeSpan.Zero) { ThrowHelper.ThrowArgumentException_PeriodMustBeGreaterThanZero(); }

            return Schedule(new RunnableScheduledTask(this, action, PreciseTime.DeadlineNanos(initialDelay), PreciseTime.ToDelayNanos(period)));
        }

        public override IScheduledTask ScheduleAtFixedRate(Action action, TimeSpan initialDelay, TimeSpan period)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }
            if (period <= TimeSpan.Zero) { ThrowHelper.ThrowArgumentException_PeriodMustBeGreaterThanZero(); }

            return Schedule(new ActionScheduledTask(this, action, PreciseTime.DeadlineNanos(initialDelay), PreciseTime.ToDelayNanos(period)));
        }

        public override IScheduledTask ScheduleAtFixedRate(Action<object> action, object state, TimeSpan initialDelay, TimeSpan period)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }
            if (period <= TimeSpan.Zero) { ThrowHelper.ThrowArgumentException_PeriodMustBeGreaterThanZero(); }

            return Schedule(new StateActionScheduledTask(this, action, state, PreciseTime.DeadlineNanos(initialDelay), PreciseTime.ToDelayNanos(period)));
        }

        public override IScheduledTask ScheduleAtFixedRate(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan period)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }
            if (period <= TimeSpan.Zero) { ThrowHelper.ThrowArgumentException_PeriodMustBeGreaterThanZero(); }

            return Schedule(new StateActionWithContextScheduledTask(this, action, context, state, PreciseTime.DeadlineNanos(initialDelay), PreciseTime.ToDelayNanos(period)));
        }

        public override IScheduledTask ScheduleWithFixedDelay(IRunnable action, TimeSpan initialDelay, TimeSpan delay)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }
            if (delay <= TimeSpan.Zero) { ThrowHelper.ThrowArgumentException_DelayMustBeGreaterThanZero(); }

            return Schedule(new RunnableScheduledTask(this, action, PreciseTime.DeadlineNanos(initialDelay), -PreciseTime.ToDelayNanos(delay)));
        }

        public override IScheduledTask ScheduleWithFixedDelay(Action action, TimeSpan initialDelay, TimeSpan delay)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }
            if (delay <= TimeSpan.Zero) { ThrowHelper.ThrowArgumentException_DelayMustBeGreaterThanZero(); }

            return Schedule(new ActionScheduledTask(this, action, PreciseTime.DeadlineNanos(initialDelay), -PreciseTime.ToDelayNanos(delay)));
        }

        public override IScheduledTask ScheduleWithFixedDelay(Action<object> action, object state, TimeSpan initialDelay, TimeSpan delay)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }
            if (delay <= TimeSpan.Zero) { ThrowHelper.ThrowArgumentException_DelayMustBeGreaterThanZero(); }

            return Schedule(new StateActionScheduledTask(this, action, state, PreciseTime.DeadlineNanos(initialDelay), -PreciseTime.ToDelayNanos(delay)));
        }

        public override IScheduledTask ScheduleWithFixedDelay(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan delay)
        {
            if (action is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action); }
            if (delay <= TimeSpan.Zero) { ThrowHelper.ThrowArgumentException_DelayMustBeGreaterThanZero(); }

            return Schedule(new StateActionWithContextScheduledTask(this, action, context, state, PreciseTime.DeadlineNanos(initialDelay), -PreciseTime.ToDelayNanos(delay)));
        }

        public override Task ScheduleAsync(IRunnable action, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return Schedule(action, delay).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }

            return Schedule(new RunnableScheduledAsyncTask(this, action, PreciseTime.DeadlineNanos(delay), cancellationToken)).Completion;
        }

        public override Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return Schedule(action, delay).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }

            return Schedule(new ActionScheduledAsyncTask(this, action, PreciseTime.DeadlineNanos(delay), cancellationToken)).Completion;
        }

        public override Task ScheduleAsync(Action<object> action, object state, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return Schedule(action, state, delay).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }

            return Schedule(new StateActionScheduledAsyncTask(this, action, state, PreciseTime.DeadlineNanos(delay), cancellationToken)).Completion;
        }

        public override Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return Schedule(action, context, state, delay).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }

            return Schedule(new StateActionWithContextScheduledAsyncTask(this, action, context, state, PreciseTime.DeadlineNanos(delay), cancellationToken)).Completion;
        }

        public override Task ScheduleAtFixedRateAsync(IRunnable action, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return ScheduleAtFixedRate(action, initialDelay, period).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }
            if (period <= TimeSpan.Zero) { return ThrowHelper.FromArgumentException_PeriodMustBeGreaterThanZero(); }

            return Schedule(new RunnableScheduledAsyncTask(this, action, PreciseTime.DeadlineNanos(initialDelay), PreciseTime.ToDelayNanos(period), cancellationToken)).Completion;
        }

        public override Task ScheduleAtFixedRateAsync(Action action, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return ScheduleAtFixedRate(action, initialDelay, period).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }
            if (period <= TimeSpan.Zero) { return ThrowHelper.FromArgumentException_PeriodMustBeGreaterThanZero(); }

            return Schedule(new ActionScheduledAsyncTask(this, action, PreciseTime.DeadlineNanos(initialDelay), PreciseTime.ToDelayNanos(period), cancellationToken)).Completion;
        }

        public override Task ScheduleAtFixedRateAsync(Action<object> action, object state, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return ScheduleAtFixedRate(action, state, initialDelay, period).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }
            if (period <= TimeSpan.Zero) { return ThrowHelper.FromArgumentException_PeriodMustBeGreaterThanZero(); }

            return Schedule(new StateActionScheduledAsyncTask(this, action, state, PreciseTime.DeadlineNanos(initialDelay), PreciseTime.ToDelayNanos(period), cancellationToken)).Completion;
        }

        public override Task ScheduleAtFixedRateAsync(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return ScheduleAtFixedRate(action, context, state, initialDelay, period).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }
            if (period <= TimeSpan.Zero) { return ThrowHelper.FromArgumentException_PeriodMustBeGreaterThanZero(); }

            return Schedule(new StateActionWithContextScheduledAsyncTask(this, action, context, state, PreciseTime.DeadlineNanos(initialDelay), PreciseTime.ToDelayNanos(period), cancellationToken)).Completion;
        }

        public override Task ScheduleWithFixedDelayAsync(IRunnable action, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return ScheduleWithFixedDelay(action, initialDelay, delay).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }
            if (delay <= TimeSpan.Zero) { return ThrowHelper.FromArgumentException_DelayMustBeGreaterThanZero(); }

            return Schedule(new RunnableScheduledAsyncTask(this, action, PreciseTime.DeadlineNanos(initialDelay), -PreciseTime.ToDelayNanos(delay), cancellationToken)).Completion;
        }

        public override Task ScheduleWithFixedDelayAsync(Action action, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return ScheduleWithFixedDelay(action, initialDelay, delay).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }
            if (delay <= TimeSpan.Zero) { return ThrowHelper.FromArgumentException_DelayMustBeGreaterThanZero(); }

            return Schedule(new ActionScheduledAsyncTask(this, action, PreciseTime.DeadlineNanos(initialDelay), -PreciseTime.ToDelayNanos(delay), cancellationToken)).Completion;
        }

        public override Task ScheduleWithFixedDelayAsync(Action<object> action, object state, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return ScheduleWithFixedDelay(action, state, initialDelay, delay).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }
            if (delay <= TimeSpan.Zero) { return ThrowHelper.FromArgumentException_DelayMustBeGreaterThanZero(); }

            return Schedule(new StateActionScheduledAsyncTask(this, action, state, PreciseTime.DeadlineNanos(initialDelay), -PreciseTime.ToDelayNanos(delay), cancellationToken)).Completion;
        }

        public override Task ScheduleWithFixedDelayAsync(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskUtil.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return ScheduleWithFixedDelay(action, context, state, initialDelay, delay).Completion;
            }

            if (action is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.action); }
            if (delay <= TimeSpan.Zero) { return ThrowHelper.FromArgumentException_DelayMustBeGreaterThanZero(); }

            return Schedule(new StateActionWithContextScheduledAsyncTask(this, action, context, state, PreciseTime.DeadlineNanos(initialDelay), -PreciseTime.ToDelayNanos(delay), cancellationToken)).Completion;
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        internal void ScheduleFromEventLoop(IScheduledRunnable task)
        {
            // nextTaskId a long and so there is no chance it will overflow back to 0
            var nextTaskId = ++_nextTaskId;
            if (nextTaskId == long.MaxValue) { _nextTaskId = 0; }

            var isBacklogEmpty = !HasTasks && _scheduledTaskQueue.IsEmpty;

            _ = _scheduledTaskQueue.TryEnqueue(task.SetId(nextTaskId));

            if (isBacklogEmpty)
            {
                // 在 Libuv.LoopExecutor 中，当任务执行完毕，清空任务队列后，后续如果只有 ScheduledTask 入队的情况下，
                // 并不会激发线程进行任务处理，需唤醒
                EnusreWakingUp(true);
            }
        }

        private IScheduledRunnable Schedule(IScheduledRunnable task)
        {
            if (InEventLoop)
            {
                ScheduleFromEventLoop(task);
            }
            else
            {
                var deadlineNanos = task.DeadlineNanos;
                // task will add itself to scheduled task queue when run if not expired
                if (BeforeScheduledTaskSubmitted(deadlineNanos))
                {
                    Execute(task);
                }
                else
                {
                    LazyExecute(task);
                    // Second hook after scheduling to facilitate race-avoidance
                    if (AfterScheduledTaskSubmitted(deadlineNanos))
                    {
                        Execute(WakeupTask);
                    }
                }
            }
            return task;
        }

        internal void RemoveScheduled(IScheduledRunnable task)
        {
            if (InEventLoop)
            {
                _ = _scheduledTaskQueue.TryRemove(task);
            }
            else
            {
                // task will remove itself from scheduled task queue when it runs
                LazyExecute(task);
            }
        }

        /// <summary>
        /// Called from arbitrary non-<see cref="IEventExecutor"/> threads prior to scheduled task submission.
        /// Returns <c>true</c> if the <see cref="IEventExecutor"/> thread should be woken immediately to
        /// process the scheduled task (if not already awake).
        /// 
        /// <para>If <c>false</c> is returned, <see cref="AfterScheduledTaskSubmitted(long)"/> will be called with
        /// the same value <i>after</i> the scheduled task is enqueued, providing another opportunity
        /// to wake the <see cref="IEventExecutor"/> thread if required.</para>
        /// </summary>
        /// <param name="deadlineNanos">deadline of the to-be-scheduled task
        /// relative to <see cref="NanoTime()"/></param>
        /// <returns><c>true</c> if the <see cref="IEventExecutor"/> thread should be woken, <c>false</c> otherwise</returns>
        protected virtual bool BeforeScheduledTaskSubmitted(long deadlineNanos)
        {
            return true;
        }

        /// <summary>
        /// See <see cref="BeforeScheduledTaskSubmitted(long)"/>. Called only after that method returns false.
        /// </summary>
        /// <param name="deadlineNanos">relative to <see cref="NanoTime()"/></param>
        /// <returns><c>true</c> if the <see cref="IEventExecutor"/> thread should be woken, <c>false</c> otherwise</returns>
        protected virtual bool AfterScheduledTaskSubmitted(long deadlineNanos)
        {
            return true;
        }

        /// <summary>TBD</summary>
        /// <param name="inEventLoop"></param>
        protected virtual void EnusreWakingUp(bool inEventLoop) { }

        sealed class NoOpRunnable : IRunnable
        {
            public void Run()
            {
                // NOOP
            }
        }
    }
}