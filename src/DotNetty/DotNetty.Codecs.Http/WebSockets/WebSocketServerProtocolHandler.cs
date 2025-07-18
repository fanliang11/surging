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

namespace DotNetty.Codecs.Http.WebSockets
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    using static HttpVersion;

    /// <summary>
    /// This handler does all the heavy lifting for you to run a websocket server.
    ///
    /// It takes care of websocket handshaking as well as processing of control frames (Close, Ping, Pong). Text and Binary
    /// data frames are passed to the next handler in the pipeline (implemented by you) for processing.
    ///
    /// See <tt>io.netty.example.http.websocketx.html5.WebSocketServer</tt> for usage.
    ///
    /// The implementation of this handler assumes that you just want to run  a websocket server and not process other types
    /// HTTP requests (like GET and POST). If you wish to support both HTTP requests and websockets in the one server, refer
    /// to the <tt>io.netty.example.http.websocketx.server.WebSocketServer</tt> example.
    ///
    /// To know once a handshake was done you can intercept the
    /// <see cref="IChannelHandler.UserEventTriggered(IChannelHandlerContext, object)"/> and check if the event was instance
    /// of <see cref="HandshakeComplete"/>, the event will contain extra information about the handshake such as the request and
    /// selected subprotocol.
    /// </summary>
    public class WebSocketServerProtocolHandler : WebSocketProtocolHandler
    {
        /// <summary>
        /// Events that are fired to notify about handshake status
        /// </summary>
        public enum ServerHandshakeStateEvent
        {
            /// <summary>
            /// The Handshake was completed successfully and the channel was upgraded to websockets.
            /// </summary>
            HandshakeComplete,

            /// <summary>
            /// The Handshake was timed out
            /// </summary>
            HandshakeTimeout,
        }

        public sealed class HandshakeComplete
        {
            private readonly string _requestUri;
            private readonly HttpHeaders _requestHeaders;
            private readonly string _selectedSubprotocol;

            internal HandshakeComplete(string requestUri, HttpHeaders requestHeaders, string selectedSubprotocol)
            {
                _requestUri = requestUri;
                _requestHeaders = requestHeaders;
                _selectedSubprotocol = selectedSubprotocol;
            }

            public string RequestUri => _requestUri;

            public HttpHeaders RequestHeaders => _requestHeaders;

            public string SelectedSubprotocol => _selectedSubprotocol;
        }

        private static readonly AttributeKey<WebSocketServerHandshaker> HandshakerAttrKey =
            AttributeKey<WebSocketServerHandshaker>.ValueOf("HANDSHAKER");

        private readonly WebSocketServerProtocolConfig _serverConfig;

        public WebSocketServerProtocolHandler(WebSocketServerProtocolConfig serverConfig)
            : base(CheckNotNull(serverConfig).DropPongFrames, serverConfig.SendCloseFrame, serverConfig.ForceCloseTimeoutMillis)
        {
            _serverConfig = serverConfig;
        }

        private static WebSocketServerProtocolConfig CheckNotNull(WebSocketServerProtocolConfig serverConfig)
        {
            if (serverConfig is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.serverConfig); }
            return serverConfig;
        }

        public WebSocketServerProtocolHandler(string websocketPath, bool enableUtf8Validator = true)
            : this(websocketPath, WebSocketServerProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : this(websocketPath, false, handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, bool checkStartsWith, bool enableUtf8Validator = true)
            : this(websocketPath, checkStartsWith, WebSocketServerProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, bool checkStartsWith, long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : this(websocketPath, null, false, 65536, false, checkStartsWith, handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols, bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, WebSocketServerProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols, long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, false, handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols, bool allowExtensions, bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, allowExtensions, WebSocketServerProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols, bool allowExtensions,
            long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, allowExtensions, 65536, handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols,
            bool allowExtensions, int maxFrameSize, bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, allowExtensions, maxFrameSize, WebSocketServerProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols,
            bool allowExtensions, int maxFrameSize, long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, allowExtensions, maxFrameSize, false, handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols,
                bool allowExtensions, int maxFrameSize, bool allowMaskMismatch, bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, allowExtensions, maxFrameSize, allowMaskMismatch,
                 WebSocketServerProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols, bool allowExtensions,
                                              int maxFrameSize, bool allowMaskMismatch, long handshakeTimeoutMillis,
                                              bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, allowExtensions, maxFrameSize, allowMaskMismatch, false,
                 handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols,
                                              bool allowExtensions, int maxFrameSize, bool allowMaskMismatch,
                                              bool checkStartsWith, bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, allowExtensions, maxFrameSize, allowMaskMismatch, checkStartsWith,
                 WebSocketServerProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols,
                                              bool allowExtensions, int maxFrameSize, bool allowMaskMismatch,
                                              bool checkStartsWith, long handshakeTimeoutMillis, bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, allowExtensions, maxFrameSize, allowMaskMismatch, checkStartsWith, true,
                 handshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols,
                                              bool allowExtensions, int maxFrameSize, bool allowMaskMismatch,
                                              bool checkStartsWith, bool dropPongFrames, bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, allowExtensions, maxFrameSize, allowMaskMismatch, checkStartsWith,
                 dropPongFrames, WebSocketServerProtocolConfig.DefaultHandshakeTimeoutMillis, enableUtf8Validator)
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols, bool allowExtensions,
                                              int maxFrameSize, bool allowMaskMismatch, bool checkStartsWith,
                                              bool dropPongFrames, long handshakeTimeoutMillis,
                                              bool enableUtf8Validator = true)
            : this(websocketPath, subprotocols, checkStartsWith, dropPongFrames, handshakeTimeoutMillis,
                WebSocketDecoderConfig.NewBuilder()
                    .MaxFramePayloadLength(maxFrameSize)
                    .AllowMaskMismatch(allowMaskMismatch)
                    .AllowExtensions(allowExtensions)
                    .WithUTF8Validator(enableUtf8Validator)
                    .Build())
        {
        }

        public WebSocketServerProtocolHandler(string websocketPath, string subprotocols, bool checkStartsWith,
                                              bool dropPongFrames, long handshakeTimeoutMillis,
                                              WebSocketDecoderConfig decoderConfig)
            : this(WebSocketServerProtocolConfig.NewBuilder()
                .WebsocketPath(websocketPath)
                .Subprotocols(subprotocols)
                .CheckStartsWith(checkStartsWith)
                .HandshakeTimeoutMillis(handshakeTimeoutMillis)
                .DropPongFrames(dropPongFrames)
                .DecoderConfig(decoderConfig)
                .Build())
        {
        }

        public override void HandlerAdded(IChannelHandlerContext ctx)
        {
            IChannelPipeline cp = ctx.Pipeline;
            if (cp.Get<WebSocketServerProtocolHandshakeHandler>() is null)
            {
                // Add the WebSocketHandshakeHandler before this one.
                _ = cp.AddBefore(ctx.Name, nameof(WebSocketServerProtocolHandshakeHandler),
                    new WebSocketServerProtocolHandshakeHandler(_serverConfig));
            }

            if (_serverConfig.DecoderConfig.WithUTF8Validator && cp.Get<Utf8FrameValidator>() is null)
            {
                // Add the UFT8 checking before this one.
                _ = cp.AddBefore(ctx.Name, nameof(Utf8FrameValidator), new Utf8FrameValidator());
            }
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

                case Opcode.Close when _serverConfig.HandleCloseFrames:
                    WebSocketServerHandshaker handshaker = GetHandshaker(ctx.Channel);
                    if (handshaker is object)
                    {
                        _ = frame.Retain();
                        _ = handshaker.CloseAsync(ctx.Channel, (CloseWebSocketFrame)frame);
                    }
                    else
                    {
                        _ = ctx.WriteAndFlushAsync(Unpooled.Empty).CloseOnComplete(ctx);
                    }

                    return;

                default: // 从 WebSocketProtocolHandler.Decode 直接复制
                    output.Add(frame.Retain());
                    break;
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            if (cause is WebSocketHandshakeException)
            {
                var response = new DefaultFullHttpResponse(Http11, HttpResponseStatus.BadRequest,
                    Unpooled.WrappedBuffer(Encoding.ASCII.GetBytes(cause.Message)));
                _ = ctx.Channel.WriteAndFlushAsync(response).CloseOnComplete(ctx);
            }
            else
            {
                _ = ctx.FireExceptionCaught(cause);
                _ = ctx.CloseAsync();
            }
        }

        internal static WebSocketServerHandshaker GetHandshaker(IChannel channel) => channel.GetAttribute(HandshakerAttrKey).Get();

        internal static void SetHandshaker(IChannel channel, WebSocketServerHandshaker handshaker) => channel.GetAttribute(HandshakerAttrKey).Set(handshaker);
    }
}
