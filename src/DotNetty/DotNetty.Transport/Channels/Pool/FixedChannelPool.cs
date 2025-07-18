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

namespace DotNetty.Transport.Channels.Pool
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Bootstrapping;

    /// <summary>
    /// An <see cref="IChannelPool"/> implementation that takes another <see cref="IChannelPool"/> implementation and
    /// enforces a maximum number of concurrent connections.
    /// </summary>
    public class FixedChannelPool : SimpleChannelPool
    {
        new internal static readonly InvalidOperationException FullException = new InvalidOperationException("Too many outstanding acquire operations");

        private static readonly TimeoutException TimeoutException = new TimeoutException("Acquire operation took longer then configured maximum time");

        internal static readonly InvalidOperationException PoolClosedOnReleaseException = new InvalidOperationException("FixedChannelPool was closed");

        internal static readonly InvalidOperationException PoolClosedOnAcquireException = new InvalidOperationException("FixedChannelPool was closed");

        public enum AcquireTimeoutAction
        {
            None,

            /// <summary>
            /// Creates a new connection when the timeout is detected.
            /// </summary>
            New,

            /// <summary>
            /// Fails the <see cref="DefaultPromise"/> of the acquire call with a <see cref="System.TimeoutException"/>.
            /// </summary>
            Fail
        }

        private readonly IEventExecutor _executor;
        private readonly TimeSpan _acquireTimeout;
        private readonly IRunnable _timeoutTask;

        // There is no need to worry about synchronization as everything that modified the queue or counts is done
        // by the above EventExecutor.
        private readonly IQueue<AcquireTask> _pendingAcquireQueue;

        private readonly int _maxConnections;
        private readonly int _maxPendingAcquires;
        private int v_acquiredChannelCount;
        private int v_pendingAcquireCount;
        private int v_closed;

        /// <summary>
        /// Creates a new <see cref="FixedChannelPool"/> instance using the <see cref="ChannelActiveHealthChecker"/>.
        /// </summary>
        /// <param name="bootstrap">The <see cref="Bootstrap"/> that is used for connections.</param>
        /// <param name="handler">
        /// The <see cref="IChannelPoolHandler"/> that will be notified for the different pool actions.
        /// </param>
        /// <param name="maxConnections">
        /// The number of maximal active connections. Once this is reached, new attempts to acquire an
        /// <see cref="IChannel"/> will be delayed until a connection is returned to the pool again.
        /// </param>
        /// <param name="maxPendingAcquires">
        /// The maximum number of pending acquires. Once this is exceeded, acquire attempts will be failed.
        /// </param>
        public FixedChannelPool(Bootstrap bootstrap, IChannelPoolHandler handler, int maxConnections, int maxPendingAcquires = int.MaxValue)
            : this(bootstrap, handler, ChannelActiveHealthChecker.Instance, AcquireTimeoutAction.None, Timeout.InfiniteTimeSpan, maxConnections, maxPendingAcquires)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FixedChannelPool"/> instance.
        /// </summary>
        /// <param name="bootstrap">The <see cref="Bootstrap"/> that is used for connections.</param>
        /// <param name="handler">
        /// The <see cref="IChannelPoolHandler"/> that will be notified for the different pool actions.
        /// </param>
        /// <param name="healthChecker">
        /// The <see cref="IChannelHealthChecker"/> that will be used to check if a <see cref="IChannel"/> is still
        /// healthy when obtained from the <see cref="IChannelPool"/>.
        /// </param>
        /// <param name="action">
        /// The <see cref="AcquireTimeoutAction"/> to use or <c>null</c> if none should be used. In this case,
        /// <paramref name="acquireTimeout"/> must also be <c>null</c>.
        /// </param>
        /// <param name="acquireTimeout">
        /// A <see cref="TimeSpan"/> after which an pending acquire must complete, or the
        /// <see cref="AcquireTimeoutAction"/> takes place.
        /// </param>
        /// <param name="maxConnections">
        /// The number of maximal active connections. Once this is reached, new attempts to acquire an
        /// <see cref="IChannel"/> will be delayed until a connection is returned to the pool again.
        /// </param>
        /// <param name="maxPendingAcquires">
        /// The maximum number of pending acquires. Once this is exceeded, acquire attempts will be failed.
        /// </param>
        public FixedChannelPool(Bootstrap bootstrap, IChannelPoolHandler handler, IChannelHealthChecker healthChecker, AcquireTimeoutAction action, TimeSpan acquireTimeout, int maxConnections, int maxPendingAcquires)
            : this(bootstrap, handler, healthChecker, action, acquireTimeout, maxConnections, maxPendingAcquires, true)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FixedChannelPool"/> instance.
        /// </summary>
        /// <param name="bootstrap">The <see cref="Bootstrap"/> that is used for connections.</param>
        /// <param name="handler">
        /// The <see cref="IChannelPoolHandler"/> that will be notified for the different pool actions.
        /// </param>
        /// <param name="healthChecker">
        /// The <see cref="IChannelHealthChecker"/> that will be used to check if a <see cref="IChannel"/> is still
        /// healthy when obtained from the <see cref="IChannelPool"/>.
        /// </param>
        /// <param name="action">
        /// The <see cref="AcquireTimeoutAction"/> to use or <c>null</c> if none should be used. In this case,
        /// <paramref name="acquireTimeout"/> must also be <c>null</c>.
        /// </param>
        /// <param name="acquireTimeout">
        /// A <see cref="TimeSpan"/> after which an pending acquire must complete, or the
        /// <see cref="AcquireTimeoutAction"/> takes place.
        /// </param>
        /// <param name="maxConnections">
        /// The number of maximal active connections. Once this is reached, new attempts to acquire an
        /// <see cref="IChannel"/> will be delayed until a connection is returned to the pool again.
        /// </param>
        /// <param name="maxPendingAcquires">
        /// The maximum number of pending acquires. Once this is exceeded, acquire attempts will be failed.
        /// </param>
        /// <param name="releaseHealthCheck">If <c>true</c>, will check channel health before offering it back.</param>
        public FixedChannelPool(Bootstrap bootstrap, IChannelPoolHandler handler, IChannelHealthChecker healthChecker, AcquireTimeoutAction action, TimeSpan acquireTimeout, int maxConnections, int maxPendingAcquires, bool releaseHealthCheck)
            : this(bootstrap, handler, healthChecker, action, acquireTimeout, maxConnections, maxPendingAcquires, releaseHealthCheck, true)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FixedChannelPool"/> instance.
        /// </summary>
        /// <param name="bootstrap">The <see cref="Bootstrap"/> that is used for connections.</param>
        /// <param name="handler">
        /// The <see cref="IChannelPoolHandler"/> that will be notified for the different pool actions.
        /// </param>
        /// <param name="healthChecker">
        /// The <see cref="IChannelHealthChecker"/> that will be used to check if a <see cref="IChannel"/> is still
        /// healthy when obtained from the <see cref="IChannelPool"/>.
        /// </param>
        /// <param name="action">
        /// The <see cref="AcquireTimeoutAction"/> to use or <c>null</c> if none should be used. In this case,
        /// <paramref name="acquireTimeout"/> must also be <c>null</c>.
        /// </param>
        /// <param name="acquireTimeout">
        /// A <see cref="TimeSpan"/> after which an pending acquire must complete, or the
        /// <see cref="AcquireTimeoutAction"/> takes place.
        /// </param>
        /// <param name="maxConnections">
        /// The number of maximal active connections. Once this is reached, new attempts to acquire an
        /// <see cref="IChannel"/> will be delayed until a connection is returned to the pool again.
        /// </param>
        /// <param name="maxPendingAcquires">
        /// The maximum number of pending acquires. Once this is exceeded, acquire attempts will be failed.
        /// </param>
        /// <param name="releaseHealthCheck">If <c>true</c>, will check channel health before offering it back.</param>
        /// <param name="lastRecentUsed">
        /// If <c>true</c>, <see cref="IChannel"/> selection will be LIFO. If <c>false</c>, it will be FIFO.
        /// </param>
        public FixedChannelPool(Bootstrap bootstrap, IChannelPoolHandler handler, IChannelHealthChecker healthChecker, AcquireTimeoutAction action, TimeSpan acquireTimeout, int maxConnections, int maxPendingAcquires, bool releaseHealthCheck, bool lastRecentUsed)
            : base(bootstrap, handler, healthChecker, releaseHealthCheck, lastRecentUsed)
        {
            if ((uint)(maxConnections - 1) > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentException_MaxConnections(maxConnections);
            }
            if ((uint)(maxPendingAcquires - 1) > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentException_MaxPendingAcquires(maxPendingAcquires);
            }

            _acquireTimeout = acquireTimeout;
            if (action == AcquireTimeoutAction.None && acquireTimeout == Timeout.InfiniteTimeSpan)
            {
                _timeoutTask = null;
            }
            else if (action == AcquireTimeoutAction.None && acquireTimeout != Timeout.InfiniteTimeSpan)
            {
                ThrowHelper.ThrowArgumentException_Action();
            }
            else if (action != AcquireTimeoutAction.None && acquireTimeout < TimeSpan.Zero)
            {
                ThrowHelper.ThrowArgumentException_AcquireTimeoutMillis(acquireTimeout);
            }
            else
            {
                switch (action)
                {
                    case AcquireTimeoutAction.Fail:
                        _timeoutTask = new TimeoutTask(this, OnTimeoutFail);
                        break;
                    case AcquireTimeoutAction.New:
                        _timeoutTask = new TimeoutTask(this, OnTimeoutNew);
                        break;
                    default:
                        ThrowHelper.ThrowArgumentException_Action(); break;
                }
            }

            _executor = bootstrap.Group().GetNext();
            _maxConnections = maxConnections;
            _maxPendingAcquires = maxPendingAcquires;

            _pendingAcquireQueue = PlatformDependent.NewMpscQueue<AcquireTask>();
        }

        /// <summary>Returns the number of acquired channels that this pool thinks it has.</summary>
        public int AcquiredChannelCount => Volatile.Read(ref v_acquiredChannelCount);

        private bool IsClosed
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => SharedConstants.False < (uint)Volatile.Read(ref v_closed);
        }

        public override ValueTask<IChannel> AcquireAsync()
        {
            if (_executor.InEventLoop)
            {
                return DoAcquireAsync(null);
            }

            var promise = new ManualResetValueTaskSource<IChannel>();
            _executor.Execute(Acquire0, promise);
            return promise.AwaitValue(CancellationToken.None);
        }

        async void Acquire0(object state)
        {
            var promise = (ManualResetValueTaskSource<IChannel>)state;
            try
            {
                var result = await DoAcquireAsync(promise);
                _ = promise.SetResult(result);
            }
            catch (Exception ex)
            {
                  promise.SetException(ex);
            }
        }

        ValueTask<IChannel> DoAcquireAsync(ManualResetValueTaskSource<IChannel> promise)
        {
            Debug.Assert(_executor.InEventLoop);

            if (IsClosed)
            {
                return ThrowHelper.FromInvalidOperationException_PoolClosedOnAcquireException();
            }

            if (Volatile.Read(ref v_acquiredChannelCount) < _maxConnections)
            {
                Debug.Assert(Volatile.Read(ref v_acquiredChannelCount) >= 0);
                return new AcquireTask(this, promise).AcquireAsync();
            }
            else
            {
                if (Volatile.Read(ref v_pendingAcquireCount) >= _maxPendingAcquires)
                {
                    return ThrowHelper.FromInvalidOperationException_TooManyOutstandingAcquireOperations();
                }
                else
                {
                    promise  = promise?? new ManualResetValueTaskSource<IChannel>();
                    var task = new AcquireTask(this, promise);
                    if (_pendingAcquireQueue.TryEnqueue(task))
                    {
                        Interlocked.Increment(ref v_pendingAcquireCount);

                        if (_timeoutTask is object)
                        {
                            task.TimeoutTask = _executor.Schedule(_timeoutTask, _acquireTimeout);
                        }
                    }
                    else
                    {
                        return ThrowHelper.FromInvalidOperationException_TooManyOutstandingAcquireOperations();
                    }

                    return promise.AwaitValue(CancellationToken.None);
                }
            }
        }

        ValueTask<IChannel> DoAcquireAsync() => base.AcquireAsync();

        public override async Task<bool> ReleaseAsync(IChannel channel)
        {
            if (channel is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.channel); }

            if (_executor.InEventLoop)
            {
                return await DoReleaseAsync(channel);
            }
            else
            {
                var promise = new ManualResetValueTaskSource<bool>();
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                _ = _executor.Schedule((c, p) => Release0(c, p), channel, promise, TimeSpan.Zero);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                return await promise.AwaitValue(CancellationToken.None);
            }
        }

        async void Release0(object channel, object promise)
        {
            var tsc = promise as ManualResetValueTaskSource<bool>;
            try
            {
                var result = await DoReleaseAsync((IChannel)channel);
                _ = tsc.SetResult(result);
            }
            catch (Exception ex)
            {
                tsc.SetException(ex);
            }
        }

        async Task<bool> DoReleaseAsync(IChannel channel)
        {
            Debug.Assert(_executor.InEventLoop);

            try
            {
                await base.ReleaseAsync(channel);
                FailIfClosed(channel);

                DecrementAndRunTaskQueue();
                return true;
            }
            catch (Exception ex)
            {
                FailIfClosed(channel);
                if (!(ex is ArgumentException))
                {
                    DecrementAndRunTaskQueue();
                }

                throw;
            }

            void FailIfClosed(IChannel ch)
            {
                if (IsClosed)
                {
                    _ = ch.CloseAsync();
                    ThrowHelper.ThrowInvalidOperationException_PoolClosedOnReleaseException();
                }
            }
        }


        void DecrementAndRunTaskQueue()
        {
            var acquiredCount = Interlocked.Decrement(ref v_acquiredChannelCount);

            // We should never have a negative value.
            Debug.Assert(acquiredCount >= 0);

            // Run the pending acquire tasks before notify the original promise so if the user would
            // try to acquire again from the ChannelFutureListener and the pendingAcquireCount is >=
            // maxPendingAcquires we may be able to run some pending tasks first and so allow to add
            // more.
            RunTaskQueue();
        }

        void RunTaskQueue()
        {
            while (Volatile.Read(ref v_acquiredChannelCount) < _maxConnections)
            {
                if (!_pendingAcquireQueue.TryDequeue(out AcquireTask task))
                {
                    break;
                }

                // Cancel the timeout if one was scheduled
                task.TimeoutTask?.Cancel();

                Interlocked.Decrement(ref v_pendingAcquireCount);

                task.AcquireAsync();
            }

            // We should never have a negative value.
            Debug.Assert(Volatile.Read(ref v_pendingAcquireCount) >= 0);
            Debug.Assert(Volatile.Read(ref v_acquiredChannelCount) >= 0);
        }

        public override void Close()
        {
            try
            {
                CloseAsync().GetAwaiter().GetResult();
            }
            catch(Exception exc)
            {
                ExceptionDispatchInfo.Capture(exc).Throw();
            }
        }

        public override Task CloseAsync()
        {
            if (_executor.InEventLoop)
            {
                return InternalCloseAsync();
            }
            else
            {
                var closeComplete = _executor.NewPromise();
                _executor.Execute((c, p) => ((FixedChannelPool)c).InternalCloseAsync().LinkOutcome((IPromise)p), this, closeComplete);
                return closeComplete.Task;
            }
        }

        private Task InternalCloseAsync()
        {
            if (SharedConstants.False < (uint)Interlocked.Exchange(ref v_closed, SharedConstants.True))
            {
                return TaskUtil.Completed;
            }

            while (_pendingAcquireQueue.TryDequeue(out AcquireTask task))
            {
                task.TimeoutTask?.Cancel();
                 task.Promise.SetException(ThrowHelper.GetClosedChannelException());
            }

            Interlocked.Exchange(ref v_acquiredChannelCount, 0);
            Interlocked.Exchange(ref v_pendingAcquireCount, 0);

            // Ensure we dispatch this on another Thread as close0 will be called from the EventExecutor and we need
            // to ensure we will not block in a EventExecutor.
            Task.Run(() => base.Close());
            return TaskUtil.Completed;
        }

        void OnTimeoutNew(AcquireTask task) => task.AcquireAsync();

        void OnTimeoutFail(AcquireTask task) => task.Promise.SetException(TimeoutException);

        sealed class TimeoutTask : IRunnable
        {
            private readonly FixedChannelPool _pool;
            private readonly Action<AcquireTask> _onTimeout;

            public TimeoutTask(FixedChannelPool pool, Action<AcquireTask> onTimeout)
            {
                _pool = pool;
                _onTimeout = onTimeout;
            }

            public void Run()
            {
                Debug.Assert(_pool._executor.InEventLoop);
                while (true)
                {
                    if (!_pool._pendingAcquireQueue.TryPeek(out AcquireTask task) || PreciseTimeSpan.FromTicks(Stopwatch.GetTimestamp()) < task.ExpireTime)
                    {
                        break;
                    }

                    _ = _pool._pendingAcquireQueue.TryDequeue(out _);

                    _ = Interlocked.Decrement(ref _pool.v_pendingAcquireCount);
                    _onTimeout(task);
                }
            }
        }

        sealed class AcquireTask
        {
            private readonly FixedChannelPool _pool;

            public readonly ManualResetValueTaskSource<IChannel> Promise;
            public readonly PreciseTimeSpan ExpireTime;
            public IScheduledTask TimeoutTask;

            private bool _acquired;

            public AcquireTask(FixedChannelPool pool, ManualResetValueTaskSource<IChannel> promise)
            {
                _pool = pool;
                Promise = promise;
                ExpireTime = PreciseTimeSpan.FromTicks(Stopwatch.GetTimestamp()) + pool._acquireTimeout;
            }

            // Increment the acquire count and delegate to super to actually acquire a Channel which will
            // create a new connection.
            public ValueTask<IChannel> AcquireAsync()
            {
                var promise = Promise;

                if (_pool.IsClosed)
                {
                    if (promise is object)
                    {
                        promise.SetException(PoolClosedOnAcquireException);
                        return promise.AwaitValue(CancellationToken.None);
                    }
                    else
                    {
                        return ThrowHelper.FromInvalidOperationException_PoolClosedOnAcquireException();
                    }
                }

                Acquired();

                ValueTask<IChannel> future;
                try
                {
                    future = _pool.DoAcquireAsync();
                    if (future.IsCompletedSuccessfully)
                    {
                        //pool never closed here
                        var channel = future.Result;
                        if (promise is object)
                        {
                            _ = promise.SetResult(channel);
                            return  promise.AwaitValue(CancellationToken.None);
                        }
                        else
                        {
                            return future;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //pool never closed here
                    ResumeQueue();

                    if (promise is object)
                    {
                         promise.SetException(ex);
                        return promise.AwaitValue(CancellationToken.None);
                    }
                    else
                    {
                        throw;
                    }
                }

                //at this point 'future' is a real Task
                promise  = promise ?? new ManualResetValueTaskSource<IChannel>();
                future
                    .AsTask()
                    .ContinueWith(
                    t =>
                    {
                        Debug.Assert(_pool._executor.InEventLoop);
                        if (_pool.IsClosed)
                        {
                            if (t.IsSuccess())
                            {
                                // Since the pool is closed, we have no choice but to close the channel
                                _ = t.Result.CloseAsync();
                            }
                              promise.SetException(PoolClosedOnAcquireException);
                        }
                        else if (t.IsSuccess())
                        {
                            _ = promise.SetResult(future.Result);
                        }
                        else
                        {
                            ResumeQueue();
                            promise.SetException(t.Exception);
                        }
                    });

                return promise.AwaitValue(CancellationToken.None);

                void ResumeQueue()
                {
                    if (_acquired)
                    {
                        _pool.DecrementAndRunTaskQueue();
                    }
                    else
                    {
                        _pool.RunTaskQueue();
                    }
                }
            }

            void Acquired()
            {
                if (_acquired)
                {
                    return;
                }

                _ = Interlocked.Increment(ref _pool.v_acquiredChannelCount);
                _acquired = true;
            }
        }
    }
}
