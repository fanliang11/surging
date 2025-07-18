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
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.WebSockets
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    using static Buffers.ByteBufferUtil;

    /// <summary>
    /// Decodes a web socket frame from wire protocol version 8 format. This code was forked from <a
    /// href="https://github.com/joewalnes/webbit">webbit</a> and modified.
    /// </summary>
    public class WebSocket08FrameDecoder : ByteToMessageDecoder, IWebSocketFrameDecoder
    {
        enum State
        {
            ReadingFirst,
            ReadingSecond,
            ReadingSize,
            MaskingKey,
            Payload,
            Corrupt
        }

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<WebSocket08FrameDecoder>();

        const byte OpcodeCont = 0x0;
        const byte OpcodeText = 0x1;
        const byte OpcodeBinary = 0x2;
        const byte OpcodeClose = 0x8;
        const byte OpcodePing = 0x9;
        const byte OpcodePong = 0xA;

        private readonly WebSocketDecoderConfig _config;

        int fragmentedFramesCount;
        bool frameFinalFlag;
        bool frameMasked;
        int frameRsv;
        int frameOpcode;
        long framePayloadLength;
        byte[] maskingKey;
        int framePayloadLen1;
        bool receivedClosingHandshake;
        State state = State.ReadingFirst;

        public WebSocket08FrameDecoder(bool expectMaskedFrames, bool allowExtensions, int maxFramePayloadLength)
            : this(expectMaskedFrames, allowExtensions, maxFramePayloadLength, false)
        {
        }

        public WebSocket08FrameDecoder(bool expectMaskedFrames, bool allowExtensions, int maxFramePayloadLength, bool allowMaskMismatch)
            : this(WebSocketDecoderConfig.NewBuilder()
                .ExpectMaskedFrames(expectMaskedFrames)
                .AllowExtensions(allowExtensions)
                .MaxFramePayloadLength(maxFramePayloadLength)
                .AllowMaskMismatch(allowMaskMismatch)
                .Build())
        {
        }
        public WebSocket08FrameDecoder(WebSocketDecoderConfig decoderConfig)
        {
            if (decoderConfig is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.decoderConfig); }

            _config = decoderConfig;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            // Discard all data received if closing handshake was received before.
            if (this.receivedClosingHandshake)
            {
                _ = input.SkipBytes(this.ActualReadableBytes);
                return;
            }

            switch (this.state)
            {
                case State.ReadingFirst:
                    if (!input.IsReadable())
                    {
                        return;
                    }

                    this.framePayloadLength = 0;

                    // FIN, RSV, OPCODE
                    byte b = input.ReadByte();
                    this.frameFinalFlag = (b & 0x80) != 0;
                    this.frameRsv = (b & 0x70) >> 4;
                    this.frameOpcode = b & 0x0F;

#if DEBUG
                    if (Logger.TraceEnabled)
                    {
                        Logger.DecodingWebSocketFrameOpCode(this.frameOpcode);
                    }
#endif

                    this.state = State.ReadingSecond;
                    goto case State.ReadingSecond;
                case State.ReadingSecond:
                    if (!input.IsReadable())
                    {
                        return;
                    }

                    // MASK, PAYLOAD LEN 1
                    b = input.ReadByte();
                    this.frameMasked = (b & 0x80) != 0;
                    this.framePayloadLen1 = b & 0x7F;

                    if (this.frameRsv != 0 && !_config.AllowExtensions)
                    {
                        this.ProtocolViolation_RSVNoExtensionNegotiated(context, input, this.frameRsv);
                        return;
                    }

                    if (!_config.AllowMaskMismatch && _config.ExpectMaskedFrames != this.frameMasked)
                    {
                        this.ProtocolViolation_RecAFrameThatIsNotMaskedAsExected(context, input);
                        return;
                    }

                    // control frame (have MSB in opcode set)
                    if (this.frameOpcode > 7)
                    {
                        // control frames MUST NOT be fragmented
                        if (!this.frameFinalFlag)
                        {
                            this.ProtocolViolation_FragmentedControlFrame(context, input);
                            return;
                        }

                        // control frames MUST have payload 125 octets or less
                        if (this.framePayloadLen1 > 125)
                        {
                            this.ProtocolViolation_ControlFrameWithPayloadLength125Octets(context, input);
                            return;
                        }

                        switch (this.frameOpcode)
                        {
                            // close frame : if there is a body, the first two bytes of the
                            // body MUST be a 2-byte unsigned integer (in network byte
                            // order) representing a getStatus code
                            case OpcodeClose:
                                if (this.framePayloadLen1 == 1)
                                {
                                    this.ProtocolViolation_RecCloseControlFrame(context, input);
                                    return;
                                }
                                break;

                            case OpcodePing:
                            case OpcodePong:
                                break;

                            // check for reserved control frame opcodes
                            default:
                                this.ProtocolViolation_ControlFrameUsingReservedOpcode(context, input, this.frameOpcode);
                                return;
                        }
                    }
                    else // data frame
                    {
                        switch (this.frameOpcode)
                        {
                            case OpcodeCont:
                            case OpcodeText:
                            case OpcodeBinary:
                                break;
                            // check for reserved data frame opcodes
                            default:
                                this.ProtocolViolation_DataFrameUsingReservedOpcode(context, input, this.frameOpcode);
                                return;
                        }

                        uint uFragmentedFramesCount = (uint)this.fragmentedFramesCount;
                        // check opcode vs message fragmentation state 1/2
                        if (0u >= uFragmentedFramesCount && this.frameOpcode == OpcodeCont)
                        {
                            this.ProtocolViolation_RecContionuationDataFrame(context, input);
                            return;
                        }

                        // check opcode vs message fragmentation state 2/2
                        if (uFragmentedFramesCount > 0u && this.frameOpcode != OpcodeCont && this.frameOpcode != OpcodePing)
                        {
                            this.ProtocolViolation_RecNonContionuationDataFrame(context, input);
                            return;
                        }
                    }

                    this.state = State.ReadingSize;
                    goto case State.ReadingSize;
                case State.ReadingSize:
                    // Read frame payload length
                    switch (this.framePayloadLen1)
                    {
                        case 126:
                            if (input.ReadableBytes < 2)
                            {
                                return;
                            }
                            this.framePayloadLength = input.ReadUnsignedShort();
                            if (this.framePayloadLength < 126)
                            {
                                this.ProtocolViolation_InvalidDataFrameLength(context, input);
                                return;
                            }
                            break;

                        case 127:
                            if (input.ReadableBytes < 8)
                            {
                                return;
                            }
                            this.framePayloadLength = input.ReadLong();
                            // TODO: check if it's bigger than 0x7FFFFFFFFFFFFFFF, Maybe
                            // just check if it's negative?

                            if (this.framePayloadLength < 65536)
                            {
                                this.ProtocolViolation_InvalidDataFrameLength(context, input);
                                return;
                            }
                            break;

                        default:
                            this.framePayloadLength = this.framePayloadLen1;
                            break;
                    }

                    var maxFramePayloadLength = _config.MaxFramePayloadLength;
                    if (this.framePayloadLength > maxFramePayloadLength)
                    {
                        this.ProtocolViolation_MaxFrameLengthHasBeenExceeded(context, input, maxFramePayloadLength);
                        return;
                    }

#if DEBUG
                    if (Logger.TraceEnabled)
                    {
                        Logger.DecodingWebSocketFrameLength(this.framePayloadLength);
                    }
#endif

                    this.state = State.MaskingKey;
                    goto case State.MaskingKey;
                case State.MaskingKey:
                    if (this.frameMasked)
                    {
                        if (input.ReadableBytes < 4)
                        {
                            return;
                        }
                        if (this.maskingKey is null)
                        {
                            this.maskingKey = new byte[4];
                        }
                        _ = input.ReadBytes(this.maskingKey);
                    }
                    this.state = State.Payload;
                    goto case State.Payload;
                case State.Payload:
                    if (input.ReadableBytes < this.framePayloadLength)
                    {
                        return;
                    }

                    IByteBuffer payloadBuffer = null;
                    try
                    {
                        payloadBuffer = ReadBytes(context.Allocator, input, ToFrameLength(this.framePayloadLength));

                        // Now we have all the data, the next checkpoint must be the next
                        // frame
                        this.state = State.ReadingFirst;

                        // Unmask data if needed
                        if (this.frameMasked)
                        {
                            this.Unmask(payloadBuffer);
                        }

                        // Processing ping/pong/close frames because they cannot be
                        // fragmented
                        switch (this.frameOpcode)
                        {
                            case OpcodePing:
                                output.Add(new PingWebSocketFrame(this.frameFinalFlag, this.frameRsv, payloadBuffer));
                                payloadBuffer = null;
                                return;

                            case OpcodePong:
                                output.Add(new PongWebSocketFrame(this.frameFinalFlag, this.frameRsv, payloadBuffer));
                                payloadBuffer = null;
                                return;

                            case OpcodeClose:
                                this.receivedClosingHandshake = true;
                                this.CheckCloseFrameBody(context, payloadBuffer);
                                output.Add(new CloseWebSocketFrame(this.frameFinalFlag, this.frameRsv, payloadBuffer));
                                payloadBuffer = null;
                                return;
                        }

                        // Processing for possible fragmented messages for text and binary
                        // frames
                        if (this.frameFinalFlag)
                        {
                            // Final frame of the sequence. Apparently ping frames are
                            // allowed in the middle of a fragmented message
                            if (this.frameOpcode != OpcodePing)
                            {
                                this.fragmentedFramesCount = 0;
                            }
                        }
                        else
                        {
                            // Increment counter
                            this.fragmentedFramesCount++;
                        }

                        // Return the frame
                        switch (this.frameOpcode)
                        {
                            case OpcodeText:
                                output.Add(new TextWebSocketFrame(this.frameFinalFlag, this.frameRsv, payloadBuffer));
                                payloadBuffer = null;
                                return;

                            case OpcodeBinary:
                                output.Add(new BinaryWebSocketFrame(this.frameFinalFlag, this.frameRsv, payloadBuffer));
                                payloadBuffer = null;
                                return;

                            case OpcodeCont:
                                output.Add(new ContinuationWebSocketFrame(this.frameFinalFlag, this.frameRsv, payloadBuffer));
                                payloadBuffer = null;
                                return;

                            default:
                                ThrowNotSupportedException(this.frameOpcode); return;
                        }
                    }
                    finally
                    {
                        _ = (payloadBuffer?.Release());
                    }
                case State.Corrupt:
                    if (input.IsReadable())
                    {
                        // If we don't keep reading Netty will throw an exception saying
                        // we can't return null if no bytes read and state not changed.
                        _ = input.ReadByte();
                    }
                    return;
                default:
                    ThrowHelper.ThrowException_FrameDecoder(); break;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowNotSupportedException(int frameOpcode)
        {
            throw GetNotSupportedException();

            NotSupportedException GetNotSupportedException()
            {
                return new NotSupportedException($"Cannot decode web socket frame with opcode: {frameOpcode}");
            }
        }

        void Unmask(IByteBuffer frame)
        {
            int i = frame.ReaderIndex;
            int end = frame.WriterIndex;

            int intMask = (this.maskingKey[0] << 24)
                  | (this.maskingKey[1] << 16)
                  | (this.maskingKey[2] << 8)
                  | this.maskingKey[3];

            for (; i + 3 < end; i += 4)
            {
                int unmasked = frame.GetInt(i) ^ intMask;
                _ = frame.SetInt(i, unmasked);
            }
            for (; i < end; i++)
            {
                _ = frame.SetByte(i, frame.GetByte(i) ^ this.maskingKey[i % 4]);
            }
        }

        internal void ProtocolViolation0(IChannelHandlerContext ctx, IByteBuffer input, string reason)
            => ProtocolViolation0(ctx, input, WebSocketCloseStatus.ProtocolError, reason);

        internal void ProtocolViolation0(IChannelHandlerContext ctx, IByteBuffer input, WebSocketCloseStatus status, string reason)
        {
            ProtocolViolation(ctx, input, new CorruptedWebSocketFrameException(status, reason));
        }

        void ProtocolViolation(IChannelHandlerContext ctx, IByteBuffer input, CorruptedWebSocketFrameException ex)
        {
            this.state = State.Corrupt;
            int readableBytes = input.ReadableBytes;
            if (readableBytes > 0)
            {
                // Fix for memory leak, caused by ByteToMessageDecoder#channelRead:
                // buffer 'cumulation' is released ONLY when no more readable bytes available.
                _ = input.SkipBytes(readableBytes);
            }
            if (ctx.Channel.IsActive && _config.CloseOnProtocolViolation)
            {
                object closeMessage;
                if (this.receivedClosingHandshake)
                {
                    closeMessage = Unpooled.Empty;
                }
                else
                {
                    WebSocketCloseStatus closeStatus = ex.CloseStatus;
                    var errMsg = ex.Message;
                    ICharSequence reasonText = !string.IsNullOrWhiteSpace(errMsg) ? new StringCharSequence(errMsg) : closeStatus.ReasonText;
                    closeMessage = new CloseWebSocketFrame(closeStatus, reasonText);
                }
                _ = ctx.WriteAndFlushAsync(closeMessage).CloseOnComplete(ctx.Channel);
            }
            ExceptionDispatchInfo.Capture(ex).Throw();
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static int ToFrameLength(long l)
        {
            if (l > int.MaxValue)
            {
                ThrowTooLongFrameException(l);
            }
            return (int)l;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowTooLongFrameException(long l)
        {
            throw GetTooLongFrameException();

            TooLongFrameException GetTooLongFrameException()
            {
                return new TooLongFrameException(string.Format("Length: {0}", l));
            }
        }

        protected void CheckCloseFrameBody(IChannelHandlerContext ctx, IByteBuffer buffer)
        {
            if (buffer is null || !buffer.IsReadable())
            {
                return;
            }
            if (buffer.ReadableBytes == 1)
            {
                this.ProtocolViolation_InvalidCloseFrameBody(ctx, buffer);
            }

            // Save reader index
            int idx = buffer.ReaderIndex;
            _ = buffer.SetReaderIndex(0);

            // Must have 2 byte integer within the valid range
            int statusCode = buffer.ReadShort();
            if (!WebSocketCloseStatus.IsValidStatusCode(statusCode))
            {
                this.ProtocolViolation_InvalidCloseFrameStatusCode(ctx, buffer, statusCode);
            }

            // May have UTF-8 message
            if (buffer.IsReadable())
            {
                try
                {
                    new Utf8Validator().Check(buffer);
                }
                catch (CorruptedWebSocketFrameException ex)
                {
                    this.ProtocolViolation(ctx, buffer, ex);
                }
            }

            // Restore reader index
            _ = buffer.SetReaderIndex(idx);
        }
    }
}
