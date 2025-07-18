using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetty.Common.Concurrency
{
    internal class AloneExecutorTaskScheduler : TaskScheduler
    {
        private readonly IEventExecutor _executor;
        private bool _started;
        private readonly BlockingCollection<Task> _tasks = new();
        private readonly Thread[] _threads;
        protected override IEnumerable<Task>? GetScheduledTasks() => _tasks;
      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void QueueTask(Task task)
        {
            if (_started)
            {
                _tasks.Add(task);
            }
            else
            {
                // hack: enables this executor to be seen as default on Executor's worker thread.
                // This is a special case for SingleThreadEventExecutor.Loop initiated task.
                _started = true;
                _ = TryExecuteTask(task);
            }
        }
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
        public AloneExecutorTaskScheduler(IEventExecutor executor,int threadCount)
        {
            _executor = executor;
            _threads = new Thread[threadCount];
            for (int index = 0; index < threadCount; index++)
            {
                _threads[index] = new Thread(_ =>
                {
                    while (true)
                    {
                        _executor.Execute(new TaskQueueNode(this, _tasks.Take())); 
                    }
                });
            }
            Array.ForEach(_threads, it => it.Start());
        }

        sealed class TaskQueueNode : IRunnable
        {
            readonly AloneExecutorTaskScheduler _scheduler;
            readonly Task _task;

            public TaskQueueNode(AloneExecutorTaskScheduler scheduler, Task task)
            {
                _scheduler = scheduler;
                _task = task;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Run() => _scheduler.TryExecuteTask(_task);
        }
    }
}