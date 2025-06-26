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
    using System.IO;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    partial class TlsHandler
    {
        private const int c_fallbackReadBufferSize = 256;

        private int _packetLength;
        private bool _firedChannelRead;
        private IByteBuffer _pendingSslStreamReadBuffer;
        private Task<int> _pendingSslStreamReadFuture;

        // This is set on the first packet to figure out the framing style.
        private Framing _framing = Framing.Unknown;

        public override void Read(IChannelHandlerContext context)
        {
            var oldState = State;
            if (!oldState.HasAny(TlsHandlerState.AuthenticationCompleted))
            {
                State = oldState | TlsHandlerState.ReadRequestedBeforeAuthenticated;
            }

            _ = context.Read();
        }

        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            // Discard bytes of the cumulation buffer if needed.
            DiscardSomeReadBytes();

            ReadIfNeeded(ctx);

            _firedChannelRead = false;
            _ = ctx.FireChannelReadComplete();
        }

        private void ReadIfNeeded(IChannelHandlerContext ctx)
        {
            // if handshake is not finished yet, we need more data
            if (!ctx.Channel.Configuration.IsAutoRead && (!_firedChannelRead || !State.HasAny(TlsHandlerState.AuthenticationCompleted)))
            {
                // No auto-read used and no message was passed through the ChannelPipeline or the handshake was not completed
                // yet, which means we need to trigger the read to ensure we will not stall
                _ = ctx.Read();
            }
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            int packetLength = _packetLength;
            // If we calculated the length of the current SSL record before, use that information.
            if (packetLength > 0)
            {
                if (input.ReadableBytes < packetLength) { return; }
            }
            else
            {
                // Get the packet length and wait until we get a packets worth of data to unwrap.
                int readableBytes = input.ReadableBytes;
                if (readableBytes < TlsUtils.SSL_RECORD_HEADER_LENGTH) { return; }

                if (!State.HasAny(TlsHandlerState.AuthenticationCompleted))
                {
                    if (_framing == Framing.Unified || _framing == Framing.Unknown)
                    {
                        _framing = DetectFraming(input);
                    }
                }
                packetLength = GetFrameSize(_framing, input);
                if ((uint)packetLength > SharedConstants.TooBigOrNegative) // < 0
                {
                    HandleInvalidTlsFrameSize(context, input);
                }
                Debug.Assert(packetLength > 0);
                if (packetLength > readableBytes)
                {
                    // wait until the whole packet can be read
                    _packetLength = packetLength;
                    return;
                }
            }

            // Reset the state of this class so we can get the length of the next packet. We assume the entire packet will
            // be consumed by the SSLEngine.
            _packetLength = 0;
            try
            {
                Unwrap(context, input, input.ReaderIndex, packetLength);
                input.SkipBytes(packetLength);
                //Debug.Assert(bytesConsumed == packetLength || engine.isInboundDone() :
                //    "we feed the SSLEngine a packets worth of data: " + packetLength + " but it only consumed: " +
                //            bytesConsumed);
            }
            catch (Exception cause)
            {
                HandleUnwrapThrowable(context, cause);
            }
        }

        /// <summary>Unwraps inbound SSL records.</summary>
        private void Unwrap(IChannelHandlerContext ctx, IByteBuffer packet, int offset, int length)
        {
            bool pending = false;

            IByteBuffer outputBuffer = null;
            try
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                ReadOnlyMemory<byte> inputIoBuffer = packet.GetReadableMemory(offset, length);
                _mediationStream.SetSource(inputIoBuffer, ctx.Allocator);
#else
                ArraySegment<byte> inputIoBuffer = packet.GetIoBuffer(offset, length);
                _mediationStream.SetSource(inputIoBuffer.Array, inputIoBuffer.Offset, ctx.Allocator);
#endif
                if (!EnsureAuthenticationCompleted(ctx))
                {
                    _mediationStream.ExpandSource(length);
                    return;
                }

                _mediationStream.ExpandSource(length);

                var currentReadFuture = _pendingSslStreamReadFuture;

                if (currentReadFuture is object)
                {
                    // restoring context from previous read
                    Debug.Assert(_pendingSslStreamReadBuffer is object);

                    outputBuffer = _pendingSslStreamReadBuffer;
                    var outputBufferLength = outputBuffer.WritableBytes;

                    _pendingSslStreamReadFuture = null;
                    _pendingSslStreamReadBuffer = null;

                    // there was a read pending already, so we make sure we completed that first
                    if (currentReadFuture.IsCompleted)
                    {
                        if (currentReadFuture.IsFailure())
                        {
                            // The decryption operation failed
                            ExceptionDispatchInfo.Capture(currentReadFuture.Exception.InnerException).Throw();
                        }
                        int read = currentReadFuture.Result;
                        if (0u >= (uint)read)
                        {
                            // Stream closed
                            NotifyClosePromise(null);
                            return;
                        }

                        // Now output the result of previous read and decide whether to do an extra read on the same source or move forward
                        outputBuffer.Advance(read);
                        _firedChannelRead = true;
                        ctx.FireChannelRead(outputBuffer);

                        currentReadFuture = null;
                        outputBuffer = null;

                        if (0u >= (uint)_mediationStream.SourceReadableBytes)
                        {
                            // we just made a frame available for reading but there was already pending read so SslStream read it out to make further progress there

                            if (read < outputBufferLength)
                            {
                                // SslStream returned non-full buffer and there's no more input to go through ->
                                // typically it means SslStream is done reading current frame so we skip
                                return;
                            }

                            // we've read out `read` bytes out of current packet to fulfil previously outstanding read
                            outputBufferLength = length - read;
                            if ((uint)(outputBufferLength - 1) > SharedConstants.TooBigOrNegative) // <= 0
                            {
                                // after feeding to SslStream current frame it read out more bytes than current packet size
                                outputBufferLength = c_fallbackReadBufferSize;
                            }
                        }
                        outputBuffer = ctx.Allocator.Buffer(outputBufferLength);
                        currentReadFuture = ReadFromSslStreamAsync(outputBuffer, outputBufferLength);
                    }
                }
                else
                {
                    // there was no pending read before so we estimate buffer of `length` bytes to be sufficient
                    outputBuffer = ctx.Allocator.Buffer(length);
                    currentReadFuture = ReadFromSslStreamAsync(outputBuffer, length);
                }

                // read out the rest of SslStream's output (if any) at risk of going async
                // using FallbackReadBufferSize - buffer size we're ok to have pinned with the SslStream until it's done reading
                while (true)
                {
                    if (currentReadFuture is object)
                    {
                        if (!currentReadFuture.IsCompleted) { break; }
                        if (currentReadFuture.IsFailure())
                        {
                            // The decryption operation failed
                            ExceptionDispatchInfo.Capture(currentReadFuture.Exception.InnerException).Throw();
                        }
                        int read = currentReadFuture.Result;

                        if (0u >= (uint)read)
                        {
                            // Stream closed
                            NotifyClosePromise(null);
                            return;
                        }

                        outputBuffer.Advance(read);
                        _firedChannelRead = true;
                        ctx.FireChannelRead(outputBuffer);

                        currentReadFuture = null;
                        outputBuffer = null;
                        if (0u >= (uint)_mediationStream.SourceReadableBytes) { return; }
                    }
                    outputBuffer = ctx.Allocator.Buffer(c_fallbackReadBufferSize);
                    currentReadFuture = ReadFromSslStreamAsync(outputBuffer, c_fallbackReadBufferSize);
                }

                pending = true;
                _pendingSslStreamReadBuffer = outputBuffer;
                _pendingSslStreamReadFuture = currentReadFuture;
            }
            finally
            {
                _mediationStream.ResetSource(ctx.Allocator);
                if (!pending && outputBuffer is object)
                {
                    if (outputBuffer.IsReadable())
                    {
                        _firedChannelRead = true;
                        ctx.FireChannelRead(outputBuffer);
                    }
                    else
                    {
                        outputBuffer.SafeRelease();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void HandleInvalidTlsFrameSize(IChannelHandlerContext context, IByteBuffer input)
        {
            // Not an SSL/TLS packet
            var ex = GetNotSslRecordException(input);
            _ = input.SkipBytes(input.ReadableBytes);

            // First fail the handshake promise as we may need to have access to the SSLEngine which may
            // be released because the user will remove the SslHandler in an exceptionCaught(...) implementation.
            HandleFailure(context, ex);
            throw ex;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void HandleUnwrapThrowable(IChannelHandlerContext context, Exception cause)
        {
            try
            {
                // We should attempt to notify the handshake failure before writing any pending data. If we are in unwrap
                // and failed during the handshake process, and we attempt to wrap, then promises will fail, and if
                // listeners immediately close the Channel then we may end up firing the handshake event after the Channel
                // has been closed.
                if (_handshakePromise.TrySetException(cause))
                {
                    context.FireUserEventTriggered(new TlsHandshakeCompletionEvent(cause));
                }

                // We need to flush one time as there may be an alert that we should send to the remote peer because
                // of the SSLException reported here.
                WrapAndFlush(context);
            }
            catch (Exception exc)
            {
                if (exc is ArgumentNullException // sslstream closed
                    or IOException
                    or NotSupportedException
                    or OperationCanceledException)
                {
#if DEBUG
                    if (s_logger.DebugEnabled)
                    {
                        s_logger.Debug("SSLException during trying to call TlsHandler.Wrap(...)" +
                                " because of an previous SSLException, ignoring...", exc);
                    }
#endif
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                // ensure we always flush and close the channel.
                HandleFailure(context, cause, true, false, true);
            }
            ExceptionDispatchInfo.Capture(cause).Throw();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NotSslRecordException GetNotSslRecordException(IByteBuffer input)
        {
            return new NotSslRecordException(
                "not an SSL/TLS record: " + ByteBufferUtil.HexDump(input));
        }

        private Task<int> ReadFromSslStreamAsync(IByteBuffer outputBuffer, int outputBufferLength)
        {
            if (_sslStream is null) { return TaskUtil.Zero; }
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            Memory<byte> outlet = outputBuffer.GetMemory(outputBuffer.WriterIndex, outputBufferLength);
            return _sslStream.ReadAsync(outlet).AsTask();
#else
            ArraySegment<byte> outlet = outputBuffer.GetIoBuffer(outputBuffer.WriterIndex, outputBufferLength);
            return _sslStream.ReadAsync(outlet.Array, outlet.Offset, outlet.Count);
#endif
        }

        private static readonly Action<Task, object> s_handleReadFromSslStreamThrowableFunc = (t, s) => HandleReadFromSslStreamThrowable(t, s);
        private static void HandleReadFromSslStreamThrowable(Task task, object state)
        {
            var (owner, ctx) = ((TlsHandler, IChannelHandlerContext))state;
            if (task.IsFailure())
            {
                owner.HandleUnwrapThrowable(ctx, task.Exception.InnerException);
            }
        }
    }
}
