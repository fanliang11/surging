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

namespace DotNetty.Codecs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// An abstract <see cref="T:DotNetty.Transport.Channels.IChannelHandler" /> that aggregates a series of message objects 
    /// into a single aggregated message.
    /// 'A series of messages' is composed of the following:
    /// a single start message which optionally contains the first part of the content, and
    /// 1 or more content messages. The content of the aggregated message will be the merged 
    /// content of the start message and its following content messages. If this aggregator 
    /// encounters a content message where <see cref="IsLastContentMessage(TContent)"/>
    /// return true for, the aggregator will finish the aggregation and produce the aggregated 
    /// message and expect another start message.
    /// </summary>
    /// <typeparam name="TMessage">The type that covers both start message and content message</typeparam>
    /// <typeparam name="TStart">The type of the start message</typeparam>
    /// <typeparam name="TContent">The type of the content message</typeparam>
    /// <typeparam name="TOutput">The type of the aggregated message</typeparam>
    public abstract class MessageAggregator<TMessage, TStart, TContent, TOutput> : MessageToMessageDecoder<TMessage>
        where TContent : IByteBufferHolder
        where TOutput : IByteBufferHolder
    {
        private const int DefaultMaxCompositebufferComponents = 1024;

        private int _maxCumulationBufferComponents = DefaultMaxCompositebufferComponents;

        protected TOutput _currentMessage;
        protected bool _handlingOversizedMessage;

        private IChannelHandlerContext _handlerContext;

        private bool _aggregating;

        /// <summary>Creates a new instance.</summary>
        /// <param name="maxContentLength">the maximum length of the aggregated content.
        /// If the length of the aggregated content exceeds this value,
        /// <see cref="HandleOversizedMessage(IChannelHandlerContext, TStart)"/> will be called.</param>
        protected MessageAggregator(int maxContentLength)
        {
            ValidateMaxContentLength(maxContentLength);
            MaxContentLength = maxContentLength;
        }

        static void ValidateMaxContentLength(int maxContentLength)
        {
            if ((uint)maxContentLength > SharedConstants.TooBigOrNegative) // < 0
            {
                CThrowHelper.ThrowArgumentException_MaxContentLength(maxContentLength);
            }
        }

        /// <inheritdoc />
        public override bool AcceptInboundMessage(object msg)
        {
            // No need to match last and full types because they are subset of first and middle types.
            if (!(msg is TMessage message)) // !base.AcceptInboundMessage(msg)
            {
                return false;
            }

            if (IsAggregated(message))
            {
                return false;
            }

            // NOTE: It's tempting to make this check only if aggregating is false. There are however
            // side conditions in decode(...) in respect to large messages.
            if (IsStartMessage(message))
            {
                _aggregating = true;
                return true;
            }
            else if (_aggregating && IsContentMessage(message))
            {
                return true;
            }

            return false;
        }

        /// <summary>Returns <c>true</c> if and only if the specified message is a start message.</summary>
        /// <param name="msg"></param>
        protected abstract bool IsStartMessage(TMessage msg);

        /// <summary>Returns <c>true</c> if and only if the specified message is a content message.</summary>
        /// <param name="msg"></param>
        protected abstract bool IsContentMessage(TMessage msg);

        /// <summary>Returns <c>true</c> if and only if the specified message is the last content message.</summary>
        /// <param name="msg"></param>
        protected abstract bool IsLastContentMessage(TContent msg);

        /// <summary>
        /// Returns <c>true</c> if and only if the specified message is already aggregated.  If this method returns
        /// <c>true</c>, this handler will simply forward the message to the next handler as-is.
        /// </summary>
        /// <param name="msg"></param>
        protected abstract bool IsAggregated(TMessage msg);

        /// <summary>
        /// Returns the maximum allowed length of the aggregated message in bytes.
        /// </summary>
        public int MaxContentLength { get; }

        /// <summary>
        /// Gets or sets the maximum number of components in the cumulation buffer.  If the number of
        /// the components in the cumulation buffer exceeds this value, the components of the
        /// cumulation buffer are consolidated into a single component, involving memory copies.
        /// The default value of this property is <see cref="DefaultMaxCompositebufferComponents"/>.
        /// and its minimum allowed value is <code>2</code>.
        /// </summary>
        public int MaxCumulationBufferComponents
        {
            get => _maxCumulationBufferComponents;
            set
            {
                if (value < 2)
                {
                    CThrowHelper.ThrowArgumentException_MaxCumulationBufferComponents(value);
                }
                if (_handlerContext is object)
                {
                    CThrowHelper.ThrowInvalidOperationException_DecoderProperties();
                }

                _maxCumulationBufferComponents = value;
            }
        }

        public bool IsHandlingOversizedMessage => _handlingOversizedMessage;

        protected IChannelHandlerContext HandlerContext()
        {
            if (_handlerContext is null)
            {
                CThrowHelper.ThrowInvalidOperationException_NotAddedToAPipelineYet();
            }

            return _handlerContext;
        }

        /// <inheritdoc />
        protected internal override void Decode(IChannelHandlerContext context, TMessage message, List<object> output)
        {
            Debug.Assert(_aggregating);

            if (IsStartMessage(message))
            {
                _handlingOversizedMessage = false;
                if (_currentMessage is object)
                {
                    _ = _currentMessage.Release();
                    _currentMessage = default;

                    CThrowHelper.ThrowMessageAggregationException_StartMessage();
                }

                var m = As<TStart>(message);

                // Send the continue response if necessary(e.g. 'Expect: 100-continue' header)
                // Check before content length. Failing an expectation may result in a different response being sent.
                object continueResponse = NewContinueResponse(m, MaxContentLength, context.Pipeline);
                if (continueResponse is object)
                {
                    // Make sure to call this before writing, otherwise reference counts may be invalid.
                    bool closeAfterWrite = CloseAfterContinueResponse(continueResponse);
                    _handlingOversizedMessage = IgnoreContentAfterContinueResponse(continueResponse);

                    Task task = context
                        .WriteAndFlushAsync(continueResponse)
                        .FireExceptionOnFailure(context);

                    if (closeAfterWrite)
                    {
                        _ = task.CloseOnComplete(context.Channel);
                        return;
                    }

                    if (_handlingOversizedMessage)
                    {
                        return;
                    }
                }
                else if (IsContentLengthInvalid(m, MaxContentLength))
                {
                    // if content length is set, preemptively close if it's too large
                    InvokeHandleOversizedMessage(context, m);
                    return;
                }

                if (m is IDecoderResultProvider provider && !provider.Result.IsSuccess)
                {
                    TOutput aggregated;
                    if (m is IByteBufferHolder holder)
                    {
                        aggregated = BeginAggregation(m, (IByteBuffer)holder.Content.Retain());
                    }
                    else
                    {
                        aggregated = BeginAggregation(m, Unpooled.Empty);
                    }
                    FinishAggregation0(aggregated);
                    output.Add(aggregated);
                    return;
                }

                // A streamed message - initialize the cumulative buffer, and wait for incoming chunks.
                CompositeByteBuffer content = context.Allocator.CompositeBuffer(_maxCumulationBufferComponents);
                if (m is IByteBufferHolder bufferHolder)
                {
                    AppendPartialContent(content, bufferHolder.Content);
                }
                _currentMessage = BeginAggregation(m, content);
            }
            else if (IsContentMessage(message))
            {
                if (_currentMessage is null)
                {
                    // it is possible that a TooLongFrameException was already thrown but we can still discard data
                    // until the begging of the next request/response.
                    return;
                }

                // Merge the received chunk into the content of the current message.
                var content = (CompositeByteBuffer)_currentMessage.Content;

                var m = As<TContent>(message);

                // Handle oversized message.
                if (content.ReadableBytes > MaxContentLength - m.Content.ReadableBytes)
                {
                    // By convention, full message type extends first message type.
                    var s = As<TStart>(_currentMessage);

                    InvokeHandleOversizedMessage(context, s);
                    return;
                }

                // Append the content of the chunk.
                AppendPartialContent(content, m.Content);

                // Give the subtypes a chance to merge additional information such as trailing headers.
                Aggregate(_currentMessage, m);

                bool last;
                if (m is IDecoderResultProvider provider)
                {
                    DecoderResult decoderResult = provider.Result;
                    if (!decoderResult.IsSuccess)
                    {
                        if (_currentMessage is IDecoderResultProvider resultProvider)
                        {
                            resultProvider.Result = DecoderResult.Failure(decoderResult.Cause);
                        }

                        last = true;
                    }
                    else
                    {
                        last = IsLastContentMessage(m);
                    }
                }
                else
                {
                    last = IsLastContentMessage(m);
                }

                if (last)
                {
                    FinishAggregation0(_currentMessage);

                    // All done
                    output.Add(_currentMessage);
                    _currentMessage = default;
                }
            }
            else
            {
                CThrowHelper.ThrowMessageAggregationException_UnknownAggregationState();
            }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private static T As<T>(object obj) => (T)obj;

        protected static void AppendPartialContent(CompositeByteBuffer content, IByteBuffer partialContent)
        {
            if (partialContent.IsReadable())
            {
                _ = content.AddComponent(true, (IByteBuffer)partialContent.Retain());
            }
        }

        /// <summary>
        /// Determine if the message <paramref name="start"/>'s content length is known, and if it greater than
        /// <paramref name="maxContentLength"/>.
        /// </summary>
        /// <param name="start">The message which may indicate the content length.</param>
        /// <param name="maxContentLength">The maximum allowed content length.</param>
        /// <returns><c>true</c> if the message <paramref name="start"/>'s content length is known, and if it greater than
        /// <paramref name="maxContentLength"/>. <c>false</c> otherwise.</returns>
        protected abstract bool IsContentLengthInvalid(TStart start, int maxContentLength);

        /// <summary>
        /// Returns the 'continue response' for the specified start message if necessary. For example, this method is
        /// useful to handle an HTTP 100-continue header.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="maxContentLength"></param>
        /// <param name="pipeline"></param>
        /// <returns>the 'continue response', or <code>null</code> if there's no message to send</returns>
        protected abstract object NewContinueResponse(TStart start, int maxContentLength, IChannelPipeline pipeline);

        /// <summary>
        /// Determine if the channel should be closed after the result of
        /// <see cref="NewContinueResponse(TStart, int, IChannelPipeline)"/> is written.
        /// </summary>
        /// <param name="msg">The return value from <see cref="NewContinueResponse(TStart, int, IChannelPipeline)"/>.</param>
        /// <returns><c>true</c> if the channel should be closed after the result of
        /// <see cref="NewContinueResponse(TStart, int, IChannelPipeline)"/> is written. <c>false</c> otherwise.</returns>
        protected abstract bool CloseAfterContinueResponse(object msg);

        /// <summary>
        /// Determine if all objects for the current request/response should be ignored or not.
        /// Messages will stop being ignored the next time <see cref="IsContentMessage(TMessage)"/> returns <c>true</c>.
        /// </summary>
        /// <param name="msg">The return value from <see cref="NewContinueResponse(TStart, int, IChannelPipeline)"/>.</param>
        /// <returns><c>true</c> if all objects for the current request/response should be ignored or not.
        /// <c>false</c> otherwise.</returns>
        protected abstract bool IgnoreContentAfterContinueResponse(object msg);

        /// <summary>
        /// Creates a new aggregated message from the specified start message and the specified content.  If the start
        /// message implements <see cref="IByteBufferHolder"/>, its content is appended to the specified <typeparamref name="TContent"/>.
        /// This aggregator will continue to append the received content to the specified <typeparamref name="TContent"/>.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        protected abstract TOutput BeginAggregation(TStart start, IByteBuffer content);

        /// <summary>
        /// Transfers the information provided by the specified content message to the specified aggregated message.
        /// Note that the content of the specified content message has been appended to the content of the specified
        /// aggregated message already, so that you don't need to.  Use this method to transfer the additional information
        /// that the content message provides to <paramref name="aggregated"/>.
        /// </summary>
        /// <param name="aggregated"></param>
        /// <param name="content"></param>
        protected virtual void Aggregate(TOutput aggregated, TContent content)
        {
        }

        protected void FinishAggregation0(TOutput aggregated)
        {
            _aggregating = false;
            FinishAggregation(aggregated);
        }

        /// <summary>
        /// Invoked when the specified <paramref name="aggregated"/> message is about to be passed to the next handler in the pipeline.
        /// </summary>
        /// <param name="aggregated"></param>
        protected virtual void FinishAggregation(TOutput aggregated)
        {
        }

        protected void InvokeHandleOversizedMessage(IChannelHandlerContext ctx, TStart oversized)
        {
            _handlingOversizedMessage = true;
            _currentMessage = default;
            try
            {
                HandleOversizedMessage(ctx, oversized);
            }
            finally
            {
                // Release the message in case it is a full one.
                _ = ReferenceCountUtil.Release(oversized);
            }
        }

        /// <summary>
        /// Invoked when an incoming request exceeds the maximum content length.  The default behvaior is to trigger an
        /// <see cref="IChannelHandler.ExceptionCaught(IChannelHandlerContext, Exception)"/> event with a <see cref="TooLongFrameException"/>.
        /// </summary>
        /// <param name="ctx">the <see cref="IChannelHandlerContext"/>.</param>
        /// <param name="oversized">the accumulated message up to this point, whose type is <typeparamref name="TStart"/>
        /// or <typeparamref name="TOutput"/>.</param>
        protected virtual void HandleOversizedMessage(IChannelHandlerContext ctx, TStart oversized) =>
            ctx.FireExceptionCaught(CThrowHelper.GetTooLongFrameException(MaxContentLength));

        /// <inheritdoc />
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            // We might need keep reading the channel until the full message is aggregated.
            //
            // See https://github.com/netty/netty/issues/6583
            if (_currentMessage is object && !_handlerContext.Channel.Configuration.IsAutoRead)
            {
                _ = context.Read();
            }

            _ = context.FireChannelReadComplete();
        }

        /// <inheritdoc />
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            try
            {
                // release current message if it is not null as it may be a left-over
                base.ChannelInactive(context);
            }
            finally
            {
                ReleaseCurrentMessage();
            }
        }

        /// <inheritdoc />
        public override void HandlerAdded(IChannelHandlerContext context) => _handlerContext = context;

        /// <inheritdoc />
        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            try
            {
                base.HandlerRemoved(context);
            }
            finally
            {
                // release current message if it is not null as it may be a left-over as there is not much more we can do in
                // this case
                ReleaseCurrentMessage();
            }
        }

        void ReleaseCurrentMessage()
        {
            if (_currentMessage is null)
            {
                return;
            }

            _ = _currentMessage.Release();
            _currentMessage = default;
            _handlingOversizedMessage = false;
            _aggregating = false;
        }
    }
}
