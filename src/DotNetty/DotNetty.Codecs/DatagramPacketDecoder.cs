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

namespace DotNetty.Codecs
{
    using System;
    using System.Collections.Generic;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;

    /// <summary>
    /// A decoder that decodes the content of the received <see cref="DatagramPacket"/> using
    /// the specified <see cref="IByteBuffer"/> decoder. E.g.,
    /// <code>
    /// <see cref="IChannelPipeline"/> pipeline = ...;
    /// pipeline.AddLast("udpDecoder", new <see cref="DatagramPacketDecoder"/>(new <see cref="T:ProtobufDecoder"/>(...));
    /// </code>
    /// </summary>
    public class DatagramPacketDecoder : MessageToMessageDecoder<DatagramPacket>
    {
        readonly MessageToMessageDecoder<IByteBuffer> decoder;

        /// <summary>
        /// Create a <see cref="DatagramPacket"/> decoder using the specified <see cref="IByteBuffer"/> decoder.
        /// </summary>
        /// <param name="decoder">the specified <see cref="IByteBuffer"/> decoder</param>
        public DatagramPacketDecoder(MessageToMessageDecoder<IByteBuffer> decoder)
        {
            if (decoder is null) { CThrowHelper.ThrowArgumentNullException(CExceptionArgument.decoder); }

            this.decoder = decoder;
        }

        /// <inheritdoc />
        public override bool AcceptInboundMessage(object msg)
        {
            return msg is DatagramPacket envelope
                && this.decoder.AcceptInboundMessage(envelope.Content);
        }

        /// <inheritdoc />
        protected internal override void Decode(IChannelHandlerContext context, DatagramPacket message, List<object> output) => 
            this.decoder.Decode(context, message.Content, output);

        /// <inheritdoc />
        public override void ChannelRegistered(IChannelHandlerContext context) => 
            this.decoder.ChannelRegistered(context);

        /// <inheritdoc />
        public override void ChannelUnregistered(IChannelHandlerContext context) => 
            this.decoder.ChannelUnregistered(context);

        /// <inheritdoc />
        public override void ChannelActive(IChannelHandlerContext context) => this.decoder.ChannelActive(context);

        /// <inheritdoc />
        public override void ChannelInactive(IChannelHandlerContext context) => this.decoder.ChannelInactive(context);

        /// <inheritdoc />
        public override void ChannelReadComplete(IChannelHandlerContext context) => this.decoder.ChannelReadComplete(context);

        /// <inheritdoc />
        public override void UserEventTriggered(IChannelHandlerContext context, object evt) => this.decoder.UserEventTriggered(context, evt);

        /// <inheritdoc />
        public override void ChannelWritabilityChanged(IChannelHandlerContext context) => this.decoder.ChannelWritabilityChanged(context);

        /// <inheritdoc />
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception) => this.decoder.ExceptionCaught(context, exception);

        /// <inheritdoc />
        public override void HandlerAdded(IChannelHandlerContext context) => this.decoder.HandlerAdded(context);

        /// <inheritdoc />
        public override void HandlerRemoved(IChannelHandlerContext context) => this.decoder.HandlerRemoved(context);
    }
}
