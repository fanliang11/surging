using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Utilities
{
    public static class CancellationTokenExtensions
    {
        public static Task WhenCanceled(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

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
    }
}
