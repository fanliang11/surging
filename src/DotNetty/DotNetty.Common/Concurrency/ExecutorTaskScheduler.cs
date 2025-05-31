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
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public sealed class ExecutorTaskScheduler : TaskScheduler
    {
        private readonly IEventExecutor _executor;
        private bool _started;

        public ExecutorTaskScheduler(IEventExecutor executor)
        {
            _executor = executor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void QueueTask(Task task)
        {
            if (_started)
            {
                _executor.Execute(new TaskQueueNode(this, task));
            }
            else
            {
                // hack: enables this executor to be seen as default on Executor's worker thread.
                // This is a special case for SingleThreadEventExecutor.Loop initiated task.
                _started = true;
                _ = TryExecuteTask(task);
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued || !_executor.InEventLoop)
            {
                return false;
            }

            return TryExecuteTask(task);
        }

        protected override IEnumerable<Task> GetScheduledTasks() => null;

        protected override bool TryDequeue(Task task) => false;

        sealed class TaskQueueNode : IRunnable
        {
            readonly ExecutorTaskScheduler _scheduler;
            readonly Task _task;

            public TaskQueueNode(ExecutorTaskScheduler scheduler, Task task)
            {
                _scheduler = scheduler;
                _task = task;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Run() => _scheduler.TryExecuteTask(_task);
        }
    }
}