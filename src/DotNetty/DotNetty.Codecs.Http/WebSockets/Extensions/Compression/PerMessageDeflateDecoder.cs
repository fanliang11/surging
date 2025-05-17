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
    using System.Collections.Generic;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Per-message implementation of deflate decompressor.
    /// </summary>
    sealed class PerMessageDeflateDecoder : DeflateDecoder
    {
        private bool _compressing;

        /// <summary>Constructor</summary>
        /// <param name="noContext">true to disable context takeover.</param>
        public PerMessageDeflateDecoder(bool noContext)
            : base(noContext, NeverSkipWebSocketExtensionFilter.Instance)
        {
        }

        /// <summary>Constructor</summary>
        /// <param name="noContext">true to disable context takeover.</param>
        /// <param name="extensionDecoderFilter">extension decoder for per message deflate decoder.</param>
        public PerMessageDeflateDecoder(bool noContext, IWebSocketExtensionFilter extensionDecoderFilter)
            : base(noContext, extensionDecoderFilter)
        {
        }

        /// <inheritdoc />
        public override bool AcceptInboundMessage(object msg)
        {
            if (!(msg is WebSocketFrame wsFrame)) { return false; }

            if (ExtensionDecoderFilter.MustSkip(wsFrame))
            {
                if (_compressing)
                {
                    ThrowHelper.ThrowInvalidOperationException_Cannot_skip_per_message_deflate_decoder();
                }
                return false;
            }

            switch (wsFrame.Opcode)
            {
                case Opcode.Text:
                case Opcode.Binary:
                    return (wsFrame.Rsv & WebSocketRsv.Rsv1) > 0;
                case Opcode.Cont:
                    return _compressing;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        protected override int NewRsv(WebSocketFrame msg) =>
            (msg.Rsv & WebSocketRsv.Rsv1) > 0 ? msg.Rsv ^ WebSocketRsv.Rsv1 : msg.Rsv;

        /// <inheritdoc />
        protected override bool AppendFrameTail(WebSocketFrame msg) => msg.IsFinalFragment;

        /// <inheritdoc />
        protected override void Decode(IChannelHandlerContext ctx, WebSocketFrame msg, List<object> output)
        {
            base.Decode(ctx, msg, output);

            if (msg.IsFinalFragment)
            {
                _compressing = false;
            }
            else //if (msg is TextWebSocketFrame || msg is BinaryWebSocketFrame)
            {
                switch (msg.Opcode)
                {
                    case Opcode.Text:
                    case Opcode.Binary:
                        _compressing = true;
                        break;
                }
            }
        }
    }
}
