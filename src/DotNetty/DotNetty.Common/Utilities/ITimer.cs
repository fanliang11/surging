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

namespace DotNetty.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;

    /// <summary>
    /// Schedules <see cref="ITimerTask"/>s for one-time future execution in a background
    /// thread.
    /// </summary>
    public interface ITimer
    {
        /// <summary>
        /// Schedules the specified <see cref="ITimerTask"/> for one-time execution after the specified delay.
        /// </summary>
        /// <returns>a handle which is associated with the specified task</returns>
        /// <exception cref="InvalidOperationException">if this timer has been stopped already</exception>
        /// <exception cref="RejectedExecutionException">if the pending timeouts are too many and creating new timeout
        /// can cause instability in the system.</exception>
        ITimeout NewTimeout(ITimerTask task, TimeSpan delay);

        /// <summary>
        /// Releases all resources acquired by this <see cref="ITimer"/> and cancels all
        /// tasks which were scheduled but not executed yet.
        /// </summary>
        /// <returns>the handles associated with the tasks which were canceled by
        /// this method</returns>
        Task<ISet<ITimeout>> StopAsync();
    }
}