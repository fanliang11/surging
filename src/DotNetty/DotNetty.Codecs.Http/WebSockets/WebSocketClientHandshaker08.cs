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
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// Performs client side opening and closing handshakes for web socket specification version <a
    /// href="http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-10" >draft-ietf-hybi-thewebsocketprotocol-10</a>
    /// </summary>
    public class WebSocketClientHandshaker08 : WebSocketClientHandshaker
    {
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<WebSocketClientHandshaker08>();

        public const string MagicGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private AsciiString _expectedChallengeResponseString;

        private readonly bool _allowExtensions;
        private readonly bool _performMasking;
        private readonly bool _allowMaskMismatch;

        /// <summary>Creates a new instance.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        public WebSocketClientHandshaker08(Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength)
            : this(webSocketUrl, version, subprotocol, allowExtensions, customHeaders, maxFramePayloadLength, true,
                false, DefaultForceCloseTimeoutMillis)
        {
        }

        /// <summary>Creates a new instance.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="performMasking">Whether to mask all written websocket frames. This must be set to true in order to be fully compatible
        /// with the websocket specifications. Client applications that communicate with a non-standard server
        /// which doesn't require masking might set this to false to achieve a higher performance.</param>
        /// <param name="allowMaskMismatch">When set to true, frames which are not masked properly according to the standard will still be
        /// accepted</param>
        public WebSocketClientHandshaker08(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            bool performMasking, bool allowMaskMismatch)
            : this(webSocketUrl, version, subprotocol, allowExtensions, customHeaders, maxFramePayloadLength, performMasking,
                allowMaskMismatch, DefaultForceCloseTimeoutMillis)
        {
        }

        /// <summary>Creates a new instance.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="performMasking">Whether to mask all written websocket frames. This must be set to true in order to be fully compatible
        /// with the websocket specifications. Client applications that communicate with a non-standard server
        /// which doesn't require masking might set this to false to achieve a higher performance.</param>
        /// <param name="allowMaskMismatch">When set to true, frames which are not masked properly according to the standard will still be
        /// accepted</param>
        /// <param name="forceCloseTimeoutMillis">Close the connection if it was not closed by the server after timeout specified.</param>
        public WebSocketClientHandshaker08(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            bool performMasking, bool allowMaskMismatch, long forceCloseTimeoutMillis)
            : this(webSocketUrl, version, subprotocol, allowExtensions, customHeaders, maxFramePayloadLength, performMasking,
                allowMaskMismatch, forceCloseTimeoutMillis, false)
        {
        }

        /// <summary>Creates a new instance.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="performMasking">Whether to mask all written websocket frames. This must be set to true in order to be fully compatible
        /// with the websocket specifications. Client applications that communicate with a non-standard server
        /// which doesn't require masking might set this to false to achieve a higher performance.</param>
        /// <param name="allowMaskMismatch">When set to true, frames which are not masked properly according to the standard will still be
        /// accepted</param>
        /// <param name="forceCloseTimeoutMillis">Close the connection if it was not closed by the server after timeout specified.</param>
        /// <param name="absoluteUpgradeUrl">Use an absolute url for the Upgrade request, typically when connecting through an HTTP proxy over
        /// clear HTTP</param>
        public WebSocketClientHandshaker08(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            bool performMasking, bool allowMaskMismatch, long forceCloseTimeoutMillis,
            bool absoluteUpgradeUrl)
            : base(webSocketUrl, version, subprotocol, customHeaders, maxFramePayloadLength, forceCloseTimeoutMillis,
                absoluteUpgradeUrl)
        {
            _allowExtensions = allowExtensions;
            _performMasking = performMasking;
            _allowMaskMismatch = allowMaskMismatch;
        }

        protected internal override IFullHttpRequest NewHandshakeRequest()
        {
            Uri wsUrl = Uri;

            // Get 16 bit nonce and base 64 encode it
            byte[] nonce = WebSocketUtil.RandomBytes(16);
            string key = WebSocketUtil.Base64String(nonce);

            string acceptSeed = key + MagicGuid;
            byte[] sha1 = WebSocketUtil.Sha1(Encoding.ASCII.GetBytes(acceptSeed));
            _expectedChallengeResponseString = new AsciiString(WebSocketUtil.Base64String(sha1));

#if DEBUG
            if (Logger.DebugEnabled)
            {
                Logger.WebSocketVersion08ClientHandshakeKey(key, _expectedChallengeResponseString);
            }
#endif

            // Format request
            var request = new DefaultFullHttpRequest(HttpVersion.Http11, HttpMethod.Get, UpgradeUrl(wsUrl),
                Unpooled.Empty);
            HttpHeaders headers = request.Headers;

            if (CustomHeaders is object)
            {
                _ = headers.Add(CustomHeaders);
                if (!headers.Contains(HttpHeaderNames.Host))
                {
                    // Only add HOST header if customHeaders did not contain it.
                    //
                    // See https://github.com/netty/netty/issues/10101
                    _ = headers.Set(HttpHeaderNames.Host, WebsocketHostValue(wsUrl));
                }
            }
            else
            {
                _ = headers.Set(HttpHeaderNames.Host, WebsocketHostValue(wsUrl));
            }

            _ = headers.Set(HttpHeaderNames.Upgrade, HttpHeaderValues.Websocket)
                .Set(HttpHeaderNames.Connection, HttpHeaderValues.Upgrade)
                .Set(HttpHeaderNames.SecWebsocketKey, key);

            if (!headers.Contains(HttpHeaderNames.SecWebsocketOrigin))
            {
                _ = headers.Set(HttpHeaderNames.SecWebsocketOrigin, WebsocketOriginValue(wsUrl));
            }

            string expectedSubprotocol = ExpectedSubprotocol;
            if (!string.IsNullOrEmpty(expectedSubprotocol))
            {
                _ = headers.Set(HttpHeaderNames.SecWebsocketProtocol, expectedSubprotocol);
            }

            _ = headers.Set(HttpHeaderNames.SecWebsocketVersion, Version.ToString());

            return request;
        }

        protected override void Verify(IFullHttpResponse response)
        {
            HttpResponseStatus status = HttpResponseStatus.SwitchingProtocols;
            HttpHeaders headers = response.Headers;

            if (!response.Status.Equals(status))
            {
                ThrowHelper.ThrowWebSocketHandshakeException_InvalidHandshakeResponseGS(response);
            }

            if (!headers.TryGet(HttpHeaderNames.Upgrade, out ICharSequence upgrade)
                || !HttpHeaderValues.Websocket.ContentEqualsIgnoreCase(upgrade))
            {
                ThrowHelper.ThrowWebSocketHandshakeException_InvalidHandshakeResponseU(upgrade);
            }

            if (!headers.ContainsValue(HttpHeaderNames.Connection, HttpHeaderValues.Upgrade, true))
            {
                _ = headers.TryGet(HttpHeaderNames.Connection, out upgrade);
                ThrowHelper.ThrowWebSocketHandshakeException_InvalidHandshakeResponseConn(upgrade);
            }

            if (!headers.TryGet(HttpHeaderNames.SecWebsocketAccept, out ICharSequence accept)
                || !accept.Equals(_expectedChallengeResponseString))
            {
                ThrowHelper.ThrowWebSocketHandshakeException_InvalidChallenge(accept, _expectedChallengeResponseString);
            }
        }

        protected internal override IWebSocketFrameDecoder NewWebSocketDecoder() => new WebSocket08FrameDecoder(
            false, _allowExtensions, MaxFramePayloadLength, _allowMaskMismatch);

        protected internal override IWebSocketFrameEncoder NewWebSocketEncoder() => new WebSocket08FrameEncoder(_performMasking);
    }
}
