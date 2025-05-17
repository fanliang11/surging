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
    using DotNetty.Common.Internal;

    /// <summary>
    /// Default <see cref="SingleThreadEventExecutor"/> implementation which just execute all submitted task in a serial fashion.
    /// </summary>
    public sealed class DefaultEventExecutor : SingleThreadEventExecutor
    {
        public DefaultEventExecutor()
            : this(DefaultMaxPendingExecutorTasks)
        {
        }

        public DefaultEventExecutor(int maxPendingTasks)
            : this(RejectedExecutionHandlers.Reject(), maxPendingTasks)
        {
        }

        public DefaultEventExecutor(IRejectedExecutionHandler rejectedHandler)
            : this(rejectedHandler, DefaultMaxPendingExecutorTasks)
        {
        }

        public DefaultEventExecutor(IEventExecutorTaskQueueFactory queueFactory)
            : this(RejectedExecutionHandlers.Reject(), queueFactory)
        {
        }

        public DefaultEventExecutor(IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : this(null, DefaultThreadFactory<DefaultEventExecutor>.Instance, rejectedHandler, maxPendingTasks)
        {
        }

        public DefaultEventExecutor(IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory)
            : this(null, DefaultThreadFactory<DefaultEventExecutor>.Instance, rejectedHandler, queueFactory)
        {
        }


        public DefaultEventExecutor(IThreadFactory threadFactory)
            : this(threadFactory, DefaultMaxPendingExecutorTasks)
        {
        }

        public DefaultEventExecutor(IThreadFactory threadFactory, int maxPendingTasks)
            : this(threadFactory, RejectedExecutionHandlers.Reject(), maxPendingTasks)
        {
        }

        public DefaultEventExecutor(IThreadFactory threadFactory, IEventExecutorTaskQueueFactory queueFactory)
            : this(threadFactory, RejectedExecutionHandlers.Reject(), queueFactory)
        {
        }

        public DefaultEventExecutor(IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler)
            : this(threadFactory, rejectedHandler, DefaultMaxPendingExecutorTasks)
        {
        }

        public DefaultEventExecutor(IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : this(null, threadFactory, rejectedHandler, maxPendingTasks)
        {
        }

        public DefaultEventExecutor(IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory)
            : this(null, threadFactory, rejectedHandler, queueFactory)
        {
        }


        public DefaultEventExecutor(IEventExecutorGroup parent)
            : this(parent, DefaultMaxPendingExecutorTasks)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, int maxPendingTasks)
            : this(parent, RejectedExecutionHandlers.Reject(), maxPendingTasks)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IEventExecutorTaskQueueFactory queueFactory)
            : this(parent, RejectedExecutionHandlers.Reject(), queueFactory)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IRejectedExecutionHandler rejectedHandler)
            : this(parent, rejectedHandler, queueFactory: null)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : this(parent, DefaultThreadFactory<DefaultEventExecutor>.Instance, rejectedHandler, maxPendingTasks)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory)
            : this(parent, DefaultThreadFactory<DefaultEventExecutor>.Instance, rejectedHandler, queueFactory)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, int maxPendingTasks)
            : this(parent, threadFactory, RejectedExecutionHandlers.Reject(), maxPendingTasks)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, IEventExecutorTaskQueueFactory queueFactory)
            : this(parent, threadFactory, RejectedExecutionHandlers.Reject(), queueFactory)
        {
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, int maxPendingTasks)
            : base(parent, threadFactory, true, NewBlockingTaskQueue(maxPendingTasks), rejectedHandler)
        {
            Start();
        }

        public DefaultEventExecutor(IEventExecutorGroup parent, IThreadFactory threadFactory, IRejectedExecutionHandler rejectedHandler, IEventExecutorTaskQueueFactory queueFactory)
            : base(parent, threadFactory, true, NewBlockingTaskQueue(queueFactory), rejectedHandler)
        {
            Start();
        }

        private static IQueue<IRunnable> NewBlockingTaskQueue(IEventExecutorTaskQueueFactory queueFactory)
        {
            if (queueFactory is null)
            {
                return NewBlockingTaskQueue(DefaultMaxPendingExecutorTasks);
            }
            return queueFactory.NewTaskQueue(DefaultMaxPendingExecutorTasks);
        }

        protected override void Run()
        {
            do
            {
                IRunnable task = TakeTask();
                if (task is object)
                {
                    task.Run();
                    UpdateLastExecutionTime();
                }
            } while (!ConfirmShutdown());
        }
    }
}