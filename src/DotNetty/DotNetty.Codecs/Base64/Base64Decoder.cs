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

namespace DotNetty.Codecs.Base64
{
    using System.Collections.Generic;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;

    public sealed class Base64Decoder : MessageToMessageDecoder<IByteBuffer>
    {
        readonly IBase64Dialect dialect;

        public Base64Decoder()
            : this(Base64Dialect.Standard)
        {
        }

        public Base64Decoder(IBase64Dialect dialect)
        {
            this.dialect = dialect;
        }

        protected internal override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output) => output.Add(Base64.Decode(message, this.dialect));

        public override bool IsSharable => true;
    }
}