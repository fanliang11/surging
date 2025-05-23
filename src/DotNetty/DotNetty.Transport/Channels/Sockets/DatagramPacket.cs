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
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Transport.Channels.Sockets
{
    using System.Net;
    using DotNetty.Buffers;
    using DotNetty.Common;

    public sealed class DatagramPacket : DefaultAddressedEnvelope<IByteBuffer>, IByteBufferHolder
    {
        public DatagramPacket(IByteBuffer message, EndPoint recipient)
            : base(message, recipient)
        {
        }

        public DatagramPacket(IByteBuffer message, EndPoint sender, EndPoint recipient)
            : base(message, sender, recipient)
        {
        }

        public IByteBufferHolder Copy() => new DatagramPacket(Content.Copy(), Sender, Recipient);

        public IByteBufferHolder Duplicate() => new DatagramPacket(Content.Duplicate(), Sender, Recipient);

        public IByteBufferHolder RetainedDuplicate() => Replace(Content.RetainedDuplicate());

        public IByteBufferHolder Replace(IByteBuffer content) => new DatagramPacket(content, Recipient, Sender);

        public override IReferenceCounted Retain()
        {
            _ = base.Retain();
            return this;
        }

        public override IReferenceCounted Retain(int increment)
        {
            _ = base.Retain(increment);
            return this;
        }

        public override IReferenceCounted Touch()
        {
            _ = base.Touch();
            return this;
        }

        public override IReferenceCounted Touch(object hint)
        {
            _ = base.Touch(hint);
            return this;
        }
    }
}