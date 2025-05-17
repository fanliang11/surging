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

namespace DotNetty.Handlers.Logging
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    ///     A <see cref="IChannelHandler" /> that logs all events using a logging framework.
    ///     By default, all events are logged at <tt>DEBUG</tt> level.
    /// </summary>
    public class LoggingHandler : ChannelHandlerAdapter
    {
        const LogLevel DefaultLevel = LogLevel.DEBUG;

        protected readonly InternalLogLevel InternalLevel;
        protected readonly IInternalLogger Logger;
        protected readonly ByteBufferFormat BufferFormat;

        /// <summary>
        ///     Creates a new instance whose logger name is the fully qualified class
        ///     name of the instance with hex dump enabled.
        /// </summary>
        public LoggingHandler()
            : this(typeof(LoggingHandler), DefaultLevel, ByteBufferFormat.HexDump)
        {
        }

        /// <summary>
        ///     Creates a new instance whose logger name is the fully qualified class
        ///     name of the instance
        /// </summary>
        /// <param name="level">the log level</param>
        public LoggingHandler(LogLevel level)
            : this(typeof(LoggingHandler), level, ByteBufferFormat.HexDump)
        {
        }

        /// <summary>
        ///     Creates a new instance whose logger name is the fully qualified class
        ///     name of the instance
        /// </summary>
        /// <param name="level">the log level</param>
        /// <param name="bufferFormat">the ByteBuf format</param>
        public LoggingHandler(LogLevel level, ByteBufferFormat bufferFormat)
            : this(typeof(LoggingHandler), level, bufferFormat)
        {
        }

        /// <summary>
        ///     Creates a new instance with the specified logger name and with hex dump
        ///     enabled
        /// </summary>
        /// <param name="type">the class type to generate the logger for</param>
        public LoggingHandler(Type type)
            : this(type, DefaultLevel, ByteBufferFormat.HexDump)
        {
        }

        /// <summary>
        ///     Creates a new instance with the specified logger name.
        /// </summary>
        /// <param name="type">the class type to generate the logger for</param>
        /// <param name="level">the log level</param>
        public LoggingHandler(Type type, LogLevel level)
            : this(type, level, ByteBufferFormat.HexDump)
        {
        }

        /// <summary>
        ///     Creates a new instance with the specified logger name.
        /// </summary>
        /// <param name="type">the class type to generate the logger for</param>
        /// <param name="level">the log level</param>
        /// <param name="bufferFormat">the ByteBuf format</param>
        public LoggingHandler(Type type, LogLevel level, ByteBufferFormat bufferFormat)
        {
            if (type is null)
            {
                ThrowHelper.ThrowNullReferenceException(ExceptionArgument.type);
            }

            Logger = InternalLoggerFactory.GetInstance(type);
            Level = level;
            InternalLevel = level.ToInternalLevel();
            BufferFormat = bufferFormat;
        }

        /// <summary>
        ///     Creates a new instance with the specified logger name using the default log level.
        /// </summary>
        /// <param name="name">the name of the class to use for the logger</param>
        public LoggingHandler(string name)
            : this(name, DefaultLevel, ByteBufferFormat.HexDump)
        {
        }

        /// <summary>
        ///     Creates a new instance with the specified logger name.
        /// </summary>
        /// <param name="name">the name of the class to use for the logger</param>
        /// <param name="level">the log level</param>
        public LoggingHandler(string name, LogLevel level)
            : this(name, level, ByteBufferFormat.HexDump)
        {
        }

        /// <summary>
        ///     Creates a new instance with the specified logger name.
        /// </summary>
        /// <param name="name">the name of the class to use for the logger</param>
        /// <param name="level">the log level</param>
        /// <param name="bufferFormat">the ByteBuf format</param>
        public LoggingHandler(string name, LogLevel level, ByteBufferFormat bufferFormat)
        {
            if (name is null)
            {
                ThrowHelper.ThrowNullReferenceException(ExceptionArgument.name);
            }

            Logger = InternalLoggerFactory.GetInstance(name);
            Level = level;
            InternalLevel = level.ToInternalLevel();
            BufferFormat = bufferFormat;
        }

        public override bool IsSharable => true;

        /// <summary>
        ///     Returns the <see cref="LogLevel" /> that this handler uses to log
        /// </summary>
        public LogLevel Level { get; }

        public override void ChannelRegistered(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "REGISTERED"));
            }
            ctx.FireChannelRegistered();
        }

        public override void ChannelUnregistered(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "UNREGISTERED"));
            }
            ctx.FireChannelUnregistered();
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "ACTIVE"));
            }
            ctx.FireChannelActive();
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "INACTIVE"));
            }
            ctx.FireChannelInactive();
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "EXCEPTION", cause), cause);
            }
            ctx.FireExceptionCaught(cause);
        }

        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "USER_EVENT", evt));
            }
            ctx.FireUserEventTriggered(evt);
        }

        public override Task BindAsync(IChannelHandlerContext ctx, EndPoint localAddress)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "BIND", localAddress));
            }
            return ctx.BindAsync(localAddress);
        }

        public override Task ConnectAsync(IChannelHandlerContext ctx, EndPoint remoteAddress, EndPoint localAddress)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "CONNECT", remoteAddress, localAddress));
            }
            return ctx.ConnectAsync(remoteAddress, localAddress);
        }

        public override void Disconnect(IChannelHandlerContext ctx, IPromise promise)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "DISCONNECT"));
            }
            ctx.DisconnectAsync(promise);
        }

        public override void Close(IChannelHandlerContext ctx, IPromise promise)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "CLOSE"));
            }
            ctx.CloseAsync(promise);
        }

        public override void Deregister(IChannelHandlerContext ctx, IPromise promise)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "DEREGISTER"));
            }
            ctx.DeregisterAsync(promise);
        }

        public override void ChannelRead(IChannelHandlerContext ctx, object message)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "RECEIVED", message));
            }
            ctx.FireChannelRead(message);
        }

        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "RECEIVED_COMPLETE"));
            }
            ctx.FireChannelReadComplete();
        }

        public override void ChannelWritabilityChanged(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "WRITABILITY", ctx.Channel.IsWritable));
            }
            ctx.FireChannelWritabilityChanged();
        }

        public override void HandlerAdded(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "HANDLER_ADDED"));
            }
        }
        public override void HandlerRemoved(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "HANDLER_REMOVED"));
            }
        }

        public override void Read(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "READ"));
            }
            ctx.Read();
        }

        public override void Write(IChannelHandlerContext ctx, object msg, IPromise promise)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "WRITE", msg));
            }
            ctx.WriteAsync(msg, promise);
        }

        public override void Flush(IChannelHandlerContext ctx)
        {
            if (Logger.IsEnabled(InternalLevel))
            {
                Logger.Log(InternalLevel, Format(ctx, "FLUSH"));
            }
            ctx.Flush();
        }

        /// <summary>
        ///     Formats an event and returns the formatted message
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="eventName">the name of the event</param>
        protected virtual string Format(IChannelHandlerContext ctx, string eventName)
        {
            string chStr = ctx.Channel.ToString();
            var sb = StringBuilderManager.Allocate(chStr.Length + 1 + eventName.Length)
                .Append(chStr)
                .Append(' ')
                .Append(eventName);
            return StringBuilderManager.ReturnAndFree(sb);
        }

        /// <summary>
        ///     Formats an event and returns the formatted message.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="eventName">the name of the event</param>
        /// <param name="arg">the argument of the event</param>
        protected virtual string Format(IChannelHandlerContext ctx, string eventName, object arg) => arg switch
        {
            IByteBuffer byteBuffer => FormatByteBuffer(ctx, eventName, byteBuffer),
            IByteBufferHolder byteBufferHolder => FormatByteBufferHolder(ctx, eventName, byteBufferHolder),
            _ => FormatSimple(ctx, eventName, arg),
        };

        /// <summary>
        ///     Formats an event and returns the formatted message.  This method is currently only used for formatting
        ///     <see cref="IChannelHandler.ConnectAsync(IChannelHandlerContext, EndPoint, EndPoint)" />
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="eventName">the name of the event</param>
        /// <param name="firstArg">the first argument of the event</param>
        /// <param name="secondArg">the second argument of the event</param>
        protected virtual string Format(IChannelHandlerContext ctx, string eventName, object firstArg, object secondArg)
        {
            if (secondArg is null)
            {
                return FormatSimple(ctx, eventName, firstArg);
            }
            string chStr = ctx.Channel.ToString();
            string arg1Str = firstArg.ToString();
            string arg2Str = secondArg.ToString();

            var buf = StringBuilderManager.Allocate(
                chStr.Length + 1 + eventName.Length + 2 + arg1Str.Length + 2 + arg2Str.Length);
            buf.Append(chStr).Append(' ').Append(eventName).Append(": ")
                .Append(arg1Str).Append(", ").Append(arg2Str);
            return StringBuilderManager.ReturnAndFree(buf);
        }

        /// <summary>
        ///     Generates the default log message of the specified event whose argument is a  <see cref="IByteBuffer" />.
        /// </summary>
        string FormatByteBuffer(IChannelHandlerContext ctx, string eventName, IByteBuffer msg)
        {
            string chStr = ctx.Channel.ToString();
            int length = msg.ReadableBytes;
            if (0u >= (uint)length)
            {
                var buf = StringBuilderManager.Allocate(chStr.Length + 1 + eventName.Length + 4);
                buf.Append(chStr).Append(' ').Append(eventName).Append(": 0B");
                return StringBuilderManager.ReturnAndFree(buf);
            }
            else
            {
                int outputLength = chStr.Length + 1 + eventName.Length + 2 + 10 + 1;
                if (BufferFormat == ByteBufferFormat.HexDump)
                {
                    int rows = length / 16 + (length % 15 == 0 ? 0 : 1) + 4;
                    int hexDumpLength = 2 + rows * 80;
                    outputLength += hexDumpLength;
                }
                var buf = StringBuilderManager.Allocate(outputLength);
                buf.Append(chStr).Append(' ').Append(eventName).Append(": ").Append(length).Append('B');
                if (BufferFormat == ByteBufferFormat.HexDump)
                {
                    buf.Append(StringUtil.Newline);
                    ByteBufferUtil.AppendPrettyHexDump(buf, msg);
                }

                return StringBuilderManager.ReturnAndFree(buf);
            }
        }

        /// <summary>
        ///     Generates the default log message of the specified event whose argument is a <see cref="IByteBufferHolder" />.
        /// </summary>
        string FormatByteBufferHolder(IChannelHandlerContext ctx, string eventName, IByteBufferHolder msg)
        {
            string chStr = ctx.Channel.ToString();
            string msgStr = msg.ToString();
            IByteBuffer content = msg.Content;
            int length = content.ReadableBytes;
            if (0u >= (uint)length)
            {
                var buf = StringBuilderManager.Allocate(chStr.Length + 1 + eventName.Length + 2 + msgStr.Length + 4);
                buf.Append(chStr).Append(' ').Append(eventName).Append(", ").Append(msgStr).Append(", 0B");
                return StringBuilderManager.ReturnAndFree(buf);
            }
            else
            {
                int outputLength = chStr.Length + 1 + eventName.Length + 2 + msgStr.Length + 2 + 10 + 1;
                if (BufferFormat == ByteBufferFormat.HexDump)
                {
                    int rows = length / 16 + (length % 15 == 0 ? 0 : 1) + 4;
                    int hexDumpLength = 2 + rows * 80;
                    outputLength += hexDumpLength;
                }
                var buf = StringBuilderManager.Allocate(outputLength);
                buf.Append(chStr).Append(' ').Append(eventName).Append(": ")
                   .Append(msgStr).Append(", ").Append(length).Append('B');
                if (BufferFormat == ByteBufferFormat.HexDump)
                {
                    buf.Append(StringUtil.Newline);
                    ByteBufferUtil.AppendPrettyHexDump(buf, content);
                }

                return StringBuilderManager.ReturnAndFree(buf);
            }
        }

        /// <summary>
        ///     Generates the default log message of the specified event whose argument is an arbitrary object.
        /// </summary>
        string FormatSimple(IChannelHandlerContext ctx, string eventName, object msg)
        {
            string chStr = ctx.Channel.ToString();
            string msgStr = msg.ToString();
            var buf = StringBuilderManager.Allocate(chStr.Length + 1 + eventName.Length + 2 + msgStr.Length);
            return StringBuilderManager.ReturnAndFree(buf.Append(chStr).Append(' ').Append(eventName).Append(": ").Append(msgStr));
        }
    }
}