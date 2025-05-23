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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#if DESKTOPCLR
namespace DotNetty.Handlers.Tls
{
    using System;
    using System.Diagnostics;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;

    partial class TlsHandler
    {
        partial class MediationStream
        {
            private byte[] _input;
            private ArraySegment<byte> _sslOwnedBuffer;
            private int _inputStartOffset;
            private SynchronousAsyncResult<int> _syncReadResult;
            private AsyncCallback _readCallback;
            private IPromise _writeCompletion;
            private AsyncCallback _writeCallback;

            public void SetSource(byte[] source, int offset, IByteBufferAllocator allocator)
            {
                _input = source;
                _inputStartOffset = offset;
                _inputOffset = 0;
                _inputLength = 0;
            }

            public void ResetSource(IByteBufferAllocator allocator)
            {
                _input = null;
                _inputLength = 0;
            }

            public void ExpandSource(int count)
            {
                Debug.Assert(_input is object);

                _inputLength += count;

                ArraySegment<byte> sslBuffer = _sslOwnedBuffer;
                if (sslBuffer.Array is null)
                {
                    // there is no pending read operation - keep for future
                    return;
                }
                _sslOwnedBuffer = default(ArraySegment<byte>);

                int read = ReadFromInput(sslBuffer.Array, sslBuffer.Offset, sslBuffer.Count);

                TaskCompletionSource<int> promise = _readCompletionSource;
                _readCompletionSource = null;
                _ = promise.TrySetResult(read);

                AsyncCallback callback = _readCallback;
                _readCallback = null;
                callback?.Invoke(promise.Task);
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                if (this.SourceReadableBytes > 0)
                {
                    // we have the bytes available upfront - write out synchronously
                    int read = ReadFromInput(buffer, offset, count);
                    var res = this.PrepareSyncReadResult(read, state);
                    callback?.Invoke(res);
                    return res;
                }

                Debug.Assert(_sslOwnedBuffer.Array is null);
                // take note of buffer - we will pass bytes there once available
                _sslOwnedBuffer = new ArraySegment<byte>(buffer, offset, count);
                _readCompletionSource = new TaskCompletionSource<int>(state);
                _readCallback = callback;
                return _readCompletionSource.Task;
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                SynchronousAsyncResult<int> syncResult = _syncReadResult;
                if (ReferenceEquals(asyncResult, syncResult))
                {
                    return syncResult.Result;
                }

                Debug.Assert(_readCompletionSource is null || _readCompletionSource.Task == asyncResult);
                Debug.Assert(!((Task<int>)asyncResult).IsCanceled);

                try
                {
                    return ((Task<int>)asyncResult).Result;
                }
                catch (AggregateException ex)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    throw; // unreachable
                }
            }

            private IAsyncResult PrepareSyncReadResult(int readBytes, object state)
            {
                // it is safe to reuse sync result object as it can't lead to leak (no way to attach to it via handle)
                SynchronousAsyncResult<int> result = _syncReadResult ?? (_syncReadResult = new SynchronousAsyncResult<int>());
                result.Result = readBytes;
                result.AsyncState = state;
                return result;
            }

            private int ReadFromInput(byte[] destination, int destinationOffset, int destinationCapacity)
            {
                Debug.Assert(destination is object);

                byte[] source = _input;
                int readableBytes = this.SourceReadableBytes;
                int length = Math.Min(readableBytes, destinationCapacity);
                Buffer.BlockCopy(source, _inputStartOffset + _inputOffset, destination, destinationOffset, length);
                _inputOffset += length;
                return length;
            }

            public override void Write(byte[] buffer, int offset, int count) => _owner.FinishWrap(buffer, offset, count, _owner._lastContextWritePromise);

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => _owner.FinishWrapNonAppDataAsync(buffer, offset, count, _owner.CapturedContext.NewPromise());

            private static readonly Action<Task, object> s_writeCompleteCallback = (t, s) => HandleChannelWriteComplete(t, s);

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                Task task = this.WriteAsync(buffer, offset, count);
                if (task.IsSuccess())
                {
                    // write+flush completed synchronously (and successfully)
                    var result = new SynchronousAsyncResult<int>
                    {
                        AsyncState = state
                    };
                    callback?.Invoke(result);
                    return result;
                }
                else
                {
                    if (callback is object || state != task.AsyncState)
                    {
                        Debug.Assert(_writeCompletion is null);
                        _writeCallback = callback;
                        var tcs = _owner.CapturedContext.NewPromise(state);
                        _writeCompletion = tcs;
                        _ = task.ContinueWith(s_writeCompleteCallback, this, TaskContinuationOptions.ExecuteSynchronously);
                        return tcs.Task;
                    }
                    else
                    {
                        return task;
                    }
                }
            }

            private static void HandleChannelWriteComplete(Task writeTask, object state)
            {
                var self = (MediationStream)state;

                AsyncCallback callback = self._writeCallback;
                self._writeCallback = null;

                var promise = self._writeCompletion;
                self._writeCompletion = null;

                if (writeTask.IsCanceled)
                {
                    _ = promise.TrySetCanceled();
                }
                else if (writeTask.IsFaulted)
                {
                    _ = promise.TrySetException(writeTask.Exception.InnerExceptions);
                }
                else if (writeTask.IsCompleted)
                {
                    _ = promise.TryComplete();
                }

                callback?.Invoke(promise.Task);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                if (asyncResult is SynchronousAsyncResult<int>)
                {
                    return;
                }

                try
                {
                    ((Task)asyncResult).Wait();
                }
                catch (AggregateException ex)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    throw;
                }
            }

            #region sync result

            private sealed class SynchronousAsyncResult<T> : IAsyncResult
            {
                public T Result { get; set; }

                public bool IsCompleted => true;

                public WaitHandle AsyncWaitHandle
                {
                    get { throw new InvalidOperationException("Cannot wait on a synchronous result."); }
                }

                public object AsyncState { get; set; }

                public bool CompletedSynchronously => true;
            }

            #endregion
        }
    }
}
#endif
