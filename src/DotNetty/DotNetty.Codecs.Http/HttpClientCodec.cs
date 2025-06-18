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
    using System.Collections.Generic;
    using System.Threading;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
    using DotNetty.Codecs;

    public class HttpClientCodec : CombinedChannelDuplexHandler<HttpResponseDecoder, HttpRequestEncoder>,
        HttpClientUpgradeHandler.ISourceCodec
    {
        public const bool DefaultFailOnMissingResponse = false;
        public const bool DefaultParseHttpAfterConnectRequest = false;

        // A queue that is used for correlating a request and a response.
        private readonly Deque<HttpMethod> _queue = new Deque<HttpMethod>();
        private readonly bool _parseHttpAfterConnectRequest;

        // If true, decoding stops (i.e. pass-through)
        private bool _done;

        private long _requestResponseCounter;
        private readonly bool _failOnMissingResponse;

        public HttpClientCodec()
            : this(HttpObjectDecoder.DefaultMaxInitialLineLength, HttpObjectDecoder.DefaultMaxHeaderSize, HttpObjectDecoder.DefaultMaxChunkSize, DefaultFailOnMissingResponse)
        {
        }

        public HttpClientCodec(int maxInitialLineLength, int maxHeaderSize, int maxChunkSize)
            : this(maxInitialLineLength, maxHeaderSize, maxChunkSize, DefaultFailOnMissingResponse)
        {
        }

        public HttpClientCodec(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool failOnMissingResponse)
            : this(maxInitialLineLength, maxHeaderSize, maxChunkSize, failOnMissingResponse, HttpObjectDecoder.DefaultValidateHeaders)
        {
        }

        public HttpClientCodec(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool failOnMissingResponse,
            bool validateHeaders)
            : this(maxInitialLineLength, maxHeaderSize, maxChunkSize, failOnMissingResponse, validateHeaders, DefaultParseHttpAfterConnectRequest)
        {
        }

        public HttpClientCodec(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool failOnMissingResponse,
            bool validateHeaders, bool parseHttpAfterConnectRequest)
        {
            Init(new Decoder(this, maxInitialLineLength, maxHeaderSize, maxChunkSize, validateHeaders), new Encoder(this));
            _failOnMissingResponse = failOnMissingResponse;
            _parseHttpAfterConnectRequest = parseHttpAfterConnectRequest;
        }

        public HttpClientCodec(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool failOnMissingResponse,
            bool validateHeaders, int initialBufferSize)
            : this(maxInitialLineLength, maxHeaderSize, maxChunkSize, failOnMissingResponse, validateHeaders,
                  initialBufferSize, DefaultParseHttpAfterConnectRequest)
        {
        }

        public HttpClientCodec(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool failOnMissingResponse,
            bool validateHeaders, int initialBufferSize, bool parseHttpAfterConnectRequest)
        {
            Init(new Decoder(this, maxInitialLineLength, maxHeaderSize, maxChunkSize, validateHeaders, initialBufferSize,
                HttpObjectDecoder.DefaultAllowDuplicateContentLengths), new Encoder(this));
            _parseHttpAfterConnectRequest = parseHttpAfterConnectRequest;
            _failOnMissingResponse = failOnMissingResponse;
        }

        public HttpClientCodec(
            int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool failOnMissingResponse,
            bool validateHeaders, int initialBufferSize, bool parseHttpAfterConnectRequest,
            bool allowDuplicateContentLengths)
        {
            Init(new Decoder(this, maxInitialLineLength, maxHeaderSize, maxChunkSize, validateHeaders, initialBufferSize,
                allowDuplicateContentLengths), new Encoder(this));
            _parseHttpAfterConnectRequest = parseHttpAfterConnectRequest;
            _failOnMissingResponse = failOnMissingResponse;
        }

        public void PrepareUpgradeFrom(IChannelHandlerContext ctx) => ((Encoder)OutboundHandler).Upgraded = true;

        public void UpgradeFrom(IChannelHandlerContext ctx)
        {
            IChannelPipeline p = ctx.Pipeline;
            _ = p.Remove(this);
        }

        public bool SingleDecode
        {
            get => InboundHandler.SingleDecode;
            set => InboundHandler.SingleDecode = value;
        }

        sealed class Encoder : HttpRequestEncoder
        {
            private readonly HttpClientCodec _clientCodec;
            internal bool Upgraded;

            public Encoder(HttpClientCodec clientCodec)
            {
                _clientCodec = clientCodec;
            }

            protected override void Encode(IChannelHandlerContext context, object message, List<object> output)
            {
                if (Upgraded)
                {
                    output.Add(ReferenceCountUtil.Retain(message));
                    return;
                }

                if (message is IHttpRequest request)
                {
                    _clientCodec._queue.AddLast​(request.Method);
                }

                base.Encode(context, message, output);

                if (_clientCodec._failOnMissingResponse && !_clientCodec._done)
                {
                    // check if the request is chunked if so do not increment
                    if (message is ILastHttpContent)
                    {
                        // increment as its the last chunk
                        _ = Interlocked.Increment(ref _clientCodec._requestResponseCounter);
                    }
                }
            }
        }

        sealed class Decoder : HttpResponseDecoder
        {
            private readonly HttpClientCodec _clientCodec;

            internal Decoder(HttpClientCodec clientCodec, int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders)
                : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, validateHeaders)
            {
                _clientCodec = clientCodec;
            }

            internal Decoder(HttpClientCodec clientCodec, int maxInitialLineLength, int maxHeaderSize, int maxChunkSize, bool validateHeaders,
                int initialBufferSize, bool allowDuplicateContentLengths)
                : base(maxInitialLineLength, maxHeaderSize, maxChunkSize, validateHeaders, initialBufferSize, allowDuplicateContentLengths)
            {
                _clientCodec = clientCodec;
            }

            protected override void Decode(IChannelHandlerContext context, IByteBuffer buffer, List<object> output)
            {
                if (_clientCodec._done)
                {
                    int readable = ActualReadableBytes;
                    if (0u >= (uint)readable)
                    {
                        // if non is readable just return null
                        // https://github.com/netty/netty/issues/1159
                        return;
                    }
                    output.Add(buffer.ReadBytes(readable));
                }
                else
                {
                    int oldSize = output.Count;
                    base.Decode(context, buffer, output);
                    if (_clientCodec._failOnMissingResponse)
                    {
                        int size = output.Count;
                        for (int i = oldSize; i < size; i++)
                        {
                            Decrement(output[i]);
                        }
                    }
                }
            }

            void Decrement(object msg)
            {
                if (msg is null)
                {
                    return;
                }

                // check if it's an Header and its transfer encoding is not chunked.
                if (msg is ILastHttpContent)
                {
                    _ = Interlocked.Decrement(ref _clientCodec._requestResponseCounter);
                }
            }

            protected override bool IsContentAlwaysEmpty(IHttpMessage msg)
            {
                // Get the method of the HTTP request that corresponds to the
                // current response.
                //
                // Even if we do not use the method to compare we still need to poll it to ensure we keep
                // request / response pairs in sync.
                _ = _clientCodec._queue.TryRemoveFirst(out HttpMethod method);

                int statusCode = ((IHttpResponse)msg).Status.Code;
                if (statusCode >= 100 && statusCode < 200)
                {
                    // An informational response should be excluded from paired comparison.
                    // Just delegate to super method which has all the needed handling.
                    return base.IsContentAlwaysEmpty(msg);
                }

                // If the remote peer did for example send multiple responses for one request (which is not allowed per
                // spec but may still be possible) method will be null so guard against it.
                if (method is object)
                {
                    char firstChar = method.AsciiName[0];
                    switch (firstChar)
                    {
                        case 'H':
                            // According to 4.3, RFC2616:
                            // All responses to the HEAD request getMethod MUST NOT include a
                            // message-body, even though the presence of entity-header fields
                            // might lead one to believe they do.
                            if (HttpMethod.Head.Equals(method))
                            {
                                return true;

                                // The following code was inserted to work around the servers
                                // that behave incorrectly.  It has been commented out
                                // because it does not work with well behaving servers.
                                // Please note, even if the 'Transfer-Encoding: chunked'
                                // header exists in the HEAD response, the response should
                                // have absolutely no content.
                                //
                                // Interesting edge case:
                                // Some poorly implemented servers will send a zero-byte
                                // chunk if Transfer-Encoding of the response is 'chunked'.
                                //
                                // return !msg.isChunked();
                            }
                            break;
                        case 'C':
                            // Successful CONNECT request results in a response with empty body.
                            if (statusCode == 200)
                            {
                                if (HttpMethod.Connect.Equals(method))
                                {
                                    // Proxy connection established - Parse HTTP only if configured by parseHttpAfterConnectRequest,
                                    // else pass through.
                                    if (!_clientCodec._parseHttpAfterConnectRequest)
                                    {
                                        _clientCodec._done = true;
                                        _clientCodec._queue.Clear();
                                    }
                                    return true;
                                }
                            }
                            break;
                    }
                }
                return base.IsContentAlwaysEmpty(msg);
            }

            public override void ChannelInactive(IChannelHandlerContext ctx)
            {
                base.ChannelInactive(ctx);

                if (_clientCodec._failOnMissingResponse)
                {
                    long missingResponses = Volatile.Read(ref _clientCodec._requestResponseCounter);
                    if (missingResponses > 0)
                    {
                        _ = ctx.FireExceptionCaught(new PrematureChannelClosureException(
                            $"channel gone inactive with {missingResponses} missing response(s)"));
                    }
                }
            }
        }
    }
}
