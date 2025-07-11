using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Surging.Core.CPlatform.Utilities
{
    public class ManualResetValueTaskSource2<T> : IValueTaskSource<T>
    {
        private ManualResetValueTaskSourceCore<T> _taskSource = new();

        public short Version => _taskSource.Version;

        public ManualResetValueTaskSource2()
        {
            _taskSource.RunContinuationsAsynchronously = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueTask<T> GetTask() => new(this, _taskSource.Version);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetResult(T result) => _taskSource.SetResult(result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Reset() => _taskSource.Reset();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetResult(short token) => _taskSource.GetResult(token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTaskSourceStatus GetStatus(short token) => _taskSource.GetStatus(token);
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception error) => _taskSource.SetException(error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) => _taskSource.OnCompleted(continuation, state, token, flags);
    }
}
