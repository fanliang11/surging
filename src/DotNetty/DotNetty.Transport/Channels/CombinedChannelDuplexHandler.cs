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
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal.Logging;

    public partial class CombinedChannelDuplexHandler<TIn, TOut> : ChannelDuplexHandler
        where TIn : IChannelHandler
        where TOut : IChannelHandler
    {
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<CombinedChannelDuplexHandler<TIn, TOut>>();

        private DelegatingChannelHandlerContext _inboundCtx;
        private DelegatingChannelHandlerContext _outboundCtx;
        private int _handlerAdded;

        protected internal CombinedChannelDuplexHandler()
        {
            EnsureNotSharable();
        }

        public CombinedChannelDuplexHandler(TIn inboundHandler, TOut outboundHandler)
        {
            if (inboundHandler is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.inboundHandler); }
            if (outboundHandler is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.outboundHandler); }

            EnsureNotSharable();
            Init(inboundHandler, outboundHandler);
        }

        protected void Init(TIn inbound, TOut outbound)
        {
            Validate(inbound, outbound);

            InboundHandler = inbound;
            OutboundHandler = outbound;
        }

        protected TIn InboundHandler { get; private set; }

        protected TOut OutboundHandler { get; private set; }

        void Validate(TIn inbound, TOut outbound)
        {
            if (InboundHandler is object)
            {
                ThrowHelper.ThrowInvalidOperationException_InitCannotBeInvokedIf(this);
            }

            if (inbound is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.inbound);
            }

            if (outbound is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.outbound);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        void CheckAdded()
        {
            if (SharedConstants.False >= (uint)Volatile.Read(ref _handlerAdded))
            {
                ThrowHelper.ThrowInvalidOperationException_HandlerNotAddedToPipeYet();
            }
        }

        public void RemoveInboundHandler()
        {
            CheckAdded();
            _inboundCtx.Remove();
        }

        public void RemoveOutboundHandler()
        {
            CheckAdded();
            _outboundCtx.Remove();
        }

        public override void HandlerAdded(IChannelHandlerContext context)
        {
            if (InboundHandler is null)
            {
                ThrowHelper.ThrowInvalidOperationException_InitMustBeInvokedBefore(this);
            }

            _outboundCtx = new DelegatingChannelHandlerContext(context, OutboundHandler);
            _inboundCtx = new DelegatingChannelHandlerContext(context, InboundHandler, OnExceptionCaught);

            // The inboundCtx and outboundCtx were created and set now it's safe to call removeInboundHandler() and
            // removeOutboundHandler().
            _ = Interlocked.Exchange(ref _handlerAdded, SharedConstants.True);

            try
            {
                InboundHandler.HandlerAdded(_inboundCtx);
            }
            finally
            {
                OutboundHandler.HandlerAdded(_outboundCtx);
            }
        }

        private void OnExceptionCaught(Exception cause)
        {
            try
            {
                OutboundHandler.ExceptionCaught(_outboundCtx, cause);
            }
            catch (Exception error)
            {
#if DEBUG
                if (Logger.DebugEnabled)
                {
                    Logger.FreedThreadLocalBufferFromThreadFull(error, cause);
                }
                else
#endif
                if (Logger.WarnEnabled)
                {
                    Logger.FreedThreadLocalBufferFromThread(error, cause);
                }
            }
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            try
            {
                _inboundCtx.Remove();
            }
            finally
            {
                _outboundCtx.Remove();
            }
        }

        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            Debug.Assert(context == _inboundCtx.InnerContext);

            if (!_inboundCtx.IsRemoved)
            {
                InboundHandler.ChannelRegistered(_inboundCtx);
            }
            else
            {
                _ = _inboundCtx.FireChannelRegistered();
            }
        }

        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            Debug.Assert(context == _inboundCtx.InnerContext);

            if (!_inboundCtx.IsRemoved)
            {
                InboundHandler.ChannelUnregistered(_inboundCtx);
            }
            else
            {
                _ = _inboundCtx.FireChannelUnregistered();
            }
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            Debug.Assert(context == _inboundCtx.InnerContext);

            if (!_inboundCtx.IsRemoved)
            {
                InboundHandler.ChannelActive(_inboundCtx);
            }
            else
            {
                _ = _inboundCtx.FireChannelActive();
            }
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Debug.Assert(context == _inboundCtx.InnerContext);

            if (!_inboundCtx.IsRemoved)
            {
                InboundHandler.ChannelInactive(_inboundCtx);
            }
            else
            {
                _ = _inboundCtx.FireChannelInactive();
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Debug.Assert(context == _inboundCtx.InnerContext);

            if (!_inboundCtx.IsRemoved)
            {
                InboundHandler.ExceptionCaught(_inboundCtx, exception);
            }
            else
            {
                _ = _inboundCtx.FireExceptionCaught(exception);
            }
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            Debug.Assert(context == _inboundCtx.InnerContext);

            if (!_inboundCtx.IsRemoved)
            {
                InboundHandler.UserEventTriggered(_inboundCtx, evt);
            }
            else
            {
                _ = _inboundCtx.FireUserEventTriggered(evt);
            }
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            Debug.Assert(context == _inboundCtx.InnerContext);

            if (!_inboundCtx.IsRemoved)
            {
                InboundHandler.ChannelRead(_inboundCtx, message);
            }
            else
            {
                _ = _inboundCtx.FireChannelRead(message);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            Debug.Assert(context == _inboundCtx.InnerContext);

            if (!_inboundCtx.IsRemoved)
            {
                InboundHandler.ChannelReadComplete(_inboundCtx);
            }
            else
            {
                _ = _inboundCtx.FireChannelReadComplete();
            }
        }

        public override void ChannelWritabilityChanged(IChannelHandlerContext context)
        {
            Debug.Assert(context == _inboundCtx.InnerContext);

            if (!_inboundCtx.IsRemoved)
            {
                InboundHandler.ChannelWritabilityChanged(_inboundCtx);
            }
            else
            {
                _ = _inboundCtx.FireChannelWritabilityChanged();
            }
        }

        public override Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
        {
            Debug.Assert(context == _outboundCtx.InnerContext);

            if (!_outboundCtx.IsRemoved)
            {
                return OutboundHandler.BindAsync(_outboundCtx, localAddress);
            }
            else
            {
                return _outboundCtx.BindAsync(localAddress);
            }
        }

        public override Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
        {
            Debug.Assert(context == _outboundCtx.InnerContext);

            if (!_outboundCtx.IsRemoved)
            {
                return OutboundHandler.ConnectAsync(_outboundCtx, remoteAddress, localAddress);
            }
            else
            {
                return _outboundCtx.ConnectAsync(localAddress);
            }
        }

        public override void Disconnect(IChannelHandlerContext context, IPromise promise)
        {
            Debug.Assert(context == _outboundCtx.InnerContext);

            if (!_outboundCtx.IsRemoved)
            {
                OutboundHandler.Disconnect(_outboundCtx, promise);
            }
            else
            {
                _ = _outboundCtx.DisconnectAsync(promise);
            }
        }

        public override void Close(IChannelHandlerContext context, IPromise promise)
        {
            Debug.Assert(context == _outboundCtx.InnerContext);

            if (!_outboundCtx.IsRemoved)
            {
                OutboundHandler.Close(_outboundCtx, promise);
            }
            else
            {
                _ = _outboundCtx.CloseAsync(promise);
            }
        }

        public override void Deregister(IChannelHandlerContext context, IPromise promise)
        {
            Debug.Assert(context == _outboundCtx.InnerContext);

            if (!_outboundCtx.IsRemoved)
            {
                OutboundHandler.Deregister(_outboundCtx, promise);
            }
            else
            {
                _ = _outboundCtx.DeregisterAsync(promise);
            }
        }

        public override void Read(IChannelHandlerContext context)
        {
            Debug.Assert(context == _outboundCtx.InnerContext);

            if (!_outboundCtx.IsRemoved)
            {
                OutboundHandler.Read(_outboundCtx);
            }
            else
            {
                _ = _outboundCtx.Read();
            }
        }

        public override void Write(IChannelHandlerContext context, object message, IPromise promise)
        {
            Debug.Assert(context == _outboundCtx.InnerContext);

            if (!_outboundCtx.IsRemoved)
            {
                OutboundHandler.Write(_outboundCtx, message, promise);
            }
            else
            {
                _ = _outboundCtx.WriteAsync(message, promise);
            }
        }

        public override void Flush(IChannelHandlerContext context)
        {
            Debug.Assert(context == _outboundCtx.InnerContext);

            if (!_outboundCtx.IsRemoved)
            {
                OutboundHandler.Flush(_outboundCtx);
            }
            else
            {
                _ = _outboundCtx.Flush();
            }
        }

    }
}
