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
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal.Logging;

    public static class TaskUtil
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance(typeof(TaskUtil));

        public static readonly Task<int> Zero = Task.FromResult(0);

#if NET451
        public static readonly Task Completed = Zero;
#else
        public static readonly Task Completed = Task.CompletedTask;
#endif

        public static readonly Task<int> Cancelled = CreateCancelledTask();

        public static readonly Task<bool> True = Task.FromResult(true);

        public static readonly Task<bool> False = Task.FromResult(false);

        static Task<int> CreateCancelledTask()
        {
            var tcs = new TaskCompletionSource<int>(
#if !NET451
                TaskCreationOptions.RunContinuationsAsynchronously
#endif
                );
            tcs.SetCanceled();
            return tcs.Task;
        }

        public static Task FromCanceled(CancellationToken cancellationToken)
        {
#if NET451
            return CreateCancelledTask();
#else
            return Task.FromCanceled(cancellationToken);
#endif
        }

        public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken)
        {
#if NET451
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetCanceled();
            return tcs.Task;
#else
            return Task.FromCanceled<TResult>(cancellationToken);
#endif
        }

        public static Task FromException(Exception exception)
        {
#if NET451
            var tcs = new TaskCompletionSource<int>();
            tcs.TrySetException(exception);
            return tcs.Task;
#else
            return Task.FromException(exception);
#endif
        }

        public static Task<T> FromException<T>(Exception exception)
        {
#if NET451
            var tcs = new TaskCompletionSource<T>();
            tcs.TrySetException(exception);
            return tcs.Task;
#else
            return Task.FromException<T>(exception);
#endif
        }

        static readonly Action<Task, object> LinkOutcomeContinuationAction = (t, s) => LinkOutcomeContinuation(t, s);
        private static void LinkOutcomeContinuation(Task t, object tcs)
        {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            if (t.IsCompletedSuccessfully)
            {
                ((IPromise)tcs).TryComplete(); return;
            }
            else if (t.IsCanceled)
            {
                ((IPromise)tcs).TrySetCanceled(); return;
            }
            else if (t.IsFaulted)
            {
                ((IPromise)tcs).TrySetException(t.Exception.InnerExceptions); return;
            }
#else
            if (t.IsCanceled)
            {
                ((IPromise)tcs).TrySetCanceled(); return;
            }
            else if (t.IsFaulted)
            {
                ((IPromise)tcs).TrySetException(t.Exception.InnerExceptions); return;
            }
            else if (t.IsCompleted)
            {
                ((IPromise)tcs).TryComplete(); return;
            }
#endif
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        public static void LinkOutcome(this Task task, IPromise promise)
        {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            if (task.IsCompletedSuccessfully)
            {
                promise.TryComplete(); return;
            }
            else if (task.IsCanceled)
            {
                promise.TrySetCanceled(); return;
            }
            else if (task.IsFaulted)
            {
                promise.TrySetException(task.Exception.InnerExceptions); return;
            }
#else
            if (task.IsCanceled)
            {
                promise.TrySetCanceled(); return;
            }
            else if (task.IsFaulted)
            {
                promise.TrySetException(task.Exception.InnerExceptions); return;
            }
            else if (task.IsCompleted)
            {
                promise.TryComplete(); return;
            }
#endif
            task.ContinueWith(
                LinkOutcomeContinuationAction,
                promise,
                TaskContinuationOptions.ExecuteSynchronously);
        }

        static readonly Action<Task, object> CascadeToContinuationAction = (t, s) => CascadeToContinuation(t, s);
        private static void CascadeToContinuation(Task t, object s)
        {
            var wrapped = ((IPromise, IInternalLogger))s;
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            if (t.IsCompletedSuccessfully)
            {
                wrapped.Item1.TryComplete(wrapped.Item2); return;
            }
            else if (t.IsCanceled)
            {
                wrapped.Item1.TrySetCanceled(wrapped.Item2); return;
            }
            else if (t.IsFaulted)
            {
                wrapped.Item1.TrySetException(t.Exception, wrapped.Item2); return;
            }
#else
            if (t.IsCanceled)
            {
                wrapped.Item1.TrySetCanceled(wrapped.Item2); return;
            }
            else if (t.IsFaulted)
            {
                wrapped.Item1.TrySetException(t.Exception, wrapped.Item2); return;
            }
            else if (t.IsCompleted)
            {
                wrapped.Item1.TryComplete(wrapped.Item2); return;
            }
#endif
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        public static void CascadeTo(this Task task, IPromise promise, IInternalLogger logger = null)
        {
            logger ??= Logger;
            var internalLogger = !promise.IsVoid ? logger : null;

#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            if (task.IsCompletedSuccessfully)
            {
                promise.TryComplete(internalLogger); return;
            }
            else if (task.IsCanceled)
            {
                promise.TrySetCanceled(internalLogger); return;
            }
            else if (task.IsFaulted)
            {
                promise.TrySetException(task.Exception, internalLogger); return;
            }
#else
            if (task.IsCanceled)
            {
                promise.TrySetCanceled(internalLogger); return;
            }
            else if (task.IsFaulted)
            {
                promise.TrySetException(task.Exception, internalLogger); return;
            }
            else if (task.IsCompleted)
            {
                promise.TryComplete(internalLogger); return;
            }
#endif
            task.ContinueWith(
                CascadeToContinuationAction,
                (promise, internalLogger),
                TaskContinuationOptions.ExecuteSynchronously);
        }

        static class LinkOutcomeActionHost<T>
        {
            public static readonly Action<Task<T>, object> Action =
                (t, tcs) =>
                {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                    if (t.IsCompletedSuccessfully)
                    {
                        ((TaskCompletionSource<T>)tcs).TrySetResult(t.Result); return;
                    }
                    else if (t.IsCanceled)
                    {
                        ((TaskCompletionSource<T>)tcs).TrySetCanceled(); return;
                    }
                    else if (t.IsFaulted)
                    {
                        ((TaskCompletionSource<T>)tcs).TryUnwrap(t.Exception); return;
                    }
#else
                    if (t.IsCanceled)
                    {
                        ((TaskCompletionSource<T>)tcs).TrySetCanceled(); return;
                    }
                    else if (t.IsFaulted)
                    {
                        ((TaskCompletionSource<T>)tcs).TryUnwrap(t.Exception); return;
                    }
                    else if (t.IsCompleted)
                    {
                        ((TaskCompletionSource<T>)tcs).TrySetResult(t.Result); return;
                    }
#endif
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                };
        }

        public static void LinkOutcome<T>(this Task<T> task, TaskCompletionSource<T> taskCompletionSource)
        {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            if (task.IsCompletedSuccessfully)
            {
                taskCompletionSource.TrySetResult(task.Result); return;
            }
            else if (task.IsCanceled)
            {
                taskCompletionSource.TrySetCanceled(); return;
            }
            else if (task.IsFaulted)
            {
                taskCompletionSource.TryUnwrap(task.Exception); return;
            }
#else
            if (task.IsCanceled)
            {
                taskCompletionSource.TrySetCanceled(); return;
            }
            else if (task.IsFaulted)
            {
                taskCompletionSource.TryUnwrap(task.Exception); return;
            }
            else if (task.IsCompleted)
            {
                taskCompletionSource.TrySetResult(task.Result); return;
            }
#endif
            task.ContinueWith(LinkOutcomeActionHost<T>.Action, taskCompletionSource, TaskContinuationOptions.ExecuteSynchronously);
        }

        public static void TryUnwrap<T>(this TaskCompletionSource<T> completionSource, Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                _ = completionSource.TrySetException(aggregateException.InnerExceptions);
            }
            else
            {
                _ = completionSource.TrySetException(exception);
            }
        }

        public static Exception Unwrap(this Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                return aggregateException.InnerException;
            }

            return exception;
        }

        /// <summary>TBD</summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsSuccess(this Task task)
        {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            return task.IsCompletedSuccessfully;
#else
            return task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
#endif
        }

        /// <summary>TBD</summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsSuccess<T>(this Task<T> task)
        {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            return task.IsCompletedSuccessfully;
#else
            return task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
#endif
        }

        /// <summary>TBD</summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsFailure(this Task task)
        {
            return task.IsFaulted || task.IsCanceled;
        }

        /// <summary>TBD</summary>
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        public static bool IsFailure<T>(this Task<T> task)
        {
            return task.IsFaulted || task.IsCanceled;
        }

        private static readonly Action<Task> IgnoreTaskContinuation = t => { _ = t.Exception; };

        /// <summary>Observes and ignores a potential exception on a given Task.
        /// If a Task fails and throws an exception which is never observed, it will be caught by the .NET finalizer thread.
        /// This function awaits the given task and if the exception is thrown, it observes this exception and simply ignores it.
        /// This will prevent the escalation of this exception to the .NET finalizer thread.</summary>
        /// <param name="task">The task to be ignored.</param>
        public static void Ignore(this Task task)
        {
            if (task.IsCompleted)
            {
                _ = task.Exception;
            }
            else
            {
                _ = task.ContinueWith(
                    IgnoreTaskContinuation,
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }

        public static async Task<bool> WaitAsync(Task task, TimeSpan timeout)
        {
            return await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false) == task;
        }

        public static void WaitWithThrow(this Task task, TimeSpan timeout)
        {
            if (!task.Wait(timeout))
            {
                ThrowHelper.ThrowTimeoutException_WaitWithThrow(timeout);
            }
        }

        public static T WaitWithThrow<T>(this Task<T> task, TimeSpan timeout)
        {
            if (!task.Wait(timeout))
            {
                ThrowHelper.ThrowTimeoutException_WaitWithThrow(timeout);
            }
            return task.Result;
        }

        /// <summary>
        /// This will apply a timeout delay to the task, allowing us to exit early
        /// </summary>
        /// <param name="taskToComplete">The task we will timeout after timeSpan</param>
        /// <param name="timeout">Amount of time to wait before timing out</param>
        /// <returns>The completed task</returns>
        public static async Task WithTimeout(this Task taskToComplete, TimeSpan timeout)
        {
            if (taskToComplete.IsCompleted)
            {
                await taskToComplete;
                return;
            }

            var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(taskToComplete, Task.Delay(timeout, timeoutCancellationTokenSource.Token));

            // We got done before the timeout, or were able to complete before this code ran, return the result
            if (taskToComplete == completedTask)
            {
                timeoutCancellationTokenSource.Cancel();
                // Await this so as to propagate the exception correctly
                await taskToComplete;
                return;
            }

            // We did not complete before the timeout, we fire and forget to ensure we observe any exceptions that may occur
            taskToComplete.Ignore();
            ThrowHelper.ThrowTimeoutException_WithTimeout(timeout);
        }

        /// <summary>
        /// This will apply a timeout delay to the task, allowing us to exit early
        /// </summary>
        /// <param name="taskToComplete">The task we will timeout after timeSpan</param>
        /// <param name="timeSpan">Amount of time to wait before timing out</param>
        /// <exception cref="TimeoutException">If we time out we will get this exception</exception>
        /// <exception cref="TimeoutException">If we time out we will get this exception</exception>
        /// <returns>The value of the completed task</returns>
        public static async Task<T> WithTimeout<T>(this Task<T> taskToComplete, TimeSpan timeSpan)
        {
            if (taskToComplete.IsCompleted)
            {
                return await taskToComplete;
            }

            var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(taskToComplete, Task.Delay(timeSpan, timeoutCancellationTokenSource.Token));

            // We got done before the timeout, or were able to complete before this code ran, return the result
            if (taskToComplete == completedTask)
            {
                timeoutCancellationTokenSource.Cancel();
                // Await this so as to propagate the exception correctly
                return await taskToComplete;
            }

            // We did not complete before the timeout, we fire and forget to ensure we observe any exceptions that may occur
            taskToComplete.Ignore();
            throw ThrowHelper.GetTimeoutException_WithTimeout(timeSpan);
        }

        /// <summary>
        /// For making an uncancellable task cancellable, by ignoring its result.
        /// </summary>
        /// <param name="taskToComplete">The task to wait for unless cancelled</param>
        /// <param name="cancellationToken">A cancellation token for cancelling the wait</param>
        /// <param name="message">Message to set in the exception</param>
        /// <returns></returns>
        public static async Task WithCancellation(this Task taskToComplete, CancellationToken cancellationToken, string message)
        {
            try
            {
                await taskToComplete.WithCancellation(cancellationToken);
            }
            catch (TaskCanceledException ex)
            {
                throw new TaskCanceledException(message, ex);
            }
        }

        /// <summary>
        /// For making an uncancellable task cancellable, by ignoring its result.
        /// </summary>
        /// <param name="taskToComplete">The task to wait for unless cancelled</param>
        /// <param name="cancellationToken">A cancellation token for cancelling the wait</param>
        /// <returns></returns>
        public static Task WithCancellation(this Task taskToComplete, CancellationToken cancellationToken)
        {
            if (taskToComplete.IsCompleted || !cancellationToken.CanBeCanceled)
            {
                return taskToComplete;
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                return FromCanceled<object>(cancellationToken);
            }
            else
            {
                return MakeCancellable(taskToComplete, cancellationToken);
            }
        }

        private static async Task MakeCancellable(Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>(
#if !NET451
                TaskCreationOptions.RunContinuationsAsynchronously
#endif
                );
            using (cancellationToken.Register(() =>
                      tcs.TrySetCanceled(
#if !NET451
                          cancellationToken
#endif
                          ), useSynchronizationContext: false))
            {
                var firstToComplete = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);

                if (firstToComplete != task)
                {
                    task.Ignore();
                }

                await firstToComplete.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// For making an uncancellable task cancellable, by ignoring its result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taskToComplete">The task to wait for unless cancelled</param>
        /// <param name="cancellationToken">A cancellation token for cancelling the wait</param>
        /// <param name="message">Message to set in the exception</param>
        /// <returns></returns>
        public static async Task<T> WithCancellation<T>(this Task<T> taskToComplete, CancellationToken cancellationToken, string message)
        {
            try
            {
                return await taskToComplete.WithCancellation(cancellationToken);
            }
            catch (TaskCanceledException ex)
            {
                throw new TaskCanceledException(message, ex);
            }
        }

        /// <summary>
        /// For making an uncancellable task cancellable, by ignoring its result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taskToComplete">The task to wait for unless cancelled</param>
        /// <param name="cancellationToken">A cancellation token for cancelling the wait</param>
        /// <returns></returns>
        public static Task<T> WithCancellation<T>(this Task<T> taskToComplete, CancellationToken cancellationToken)
        {
            if (taskToComplete.IsCompleted || !cancellationToken.CanBeCanceled)
            {
                return taskToComplete;
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                return FromCanceled<T>(cancellationToken);
            }
            else
            {
                return MakeCancellable(taskToComplete, cancellationToken);
            }
        }

        private static async Task<T> MakeCancellable<T>(Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>(
#if !NET451
                TaskCreationOptions.RunContinuationsAsynchronously
#endif
                );
            using (cancellationToken.Register(() =>
                      tcs.TrySetCanceled(
#if !NET451
                          cancellationToken
#endif
                          ), useSynchronizationContext: false))
            {
                var firstToComplete = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);

                if (firstToComplete != task)
                {
                    task.Ignore();
                }

                return await firstToComplete.ConfigureAwait(false);
            }
        }
    }
}