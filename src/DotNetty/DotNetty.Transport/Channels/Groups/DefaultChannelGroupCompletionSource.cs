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
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;

    public class DefaultChannelGroupCompletionSource : ManualResetValueTaskSource<int>, IChannelGroupTaskCompletionSource
    {
        private readonly Dictionary<IChannel, Task> _futures;
          private int _lock;
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
            var failed = new List<KeyValuePair<IChannel, Exception>>();
            _futures = new Dictionary<IChannel, Task>(ChannelComparer.Default);
#pragma warning disable IDE0039 // 使用本地函数
            Action<Task,object> continueAction =  (Task x, object obj) =>
#pragma warning restore IDE0039 // 使用本地函数
            {
                var channel=obj as IChannel;
                bool success = x.IsSuccess(); 
                if (GetCallSetDone(success))
                {
                    if (_failureCount > 0)
                    {
                        if (x.IsFailure())
                        {
                            if (x.Exception is object)
                            {
                                failed.Add(new KeyValuePair<IChannel, Exception>(channel, x.Exception.InnerException));
                            }
                        }
                        SetException(new ChannelGroupException(failed));
                    }
                    else
                    {
                        _ = SetResult(0);
                    }
                }
                //the task does not need to be call, so  Dispose it
                x.Dispose();
            };
            foreach (KeyValuePair<IChannel, Task> pair in futures)
            {
                _futures.Add(pair.Key, pair.Value);
                _ = pair.Value.ContinueWith(continueAction, pair.Key);
            }

            // Done on arrival?
            if (0u >= (uint)futures.Count)
            {
                _ = SetResult(0);
            }
        }

        public IChannelGroup Group { get; }

        public bool GetCallSetDone(bool success)
        {
            bool callSetDone = false;
            while (true)
            {
                if (Interlocked.Exchange(ref _lock, 1) != 0)
                {
                    default(SpinWait).SpinOnce();
                    continue;
                }

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
                Interlocked.Exchange(ref _lock, 0);

                return callSetDone;
            }
        }

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
            var task = AwaitVoid(System.Threading.CancellationToken.None);
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

        public ChannelGroupException Cause => (ChannelGroupException)GetException(Version).SourceException.InnerException;

        public Task Current => _futures.Values.GetEnumerator().Current;

        public void Dispose() => _futures.Values.GetEnumerator().Dispose();

        object IEnumerator.Current => _futures.Values.GetEnumerator().Current;

        public bool MoveNext() => _futures.Values.GetEnumerator().MoveNext();

        public void Reset() => ((IEnumerator)_futures.Values.GetEnumerator()).Reset();
    }
}