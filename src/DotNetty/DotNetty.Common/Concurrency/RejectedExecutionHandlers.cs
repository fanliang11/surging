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

    /// <summary>
    /// Expose helper methods which create different <see cref="IRejectedExecutionHandler"/>s.
    /// </summary>
    public sealed class RejectedExecutionHandlers
    {
        /// <summary>
        /// Returns a <see cref="IRejectedExecutionHandler"/> that will always just throw a <see cref="RejectedExecutionException"/>.
        /// </summary>
        public static IRejectedExecutionHandler Reject()
        {
            return DefaultRejectedExecutionHandler.Instance;
        }

        /// <summary>
        /// Tries to backoff when the task can not be added due restrictions for an configured amount of time. This
        /// </summary>
        /// <param name="retries"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static IRejectedExecutionHandler Backoff(int retries, TimeSpan delay)
        {
            return new FixedBackoffRejectedExecutionHandler(retries, delay);
        }

        public static IRejectedExecutionHandler Backoff(int retries, TimeSpan minDelay, TimeSpan maxDelay, TimeSpan step)
        {
            return new ExponentialBackoffRejectedExecutionHandler(retries, minDelay, maxDelay, step);
        }
    }
}
