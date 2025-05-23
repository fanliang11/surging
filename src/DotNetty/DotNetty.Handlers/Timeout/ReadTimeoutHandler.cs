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

namespace DotNetty.Handlers.Timeout
{
    using System;
    using System.Diagnostics;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Raises a <see cref="ReadTimeoutException"/> when no data was read within a certain
    /// period of time.
    ///
    /// <pre>
    /// The connection is closed when there is no inbound traffic
    /// for 30 seconds.
    ///
    /// <example>
    /// <c>
    /// var bootstrap = new <see cref="DotNetty.Transport.Bootstrapping.ServerBootstrap"/>();
    ///
    /// bootstrap.ChildHandler(new ActionChannelInitializer&lt;ISocketChannel&gt;(channel =>
    /// {
    ///     IChannelPipeline pipeline = channel.Pipeline;
    ///     
    ///     pipeline.AddLast("readTimeoutHandler", new <see cref="ReadTimeoutHandler"/>(30));
    ///     pipeline.AddLast("myHandler", new MyHandler());
    /// } 
    /// </c>
    ///            
    /// <c>
    /// public class MyHandler : ChannelDuplexHandler 
    /// {
    ///     public override void ExceptionCaught(<see cref="IChannelHandlerContext"/> context, <see cref="Exception"/> exception)
    ///     {
    ///         if(exception is <see cref="ReadTimeoutException"/>) 
    ///         {
    ///             // do somethind
    ///         }
    ///         else
    ///         {
    ///             base.ExceptionCaught(context, cause);
    ///         }
    ///      }
    /// }
    /// </c>
    /// </example>
    /// </pre>
    /// 
    /// <seealso cref="WriteTimeoutHandler"/>
    /// <seealso cref="IdleStateHandler"/>
    /// </summary>
    public class ReadTimeoutHandler : IdleStateHandler
    {
        bool _closed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetty.Handlers.Timeout.ReadTimeoutHandler"/> class.
        /// </summary>
        /// <param name="timeoutSeconds">Timeout in seconds.</param>
        public ReadTimeoutHandler(int timeoutSeconds)
            : this(TimeSpan.FromSeconds(timeoutSeconds))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetty.Handlers.Timeout.ReadTimeoutHandler"/> class.
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public ReadTimeoutHandler(TimeSpan timeout)
            : base(timeout, TimeSpan.Zero, TimeSpan.Zero)
        {
        }

        protected override void ChannelIdle(IChannelHandlerContext context, IdleStateEvent stateEvent)
        {
            Debug.Assert(stateEvent.State == IdleState.ReaderIdle);
            ReadTimedOut(context);
        }

        /// <summary>
        /// Is called when a read timeout was detected.
        /// </summary>
        /// <param name="context">Context.</param>
        protected virtual void ReadTimedOut(IChannelHandlerContext context)
        {
            if(!_closed)
            {
                context.FireExceptionCaught(ReadTimeoutException.Instance);
                context.CloseAsync();
                _closed = true;
            }
        }
    }
}

