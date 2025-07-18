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
    using DotNetty.Transport.Channels;

    /// <summary>
    /// This handler does all the heavy lifting for you to run a websocket client.
    /// <para>
    /// It takes care of websocket handshaking as well as processing of Ping, Pong frames. Text and Binary
    /// data frames are passed to the next handler in the pipeline (implemented by you) for processing.
    /// Also the close frame is passed to the next handler as you may want inspect it before close the connection if
    /// the <see cref="WebSocketClientProtocolConfig.HandleCloseFrames"/> is <c>false</c>, default is <c>true</c>.
    /// </para>
    /// <para>
    /// This implementation will establish the websocket connection once the connection to the remote server was complete.
    /// </para>
    /// <para>
    /// To know once a handshake was done you can intercept the
    /// <see cref="IChannelHandler.UserEventTriggered(IChannelHandlerContext, object)"/> and check if the event was of type
    /// <see cref="ClientHandshakeStateEvent.HandshakeIssued"/> or <see cref="ClientHandshakeStateEvent.HandshakeComplete"/>
    /// </para>
    /// </summary>
    public class WebSocketClientProtocolHandler : WebSocketProtocolHandler
    {
        private readonly WebSocketClientHandshaker _handshaker;
        private readonly WebSocketClientProtocolConfig _clientConfig;

        /// <summary>
        /// Returns the used handshaker
        /// </summary>
        public WebSocketClientHandshaker Handshaker => _handshaker;

        /// <summary>
        /// Events that are fired to notify about handshake status
        /// </summary>
        public enum ClientHandshakeStateEvent
        {
            /// <summary>
            /// The Handshake was timed out
            /// </summary>
            HandshakeTimeout,

            /// <summary>
            /// The Handshake was started but the server did not response yet to the request
            /// </summary>
            HandshakeIssued,

            /// <summary>
            /// The Handshake was complete succesful and so the channel was upgraded to websockets
            /// </summary>
            HandshakeComplete
        }

        /// <summary>Base constructor</summary>
        /// <param name="clientConfig">Client protocol configuration.</param>
        public WebSocketClientProtocolHandler(WebSocketClientProtocolConfig clientConfig)
            : base(CheckNotNull(clientConfig).DropPongFrames, clientConfig.SendCloseFrame, clientConfig.ForceCloseTimeoutMillis)
        {
            _handshaker = WebSocketClientHandshakerFactory.NewHandshaker(
                clientConfig.WebSocketUri,
                clientConfig.Version,
                clientConfig.Subprotocol,
                clientConfig.AllowExtensions,
                clientConfig.CustomHeaders,
                clientConfig.MaxFramePayloadLength,
                clientConfig.PerformMasking,
                clientConfig.AllowMaskMismatch,
                clientConfig.ForceCloseTimeoutMillis,
                clientConfig.AbsoluteUpgradeUrl
            );
            _clientConfig = clientConfig;
        }

        private static WebSocketClientProtocolConfig CheckNotNull(WebSocketClientProtocolConfig clientConfig)
        {
            if (clientConfig is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.clientConfig); }
            return clientConfig;
        }

        /// <summary>Base constructor</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol"></param>
        /// <param name="allowExtensions">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            bool enableUtf8Validator = true)
            : this(webSocketUrl, version, subprotocol,
                 allowExtensions, customHeaders, maxFramePayloadLength, WebSocketClientProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol"></param>
        /// <param name="allowExtensions">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="handshakeTimeoutMillis">Handshake timeout in mills, when handshake timeout, will trigger user
        /// event <see cref="ClientHandshakeStateEvent.HandshakeTimeout"/></param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : this(webSocketUrl, version, subprotocol,
                 allowExtensions, customHeaders, maxFramePayloadLength, WebSocketClientProtocolConfig.DefaultHandleCloseFrames, handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol"></param>
        /// <param name="allowExtensions">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="handleCloseFrames"><c>true</c> if close frames should not be forwarded and just close the channel</param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            bool handleCloseFrames, bool enableUtf8Validator = true)
            : this(webSocketUrl, version, subprotocol, allowExtensions, customHeaders, maxFramePayloadLength,
                 handleCloseFrames, WebSocketClientProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol"></param>
        /// <param name="allowExtensions">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="handleCloseFrames"><c>true</c> if close frames should not be forwarded and just close the channel</param>
        /// <param name="handshakeTimeoutMillis">Handshake timeout in mills, when handshake timeout, will trigger user
        /// event <see cref="ClientHandshakeStateEvent.HandshakeTimeout"/></param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            bool handleCloseFrames, long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : this(webSocketUrl, version, subprotocol, allowExtensions, customHeaders, maxFramePayloadLength,
                 handleCloseFrames, WebSocketClientProtocolConfig.DefaultPerformMasking, WebSocketClientProtocolConfig.DefaultAllowMaskMismatch, handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol"></param>
        /// <param name="allowExtensions">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="handleCloseFrames"><c>true</c> if close frames should not be forwarded and just close the channel</param>
        /// <param name="performMasking">Whether to mask all written websocket frames. This must be set to true in order to be fully compatible
        /// with the websocket specifications. Client applications that communicate with a non-standard server
        /// which doesn't require masking might set this to false to achieve a higher performance.</param>
        /// <param name="allowMaskMismatch">When set to true, frames which are not masked properly according to the standard will still be
        /// accepted.</param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            bool handleCloseFrames, bool performMasking, bool allowMaskMismatch,
            bool enableUtf8Validator = true)
            : this(webSocketUrl, version, subprotocol, allowExtensions, customHeaders,
                 maxFramePayloadLength, handleCloseFrames, performMasking, allowMaskMismatch,
                 WebSocketClientProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol"></param>
        /// <param name="allowExtensions">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="handleCloseFrames"><c>true</c> if close frames should not be forwarded and just close the channel</param>
        /// <param name="performMasking">Whether to mask all written websocket frames. This must be set to true in order to be fully compatible
        /// with the websocket specifications. Client applications that communicate with a non-standard server
        /// which doesn't require masking might set this to false to achieve a higher performance.</param>
        /// <param name="allowMaskMismatch">When set to true, frames which are not masked properly according to the standard will still be
        /// accepted.</param>
        /// <param name="handshakeTimeoutMillis">Handshake timeout in mills, when handshake timeout, will trigger user
        /// event <see cref="ClientHandshakeStateEvent.HandshakeTimeout"/></param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            bool handleCloseFrames, bool performMasking, bool allowMaskMismatch,
            long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : this(WebSocketClientHandshakerFactory.NewHandshaker(webSocketUrl, version, subprotocol,
                allowExtensions, customHeaders, maxFramePayloadLength, performMasking, allowMaskMismatch),
                handleCloseFrames, WebSocketClientProtocolConfig.DefaultDropPongFrames, handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="handshaker">The <see cref="WebSocketClientHandshaker"/> which will be used to issue the handshake once the connection
        /// was established to the remote peer.</param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(WebSocketClientHandshaker handshaker, bool enableUtf8Validator = true)
            : this(handshaker, WebSocketClientProtocolConfig.DefaultHandleCloseFrames, WebSocketClientProtocolConfig.DefaultDropPongFrames, WebSocketClientProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="handshaker">The <see cref="WebSocketClientHandshaker"/> which will be used to issue the handshake once the connection
        /// was established to the remote peer.</param>
        /// <param name="handshakeTimeoutMillis">Handshake timeout in mills, when handshake timeout, will trigger user
        /// event <see cref="ClientHandshakeStateEvent.HandshakeTimeout"/></param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(WebSocketClientHandshaker handshaker,
            long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : this(handshaker, WebSocketClientProtocolConfig.DefaultHandleCloseFrames, WebSocketClientProtocolConfig.DefaultDropPongFrames, handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="handshaker">The <see cref="WebSocketClientHandshaker"/> which will be used to issue the handshake once the connection
        /// was established to the remote peer.</param>
        /// <param name="handleCloseFrames"><c>true</c> if close frames should not be forwarded and just close the channel</param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(WebSocketClientHandshaker handshaker,
            bool handleCloseFrames, bool enableUtf8Validator = true)
            : this(handshaker, handleCloseFrames, WebSocketClientProtocolConfig.DefaultDropPongFrames, WebSocketClientProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="handshaker">The <see cref="WebSocketClientHandshaker"/> which will be used to issue the handshake once the connection
        /// was established to the remote peer.</param>
        /// <param name="handleCloseFrames"><c>true</c> if close frames should not be forwarded and just close the channel</param>
        /// <param name="handshakeTimeoutMillis">Handshake timeout in mills, when handshake timeout, will trigger user
        /// event <see cref="ClientHandshakeStateEvent.HandshakeTimeout"/></param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(WebSocketClientHandshaker handshaker,
            bool handleCloseFrames, long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : this(handshaker, handleCloseFrames, WebSocketClientProtocolConfig.DefaultDropPongFrames, handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="handshaker">The <see cref="WebSocketClientHandshaker"/> which will be used to issue the handshake once the connection
        /// was established to the remote peer.</param>
        /// <param name="handleCloseFrames"><c>true</c> if close frames should not be forwarded and just close the channel</param>
        /// <param name="dropPongFrames"><c>true</c> if pong frames should not be forwarded</param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(WebSocketClientHandshaker handshaker,
            bool handleCloseFrames, bool dropPongFrames, bool enableUtf8Validator = true)
            : this(handshaker, handleCloseFrames, dropPongFrames, WebSocketClientProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="handshaker">The <see cref="WebSocketClientHandshaker"/> which will be used to issue the handshake once the connection
        /// was established to the remote peer.</param>
        /// <param name="handleCloseFrames"><c>true</c> if close frames should not be forwarded and just close the channel</param>
        /// <param name="dropPongFrames"><c>true</c> if pong frames should not be forwarded</param>
        /// <param name="handshakeTimeoutMillis">Handshake timeout in mills, when handshake timeout, will trigger user
        /// event <see cref="ClientHandshakeStateEvent.HandshakeTimeout"/></param>
        /// <param name="enableUtf8Validator"></param>
        public WebSocketClientProtocolHandler(WebSocketClientHandshaker handshaker,
            bool handleCloseFrames, bool dropPongFrames, long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : base(dropPongFrames)
        {
            if (handshakeTimeoutMillis <= 0L) { ThrowHelper.ThrowArgumentException_Positive(handshakeTimeoutMillis, ExceptionArgument.handshakeTimeoutMillis); }

            _handshaker = handshaker;
            _clientConfig = WebSocketClientProtocolConfig.NewBuilder()
                .HandleCloseFrames(handleCloseFrames)
                .HandshakeTimeoutMillis(handshakeTimeoutMillis)
                .WithUTF8Validator(enableUtf8Validator)
                .Build();
        }

        protected override void Decode(IChannelHandlerContext ctx, WebSocketFrame frame, List<object> output)
        {
            switch (frame.Opcode)
            {
                case Opcode.Ping: // 从 WebSocketProtocolHandler.Decode 直接复制
                    var contect = frame.Content;
                    _ = contect.Retain();
                    _ = ctx.Channel.WriteAndFlushAsync(new PongWebSocketFrame(contect));
                    ReadIfNeeded(ctx);
                    return;

                case Opcode.Pong when DropPongFrames: // 从 WebSocketProtocolHandler.Decode 直接复制
                    // Pong frames need to get ignored
                    ReadIfNeeded(ctx);
                    return;

                case Opcode.Close when _clientConfig.HandleCloseFrames:
                    _ = ctx.CloseAsync();
                    return;

                default: // 从 WebSocketProtocolHandler.Decode 直接复制
                    output.Add(frame.Retain());
                    break;
            }
        }

        public override void HandlerAdded(IChannelHandlerContext ctx)
        {
            var cp = ctx.Pipeline;
            if (cp.Get<WebSocketClientProtocolHandshakeHandler>() is null)
            {
                // Add the WebSocketClientProtocolHandshakeHandler before this one.
                _ = cp.AddBefore(ctx.Name, nameof(WebSocketClientProtocolHandshakeHandler),
                    new WebSocketClientProtocolHandshakeHandler(_handshaker, _clientConfig.HandshakeTimeoutMillis));
            }
            if (_clientConfig.WithUTF8Validator && cp.Get<Utf8FrameValidator>() is null)
            {
                // Add the UFT8 checking before this one.
                _ = cp.AddBefore(ctx.Name, nameof(Utf8FrameValidator),
                    new Utf8FrameValidator());
            }
        }
    }
}
