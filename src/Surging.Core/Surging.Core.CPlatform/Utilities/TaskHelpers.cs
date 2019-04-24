using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Utilities
{
    internal static class TaskHelpers
    {
        private static readonly Task _defaultCompleted = Task.FromResult<AsyncVoid>(default(AsyncVoid));

        private static readonly Task<object> _completedTaskReturningNull = Task.FromResult<object>(null);
        
        internal static Task Canceled()
        {
            return CancelCache<AsyncVoid>.Canceled;
        }
        
        internal static Task<TResult> Canceled<TResult>()
        {
            return CancelCache<TResult>.Canceled;
        }
       
        internal static Task Completed()
        {
            return _defaultCompleted;
        }
        
        internal static Task FromError(Exception exception)
        {
            return FromError<AsyncVoid>(exception);
        }
        
        internal static Task<TResult> FromError<TResult>(Exception exception)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        internal static Task<object> NullResult()
        {
            return _completedTaskReturningNull;
        }
        
        private struct AsyncVoid
        {
        }
        
        private static class CancelCache<TResult>
        {
            public static readonly Task<TResult> Canceled = GetCancelledTask();

            private static Task<TResult> GetCancelledTask()
            {
                TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
                tcs.SetCanceled();
                return tcs.Task;
            }
        }
    }
}
