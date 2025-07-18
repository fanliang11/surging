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

namespace DotNetty.Handlers.Logging
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Transport.Channels;
    using Microsoft.Extensions.Logging;
    using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

    /// <summary>A <see cref="IChannelHandler" /> that logs all events using a logging framework.
    /// By default, all events are logged at <tt>DEBUG</tt> level.</summary>
    public class MsLoggingHandler : ChannelHandlerAdapter
    {
        protected readonly ILogger Logger;

        /// <summary>Creates a new instance whose logger name is the fully qualified class
        /// name of the instance with hex dump enabled.</summary>
        public MsLoggingHandler() : this(typeof(LoggingHandler)) { }

        /// <summary>Creates a new instance with the specified logger name.</summary>
        /// <param name="type">the class type to generate the logger for</param>
        public MsLoggingHandler(Type type)
        {
            if (type is null) { ThrowHelper.ThrowNullReferenceException(ExceptionArgument.type); }

            Logger = InternalLoggerFactory.DefaultFactory.CreateLogger(type);
        }

        /// <summary>Creates a new instance with the specified logger name using the default log level.</summary>
        /// <param name="name">the name of the class to use for the logger</param>
        public MsLoggingHandler(string name)
        {
            if (name is null) { ThrowHelper.ThrowNullReferenceException(ExceptionArgument.name); }

            Logger = InternalLoggerFactory.DefaultFactory.CreateLogger(name);
        }

        /// <inheritdoc />
        public override bool IsSharable => true;

        /// <inheritdoc />
        public override void ChannelRegistered(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(MsLogLevel.Debug))
            {
                Logger.LogDebug("Channel {0} registered", ctx.Channel);
            }
            ctx.FireChannelRegistered();
        }

        /// <inheritdoc />
        public override void ChannelUnregistered(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(MsLogLevel.Debug))
            {
                Logger.LogDebug("Channel {0} unregistered", ctx.Channel);
            }
            ctx.FireChannelUnregistered();
        }

        /// <inheritdoc />
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(MsLogLevel.Debug))
            {
                Logger.LogDebug("Channel {0} active", ctx.Channel);
            }
            ctx.FireChannelActive();
        }

        /// <inheritdoc />
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(MsLogLevel.Debug))
            {
                Logger.LogDebug("Channel {0} inactive", ctx.Channel);
            }
            ctx.FireChannelInactive();
        }

        /// <inheritdoc />
        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            if (Logger.IsEnabled(MsLogLevel.Warning))
            {
                Logger.LogError(cause, "Channel {0} caught exception", ctx.Channel);
            }
            ctx.FireExceptionCaught(cause);
        }

        /// <inheritdoc />
        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            if (Logger.IsEnabled(MsLogLevel.Debug))
            {
                Logger.LogDebug("Channel {0} triggered user event [{1}]", ctx.Channel, evt);
            }
            ctx.FireUserEventTriggered(evt);
        }

        /// <inheritdoc />
        public override Task BindAsync(IChannelHandlerContext ctx, EndPoint localAddress)
        {
            if (Logger.IsEnabled(MsLogLevel.Information))
            {
                Logger.LogInformation("Channel {0} bind to address {1}", ctx.Channel, localAddress);
            }
            return ctx.BindAsync(localAddress);
        }

        /// <inheritdoc />
        public override Task ConnectAsync(IChannelHandlerContext ctx, EndPoint remoteAddress, EndPoint localAddress)
        {
            if (Logger.IsEnabled(MsLogLevel.Information))
            {
                Logger.LogInformation("Channel {0} connect (remote: {1}, local: {2})", ctx.Channel, remoteAddress, localAddress);
            }
            return ctx.ConnectAsync(remoteAddress, localAddress);
        }

        /// <inheritdoc />
        public override void Disconnect(IChannelHandlerContext ctx, IPromise promise)
        {
            if (Logger.IsEnabled(MsLogLevel.Information))
            {
                Logger.LogInformation("Channel {0} disconnect", ctx.Channel);
            }
            ctx.DisconnectAsync(promise);
        }

        /// <inheritdoc />
        public override void Close(IChannelHandlerContext ctx, IPromise promise)
        {
            if (Logger.IsEnabled(MsLogLevel.Information))
            {
                Logger.LogInformation("Channel {0} close", ctx.Channel);
            }
            ctx.CloseAsync(promise);
        }

        /// <inheritdoc />
        public override void Deregister(IChannelHandlerContext ctx, IPromise promise)
        {
            if (Logger.IsEnabled(MsLogLevel.Debug))
            {
                Logger.LogDebug("Channel {0} deregister", ctx.Channel);
            }
            ctx.DeregisterAsync(promise);
        }

        /// <inheritdoc />
        public override void ChannelRead(IChannelHandlerContext ctx, object message)
        {
            if (Logger.IsEnabled(MsLogLevel.Trace))
            {
                Logger.LogTrace("Channel {0} received : {1}", ctx.Channel, FormatMessage(message));
            }
            ctx.FireChannelRead(message);
        }

        /// <inheritdoc />
        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(MsLogLevel.Trace))
            {
                Logger.LogTrace("Channel {0} receive complete", ctx.Channel);
            }
            ctx.FireChannelReadComplete();
        }

        /// <inheritdoc />
        public override void ChannelWritabilityChanged(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(MsLogLevel.Debug))
            {
                Logger.LogDebug("Channel {0} writability", ctx.Channel);
            }
            ctx.FireChannelWritabilityChanged();
        }

        /// <inheritdoc />
        public override void HandlerAdded(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(MsLogLevel.Debug))
            {
                Logger.LogDebug("Channel {0} handler added", ctx.Channel);
            }
        }

        /// <inheritdoc />
        public override void HandlerRemoved(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(MsLogLevel.Debug))
            {
                Logger.LogDebug("Channel {0} handler removed", ctx.Channel);
            }
        }

        /// <inheritdoc />
        public override void Read(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(MsLogLevel.Trace))
            {
                Logger.LogTrace("Channel {0} reading", ctx.Channel);
            }
            ctx.Read();
        }

        /// <inheritdoc />
        public override void Write(IChannelHandlerContext ctx, object msg, IPromise promise)
        {
            if (Logger.IsEnabled(MsLogLevel.Trace))
            {
                Logger.LogTrace("Channel {0} writing: {1}", ctx.Channel, FormatMessage(msg));
            }
            ctx.WriteAsync(msg, promise);
        }

        /// <inheritdoc />
        public override void Flush(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(MsLogLevel.Trace))
            {
                Logger.LogTrace("Channel {0} flushing", ctx.Channel);
            }
            ctx.Flush();
        }

        /// <summary>Formats an event and returns the formatted message.</summary>
        /// <param name="arg">the argument of the event</param>
        public static object FormatMessage(object arg) => arg switch
        {
            IByteBuffer byteBuffer => FormatByteBuffer(byteBuffer),
            IByteBufferHolder byteBufferHolder => FormatByteBufferHolder(byteBufferHolder),
            _ => arg,
        };

        /// <summary>Generates the default log message of the specified event whose argument is a  <see cref="IByteBuffer" />.</summary>
        public static string FormatByteBuffer(IByteBuffer msg)
        {
            int length = msg.ReadableBytes;
            if (0u >= (uint)length)
            {
                return $"0B";
            }
            else
            {
                int rows = length / 16 + (length % 15 == 0 ? 0 : 1) + 4;
                var buf = StringBuilderManager.Allocate(10 + 1 + 2 + rows * 80);

                buf.Append(length).Append('B').Append('\n');
                ByteBufferUtil.AppendPrettyHexDump(buf, msg);

                return StringBuilderManager.ReturnAndFree(buf);
            }
        }

        /// <summary>Generates the default log message of the specified event whose argument is a <see cref="IByteBufferHolder" />.</summary>
        public static string FormatByteBufferHolder(IByteBufferHolder msg)
        {
            string msgStr = msg.ToString();
            IByteBuffer content = msg.Content;
            int length = content.ReadableBytes;
            if (0u >= (uint)length)
            {
                return $"{msgStr}, 0B";
            }
            else
            {
                int rows = length / 16 + (length % 15 == 0 ? 0 : 1) + 4;
                var buf = StringBuilderManager.Allocate(msgStr.Length + 2 + 10 + 1 + 2 + rows * 80);

                buf.Append(msgStr).Append(", ").Append(length).Append('B').Append('\n');
                ByteBufferUtil.AppendPrettyHexDump(buf, content);

                return StringBuilderManager.ReturnAndFree(buf);
            }
        }
    }
}