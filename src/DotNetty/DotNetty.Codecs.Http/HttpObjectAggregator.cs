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

namespace DotNetty.Codecs.Http
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// A <see cref="IChannelHandler"/> that aggregates an <see cref="IHttpMessage"/>
    /// and its following <see cref="IHttpContent"/>s into a single <see cref="IFullHttpRequest"/>
    /// or <see cref="IFullHttpResponse"/> (depending on if it used to handle requests or responses)
    /// with no following <see cref="IHttpContent"/>s.  It is useful when you don't want to take
    /// care of HTTP messages whose transfer encoding is 'chunked'.  Insert this
    /// handler after <see cref="HttpResponseDecoder"/> in the <see cref="IChannelPipeline"/> if being used to handle
    /// responses, or after <see cref="HttpRequestDecoder"/> and <see cref="HttpResponseEncoder"/> in the
    /// <see cref="IChannelPipeline"/> if being used to handle requests.
    /// <blockquote>
    ///  <para>
    ///  <see cref="IChannelPipeline"/> p = ...;
    ///  ...
    ///  p.addLast("decoder", <b>new <see cref="HttpRequestDecoder"/>()</b>);
    ///  p.addLast("encoder", <b>new <see cref="HttpResponseEncoder"/>()</b>);
    ///  p.addLast("aggregator", <b>new <see cref="HttpObjectAggregator"/>(1048576)</b>);
    ///  ...
    ///  p.addLast("handler", new HttpRequestHandler());
    ///  </para>
    /// </blockquote>
    /// <p>
    /// For convenience, consider putting a <see cref="HttpServerCodec"/> before the <see cref="HttpObjectAggregator"/>
    /// as it functions as both a <see cref="HttpRequestDecoder"/> and a <see cref="HttpResponseEncoder"/>.
    /// </p>
    /// Be aware that <see cref="HttpObjectAggregator"/> may end up sending a <see cref="IHttpResponse"/>:
    /// <table>
    ///   <tbody>
    ///     <tr>
    ///       <th>Response Status</th>
    ///       <th>Condition When Sent</th>
    ///     </tr>
    ///     <tr>
    ///       <td>100 Continue</td>
    ///       <td>A '100-continue' expectation is received and the 'content-length' doesn't exceed maxContentLength</td>
    ///     </tr>
    ///     <tr>
    ///       <td>417 Expectation Failed</td>
    ///       <td>A '100-continue' expectation is received and the 'content-length' exceeds maxContentLength</td>
    ///     </tr>
    ///     <tr>
    ///       <td>413 Request Entity Too Large</td>
    ///       <td>Either the 'content-length' or the bytes received so far exceed maxContentLength</td>
    ///     </tr>
    ///   </tbody>
    /// </table>
    /// </summary>
    public class HttpObjectAggregator : MessageAggregator<IHttpObject, IHttpMessage, IHttpContent, IFullHttpMessage>
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<HttpObjectAggregator>();
        static readonly IFullHttpResponse Continue = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.Continue, Unpooled.Empty);
        static readonly IFullHttpResponse ExpectationFailed = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.ExpectationFailed, Unpooled.Empty);
        static readonly IFullHttpResponse TooLargeClose = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.RequestEntityTooLarge, Unpooled.Empty);
        static readonly IFullHttpResponse TooLarge = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.RequestEntityTooLarge, Unpooled.Empty);

        static HttpObjectAggregator()
        {
            _ = ExpectationFailed.Headers.Set(HttpHeaderNames.ContentLength, HttpHeaderValues.Zero);
            _ = TooLarge.Headers.Set(HttpHeaderNames.ContentLength, HttpHeaderValues.Zero);

            _ = TooLargeClose.Headers.Set(HttpHeaderNames.ContentLength, HttpHeaderValues.Zero);
            _ = TooLargeClose.Headers.Set(HttpHeaderNames.Connection, HttpHeaderValues.Close);
        }

        private readonly bool _closeOnExpectationFailed;

        /// <summary>Creates a new instance.</summary>
        /// <param name="maxContentLength">the maximum length of the aggregated content in bytes.
        /// If the length of the aggregated content exceeds this value,
        /// <see cref="HandleOversizedMessage(IChannelHandlerContext, IHttpMessage)"/> will be called.</param>
        public HttpObjectAggregator(int maxContentLength)
            : this(maxContentLength, false)
        {
        }

        /// <summary>Creates a new instance.</summary>
        /// <param name="maxContentLength">the maximum length of the aggregated content in bytes.
        /// If the length of the aggregated content exceeds this value,
        /// <see cref="HandleOversizedMessage(IChannelHandlerContext, IHttpMessage)"/> will be called.</param>
        /// <param name="closeOnExpectationFailed">If a 100-continue response is detected but the content length is too large
        /// then <c>true</c> means close the connection. otherwise the connection will remain open and data will be
        /// consumed and discarded until the next request is received.</param>
        public HttpObjectAggregator(int maxContentLength, bool closeOnExpectationFailed)
            : base(maxContentLength)
        {
            _closeOnExpectationFailed = closeOnExpectationFailed;
        }

        /// <inheritdoc />
        protected override bool IsStartMessage(IHttpObject msg) => msg is IHttpMessage;

        /// <inheritdoc />
        protected override bool IsContentMessage(IHttpObject msg) => msg is IHttpContent;

        /// <inheritdoc />
        protected override bool IsLastContentMessage(IHttpContent msg) => msg is ILastHttpContent;

        /// <inheritdoc />
        protected override bool IsAggregated(IHttpObject msg) => msg is IFullHttpMessage;

        /// <inheritdoc />
        protected override bool IsContentLengthInvalid(IHttpMessage start, int maxContentLength)
        {
            try
            {
                return HttpUtil.GetContentLength(start, -1) > maxContentLength;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        static object ContinueResponse(IHttpMessage start, int maxContentLength, IChannelPipeline pipeline)
        {
            if (HttpUtil.IsUnsupportedExpectation(start))
            {
                // if the request contains an unsupported expectation, we return 417
                _ = pipeline.FireUserEventTriggered(HttpExpectationFailedEvent.Default);
                return ExpectationFailed.RetainedDuplicate();
            }
            else if (HttpUtil.Is100ContinueExpected(start))
            {
                // if the request contains 100-continue but the content-length is too large, we return 413
                if (HttpUtil.GetContentLength(start, -1L) <= maxContentLength)
                {
                    return Continue.RetainedDuplicate();
                }
                _ = pipeline.FireUserEventTriggered(HttpExpectationFailedEvent.Default);
                return TooLarge.RetainedDuplicate();
            }

            return null;
        }

        /// <inheritdoc />
        protected override object NewContinueResponse(IHttpMessage start, int maxContentLength, IChannelPipeline pipeline)
        {
            object response = ContinueResponse(start, maxContentLength, pipeline);
            // we're going to respond based on the request expectation so there's no
            // need to propagate the expectation further.
            if (response is object)
            {
                _ = start.Headers.Remove(HttpHeaderNames.Expect);
            }
            return response;
        }

        /// <inheritdoc />
        protected override bool CloseAfterContinueResponse(object msg) =>
            _closeOnExpectationFailed && IgnoreContentAfterContinueResponse(msg);

        /// <inheritdoc />
        protected override bool IgnoreContentAfterContinueResponse(object msg) =>
            msg is IHttpResponse response && response.Status.CodeClass.Equals(HttpStatusClass.ClientError);

        /// <inheritdoc />
        protected override IFullHttpMessage BeginAggregation(IHttpMessage start, IByteBuffer content)
        {
            Debug.Assert(!(start is IFullHttpMessage));

            HttpUtil.SetTransferEncodingChunked(start, false);

            switch (start)
            {
                case IHttpRequest request:
                    return new AggregatedFullHttpRequest(request, content, null);
                case IHttpResponse response:
                    return new AggregatedFullHttpResponse(response, content, null);
                default:
                    return ThrowHelper.FromCodecException_InvalidType(start);
            }
        }

        /// <inheritdoc />
        protected override void Aggregate(IFullHttpMessage aggregated, IHttpContent content)
        {
            if (content is ILastHttpContent httpContent)
            {
                // Merge trailing headers into the message.
                ((AggregatedFullHttpMessage)aggregated).TrailingHeaders = httpContent.TrailingHeaders;
            }
        }

        /// <inheritdoc />
        protected override void FinishAggregation(IFullHttpMessage aggregated)
        {
            // Set the 'Content-Length' header. If one isn't already set.
            // This is important as HEAD responses will use a 'Content-Length' header which
            // does not match the actual body, but the number of bytes that would be
            // transmitted if a GET would have been used.
            //
            // See rfc2616 14.13 Content-Length
            if (!HttpUtil.IsContentLengthSet(aggregated))
            {
                _ = aggregated.Headers.Set(
                    HttpHeaderNames.ContentLength,
                    new AsciiString(aggregated.Content.ReadableBytes.ToString(CultureInfo.InvariantCulture)));
            }
        }

        /// <inheritdoc />
        protected override void HandleOversizedMessage(IChannelHandlerContext ctx, IHttpMessage oversized)
        {
            if (oversized is IHttpRequest)
            {
                // send back a 413 and close the connection

                // If the client started to send data already, close because it's impossible to recover.
                // If keep-alive is off and 'Expect: 100-continue' is missing, no need to leave the connection open.
                if (oversized is IFullHttpMessage ||
                    !HttpUtil.Is100ContinueExpected(oversized) && !HttpUtil.IsKeepAlive(oversized))
                {
                    _ = ctx.WriteAndFlushAsync(TooLargeClose.RetainedDuplicate()).ContinueWith(CloseOnCompleteAction, ctx, TaskContinuationOptions.ExecuteSynchronously);
                }
                else
                {
                    _ = ctx.WriteAndFlushAsync(TooLarge.RetainedDuplicate()).ContinueWith(CloseOnFaultAction, ctx, TaskContinuationOptions.ExecuteSynchronously);
                }
            }
            else if (oversized is IHttpResponse)
            {
                _ = ctx.CloseAsync();
                ThrowHelper.ThrowTooLongFrameException_ResponseTooLarge(oversized);
            }
            else
            {
                ThrowHelper.ThrowInvalidOperationException_InvalidType(oversized);
            }
        }

        static readonly Action<Task, object> CloseOnCompleteAction = (t, s) => CloseOnComplete(t, s);
        static void CloseOnComplete(Task t, object s)
        {
#if DEBUG
            if (t.IsFaulted)
            {
                if (Logger.DebugEnabled) Logger.FailedToSendA413RequestEntityTooLarge(t);
            }
#endif
            _ = ((IChannelHandlerContext)s).CloseAsync();
        }

        static readonly Action<Task, object> CloseOnFaultAction = (t, s) => CloseOnFault(t, s);
        static void CloseOnFault(Task t, object s)
        {
            if (t.IsFaulted)
            {
#if DEBUG
                if (Logger.DebugEnabled) Logger.FailedToSendA413RequestEntityTooLarge(t);
#endif
                _ = ((IChannelHandlerContext)s).CloseAsync();
            }
        }

        abstract class AggregatedFullHttpMessage : IFullHttpMessage
        {
            protected readonly IHttpMessage Message;
            private readonly IByteBuffer _content;
            private HttpHeaders _trailingHeaders;

            protected AggregatedFullHttpMessage(IHttpMessage message, IByteBuffer content, HttpHeaders trailingHeaders)
            {
                Message = message;
                _content = content;
                _trailingHeaders = trailingHeaders;
            }

            public HttpHeaders TrailingHeaders
            {
                get
                {
                    HttpHeaders headers = _trailingHeaders;
                    return headers ?? EmptyHttpHeaders.Default;
                }
                internal set => _trailingHeaders = value;
            }

            public HttpVersion ProtocolVersion => Message.ProtocolVersion;

            public IHttpMessage SetProtocolVersion(HttpVersion version)
            {
                _ = Message.SetProtocolVersion(version);
                return this;
            }

            public HttpHeaders Headers => Message.Headers;

            public DecoderResult Result
            {
                get => Message.Result;
                set => Message.Result = value;
            }

            public IByteBuffer Content => _content;

            public int ReferenceCount => _content.ReferenceCount;

            public IReferenceCounted Retain()
            {
                _ = _content.Retain();
                return this;
            }

            public IReferenceCounted Retain(int increment)
            {
                _ = _content.Retain(increment);
                return this;
            }

            public IReferenceCounted Touch()
            {
                _ = _content.Touch();
                return this;
            }

            public IReferenceCounted Touch(object hint)
            {
                _ = _content.Touch(hint);
                return this;
            }

            public bool Release() => _content.Release();

            public bool Release(int decrement) => _content.Release(decrement);

            public abstract IByteBufferHolder Copy();

            public abstract IByteBufferHolder Duplicate();

            public abstract IByteBufferHolder RetainedDuplicate();

            public abstract IByteBufferHolder Replace(IByteBuffer content);
        }

        sealed class AggregatedFullHttpRequest : AggregatedFullHttpMessage, IFullHttpRequest
        {
            internal AggregatedFullHttpRequest(IHttpRequest message, IByteBuffer content, HttpHeaders trailingHeaders)
                : base(message, content, trailingHeaders)
            {
            }

            public override IByteBufferHolder Copy() => Replace(Content.Copy());

            public override IByteBufferHolder Duplicate() => Replace(Content.Duplicate());

            public override IByteBufferHolder RetainedDuplicate() => Replace(Content.RetainedDuplicate());

            public override IByteBufferHolder Replace(IByteBuffer content)
            {
                var dup = new DefaultFullHttpRequest(ProtocolVersion, Method, Uri, content,
                    Headers.Copy(), TrailingHeaders.Copy());
                dup.Result = Result;
                return dup;
            }

            public HttpMethod Method => ((IHttpRequest)Message).Method;

            public IHttpRequest SetMethod(HttpMethod method)
            {
                _ = ((IHttpRequest)Message).SetMethod(method);
                return this;
            }

            public string Uri => ((IHttpRequest)Message).Uri;

            public IHttpRequest SetUri(string uri)
            {
                _ = ((IHttpRequest)Message).SetUri(uri);
                return this;
            }

            public override string ToString() => StringBuilderManager.ReturnAndFree(HttpMessageUtil.AppendFullRequest(StringBuilderManager.Allocate(256), this));
        }

        sealed class AggregatedFullHttpResponse : AggregatedFullHttpMessage, IFullHttpResponse
        {
            public AggregatedFullHttpResponse(IHttpResponse message, IByteBuffer content, HttpHeaders trailingHeaders)
                : base(message, content, trailingHeaders)
            {
            }

            public override IByteBufferHolder Copy() => Replace(Content.Copy());

            public override IByteBufferHolder Duplicate() => Replace(Content.Duplicate());

            public override IByteBufferHolder RetainedDuplicate() => Replace(Content.RetainedDuplicate());

            public override IByteBufferHolder Replace(IByteBuffer content)
            {
                var dup = new DefaultFullHttpResponse(ProtocolVersion, Status, content,
                    Headers.Copy(), TrailingHeaders.Copy());
                dup.Result = Result;
                return dup;
            }

            public HttpResponseStatus Status => ((IHttpResponse)Message).Status;

            public IHttpResponse SetStatus(HttpResponseStatus status)
            {
                _ = ((IHttpResponse)Message).SetStatus(status);
                return this;
            }

            public override string ToString() => StringBuilderManager.ReturnAndFree(HttpMessageUtil.AppendFullResponse(StringBuilderManager.Allocate(256), this));
        }
    }
}
