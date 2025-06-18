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
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class AbstractEventExecutorGroup<TEventExecutor> : IEventExecutorGroup
        where TEventExecutor : class, IEventExecutor
    {
        static readonly TimeSpan DefaultShutdownQuietPeriod = TimeSpan.FromSeconds(2);
        static readonly TimeSpan DefaultShutdownTimeout = TimeSpan.FromSeconds(15);

        public abstract bool IsShutdown { get; }

        public abstract bool IsTerminated { get; }

        public abstract bool IsShuttingDown { get; }

        public abstract Task TerminationCompletion { get; }

        public abstract bool WaitTermination(TimeSpan timeout);

        public abstract IEnumerable<IEventExecutor> Items { get; }

        public abstract IReadOnlyList<TEventExecutor> GetItems();

        IEventExecutor IEventExecutorGroup.GetNext() => GetNext();

        public abstract TEventExecutor GetNext();

        public void Execute(IRunnable task) => GetNext().Execute(task);

        public void Execute(Action<object> action, object state) => GetNext().Execute(action, state);

        public void Execute(Action action) => GetNext().Execute(action);

        public void Execute(Action<object, object> action, object context, object state) => GetNext().Execute(action, context, state);

        public Task<T> SubmitAsync<T>(Func<T> func) => GetNext().SubmitAsync(func);

        public Task<T> SubmitAsync<T>(Func<T> func, CancellationToken cancellationToken) => GetNext().SubmitAsync(func, cancellationToken);

        public Task<T> SubmitAsync<T>(Func<object, T> func, object state) => GetNext().SubmitAsync(func, state);

        public Task<T> SubmitAsync<T>(Func<object, T> func, object state, CancellationToken cancellationToken) => GetNext().SubmitAsync(func, state, cancellationToken);

        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state) => GetNext().SubmitAsync(func, context, state);

        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state, CancellationToken cancellationToken) => GetNext().SubmitAsync(func, context, cancellationToken);

        public IScheduledTask Schedule(IRunnable action, TimeSpan delay) => GetNext().Schedule(action, delay);

        public IScheduledTask Schedule(Action action, TimeSpan delay) => GetNext().Schedule(action, delay);

        public IScheduledTask Schedule(Action<object> action, object state, TimeSpan delay) => GetNext().Schedule(action, state, delay);

        public IScheduledTask Schedule(Action<object, object> action, object context, object state, TimeSpan delay) => GetNext().Schedule(action, context, state, delay);

        public IScheduledTask ScheduleAtFixedRate(IRunnable action, TimeSpan initialDelay, TimeSpan period) => GetNext().ScheduleAtFixedRate(action, initialDelay, period);

        public IScheduledTask ScheduleAtFixedRate(Action action, TimeSpan initialDelay, TimeSpan period) => GetNext().ScheduleAtFixedRate(action, initialDelay, period);

        public IScheduledTask ScheduleAtFixedRate(Action<object> action, object state, TimeSpan initialDelay, TimeSpan period) => GetNext().ScheduleAtFixedRate(action, state, initialDelay, period);

        public IScheduledTask ScheduleAtFixedRate(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan period) => GetNext().ScheduleAtFixedRate(action, context, state, initialDelay, period);

        public IScheduledTask ScheduleWithFixedDelay(IRunnable action, TimeSpan initialDelay, TimeSpan delay) => GetNext().ScheduleWithFixedDelay(action, initialDelay, delay);

        public IScheduledTask ScheduleWithFixedDelay(Action action, TimeSpan initialDelay, TimeSpan delay) => GetNext().ScheduleWithFixedDelay(action, initialDelay, delay);

        public IScheduledTask ScheduleWithFixedDelay(Action<object> action, object state, TimeSpan initialDelay, TimeSpan delay) => GetNext().ScheduleWithFixedDelay(action, state, initialDelay, delay);

        public IScheduledTask ScheduleWithFixedDelay(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan delay) => GetNext().ScheduleWithFixedDelay(action, context, state, initialDelay, delay);

        public Task ScheduleAsync(IRunnable action, TimeSpan delay) => GetNext().ScheduleAsync(action, delay);

        public Task ScheduleAsync(IRunnable action, TimeSpan delay, CancellationToken cancellationToken) => GetNext().ScheduleAsync(action, delay, cancellationToken);

        public Task ScheduleAsync(Action action, TimeSpan delay) => GetNext().ScheduleAsync(action, delay);

        public Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken cancellationToken) => GetNext().ScheduleAsync(action, delay, cancellationToken);

        public Task ScheduleAsync(Action<object> action, object state, TimeSpan delay) => GetNext().ScheduleAsync(action, state, delay);

        public Task ScheduleAsync(Action<object> action, object state, TimeSpan delay, CancellationToken cancellationToken) => GetNext().ScheduleAsync(action, state, delay, cancellationToken);

        public Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay) => GetNext().ScheduleAsync(action, context, state, delay);

        public Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay, CancellationToken cancellationToken) => GetNext().ScheduleAsync(action, context, state, delay);

        public Task ScheduleAtFixedRateAsync(IRunnable action, TimeSpan initialDelay, TimeSpan period) => GetNext().ScheduleAtFixedRateAsync(action, initialDelay, period);

        public Task ScheduleAtFixedRateAsync(IRunnable action, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken) => GetNext().ScheduleAtFixedRateAsync(action, initialDelay, period, cancellationToken);

        public Task ScheduleAtFixedRateAsync(Action action, TimeSpan initialDelay, TimeSpan period) => GetNext().ScheduleAtFixedRateAsync(action, initialDelay, period);

        public Task ScheduleAtFixedRateAsync(Action action, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken) => GetNext().ScheduleAtFixedRateAsync(action, initialDelay, period, cancellationToken);

        public Task ScheduleAtFixedRateAsync(Action<object> action, object state, TimeSpan initialDelay, TimeSpan period) => GetNext().ScheduleAtFixedRateAsync(action, state, initialDelay, period);

        public Task ScheduleAtFixedRateAsync(Action<object> action, object state, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken) => GetNext().ScheduleAtFixedRateAsync(action, state, initialDelay, period, cancellationToken);

        public Task ScheduleAtFixedRateAsync(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan period) => GetNext().ScheduleAtFixedRateAsync(action, context, state, initialDelay, period);

        public Task ScheduleAtFixedRateAsync(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan period, CancellationToken cancellationToken) => GetNext().ScheduleAtFixedRateAsync(action, context, state, initialDelay, period, cancellationToken);

        public Task ScheduleWithFixedDelayAsync(IRunnable action, TimeSpan initialDelay, TimeSpan delay) => GetNext().ScheduleWithFixedDelayAsync(action, initialDelay, delay);

        public Task ScheduleWithFixedDelayAsync(IRunnable action, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken) => GetNext().ScheduleWithFixedDelayAsync(action, initialDelay, delay, cancellationToken);

        public Task ScheduleWithFixedDelayAsync(Action action, TimeSpan initialDelay, TimeSpan delay) => GetNext().ScheduleWithFixedDelayAsync(action, initialDelay, delay);

        public Task ScheduleWithFixedDelayAsync(Action action, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken) => GetNext().ScheduleWithFixedDelayAsync(action, initialDelay, delay, cancellationToken);

        public Task ScheduleWithFixedDelayAsync(Action<object> action, object state, TimeSpan initialDelay, TimeSpan delay) => GetNext().ScheduleWithFixedDelayAsync(action, state, initialDelay, delay);

        public Task ScheduleWithFixedDelayAsync(Action<object> action, object state, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken) => GetNext().ScheduleWithFixedDelayAsync(action, state, initialDelay, delay, cancellationToken);

        public Task ScheduleWithFixedDelayAsync(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan delay) => GetNext().ScheduleWithFixedDelayAsync(action, context, state, initialDelay, delay);

        public Task ScheduleWithFixedDelayAsync(Action<object, object> action, object context, object state, TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken) => GetNext().ScheduleWithFixedDelayAsync(action, context, state, initialDelay, delay, cancellationToken);

        public Task ShutdownGracefullyAsync() => ShutdownGracefullyAsync(DefaultShutdownQuietPeriod, DefaultShutdownTimeout);

        public abstract Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout);
    }
}