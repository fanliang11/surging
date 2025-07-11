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
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// Performs client side opening and closing handshakes for web socket specification version <a
    /// href="http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-00" >draft-ietf-hybi-thewebsocketprotocol-00</a>
    /// <para>A very large portion of this code was taken from the Netty 3.2 HTTP example.</para>
    /// </summary>
    public class WebSocketClientHandshaker00 : WebSocketClientHandshaker
    {
        private static readonly AsciiString Websocket = AsciiString.Cached("WebSocket");

        private IByteBuffer _expectedChallengeResponseBytes;

        /// <summary>Creates a new instance with the specified destination WebSocket location and version to initiate.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        public WebSocketClientHandshaker00(Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            HttpHeaders customHeaders, int maxFramePayloadLength)
            : this(webSocketUrl, version, subprotocol, customHeaders, maxFramePayloadLength, DefaultForceCloseTimeoutMillis)
        {
        }

        /// <summary>Creates a new instance with the specified destination WebSocket location and version to initiate.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="forceCloseTimeoutMillis">Close the connection if it was not closed by the server after timeout specified</param>
        internal WebSocketClientHandshaker00(Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            HttpHeaders customHeaders, int maxFramePayloadLength, long forceCloseTimeoutMillis)
            : this(webSocketUrl, version, subprotocol, customHeaders, maxFramePayloadLength, forceCloseTimeoutMillis, false)
        {
        }

        /// <summary>Creates a new instance with the specified destination WebSocket location and version to initiate.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="forceCloseTimeoutMillis">Close the connection if it was not closed by the server after timeout specified</param>
        /// <param name="absoluteUpgradeUrl">Use an absolute url for the Upgrade request, typically when connecting through an HTTP proxy over
        /// clear HTTP</param>
        internal WebSocketClientHandshaker00(Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            HttpHeaders customHeaders, int maxFramePayloadLength, long forceCloseTimeoutMillis, bool absoluteUpgradeUrl)
            : base(webSocketUrl, version, subprotocol, customHeaders, maxFramePayloadLength, forceCloseTimeoutMillis, absoluteUpgradeUrl)
        {
        }

        protected internal override unsafe IFullHttpRequest NewHandshakeRequest()
        {
            // Make keys
            int spaces1 = WebSocketUtil.RandomNumber(1, 12);
            int spaces2 = WebSocketUtil.RandomNumber(1, 12);

            int max1 = int.MaxValue / spaces1;
            int max2 = int.MaxValue / spaces2;

            int number1 = WebSocketUtil.RandomNumber(0, max1);
            int number2 = WebSocketUtil.RandomNumber(0, max2);

            int product1 = number1 * spaces1;
            int product2 = number2 * spaces2;

            string key1 = product1.ToString(CultureInfo.InvariantCulture);
            string key2 = product2.ToString(CultureInfo.InvariantCulture);

            key1 = InsertRandomCharacters(key1);
            key2 = InsertRandomCharacters(key2);

            key1 = InsertSpaces(key1, spaces1);
            key2 = InsertSpaces(key2, spaces2);

            byte[] key3 = WebSocketUtil.RandomBytes(8);

            var challenge = new byte[16];
            fixed (byte* bytes = challenge)
            {
                Unsafe.WriteUnaligned(bytes, number1);
                Unsafe.WriteUnaligned(bytes + 4, number2);
                PlatformDependent.CopyMemory(key3, 0, bytes + 8, 8);
            }

            _expectedChallengeResponseBytes = Unpooled.WrappedBuffer(WebSocketUtil.Md5(challenge));

            Uri wsUrl = Uri;

            // Format request
            var request = new DefaultFullHttpRequest(HttpVersion.Http11, HttpMethod.Get, UpgradeUrl(wsUrl),
                Unpooled.WrappedBuffer(key3));
            HttpHeaders headers = request.Headers;

            if (CustomHeaders is object)
            {
                _ = headers.Add(CustomHeaders);
            }

            _ = headers.Set(HttpHeaderNames.Upgrade, Websocket)
                .Set(HttpHeaderNames.Connection, HttpHeaderValues.Upgrade)
                .Set(HttpHeaderNames.Host, WebsocketHostValue(wsUrl))
                .Set(HttpHeaderNames.SecWebsocketKey1, key1)
                .Set(HttpHeaderNames.SecWebsocketKey2, key2);

            if (!headers.Contains(HttpHeaderNames.Origin))
            {
                _ = headers.Set(HttpHeaderNames.Origin, WebsocketOriginValue(wsUrl));
            }

            string expectedSubprotocol = ExpectedSubprotocol;
            if (!string.IsNullOrEmpty(expectedSubprotocol))
            {
                _ = headers.Set(HttpHeaderNames.SecWebsocketProtocol, expectedSubprotocol);
            }

            // Set Content-Length to workaround some known defect.
            // See also: http://www.ietf.org/mail-archive/web/hybi/current/msg02149.html
            _ = headers.Set(HttpHeaderNames.ContentLength, key3.Length);
            return request;
        }

        protected override void Verify(IFullHttpResponse response)
        {
            if (!response.Status.Equals(HttpResponseStatus.SwitchingProtocols))
            {
                ThrowHelper.ThrowWebSocketHandshakeException_InvalidHandshakeResponseGS(response);
            }

            HttpHeaders headers = response.Headers;

            if (!headers.TryGet(HttpHeaderNames.Upgrade, out ICharSequence upgrade)
                || !Websocket.ContentEqualsIgnoreCase(upgrade))
            {
                ThrowHelper.ThrowWebSocketHandshakeException_InvalidHandshakeResponseU(upgrade);
            }

            if (!headers.ContainsValue(HttpHeaderNames.Connection, HttpHeaderValues.Upgrade, true))
            {
                _ = headers.TryGet(HttpHeaderNames.Connection, out upgrade);
                ThrowHelper.ThrowWebSocketHandshakeException_InvalidHandshakeResponseConn(upgrade);
            }

            IByteBuffer challenge = response.Content;
            if (!challenge.Equals(_expectedChallengeResponseBytes))
            {
                ThrowHelper.ThrowWebSocketHandshakeException_InvalidChallenge();
            }
        }

        static string InsertRandomCharacters(string key)
        {
            int count = WebSocketUtil.RandomNumber(1, 12);

            var randomChars = new char[count];
            int randCount = 0;
            while (randCount < count)
            {
                int rand = unchecked((int)(WebSocketUtil.RandomNext() * 0x7e + 0x21));
                if (0x21 < rand && rand < 0x2f || 0x3a < rand && rand < 0x7e)
                {
                    randomChars[randCount] = (char)rand;
                    randCount += 1;
                }
            }

            for (int i = 0; i < count; i++)
            {
                int split = WebSocketUtil.RandomNumber(0, key.Length);
                string part1 = key.Substring(0, split);
                string part2 = key.Substring(split);
                key = part1 + randomChars[i] + part2;
            }

            return key;
        }

        static string InsertSpaces(string key, int spaces)
        {
            for (int i = 0; i < spaces; i++)
            {
                int split = WebSocketUtil.RandomNumber(1, key.Length - 1);
                string part1 = key.Substring(0, split);
                string part2 = key.Substring(split);
                key = part1 + ' ' + part2;
            }

            return key;
        }

        protected internal override IWebSocketFrameDecoder NewWebSocketDecoder() => new WebSocket00FrameDecoder(MaxFramePayloadLength);

        protected internal override IWebSocketFrameEncoder NewWebSocketEncoder() => new WebSocket00FrameEncoder();
    }
}
