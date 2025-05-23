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
    using System.Runtime.CompilerServices;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    internal static class ProtocolViolationHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_InvalidCloseFrameBody(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input)
        {
            decoder.ProtocolViolation0(ctx, input, WebSocketCloseStatus.InvalidPayloadData, "Invalid close frame body");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_InvalidCloseFrameStatusCode(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input, int statusCode)
        {
            decoder.ProtocolViolation0(ctx, input, $"Invalid close frame getStatus code: {statusCode}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_MaxFrameLengthHasBeenExceeded(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input, long maxFramePayloadLength)
        {
            decoder.ProtocolViolation0(ctx, input, WebSocketCloseStatus.MessageTooBig, $"Max frame length of {maxFramePayloadLength} has been exceeded.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_InvalidDataFrameLength(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input)
        {
            decoder.ProtocolViolation0(ctx, input, "invalid data frame length (not using minimal length encoding)");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_RecNonContionuationDataFrame(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input)
        {
            decoder.ProtocolViolation0(ctx, input, "received non-continuation data frame while inside fragmented message");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_RecContionuationDataFrame(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input)
        {
            decoder.ProtocolViolation0(ctx, input, "received continuation data frame outside fragmented message");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_DataFrameUsingReservedOpcode(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input, int frameOpcode)
        {
            decoder.ProtocolViolation0(ctx, input, $"data frame using reserved opcode {frameOpcode}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_RecCloseControlFrame(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input)
        {
            decoder.ProtocolViolation0(ctx, input, "received close control frame with payload len 1");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_ControlFrameUsingReservedOpcode(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input, int frameOpcode)
        {
            decoder.ProtocolViolation0(ctx, input, $"control frame using reserved opcode {frameOpcode}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_ControlFrameWithPayloadLength125Octets(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input)
        {
            decoder.ProtocolViolation0(ctx, input, "control frame with payload length > 125 octets");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_FragmentedControlFrame(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input)
        {
            decoder.ProtocolViolation0(ctx, input, "fragmented control frame");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_RecAFrameThatIsNotMaskedAsExected(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input)
        {
            decoder.ProtocolViolation0(ctx, input, "received a frame that is not masked as expected");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProtocolViolation_RSVNoExtensionNegotiated(this WebSocket08FrameDecoder decoder, IChannelHandlerContext ctx, IByteBuffer input, int frameRsv)
        {
            decoder.ProtocolViolation0(ctx, input, $"RSV != 0 and no extension negotiated, RSV:{frameRsv}");
        }
    }
}
