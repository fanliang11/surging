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
    using System.Text;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// Performs server side opening and closing handshakes for web socket specification version <a
    /// href="http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-10" >draft-ietf-hybi-thewebsocketprotocol-10</a>
    /// </summary>
    public class WebSocketServerHandshaker08 : WebSocketServerHandshaker
    {
        public const string Websocket08AcceptGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        /// <summary>Constructor specifying the destination web socket location</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath".
        /// Subsequent web socket frames will be sent to this URL.</param>
        /// <param name="subprotocols">CSV of supported protocols</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="maxFramePayloadLength">Maximum allowable frame payload length. Setting this value to your application's
        /// requirement may reduce denial of service attacks using long data frames.</param>
        public WebSocketServerHandshaker08(string webSocketUrl, string subprotocols, bool allowExtensions, int maxFramePayloadLength)
            : this(webSocketUrl, subprotocols, allowExtensions, maxFramePayloadLength, false)
        {
        }

        /// <summary>Constructor specifying the destination web socket location</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath".
        /// Subsequent web socket frames will be sent to this URL.</param>
        /// <param name="subprotocols">CSV of supported protocols</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="maxFramePayloadLength">Maximum allowable frame payload length. Setting this value to your application's
        /// requirement may reduce denial of service attacks using long data frames.</param>
        /// <param name="allowMaskMismatch">When set to true, frames which are not masked properly according to the standard will still be
        /// accepted.</param>
        public WebSocketServerHandshaker08(string webSocketUrl, string subprotocols, bool allowExtensions, int maxFramePayloadLength, bool allowMaskMismatch)
            : this(webSocketUrl, subprotocols, WebSocketDecoderConfig.NewBuilder()
                .AllowExtensions(allowExtensions)
                .MaxFramePayloadLength(maxFramePayloadLength)
                .AllowMaskMismatch(allowMaskMismatch)
                .Build())
        {
        }

        /// <summary>Constructor specifying the destination web socket location</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath".
        /// Subsequent web socket frames will be sent to this URL.</param>
        /// <param name="subprotocols">CSV of supported protocols</param>
        /// <param name="decoderConfig">Frames decoder configuration.</param>
        public WebSocketServerHandshaker08(string webSocketUrl, string subprotocols, WebSocketDecoderConfig decoderConfig)
            : base(WebSocketVersion.V08, webSocketUrl, subprotocols, decoderConfig)
        {
        }

        /// <summary>
        /// Handle the web socket handshake for the web socket specification <a href=
        /// "http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-08">HyBi version 8 to 10</a>. Version 8, 9 and
        /// 10 share the same wire protocol.
        ///
        /// <para>
        /// Browser request to the server:
        /// </para>
        ///
        /// <![CDATA[
        /// GET /chat HTTP/1.1
        /// Host: server.example.com
        /// Upgrade: websocket
        /// Connection: Upgrade
        /// Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==
        /// Sec-WebSocket-Origin: http://example.com
        /// Sec-WebSocket-Protocol: chat, superchat
        /// Sec-WebSocket-Version: 8
        /// ]]>
        ///
        /// <para>
        /// Server response:
        /// </para>
        ///
        /// <![CDATA[
        /// HTTP/1.1 101 Switching Protocols
        /// Upgrade: websocket
        /// Connection: Upgrade
        /// Sec-WebSocket-Accept: s3pPLMBiTxaQ9kYGzzhZRbK+xOo=
        /// Sec-WebSocket-Protocol: chat
        /// ]]>
        /// </summary>
        protected internal override IFullHttpResponse NewHandshakeResponse(IFullHttpRequest req, HttpHeaders headers)
        {
            if (!req.Headers.TryGet(HttpHeaderNames.SecWebsocketKey, out ICharSequence key)
                || key is null)
            {
                ThrowHelper.ThrowWebSocketHandshakeException_MissingKey();
            }

            var res = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.SwitchingProtocols,
                req.Content.Allocator.Buffer(0));

            if (headers is object)
            {
                _ = res.Headers.Add(headers);
            }

            string acceptSeed = key + Websocket08AcceptGuid;
            byte[] sha1 = WebSocketUtil.Sha1(Encoding.ASCII.GetBytes(acceptSeed));
            string accept = WebSocketUtil.Base64String(sha1);

#if DEBUG
            if (Logger.DebugEnabled)
            {
                Logger.WebSocketVersion08ServerHandshakeKey(key, accept);
            }
#endif

            _ = res.Headers.Set(HttpHeaderNames.Upgrade, HttpHeaderValues.Websocket)
                           .Set(HttpHeaderNames.Connection, HttpHeaderValues.Upgrade)
                           .Set(HttpHeaderNames.SecWebsocketAccept, accept);

            if (req.Headers.TryGet(HttpHeaderNames.SecWebsocketProtocol, out ICharSequence subprotocols)
                && subprotocols is object)
            {
                string selectedSubprotocol = this.SelectSubprotocol(subprotocols.ToString());
                if (selectedSubprotocol is null)
                {
#if DEBUG
                    if (Logger.DebugEnabled)
                    {
                        Logger.RequestedSubprotocolNotSupported(subprotocols);
                    }
#endif
                }
                else
                {
                    _ = res.Headers.Add(HttpHeaderNames.SecWebsocketProtocol, selectedSubprotocol);
                }
            }
            return res;
        }

        /// <inheritdoc />
        protected internal override IWebSocketFrameDecoder NewWebsocketDecoder() => new WebSocket08FrameDecoder(DecoderConfig);

        /// <inheritdoc />
        protected internal override IWebSocketFrameEncoder NewWebSocketEncoder() => new WebSocket08FrameEncoder(false);
    }
}
