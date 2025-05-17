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
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// A promise combiner monitors the outcome of a number of discrete futures, then notifies a final, aggregate promise
    /// when all of the combined futures are finished. The aggregate promise will succeed if and only if all of the combined
    /// futures succeed. If any of the combined futures fail, the aggregate promise will fail. The cause failure for the
    /// aggregate promise will be the failure for one of the failed combined futures; if more than one of the combined
    /// futures fails, exactly which cause of failure will be assigned to the aggregate promise is undefined.
    /// 
    /// <para>Callers may populate a promise combiner with any number of futures to be combined via the
    /// <see cref="PromiseCombiner.Add(Task)"/> and <see cref="PromiseCombiner.AddAll(Task[])"/> methods. When all futures to be
    /// combined have been added, callers must provide an aggregate promise to be notified when all combined promises have
    /// finished via the <see cref="PromiseCombiner.Finish(IPromise)"/> method.</para>
    /// </summary>
    public sealed class PromiseCombiner
    {
        private readonly IEventExecutor _executor;
        private int _expectedCount;
        private int _doneCount;
        private IPromise _aggregatePromise;
        private Exception _cause;

        public PromiseCombiner(IEventExecutor executor)
        {
            if (executor is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.executor); }
            _executor = executor;
        }

        public void Add(IPromise promise)
        {
            Add(promise.Task);
        }

        public void Add(Task future)
        {
            CheckAddAllowed();
            CheckInEventLoop();
            ++_expectedCount;

            if (future.IsCompleted)
            {
                OperationComplete(future);
            }
            else
            {
                _ = future.ContinueWith(OperationCompleteAction, this, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        public void AddAll(params IPromise[] promises)
        {
            if (promises is null || 0u >= (uint)promises.Length) { return; }

            for (int i = 0; i < promises.Length; i++)
            {
                Add(promises[i].Task);
            }
        }

        public void AddAll(params Task[] futures)
        {
            if (futures is null || 0u >= (uint)futures.Length) { return; }

            for (int i = 0; i < futures.Length; i++)
            {
                Add(futures[i]);
            }
        }

        public void Finish(IPromise aggregatePromise)
        {
            if (aggregatePromise is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.aggregatePromise);
            }
            CheckInEventLoop();
            if (_aggregatePromise is object)
            {
                ThrowHelper.ThrowInvalidOperationException_AlreadyFinished();
            }
            _aggregatePromise = aggregatePromise;
            if (0u >= (uint)(_doneCount - _expectedCount))
            {
                _ = TryPromise();
            }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void CheckInEventLoop()
        {
            if (!_executor.InEventLoop)
            {
                ThrowHelper.ThrowInvalidOperationException_MustBeCalledFromEventexecutorThread();
            }
        }

        private bool TryPromise()
        {
            return (_cause is null) ? _aggregatePromise.TryComplete() : _aggregatePromise.TrySetException(_cause);
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private void CheckAddAllowed()
        {
            if (_aggregatePromise is object)
            {
                ThrowHelper.ThrowInvalidOperationException_AddingPromisesIsNotAllowedAfterFinishedAdding();
            }
        }

        private static readonly Action<Task, object> OperationCompleteAction = OperationComplete;
        private static void OperationComplete(Task future, object state)
        {
            var self = (PromiseCombiner)state;
            var executor = self._executor;
            if (executor.InEventLoop)
            {
                self.OperationComplete(future);
            }
            else
            {
                executor.Execute(new OperationCompleteTask(self, future));
            }
        }

        private void OperationComplete(Task future)
        {
            Debug.Assert(_executor.InEventLoop);
            ++_doneCount;
            if (future.IsFailure() && _cause is null)
            {
                _cause = future.Exception.InnerException;
            }
            if (0u >= (uint)(_doneCount - _expectedCount) && _aggregatePromise is object)
            {
                _ = TryPromise();
            }
        }

        sealed class OperationCompleteTask : IRunnable
        {
            private readonly PromiseCombiner _owner;
            private readonly Task _future;

            public OperationCompleteTask(PromiseCombiner owner, Task future)
            {
                _owner = owner;
                _future = future;
            }

            public void Run()
            {
                _owner.OperationComplete(_future);
            }
        }
    }
}
