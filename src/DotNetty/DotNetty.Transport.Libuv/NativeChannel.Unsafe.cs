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

namespace DotNetty.Transport.Libuv
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Libuv.Native;

    partial class NativeChannel<TChannel, TUnsafe>
    {
        public abstract class NativeChannelUnsafe : AbstractUnsafe, INativeUnsafe
        {
            private static readonly Action<object, object> CancelConnectAction = (c, s) => CancelConnect(c, s);

            protected NativeChannelUnsafe() : base()
            {
            }

            public override Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
            {
                var ch = _channel;
                if (!ch.IsOpen)
                {
                    return CreateClosedChannelExceptionTask();
                }

                try
                {
                    if (ch._connectPromise is object)
                    {
                        ThrowHelper.ThrowInvalidOperationException_ConnAttempt();
                    }

                    ch._connectPromise = ch.NewPromise(remoteAddress);

                    // Schedule connect timeout.
                    TimeSpan connectTimeout = ch.Configuration.ConnectTimeout;
                    if (connectTimeout > TimeSpan.Zero)
                    {
                        ch._connectCancellationTask = ch.EventLoop
                            .Schedule(CancelConnectAction, ch, remoteAddress, connectTimeout);
                    }

                    ch.DoConnect(remoteAddress, localAddress);
                    return ch._connectPromise.Task;
                }
                catch (Exception ex)
                {
                    CloseIfClosed();
                    return TaskUtil.FromException(AnnotateConnectException(ex, remoteAddress));
                }
            }

            static void CancelConnect(object context, object state)
            {
                var ch = (TChannel)context;
                var address = (IPEndPoint)state;
                var promise = ch._connectPromise;
                var cause = new ConnectTimeoutException($"connection timed out: {address}");
                if (promise is object && promise.TrySetException(cause))
                {
                    ch.Unsafe.CloseSafe();
                }
            }

            // Connect request callback from libuv thread
            void INativeUnsafe.FinishConnect(ConnectRequest request)
            {
                var ch = _channel;
                _ = (ch._connectCancellationTask?.Cancel());

                var promise = ch._connectPromise;
                bool success = false;
                try
                {
                    if (promise is object) // Not cancelled from timed out
                    {
                        OperationException error = request.Error;
                        if (error is object)
                        {
                            if (error.ErrorCode == ErrorCode.ETIMEDOUT)
                            {
                                // Connection timed out should use the standard ConnectTimeoutException
                                _ = promise.TrySetException(ThrowHelper.GetConnectTimeoutException(error));
                            }
                            else
                            {
                                _ = promise.TrySetException(ThrowHelper.GetChannelException(error));
                            }
                        }
                        else
                        {
                            bool wasActive = ch.IsActive;
                            ch.DoFinishConnect();
                            success = promise.TryComplete();

                            // Regardless if the connection attempt was cancelled, channelActive() 
                            // event should be triggered, because what happened is what happened.
                            if (!wasActive && ch.IsActive)
                            {
                                _ = ch.Pipeline.FireChannelActive();
                            }
                        }
                    }
                }
                finally
                {
                    request.Dispose();
                    ch._connectPromise = null;
                    if (!success)
                    {
                        CloseSafe();
                    }
                }
            }

            public abstract IntPtr UnsafeHandle { get; }

            // Allocate callback from libuv thread
            uv_buf_t INativeUnsafe.PrepareRead(ReadOperation readOperation)
            {
                Debug.Assert(readOperation is object);

                var ch = _channel;
                IChannelConfiguration config = ch.Configuration;
                IByteBufferAllocator allocator = config.Allocator;

                IRecvByteBufAllocatorHandle allocHandle = RecvBufAllocHandle;
                IByteBuffer buffer = allocHandle.Allocate(allocator);
                allocHandle.AttemptedBytesRead = buffer.WritableBytes;

                return readOperation.GetBuffer(buffer);
            }

            // Read callback from libuv thread
            void INativeUnsafe.FinishRead(ReadOperation operation)
            {
                var ch = _channel;
                IChannelConfiguration config = ch.Configuration;
                IChannelPipeline pipeline = ch.Pipeline;
                OperationException error = operation.Error;

                bool close = error is object || operation.EndOfStream;
                IRecvByteBufAllocatorHandle allocHandle = RecvBufAllocHandle;
                allocHandle.Reset(config);

                IByteBuffer buffer = operation.Buffer;
                Debug.Assert(buffer is object);

                allocHandle.LastBytesRead = operation.Status;
                if (allocHandle.LastBytesRead <= 0)
                {
                    // nothing was read -> release the buffer.
                    _ = buffer.Release();
                    close = allocHandle.LastBytesRead < 0;
                    if (close)
                    {
                        // There is nothing left to read as we received an EOF.
                        ch.ReadPending = false;
                    }
                }
                else
                {
                    _ = buffer.SetWriterIndex(buffer.WriterIndex + operation.Status);
                    allocHandle.IncMessagesRead(1);

                    ch.ReadPending = false;
                    _ = pipeline.FireChannelRead(buffer);
                }

                allocHandle.ReadComplete();
                _ = pipeline.FireChannelReadComplete();

                if (close)
                {
                    if (error is object)
                    {
                        _ = pipeline.FireExceptionCaught(ThrowHelper.GetChannelException(error));
                    }
                    if (ch.IsOpen) { Close(VoidPromise()); }
                }
                else
                {
                    // If read is called from channel read or read complete
                    // do not stop reading
                    if (!ch.ReadPending && !config.IsAutoRead)
                    {
                        ch.DoStopRead();
                    }
                }
            }

            internal void CloseSafe() => CloseSafe(_channel, _channel.CloseAsync());

            internal static async void CloseSafe(object channelObject, Task closeTask)
            {
                try
                {
                    await closeTask;
                }
                catch (TaskCanceledException)
                {
                }
#if DEBUG
                catch (Exception ex)
                {
                    if (Logger.DebugEnabled)
                    {
                        Logger.FailedToCloseChannelCleanly(channelObject, ex);
                    }
                }
#else
                catch (Exception) { }
#endif
            }

            protected sealed override void Flush0()
            {
                var ch = _channel;
                if (!ch.IsInState(StateFlags.WriteScheduled))
                {
                    base.Flush0();
                }
            }

            // Write request callback from libuv thread
            void INativeUnsafe.FinishWrite(int bytesWritten, OperationException error)
            {
                var ch = _channel;
                bool resetWritePending = ch.TryResetState(StateFlags.WriteScheduled);
                Debug.Assert(resetWritePending);

                try
                {
                    var input = OutboundBuffer;
                    if (error is object)
                    {
                        input?.FailFlushed(error, true);
                        _ = ch.Pipeline.FireExceptionCaught(error);
                        Close(VoidPromise(), ThrowHelper.GetChannelException_FailedToWrite(error), WriteClosedChannelException, false);
                    }
                    else
                    {
                        if (bytesWritten > 0)
                        {
                            input?.RemoveBytes(bytesWritten);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Close(VoidPromise(), ThrowHelper.GetClosedChannelException_FailedToWrite(ex), WriteClosedChannelException, false);
                }
                Flush0();
            }
        }
    }

    internal interface INativeUnsafe
    {
        IntPtr UnsafeHandle { get; }

        void FinishConnect(ConnectRequest request);

        uv_buf_t PrepareRead(ReadOperation readOperation);

        void FinishRead(ReadOperation readOperation);

        void FinishWrite(int bytesWritten, OperationException error);
    }
}
