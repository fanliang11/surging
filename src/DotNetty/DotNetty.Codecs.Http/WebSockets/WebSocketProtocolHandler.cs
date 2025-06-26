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
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    public abstract class WebSocketProtocolHandler : MessageToMessageDecoder<WebSocketFrame>
    {
        private static readonly Action<Task, object> AbortCloseSetAction = (t, s) => AbortCloseSet(t, s);

        internal readonly bool DropPongFrames;
        private readonly WebSocketCloseStatus _closeStatus;
        private readonly long _forceCloseTimeoutMillis;
        private IPromise _closeSent;

        /// <summary>
        /// Creates a new <see cref="WebSocketProtocolHandler"/> that will <i>drop</i> <see cref="PongWebSocketFrame"/>s.
        /// </summary>
        internal WebSocketProtocolHandler()
            : this(true, null, 0L)
        {
        }

        /// <summary>
        /// Creates a new <see cref="WebSocketProtocolHandler"/>, given a parameter that determines whether or not to drop
        /// <see cref="PongWebSocketFrame"/>s.
        /// </summary>
        /// <param name="dropPongFrames"></param>
        internal WebSocketProtocolHandler(bool dropPongFrames)
            : this(dropPongFrames, null, 0L)
        {
        }

        internal WebSocketProtocolHandler(
            bool dropPongFrames, WebSocketCloseStatus closeStatus, long forceCloseTimeoutMillis)
        {
            DropPongFrames = dropPongFrames;
            _closeStatus = closeStatus;
            _forceCloseTimeoutMillis = forceCloseTimeoutMillis;
        }

        protected override void Decode(IChannelHandlerContext ctx, WebSocketFrame frame, List<object> output)
        {
            // 须同时修改 WebSocketServerProtocolHandler & WebSocketClientProtocolHandler
            switch (frame.Opcode)
            {
                case Opcode.Ping:
                    var contect = frame.Content;
                    _ = contect.Retain();
                    _ = ctx.Channel.WriteAndFlushAsync(new PongWebSocketFrame(contect));
                    ReadIfNeeded(ctx);
                    return;

                case Opcode.Pong when DropPongFrames:
                    // Pong frames need to get ignored
                    ReadIfNeeded(ctx);
                    return;

                default:
                    output.Add(frame.Retain());
                    break;
            }
        }

        protected static void ReadIfNeeded(IChannelHandlerContext ctx)
        {
            if (!ctx.Channel.Configuration.IsAutoRead)
            {
                _ = ctx.Read();
            }
        }

        public override void Close(IChannelHandlerContext ctx, IPromise promise)
        {
            if (_closeStatus is null || !ctx.Channel.IsActive)
            {
                _ = ctx.CloseAsync(promise);
            }
            else
            {
                if (_closeSent is null)
                {
                    Write(ctx, new CloseWebSocketFrame(_closeStatus), ctx.NewPromise());
                }
                Flush(ctx);
                ApplyCloseSentTimeout(ctx);
                _ = _closeSent.Task.CloseOnComplete(ctx, promise);
            }
        }

        public override void Write(IChannelHandlerContext ctx, object message, IPromise promise)
        {
            if (_closeSent is object)
            {
                _ = ReferenceCountUtil.Release(message);
                promise.SetException(ThrowHelper.GetClosedChannelException());
            }
            else if (message is CloseWebSocketFrame)
            {
                _closeSent = promise.Unvoid();
                ctx.WriteAsync(message).LinkOutcome(_closeSent);
            }
            else
            {
                _ = ctx.WriteAsync(message, promise);
            }
        }

        private void ApplyCloseSentTimeout(IChannelHandlerContext ctx)
        {
            if (_closeSent.IsCompleted || _forceCloseTimeoutMillis < 0L)
            {
                return;
            }

            var timeoutTask = ctx.Executor.Schedule(new CloseTask(_closeSent), TimeSpan.FromMilliseconds(_forceCloseTimeoutMillis));
            _ = _closeSent.Task.ContinueWith(AbortCloseSetAction, timeoutTask, TaskContinuationOptions.ExecuteSynchronously);
        }

        private static void AbortCloseSet(Task t, object s)
        {
            _ = ((IScheduledTask)s).Cancel();
        }

        sealed class CloseTask : IRunnable
        {
            private readonly IPromise _closeSent;

            public CloseTask(IPromise closeSet)
            {
                _closeSent = closeSet;
            }

            public void Run()
            {
                if (!_closeSent.IsCompleted)
                {
                    _ = _closeSent.TrySetException(ThrowHelper.GetWebSocketHandshakeException_SendCloseFrameTimedOut());
                }
            }
        }

        public override Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
        {
            return context.BindAsync(localAddress);
        }

        public override Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
        {
            return context.ConnectAsync(remoteAddress, localAddress);
        }

        public override void Disconnect(IChannelHandlerContext context, IPromise promise)
        {
            _ = context.DisconnectAsync(promise);
        }

        public override void Deregister(IChannelHandlerContext context, IPromise promise)
        {
            _ = context.DeregisterAsync(promise);
        }

        public override void Read(IChannelHandlerContext context)
        {
            _ = context.Read();
        }

        public override void Flush(IChannelHandlerContext context)
        {
            _ = context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            _ = ctx.FireExceptionCaught(cause);
            _ = ctx.CloseAsync();
        }
    }
}
