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
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;

    public interface IRejectedExecutionHandler
    {
        /// <summary>
        /// Called when someone tried to add a task to <see cref="SingleThreadEventExecutor"/> but this failed due capacity
        /// restrictions.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="executor"></param>
        void Rejected(IRunnable task, SingleThreadEventExecutor executor);
    }

    sealed class DefaultRejectedExecutionHandler : IRejectedExecutionHandler
    {
        public static readonly DefaultRejectedExecutionHandler Instance = new DefaultRejectedExecutionHandler();

        private DefaultRejectedExecutionHandler() { }

        public void Rejected(IRunnable task, SingleThreadEventExecutor executor)
        {
            ThrowHelper.ThrowRejectedExecutionException();
        }
    }

    sealed class FixedBackoffRejectedExecutionHandler : IRejectedExecutionHandler
    {
        private readonly int _retries;
        private readonly TimeSpan _delay;

        public FixedBackoffRejectedExecutionHandler(int retries, TimeSpan delay)
        {
            if ((uint)(retries - 1) > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_Positive(retries, ExceptionArgument.retries); }
            if (delay <= TimeSpan.Zero) { ThrowHelper.ArgumentOutOfRangeException_Positive(delay, ExceptionArgument.delay); }

            _retries = retries;
            _delay = delay;
        }

        public void Rejected(IRunnable task, SingleThreadEventExecutor executor)
        {
            if (!executor.InEventLoop)
            {
                for (int i = 0; i < _retries; i++)
                {
                    // Try to wake up the executor so it will empty its task queue.
                    executor.WakeUp(false);

                    Thread.Sleep(_delay);
                    if (executor.OfferTask(task)) { return; }
                }
            }
            // Either we tried to add the task from within the EventLoop or we was not able to add it even with backoff.
            ThrowHelper.ThrowRejectedExecutionException();
        }
    }

    sealed class ExponentialBackoffRejectedExecutionHandler : IRejectedExecutionHandler
    {
        private readonly SafeRandom _random;
        private readonly int _retries;
        private readonly TimeSpan _minDelay;
        private readonly TimeSpan _maxDelay;
        private readonly TimeSpan _step;

        public ExponentialBackoffRejectedExecutionHandler(int retries, TimeSpan minDelay, TimeSpan maxDelay, TimeSpan step)
        {
            if ((uint)(retries - 1) > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_Positive(retries, ExceptionArgument.retries); }
            if (minDelay <= TimeSpan.Zero) { ThrowHelper.ArgumentOutOfRangeException_Positive(minDelay, ExceptionArgument.minDelay); }
            if (maxDelay <= TimeSpan.Zero) { ThrowHelper.ArgumentOutOfRangeException_Positive(maxDelay, ExceptionArgument.maxDelay); }
            if (step <= TimeSpan.Zero) { ThrowHelper.ArgumentOutOfRangeException_Positive(step, ExceptionArgument.step); }
            if (minDelay >= maxDelay) { ThrowHelper.ArgumentOutOfRangeException_Invalid_minValue(minDelay); }

            _retries = retries;
            _minDelay = minDelay;
            _maxDelay = maxDelay;
            _step = step;
            _random = new SafeRandom();
        }

        public void Rejected(IRunnable task, SingleThreadEventExecutor executor)
        {
            if (!executor.InEventLoop)
            {
                for (int i = 0; i < _retries; i++)
                {
                    // Try to wake up the executor so it will empty its task queue.
                    executor.WakeUp(false);

                    Thread.Sleep(Next(i));
                    if (executor.OfferTask(task)) { return; }
                }
            }
            // Either we tried to add the task from within the EventLoop or we was not able to add it even with backoff.
            ThrowHelper.ThrowRejectedExecutionException();
        }

        public TimeSpan Next(int attempt)
        {
            TimeSpan currMax;
            try
            {
                long multiple = checked(1 << attempt);
                currMax = _minDelay + _step.Multiply(multiple); // may throw OverflowException
                if (currMax <= TimeSpan.Zero) { ThrowHelper.ThrowOverflowException(); }
            }
            catch (OverflowException)
            {
                currMax = _maxDelay;
            }
            currMax = TimeUtil.Min(currMax, _maxDelay);

            if (_minDelay >= currMax) { ThrowHelper.ArgumentOutOfRangeException_Invalid_minValue(_minDelay, currMax); }
            return _random.NextTimeSpan(_minDelay, currMax);
        }
    }
}
