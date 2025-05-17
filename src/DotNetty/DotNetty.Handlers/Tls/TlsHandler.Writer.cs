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

namespace DotNetty.Handlers.Tls
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Net.Security;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
    using System.Runtime.InteropServices;
#endif

    partial class TlsHandler
    {
        private IPromise _lastContextWritePromise;
        private volatile int v_wrapDataSize = TlsUtils.MAX_PLAINTEXT_LENGTH;

        /// <summary>
        /// Gets or Sets the number of bytes to pass to each <see cref="SslStream.Write(byte[], int, int)"/> call.
        /// </summary>
        /// <remarks>
        /// This value will partition data which is passed to write
        /// <see cref="Write(IChannelHandlerContext, object, IPromise)"/> The partitioning will work as follows:
        /// <ul>
        /// <li>If <code>wrapDataSize &lt;= 0</code> then we will write each data chunk as is.</li>
        /// <li>If <code>wrapDataSize > data size</code> then we will attempt to aggregate multiple data chunks together.</li>
        /// <li>Else if <code>wrapDataSize &lt;= data size</code> then we will divide the data into chunks of <c>wrapDataSize</c> when writing.</li>
        /// </ul>
        /// </remarks>
        public int WrapDataSize
        {
            get => v_wrapDataSize;
            set => v_wrapDataSize = value;
        }

        public override void Write(IChannelHandlerContext context, object message, IPromise promise)
        {
            if (message is not IByteBuffer buf)
            {
                InvalidMessage(message, promise);
                return;
            }
            if (_pendingUnencryptedWrites is object)
            {
                _pendingUnencryptedWrites.Add(buf, promise);
            }
            else
            {
                ReferenceCountUtil.SafeRelease(buf);
                _ = promise.TrySetException(NewPendingWritesNullException());
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InvalidMessage(object message, IPromise promise)
        {
            ReferenceCountUtil.SafeRelease(message);
            _ = promise.TrySetException(ThrowHelper.GetUnsupportedMessageTypeException(message));
        }

        public override void Flush(IChannelHandlerContext context)
        {
            try
            {
                WrapAndFlush(context);
            }
            catch (Exception cause)
            {
                // Fail pending writes.
                HandleFailure(context, cause, true, false, true);
                ExceptionDispatchInfo.Capture(cause).Throw();
            }
        }

        private void Flush(IChannelHandlerContext ctx, IPromise promise)
        {
            if (_pendingUnencryptedWrites is object)
            {
                _pendingUnencryptedWrites.Add(Unpooled.Empty, promise);
            }
            else
            {
                _ = promise.TrySetException(NewPendingWritesNullException());
            }
            Flush(ctx);
        }

        private void WrapAndFlush(IChannelHandlerContext context)
        {
            if (_pendingUnencryptedWrites.IsEmpty())
            {
                // It's important to NOT use a voidPromise here as the user
                // may want to add a ChannelFutureListener to the ChannelPromise later.
                //
                // See https://github.com/netty/netty/issues/3364
                _pendingUnencryptedWrites.Add(Unpooled.Empty, context.NewPromise());
            }

            if (!EnsureAuthenticationCompleted(context))
            {
                State |= TlsHandlerState.FlushedBeforeHandshake;
                return;
            }

            try
            {
                Wrap(context);
            }
            finally
            {
                // We may have written some parts of data before an exception was thrown so ensure we always flush.
                _ = context.Flush();
            }
        }

        private void Wrap(IChannelHandlerContext context)
        {
            Debug.Assert(context == CapturedContext);

            IByteBufferAllocator alloc = context.Allocator;
            IByteBuffer buf = null;
            try
            {
                int wrapDataSize = v_wrapDataSize;
                // Only continue to loop if the handler was not removed in the meantime.
                // See https://github.com/netty/netty/issues/5860
                while (!context.IsRemoved)
                {
                    var promise = context.NewPromise();
                    buf = wrapDataSize > 0
                        ? _pendingUnencryptedWrites.Remove(alloc, wrapDataSize, promise)
                        : _pendingUnencryptedWrites.RemoveFirst(promise);
                    if (buf is null) { break; }

                    try
                    {
                        var readableBytes = buf.ReadableBytes;
                        if (buf is CompositeByteBuffer composite && !composite.IsSingleIoBuffer)
                        {
                            buf = context.Allocator.Buffer(readableBytes);
                            _ = composite.ReadBytes(buf, readableBytes);
                            composite.Release();
                        }
                        _lastContextWritePromise = promise;
                        _ = buf.ReadBytes(_sslStream, readableBytes); // this leads to FinishWrap being called 0+ times
                    }
                    catch (Exception exc)
                    {
                        promise.TrySetException(exc);
                        // SslStream has been closed already.
                        // Any further write attempts should be denied.
                        _pendingUnencryptedWrites?.ReleaseAndFailAll(exc);
                        throw;
                    }
                    finally
                    {
                        buf.Release();
                        buf = null;
                        promise = null;
                        _lastContextWritePromise = null;
                    }
                }
            }
            finally
            {
                // Ownership of buffer was not transferred, release it.
                buf?.Release();
            }
        }

#if NETCOREAPP || NETSTANDARD_2_0_GREATER
        private void FinishWrap(in ReadOnlySpan<byte> buffer, IPromise promise)
        {
            IByteBuffer output;
            var capturedContext = CapturedContext;
            if (buffer.IsEmpty)
            {
                output = Unpooled.Empty;
            }
            else
            {
                var bufLen = buffer.Length;
                output = capturedContext.Allocator.Buffer(bufLen);
                buffer.CopyTo(output.FreeSpan);
                output.Advance(bufLen);
            }

            _ = capturedContext.WriteAsync(output, promise);
        }
#endif

        private void FinishWrap(byte[] buffer, int offset, int count, IPromise promise)
        {
            IByteBuffer output;
            var capturedContext = CapturedContext;
            if (0u >= (uint)count)
            {
                output = Unpooled.Empty;
            }
            else
            {
                output = capturedContext.Allocator.Buffer(count);
                _ = output.WriteBytes(buffer, offset, count);
            }

            _ = capturedContext.WriteAsync(output, promise);
        }

#if NETCOREAPP || NETSTANDARD_2_0_GREATER
        private Task FinishWrapNonAppDataAsync(in ReadOnlyMemory<byte> buffer, IPromise promise)
        {
            var capturedContext = CapturedContext;
            Task future;
            if (MemoryMarshal.TryGetArray(buffer, out var seg))
            {
                future = capturedContext.WriteAndFlushAsync(Unpooled.WrappedBuffer(seg.Array, seg.Offset, seg.Count), promise);
            }
            else
            {
                future = capturedContext.WriteAndFlushAsync(Unpooled.WrappedBuffer(buffer.ToArray()), promise);
            }
            this.ReadIfNeeded(capturedContext);
            return future;
        }
#endif

        private Task FinishWrapNonAppDataAsync(byte[] buffer, int offset, int count, IPromise promise)
        {
            var capturedContext = CapturedContext;
            var future = capturedContext.WriteAndFlushAsync(Unpooled.WrappedBuffer(buffer, offset, count), promise);
            this.ReadIfNeeded(capturedContext);
            return future;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException NewPendingWritesNullException()
        {
            return new InvalidOperationException("pendingUnencryptedWrites is null, handlerRemoved0 called?");
        }
    }
}
