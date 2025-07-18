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
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Internal.Logging;
    using Thread = XThread;

    /// <summary>
    ///     Abstract base class for <see cref="IEventExecutor" /> implementations
    /// </summary>
    public abstract class AbstractEventExecutor : AbstractExecutorService, IEventExecutor
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<AbstractEventExecutor>();

        static readonly TimeSpan DefaultShutdownQuietPeriod = TimeSpan.FromSeconds(2);
        static readonly TimeSpan DefaultShutdownTimeout = TimeSpan.FromSeconds(15);

        /// <summary>Creates an instance of <see cref="AbstractEventExecutor"/>.</summary>
        protected AbstractEventExecutor()
            : this(null)
        {
        }

        /// <summary>Creates an instance of <see cref="AbstractEventExecutor"/>.</summary>
        protected AbstractEventExecutor(IEventExecutorGroup parent)
        {
            Parent = parent;
        }

        /// <inheritdoc />
        public abstract bool IsShuttingDown { get; }

        /// <inheritdoc cref="IEventExecutorGroup.TerminationCompletion"/>
        public abstract Task TerminationCompletion { get; }

        /// <inheritdoc cref="IEventExecutorGroup.GetNext()"/>
        public IEventExecutor GetNext() => this;

        /// <inheritdoc />
        public IEventExecutorGroup Parent { get; }

        /// <inheritdoc />
        public bool InEventLoop => IsInEventLoop(Thread.CurrentThread);

        /// <inheritdoc />
        public IEnumerable<IEventExecutor> Items => GetItems();

        protected abstract IEnumerable<IEventExecutor> GetItems();

        /// <inheritdoc />
        public abstract bool IsInEventLoop(Thread thread);

        /// <inheritdoc />
        public virtual IScheduledTask Schedule(IRunnable action, TimeSpan delay)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual IScheduledTask Schedule(Action action, TimeSpan delay)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual IScheduledTask Schedule(Action<object> action, object state, TimeSpan delay)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual IScheduledTask Schedule(Action<object, object> action, object context, object state, TimeSpan delay)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual IScheduledTask ScheduleAtFixedRate(IRunnable action, TimeSpan initialDelay, TimeSpan period)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual IScheduledTask ScheduleAtFixedRate(Action action, TimeSpan initialDelay, TimeSpan period)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual IScheduledTask ScheduleAtFixedRate(Action<object> action, object state, TimeSpan initialDelay, TimeSpan period)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual IScheduledTask ScheduleAtFixedRate(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan period)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual IScheduledTask ScheduleWithFixedDelay(IRunnable action, TimeSpan initialDelay, TimeSpan delay)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual IScheduledTask ScheduleWithFixedDelay(Action action, TimeSpan initialDelay, TimeSpan delay)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual IScheduledTask ScheduleWithFixedDelay(Action<object> action, object state, TimeSpan initialDelay, TimeSpan delay)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual IScheduledTask ScheduleWithFixedDelay(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan delay)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual Task ScheduleAsync(IRunnable action, TimeSpan delay) =>
            ScheduleAsync(action, delay, CancellationToken.None);

        /// <inheritdoc />
        public virtual Task ScheduleAsync(IRunnable action, TimeSpan delay, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual Task ScheduleAsync(Action action, TimeSpan delay) =>
            ScheduleAsync(action, delay, CancellationToken.None);

        /// <inheritdoc />
        public virtual Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual Task ScheduleAsync(Action<object> action, object state, TimeSpan delay) =>
            ScheduleAsync(action, state, delay, CancellationToken.None);

        /// <inheritdoc />
        public virtual Task ScheduleAsync(Action<object> action, object state, TimeSpan delay, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public virtual Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay) =>
            ScheduleAsync(action, context, state, delay, CancellationToken.None);

        /// <inheritdoc />
        public virtual Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual Task ScheduleAtFixedRateAsync(IRunnable action, TimeSpan initialDelay, TimeSpan period) =>
            ScheduleAtFixedRateAsync(action, initialDelay, period, CancellationToken.None);

        public virtual Task ScheduleAtFixedRateAsync(IRunnable action, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual Task ScheduleAtFixedRateAsync(Action action, TimeSpan initialDelay, TimeSpan period) =>
            ScheduleAtFixedRateAsync(action, initialDelay, period, CancellationToken.None);

        public virtual Task ScheduleAtFixedRateAsync(Action action, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual Task ScheduleAtFixedRateAsync(Action<object> action, object state, TimeSpan initialDelay, TimeSpan period) =>
            ScheduleAtFixedRateAsync(action, state, initialDelay, period, CancellationToken.None);

        public virtual Task ScheduleAtFixedRateAsync(Action<object> action, object state, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual Task ScheduleAtFixedRateAsync(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan period) =>
            ScheduleAtFixedRateAsync(action, context, state, initialDelay, period, CancellationToken.None);

        public virtual Task ScheduleAtFixedRateAsync(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual Task ScheduleWithFixedDelayAsync(IRunnable action, TimeSpan initialDelay, TimeSpan delay) =>
            ScheduleWithFixedDelayAsync(action, initialDelay, delay, CancellationToken.None);

        public virtual Task ScheduleWithFixedDelayAsync(IRunnable action, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual Task ScheduleWithFixedDelayAsync(Action action, TimeSpan initialDelay, TimeSpan delay) =>
            ScheduleWithFixedDelayAsync(action, initialDelay, delay, CancellationToken.None);

        public virtual Task ScheduleWithFixedDelayAsync(Action action, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual Task ScheduleWithFixedDelayAsync(Action<object> action, object state, TimeSpan initialDelay, TimeSpan delay) =>
            ScheduleWithFixedDelayAsync(action, state, initialDelay, delay, CancellationToken.None);

        public virtual Task ScheduleWithFixedDelayAsync(Action<object> action, object state, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual Task ScheduleWithFixedDelayAsync(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan delay) =>
            ScheduleWithFixedDelayAsync(action, context, state, initialDelay, delay, CancellationToken.None);

        public virtual Task ScheduleWithFixedDelayAsync(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        /// <inheritdoc />
        public Task ShutdownGracefullyAsync() => ShutdownGracefullyAsync(DefaultShutdownQuietPeriod, DefaultShutdownTimeout);

        /// <inheritdoc />
        public abstract Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout);

        public IPromise NewPromise() => new DefaultValueTaskPromise();

        public IPromise NewPromise(object state) => new DefaultValueTaskPromise(state);

        /// <inheritdoc />
        protected void SetCurrentExecutor(IEventExecutor executor) => ExecutionEnvironment.SetCurrentExecutor(executor);

        /// <summary>
        /// Try to execute the given <see cref="IRunnable"/> and just log if it throws a <see cref="Exception"/>.
        /// </summary>
        /// <param name="task"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void SafeExecute(IRunnable task)
        {
            try
            {
                task.Run();
            }
            catch (Exception ex)
            {
                Logger.ATaskRaisedAnException(task, ex);
            }
        }

        /// <summary>
        /// Like <see cref="AbstractExecutorService.Execute(IRunnable)"/> but does not guarantee the task will be run until either
        /// a non-lazy task is executed or the executor is shut down.
        /// 
        /// <para>This is equivalent to submitting a <see cref="ILazyRunnable"/> to
        /// <see cref="AbstractExecutorService.Execute(IRunnable)"/> but for an arbitrary <see cref="IRunnable"/>.</para>
        /// </summary>
        /// <remarks>The default implementation just delegates to <see cref="AbstractExecutorService.Execute(IRunnable)"/>.</remarks>
        /// <param name="task"></param>
        public virtual void LazyExecute(IRunnable task)
        {
            Execute(task);
        }
    }
}