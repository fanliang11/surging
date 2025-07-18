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
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.WebSockets.Extensions.Compression
{
    /// <summary>
    /// Per-frame implementation of deflate decompressor.
    /// </summary>
    sealed class PerFrameDeflateDecoder : DeflateDecoder
    {
        /// <summary>Constructor</summary>
        /// <param name="noContext">true to disable context takeover.</param>
        public PerFrameDeflateDecoder(bool noContext)
            : base(noContext, NeverSkipWebSocketExtensionFilter.Instance)
        {
        }

        /// <summary>Constructor</summary>
        /// <param name="noContext">true to disable context takeover.</param>
        /// <param name="extensionDecoderFilter">extension decoder filter for per frame deflate decoder.</param>
        public PerFrameDeflateDecoder(bool noContext, IWebSocketExtensionFilter extensionDecoderFilter)
            : base(noContext, extensionDecoderFilter)
        {
        }

        /// <inheritdoc />
        public override bool AcceptInboundMessage(object msg)
        {
            if (!(msg is WebSocketFrame wsFrame)) { return false; }

            if (ExtensionDecoderFilter.MustSkip(wsFrame)) { return false; }

            switch (wsFrame.Opcode)
            {
                case Opcode.Text:
                case Opcode.Binary:
                case Opcode.Cont:
                    return (wsFrame.Rsv & WebSocketRsv.Rsv1) > 0;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        protected override int NewRsv(WebSocketFrame msg) => msg.Rsv ^ WebSocketRsv.Rsv1;

        /// <inheritdoc />
        protected override bool AppendFrameTail(WebSocketFrame msg) => true;
    }
}
