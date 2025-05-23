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

namespace DotNetty.Transport.Channels.Groups
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;

    public class DefaultChannelGroupCompletionSource : TaskCompletionSource<int>, IChannelGroupTaskCompletionSource
    {
        private readonly Dictionary<IChannel, Task> _futures;
        private int _failureCount;
        private int _successCount;

        public DefaultChannelGroupCompletionSource(IChannelGroup group, Dictionary<IChannel, Task> futures /*, IEventExecutor executor*/)
            : this(group, futures /*,executor*/, null)
        {
        }

        public DefaultChannelGroupCompletionSource(IChannelGroup group, Dictionary<IChannel, Task> futures /*, IEventExecutor executor*/, object state)
            : base(state)
        {
            if (group is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.group); }
            if (futures is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.futures); }

            Group = group;
            _futures = new Dictionary<IChannel, Task>(ChannelComparer.Default);
#pragma warning disable IDE0039 // 使用本地函数
            Action<Task> continueAction = (Task x) =>
#pragma warning restore IDE0039 // 使用本地函数
            {
                bool success = x.IsSuccess();
                bool callSetDone;
                lock (this)
                {
                    if (success)
                    {
                        _successCount++;
                    }
                    else
                    {
                        _failureCount++;
                    }

                    callSetDone = _successCount + _failureCount == _futures.Count;
                    Debug.Assert(_successCount + _failureCount <= _futures.Count);
                }

                if (callSetDone)
                {
                    if (_failureCount > 0)
                    {
                        var failed = new List<KeyValuePair<IChannel, Exception>>();
                        foreach (KeyValuePair<IChannel, Task> ft in _futures)
                        {
                            IChannel c = ft.Key;
                            Task f = ft.Value;
                            if (f.IsFailure())
                            {
                                if (f.Exception is object)
                                {
                                    failed.Add(new KeyValuePair<IChannel, Exception>(c, f.Exception.InnerException));
                                }
                            }
                        }
                        _ = TrySetException(new ChannelGroupException(failed));
                    }
                    else
                    {
                        _ = TrySetResult(0);
                    }
                }
            };
            foreach (KeyValuePair<IChannel, Task> pair in futures)
            {
                _futures.Add(pair.Key, pair.Value);
                _ = pair.Value.ContinueWith(continueAction);
            }

            // Done on arrival?
            if (0u >= (uint)futures.Count)
            {
                _ = TrySetResult(0);
            }
        }

        public IChannelGroup Group { get; }

        public Task Find(IChannel channel) => _futures[channel];

        public bool IsPartialSucess()
        {
            lock (this)
            {
                return _successCount != 0 && _successCount != _futures.Count;
            }
        }

        public bool IsSucess()
        {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            return Task.IsCompletedSuccessfully;
#else
            var task = Task;
            return task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
#endif
        }

        public bool IsPartialFailure()
        {
            lock (this)
            {
                return _failureCount != 0 && _failureCount != _futures.Count;
            }
        }

        public ChannelGroupException Cause => (ChannelGroupException)Task.Exception.InnerException;

        public Task Current => _futures.Values.GetEnumerator().Current;

        public void Dispose() => _futures.Values.GetEnumerator().Dispose();

        object IEnumerator.Current => _futures.Values.GetEnumerator().Current;

        public bool MoveNext() => _futures.Values.GetEnumerator().MoveNext();

        public void Reset() => ((IEnumerator)_futures.Values.GetEnumerator()).Reset();
    }
}