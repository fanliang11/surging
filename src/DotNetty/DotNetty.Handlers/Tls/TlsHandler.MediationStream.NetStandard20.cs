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

#if NETSTANDARD2_0
namespace DotNetty.Handlers.Tls
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Buffers;

    partial class TlsHandler
    {
        partial class MediationStream
        {
            private byte[] _input;
            private ArraySegment<byte> _sslOwnedBuffer;
            private int _inputStartOffset;
            private int _readByteCount;

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
                _sslOwnedBuffer = default;

                _readByteCount = this.ReadFromInput(sslBuffer.Array, sslBuffer.Offset, sslBuffer.Count);
                // hack: this tricks SslStream's continuation to run synchronously instead of dispatching to TP. Remove once Begin/EndRead are available. 
                new Task(ReadCompletionAction, this).RunSynchronously(TaskScheduler.Default);
            }

            static readonly Action<object> ReadCompletionAction = m => ReadCompletion(m);
            static void ReadCompletion(object ms)
            {
                var self = (MediationStream)ms;
                TaskCompletionSource<int> p = self._readCompletionSource;
                self._readCompletionSource = null;
                _ = p.TrySetResult(self._readByteCount);
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (this.SourceReadableBytes > 0)
                {
                    // we have the bytes available upfront - write out synchronously
                    int read = ReadFromInput(buffer, offset, count);
                    return Task.FromResult(read);
                }

                Debug.Assert(_sslOwnedBuffer.Array is null);
                // take note of buffer - we will pass bytes there once available
                _sslOwnedBuffer = new ArraySegment<byte>(buffer, offset, count);
                _readCompletionSource = new TaskCompletionSource<int>();
                return _readCompletionSource.Task;
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
        }
    }
}
#endif