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

namespace DotNetty.Transport.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;

    /// <summary>
    /// The class for <see cref="IEventLoop"/>s that execute all its submitted tasks in a single thread.
    /// </summary>
    public abstract class SingleThreadEventLoopBase : SingleThreadEventExecutor, IEventLoop
    {
        protected static readonly int DefaultMaxPendingTasks = Math.Max(16,
                SystemPropertyUtil.GetInt("io.netty.eventLoop.maxPendingTasks", int.MaxValue));

#if DEBUG
        protected readonly IQueue<IRunnable> _tailTasks;
#endif

        protected SingleThreadEventLoopBase(IEventLoopGroup parent, IThreadFactory threadFactory, bool addTaskWakesUp)
            : this(parent, threadFactory, addTaskWakesUp, DefaultMaxPendingTasks, RejectedExecutionHandlers.Reject())
        {
        }

        protected SingleThreadEventLoopBase(IEventLoopGroup parent, IThreadFactory threadFactory, bool addTaskWakesUp,
            IRejectedExecutionHandler rejectedHandler)
            : this(parent, threadFactory, addTaskWakesUp, DefaultMaxPendingTasks, rejectedHandler)
        {
        }

        protected SingleThreadEventLoopBase(IEventLoopGroup parent, IThreadFactory threadFactory, bool addTaskWakesUp,
            int maxPendingTasks, IRejectedExecutionHandler rejectedHandler)
            : base(parent, threadFactory, addTaskWakesUp, maxPendingTasks, rejectedHandler)
        {
#if DEBUG
            _tailTasks = NewTaskQueue(maxPendingTasks);
#endif
        }

#if DEBUG
        protected SingleThreadEventLoopBase(IEventLoopGroup parent, IThreadFactory threadFactory, bool addTaskWakesUp,
            IQueue<IRunnable> taskQueue, IQueue<IRunnable> tailTaskQueue, IRejectedExecutionHandler rejectedHandler)
            : base(parent, threadFactory, addTaskWakesUp, taskQueue, rejectedHandler)
        {
            if (tailTaskQueue is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.tailTaskQueue); }
            _tailTasks = tailTaskQueue;
        }
#else
        protected SingleThreadEventLoopBase(IEventLoopGroup parent, IThreadFactory threadFactory, bool addTaskWakesUp,
            IQueue<IRunnable> taskQueue, IRejectedExecutionHandler rejectedHandler)
            : base(parent, threadFactory, addTaskWakesUp, taskQueue, rejectedHandler)
        {
        }
#endif

        public new IEventLoop GetNext() => this;

        public new IEventLoopGroup Parent => (IEventLoopGroup)base.Parent;

        public new IEnumerable<IEventLoop> Items => new[] { this };

        /// <inheritdoc />
        public Task RegisterAsync(IChannel channel) => channel.Unsafe.RegisterAsync(this);

#if DEBUG
        protected override bool HasTasks => base.HasTasks || _tailTasks.NonEmpty;

        public override int PendingTasks => base.PendingTasks + _tailTasks.Count;

        /// <summary>
        /// Adds a task to be run once at the end of next (or current) <c>eventloop</c> iteration.
        /// </summary>
        /// <param name="task"></param>
        public void ExecuteAfterEventLoopIteration(IRunnable task)
        {
            if (task is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.task); }

            if (IsShutdown) { Reject(); }

            if (!_tailTasks.TryEnqueue(task))
            {
                Reject(task);
            }

            if (!(task is ILazyRunnable) && WakesUpForTask(task))
            {
                WakeUp(InEventLoop);
            }
        }

        protected override void AfterRunningAllTasks()
        {
            RunAllTasksFrom(_tailTasks);
        }
#endif
    }
}