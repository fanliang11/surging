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
    using DotNetty.Common.Utilities;
    using DotNetty.Common.Concurrency;
    using System.Threading;

    public sealed class VoidChannelPromise : IPromise
    {
        static readonly Exception Error;

        private readonly IChannel _channel;
        // Will be null if we should not propagate exceptions through the pipeline on failure case.
        private readonly bool _fireException;
        private readonly Lazy<Task> _task;

        static VoidChannelPromise()
        {
            Error = new InvalidOperationException("No operations are allowed on void promise");
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="channel">channel the <see cref="IChannel"/> associated with this future</param>
        /// <param name="fireException"></param>
        public VoidChannelPromise(IChannel channel, bool fireException)
        {
            if (channel is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.channel); }
            _channel = channel;
            _fireException = fireException;
            _task = new Lazy<Task>(
#if NET
                static
#endif
                () => TaskUtil.FromException(Error), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public Task Task => _task.Value;

        public bool IsVoid => true;

        public bool IsSuccess => false;

        public bool IsCompleted => false;

        public bool IsFaulted => false;

        public bool IsCanceled => false;

        public bool TryComplete() => false;

        public void Complete() { }

        public bool TrySetException(Exception exception)
        {
            if (!_fireException) { return false; }
            if (exception is AggregateException aggregateException)
            {
                return TrySetException(aggregateException.InnerExceptions);
            }
            FireException0(exception);
            return false;
        }

        public bool TrySetException(IEnumerable<Exception> exceptions)
        {
            if (!_fireException) { return false; }
            foreach (var item in exceptions)
            {
                FireException0(item);
            }
            return false;
        }

        public void SetException(Exception exception)
        {
            if (!_fireException) { return; }
            if (exception is AggregateException aggregateException)
            {
                SetException(aggregateException.InnerExceptions);
                return;
            }
            FireException0(exception);
        }

        public void SetException(IEnumerable<Exception> exceptions)
        {
            if (!_fireException) { return; }
            foreach (var item in exceptions)
            {
                FireException0(item);
            }
        }

        public bool TrySetCanceled() => false;

        public void SetCanceled() { }

        public bool SetUncancellable() => true;

        public IPromise Unvoid()
        {
            var promise = new DefaultPromise();
            if (_fireException)
            {
                _ = promise.Task.ContinueWith(FireExceptionOnFailureAction, _channel, TaskContinuationOptions.ExecuteSynchronously);
            }
            return promise;
        }

        private static readonly Action<Task, object> FireExceptionOnFailureAction = (t, s) => FireExceptionOnFailure(t, s);
        private static void FireExceptionOnFailure(Task t, object s)
        {
            var ch = (IChannel)s;
            if (t.IsFaulted)// && ch.Registered)
            {
                _ = ch.Pipeline.FireExceptionCaught(t.Exception.InnerException);
            }
        }

        private void FireException0(Exception cause)
        {
            // Only fire the exception if the channel is open and registered
            // if not the pipeline is not setup and so it would hit the tail
            // of the pipeline.
            // See https://github.com/netty/netty/issues/1517
            if (_channel.IsRegistered)
            {
                _ = _channel.Pipeline.FireExceptionCaught(cause);
            }
        }

        public override string ToString() => "VoidPromise";
    }
}