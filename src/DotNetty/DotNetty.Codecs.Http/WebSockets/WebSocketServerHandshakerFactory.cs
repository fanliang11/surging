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
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Auto-detects the version of the Web Socket protocol in use and creates a new proper
    /// <see cref="WebSocketServerHandshaker"/>
    /// </summary>
    public class WebSocketServerHandshakerFactory
    {
        private readonly string _webSocketUrl;

        private readonly string _subprotocols;

        private readonly WebSocketDecoderConfig _decoderConfig;

        /// <summary>Constructor specifying the destination web socket location</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath".
        /// Subsequent web socket frames will be sent to this URL.</param>
        /// <param name="subprotocols">CSV of supported protocols. Null if sub protocols not supported.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        public WebSocketServerHandshakerFactory(string webSocketUrl, string subprotocols, bool allowExtensions)
            : this(webSocketUrl, subprotocols, allowExtensions, 65536)
        {
        }

        /// <summary>Constructor specifying the destination web socket location</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath".
        /// Subsequent web socket frames will be sent to this URL.</param>
        /// <param name="subprotocols">CSV of supported protocols. Null if sub protocols not supported.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="maxFramePayloadLength">Maximum allowable frame payload length. Setting this value to your application's
        /// requirement may reduce denial of service attacks using long data frames.</param>
        public WebSocketServerHandshakerFactory(string webSocketUrl, string subprotocols, bool allowExtensions,
            int maxFramePayloadLength)
            : this(webSocketUrl, subprotocols, allowExtensions, maxFramePayloadLength, false)
        {
        }

        /// <summary>Constructor specifying the destination web socket location</summary>
        /// <param name="webSocketUrl">URL for web socket communications. e.g "ws://myhost.com/mypath".
        /// Subsequent web socket frames will be sent to this URL.</param>
        /// <param name="subprotocols">CSV of supported protocols. Null if sub protocols not supported.</param>
        /// <param name="allowExtensions">Allow extensions to be used in the reserved bits of the web socket frame</param>
        /// <param name="maxFramePayloadLength">Maximum allowable frame payload length. Setting this value to your application's
        /// requirement may reduce denial of service attacks using long data frames.</param>
        /// <param name="allowMaskMismatch">When set to true, frames which are not masked properly according to the standard will still be
        /// accepted.</param>
        public WebSocketServerHandshakerFactory(string webSocketUrl, string subprotocols, bool allowExtensions,
            int maxFramePayloadLength, bool allowMaskMismatch)
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
        /// <param name="subprotocols">CSV of supported protocols. Null if sub protocols not supported.</param>
        /// <param name="decoderConfig">Frames decoder options.</param>
        public WebSocketServerHandshakerFactory(string webSocketUrl, string subprotocols, WebSocketDecoderConfig decoderConfig)
        {
            if (decoderConfig is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.decoderConfig); }

            _webSocketUrl = webSocketUrl;
            _subprotocols = subprotocols;
            _decoderConfig = decoderConfig;
        }

        /// <summary>Instances a new handshaker</summary>
        /// <param name="req"></param>
        /// <returns>A new WebSocketServerHandshaker for the requested web socket version. Null if web
        /// socket version is not supported.</returns>
        public WebSocketServerHandshaker NewHandshaker(IHttpRequest req)
        {
            if (req.Headers.TryGet(HttpHeaderNames.SecWebsocketVersion, out ICharSequence version)
                && version is object)
            {
                if (version.Equals(WebSocketVersion.V13.ToHttpHeaderValue()))
                {
                    // Version 13 of the wire protocol - RFC 6455 (version 17 of the draft hybi specification).
                    return new WebSocketServerHandshaker13(_webSocketUrl, _subprotocols, _decoderConfig);
                }
                else if (version.Equals(WebSocketVersion.V08.ToHttpHeaderValue()))
                {
                    // Version 8 of the wire protocol - version 10 of the draft hybi specification.
                    return new WebSocketServerHandshaker08(_webSocketUrl, _subprotocols, _decoderConfig);
                }
                else if (version.Equals(WebSocketVersion.V07.ToHttpHeaderValue()))
                {
                    // Version 8 of the wire protocol - version 07 of the draft hybi specification.
                    return new WebSocketServerHandshaker07(_webSocketUrl, _subprotocols, _decoderConfig);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // Assume version 00 where version header was not specified
                return new WebSocketServerHandshaker00(_webSocketUrl, _subprotocols, _decoderConfig);
            }
        }

        /// <summary>
        /// Return that we need cannot not support the web socket version
        /// </summary>
        public static Task SendUnsupportedVersionResponse(IChannel channel)
        {
            var res = new DefaultFullHttpResponse(
                HttpVersion.Http11,
                HttpResponseStatus.UpgradeRequired,
                channel.Allocator.Buffer(0));
            _ = res.Headers.Set(HttpHeaderNames.SecWebsocketVersion, WebSocketVersion.V13.ToHttpHeaderValue());
            HttpUtil.SetContentLength(res, 0);
            return channel.WriteAndFlushAsync(res);
        }
    }
}
