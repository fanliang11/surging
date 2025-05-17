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

namespace DotNetty.Codecs.Http.WebSockets.Extensions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    public class WebSocketClientExtensionHandler : ChannelHandlerAdapter
    {
        readonly List<IWebSocketClientExtensionHandshaker> extensionHandshakers;

        public WebSocketClientExtensionHandler(params IWebSocketClientExtensionHandshaker[] extensionHandshakers)
        {
            if (extensionHandshakers is null || 0u >= (uint)extensionHandshakers.Length) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.extensionHandshakers); }
            this.extensionHandshakers = new List<IWebSocketClientExtensionHandshaker>(extensionHandshakers);
        }

        public override void Write(IChannelHandlerContext ctx, object msg, IPromise promise)
        {
            if (msg is IHttpRequest request && WebSocketExtensionUtil.IsWebsocketUpgrade(request.Headers))
            {
                string headerValue = null;
                if (request.Headers.TryGet(HttpHeaderNames.SecWebsocketExtensions, out ICharSequence value))
                {
                    headerValue = value.ToString();
                }

                foreach (IWebSocketClientExtensionHandshaker extensionHandshaker in this.extensionHandshakers)
                {
                    WebSocketExtensionData extensionData = extensionHandshaker.NewRequestData();
                    headerValue = WebSocketExtensionUtil.AppendExtension(headerValue,
                        extensionData.Name, extensionData.Parameters);
                }

                _ = request.Headers.Set(HttpHeaderNames.SecWebsocketExtensions, headerValue);
            }

            base.Write(ctx, msg, promise);
        }

        public override void ChannelRead(IChannelHandlerContext ctx, object msg)
        {
            if (msg is IHttpResponse response
                && WebSocketExtensionUtil.IsWebsocketUpgrade(response.Headers))
            {
                string extensionsHeader = null;
                if (response.Headers.TryGet(HttpHeaderNames.SecWebsocketExtensions, out ICharSequence value))
                {
                    extensionsHeader = value.ToString();
                }

                var pipeline = ctx.Pipeline;
                if (extensionsHeader is object)
                {
                    List<WebSocketExtensionData> extensions =
                        WebSocketExtensionUtil.ExtractExtensions(extensionsHeader);
                    var validExtensions = new List<IWebSocketClientExtension>(extensions.Count);
                    int rsv = 0;

                    foreach (WebSocketExtensionData extensionData in extensions)
                    {
                        IWebSocketClientExtension validExtension = null;
                        foreach (IWebSocketClientExtensionHandshaker extensionHandshaker in this.extensionHandshakers)
                        {
                            validExtension = extensionHandshaker.HandshakeExtension(extensionData);
                            if (validExtension is object)
                            {
                                break;
                            }
                        }

                        if (validExtension is object && 0u >= (uint)(validExtension.Rsv & rsv))
                        {
                            rsv = rsv | validExtension.Rsv;
                            validExtensions.Add(validExtension);
                        }
                        else
                        {
                            ThrowHelper.ThrowCodecException_InvalidWSExHandshake(extensionsHeader);
                        }
                    }

                    foreach (IWebSocketClientExtension validExtension in validExtensions)
                    {
                        WebSocketExtensionDecoder decoder = validExtension.NewExtensionDecoder();
                        WebSocketExtensionEncoder encoder = validExtension.NewExtensionEncoder();
                        _ = pipeline.AddAfter(ctx.Name, decoder.GetType().Name, decoder);
                        _ = pipeline.AddAfter(ctx.Name, encoder.GetType().Name, encoder);
                    }
                }

                _ = pipeline.Remove(ctx.Name);
            }

            base.ChannelRead(ctx, msg);
        }
    }
}
