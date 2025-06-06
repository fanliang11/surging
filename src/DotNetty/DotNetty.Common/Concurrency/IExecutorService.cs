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
    using System.Threading;
    using System.Threading.Tasks;

    public interface IExecutorService : IExecutor
    {
        /// <summary>
        ///     Returns <c>true</c> if this executor has been shut down, <c>false</c> otherwise.
        /// </summary>
        bool IsShutdown { get; }

        /// <summary>
        ///     Returns <c>true</c> if all tasks have completed following shut down.
        /// </summary>
        /// <remarks>
        ///     Note that <see cref="IsTerminated" /> is never <c>true</c> unless <see cref="IEventExecutorGroup.ShutdownGracefullyAsync()" /> was called first.
        /// </remarks>
        bool IsTerminated { get; }

        bool WaitTermination(TimeSpan timeout);

        /// <summary>
        ///     Executes the given function and returns <see cref="Task{T}" /> indicating completion status and result of
        ///     execution.
        /// </summary>
        /// <remarks>
        ///     <para>Threading specifics are determined by <c>IEventExecutor</c> implementation.</para>
        /// </remarks>
        Task<T> SubmitAsync<T>(Func<T> func);

        /// <summary>
        ///     Executes the given action and returns <see cref="Task{T}" /> indicating completion status and result of execution.
        /// </summary>
        /// <remarks>
        ///     <para>Threading specifics are determined by <c>IEventExecutor</c> implementation.</para>
        /// </remarks>
        Task<T> SubmitAsync<T>(Func<T> func, CancellationToken cancellationToken);

        /// <summary>
        ///     Executes the given action and returns <see cref="Task{T}" /> indicating completion status and result of execution.
        /// </summary>
        /// <remarks>
        ///     <paramref name="state" /> parameter is useful to when repeated execution of an action against
        ///     different objects is needed.
        ///     <para>Threading specifics are determined by <c>IEventExecutor</c> implementation.</para>
        /// </remarks>
        Task<T> SubmitAsync<T>(Func<object, T> func, object state);

        /// <summary>
        ///     Executes the given action and returns <see cref="Task{T}" /> indicating completion status and result of execution.
        /// </summary>
        /// <remarks>
        ///     <paramref name="state" /> parameter is useful to when repeated execution of an action against
        ///     different objects is needed.
        ///     <para>Threading specifics are determined by <c>IEventExecutor</c> implementation.</para>
        /// </remarks>
        Task<T> SubmitAsync<T>(Func<object, T> func, object state, CancellationToken cancellationToken);

        /// <summary>
        ///     Executes the given action and returns <see cref="Task{T}" /> indicating completion status and result of execution.
        /// </summary>
        /// <remarks>
        ///     <paramref name="context" /> and <paramref name="state" /> parameters are useful when repeated execution of
        ///     an action against different objects in different context is needed.
        ///     <para>Threading specifics are determined by <c>IEventExecutor</c> implementation.</para>
        /// </remarks>
        Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state);

        /// <summary>
        ///     Executes the given action and returns <see cref="Task{T}" /> indicating completion status and result of execution.
        /// </summary>
        /// <remarks>
        ///     <paramref name="context" /> and <paramref name="state" /> parameters are useful when repeated execution of
        ///     an action against different objects in different context is needed.
        ///     <para>Threading specifics are determined by <c>IEventExecutor</c> implementation.</para>
        /// </remarks>
        Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state, CancellationToken cancellationToken);
    }
}