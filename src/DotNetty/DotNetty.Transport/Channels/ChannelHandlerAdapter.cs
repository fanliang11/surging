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
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;

    public class ChannelHandlerAdapter : IChannelHandler
    {
        internal bool Added;

        [Skip]
        public virtual void ChannelRegistered(IChannelHandlerContext context) => context.FireChannelRegistered();

        [Skip]
        public virtual void ChannelUnregistered(IChannelHandlerContext context) => context.FireChannelUnregistered();

        [Skip]
        public virtual void ChannelActive(IChannelHandlerContext context) => context.FireChannelActive();

        [Skip]
        public virtual void ChannelInactive(IChannelHandlerContext context) => context.FireChannelInactive();

        [Skip]
        public virtual void ChannelRead(IChannelHandlerContext context, object message) => context.FireChannelRead(message);

        [Skip]
        public virtual void ChannelReadComplete(IChannelHandlerContext context) => context.FireChannelReadComplete();

        [Skip]
        public virtual void ChannelWritabilityChanged(IChannelHandlerContext context) => context.FireChannelWritabilityChanged();

        public virtual void HandlerAdded(IChannelHandlerContext context)
        {
        }

        public virtual void HandlerRemoved(IChannelHandlerContext context)
        {
        }

        [Skip]
        public virtual void UserEventTriggered(IChannelHandlerContext context, object evt) => context.FireUserEventTriggered(evt);

        [Skip]
        public virtual void Write(IChannelHandlerContext context, object message, IPromise promise) => context.WriteAsync(message, promise);

        [Skip]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Flush(IChannelHandlerContext context) => context.Flush();

        [Skip]
        public virtual Task BindAsync(IChannelHandlerContext context, EndPoint localAddress) => context.BindAsync(localAddress);

        [Skip]
        public virtual Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress) => context.ConnectAsync(remoteAddress, localAddress);

        [Skip]
        public virtual void Disconnect(IChannelHandlerContext context, IPromise promise) => context.DisconnectAsync(promise);

        [Skip]
        public virtual void Close(IChannelHandlerContext context, IPromise promise) => context.CloseAsync(promise);

        [Skip]
        public virtual void ExceptionCaught(IChannelHandlerContext context, Exception exception) => context.FireExceptionCaught(exception);

        [Skip]
        public virtual void Deregister(IChannelHandlerContext context, IPromise promise) => context.DeregisterAsync(promise);

        [Skip]
        public virtual void Read(IChannelHandlerContext context) => context.Read();

        public virtual bool IsSharable => false;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        protected void EnsureNotSharable()
        {
            if (IsSharable)
            {
                ThrowHelper.ThrowInvalidOperationException_EnsureNotSharable(this);
            }
        }
    }
}