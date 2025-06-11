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

    using static WebSocketVersion;

    /// <summary>
    /// Creates a new <see cref="WebSocketClientHandshaker"/> of desired protocol version.
    /// </summary>
    public static class WebSocketClientHandshakerFactory
    {
        /// <summary>Creates a new handshaker.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath".
        /// Subsequent web socket frames will be sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server. Null if no sub-protocol support is required.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="customHeaders">Custom HTTP headers to send during the handshake</param>
        public static WebSocketClientHandshaker NewHandshaker(Uri webSocketUrl, WebSocketVersion version, string subprotocol, bool allowExtensions, HttpHeaders customHeaders) =>
            NewHandshaker(webSocketUrl, version, subprotocol, allowExtensions, customHeaders, 65536);

        /// <summary>Creates a new handshaker.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath".
        /// Subsequent web socket frames will be sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server. Null if no sub-protocol support is required.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="customHeaders">Custom HTTP headers to send during the handshake</param>
        /// <param name="maxFramePayloadLength">Maximum allowable frame payload length. Setting this value to your application's
        /// requirement may reduce denial of service attacks using long data frames.</param>
        public static WebSocketClientHandshaker NewHandshaker(Uri webSocketUrl, WebSocketVersion version, string subprotocol, bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength)
            => NewHandshaker(webSocketUrl, version, subprotocol, allowExtensions, customHeaders, maxFramePayloadLength, true, false);

        /// <summary>Creates a new handshaker.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath".
        /// Subsequent web socket frames will be sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server. Null if no sub-protocol support is required.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="customHeaders">Custom HTTP headers to send during the handshake</param>
        /// <param name="maxFramePayloadLength">Maximum allowable frame payload length. Setting this value to your application's
        /// requirement may reduce denial of service attacks using long data frames.</param>
        /// <param name="performMasking">Whether to mask all written websocket frames. This must be set to true in order to be fully compatible
        /// with the websocket specifications. Client applications that communicate with a non-standard server
        /// which doesn't require masking might set this to false to achieve a higher performance.</param>
        /// <param name="allowMaskMismatch">When set to true, frames which are not masked properly according to the standard will still be
        /// accepted.</param>
        public static WebSocketClientHandshaker NewHandshaker(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            bool performMasking, bool allowMaskMismatch)
            => NewHandshaker(webSocketUrl, version, subprotocol, allowExtensions, customHeaders,
                    maxFramePayloadLength, performMasking, allowMaskMismatch, -1);

        /// <summary>Creates a new handshaker.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath".
        /// Subsequent web socket frames will be sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server. Null if no sub-protocol support is required.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="customHeaders">Custom HTTP headers to send during the handshake</param>
        /// <param name="maxFramePayloadLength">Maximum allowable frame payload length. Setting this value to your application's
        /// requirement may reduce denial of service attacks using long data frames.</param>
        /// <param name="performMasking">Whether to mask all written websocket frames. This must be set to true in order to be fully compatible
        /// with the websocket specifications. Client applications that communicate with a non-standard server
        /// which doesn't require masking might set this to false to achieve a higher performance.</param>
        /// <param name="allowMaskMismatch">When set to true, frames which are not masked properly according to the standard will still be
        /// accepted.</param>
        /// <param name="forceCloseTimeoutMillis">Close the connection if it was not closed by the server after timeout specified</param>
        /// <returns></returns>
        public static WebSocketClientHandshaker NewHandshaker(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            bool performMasking, bool allowMaskMismatch, long forceCloseTimeoutMillis)
        {
            if (version == V13)
            {
                return new WebSocketClientHandshaker13(
                    webSocketUrl, V13, subprotocol, allowExtensions, customHeaders,
                    maxFramePayloadLength, performMasking, allowMaskMismatch, forceCloseTimeoutMillis);
            }
            if (version == V08)
            {
                return new WebSocketClientHandshaker08(
                    webSocketUrl, V08, subprotocol, allowExtensions, customHeaders,
                    maxFramePayloadLength, performMasking, allowMaskMismatch, forceCloseTimeoutMillis);
            }
            if (version == V07)
            {
                return new WebSocketClientHandshaker07(
                    webSocketUrl, V07, subprotocol, allowExtensions, customHeaders,
                    maxFramePayloadLength, performMasking, allowMaskMismatch, forceCloseTimeoutMillis);
            }
            if (version == V00)
            {
                return new WebSocketClientHandshaker00(
                    webSocketUrl, V00, subprotocol, customHeaders,
                    maxFramePayloadLength, forceCloseTimeoutMillis);
            }

            return ThrowHelper.FromWebSocketHandshakeException_InvalidVersion(version);
        }

        /// <summary>Creates a new handshaker.</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath".
        /// Subsequent web socket frames will be sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server. Null if no sub-protocol support is required.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="customHeaders">Custom HTTP headers to send during the handshake</param>
        /// <param name="maxFramePayloadLength">Maximum allowable frame payload length. Setting this value to your application's
        /// requirement may reduce denial of service attacks using long data frames.</param>
        /// <param name="performMasking">Whether to mask all written websocket frames. This must be set to true in order to be fully compatible
        /// with the websocket specifications. Client applications that communicate with a non-standard server
        /// which doesn't require masking might set this to false to achieve a higher performance.</param>
        /// <param name="allowMaskMismatch">When set to true, frames which are not masked properly according to the standard will still be
        /// accepted.</param>
        /// <param name="forceCloseTimeoutMillis">Close the connection if it was not closed by the server after timeout specified</param>
        /// <param name="absoluteUpgradeUrl">Use an absolute url for the Upgrade request, typically when connecting through an HTTP proxy over
        /// clear HTTP</param>
        /// <returns></returns>
        public static WebSocketClientHandshaker NewHandshaker(
            Uri webSocketUrl, WebSocketVersion version, string subprotocol,
            bool allowExtensions, HttpHeaders customHeaders, int maxFramePayloadLength,
            bool performMasking, bool allowMaskMismatch, long forceCloseTimeoutMillis, bool absoluteUpgradeUrl)
        {
            if (version == V13)
            {
                return new WebSocketClientHandshaker13(
                    webSocketUrl, V13, subprotocol, allowExtensions, customHeaders,
                    maxFramePayloadLength, performMasking, allowMaskMismatch, forceCloseTimeoutMillis, absoluteUpgradeUrl);
            }
            if (version == V08)
            {
                return new WebSocketClientHandshaker08(
                    webSocketUrl, V08, subprotocol, allowExtensions, customHeaders,
                    maxFramePayloadLength, performMasking, allowMaskMismatch, forceCloseTimeoutMillis, absoluteUpgradeUrl);
            }
            if (version == V07)
            {
                return new WebSocketClientHandshaker07(
                    webSocketUrl, V07, subprotocol, allowExtensions, customHeaders,
                    maxFramePayloadLength, performMasking, allowMaskMismatch, forceCloseTimeoutMillis, absoluteUpgradeUrl);
            }
            if (version == V00)
            {
                return new WebSocketClientHandshaker00(
                    webSocketUrl, V00, subprotocol, customHeaders,
                    maxFramePayloadLength, forceCloseTimeoutMillis, absoluteUpgradeUrl);
            }

            return ThrowHelper.FromWebSocketHandshakeException_InvalidVersion(version);
        }
    }
}
