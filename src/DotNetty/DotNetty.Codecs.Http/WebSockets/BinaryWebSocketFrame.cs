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
    using DotNetty.Buffers;

    /// <summary>
    /// Web Socket frame containing binary data.
    /// </summary>
    public class BinaryWebSocketFrame : WebSocketFrame
    {
        /// <summary>
        /// Creates a new empty binary frame.
        /// </summary>
        public BinaryWebSocketFrame() 
            : base(true, 0, Opcode.Binary, ArrayPooled.Buffer(0))
        {
        }

        /// <summary>
        /// Creates a new binary frame with the specified binary data. The final fragment flag is set to true.
        /// </summary>
        /// <param name="binaryData">the content of the frame.</param>
        public BinaryWebSocketFrame(IByteBuffer binaryData)
            : base(true, 0, Opcode.Binary, binaryData)
        {
        }

        /// <summary>
        /// Creates a new binary frame with the specified binary data and the final fragment flag.
        /// </summary>
        /// <param name="finalFragment">flag indicating if this frame is the final fragment</param>
        /// <param name="binaryData">the content of the frame.</param>
        public BinaryWebSocketFrame(bool finalFragment, IByteBuffer binaryData)
            : base(finalFragment, 0, Opcode.Binary, binaryData)
        {
        }

        /// <summary>
        /// Creates a new binary frame with the specified binary data and the final fragment flag.
        /// </summary>
        /// <param name="finalFragment">flag indicating if this frame is the final fragment</param>
        /// <param name="rsv">reserved bits used for protocol extensions</param>
        /// <param name="binaryData">the content of the frame.</param>
        public BinaryWebSocketFrame(bool finalFragment, int rsv, IByteBuffer binaryData)
            : base(finalFragment, rsv, Opcode.Binary, binaryData)
        {
        }

        /// <inheritdoc />
        public override IByteBufferHolder Replace(IByteBuffer content) => new BinaryWebSocketFrame(this.IsFinalFragment, this.Rsv, content);
    }
}
