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
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Performs server side opening and closing handshakes for web socket specification version <a
    /// href="http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-00" >draft-ietf-hybi-thewebsocketprotocol-00</a>
    /// </summary>
    public class WebSocketServerHandshaker00 : WebSocketServerHandshaker
    {
        static readonly Regex BeginningDigit = new Regex("[^0-9]", RegexOptions.Compiled);
        static readonly Regex BeginningSpace = new Regex("[^ ]", RegexOptions.Compiled);

        /// <summary>
        /// Constructor specifying the destination web socket location
        /// </summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="subprotocols">CSV of supported protocols</param>
        /// <param name="maxFramePayloadLength">Maximum allowable frame payload length. Setting this value to your application's requirement may
        /// reduce denial of service attacks using long data frames.</param>
        public WebSocketServerHandshaker00(string webSocketUrl, string subprotocols, int maxFramePayloadLength)
            : base(WebSocketVersion.V00, webSocketUrl, subprotocols, WebSocketDecoderConfig.NewBuilder().MaxFramePayloadLength(maxFramePayloadLength).Build())
        {
        }

        /// <summary>
        /// Constructor specifying the destination web socket location
        /// </summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="subprotocols">CSV of supported protocols</param>
        /// <param name="decoderConfig">Frames decoder configuration.</param>
        public WebSocketServerHandshaker00(string webSocketUrl, string subprotocols, WebSocketDecoderConfig decoderConfig)
            : base(WebSocketVersion.V00, webSocketUrl, subprotocols, decoderConfig)
        {
        }

        /// <summary>
        /// <para>
        /// Handle the web socket handshake for the web socket specification <a href=
        /// "http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-00">HyBi version 0</a> and lower. This standard
        /// is really a rehash of <a href="http://tools.ietf.org/html/draft-hixie-thewebsocketprotocol-76" >hixie-76</a> and
        /// <a href="http://tools.ietf.org/html/draft-hixie-thewebsocketprotocol-75" >hixie-75</a>.
        /// </para>
        ///
        /// <para>
        /// Browser request to the server:
        /// </para>
        ///
        /// <![CDATA[
        /// GET /demo HTTP/1.1
        /// Upgrade: WebSocket
        /// Connection: Upgrade
        /// Host: example.com
        /// Origin: http://example.com
        /// Sec-WebSocket-Protocol: chat, sample
        /// Sec-WebSocket-Key1: 4 @1  46546xW%0l 1 5
        /// Sec-WebSocket-Key2: 12998 5 Y3 1  .P00
        ///
        /// ^n:ds[4U
        /// ]]>
        ///
        /// <para>
        /// Server response:
        /// </para>
        ///
        /// <![CDATA[
        /// HTTP/1.1 101 WebSocket Protocol Handshake
        /// Upgrade: WebSocket
        /// Connection: Upgrade
        /// Sec-WebSocket-Origin: http://example.com
        /// Sec-WebSocket-Location: ws://example.com/demo
        /// Sec-WebSocket-Protocol: sample
        ///
        /// 8jKS'y:G*Co,Wxa-
        /// ]]>
        /// </summary>
        /// <param name="req"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        protected internal override IFullHttpResponse NewHandshakeResponse(IFullHttpRequest req, HttpHeaders headers)
        {
            // Serve the WebSocket handshake request.
            if (!req.Headers.ContainsValue(HttpHeaderNames.Connection, HttpHeaderValues.Upgrade, true)
                || !req.Headers.TryGet(HttpHeaderNames.Upgrade, out ICharSequence value)
                || !HttpHeaderValues.Websocket.ContentEqualsIgnoreCase(value))
            {
                ThrowHelper.ThrowWebSocketHandshakeException_MissingUpgrade();
            }

            // Hixie 75 does not contain these headers while Hixie 76 does
            bool isHixie76 = req.Headers.Contains(HttpHeaderNames.SecWebsocketKey1)
                && req.Headers.Contains(HttpHeaderNames.SecWebsocketKey2);

            var origin = req.Headers.Get(HttpHeaderNames.Origin, null);
            //throw before allocating FullHttpResponse
            if (origin is null && !isHixie76)
            {
                ThrowHelper.ThrowWebSocketHandshakeException_Missing_origin_header(req);
            }

            // Create the WebSocket handshake response.
            var res = new DefaultFullHttpResponse(HttpVersion.Http11, new HttpResponseStatus(101,
                    new AsciiString(isHixie76 ? "WebSocket Protocol Handshake" : "Web Socket Protocol Handshake")),
                    req.Content.Allocator.Buffer(0));
            if (headers is object)
            {
                _ = res.Headers.Add(headers);
            }

            _ = res.Headers.Set(HttpHeaderNames.Upgrade, HttpHeaderValues.Websocket)
                           .Set(HttpHeaderNames.Connection, HttpHeaderValues.Upgrade);

            // Fill in the headers and contents depending on handshake getMethod.
            if (isHixie76)
            {
                // New handshake getMethod with a challenge:
                _ = res.Headers.Add(HttpHeaderNames.SecWebsocketOrigin, origin);
                _ = res.Headers.Add(HttpHeaderNames.SecWebsocketLocation, this.Uri);

                if (req.Headers.TryGet(HttpHeaderNames.SecWebsocketProtocol, out ICharSequence subprotocols))
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

                // Calculate the answer of the challenge.
                value = req.Headers.Get(HttpHeaderNames.SecWebsocketKey1, null);
                Debug.Assert(value is object, $"{HttpHeaderNames.SecWebsocketKey1} must exist");
                string key1 = value.ToString();
                value = req.Headers.Get(HttpHeaderNames.SecWebsocketKey2, null);
                Debug.Assert(value is object, $"{HttpHeaderNames.SecWebsocketKey2} must exist");
                string key2 = value.ToString();
                int a = (int)(long.Parse(BeginningDigit.Replace(key1, "")) /
                    BeginningSpace.Replace(key1, "").Length);
                int b = (int)(long.Parse(BeginningDigit.Replace(key2, "")) /
                    BeginningSpace.Replace(key2, "").Length);
                long c = req.Content.ReadLong();
                IByteBuffer input = Unpooled.WrappedBuffer(new byte[16]).SetIndex(0, 0);
                _ = input.WriteInt(a);
                _ = input.WriteInt(b);
                _ = input.WriteLong(c);
                _ = res.Content.WriteBytes(WebSocketUtil.Md5(input.Array));
            }
            else
            {
                // Old Hixie 75 handshake getMethod with no challenge:
                _ = res.Headers.Add(HttpHeaderNames.WebsocketOrigin, origin);
                _ = res.Headers.Add(HttpHeaderNames.WebsocketLocation, this.Uri);

                if (req.Headers.TryGet(HttpHeaderNames.WebsocketProtocol, out ICharSequence protocol))
                {
                    _ = res.Headers.Add(HttpHeaderNames.WebsocketProtocol, this.SelectSubprotocol(protocol.ToString()));
                }
            }

            return res;
        }

        /// <summary>
        /// Echo back the closing frame
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="frame">Web Socket frame that was received</param>
        /// <returns></returns>
        public override Task CloseAsync(IChannel channel, CloseWebSocketFrame frame) => channel.WriteAndFlushAsync(frame);

        /// <inheritdoc/>
        protected internal override IWebSocketFrameDecoder NewWebsocketDecoder() => new WebSocket00FrameDecoder(this.DecoderConfig);

        /// <inheritdoc/>
        protected internal override IWebSocketFrameEncoder NewWebSocketEncoder() => new WebSocket00FrameEncoder();
    }
}
