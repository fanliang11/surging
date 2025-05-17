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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

using System;

namespace DotNetty.Codecs.Http.WebSockets
{
    /// <summary>
    /// An <see cref="DecoderException"/> which is thrown when the received <see cref="WebSocketFrame"/> data could not be decoded by
    /// an inbound handler.
    /// </summary>
    public class CorruptedWebSocketFrameException : CorruptedFrameException
    {
        public CorruptedWebSocketFrameException()
            : this(WebSocketCloseStatus.ProtocolError, null, null)
        {
        }

        public CorruptedWebSocketFrameException(WebSocketCloseStatus status, string message)
            : this(status, message, null)
        {
        }

        public CorruptedWebSocketFrameException(WebSocketCloseStatus status, Exception cause)
            : this(status, null, cause)
        {
        }

        public CorruptedWebSocketFrameException(WebSocketCloseStatus status, string message, Exception cause)
            : base(message is null ? status.ReasonText.ToString() : message, cause)
        {
            CloseStatus = status;
        }

        public WebSocketCloseStatus CloseStatus { get; }
    }
}
