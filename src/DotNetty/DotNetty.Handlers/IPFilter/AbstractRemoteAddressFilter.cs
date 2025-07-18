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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Handlers.IPFilter
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// This class provides the functionality to either accept or reject new <see cref="IChannel"/>s
    /// based on their IP address.
    /// You should inherit from this class if you would like to implement your own IP-based filter. Basically you have to
    /// implement <see cref="Accept"/> to decided whether you want to accept or reject
    /// a connection from the remote address.
    /// Furthermore overriding <see cref="ChannelRejected"/> gives you the
    /// flexibility to respond to rejected (denied) connections. If you do not want to send a response, just have it return
    /// null. Take a look at <see cref="RuleBasedIPFilter"/> for details.
    /// </summary>
    public abstract class AbstractRemoteAddressFilter<T>: ChannelHandlerAdapter where T:EndPoint
    {
        public override void ChannelRegistered(IChannelHandlerContext ctx)
        {
            this.HandleNewChannel(ctx);
            ctx.FireChannelRegistered();
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            if (!this.HandleNewChannel(ctx))
            {
                ThrowHelper.ThrowArgumentException_CannotDetermineToAcceptOrRejectAChannel(ctx);
            }
            else
            {
                ctx.FireChannelActive();
            }
        }

        bool HandleNewChannel(IChannelHandlerContext ctx)
        {
            var remoteAddress = (T)ctx.Channel.RemoteAddress;
            
            // If the remote address is not available yet, defer the decision.
            if (remoteAddress is null)
            {
                return false;
            }
            
            // No need to keep this handler in the pipeline anymore because the decision is going to be made now.
            // Also, this will prevent the subsequent events from being handled by this handler.
            ctx.Pipeline.Remove(this);
            if (this.Accept(ctx, remoteAddress))
            {
                this.ChannelAccepted(ctx, remoteAddress);
            }
            else
            {
                Task rejectedTask = this.ChannelRejected(ctx, remoteAddress);
                if (rejectedTask is object)
                {
                    rejectedTask.CloseOnComplete(ctx);
                }
                else
                {
                    ctx.CloseAsync();    
                }
            }
            return true;
        }
        
        /// <summary>
        /// This method is called immediately after a <see cref="IChannel"/> gets registered.
        /// </summary>
        /// <returns>Return true if connections from this IP address and port should be accepted. False otherwise.</returns>
        protected abstract bool Accept(IChannelHandlerContext ctx, T remoteAddress);
        
        /// <summary>
        /// This method is called if <paramref name="remoteAddress"/> gets accepted by
        /// <see cref="Accept"/>.  You should override it if you would like to handle
        /// (e.g. respond to) accepted addresses.
        /// </summary>
        protected virtual void ChannelAccepted(IChannelHandlerContext ctx, T remoteAddress) { }
        

        /// <summary>
        /// This method is called if <paramref name="remoteAddress"/> gets rejected by
        /// <see cref="Accept"/>.  You should override it if you would like to handle
        /// (e.g. respond to) rejected addresses.
        /// <returns>
        /// A <see cref="Task"/> if you perform I/O operations, so that
        /// the <see cref="IChannel"/> can be closed once it completes. Null otherwise.
        /// </returns>
        /// </summary>
        protected virtual Task ChannelRejected(IChannelHandlerContext ctx, T remoteAddress)
        {
            return null;
        }
    }
}