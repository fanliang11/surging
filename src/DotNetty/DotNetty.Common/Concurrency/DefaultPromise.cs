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

using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetty.Common.Concurrency
{
    public class DefaultPromise : IPromise
    {
#if NET
        private readonly TaskCompletionSource _tcs;
#else
        private readonly TaskCompletionSource<int> _tcs;
#endif

        private int v_uncancellable = SharedConstants.False;

        public DefaultPromise()
        {
#if NET
            _tcs = new TaskCompletionSource();
#else
            _tcs = new TaskCompletionSource<int>();
#endif
        }

        public DefaultPromise(object state)
        {
#if NET
            _tcs = new TaskCompletionSource(state);
#else
            _tcs = new TaskCompletionSource<int>(state);
#endif
        }

        public Task Task
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => _tcs.Task;
        }

        public bool IsVoid => false;

        public bool IsSuccess => Task.IsSuccess();

        public bool IsCompleted => Task.IsCompleted;

        public bool IsFaulted => Task.IsFaulted;

        public bool IsCanceled => Task.IsCanceled;

        public ValueTask ValueTask =>new ValueTask(_tcs.Task);

        public virtual bool TryComplete()
        {
#if NET
            return _tcs.TrySetResult();
#else
            return _tcs.TrySetResult(0);
#endif
        }

        public virtual void Complete()
        {
#if NET
            _tcs.SetResult();
#else
            _tcs.SetResult(0);
#endif
        }
        public virtual void SetCanceled()
        {
            if (SharedConstants.False < (uint)Volatile.Read(ref v_uncancellable)) { return; }
            _tcs.SetCanceled();
        }

        public virtual void SetException(Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                SetException(aggregateException.InnerExceptions);
                return;
            }
            _tcs.SetException(exception);
        }

        public virtual void SetException(IEnumerable<Exception> exceptions)
        {
            _tcs.SetException(exceptions);
        }

        public virtual bool TrySetCanceled()
        {
            if (SharedConstants.False < (uint)Volatile.Read(ref v_uncancellable)) { return false; }
            return _tcs.TrySetCanceled();
        }

        public virtual bool TrySetException(Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                return TrySetException(aggregateException.InnerExceptions);
            }
            return _tcs.TrySetException(exception);
        }

        public virtual bool TrySetException(IEnumerable<Exception> exceptions)
        {
            return _tcs.TrySetException(exceptions);
        }

        public bool SetUncancellable()
        {
            if (SharedConstants.False >= (uint)Interlocked.CompareExchange(ref v_uncancellable, SharedConstants.True, SharedConstants.False))
            {
                return true;
            }
            return !IsCompleted;
        }

        public override string ToString() => "TaskCompletionSource[status: " + Task.Status.ToString() + "]";

        public IPromise Unvoid() => this;

        public Exception Execption()
        {
           return Task.Exception.InnerException;
        }

        public void Dispose()
        {
            Task.Dispose();
        }
    }
}