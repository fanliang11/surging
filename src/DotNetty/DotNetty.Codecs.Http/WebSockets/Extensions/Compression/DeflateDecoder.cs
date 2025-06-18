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
    using DotNetty.Buffers;
    using DotNetty.Codecs.Compression;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Embedded;

    /// <summary>
    /// Deflate implementation of a payload decompressor for
    /// <tt>io.netty.handler.codec.http.websocketx.WebSocketFrame</tt>.
    /// </summary>
    abstract class DeflateDecoder : WebSocketExtensionDecoder
    {
        internal static readonly IByteBuffer FrameTail = Unpooled.UnreleasableBuffer(
                Unpooled.WrappedBuffer(new byte[] { 0x00, 0x00, 0xff, 0xff }))
                .AsReadOnly();

        internal static readonly IByteBuffer EmptyDeflateBlock = Unpooled.UnreleasableBuffer(
                Unpooled.WrappedBuffer(new byte[] { 0x00 }))
                .AsReadOnly();

        private readonly bool _noContext;
        private readonly IWebSocketExtensionFilter _extensionDecoderFilter;

        private EmbeddedChannel _decoder;

        /// <summary>Constructor</summary>
        /// <param name="noContext">true to disable context takeover.</param>
        /// <param name="extensionDecoderFilter">extension decoder filter.</param>
        protected DeflateDecoder(bool noContext, IWebSocketExtensionFilter extensionDecoderFilter)
        {
            if (extensionDecoderFilter is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.extensionDecoderFilter); }
            _noContext = noContext;
            _extensionDecoderFilter = extensionDecoderFilter;
        }

        /// <summary>
        /// Returns the extension decoder filter.
        /// </summary>
        protected IWebSocketExtensionFilter ExtensionDecoderFilter => _extensionDecoderFilter;

        protected abstract bool AppendFrameTail(WebSocketFrame msg);

        protected abstract int NewRsv(WebSocketFrame msg);

        protected override void Decode(IChannelHandlerContext ctx, WebSocketFrame msg, List<object> output)
        {
            var decompressedContent = DecompressContent(ctx, msg);

            WebSocketFrame outMsg = null;
            switch (msg.Opcode)
            {
                case Opcode.Text:
                    outMsg = new TextWebSocketFrame(msg.IsFinalFragment, NewRsv(msg), decompressedContent);
                    break;
                case Opcode.Binary:
                    outMsg = new BinaryWebSocketFrame(msg.IsFinalFragment, NewRsv(msg), decompressedContent);
                    break;
                case Opcode.Cont:
                    outMsg = new ContinuationWebSocketFrame(msg.IsFinalFragment, NewRsv(msg), decompressedContent);
                    break;
                default:
                    ThrowHelper.ThrowCodecException_UnexpectedFrameType(msg);
                    break;
            }
            output.Add(outMsg);
        }

        public override void HandlerRemoved(IChannelHandlerContext ctx)
        {
            Cleanup();
            base.HandlerRemoved(ctx);
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            Cleanup();
            base.ChannelInactive(ctx);
        }

        private IByteBuffer DecompressContent(IChannelHandlerContext ctx, WebSocketFrame msg)
        {
            if (_decoder is null)
            {
                switch (msg.Opcode)
                {
                    case Opcode.Text:
                    case Opcode.Binary:
                        break;
                    default:
                        ThrowHelper.ThrowCodecException_UnexpectedInitialFrameType(msg);
                        break;
                }
                _decoder = new EmbeddedChannel(ZlibCodecFactory.NewZlibDecoder(ZlibWrapper.None));
            }

            var readable = msg.Content.IsReadable();
            var emptyDeflateBlock = EmptyDeflateBlock.Equals(msg.Content);

            _ = _decoder.WriteInbound(msg.Content.Retain());
            if (AppendFrameTail(msg))
            {
                _ = _decoder.WriteInbound(FrameTail.Duplicate());
            }

            var compositeDecompressedContent = ctx.Allocator.CompositeBuffer();
            for (; ; )
            {
                var partUncompressedContent = _decoder.ReadInbound<IByteBuffer>();
                if (partUncompressedContent is null)
                {
                    break;
                }
                if (!partUncompressedContent.IsReadable())
                {
                    _ = partUncompressedContent.Release();
                    continue;
                }
                _ = compositeDecompressedContent.AddComponent(true, partUncompressedContent);
            }
            // Correctly handle empty frames
            // See https://github.com/netty/netty/issues/4348
            if (!emptyDeflateBlock && readable && compositeDecompressedContent.NumComponents <= 0)
            {
                // Sometimes after fragmentation the last frame
                // May contain left-over data that doesn't affect decompression
                if (!(msg is ContinuationWebSocketFrame))
                {
                    _ = compositeDecompressedContent.Release();
                    ThrowHelper.ThrowCodecException_CannotReadUncompressedBuf();
                }
            }

            if (msg.IsFinalFragment && _noContext)
            {
                Cleanup();
            }

            return compositeDecompressedContent;
        }

        void Cleanup()
        {
            if (_decoder is object)
            {
                // Clean-up the previous encoder if not cleaned up correctly.
                _ = _decoder.FinishAndReleaseAll();
                _decoder = null;
            }
        }
    }
}
