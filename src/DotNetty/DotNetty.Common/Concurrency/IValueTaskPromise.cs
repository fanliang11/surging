using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetty.Common.Concurrency
{
    public interface IValueTaskPromise: IPromise
    {   
            bool IsVoid { get; }

            bool IsCompleted { get; }

            bool IsSuccess { get; }

            bool IsFaulted { get; }

            bool IsCanceled { get; }

            bool TryComplete();

            void Complete();

            bool TrySetException(Exception exception);

            bool TrySetException(IEnumerable<Exception> exceptions);

            void SetException(Exception exception);

            void SetException(IEnumerable<Exception> exceptions);

            bool TrySetCanceled();

            void SetCanceled();

            bool SetUncancellable();

             IPromise Unvoid();
        }
    } 