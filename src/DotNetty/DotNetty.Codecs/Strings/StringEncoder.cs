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

namespace DotNetty.Codecs
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DotNetty.Common.Internal;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Encodes the requested <see cref="String"/> into a <see cref="IByteBuffer"/>.
    /// A typical setup for a text-based line protocol in a TCP/IP socket would be:
    /// <code>
    /// <see cref="IChannelPipeline"/> pipeline = ...;
    ///
    /// // Decoders
    /// pipeline.addLast("frameDecoder", new <see cref="LineBasedFrameDecoder"/>(80));
    /// pipeline.addLast("stringDecoder", new <see cref="StringDecoder"/>);
    ///
    /// // Encoder
    /// pipeline.addLast("stringEncoder", new <see cref="StringEncoder"/>);
    /// </code>
    /// and then you can use a <see cref="String"/> instead of a <see cref="IByteBuffer"/>
    /// as a message:
    /// <code>
    /// void channelRead(<see cref="IChannelHandlerContext"/> ctx, <see cref="String"/> msg) {
    ///     ch.write("Did you say '" + msg + "'?\n");
    /// }
    /// </code>
    /// </summary>
    public class StringEncoder : MessageToMessageEncoder<string>
    {
        readonly Encoding _encoding;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringEncoder" /> class with the current system
        ///     character set.
        /// </summary>
        public StringEncoder()
            : this(TextEncodings.UTF8NoBOM)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringEncoder" /> class with the specified character
        ///     set..
        /// </summary>
        /// <param name="encoding">Encoding.</param>
        public StringEncoder(Encoding encoding)
        {
            if (encoding is null)
            {
                CThrowHelper.ThrowNullReferenceException(CExceptionArgument.encoding);
            }

            _encoding = encoding;
        }

        public override bool IsSharable => true;

        protected internal override void Encode(IChannelHandlerContext context, string message, List<object> output)
        {
            if (0u >= (uint)message.Length)
            {
                return;
            }

            output.Add(ByteBufferUtil.EncodeString(context.Allocator, message, _encoding));
        }
    }
}