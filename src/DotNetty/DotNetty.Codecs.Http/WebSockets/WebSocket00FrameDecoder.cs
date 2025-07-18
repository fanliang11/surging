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
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    using static Buffers.ByteBufferUtil;

    /// <summary>
    /// Decodes <see cref="IByteBuffer"/>s into <see cref="WebSocketFrame"/>s.
    /// <para>For the detailed instruction on adding add Web Socket support to your HTTP server, take a look into the
    /// <tt>WebSocketServer</tt> example located in the {@code examples.http.websocket} package.</para>
    /// </summary>
    public class WebSocket00FrameDecoder : ReplayingDecoder, IWebSocketFrameDecoder
    {
        private const int DefaultMaxFrameSize = 16384;

        private readonly long _maxFrameSize;
        private bool _receivedClosingHandshake;

        public WebSocket00FrameDecoder() : this(DefaultMaxFrameSize)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="IWebSocketFrameDecoder"/> with the specified <paramref name="maxFrameSize"/>. If the client
        /// sends a frame size larger than <paramref name="maxFrameSize"/>, the channel will be closed.
        /// </summary>
        /// <param name="maxFrameSize">the maximum frame size to decode</param>
        public WebSocket00FrameDecoder(int maxFrameSize) : base()
        {
            _maxFrameSize = maxFrameSize;
        }

        public WebSocket00FrameDecoder(WebSocketDecoderConfig decoderConfig) : base()
        {
            if (decoderConfig is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.decoderConfig); }
            _maxFrameSize = decoderConfig.MaxFramePayloadLength;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            // Discard all data received if closing handshake was received before.
            if (_receivedClosingHandshake)
            {
                _ = input.SkipBytes(ActualReadableBytes);
                return;
            }

            // Decode a frame otherwise.
            byte type = input.ReadByte();
            WebSocketFrame frame;
            if ((type & 0x80) == 0x80)
            {
                // If the MSB on type is set, decode the frame length
                frame = DecodeBinaryFrame(context, type, input);
            }
            else
            {
                // Decode a 0xff terminated UTF-8 string
                frame = DecodeTextFrame(context, input);
            }

            if (frame is object)
            {
                output.Add(frame);
            }
        }

        WebSocketFrame DecodeBinaryFrame(IChannelHandlerContext ctx, byte type, IByteBuffer buffer)
        {
            long frameSize = 0;
            int lengthFieldSize = 0;
            byte b;
            do
            {
                b = buffer.ReadByte();
                frameSize <<= 7;
                frameSize |= (uint)(b & 0x7f);
                if (frameSize > _maxFrameSize)
                {
                    ThrowHelper.ThrowTooLongFrameException_WebSocket00FrameDecoder();
                }
                lengthFieldSize++;
                if (lengthFieldSize > 8)
                {
                    // Perhaps a malicious peer?
                    ThrowHelper.ThrowTooLongFrameException_WebSocket00FrameDecoder();
                }
            } while ((b & 0x80) == 0x80);

            if (type == 0xFF && 0ul >= (ulong)frameSize)
            {
                _receivedClosingHandshake = true;
                return new CloseWebSocketFrame(true, 0, ctx.Allocator.Buffer(0));
            }
            IByteBuffer payload = ReadBytes(ctx.Allocator, buffer, (int)frameSize);
            return new BinaryWebSocketFrame(payload);
        }

        WebSocketFrame DecodeTextFrame(IChannelHandlerContext ctx, IByteBuffer buffer)
        {
            int ridx = buffer.ReaderIndex;
            int rbytes = ActualReadableBytes;
            int delimPos = buffer.IndexOf(ridx, ridx + rbytes, 0xFF);
            if ((uint)delimPos > SharedConstants.TooBigOrNegative) // == -1
            {
                // Frame delimiter (0xFF) not found
                if (rbytes > _maxFrameSize)
                {
                    // Frame length exceeded the maximum
                    ThrowHelper.ThrowTooLongFrameException_WebSocket00FrameDecoder();
                }
                else
                {
                    // Wait until more data is received
                    return null;
                }
            }

            int frameSize = delimPos - ridx;
            if (frameSize > _maxFrameSize)
            {
                ThrowHelper.ThrowTooLongFrameException_WebSocket00FrameDecoder();
            }

            IByteBuffer binaryData = ReadBytes(ctx.Allocator, buffer, frameSize);
            _ = buffer.SkipBytes(1);

            var endIndex = binaryData.WriterIndex;
            int ffDelimPos = binaryData.IndexOf(binaryData.ReaderIndex, endIndex, 0xFF);
            if ((uint)endIndex >= (uint)ffDelimPos) // ffDelimPos >= 0
            {
                _ = binaryData.Release();
                ThrowHelper.ThrowArgumentException_TextFrame();
            }

            return new TextWebSocketFrame(binaryData);
        }
    }
}
