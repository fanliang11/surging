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

namespace DotNetty.Transport.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;

    /// <summary>
    /// Default <see cref="SingleThreadEventLoopBase"/> implementation which just execute all submitted task in a serial fashion.
    /// </summary>
    public class SingleThreadEventLoop : SingleThreadEventLoopBase
    {
        protected static readonly TimeSpan DefaultBreakoutInterval = TimeSpan.FromMilliseconds(100);

        private readonly long _breakoutNanosInterval;
        private readonly ManualResetEventSlim _emptyEvent;

        public SingleThreadEventLoop(IEventLoopGroup parent)
            : this(parent, DefaultBreakoutInterval)
        {
        }

        public SingleThreadEventLoop(IEventLoopGroup parent, TimeSpan breakoutInterval)
            : this(parent, RejectedExecutionHandlers.Reject(), breakoutInterval)
        {
        }

        public SingleThreadEventLoop(IEventLoopGroup parent, IRejectedExecutionHandler rejectedHandler)
            : this(parent, rejectedHandler, DefaultBreakoutInterval)
        {
        }

        public SingleThreadEventLoop(IEventLoopGroup parent, IRejectedExecutionHandler rejectedHandler, TimeSpan breakoutInterval)
            : this(parent, DefaultThreadFactory<SingleThreadEventLoop>.Instance, rejectedHandler, breakoutInterval)
        {
        }

        public SingleThreadEventLoop(IEventLoopGroup parent, IThreadFactory threadFactory)
            : this(parent, threadFactory, DefaultBreakoutInterval)
        {
        }

        public SingleThreadEventLoop(IEventLoopGroup parent, IThreadFactory threadFactory, TimeSpan breakoutInterval)
            : this(parent, threadFactory, RejectedExecutionHandlers.Reject(), breakoutInterval)
        {
        }

        public SingleThreadEventLoop(IEventLoopGroup parent, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler)
            : this(parent, threadFactory, rejectedHandler, DefaultBreakoutInterval)
        {
        }

        public SingleThreadEventLoop(IEventLoopGroup parent, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, TimeSpan breakoutInterval)
            : base(parent, threadFactory, false, int.MaxValue, rejectedHandler)
        {
            _emptyEvent = new ManualResetEventSlim(false, 1);
            _breakoutNanosInterval = PreciseTime.ToDelayNanos(breakoutInterval);
            Start();
        }

        protected sealed override IQueue<IRunnable> NewTaskQueue(int maxPendingTasks)
        {
            // This event loop never calls takeTask()
            return new CompatibleConcurrentQueue<IRunnable>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Run()
        {
            while (true)
            {
                if (!IsShuttingDown)
                {
                    RunAllTasks(_breakoutNanosInterval);
                }
                else
                {
                    if (ConfirmShutdown()) { break; }
                }
            }
        }

        /// <inheritdoc />
        public sealed override void Execute(IRunnable task)
        {
            InternalExecute(task);
        }

        public sealed override void LazyExecute(IRunnable task)
        {
            InternalExecute(task);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void InternalExecute(IRunnable task)
        {
            AddTask(task);

            if (!InEventLoop) { _emptyEvent.Set(); }
        }

        protected sealed override void WakeUp(bool inEventLoop)
        {
            if (!inEventLoop || IsShuttingDown)
            {
                Execute(WakeupTask);
            }
        }

        protected sealed override IRunnable PollTask()
        {
            const long MaxDelayMilliseconds = int.MaxValue - 1;

            Debug.Assert(InEventLoop);

            var taskQueue = _taskQueue;
            var task = PollTaskFrom(taskQueue);
            if (task is object) { return task; }
#if DEBUG
            if (_tailTasks.IsEmpty) { _emptyEvent.Reset(); }
#else
            _emptyEvent.Reset();
#endif
            task = PollTaskFrom(taskQueue);
            if (task is object || IsShuttingDown) // revisit queue as producer might have put a task in meanwhile
            {
                return task;
            }

            // revisit queue as producer might have put a task in meanwhile
            if (TryPeekScheduledTask(out IScheduledRunnable nextScheduledTask))
            {
                var delayNanos = nextScheduledTask.DelayNanos;
                if ((ulong)delayNanos > 0UL) // delayNanos 为非负值
                {
                    var timeout = PreciseTime.ToMilliseconds(delayNanos);
                    _emptyEvent.Wait((int)Math.Min(timeout, MaxDelayMilliseconds));
                }
            }
            else
            {
                if (!IsShuttingDown) { _emptyEvent.Wait(); } 
                task = PollTaskFrom(taskQueue);
            }
            return task;
        }
    }
}