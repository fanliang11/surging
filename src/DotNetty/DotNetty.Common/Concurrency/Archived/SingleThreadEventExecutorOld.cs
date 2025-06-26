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
    public class SingleThreadEventExecutorOld : AbstractScheduledEventExecutor, IOrderedEventExecutor
    {
        private const int ST_NOT_STARTED = 1;
        private const int ST_STARTED = 2;
        private const int ST_SHUTTING_DOWN = 3;
        private const int ST_SHUTDOWN = 4;
        private const int ST_TERMINATED = 5;
        private const string DefaultWorkerThreadName = "SingleThreadEventExecutorOld worker";

        private static readonly IInternalLogger Logger =
            InternalLoggerFactory.GetInstance<SingleThreadEventExecutorOld>();

        private readonly XParameterizedThreadStart _loopAction;
        private readonly Action _loopCoreAciton;

        private readonly IQueue<IRunnable> _taskQueue;
        private readonly Thread _thread;
        private int v_executionState = ST_NOT_STARTED;
        private readonly PreciseTimeSpan _preciseBreakoutInterval;
        private PreciseTimeSpan _lastExecutionTime;
        private readonly ManualResetEventSlim _emptyEvent;
        private readonly TaskScheduler _scheduler;
        private readonly IPromise _terminationCompletionSource;
        private PreciseTimeSpan _gracefulShutdownStartTime;
        private PreciseTimeSpan _gracefulShutdownQuietPeriod;
        private PreciseTimeSpan _gracefulShutdownTimeout;
        private readonly ISet<Action> _shutdownHooks;
        private long v_progress;

        private bool _firstTask; // 不需要设置 volatile

        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutorOld"/>.</summary>
        public SingleThreadEventExecutorOld(string threadName, TimeSpan breakoutInterval)
            : this(null, threadName, breakoutInterval, new CompatibleConcurrentQueue<IRunnable>())
        {
        }

        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutorOld"/>.</summary>
        public SingleThreadEventExecutorOld(IEventExecutorGroup parent, string threadName, TimeSpan breakoutInterval)
            : this(parent, threadName, breakoutInterval, new CompatibleConcurrentQueue<IRunnable>())
        {
        }

        protected SingleThreadEventExecutorOld(string threadName, TimeSpan breakoutInterval, IQueue<IRunnable> taskQueue)
            : this(null, threadName, breakoutInterval, taskQueue)
        { }

        protected SingleThreadEventExecutorOld(IEventExecutorGroup parent, string threadName, TimeSpan breakoutInterval, IQueue<IRunnable> taskQueue)
            : base(parent)
        {
            _firstTask = true;
            _emptyEvent = new ManualResetEventSlim(false, 1);
            _shutdownHooks = new HashSet<Action>();

            _loopAction = Loop;
            _loopCoreAciton = LoopCore;

            _terminationCompletionSource = NewPromise();
            _taskQueue = taskQueue;
            _preciseBreakoutInterval = PreciseTimeSpan.FromTimeSpan(breakoutInterval);
            _scheduler = new ExecutorTaskScheduler(this);
            _thread = new Thread(_loopAction);
            if (string.IsNullOrEmpty(threadName))
            {
                _thread.Name = DefaultWorkerThreadName;
            }
            else
            {
                _thread.Name = threadName;
            }
            _thread.Start();
        }

        /// <summary>
        ///     Task Scheduler that will post work to this executor's queue.
        /// </summary>
        public TaskScheduler Scheduler => _scheduler;

        /// <summary>
        ///     Allows to track whether executor is progressing through its backlog. Useful for diagnosing / mitigating stalls due to blocking calls in conjunction with IsBacklogEmpty property.
        /// </summary>
        public long Progress => Volatile.Read(ref v_progress);

        /// <summary>
        ///     Indicates whether executor's backlog is empty. Useful for diagnosing / mitigating stalls due to blocking calls in conjunction with Progress property.
        /// </summary>
        public bool IsBacklogEmpty => _taskQueue.IsEmpty;

        /// <summary>
        ///     Gets length of backlog of tasks queued for immediate execution.
        /// </summary>
        public int BacklogLength => _taskQueue.Count;

        /// <inheritdoc />
        protected override bool HasTasks => _taskQueue.NonEmpty;

        void Loop(object s)
        {
            SetCurrentExecutor(this);

            _ = Task.Factory.StartNew(_loopCoreAciton, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        void LoopCore()
        {
            try
            {
                _ = Interlocked.CompareExchange(ref v_executionState, ST_STARTED, ST_NOT_STARTED);
                while (!ConfirmShutdown())
                {
                    _ = RunAllTasks(_preciseBreakoutInterval);
                }
                CleanupAndTerminate(true);
            }
            catch (Exception ex)
            {
                Logger.ExecutionLoopFailed(_thread, ex);
                _ = Interlocked.Exchange(ref v_executionState, ST_TERMINATED);
                _ = _terminationCompletionSource.TrySetException(ex);
            }
        }

        /// <inheritdoc />
        public override bool IsShuttingDown => (uint)Volatile.Read(ref v_executionState) >= ST_SHUTTING_DOWN;

        /// <inheritdoc />
        public override Task TerminationCompletion => _terminationCompletionSource.Task;

        /// <inheritdoc />
        public override bool IsShutdown => (uint)Volatile.Read(ref v_executionState) >= ST_SHUTDOWN;

        /// <inheritdoc />
        public override bool IsTerminated => (uint)Volatile.Read(ref v_executionState) >=/*==*/ ST_TERMINATED;

        /// <inheritdoc />
        public override bool IsInEventLoop(Thread t) => _thread == t;

        /// <inheritdoc />
        public override void Execute(IRunnable task)
        {
            if (!(task is ILazyRunnable))
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

        private void InternalLazyExecute(IRunnable task)
        {
            // netty 第一个任务进来，不管是否延迟任务，都会启动线程
            var firstTask = _firstTask;
            if (firstTask) { _firstTask = false; }
            InternalExecute(task, firstTask);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void InternalExecute(IRunnable task, bool immediate)
        {
            AddTask(task);

            if (!InEventLoop)
            {
                if (IsShutdown) { ThrowHelper.ThrowRejectedExecutionException_Terminated(); }

                if (immediate) { _emptyEvent.Set(); }
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private void AddTask(IRunnable task)
        {
            if (IsShutdown)
            {
                ThrowHelper.ThrowRejectedExecutionException_Shutdown();
            }
            if (!_taskQueue.TryEnqueue(task))
            {
                ThrowHelper.ThrowRejectedExecutionException_Queue();
            }
        }

        protected override IEnumerable<IEventExecutor> GetItems() => new[] { this };

        protected internal virtual void WakeUp(bool inEventLoop)
        {
            if (!inEventLoop || (Volatile.Read(ref v_executionState) == ST_SHUTTING_DOWN))
            {
                Execute(WakeupTask);
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

        static readonly Action<object, object> AddShutdownHookAction = OnAddShutdownHook;
        static void OnAddShutdownHook(object s, object a)
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

        static readonly Action<object, object> RemoveShutdownHookAction = OnRemoveShutdownHook;
        static void OnRemoveShutdownHook(object s, object a)
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
                _lastExecutionTime = PreciseTimeSpan.FromStart;
            }

            return ran;
        }

        /// <inheritdoc />
        public override Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            if (quietPeriod < TimeSpan.Zero) { ThrowHelper.ThrowArgumentException_MustBeGreaterThanOrEquelToZero(quietPeriod); }
            if (timeout < quietPeriod) { ThrowHelper.ThrowArgumentException_MustBeGreaterThanQuietPeriod(timeout, quietPeriod); }

            if (IsShuttingDown)
            {
                return TerminationCompletion;
            }

            bool inEventLoop = InEventLoop;
            bool wakeup;
            int thisState = Volatile.Read(ref v_executionState);
            int oldState;
            do
            {
                if (IsShuttingDown)
                {
                    return TerminationCompletion;
                }
                int newState;
                wakeup = true;
                oldState = thisState;
                if (inEventLoop)
                {
                    newState = ST_SHUTTING_DOWN;
                }
                else
                {
                    switch (oldState)
                    {
                        case ST_NOT_STARTED:
                        case ST_STARTED:
                            newState = ST_SHUTTING_DOWN;
                            break;
                        default:
                            newState = oldState;
                            wakeup = false;
                            break;
                    }
                }
                thisState = Interlocked.CompareExchange(ref v_executionState, newState, oldState);
            } while (thisState != oldState);
            _gracefulShutdownQuietPeriod = PreciseTimeSpan.FromTimeSpan(quietPeriod);
            _gracefulShutdownTimeout = PreciseTimeSpan.FromTimeSpan(timeout);

            // TODO: revisit
            //if (ensureThreadStarted(oldState))
            //{
            //    return terminationFuture;
            //}

            if (wakeup)
            {
                WakeUp(inEventLoop);
            }

            return TerminationCompletion;
        }

        protected bool ConfirmShutdown()
        {
            if (!IsShuttingDown) { return false; }

            return ConfirmShutdownSlow();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected bool ConfirmShutdownSlow()
        {
            Debug.Assert(InEventLoop, "must be invoked from an event loop");

            CancelScheduledTasks();

            if (_gracefulShutdownStartTime == PreciseTimeSpan.Zero)
            {
                _gracefulShutdownStartTime = PreciseTimeSpan.FromStart;
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
                if (_gracefulShutdownQuietPeriod == PreciseTimeSpan.Zero)
                {
                    return true;
                }
                WakeUp(true);
                return false;
            }

            PreciseTimeSpan nanoTime = PreciseTimeSpan.FromStart;

            if (IsShutdown || (nanoTime - _gracefulShutdownStartTime > _gracefulShutdownTimeout))
            {
                return true;
            }

            if (nanoTime - _lastExecutionTime <= _gracefulShutdownQuietPeriod)
            {
                // Check if any tasks were added to the queue every 100ms.
                // TODO: Change the behavior of takeTask() so that it returns on timeout.
                // todo: ???
                WakeUp(true);
                Thread.Sleep(100);

                return false;
            }

            // No tasks were added for last quiet period - hopefully safe to shut down.
            // (Hopefully because we really cannot make a guarantee that there will be no execute() calls by a user.)
            return true;
        }

        protected void CleanupAndTerminate(bool success)
        {
            var thisState = Volatile.Read(ref v_executionState);
            int oldState;
            do
            {
                oldState = thisState;

                if ((uint)oldState >= ST_SHUTTING_DOWN) { break; }

                thisState = Interlocked.CompareExchange(ref v_executionState, ST_SHUTTING_DOWN, oldState);
            } while (thisState != oldState);

            // Check if confirmShutdown() was called at the end of the loop.
            if (success && (_gracefulShutdownStartTime == PreciseTimeSpan.Zero))
            {
                Logger.BuggyImplementation();
            }

            try
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
                thisState = Volatile.Read(ref v_executionState);
                do
                {
                    oldState = thisState;

                    if ((uint)oldState >= ST_SHUTDOWN) { break; }

                    thisState = Interlocked.CompareExchange(ref v_executionState, ST_SHUTDOWN, oldState);
                } while (thisState != oldState);

                // We have the final set of tasks in the queue now, no more can be added, run all remaining.
                // No need to loop here, this is the final pass.
                _ = ConfirmShutdown();
            }
            finally
            {
                try
                {
                    Cleanup();
                }
                finally
                {
                    _ = Interlocked.Exchange(ref v_executionState, ST_TERMINATED);
                    int numUserTasks = DrainTasks();
                    if ((uint)numUserTasks > 0u && Logger.WarnEnabled)
                    {
                        Logger.AnEventExecutorTerminatedWithNonEmptyTaskQueue(numUserTasks);
                    }

                    //firstRun = true;
                    _terminationCompletionSource.Complete();
                }
            }
        }

        private int DrainTasks()
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

        protected virtual void Cleanup()
        {
            // NOOP
        }

        protected bool RunAllTasks()
        {
            bool fetchedAll;
            bool ranAtLeastOne;
            do
            {
                fetchedAll = FetchFromScheduledTaskQueue();
                IRunnable task = PollTask();
                if (task is null)
                {
                    return false;
                }

                while (true)
                {
                    Volatile.Write(ref v_progress, v_progress + 1); // volatile write is enough as this is the only thread ever writing
                    SafeExecute(task);
                    task = PollTask();
                    if (task is null)
                    {
                        ranAtLeastOne = true;
                        break;
                    }
                }
            } while (!fetchedAll);  // keep on processing until we fetched all scheduled tasks.

            if (ranAtLeastOne)
            {
                _lastExecutionTime = PreciseTimeSpan.FromStart;
            }
            AfterRunningAllTasks();
            return ranAtLeastOne;
        }

        private bool RunAllTasks(PreciseTimeSpan timeout)
        {
            _ = FetchFromScheduledTaskQueue();
            IRunnable task = PollTask();
            if (task is null)
            {
                AfterRunningAllTasks();
                return false;
            }

            PreciseTimeSpan deadline = PreciseTimeSpan.Deadline(timeout);
            long runTasks = 0;
            PreciseTimeSpan executionTime;
            while (true)
            {
                SafeExecute(task);

                runTasks++;

                // Check timeout every 64 tasks because nanoTime() is relatively expensive.
                // XXX: Hard-coded value - will make it configurable if it is really a problem.
                if (0ul >= (ulong)(runTasks & 0x3F))
                {
                    executionTime = PreciseTimeSpan.FromStart;
                    if (executionTime >= deadline)
                    {
                        break;
                    }
                }

                task = PollTask();
                if (task is null)
                {
                    executionTime = PreciseTimeSpan.FromStart;
                    break;
                }
            }

            AfterRunningAllTasks();
            _lastExecutionTime = executionTime;
            return true;
        }

        /// <summary>
        /// Invoked before returning from <see cref="RunAllTasks()"/> and <see cref="RunAllTasks(PreciseTimeSpan)"/>.
        /// </summary>
        protected virtual void AfterRunningAllTasks() { }

        private bool FetchFromScheduledTaskQueue()
        {
            if (_scheduledTaskQueue.IsEmpty) { return true; }

            var nanoTime = PreciseTime.NanoTime();
            IScheduledRunnable scheduledTask = PollScheduledTask(nanoTime);
            while (scheduledTask is object)
            {
                if (!_taskQueue.TryEnqueue(scheduledTask))
                {
                    // No space left in the task queue add it back to the scheduledTaskQueue so we pick it up again.
                    _ = _scheduledTaskQueue.TryEnqueue(scheduledTask);
                    return false;
                }
                scheduledTask = PollScheduledTask(nanoTime);
            }
            return true;
        }

        private IRunnable PollTask()
        {
            Debug.Assert(InEventLoop);

            if (!_taskQueue.TryDequeue(out IRunnable task))
            {
                _emptyEvent.Reset();
                if (!_taskQueue.TryDequeue(out task) && !IsShuttingDown) // revisit queue as producer might have put a task in meanwhile
                {
                    if (_scheduledTaskQueue.TryPeek(out IScheduledRunnable nextScheduledTask))
                    {
                        PreciseTimeSpan wakeupTimeout = nextScheduledTask.Deadline - PreciseTimeSpan.FromStart;
                        if (wakeupTimeout.Ticks > 0L) // 此处不要 ulong 转换
                        {
                            double timeout = wakeupTimeout.ToTimeSpan().TotalMilliseconds;
                            _ = _emptyEvent.Wait((int)Math.Min(timeout, int.MaxValue - 1));
                        }
                    }
                    else
                    {
                        _emptyEvent.Wait();
                        _ = _taskQueue.TryDequeue(out task);
                    }
                }
            }

            return task;
        }

        public override bool WaitTermination(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}