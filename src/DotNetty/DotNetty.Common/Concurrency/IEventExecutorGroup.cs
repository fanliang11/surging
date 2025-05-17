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
    using System.Threading.Tasks;

    /// <summary>
    /// The <see cref="IEventExecutorGroup"/> is responsible for providing the <see cref="IEventExecutor"/>'s to use
    /// via its <see cref="GetNext()"/> method. Besides this, it is also responsible for handling their
    /// life-cycle and allows shutting them down in a global fashion.
    /// </summary>
    public interface IEventExecutorGroup : IScheduledExecutorService
    {
        /// <summary>
        /// Returns list of owned event executors.
        /// </summary>
        IEnumerable<IEventExecutor> Items { get; }

        /// <summary>
        ///     Returns <c>true</c> if and only if this executor is being shut down via <see cref="ShutdownGracefullyAsync()" />.
        /// </summary>
        bool IsShuttingDown { get; }

        /// <summary>
        /// Terminates this <see cref="IEventExecutorGroup"/> and all its <see cref="IEventExecutor"/>s.
        /// </summary>
        /// <returns><see cref="Task"/> for completion of termination.</returns>
        Task ShutdownGracefullyAsync();

        /// <summary>
        /// Terminates this <see cref="IEventExecutorGroup"/> and all its <see cref="IEventExecutor"/>s.
        /// </summary>
        /// <returns><see cref="Task"/> for completion of termination.</returns>
        Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout);

        /// <summary>
        /// A <see cref="Task"/> for completion of termination. <see cref="ShutdownGracefullyAsync()"/>.
        /// </summary>
        Task TerminationCompletion { get; }

        /// <summary>
        /// Returns <see cref="IEventExecutor"/>.
        /// </summary>
        IEventExecutor GetNext();
    }
}