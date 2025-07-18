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
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Concurrency;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;

    /// <summary>
    /// An encoder that encodes the content in <see cref="IAddressedEnvelope{T}"/> to <see cref="DatagramPacket"/> using
    /// the specified message encoder. E.g.,
    ///
    /// <code>
    /// <see cref="IChannelPipeline"/> pipeline = ...;
    /// pipeline.addLast("udpEncoder", new <see cref="DatagramPacketEncoder{T}"/>(new <see cref="T:ProtobufEncoder"/>(...));
    /// </code>
    ///
    /// Note: As UDP packets are out-of-order, you should make sure the encoded message size are not greater than
    /// the max safe packet size in your particular network path which guarantees no packet fragmentation.
    /// </summary>
    /// <typeparam name="T">the type of message to be encoded</typeparam>
    public class DatagramPacketEncoder<T> : MessageToMessageEncoder<IAddressedEnvelope<T>>
    {
        readonly MessageToMessageEncoder<T> encoder;

        /// <summary>
        /// Create an encoder that encodes the content in <see cref="IAddressedEnvelope{T}"/> to <see cref="DatagramPacket"/> using
        /// the specified message encoder.
        /// </summary>
        /// <param name="encoder">the specified message encoder</param>
        public DatagramPacketEncoder(MessageToMessageEncoder<T> encoder)
        {
            if (encoder is null) { CThrowHelper.ThrowArgumentNullException(CExceptionArgument.encoder); }

            this.encoder = encoder;
        }

        /// <inheritdoc />
        public override bool AcceptOutboundMessage(object msg)
        {
            return msg is IAddressedEnvelope<T> envelope
                && this.encoder.AcceptOutboundMessage(envelope.Content)
                && (/*envelope.Sender is object ||*/ envelope.Recipient is object); // Allow null sender when using DatagramPacketEncoder
        }

        /// <inheritdoc />
        protected internal override void Encode(IChannelHandlerContext context, IAddressedEnvelope<T> message, List<object> output)
        {
            this.encoder.Encode(context, message.Content, output);
            if (output.Count != 1) {
                CThrowHelper.ThrowEncoderException_MustProduceOnlyOneMsg(this.encoder.GetType());
            }

            var content = output[0] as IByteBuffer;
            if (content is null)
            {
                CThrowHelper.ThrowEncoderException_MustProduceOnlyByteBuf(this.encoder.GetType());
            }

            // Replace the ByteBuf with a DatagramPacket.
            output[0] = new DatagramPacket(content, message.Sender, message.Recipient);
        }

        /// <inheritdoc />
        public override Task BindAsync(IChannelHandlerContext context, EndPoint localAddress) => 
            this.encoder.BindAsync(context, localAddress);

        /// <inheritdoc />
        public override Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress) => 
            this.encoder.ConnectAsync(context, remoteAddress, localAddress);

        /// <inheritdoc />
        public override void Disconnect(IChannelHandlerContext context, IPromise promise) => this.encoder.Disconnect(context, promise);

        /// <inheritdoc />
        public override void Close(IChannelHandlerContext context, IPromise promise) => this.encoder.Close(context, promise);

        /// <inheritdoc />
        public override void Deregister(IChannelHandlerContext context, IPromise promise) => this.encoder.Deregister(context, promise);

        /// <inheritdoc />
        public override void Read(IChannelHandlerContext context) => this.encoder.Read(context);

        /// <inheritdoc />
        public override void Flush(IChannelHandlerContext context) => this.encoder.Flush(context);

        /// <inheritdoc />
        public override void HandlerAdded(IChannelHandlerContext context) => this.encoder.HandlerAdded(context);

        /// <inheritdoc />
        public override void HandlerRemoved(IChannelHandlerContext context) => this.encoder.HandlerRemoved(context);

        /// <inheritdoc />
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception) => 
            this.encoder.ExceptionCaught(context, exception);
    }
}
