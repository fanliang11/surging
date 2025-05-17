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

namespace DotNetty.Codecs.Protobuf
{
    using System.Diagnostics;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    ///
    /// An encoder that prepends the the Google Protocol Buffers
    /// http://code.google.com/apis/protocolbuffers/docs/encoding.html#varints
    /// Base 128 Varints integer length field. 
    /// For example:
    /// 
    /// BEFORE ENCODE (300 bytes)       AFTER ENCODE (302 bytes)
    ///  +---------------+               +--------+---------------+
    ///  | Protobuf Data |-------------->| Length | Protobuf Data |
    ///  |  (300 bytes)  |               | 0xAC02 |  (300 bytes)  |
    ///  +---------------+               +--------+---------------+
    public class ProtobufVarint32LengthFieldPrepender : MessageToByteEncoder<IByteBuffer>
    {
        protected override void Encode(IChannelHandlerContext context, IByteBuffer message, IByteBuffer output)
        {
            Debug.Assert(context is object);
            Debug.Assert(message is object);
            Debug.Assert(output is object);

            int bodyLength = message.ReadableBytes;
            int headerLength = ComputeRawVarint32Size(bodyLength);
            _ = output.EnsureWritable(headerLength + bodyLength);

            WriteRawVarint32(output, bodyLength);
            _ = output.WriteBytes(message, message.ReaderIndex, bodyLength);
        }

        internal static void WriteRawVarint32(IByteBuffer output, int value)
        {
            Debug.Assert(output is object);

            while (true)
            {
                if (0u >= (uint)(value & ~0x7F))
                {
                    _ = output.WriteByte(value);
                    return;
                }

                _ = output.WriteByte((value & 0x7F) | 0x80);
                value >>= 7;
            }
        }

        public static int ComputeRawVarint32Size(int value)
        {
            if (0ul >= (ulong)(value & (0xffffffff << 7)))
            {
                return 1;
            }

            if (0ul >= (ulong)(value & (0xffffffff << 14)))
            {
                return 2;
            }

            if (0ul >= (ulong)(value & (0xffffffff << 21)))
            {
                return 3;
            }

            if (0ul >= (ulong)(value & (0xffffffff << 28)))
            {
                return 4;
            }

            return 5;
        }

        public override bool IsSharable => true;
    }
}
