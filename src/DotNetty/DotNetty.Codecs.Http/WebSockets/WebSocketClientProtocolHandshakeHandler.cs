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
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    sealed class WebSocketClientProtocolHandshakeHandler : ChannelHandlerAdapter
    {
        private const long DefaultHandshakeTimeoutMs = 10000L;

        private readonly WebSocketClientHandshaker _handshaker;
        private readonly long _handshakeTimeoutMillis;
        private IChannelHandlerContext _ctx;
        private IPromise _handshakePromise;

        internal WebSocketClientProtocolHandshakeHandler(WebSocketClientHandshaker handshaker)
            : this(handshaker, DefaultHandshakeTimeoutMs)
        {
        }

        internal WebSocketClientProtocolHandshakeHandler(WebSocketClientHandshaker handshaker, long handshakeTimeoutMillis)
        {
            if (handshakeTimeoutMillis <= 0L) { ThrowHelper.ThrowArgumentException_Positive(handshakeTimeoutMillis, ExceptionArgument.handshakeTimeoutMillis); }

            _handshaker = handshaker;
            _handshakeTimeoutMillis = handshakeTimeoutMillis;
        }

        /// <inheritdoc/>
        public override void HandlerAdded(IChannelHandlerContext context)
        {
            _ctx = context;
            _handshakePromise = context.NewPromise();
        }

        /// <inheritdoc/>
        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);

            _ = _handshaker.HandshakeAsync(context.Channel)
                .ContinueWith(FireUserEventTriggeredAction, (context, _handshakePromise), TaskContinuationOptions.ExecuteSynchronously);

            ApplyHandshakeTimeout();
        }

        static readonly Action<Task, object> FireUserEventTriggeredAction = (t, s) => OnFireUserEventTriggered(t, s);
        static void OnFireUserEventTriggered(Task t, object state)
        {
            var wrapped = ((IChannelHandlerContext, IPromise))state;
            if (t.IsSuccess())
            {
                _ = wrapped.Item2.TrySetException(t.Exception);
                _ = wrapped.Item1.FireUserEventTriggered(WebSocketClientProtocolHandler.ClientHandshakeStateEvent.HandshakeIssued);
            }
            else
            {
                _ = wrapped.Item1.FireExceptionCaught(t.Exception);
            }
        }

        /// <inheritdoc/>
        public override void ChannelRead(IChannelHandlerContext ctx, object msg)
        {
            if (!(msg is IFullHttpResponse response))
            {
                _ = ctx.FireChannelRead(msg);
                return;
            }

            try
            {
                if (!_handshaker.IsHandshakeComplete)
                {
                    _handshaker.FinishHandshake(ctx.Channel, response);
                    _ = _handshakePromise.TryComplete();
                    _ = ctx.FireUserEventTriggered(WebSocketClientProtocolHandler.ClientHandshakeStateEvent.HandshakeComplete);
                    _ = ctx.Pipeline.Remove(this);
                    return;
                }

                ThrowHelper.ThrowInvalidOperationException_WebSocketClientHandshaker();
            }
            finally
            {
                _ = response.Release();
            }
        }

        private void ApplyHandshakeTimeout()
        {
            var localHandshakePromise = _handshakePromise;
            if (_handshakeTimeoutMillis <= 0 || localHandshakePromise.IsCompleted) { return; }

            var timeoutTask = _ctx.Executor.Schedule(FireHandshakeTimeoutAction, _ctx, localHandshakePromise, TimeSpan.FromMilliseconds(_handshakeTimeoutMillis));

            // Cancel the handshake timeout when handshake is finished.
            _ = localHandshakePromise.Task.ContinueWith(AbortHandshakeTimeoutAfterHandshakeCompletedAction, timeoutTask, TaskContinuationOptions.ExecuteSynchronously);
        }

        private static readonly Action<object, object> FireHandshakeTimeoutAction = (c, p) => FireHandshakeTimeout(c, p);
        private static void FireHandshakeTimeout(object c, object p)
        {
            var handshakePromise = (IPromise)p;
            if (handshakePromise.IsCompleted) { return; }
            if (handshakePromise.TrySetException(new WebSocketHandshakeException("handshake timed out")))
            {
                _ = ((IChannelHandlerContext)c)
                    .Flush()
                    .FireUserEventTriggered(WebSocketClientProtocolHandler.ClientHandshakeStateEvent.HandshakeTimeout)
                    .CloseAsync();
            }
        }

        private static readonly Action<Task, object> AbortHandshakeTimeoutAfterHandshakeCompletedAction = (t, s) => AbortHandshakeTimeoutAfterHandshakeCompleted(t, s);
        private static void AbortHandshakeTimeoutAfterHandshakeCompleted(Task t, object s)
        {
            _ = ((IScheduledTask)s).Cancel();
        }

        /// <summary>
        /// This method is visible for testing.
        /// </summary>
        /// <returns>current handshake future</returns>
        internal Task GetHandshakeFuture() => _handshakePromise.Task;
    }
}
