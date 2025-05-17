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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Common.Concurrency
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using Thread = XThread;

    /// <summary>
    /// <see cref="IOrderedEventExecutor"/> backed by a single thread.
    /// </summary>
    public abstract class SingleThreadEventExecutor : AbstractScheduledEventExecutor, IOrderedEventExecutor
    {
        #region @@ Fields @@

        protected const int NotStartedState = 1;
        protected const int StartedState = 2;
        protected const int ShuttingDownState = 3;
        protected const int ShutdownState = 4;
        protected const int TerminatedState = 5;

        internal static readonly int DefaultMaxPendingExecutorTasks = Math.Max(16,
                SystemPropertyUtil.GetInt("io.netty.eventexecutor.maxPendingTasks", int.MaxValue));
        protected static readonly IInternalLogger Logger =
            InternalLoggerFactory.GetInstance<SingleThreadEventExecutor>();

        private readonly Thread _thread;
        private readonly TaskScheduler _taskScheduler;
        private readonly CountdownEvent _threadLock;
        private readonly IPromise _terminationCompletionSource;
        private volatile int v_executionState = NotStartedState;

        protected readonly IQueue<IRunnable> _taskQueue;
        private readonly IBlockingQueue<IRunnable> _blockingTaskQueue;
        private readonly IRejectedExecutionHandler _rejectedExecutionHandler;
        private readonly ISet<Action> _shutdownHooks;
        private readonly bool _addTaskWakesUp;
        private readonly int _maxPendingTasks;

        private long _lastExecutionTime;
        private long _gracefulShutdownStartTime;
        private long v_gracefulShutdownQuietPeriod;
        private long v_gracefulShutdownTimeout;
        //private long v_progress;
        private bool _firstTask; // 不需要设置 volatile

        #endregion

        #region @@ Constructors @@

        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutor"/>.</summary>
        /// <param name="threadFactory">the <see cref="IThreadFactory"/> which will be used for the used <see cref="Thread"/>.</param>
        /// <param name="addTaskWakesUp"><c>true</c> if and only if invocation of <see cref="AddTask(IRunnable)"/> will wake up the executor thread.</param>
        public SingleThreadEventExecutor(IThreadFactory threadFactory, bool addTaskWakesUp)
            : this(null, threadFactory, addTaskWakesUp, DefaultMaxPendingExecutorTasks)
        {
        }

        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutor"/>.</summary>
        /// <param name="threadFactory">the <see cref="IThreadFactory"/> which will be used for the used <see cref="Thread"/>.</param>
        /// <param name="addTaskWakesUp"><c>true</c> if and only if invocation of <see cref="AddTask(IRunnable)"/> will wake up the executor thread.</param>
        /// <param name="maxPendingTasks">the maximum number of pending tasks before new tasks will be rejected.</param>
        public SingleThreadEventExecutor(IThreadFactory threadFactory, bool addTaskWakesUp, int maxPendingTasks)
            : this(null, threadFactory, addTaskWakesUp, maxPendingTasks)
        {
        }

        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutor"/>.</summary>
        /// <param name="threadFactory">the <see cref="IThreadFactory"/> which will be used for the used <see cref="Thread"/>.</param>
        /// <param name="addTaskWakesUp"><c>true</c> if and only if invocation of <see cref="AddTask(IRunnable)"/> will wake up the executor thread.</param>
        /// <param name="rejectedHandler">the <see cref="IRejectedExecutionHandler"/> to use.</param>
        protected SingleThreadEventExecutor(IThreadFactory threadFactory, bool addTaskWakesUp, IRejectedExecutionHandler rejectedHandler)
            : this(null, threadFactory, addTaskWakesUp, rejectedHandler)
        {
        }

        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutor"/>.</summary>
        /// <param name="parent">the <see cref="IEventExecutorGroup"/> which is the parent of this instance and belongs to it.</param>
        /// <param name="threadFactory">the <see cref="IThreadFactory"/> which will be used for the used <see cref="Thread"/>.</param>
        /// <param name="addTaskWakesUp"><c>true</c> if and only if invocation of <see cref="AddTask(IRunnable)"/> will wake up the executor thread.</param>
        public SingleThreadEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, bool addTaskWakesUp)
            : this(parent, threadFactory, addTaskWakesUp, DefaultMaxPendingExecutorTasks)
        {
        }

        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutor"/>.</summary>
        /// <param name="parent">the <see cref="IEventExecutorGroup"/> which is the parent of this instance and belongs to it.</param>
        /// <param name="threadFactory">the <see cref="IThreadFactory"/> which will be used for the used <see cref="Thread"/>.</param>
        /// <param name="addTaskWakesUp"><c>true</c> if and only if invocation of <see cref="AddTask(IRunnable)"/> will wake up the executor thread.</param>
        /// <param name="maxPendingTasks">the maximum number of pending tasks before new tasks will be rejected.</param>
        public SingleThreadEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, bool addTaskWakesUp, int maxPendingTasks)
            : this(parent, threadFactory, addTaskWakesUp, maxPendingTasks, RejectedExecutionHandlers.Reject())
        {
        }

        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutor"/>.</summary>
        /// <param name="parent">the <see cref="IEventExecutorGroup"/> which is the parent of this instance and belongs to it.</param>
        /// <param name="threadFactory">the <see cref="IThreadFactory"/> which will be used for the used <see cref="Thread"/>.</param>
        /// <param name="addTaskWakesUp"><c>true</c> if and only if invocation of <see cref="AddTask(IRunnable)"/> will wake up the executor thread.</param>
        /// <param name="rejectedHandler">the <see cref="IRejectedExecutionHandler"/> to use.</param>
        public SingleThreadEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, bool addTaskWakesUp, IRejectedExecutionHandler rejectedHandler)
            : this(parent, threadFactory, addTaskWakesUp, DefaultMaxPendingExecutorTasks, rejectedHandler)
        {
        }


        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutor"/>.</summary>
        /// <param name="parent">the <see cref="IEventExecutorGroup"/> which is the parent of this instance and belongs to it.</param>
        /// <param name="threadFactory">the <see cref="IThreadFactory"/> which will be used for the used <see cref="Thread"/>.</param>
        /// <param name="addTaskWakesUp"><c>true</c> if and only if invocation of <see cref="AddTask(IRunnable)"/> will wake up the executor thread.</param>
        /// <param name="maxPendingTasks">the maximum number of pending tasks before new tasks will be rejected.</param>
        /// <param name="rejectedHandler">the <see cref="IRejectedExecutionHandler"/> to use.</param>
        protected SingleThreadEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, bool addTaskWakesUp,
            int maxPendingTasks, IRejectedExecutionHandler rejectedHandler)
            : this(parent, addTaskWakesUp, rejectedHandler)
        {
            if (threadFactory is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.threadFactory); }

            _maxPendingTasks = Math.Max(16, maxPendingTasks);
            _taskQueue = NewTaskQueue(_maxPendingTasks);
            _blockingTaskQueue = _taskQueue as IBlockingQueue<IRunnable>;

            _thread = NewThread(threadFactory);
        }

        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutor"/>.</summary>
        /// <param name="parent">the <see cref="IEventExecutorGroup"/> which is the parent of this instance and belongs to it.</param>
        /// <param name="threadFactory">the <see cref="IThreadFactory"/> which will be used for the used <see cref="Thread"/>.</param>
        /// <param name="addTaskWakesUp"><c>true</c> if and only if invocation of <see cref="AddTask(IRunnable)"/> will wake up the executor thread.</param>
        /// <param name="taskQueue">The pending task queue.</param>
        /// <param name="rejectedHandler">the <see cref="IRejectedExecutionHandler"/> to use.</param>
        protected SingleThreadEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, bool addTaskWakesUp,
            IQueue<IRunnable> taskQueue, IRejectedExecutionHandler rejectedHandler)
            : this(parent, addTaskWakesUp, rejectedHandler)
        {
            if (threadFactory is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.threadFactory); }
            if (taskQueue is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.taskQueue); }

            _maxPendingTasks = DefaultMaxPendingExecutorTasks;
            _taskQueue = taskQueue;
            _blockingTaskQueue = taskQueue as IBlockingQueue<IRunnable>;

            _thread = NewThread(threadFactory);
        }

        private SingleThreadEventExecutor(IEventExecutorGroup parent, bool addTaskWakesUp, IRejectedExecutionHandler rejectedHandler)
            : base(parent)
        {
            if (rejectedHandler is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.rejectedHandler); }

            _firstTask = true;

            _loopAction = Loop;
            _loopCoreAciton = LoopCore;

            _addTaskWakesUp = addTaskWakesUp;
            _rejectedExecutionHandler = rejectedHandler;

            _shutdownHooks = new HashSet<Action>();
            _terminationCompletionSource = NewPromise();
            _threadLock = new CountdownEvent(1);

            _taskScheduler = new ExecutorTaskScheduler(this);
        }

        #endregion

        #region -- Properties --

        /// <summary>
        /// Task Scheduler that will post work to this executor's queue.
        /// </summary>
        public TaskScheduler Scheduler => _taskScheduler;

        protected Thread InnerThread => _thread;

        ///// <summary>
        ///// Allows to track whether executor is progressing through its backlog. Useful for diagnosing / mitigating stalls due to blocking calls in conjunction with IsBacklogEmpty property.
        ///// </summary>
        //public long Progress => Volatile.Read(ref v_progress);

        /// <summary>
        /// Indicates whether executor's backlog is empty. Useful for diagnosing / mitigating stalls due to blocking calls in conjunction with Progress property.
        /// </summary>
        public bool IsBacklogEmpty => !HasTasks;

        /// <summary>
        /// Gets length of backlog of tasks queued for immediate execution.
        /// </summary>
        public int BacklogLength => PendingTasks;

        /// <inheritdoc />
        protected override bool HasTasks => _taskQueue.NonEmpty;

        /// <summary>
        /// Gets the number of tasks that are pending for processing.
        /// </summary>
        /// <remarks>Be aware that this operation may be expensive as it depends on the internal implementation of the
        /// <see cref="SingleThreadEventExecutor"/>. So use it with care!</remarks>
        public virtual int PendingTasks => _taskQueue.Count;

        /// <inheritdoc />
        public override bool IsShuttingDown => (uint)v_executionState >= ShuttingDownState;

        /// <inheritdoc />
        public override bool IsShutdown => (uint)v_executionState >= ShutdownState;

        /// <inheritdoc />
        public override bool IsTerminated => (uint)v_executionState >=/*==*/ TerminatedState;

        /// <inheritdoc />
        public override Task TerminationCompletion => _terminationCompletionSource.Task;

        /// <summary>TBD</summary>
        protected IPromise TerminationCompletionSource => _terminationCompletionSource;

        /// <inheritdoc />
        public override bool IsInEventLoop(Thread t) => _thread == t;

        protected override IEnumerable<IEventExecutor> GetItems() => new[] { this };

        protected long GracefulShutdownStartTime => _gracefulShutdownStartTime;

        protected int ExecutionState => v_executionState;

        #endregion

        #region -- Thread --

        protected virtual Thread NewThread(IThreadFactory threadFactory)
        {
            return threadFactory.NewThread(_loopAction);
        }

        protected virtual void Start()
        {
            _thread.Start();
        }

        private readonly XParameterizedThreadStart _loopAction;
        private void Loop(object s)
        {
            SetCurrentExecutor(this);

            _ = Task.Factory.StartNew(_loopCoreAciton, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        private readonly Action _loopCoreAciton;
        private void LoopCore()
        {
            try
            {
                _ = CompareAndSetExecutionState(NotStartedState, StartedState);

                bool success = false;
                UpdateLastExecutionTime();
                try
                {
                    Run();
                    success = true;
                }
                catch (Exception exc)
                {
                    Logger.UnexpectedExceptionFromAnEventExecutor(exc);
                }
                finally
                {
                    CleanupAndTerminate(success);
                }
            }
            catch (Exception ex)
            {
                Logger.ExecutionLoopFailed(_thread, ex);
                SetExecutionState(TerminatedState);
                _ = _terminationCompletionSource.TrySetException(ex);
            }
        }

        #endregion

        #region -- Utilities --

        protected virtual long GetTimeFromStart()
        {
            return PreciseTime.NanoTime();
        }

        protected virtual long ToPreciseTime(TimeSpan time)
        {
            return PreciseTime.TicksToPreciseTicks(time.Ticks);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected bool CompareAndSetExecutionState(int currentState, int newState)
        {
            return currentState == Interlocked.CompareExchange(ref v_executionState, newState, currentState);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected void SetExecutionState(int newState)
        {
            _ = Interlocked.Exchange(ref v_executionState, newState);
        }

        protected void TrySetExecutionState(int newState)
        {
            var currentState = v_executionState;
            int oldState;
            do
            {
                oldState = currentState;

                if ((uint)oldState >= newState) { break; }

                currentState = Interlocked.CompareExchange(ref v_executionState, newState, oldState);
            } while (currentState != oldState);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected static void Reject()
        {
            ThrowHelper.ThrowRejectedExecutionException_Terminated();
        }

        /// <summary>
        /// Offers the task to the associated <see cref="IRejectedExecutionHandler"/>.
        /// </summary>
        /// <param name="task">The task to reject.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected void Reject(IRunnable task)
        {
            _rejectedExecutionHandler.Rejected(task, this);
        }

        #endregion

        protected static IQueue<IRunnable> NewBlockingTaskQueue(int maxPendingTasks)
        {
            maxPendingTasks = Math.Max(16, maxPendingTasks);
            return int.MaxValue == maxPendingTasks
                ? new CompatibleBlockingQueue<IRunnable>()
                : new CompatibleBlockingQueue<IRunnable>(maxPendingTasks);
        }

        protected virtual IQueue<IRunnable> NewTaskQueue(int maxPendingTasks)
        {
            return NewBlockingTaskQueue(maxPendingTasks);
        }

        protected virtual IRunnable PollTask()
        {
            Debug.Assert(InEventLoop);
            return PollTaskFrom(_taskQueue);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected static IRunnable PollTaskFrom(IQueue<IRunnable> taskQueue)
        {
            if (!taskQueue.TryDequeue(out IRunnable task)) { return null; }

            if (task != WakeupTask) { return task; }

            return PollTaskFromSlow(taskQueue);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IRunnable PollTaskFromSlow(IQueue<IRunnable> taskQueue)
        {
            for (; ; )
            {
                if (!taskQueue.TryDequeue(out IRunnable task)) { return null; }
                if (task != WakeupTask) { return task; }
            }
        }

        /// <summary>
        /// Take the next <see cref="IRunnable"/> from the task queue and so will block if no task is currently present.
        /// 
        /// <para>Be aware that this method will throw an <see cref="NotSupportedException"/> if the task queue
        /// does not implement <see cref="IBlockingQueue{T}"/>.</para>
        /// </summary>
        /// <returns><c>null</c> if the executor thread has been interrupted or waken up.</returns>
        protected IRunnable TakeTask()
        {
            Debug.Assert(InEventLoop);
            if (_blockingTaskQueue is null) { ThrowHelper.ThrowNotSupportedException(); }

            if (_scheduledTaskQueue.TryPeek(out IScheduledRunnable scheduledTask))
            {
                if (TryTakeTask(scheduledTask.DelayNanos, out IRunnable task)) { return task; }
            }
            else
            {
                var task = _blockingTaskQueue.Take();
                if (task == WakeupTask) { task = null; }
                return task;
            }

            return TakeTaskSlow();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IRunnable TakeTaskSlow()
        {
            for (; ; )
            {
                if (_scheduledTaskQueue.TryPeek(out IScheduledRunnable scheduledTask))
                {
                    if (TryTakeTask(scheduledTask.DelayNanos, out IRunnable task)) { return task; }
                }
                else
                {
                    var task = _blockingTaskQueue.Take();
                    if (task == WakeupTask) { task = null; }
                    return task;
                }
            }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private bool TryTakeTask(long delayNanos, out IRunnable task)
        {
            const long MaxDelayMilliseconds = int.MaxValue - 1;

            if ((ulong)delayNanos > 0UL) // delayNanos 为非负值
            {
                var timeout = PreciseTime.ToMilliseconds(delayNanos);
                if (_blockingTaskQueue.TryTake(out task, (int)Math.Min(timeout, MaxDelayMilliseconds)))
                {
                    return true;
                }
            }

            // We need to fetch the scheduled tasks now as otherwise there may be a chance that
            // scheduled tasks are never executed if there is always one task in the taskQueue.
            // This is for example true for the read task of OIO Transport
            // See https://github.com/netty/netty/issues/1614
            _ = FetchFromScheduledTaskQueue();
            return _blockingTaskQueue.TryTake(out task, 0);
        }

        protected bool FetchFromScheduledTaskQueue()
        {
            if (_scheduledTaskQueue.IsEmpty) { return true; }

            var nanoTime = PreciseTime.NanoTime();
            var scheduledTask = PollScheduledTask(nanoTime);
            var taskQueue = _taskQueue;
            while (scheduledTask is object)
            {
                if (!taskQueue.TryEnqueue(scheduledTask))
                {
                    // No space left in the task queue add it back to the scheduledTaskQueue so we pick it up again.
                    _ = _scheduledTaskQueue.TryEnqueue(scheduledTask);
                    return false;
                }
                scheduledTask = PollScheduledTask(nanoTime);
            }
            return true;
        }

        /// <summary>
        /// Return <c>true</c> if at least one scheduled task was executed.
        /// </summary>
        private bool ExecuteExpiredScheduledTasks()
        {
            if (_scheduledTaskQueue.IsEmpty) { return false; }

            var nanoTime = PreciseTime.NanoTime();
            var scheduledTask = PollScheduledTask(nanoTime);
            if (scheduledTask is null) { return false; }
            do
            {
                SafeExecute(scheduledTask);
            } while ((scheduledTask = PollScheduledTask(nanoTime)) is object);
            return true;
        }

        protected IRunnable PeekTask()
        {
            Debug.Assert(InEventLoop);
            return _taskQueue.TryPeek(out var task) ? task : null;
        }

        protected bool TryPeekTask(out IRunnable task)
        {
            Debug.Assert(InEventLoop);
            return _taskQueue.TryPeek(out task);
        }

        /// <summary>
        /// Add a task to the task queue, or throws a <see cref="RejectedExecutionException"/> if this instance was shutdown before.
        /// </summary>
        /// <param name="task"></param>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected void AddTask(IRunnable task)
        {
#if DEBUG
            if (task is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.task); }
#endif

            if (!OfferTask(task)) { Reject(task); }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal bool OfferTask(IRunnable task)
        {
            if (IsShutdown) { Reject(); }

            return _taskQueue.TryEnqueue(task);
        }

        /// <summary>
        /// Poll all tasks from the task queue and run them via <see cref="IRunnable.Run()"/> method.
        /// </summary>
        /// <returns><c>true</c> if and only if at least one task was run</returns>
        protected bool RunAllTasks()
        {
            Debug.Assert(InEventLoop);

            OnBeginRunningAllTasks();

            bool fetchedAll;
            bool ranAtLeastOne = false;
            var taskQueue = _taskQueue;
            do
            {
                fetchedAll = FetchFromScheduledTaskQueue();
                if (RunAllTasksFrom(taskQueue))
                {
                    ranAtLeastOne = true;
                }
            } while (!fetchedAll);  // keep on processing until we fetched all scheduled tasks.

            if (ranAtLeastOne)
            {
                UpdateLastExecutionTime();
            }

            OnEndRunningAllTasks();

            AfterRunningAllTasks();

            return ranAtLeastOne;
        }

        /// <summary>
        /// Execute all expired scheduled tasks and all current tasks in the executor queue until both queues are empty,
        /// or <paramref name="maxDrainAttempts"/> has been exceeded.
        /// </summary>
        /// <param name="maxDrainAttempts">The maximum amount of times this method attempts to drain from queues. This is to prevent
        /// continuous task execution and scheduling from preventing the EventExecutor thread to
        /// make progress and return to the selector mechanism to process inbound I/O events.</param>
        /// <returns><c>true</c> if at least one task was run.</returns>
        protected bool RunScheduledAndExecutorTasks(int maxDrainAttempts)
        {
            Debug.Assert(InEventLoop);

            OnBeginRunningAllTasks();

            bool ranAtLeastOneTask;
            int drainAttempt = 0;
            var taskQueue = _taskQueue;
            do
            {
                // We must run the taskQueue tasks first, because the scheduled tasks from outside the EventLoop are queued
                // here because the taskQueue is thread safe and the scheduledTaskQueue is not thread safe.
                ranAtLeastOneTask = RunExistingTasksFrom(taskQueue) | ExecuteExpiredScheduledTasks();
            } while (ranAtLeastOneTask && ++drainAttempt < maxDrainAttempts);

            if (drainAttempt > 0)
            {
                UpdateLastExecutionTime();
            }

            OnEndRunningAllTasks();

            AfterRunningAllTasks();

            return drainAttempt > 0;
        }

        /// <summary>
        /// Runs all tasks from the passed <paramref name="taskQueue"/>.
        /// </summary>
        /// <param name="taskQueue">To poll and execute all tasks.</param>
        /// <returns><c>true</c> if at least one task was executed.</returns>
        protected bool RunAllTasksFrom(IQueue<IRunnable> taskQueue)
        {
            IRunnable task = PollTaskFrom(taskQueue);
            if (task is null) { return false; }

            for (; ; )
            {
                //Volatile.Write(ref v_progress, v_progress + 1); // volatile write is enough as this is the only thread ever writing
                SafeExecute(task);
                task = PollTaskFrom(taskQueue);
                if (task is null)
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// What ever tasks are present in <paramref name="taskQueue"/> when this method is invoked will be <see cref="IRunnable.Run()"/>.
        /// </summary>
        /// <param name="taskQueue">the task queue to drain.</param>
        /// <returns><c>true</c> if at least <see cref="IRunnable.Run()"/> was called.</returns>
        private bool RunExistingTasksFrom(IQueue<IRunnable> taskQueue)
        {
            IRunnable task = PollTaskFrom(taskQueue);
            if (task is null) { return false; }

            int remaining = Math.Min(_maxPendingTasks, taskQueue.Count);
            SafeExecute(task);
            // Use taskQueue.poll() directly rather than pollTaskFrom() since the latter may
            // silently consume more than one item from the queue (skips over WAKEUP_TASK instances)
            while (remaining-- > 0 && taskQueue.TryDequeue(out task))
            {
                SafeExecute(task);
            }
            return true;
        }

        /// <summary>
        /// Poll all tasks from the task queue and run them via <see cref="IRunnable.Run()"/> method.  This method stops running
        /// the tasks in the task queue and returns if it ran longer than <paramref name="timeout"/>.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected bool RunAllTasks(long timeout)
        {
            _ = FetchFromScheduledTaskQueue();
            IRunnable task = PollTask();
            if (task is null)
            {
                AfterRunningAllTasks();
                return false;
            }

            OnBeginRunningAllTasks();

            long deadline = timeout > 0L ? GetTimeFromStart() + timeout : 0L;
            long runTasks = 0;
            long executionTime;
            while (true)
            {
                SafeExecute(task);

                runTasks++;

                // Check timeout every 64 tasks because nanoTime() is relatively expensive.
                // XXX: Hard-coded value - will make it configurable if it is really a problem.
                if (0ul >= (ulong)(runTasks & 0x3F))
                {
                    executionTime = GetTimeFromStart();
                    if (executionTime >= deadline) { break; }
                }

                task = PollTask();
                if (task is null)
                {
                    executionTime = GetTimeFromStart();
                    break;
                }
            }

            OnEndRunningAllTasks();

            AfterRunningAllTasks();

            _lastExecutionTime = executionTime;
            return true;
        }

        protected virtual void OnBeginRunningAllTasks() { }
        protected virtual void OnEndRunningAllTasks() { }

        /// <summary>
        /// Invoked before returning from <see cref="RunAllTasks()"/> and <see cref="RunAllTasks(long)"/>.
        /// </summary>
        protected virtual void AfterRunningAllTasks() { }

        /// <summary>
        /// Updates the internal timestamp that tells when a submitted task was executed most recently.
        /// <see cref="RunAllTasks()"/> and <see cref="RunAllTasks(long)"/> updates this timestamp automatically, and thus there's
        /// usually no need to call this method.  However, if you take the tasks manually using <see cref="TakeTask()"/> or
        /// <see cref="PollTask()"/>, you have to call this method at the end of task execution loop for accurate quiet period
        /// checks.
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected void UpdateLastExecutionTime()
        {
            _lastExecutionTime = GetTimeFromStart();
        }

        /// <summary>
        /// Run the tasks in the <c>taskQueue</c>
        /// </summary>
        protected abstract void Run();

        /// <summary>
        /// Do nothing, sub-classes may override
        /// </summary>
        protected virtual void Cleanup()
        {
            // NOOP
        }

        protected internal virtual void WakeUp(bool inEventLoop)
        {
            if (!inEventLoop/* || (v_executionState == ST_SHUTTING_DOWN)*/)
            {
                // Use offer as we actually only need this to unblock the thread and if offer fails we do not care as there
                // is already something in the queue.
                _ = _taskQueue.TryEnqueue(WakeupTask);
            }
        }

        /// <summary>
        /// Adds an <see cref="Action"/> which will be executed on shutdown of this instance.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to run on shutdown.</param>
        public void AddShutdownHook(Action action)
        {
            if (InEventLoop)
            {
                _ = _shutdownHooks.Add(action);
            }
            else
            {
                Execute(AddShutdownHookAction, _shutdownHooks, action);
            }
        }

        private static readonly Action<object, object> AddShutdownHookAction = OnAddShutdownHook;
        private static void OnAddShutdownHook(object s, object a)
        {
            _ = ((ISet<Action>)s).Add((Action)a);
        }

        /// <summary>
        /// Removes a previously added <see cref="Action"/> from the collection of <see cref="Action"/>s which will be
        /// executed on shutdown of this instance.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to remove.</param>
        public void RemoveShutdownHook(Action action)
        {
            if (InEventLoop)
            {
                _ = _shutdownHooks.Remove(action);
            }
            else
            {
                Execute(RemoveShutdownHookAction, _shutdownHooks, action);
            }
        }

        private static readonly Action<object, object> RemoveShutdownHookAction = OnRemoveShutdownHook;
        private static void OnRemoveShutdownHook(object s, object a)
        {
            _ = ((ISet<Action>)s).Remove((Action)a);
        }

        private bool RunShutdownHooks()
        {
            bool ran = false;

            // Note shutdown hooks can add / remove shutdown hooks.
            while ((uint)_shutdownHooks.Count > 0u)
            {
                var copy = _shutdownHooks.ToArray();
                _shutdownHooks.Clear();

                for (var i = 0; i < copy.Length; i++)
                {
                    try
                    {
                        copy[i]();
                    }
                    catch (Exception ex)
                    {
                        Logger.ShutdownHookRaisedAnException(ex);
                    }
                    finally
                    {
                        ran = true;
                    }
                }
            }

            if (ran)
            {
                UpdateLastExecutionTime();
            }

            return ran;
        }

        /// <inheritdoc />
        public override Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            if (quietPeriod < TimeSpan.Zero) { ThrowHelper.ThrowArgumentException_MustBeGreaterThanOrEquelToZero(quietPeriod); }
            if (timeout < quietPeriod) { ThrowHelper.ThrowArgumentException_MustBeGreaterThanQuietPeriod(timeout, quietPeriod); }

            if (IsShuttingDown) { return TerminationCompletion; }

            OnBeginShutdownGracefully();

            bool inEventLoop = InEventLoop;
            bool wakeup;
            int thisState = v_executionState;
            int oldState;
            do
            {
                if (IsShuttingDown) { return TerminationCompletion; }

                int newState;
                wakeup = true;
                oldState = thisState;
                if (inEventLoop)
                {
                    newState = ShuttingDownState;
                }
                else
                {
                    switch (oldState)
                    {
                        case NotStartedState:
                        case StartedState:
                            newState = ShuttingDownState;
                            break;
                        default:
                            newState = oldState;
                            wakeup = false;
                            break;
                    }
                }
                thisState = Interlocked.CompareExchange(ref v_executionState, newState, oldState);
            } while ((uint)(thisState - oldState) > 0u);

            _ = Interlocked.Exchange(ref v_gracefulShutdownQuietPeriod, ToPreciseTime(quietPeriod));
            _ = Interlocked.Exchange(ref v_gracefulShutdownTimeout, ToPreciseTime(timeout));

            //if (EnsureThreadStarted(oldState))
            //{
            //    return _terminationCompletionSource.Task;
            //}

            if (wakeup)
            {
                _taskQueue.TryEnqueue(WakeupTask);
                if (!_addTaskWakesUp)
                {
                    WakeUp(inEventLoop);
                }
            }

            return TerminationCompletion;
        }

        protected virtual void OnBeginShutdownGracefully()
        {
        }

        /// <summary>
        /// Confirm that the shutdown if the instance should be done now!
        /// </summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected virtual bool ConfirmShutdown()
        {
            if (!IsShuttingDown) { return false; }

            return ConfirmShutdownSlow();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool ConfirmShutdownSlow()
        {
            if (!InEventLoop) { ThrowHelper.ThrowInvalidOperationException_Must_be_invoked_from_an_event_loop(); }

            CancelScheduledTasks();

            if (0ul >= (ulong)_gracefulShutdownStartTime)
            {
                _gracefulShutdownStartTime = GetTimeFromStart();
            }

            if (RunAllTasks() || RunShutdownHooks())
            {
                if (IsShutdown)
                {
                    // Executor shut down - no new tasks anymore.
                    return true;
                }

                // There were tasks in the queue. Wait a little bit more until no tasks are queued for the quiet period or
                // terminate if the quiet period is 0.
                // See https://github.com/netty/netty/issues/4241
                if (0ul >= (ulong)Volatile.Read(ref v_gracefulShutdownQuietPeriod))
                {
                    return true;
                }
                _taskQueue.TryEnqueue(WakeupTask);
                return false;
            }

            long nanoTime = GetTimeFromStart();

            if (IsShutdown || (nanoTime - _gracefulShutdownStartTime > Volatile.Read(ref v_gracefulShutdownTimeout)))
            {
                return true;
            }

            if (nanoTime - _lastExecutionTime <= Volatile.Read(ref v_gracefulShutdownQuietPeriod))
            {
                // Check if any tasks were added to the queue every 100ms.
                // TODO: Change the behavior of takeTask() so that it returns on timeout.
                _taskQueue.TryEnqueue(WakeupTask);

                Thread.Sleep(100);

                return false;
            }

            // No tasks were added for last quiet period - hopefully safe to shut down.
            // (Hopefully because we really cannot make a guarantee that there will be no execute() calls by a user.)
            return true;
        }

        protected ShutdownStatus DoShuttingdown()
        {
            if (!InEventLoop) { ThrowHelper.ThrowInvalidOperationException_Must_be_invoked_from_an_event_loop(); }

            CancelScheduledTasks();

            if (0ul >= (ulong)_gracefulShutdownStartTime)
            {
                _gracefulShutdownStartTime = GetTimeFromStart();
            }

            if (RunAllTasks() || RunShutdownHooks())
            {
                if (IsShutdown)
                {
                    // Executor shut down - no new tasks anymore.
                    return ShutdownStatus.Completed;
                }

                // There were tasks in the queue. Wait a little bit more until no tasks are queued for the quiet period or
                // terminate if the quiet period is 0.
                // See https://github.com/netty/netty/issues/4241
                if (0ul >= (ulong)Volatile.Read(ref v_gracefulShutdownQuietPeriod))
                {
                    return ShutdownStatus.Completed;
                }
                _taskQueue.TryEnqueue(WakeupTask);
                return ShutdownStatus.Progressing;
            }

            long nanoTime = GetTimeFromStart();

            if (IsShutdown || (nanoTime - _gracefulShutdownStartTime > Volatile.Read(ref v_gracefulShutdownTimeout)))
            {
                return ShutdownStatus.Completed;
            }

            if (nanoTime - _lastExecutionTime <= Volatile.Read(ref v_gracefulShutdownQuietPeriod))
            {
                // Check if any tasks were added to the queue every 100ms.
                // TODO: Change the behavior of takeTask() so that it returns on timeout.
                _taskQueue.TryEnqueue(WakeupTask);

                return ShutdownStatus.WaitingForNextPeriod;
            }

            // No tasks were added for last quiet period - hopefully safe to shut down.
            // (Hopefully because we really cannot make a guarantee that there will be no execute() calls by a user.)
            return ShutdownStatus.Completed;
        }

        protected enum ShutdownStatus
        {
            Progressing,
            WaitingForNextPeriod,
            Completed,
        }

        protected void CleanupAndTerminate(bool success)
        {
            TrySetExecutionState(ShuttingDownState);

            // Check if confirmShutdown() was called at the end of the loop.
            if (success && (0ul >= (ulong)_gracefulShutdownStartTime))
            {
                Logger.BuggyImplementation(this);
            }

            try
            {
                if (!IsTerminated)
                {
                    // Run all remaining tasks and shutdown hooks. At this point the event loop
                    // is in ST_SHUTTING_DOWN state still accepting tasks which is needed for
                    // graceful shutdown with quietPeriod.
                    while (true)
                    {
                        if (ConfirmShutdown())
                        {
                            break;
                        }
                    }

                    // Now we want to make sure no more tasks can be added from this point. This is
                    // achieved by switching the state. Any new tasks beyond this point will be rejected.
                    TrySetExecutionState(ShutdownState);

                    // We have the final set of tasks in the queue now, no more can be added, run all remaining.
                    // No need to loop here, this is the final pass.
                    _ = ConfirmShutdown();
                }
            }
            finally
            {
                try
                {
                    Cleanup();
                }
                finally
                {
                    SetExecutionState(TerminatedState);
                    if (!_threadLock.IsSet) { _ = _threadLock.Signal(); }
                    int numUserTasks = DrainTasks();
                    if ((uint)numUserTasks > 0u && Logger.WarnEnabled)
                    {
                        Logger.AnEventExecutorTerminatedWithNonEmptyTaskQueue(numUserTasks);
                    }

                    _terminationCompletionSource.TryComplete();
                }
            }
        }

        internal int DrainTasks()
        {
            int numTasks = 0;
            while (_taskQueue.TryDequeue(out var runnable))
            {
                // WAKEUP_TASK should be just discarded as these are added internally.
                // The important bit is that we not have any user tasks left.
                if (WakeupTask != runnable)
                {
                    numTasks++;
                }
            }
            return numTasks;
        }

        public override bool WaitTermination(TimeSpan timeout)
        {
            if (InEventLoop)
            {
                ThrowHelper.ThrowInvalidOperationException_Cannot_await_termination_of_the_current_thread();
            }

            _threadLock.Wait(timeout);

            return IsTerminated;
        }

        /// <inheritdoc />
        public override void Execute(IRunnable task)
        {
            if (!(task is ILazyRunnable)
#if DEBUG
                && WakesUpForTask(task)
#endif
                )
            {
                InternalExecute(task, true);
            }
            else
            {
                InternalLazyExecute(task);
            }
        }

        public override void LazyExecute(IRunnable task)
        {
            InternalExecute(task, false);
        }

        protected virtual void InternalLazyExecute(IRunnable task)
        {
            // netty 第一个任务进来，不管是否延迟任务，都会启动线程
            // 防止线程启动后，第一个进来的就是 lazy task
            var firstTask = _firstTask;
            if (firstTask) { _firstTask = false; }
            InternalExecute(task, firstTask);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void InternalExecute(IRunnable task, bool immediate)
        {
            bool inEventLoop = InEventLoop;

            AddTask(task);
            //if (!inEventLoop)
            //{
            //    //StartThread();
            //    if (IsShutdown)
            //    {
            //        bool reject = false;
            //        try
            //        {
            //            if (removeTask(task))
            //            {
            //                reject = true;
            //            }
            //        }
            //        catch (UnsupportedOperationException e)
            //        {
            //            // The task queue does not support removal so the best thing we can do is to just move on and
            //            // hope we will be able to pick-up the task before its completely terminated.
            //            // In worst case we will log on termination.
            //        }
            //        if (reject)
            //        {
            //            Reject();
            //        }
            //    }
            //}

            if (!_addTaskWakesUp && immediate)
            {
                WakeUp(inEventLoop);
            }
        }

#if DEBUG
        /// <summary>
        /// Can be overridden to control which tasks require waking the <see cref="IEventExecutor"/> thread
        /// if it is waiting so that they can be run immediately.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected virtual bool WakesUpForTask(IRunnable task)
        {
            return true;
        }
#endif

        //protected virtual bool EnsureThreadStarted(int oldState)
        //{
        //    return true;
        //}
    }
}