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
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class AbstractExecutorService : IExecutorService
    {
        /// <inheritdoc />
        public abstract bool IsShutdown { get; }

        /// <inheritdoc />
        public abstract bool IsTerminated { get; }

        public abstract bool WaitTermination(TimeSpan timeout);

        /// <inheritdoc />
        public Task<T> SubmitAsync<T>(Func<T> func) => SubmitAsync(func, CancellationToken.None);

        /// <inheritdoc />
        public Task<T> SubmitAsync<T>(Func<T> func, CancellationToken cancellationToken)
        {
            var node = new FuncSubmitQueueNode<T>(func, cancellationToken);
            Execute(node);
            return node.Completion;
        }

        /// <inheritdoc />
        public Task<T> SubmitAsync<T>(Func<object, T> func, object state) => SubmitAsync(func, state, CancellationToken.None);

        /// <inheritdoc />
        public Task<T> SubmitAsync<T>(Func<object, T> func, object state, CancellationToken cancellationToken)
        {
            var node = new StateFuncSubmitQueueNode<T>(func, state, cancellationToken);
            Execute(node);
            return node.Completion;
        }

        /// <inheritdoc />
        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state) =>
            SubmitAsync(func, context, state, CancellationToken.None);

        /// <inheritdoc />
        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state, CancellationToken cancellationToken)
        {
            var node = new StateFuncWithContextSubmitQueueNode<T>(func, context, state, cancellationToken);
            Execute(node);
            return node.Completion;
        }

        /// <inheritdoc />
        public abstract void Execute(IRunnable task);

        /// <inheritdoc />
        public void Execute(Action<object> action, object state) => Execute(new StateActionTaskQueueNode(action, state));

        /// <inheritdoc />
        public void Execute(Action<object, object> action, object context, object state) => Execute(new StateActionWithContextTaskQueueNode(action, context, state));

        /// <inheritdoc />
        public void Execute(Action action) => Execute(new ActionTaskQueueNode(action));

        #region Queuing data structures

        sealed class ActionTaskQueueNode : IRunnable
        {
            readonly Action _action;

            public ActionTaskQueueNode(Action action)
            {
                _action = action;
            }

            public void Run() => _action();
        }

        sealed class StateActionTaskQueueNode : IRunnable
        {
            readonly Action<object> _action;
            readonly object _state;

            public StateActionTaskQueueNode(Action<object> action, object state)
            {
                _action = action;
                _state = state;
            }

            public void Run() => _action(_state);
        }

        sealed class StateActionWithContextTaskQueueNode : IRunnable
        {
            readonly Action<object, object> _action;
            readonly object _context;
            readonly object _state;

            public StateActionWithContextTaskQueueNode(Action<object, object> action, object context, object state)
            {
                _action = action;
                _context = context;
                _state = state;
            }

            public void Run() => _action(_context, _state);
        }

        abstract class FuncQueueNodeBase<T> : IRunnable
        {
            readonly TaskCompletionSource<T> _promise;
            readonly CancellationToken _cancellationToken;

            protected FuncQueueNodeBase(TaskCompletionSource<T> promise, CancellationToken cancellationToken)
            {
                _promise = promise;
                _cancellationToken = cancellationToken;
            }

            public Task<T> Completion => _promise.Task;

            public void Run()
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    _ = _promise.TrySetCanceled();
                    return;
                }

                try
                {
                    T result = Call();
                    _ = _promise.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    // todo: handle fatal
                    _ = _promise.TrySetException(ex);
                }
            }

            protected abstract T Call();
        }

        sealed class FuncSubmitQueueNode<T> : FuncQueueNodeBase<T>
        {
            readonly Func<T> _func;

            public FuncSubmitQueueNode(Func<T> func, CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(), cancellationToken)
            {
                _func = func;
            }

            protected override T Call() => _func();
        }

        sealed class StateFuncSubmitQueueNode<T> : FuncQueueNodeBase<T>
        {
            readonly Func<object, T> _func;

            public StateFuncSubmitQueueNode(Func<object, T> func, object state, CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(state), cancellationToken)
            {
                _func = func;
            }

            protected override T Call() => _func(Completion.AsyncState);
        }

        sealed class StateFuncWithContextSubmitQueueNode<T> : FuncQueueNodeBase<T>
        {
            readonly Func<object, object, T> _func;
            readonly object _context;

            public StateFuncWithContextSubmitQueueNode(
                Func<object, object, T> func,
                object context,
                object state,
                CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(state), cancellationToken)
            {
                _func = func;
                _context = context;
            }

            protected override T Call() => _func(_context, Completion.AsyncState);
        }

        #endregion
    }
}