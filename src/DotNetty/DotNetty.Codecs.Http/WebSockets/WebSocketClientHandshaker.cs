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
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// <summary>
    /// Base class for web socket client handshake implementations
    /// </summary>
    public abstract partial class WebSocketClientHandshaker
    {
        protected const int DefaultForceCloseTimeoutMillis = 10000;

        private static readonly ClosedChannelException DefaultClosedChannelException = new ClosedChannelException();

        private static readonly string HttpSchemePrefix = HttpScheme.Http + "://";
        private static readonly string HttpsSchemePrefix = HttpScheme.Https + "://";

        protected readonly HttpHeaders CustomHeaders;

        private readonly Uri _uri;
        private readonly WebSocketVersion _version;
        private readonly string _expectedSubprotocol;
        private readonly int _maxFramePayloadLength;
        private readonly bool _absoluteUpgradeUrl;

        // volatile
        private long _forceCloseTimeoutMillis = DefaultForceCloseTimeoutMillis;
        private int _forceCloseInit;
        private int _forceCloseComplete;

        private int _handshakeComplete;
        private string _actualSubprotocol;

        /// <summary>Base constructor</summary>
        /// <param name="uri">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        protected WebSocketClientHandshaker(Uri uri, WebSocketVersion version, string subprotocol,
            HttpHeaders customHeaders, int maxFramePayloadLength)
            : this(uri, version, subprotocol, customHeaders, maxFramePayloadLength, DefaultForceCloseTimeoutMillis)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="uri">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="forceCloseTimeoutMillis">Close the connection if it was not closed by the server after timeout specified</param>
        protected WebSocketClientHandshaker(Uri uri, WebSocketVersion version, string subprotocol,
            HttpHeaders customHeaders, int maxFramePayloadLength, long forceCloseTimeoutMillis)
            : this(uri, version, subprotocol, customHeaders, maxFramePayloadLength, forceCloseTimeoutMillis, false)
        {
        }

        /// <summary>Base constructor</summary>
        /// <param name="uri">URL for web socket communications. e.g "ws://myhost.com/mypath". Subsequent web socket frames will be
        /// sent to this URL.</param>
        /// <param name="version">Version of web socket specification to use to connect to the server</param>
        /// <param name="subprotocol">Sub protocol request sent to the server.</param>
        /// <param name="customHeaders">Map of custom headers to add to the client request</param>
        /// <param name="maxFramePayloadLength">Maximum length of a frame's payload</param>
        /// <param name="forceCloseTimeoutMillis">Close the connection if it was not closed by the server after timeout specified</param>
        /// <param name="absoluteUpgradeUrl">Use an absolute url for the Upgrade request, typically when connecting through an HTTP proxy over
        /// clear HTTP</param>
        protected WebSocketClientHandshaker(Uri uri, WebSocketVersion version, string subprotocol,
            HttpHeaders customHeaders, int maxFramePayloadLength, long forceCloseTimeoutMillis, bool absoluteUpgradeUrl)
        {
            _uri = uri;
            _version = version;
            _expectedSubprotocol = subprotocol;
            CustomHeaders = customHeaders;
            _maxFramePayloadLength = maxFramePayloadLength;
            _forceCloseTimeoutMillis = forceCloseTimeoutMillis;
            _absoluteUpgradeUrl = absoluteUpgradeUrl;
        }

        /// <summary>
        /// Returns the URI to the web socket. e.g. "ws://myhost.com/path"
        /// </summary>
        public Uri Uri => _uri;

        /// <summary>
        /// Version of the web socket specification that is being used
        /// </summary>
        public WebSocketVersion Version => _version;

        /// <summary>
        /// Returns the max length for any frame's payload
        /// </summary>
        public int MaxFramePayloadLength => _maxFramePayloadLength;

        /// <summary>
        /// Flag to indicate if the opening handshake is complete
        /// </summary>
        public bool IsHandshakeComplete => SharedConstants.False < (uint)Volatile.Read(ref _handshakeComplete);

        void SetHandshakeComplete() => Interlocked.Exchange(ref _handshakeComplete, SharedConstants.True);

        /// <summary>
        /// Returns the CSV of requested subprotocol(s) sent to the server as specified in the constructor
        /// </summary>
        public string ExpectedSubprotocol => _expectedSubprotocol;

        /// <summary>
        /// Returns the subprotocol response sent by the server. Only available after end of handshake.
        /// Null if no subprotocol was requested or confirmed by the server.
        /// </summary>
        public string ActualSubprotocol
        {
            get => Volatile.Read(ref _actualSubprotocol);
            private set => Interlocked.Exchange(ref _actualSubprotocol, value);
        }

        /// <summary>
        /// Gets or sets timeout to close the connection if it was not closed by the server.
        /// </summary>
        public long ForceCloseTimeoutMillis
        {
            get => Volatile.Read(ref _forceCloseTimeoutMillis);
            set => Interlocked.Exchange(ref _forceCloseTimeoutMillis, value);
        }

        /// <summary>
        /// Flag to indicate if the closing handshake was initiated because of timeout.
        /// For testing only.
        /// </summary>
        protected internal bool IsForceCloseComplete => SharedConstants.False < (uint)Volatile.Read(ref _forceCloseComplete);

        private int ForceCloseInit => Volatile.Read(ref _forceCloseInit);

        /// <summary>Begins the opening handshake</summary>
        /// <param name="channel">Channel</param>
        public Task HandshakeAsync(IChannel channel)
        {
            var pipeline = channel.Pipeline;
            var decoder = pipeline.Get<HttpResponseDecoder>();
            if (decoder is null)
            {
                var codec = pipeline.Get<HttpClientCodec>();
                if (codec is null)
                {
                    return ThrowHelper.FromInvalidOperationException_HttpResponseDecoder();
                }
            }

            IFullHttpRequest request = NewHandshakeRequest();

            var completion = channel.NewPromise();
            _ = channel.WriteAndFlushAsync(request).ContinueWith(HandshakeOnCompleteAction,
                (completion, pipeline, this),
                TaskContinuationOptions.ExecuteSynchronously);

            return completion.Task;
        }

        /// <summary>Returns a new <see cref="IFullHttpRequest"/> which will be used for the handshake.</summary>
        protected internal abstract IFullHttpRequest NewHandshakeRequest();

        static readonly Action<Task, object> HandshakeOnCompleteAction = (t, s) => HandshakeOnComplete(t, s);
        static void HandshakeOnComplete(Task t, object state)
        {
            var wrapped = ((IPromise, IChannelPipeline, WebSocketClientHandshaker))state;
            if (t.IsCanceled)
            {
                _ = wrapped.Item1.TrySetCanceled(); return;
            }
            else if (t.IsFaulted)
            {
                _ = wrapped.Item1.TrySetException(t.Exception.InnerExceptions); return;
            }
            else if (t.IsCompleted)
            {
                IChannelPipeline p = wrapped.Item2;
                IChannelHandlerContext ctx = p.Context<HttpRequestEncoder>() ?? p.Context<HttpClientCodec>();
                if (ctx is null)
                {
                    _ = wrapped.Item1.TrySetException(ThrowHelper.GetInvalidOperationException<HttpRequestEncoder>());
                    return;
                }

                _ = p.AddAfter(ctx.Name, "ws-encoder", wrapped.Item3.NewWebSocketEncoder());
                _ = wrapped.Item1.TryComplete();
                return;
            }
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        /// <summary>
        /// Validates and finishes the opening handshake initiated by <see cref="HandshakeAsync"/>.
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="response">HTTP response containing the closing handshake details</param>
        public void FinishHandshake(IChannel channel, IFullHttpResponse response)
        {
            Verify(response);

            // Verify the subprotocol that we received from the server.
            // This must be one of our expected subprotocols - or null/empty if we didn't want to speak a subprotocol
            string receivedProtocol = null;
            if (response.Headers.TryGet(HttpHeaderNames.SecWebsocketProtocol, out ICharSequence headerValue))
            {
                receivedProtocol = headerValue.ToString().Trim();
            }

            string expectedProtocol = _expectedSubprotocol ?? "";
            bool protocolValid = false;

            if (0u >= (uint)expectedProtocol.Length && receivedProtocol is null)
            {
                // No subprotocol required and none received
                protocolValid = true;
                ActualSubprotocol = _expectedSubprotocol; // null or "" - we echo what the user requested
            }
            else if ((uint)expectedProtocol.Length > 0u && !string.IsNullOrEmpty(receivedProtocol))
            {
                // We require a subprotocol and received one -> verify it
                foreach (string protocol in expectedProtocol.Split(','))
                {
                    if (string.Equals(protocol.Trim(), receivedProtocol
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                        ))
#else
                        , StringComparison.Ordinal))
#endif
                    {
                        protocolValid = true;
                        ActualSubprotocol = receivedProtocol;
                        break;
                    }
                }
            } // else mixed cases - which are all errors

            if (!protocolValid)
            {
                ThrowHelper.ThrowWebSocketHandshakeException_InvalidSubprotocol(receivedProtocol, _expectedSubprotocol);
            }

            SetHandshakeComplete();

            IChannelPipeline p = channel.Pipeline;
            // Remove decompressor from pipeline if its in use
            var decompressor = p.Get<HttpContentDecompressor>();
            if (decompressor is object)
            {
                p.Remove(decompressor);
            }

            // Remove aggregator if present before
            var aggregator = p.Get<HttpObjectAggregator>();
            if (aggregator is object)
            {
                p.Remove(aggregator);
            }

            IChannelHandlerContext ctx = p.Context<HttpResponseDecoder>();
            if (ctx is null)
            {
                ctx = p.Context<HttpClientCodec>();
                if (ctx is null)
                {
                    ThrowHelper.ThrowInvalidOperationException_HttpRequestEncoder();
                }

                var codec = (HttpClientCodec)ctx.Handler;
                // Remove the encoder part of the codec as the user may start writing frames after this method returns.
                codec.RemoveOutboundHandler();

                p.AddAfter(ctx.Name, "ws-decoder", NewWebSocketDecoder());

                // Delay the removal of the decoder so the user can setup the pipeline if needed to handle
                // WebSocketFrame messages.
                // See https://github.com/netty/netty/issues/4533
                channel.EventLoop.Execute(RemoveHandlerAction, p, codec);
            }
            else
            {
                if (p.Get<HttpRequestEncoder>() is object)
                {
                    // Remove the encoder part of the codec as the user may start writing frames after this method returns.
                    p.Remove<HttpRequestEncoder>();
                }

                IChannelHandlerContext context = ctx;
                p.AddAfter(context.Name, "ws-decoder", NewWebSocketDecoder());

                // Delay the removal of the decoder so the user can setup the pipeline if needed to handle
                // WebSocketFrame messages.
                // See https://github.com/netty/netty/issues/4533
                channel.EventLoop.Execute(RemoveHandlerAction, p, context.Handler);
            }
        }

        static readonly Action<object, object> RemoveHandlerAction = (p, h) => OnRemoveHandler(p, h);
        static void OnRemoveHandler(object p, object h) => ((IChannelPipeline)p).Remove((IChannelHandler)h);

        /// <summary>
        /// Process the opening handshake initiated by <see cref="HandshakeAsync(IChannel)"/>.
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="response">HTTP response containing the closing handshake details</param>
        /// <returns> the <see cref="Task"/> which is notified once the handshake completes.</returns>
        public Task ProcessHandshakeAsync(IChannel channel, IHttpResponse response)
        {
            var completionSource = channel.NewPromise();
            if (response is IFullHttpResponse res)
            {
                try
                {
                    FinishHandshake(channel, res);
                    _ = completionSource.TryComplete();
                }
                catch (Exception cause)
                {
                    _ = completionSource.TrySetException(cause);
                }
            }
            else
            {
                IChannelPipeline p = channel.Pipeline;
                IChannelHandlerContext ctx = p.Context<HttpResponseDecoder>();
                if (ctx is null)
                {
                    ctx = p.Context<HttpClientCodec>();
                    if (ctx is null)
                    {
                        _ = completionSource.TrySetException(ThrowHelper.GetInvalidOperationException<HttpResponseDecoder>());
                    }
                }
                else
                {
                    // Add aggregator and ensure we feed the HttpResponse so it is aggregated. A limit of 8192 should be more
                    // then enough for the websockets handshake payload.
                    //
                    // TODO: Make handshake work without HttpObjectAggregator at all.
                    const string AggregatorName = "httpAggregator";
                    _ = p.AddAfter(ctx.Name, AggregatorName, new HttpObjectAggregator(8192));
                    _ = p.AddAfter(AggregatorName, "handshaker", new Handshaker(this, channel, completionSource));
                    try
                    {
                        _ = ctx.FireChannelRead(ReferenceCountUtil.Retain(response));
                    }
                    catch (Exception cause)
                    {
                        _ = completionSource.TrySetException(cause);
                    }
                }
            }

            return completionSource.Task;
        }

        sealed class Handshaker : SimpleChannelInboundHandler<IFullHttpResponse>
        {
            readonly WebSocketClientHandshaker _clientHandshaker;
            readonly IChannel _channel;
            readonly IPromise _completion;

            public Handshaker(WebSocketClientHandshaker clientHandshaker, IChannel channel, IPromise completion)
            {
                _clientHandshaker = clientHandshaker;
                _channel = channel;
                _completion = completion;
            }

            protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpResponse msg)
            {
                // Remove and do the actual handshake
                _ = ctx.Pipeline.Remove(this);
                try
                {
                    _clientHandshaker.FinishHandshake(_channel, msg);
                    _ = _completion.TryComplete();
                }
                catch (Exception cause)
                {
                    _ = _completion.TrySetException(cause);
                }
            }

            public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
            {
                // Remove ourself and fail the handshake promise.
                _ = ctx.Pipeline.Remove(this);
                _ = _completion.TrySetException(cause);
            }

            public override void ChannelInactive(IChannelHandlerContext ctx)
            {
                // Fail promise if Channel was closed
                if (!_completion.IsCompleted)
                {
                    _ = _completion.TrySetException(DefaultClosedChannelException);
                }
                _ = ctx.FireChannelInactive();
            }
        }

        /// <summary>
        /// Verify the <see cref="IFullHttpResponse"/> and throws a <see cref="WebSocketHandshakeException"/> if something is wrong.
        /// </summary>
        /// <param name="response"></param>
        protected abstract void Verify(IFullHttpResponse response);

        /// <summary>
        /// Returns the decoder to use after handshake is complete.
        /// </summary>
        /// <returns></returns>
        protected internal abstract IWebSocketFrameDecoder NewWebSocketDecoder();

        /// <summary>
        /// Returns the encoder to use after the handshake is complete.
        /// </summary>
        /// <returns></returns>
        protected internal abstract IWebSocketFrameEncoder NewWebSocketEncoder();

        /// <summary>
        /// Performs the closing handshake
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="frame">Closing Frame that was received</param>
        /// <returns></returns>
        public Task CloseAsync(IChannel channel, CloseWebSocketFrame frame)
        {
            if (channel is null) { return ThrowHelper.FromArgumentNullException(ExceptionArgument.channel); }
            var completionSource = channel.NewPromise();
            _ = channel.WriteAndFlushAsync(frame, completionSource);
            ApplyForceCloseTimeout(channel, completionSource);
            return completionSource.Task;
        }

        private void ApplyForceCloseTimeout(IChannel channel, IPromise flushFuture)
        {
            if (ForceCloseTimeoutMillis <= 0 || !channel.IsActive || (uint)ForceCloseInit > 0u)
            {
                return;
            }
            _ = flushFuture.Task.ContinueWith(CloseOnCompleteAction, (channel, this), TaskContinuationOptions.ExecuteSynchronously);
        }

        static readonly Action<Task, object> CloseOnCompleteAction = (t, s) => CloseOnComplete(t, s);
        static void CloseOnComplete(Task t, object state)
        {
            var wrapped = ((IChannel, WebSocketClientHandshaker))state;
            var channel = wrapped.Item1;
            var self = wrapped.Item2;

            // If flush operation failed, there is no reason to expect
            // a server to receive CloseFrame. Thus this should be handled
            // by the application separately.
            // Also, close might be called twice from different threads.
            if (t.IsSuccess() && channel.IsActive &&
                0u >= (uint)Interlocked.CompareExchange(ref self._forceCloseInit, 1, 0))
            {
                var timeoutTask = channel.EventLoop.Schedule(CloseChannelAction, channel, self, TimeSpan.FromMilliseconds(self.ForceCloseTimeoutMillis));
                _ = channel.CloseCompletion.ContinueWith(AbortCloseChannelAfterChannelClosedAction, timeoutTask, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        private static readonly Action<object, object> CloseChannelAction = (c, p) => CloseChannel(c, p);
        private static void CloseChannel(object c, object p)
        {
            var channel = (IChannel)c;
            if (channel.IsActive)
            {
                _ = channel.CloseAsync();
                _ = Interlocked.Exchange(ref ((WebSocketClientHandshaker)p)._forceCloseComplete, SharedConstants.True);
            }
        }

        private static readonly Action<Task, object> AbortCloseChannelAfterChannelClosedAction = (t, s) => AbortCloseChannelAfterChannelClosed(t, s);
        private static void AbortCloseChannelAfterChannelClosed(Task t, object s)
        {
            _ = ((IScheduledTask)s).Cancel();
        }

        /// <summary>
        /// Return the constructed raw path for the give <paramref name="wsUrl"/>.
        /// </summary>
        protected string UpgradeUrl(Uri wsUrl)
        {
            if (_absoluteUpgradeUrl)
            {
                return wsUrl.OriginalString;
            }
            return wsUrl.IsAbsoluteUri ? wsUrl.PathAndQuery : "/";
        }

        internal static string WebsocketHostValue(Uri wsUrl)
        {
            string scheme;
            Uri uri;
            if (wsUrl.IsAbsoluteUri)
            {
                scheme = wsUrl.Scheme;
                uri = wsUrl;
            }
            else
            {
                scheme = null;
                uri = AbsoluteUri(wsUrl);
            }

            int port = OriginalPort(uri);
            if (port == -1)
            {
                return uri.Host;
            }
            string host = uri.Host;
            if (port == HttpScheme.Http.Port)
            {
                return HttpScheme.Http.Name.ContentEquals(scheme)
                    || WebSocketScheme.WS.Name.ContentEquals(scheme)
                        ? host : NetUtil.ToSocketAddressString(host, port);
            }
            if (port == HttpScheme.Https.Port)
            {
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                return string.Equals(HttpScheme.Https.Name.ToString(), scheme)
                    || string.Equals(WebSocketScheme.WSS.Name.ToString(), scheme)
#else
                return string.Equals(HttpScheme.Https.Name.ToString(), scheme, StringComparison.Ordinal)
                    || string.Equals(WebSocketScheme.WSS.Name.ToString(), scheme, StringComparison.Ordinal)
#endif
                        ? host : NetUtil.ToSocketAddressString(host, port);
            }

            // if the port is not standard (80/443) its needed to add the port to the header.
            // See http://tools.ietf.org/html/rfc6454#section-6.2
            return NetUtil.ToSocketAddressString(host, port);
        }

        internal static string WebsocketOriginValue(Uri wsUrl)
        {
            string scheme;
            Uri uri;
            if (wsUrl.IsAbsoluteUri)
            {
                scheme = wsUrl.Scheme;
                uri = wsUrl;
            }
            else
            {
                scheme = null;
                uri = AbsoluteUri(wsUrl);
            }

            string schemePrefix;
            int port = uri.Port;
            int defaultPort;

            if (WebSocketScheme.WSS.Name.ContentEquals(scheme)
                || HttpScheme.Https.Name.ContentEquals(scheme)
                || (scheme is null && port == WebSocketScheme.WSS.Port))
            {

                schemePrefix = HttpsSchemePrefix;
                defaultPort = WebSocketScheme.WSS.Port;
            }
            else
            {
                schemePrefix = HttpSchemePrefix;
                defaultPort = WebSocketScheme.WS.Port;
            }

            // Convert uri-host to lower case (by RFC 6454, chapter 4 "Origin of a URI")
            string host = uri.Host.ToLowerInvariant();

            if (port != defaultPort && port != -1)
            {
                // if the port is not standard (80/443) its needed to add the port to the header.
                // See http://tools.ietf.org/html/rfc6454#section-6.2
                return schemePrefix + NetUtil.ToSocketAddressString(host, port);
            }
            return schemePrefix + host;
        }

        static Uri AbsoluteUri(Uri uri)
        {
            if (uri.IsAbsoluteUri)
            {
                return uri;
            }

            string relativeUri = uri.OriginalString;
            return new Uri(relativeUri.StartsWith("//", StringComparison.Ordinal)
                ? HttpScheme.Http + ":" + relativeUri
                : HttpSchemePrefix + relativeUri);
        }

        static int OriginalPort(Uri uri)
        {
            int index = uri.Scheme.Length + 3 + uri.Host.Length;

            var originalString = uri.OriginalString;
            if ((uint)index < (uint)originalString.Length
                && originalString[index] == HttpConstants.ColonChar)
            {
                return uri.Port;
            }
            return -1;
        }
    }
}
