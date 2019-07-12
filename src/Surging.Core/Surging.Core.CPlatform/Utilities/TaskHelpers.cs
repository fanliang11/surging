using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Utilities
{
    /// <summary>
    /// Defines the <see cref="TaskHelpers" />
    /// </summary>
    internal static class TaskHelpers
    {
        #region 字段

        /// <summary>
        /// Defines the _completedTaskReturningNull
        /// </summary>
        private static readonly Task<object> _completedTaskReturningNull = Task.FromResult<object>(null);

        /// <summary>
        /// Defines the _defaultCompleted
        /// </summary>
        private static readonly Task _defaultCompleted = Task.FromResult<AsyncVoid>(default(AsyncVoid));

        #endregion 字段

        #region 方法

        /// <summary>
        /// The Canceled
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        internal static Task Canceled()
        {
            return CancelCache<AsyncVoid>.Canceled;
        }

        /// <summary>
        /// The Canceled
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns>The <see cref="Task{TResult}"/></returns>
        internal static Task<TResult> Canceled<TResult>()
        {
            return CancelCache<TResult>.Canceled;
        }

        /// <summary>
        /// The Completed
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        internal static Task Completed()
        {
            return _defaultCompleted;
        }

        /// <summary>
        /// The FromError
        /// </summary>
        /// <param name="exception">The exception<see cref="Exception"/></param>
        /// <returns>The <see cref="Task"/></returns>
        internal static Task FromError(Exception exception)
        {
            return FromError<AsyncVoid>(exception);
        }

        /// <summary>
        /// The FromError
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="exception">The exception<see cref="Exception"/></param>
        /// <returns>The <see cref="Task{TResult}"/></returns>
        internal static Task<TResult> FromError<TResult>(Exception exception)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        /// <summary>
        /// The NullResult
        /// </summary>
        /// <returns>The <see cref="Task{object}"/></returns>
        internal static Task<object> NullResult()
        {
            return _completedTaskReturningNull;
        }

        #endregion 方法

        /// <summary>
        /// Defines the <see cref="AsyncVoid" />
        /// </summary>
        private struct AsyncVoid
        {
        }

        /// <summary>
        /// Defines the <see cref="CancelCache{TResult}" />
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        private static class CancelCache<TResult>
        {
            #region 字段

            /// <summary>
            /// Defines the Canceled
            /// </summary>
            public static readonly Task<TResult> Canceled = GetCancelledTask();

            #endregion 字段

            #region 方法

            /// <summary>
            /// The GetCancelledTask
            /// </summary>
            /// <returns>The <see cref="Task{TResult}"/></returns>
            private static Task<TResult> GetCancelledTask()
            {
                TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
                tcs.SetCanceled();
                return tcs.Task;
            }

            #endregion 方法
        }
    }
}