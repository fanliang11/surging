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

#if NETCOREAPP || NETSTANDARD_2_0_GREATER

namespace DotNetty.Handlers.Tls
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    partial class TlsHandler
    {
        partial class MediationStream
        {
            private ReadOnlyMemory<byte> _input;
            private Memory<byte> _sslOwnedBuffer;

            public void SetSource(in ReadOnlyMemory<byte> source, IByteBufferAllocator allocator)
            {
                lock (this)
                {
                    ResetSource(allocator);

                    _input = source;
                    _inputOffset = 0;
                    _inputLength = 0;
                }
            }

            public void ResetSource(IByteBufferAllocator allocator)
            {
                lock (this)
                {
                    int leftLen = SourceReadableBytes;
                    var buf = _ownedInputBuffer;
                    if (leftLen > 0)
                    {
                        if (buf is object)
                        {
                            buf.DiscardSomeReadBytes();
                        }
                        else
                        {
                            buf = allocator.CompositeBuffer();
                            _ownedInputBuffer = buf;
                        }
                        buf.WriteBytes(_input.Slice(_inputOffset, leftLen));
                    }
                    else
                    {
                        buf?.DiscardSomeReadBytes();
                    }
                    _input = null;
                    _inputOffset = 0;
                    _inputLength = 0;
                }
            }

            public void ExpandSource(int count)
            {
                int readByteCount;
                TaskCompletionSource<int> readCompletionSource;
                lock (this)
                {
                    Debug.Assert(!_input.IsEmpty);

                    _inputLength += count;

                    var sslBuffer = _sslOwnedBuffer;
                    readCompletionSource = _readCompletionSource;
                    if (readCompletionSource is null)
                    {
                        // there is no pending read operation - keep for future
                        return;
                    }
                    _sslOwnedBuffer = default;

                    readByteCount = ReadFromInput(sslBuffer);
                }
                // hack: this tricks SslStream's continuation to run synchronously instead of dispatching to TP. Remove once Begin/EndRead are available. 
                // The continuation can only run synchronously when the TaskScheduler is not ExecutorTaskScheduler
                new Task(ReadCompletionAction, (this, readCompletionSource, readByteCount)).RunSynchronously(TaskScheduler.Default);
            }

            static readonly Action<object> ReadCompletionAction = s => ReadCompletion(s);
            static void ReadCompletion(object state)
            {
                var (self, readCompletionSource, readByteCount) = ((MediationStream, TaskCompletionSource<int>, int))state;
                if (ReferenceEquals(readCompletionSource, self._readCompletionSource))
                {
                    self._readCompletionSource = null;
                }
                _ = readCompletionSource.TrySetResult(readByteCount);
            }

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                if (TotalReadableBytes > 0)
                {
                    // we have the bytes available upfront - write out synchronously
                    int read = ReadFromInput(buffer);
                    return new ValueTask<int>(read);
                }

                Debug.Assert(_sslOwnedBuffer.IsEmpty);
                // take note of buffer - we will pass bytes there once available
                _sslOwnedBuffer = buffer;
                var readCompletionSource = new TaskCompletionSource<int>();
                _readCompletionSource = readCompletionSource;
                return new ValueTask<int>(readCompletionSource.Task);
            }

            private int ReadFromInput(Memory<byte> destination) // byte[] destination, int destinationOffset, int destinationCapacity
            {
                if (destination.IsEmpty) { return 0; }

                lock (this)
                {
                    int totalRead = 0;
                    var destLen = destination.Length;
                    int readableBytes;

                    var buf = _ownedInputBuffer;
                    if (buf is object)
                    {
                        readableBytes = buf.ReadableBytes;
                        if (readableBytes > 0)
                        {
                            var read = Math.Min(readableBytes, destLen);
                            buf.ReadBytes(destination);
                            totalRead += read;
                            destLen -= read;
                            if (!buf.IsReadable())
                            {
                                buf.Release();
                                _ownedInputBuffer = null;
                            }
                            if (0u > (uint)destLen) { return totalRead; }
                        }
                    }

                    readableBytes = SourceReadableBytes;
                    if (readableBytes > 0)
                    {
                        var read = Math.Min(readableBytes, destLen);
                        _input.Slice(_inputOffset, read).CopyTo(destination.Slice(totalRead));
                        totalRead += read;
                        destLen -= read;
                        _inputOffset += read;
                    }

                    return totalRead;
                }
            }

            public override void Write(ReadOnlySpan<byte> buffer)
                => _owner.FinishWrap(buffer, _owner._lastContextWritePromise);

            public override void Write(byte[] buffer, int offset, int count)
                => _owner.FinishWrap(buffer, offset, count, _owner._lastContextWritePromise);

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                return new ValueTask(_owner.FinishWrapNonAppDataAsync(buffer, _owner.CapturedContext.NewPromise()));
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => _owner.FinishWrapNonAppDataAsync(buffer, offset, count, _owner.CapturedContext.NewPromise());
        }
    }
}

#endif