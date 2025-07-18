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


namespace DotNetty.Transport.Channels
{
    using System.Net;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    public class DefaultAddressedEnvelope<T> : IAddressedEnvelope<T>
    {
        public DefaultAddressedEnvelope(T content, EndPoint recipient)
            : this(content, null, recipient)
        {
        }

        public DefaultAddressedEnvelope(T content, EndPoint sender, EndPoint recipient)
        {
            if (content is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.content); }
            if (recipient is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.recipient); }

            Content = content;
            Sender = sender;
            Recipient = recipient;
        }

        public T Content { get; }

        public EndPoint Sender { get; }

        public EndPoint Recipient { get; }

        public int ReferenceCount
        {
            get
            {
                var counted = Content as IReferenceCounted;
                return counted?.ReferenceCount ?? 1;
            }
        }

        public virtual IReferenceCounted Retain()
        {
            _ = ReferenceCountUtil.Retain(Content);
            return this;
        }

        public virtual IReferenceCounted Retain(int increment)
        {
            _ = ReferenceCountUtil.Retain(Content, increment);
            return this;
        }

        public virtual IReferenceCounted Touch()
        {
            _ = ReferenceCountUtil.Touch(Content);
            return this;
        }

        public virtual IReferenceCounted Touch(object hint)
        {
            _ = ReferenceCountUtil.Touch(Content, hint);
            return this;
        }

        public bool Release() => ReferenceCountUtil.Release(Content);

        public bool Release(int decrement) => ReferenceCountUtil.Release(Content, decrement);

        public override string ToString() => $"DefaultAddressedEnvelope<{typeof(T)}>"
            + (Sender is object
                ? $"({Sender} => {Recipient}, {Content})"
                : $"(=> {Recipient}, {Content})");
    }
}