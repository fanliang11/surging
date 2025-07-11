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
    using System.Threading.Tasks;
    using DotNetty.Codecs.Http.Cors;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Base class for server side web socket opening and closing handshakes
    /// </summary>
    public abstract class WebSocketServerHandshaker
    {
        protected static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<WebSocketServerHandshaker>();
        private static readonly ClosedChannelException ClosedChannelException = new ClosedChannelException();

        private readonly string _uri;

        private readonly string[] _subprotocols;

        private readonly WebSocketVersion _version;

        private readonly WebSocketDecoderConfig _decoderConfig;

        private string _selectedSubprotocol;

        // Use this as wildcard to support all requested sub-protocols
        public const string SubProtocolWildcard = "*";

        /// <summary>
        /// Constructor specifying the destination web socket location
        /// </summary>
        /// <param name="version">the protocol version</param>
        /// <param name="uri">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="subprotocols">CSV of supported protocols. Null if sub protocols not supported.</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        protected WebSocketServerHandshaker(WebSocketVersion version, string uri, string subprotocols, int maxFramePayloadLength)
            : this(version, uri, subprotocols, WebSocketDecoderConfig.NewBuilder().MaxFramePayloadLength(maxFramePayloadLength).Build())
        {
        }

        /// <summary>
        /// Constructor specifying the destination web socket location
        /// </summary>
        /// <param name="version">the protocol version</param>
        /// <param name="uri">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="subprotocols">CSV of supported protocols. Null if sub protocols not supported.</param>
        /// <param name="decoderConfig">Frames decoder configuration.</param>
        protected WebSocketServerHandshaker(WebSocketVersion version, string uri, string subprotocols, WebSocketDecoderConfig decoderConfig)
        {
            if (decoderConfig is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.decoderConfig); }

            _version = version;
            _uri = uri;
            if (subprotocols is object)
            {
                string[] subprotocolArray = subprotocols.Split(',');
                for (int i = 0; i < subprotocolArray.Length; i++)
                {
                    subprotocolArray[i] = subprotocolArray[i].Trim();
                }
                _subprotocols = subprotocolArray;
            }
            else
            {
                _subprotocols = EmptyArrays.EmptyStrings;
            }
            _decoderConfig = decoderConfig;
        }

        /// <summary>
        /// Returns the URL of the web socket
        /// </summary>
        public string Uri => _uri;

        /// <summary>
        /// Returns the CSV of supported sub protocols
        /// </summary>
        /// <returns></returns>
        public ISet<string> Subprotocols()
        {
            var ret = new HashSet<string>(_subprotocols, StringComparer.Ordinal);
            return ret;
        }

        /// <summary>
        /// Returns the version of the specification being supported
        /// </summary>
        public WebSocketVersion Version => _version;

        /// <summary>
        /// Gets the maximum length for any frame's payload.
        /// </summary>
        public int MaxFramePayloadLength => _decoderConfig.MaxFramePayloadLength;

        /// <summary>
        /// Gets this decoder configuration.
        /// </summary>
        public WebSocketDecoderConfig DecoderConfig => _decoderConfig;

        /// <summary>
        /// Performs the opening handshake
        /// When call this method you <c>MUST NOT</c> retain the <see cref="IFullHttpRequest"/> which is passed in.
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="req">HTTP Request</param>
        /// <returns></returns>
        public Task HandshakeAsync(IChannel channel, IFullHttpRequest req) => HandshakeAsync(channel, req, null);

        /// <summary>
        /// Performs the opening handshake
        /// When call this method you <c>MUST NOT</c> retain the <see cref="IFullHttpRequest"/> which is passed in.
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="req">HTTP Request</param>
        /// <param name="responseHeaders">Extra headers to add to the handshake response or <code>null</code> if no extra headers should be added</param>
        /// <returns></returns>
        public Task HandshakeAsync(IChannel channel, IFullHttpRequest req, HttpHeaders responseHeaders)
        {
            var completion = channel.NewPromise();
            Handshake(channel, req, responseHeaders, completion);
            return completion.Task;
        }

        /// <summary>
        /// Performs the opening handshake
        /// When call this method you <c>MUST NOT</c> retain the <see cref="IFullHttpRequest"/> which is passed in.
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="req">HTTP Request</param>
        /// <param name="responseHeaders">Extra headers to add to the handshake response or <code>null</code> if no extra headers should be added</param>
        /// <param name="completion">the <see cref="IPromise"/> to be notified when the opening handshake is done</param>
        public void Handshake(IChannel channel, IFullHttpRequest req, HttpHeaders responseHeaders, IPromise completion)
        {
#if DEBUG
            if (Logger.DebugEnabled)
            {
                Logger.WebSocketVersionServerHandshake(channel, _version);
            }
#endif

            IFullHttpResponse response = NewHandshakeResponse(req, responseHeaders);
            IChannelPipeline p = channel.Pipeline;

            if (p.Get<HttpObjectAggregator>() is object) { _ = p.Remove<HttpObjectAggregator>(); }
            if (p.Get<HttpContentCompressor>() is object) { _ = p.Remove<HttpContentCompressor>(); }
            if (p.Get<CorsHandler>() is object) { _ = p.Remove<CorsHandler>(); }
            if (p.Get<HttpServerExpectContinueHandler>() is object) { _ = p.Remove<HttpServerExpectContinueHandler>(); }
            if (p.Get<HttpServerKeepAliveHandler>() is object) { _ = p.Remove<HttpServerKeepAliveHandler>(); }

            IChannelHandlerContext ctx = p.Context<HttpRequestDecoder>();
            string encoderName;
            if (ctx is null)
            {
                // this means the user use an HttpServerCodec
                ctx = p.Context<HttpServerCodec>();
                if (ctx is null)
                {
                    _ = completion.TrySetException(ThrowHelper.GetInvalidOperationException_NoHttpDecoderAndServerCodec());
                    return;
                }

                encoderName = ctx.Name;
                _ = p.AddBefore(encoderName, "wsencoder", NewWebSocketEncoder());
                _ = p.AddBefore(encoderName, "wsdecoder", NewWebsocketDecoder());
            }
            else
            {
                _ = p.Replace(ctx.Name, "wsdecoder", NewWebsocketDecoder());

                encoderName = p.Context<HttpResponseEncoder>().Name;
                _ = p.AddBefore(encoderName, "wsencoder", NewWebSocketEncoder());
            }

            _ = channel.WriteAndFlushAsync(response).ContinueWith(RemoveHandlerAfterWriteAction, (completion, p, encoderName), TaskContinuationOptions.ExecuteSynchronously);
        }

        static readonly Action<Task, object> RemoveHandlerAfterWriteAction = (t, s) => RemoveHandlerAfterWrite(t, s);
        static void RemoveHandlerAfterWrite(Task t, object state)
        {
            var wrapped = ((IPromise, IChannelPipeline, string))state;
            if (t.IsSuccess())
            {
                _ = wrapped.Item2.Remove(wrapped.Item3);
                _ = wrapped.Item1.TryComplete();
            }
            else
            {
                _ = wrapped.Item1.TrySetException(t.Exception.InnerExceptions);
            }
        }

        /// <summary>
        /// Performs the opening handshake
        /// When call this method you <c>MUST NOT</c> retain the <see cref="IHttpRequest"/> which is passed in.
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="req">HTTP Request</param>
        /// <param name="responseHeaders">Extra headers to add to the handshake response or <code>null</code> if no extra headers should be added</param>
        /// <returns></returns>
        public Task HandshakeAsync(IChannel channel, IHttpRequest req, HttpHeaders responseHeaders)
        {
            if (req is IFullHttpRequest request)
            {
                return HandshakeAsync(channel, request, responseHeaders);
            }
#if DEBUG
            if (Logger.DebugEnabled)
            {
                Logger.WebSocketVersionServerHandshake(channel, _version);
            }
#endif
            IChannelPipeline p = channel.Pipeline;
            IChannelHandlerContext ctx = p.Context<HttpRequestDecoder>();
            if (ctx is null)
            {
                // this means the user use an HttpServerCodec
                ctx = p.Context<HttpServerCodec>();
                if (ctx is null)
                {
                    return ThrowHelper.FromInvalidOperationException_NoHttpDecoderAndServerCodec();
                }
            }

            // Add aggregator and ensure we feed the HttpRequest so it is aggregated. A limit o 8192 should be more then
            // enough for the websockets handshake payload.
            //
            // TODO: Make handshake work without HttpObjectAggregator at all.
            string aggregatorName = "httpAggregator";
            _ = p.AddAfter(ctx.Name, aggregatorName, new HttpObjectAggregator(8192));
            var completion = channel.NewPromise();
            _ = p.AddAfter(aggregatorName, "handshaker", new Handshaker(this, channel, responseHeaders, completion));
            try
            {
                _ = ctx.FireChannelRead(ReferenceCountUtil.Retain(req));
            }
            catch (Exception cause)
            {
                _ = completion.TrySetException(cause);
            }
            return completion.Task;
        }

        sealed class Handshaker : SimpleChannelInboundHandler<IFullHttpRequest>
        {
            readonly WebSocketServerHandshaker _serverHandshaker;
            readonly IChannel _channel;
            readonly HttpHeaders _responseHeaders;
            readonly IPromise _completion;

            public Handshaker(WebSocketServerHandshaker serverHandshaker, IChannel channel, HttpHeaders responseHeaders, IPromise completion)
            {
                _serverHandshaker = serverHandshaker;
                _channel = channel;
                _responseHeaders = responseHeaders;
                _completion = completion;
            }

            protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest msg)
            {
                // Remove ourself and do the actual handshake
                _ = ctx.Pipeline.Remove(this);
                _serverHandshaker.Handshake(_channel, msg, _responseHeaders, _completion);
            }

            public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
            {
                // Remove ourself and fail the handshake promise.
                _ = ctx.Pipeline.Remove(this);
                _ = _completion.TrySetException(cause);
                _ = ctx.FireExceptionCaught(cause);
            }

            public override void ChannelInactive(IChannelHandlerContext ctx)
            {
                // Fail promise if Channel was closed
                if (!_completion.IsCompleted)
                {
                    _ = _completion.TrySetException(ClosedChannelException);
                }
                _ = ctx.FireChannelInactive();
            }
        }

        /// <summary>
        /// Returns a new <see cref="IFullHttpResponse"/> which will be used for as response to the handshake request.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="responseHeaders"></param>
        /// <returns></returns>
        protected internal abstract IFullHttpResponse NewHandshakeResponse(IFullHttpRequest req, HttpHeaders responseHeaders);

        /// <summary>
        /// Performs the closing handshake
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="frame">Closing Frame that was received</param>
        public virtual Task CloseAsync(IChannel channel, CloseWebSocketFrame frame)
        {
            if (channel is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.channel); }

            return channel.WriteAndFlushAsync(frame).CloseOnComplete(channel);
        }

        /// <summary>
        /// Selects the first matching supported sub protocol
        /// </summary>
        /// <param name="requestedSubprotocols">CSV of protocols to be supported. e.g. "chat, superchat"</param>
        /// <returns>First matching supported sub protocol. Null if not found.</returns>
        protected string SelectSubprotocol(string requestedSubprotocols)
        {
            if (requestedSubprotocols is null || 0u >= (uint)_subprotocols.Length)
            {
                return null;
            }

            string[] requestedSubprotocolArray = requestedSubprotocols.Split(',');
            foreach (string p in requestedSubprotocolArray)
            {
                string requestedSubprotocol = p.Trim();

                foreach (string supportedSubprotocol in _subprotocols)
                {
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                    if (string.Equals(SubProtocolWildcard, supportedSubprotocol)
                        || string.Equals(requestedSubprotocol, supportedSubprotocol))
#else
                    if (string.Equals(SubProtocolWildcard, supportedSubprotocol, StringComparison.Ordinal)
                        || string.Equals(requestedSubprotocol, supportedSubprotocol, StringComparison.Ordinal))
#endif
                    {
                        _selectedSubprotocol = requestedSubprotocol;
                        return requestedSubprotocol;
                    }
                }
            }

            // No match found
            return null;
        }

        /// <summary>
        /// Returns the selected subprotocol. Null if no subprotocol has been selected.
        /// <para>This is only available AFTER <tt>handshake()</tt> has been called.</para>
        /// </summary>
        public string SelectedSubprotocol => _selectedSubprotocol;

        /// <summary>
        /// Returns the decoder to use after handshake is complete.
        /// </summary>
        protected internal abstract IWebSocketFrameDecoder NewWebsocketDecoder();

        /// <summary>
        /// Returns the encoder to use after the handshake is complete.
        /// </summary>
        protected internal abstract IWebSocketFrameEncoder NewWebSocketEncoder();
    }
}
