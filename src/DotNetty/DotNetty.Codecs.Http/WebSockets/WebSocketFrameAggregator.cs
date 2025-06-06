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
    using DotNetty.Codecs;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Handler that aggregate fragmented <see cref="WebSocketFrame"/>'s.
    /// 
    /// Be aware if PING/PONG/CLOSE frames are send in the middle of a fragmented <see cref="WebSocketFrame"/> they will
    /// just get forwarded to the next handler in the pipeline.
    /// </summary>
    public class WebSocketFrameAggregator : MessageAggregator<WebSocketFrame, WebSocketFrame, ContinuationWebSocketFrame, WebSocketFrame>
    {
        /// <summary>Creates a new instance</summary>
        /// <param name="maxContentLength">If the size of the aggregated frame exceeds this value,
        /// a <see cref="TooLongFrameException"/> is thrown.</param>
        public WebSocketFrameAggregator(int maxContentLength)
            : base(maxContentLength)
        {
        }

        /// <inheritdoc />
        protected override bool IsStartMessage(WebSocketFrame msg)
        {
            switch (msg.Opcode)
            {
                case Opcode.Text:
                case Opcode.Binary:
                    return true;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        protected override bool IsContentMessage(WebSocketFrame msg) => msg.Opcode == Opcode.Cont;

        /// <inheritdoc />
        protected override bool IsLastContentMessage(ContinuationWebSocketFrame msg) => msg.Opcode == Opcode.Cont && msg.IsFinalFragment;

        /// <inheritdoc />
        protected override bool IsAggregated(WebSocketFrame msg)
        {
            switch (msg.Opcode)
            {
                case Opcode.Text:
                case Opcode.Binary:
                    return msg.IsFinalFragment;
                case Opcode.Cont:
                    return false;
                default:
                    return true;
            }
            //if (msg.IsFinalFragment) { return msg.Opcode != Opcode.Cont; }
            //return !this.IsStartMessage(msg) && msg.Opcode != Opcode.Cont;
        }

        /// <inheritdoc />
        protected override bool IsContentLengthInvalid(WebSocketFrame start, int maxContentLength) => false;

        /// <inheritdoc />
        protected override object NewContinueResponse(WebSocketFrame start, int maxContentLength, IChannelPipeline pipeline) => null;

        /// <inheritdoc />
        protected override bool CloseAfterContinueResponse(object msg) => throw new NotSupportedException();

        /// <inheritdoc />
        protected override bool IgnoreContentAfterContinueResponse(object msg) => throw new NotSupportedException();

        /// <inheritdoc />
        protected override WebSocketFrame BeginAggregation(WebSocketFrame start, IByteBuffer content) => start.Opcode switch
        {
            Opcode.Text => new TextWebSocketFrame(true, start.Rsv, content),
            Opcode.Binary => new BinaryWebSocketFrame(true, start.Rsv, content),
            _ => ThrowHelper.ThrowException_UnkonwFrameType(),// Should not reach here.
        };

        /// <inheritdoc />
        protected override void Decode(IChannelHandlerContext context, WebSocketFrame message, List<object> output)
        {
            switch (message.Opcode)
            {
                case Opcode.Text:
                case Opcode.Binary:
                    _handlingOversizedMessage = false;
                    if (_currentMessage is object)
                    {
                        _ = _currentMessage.Release();
                        _currentMessage = default;

                        ThrowHelper.ThrowMessageAggregationException_StartMessage();
                    }

                    // A streamed message - initialize the cumulative buffer, and wait for incoming chunks.
                    CompositeByteBuffer content0 = context.Allocator.CompositeBuffer(MaxCumulationBufferComponents);
                    AppendPartialContent(content0, message.Content);
                    _currentMessage = BeginAggregation(message, content0);
                    break;

                case Opcode.Cont:
                    if (_currentMessage is null)
                    {
                        // it is possible that a TooLongFrameException was already thrown but we can still discard data
                        // until the begging of the next request/response.
                        return;
                    }

                    // Merge the received chunk into the content of the current message.
                    var content = (CompositeByteBuffer)_currentMessage.Content;

                    var contMsg = (ContinuationWebSocketFrame)message;

                    // Handle oversized message.
                    if (content.ReadableBytes > MaxContentLength - contMsg.Content.ReadableBytes)
                    {
                        InvokeHandleOversizedMessage(context, _currentMessage);
                        return;
                    }

                    // Append the content of the chunk.
                    AppendPartialContent(content, contMsg.Content);

                    // Give the subtypes a chance to merge additional information such as trailing headers.
                    Aggregate(_currentMessage, contMsg);

                    if (IsLastContentMessage(contMsg))
                    {
                        FinishAggregation0(_currentMessage);

                        // All done
                        output.Add(_currentMessage);
                        _currentMessage = default;
                    }
                    break;

                default:
                    ThrowHelper.ThrowMessageAggregationException_UnknownAggregationState();
                    break;
            }
        }
    }
}
