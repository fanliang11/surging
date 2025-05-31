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
    using System.Collections.Generic;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    public class WebSocket00FrameEncoder : MessageToMessageEncoder<WebSocketFrame>, IWebSocketFrameEncoder
    {
        static readonly IByteBuffer _0X00 = Unpooled.UnreleasableBuffer(
            Unpooled.DirectBuffer(1, 1).WriteByte(0x00));

        static readonly IByteBuffer _0XFF = Unpooled.UnreleasableBuffer(
            Unpooled.DirectBuffer(1, 1).WriteByte(0xFF));

        static readonly IByteBuffer _0XFF_0X00 = Unpooled.UnreleasableBuffer(
            Unpooled.DirectBuffer(2, 2).WriteByte(0xFF).WriteByte(0x00));

        public override bool IsSharable => true;

        protected override void Encode(IChannelHandlerContext context, WebSocketFrame message, List<object> output)
        {
            switch (message.Opcode)
            {
                case Opcode.Text:
                    // Text frame
                    IByteBuffer data0 = message.Content;

                    output.Add(_0X00.Duplicate());
                    output.Add(data0.Retain());
                    output.Add(_0XFF.Duplicate());
                    break;
                case Opcode.Close:
                    // Close frame, needs to call duplicate to allow multiple writes.
                    // See https://github.com/netty/netty/issues/2768
                    output.Add(_0XFF_0X00.Duplicate());
                    break;
                default:
                    // Binary frame
                    IByteBuffer data = message.Content;
                    int dataLen = data.ReadableBytes;

                    IByteBuffer buf = context.Allocator.Buffer(5);
                    bool release = true;
                    try
                    {
                        // Encode type.
                        _ = buf.WriteByte(0x80);

                        // Encode length.
                        int b1 = dataLen.RightUShift(28) & 0x7F;
                        int b2 = dataLen.RightUShift(14) & 0x7F;
                        int b3 = dataLen.RightUShift(7) & 0x7F;
                        int b4 = dataLen & 0x7F;
                        if (0u >= (uint)b1)
                        {
                            if (0u >= (uint)b2)
                            {
                                if (0u >= (uint)b3)
                                {
                                    _ = buf.WriteByte(b4);
                                }
                                else
                                {
                                    _ = buf.WriteByte(b3 | 0x80);
                                    _ = buf.WriteByte(b4);
                                }
                            }
                            else
                            {
                                _ = buf.WriteByte(b2 | 0x80);
                                _ = buf.WriteByte(b3 | 0x80);
                                _ = buf.WriteByte(b4);
                            }
                        }
                        else
                        {
                            _ = buf.WriteByte(b1 | 0x80);
                            _ = buf.WriteByte(b2 | 0x80);
                            _ = buf.WriteByte(b3 | 0x80);
                            _ = buf.WriteByte(b4);
                        }

                        // Encode binary data.
                        output.Add(buf);
                        output.Add(data.Retain());
                        release = false;
                    }
                    finally
                    {
                        if (release) { _ = buf.Release(); }
                    }
                    break;
            }
        }
    }
}
