using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Utilities
{
    /// <summary>
    /// Defines the <see cref="CancellationTokenExtensions" />
    /// </summary>
    public static class CancellationTokenExtensions
    {
        #region 方法

        /// <summary>
        /// The WhenCanceled
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public static Task WhenCanceled(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// The WithCancellation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">The task<see cref="Task{T}"/></param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public static async Task<T> WithCancellation<T>(
    this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(
                        s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new TimeoutException();
            return await task;
        }

        /// <summary>
        /// The WithCancellation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">The task<see cref="Task{T}"/></param>
        /// <param name="cts">The cts<see cref="CancellationTokenSource"/></param>
        /// <param name="requestTimeout">The requestTimeout<see cref="int"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public static async Task<T> WithCancellation<T>(
    this Task<T> task, CancellationTokenSource cts, int requestTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(requestTimeout, cts.Token)))
            {
                cts.Cancel();
                return await task;
            }
            throw new TimeoutException();
        }

        #endregion 方法
    }
}