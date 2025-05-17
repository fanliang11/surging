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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Embedded;

    public abstract class HttpContentEncoder : MessageToMessageCodec<IHttpRequest, IHttpObject>
    {
        private enum State
        {
            PassThrough,
            AwaitHeaders,
            AwaitContent
        }

        private static readonly AsciiString ZeroLengthHead = AsciiString.Cached("HEAD");
        private static readonly AsciiString ZeroLengthConnect = AsciiString.Cached("CONNECT");
        private static readonly int ContinueCode = HttpResponseStatus.Continue.Code;

        private readonly Deque<ICharSequence> _acceptEncodingQueue = new Deque<ICharSequence>();
        private EmbeddedChannel _encoder;
        private State _state = State.AwaitHeaders;

        /// <inheritdoc />
        public override bool AcceptOutboundMessage(object msg)
        {
            switch (msg)
            {
                case IHttpContent _:
                case IHttpResponse _:
                    return true;

                default:
                    return false;
            }
        }

        /// <inheritdoc />
        protected override void Decode(IChannelHandlerContext ctx, IHttpRequest msg, List<object> output)
        {
            var acceptEncodingHeaders = msg.Headers.GetAll(HttpHeaderNames.AcceptEncoding);
            ICharSequence acceptEncoding = acceptEncodingHeaders.Count switch
            {
                0 => HttpContentDecoder.Identity,
                1 => acceptEncodingHeaders[0],
                _ => StringUtil.Join(",", acceptEncodingHeaders),// Multiple message-header fields https://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
            };
            HttpMethod meth = msg.Method;
            if (HttpMethod.Head.Equals(meth))
            {
                acceptEncoding = ZeroLengthHead;
            }
            else if (HttpMethod.Connect.Equals(meth))
            {
                acceptEncoding = ZeroLengthConnect;
            }

            _acceptEncodingQueue.AddLast​(acceptEncoding);
            output.Add(ReferenceCountUtil.Retain(msg));
        }

        /// <inheritdoc />
        protected override void Encode(IChannelHandlerContext ctx, IHttpObject msg, List<object> output)
        {
            var res = msg as IHttpResponse;
            var lastContent = msg as ILastHttpContent;
            bool isFull = res is object && lastContent is object;
            switch (_state)
            {
                case State.AwaitHeaders:
                    {
                        EnsureHeaders(msg);
                        Debug.Assert(_encoder is null);

                        int code = res.Status.Code;
                        ICharSequence acceptEncoding;
                        if (code == StatusCodes.Status100Continue)
                        {
                            // We need to not poll the encoding when response with CONTINUE as another response will follow
                            // for the issued request. See https://github.com/netty/netty/issues/4079
                            acceptEncoding = null;
                        }
                        else
                        {
                            // Get the list of encodings accepted by the peer.
                            if (!_acceptEncodingQueue.TryRemoveFirst(out acceptEncoding))
                            {
                                ThrowHelper.ThrowInvalidOperationException_CannotSendMore();
                            }
                        }

                        //
                        // per rfc2616 4.3 Message Body
                        // All 1xx (informational), 204 (no content), and 304 (not modified) responses MUST NOT include a
                        // message-body. All other responses do include a message-body, although it MAY be of zero length.
                        //
                        // 9.4 HEAD
                        // The HEAD method is identical to GET except that the server MUST NOT return a message-body
                        // in the response.
                        //
                        // Also we should pass through HTTP/1.0 as transfer-encoding: chunked is not supported.
                        //
                        // See https://github.com/netty/netty/issues/5382
                        //
                        if (IsPassthru(res.ProtocolVersion, code, acceptEncoding))
                        {
                            if (isFull)
                            {
                                output.Add(ReferenceCountUtil.Retain(res));
                            }
                            else
                            {
                                output.Add(res);
                                // Pass through all following contents.
                                _state = State.PassThrough;
                            }
                            break;
                        }

                        if (isFull)
                        {
                            // Pass through the full response with empty content and continue waiting for the next resp.
                            if (!((IByteBufferHolder)res).Content.IsReadable())
                            {
                                output.Add(ReferenceCountUtil.Retain(res));
                                break;
                            }
                        }

                        // Prepare to encode the content.
                        Result result = BeginEncode(res, acceptEncoding);

                        // If unable to encode, pass through.
                        if (result is null)
                        {
                            if (isFull)
                            {
                                output.Add(ReferenceCountUtil.Retain(res));
                            }
                            else
                            {
                                output.Add(res);
                                // Pass through all following contents.
                                _state = State.PassThrough;
                            }
                            break;
                        }

                        _encoder = result.ContentEncoder;

                        // Encode the content and remove or replace the existing headers
                        // so that the message looks like a decoded message.
                        _ = res.Headers.Set(HttpHeaderNames.ContentEncoding, result.TargetContentEncoding);

                        // Output the rewritten response.
                        if (isFull)
                        {
                            // Convert full message into unfull one.
                            var newRes = new DefaultHttpResponse(res.ProtocolVersion, res.Status);
                            _ = newRes.Headers.Set(res.Headers);
                            output.Add(newRes);

                            EnsureContent(res);
                            EncodeFullResponse(newRes, (IHttpContent)res, output);
                            break;
                        }
                        else
                        {
                            // Make the response chunked to simplify content transformation.
                            _ = res.Headers.Remove(HttpHeaderNames.ContentLength);
                            _ = res.Headers.Set(HttpHeaderNames.TransferEncoding, HttpHeaderValues.Chunked);

                            output.Add(res);
                            _state = State.AwaitContent;

                            if (!(msg is IHttpContent))
                            {
                                // only break out the switch statement if we have not content to process
                                // See https://github.com/netty/netty/issues/2006
                                break;
                            }
                            // Fall through to encode the content
                            goto case State.AwaitContent;
                        }
                    }
                case State.AwaitContent:
                    {
                        EnsureContent(msg);
                        if (EncodeContent((IHttpContent)msg, output))
                        {
                            _state = State.AwaitHeaders;
                        }
                        break;
                    }
                case State.PassThrough:
                    {
                        EnsureContent(msg);
                        output.Add(ReferenceCountUtil.Retain(msg));
                        // Passed through all following contents of the current response.
                        if (lastContent is object)
                        {
                            _state = State.AwaitHeaders;
                        }
                        break;
                    }
            }
        }

        void EncodeFullResponse(IHttpResponse newRes, IHttpContent content, IList<object> output)
        {
            int existingMessages = output.Count;
            _ = EncodeContent(content, output);

            if (HttpUtil.IsContentLengthSet(newRes))
            {
                // adjust the content-length header
                int messageSize = 0;
                for (int i = existingMessages; i < output.Count; i++)
                {
                    if (output[i] is IHttpContent httpContent)
                    {
                        messageSize += httpContent.Content.ReadableBytes;
                    }
                }
                HttpUtil.SetContentLength(newRes, messageSize);
            }
            else
            {
                _ = newRes.Headers.Set(HttpHeaderNames.TransferEncoding, HttpHeaderValues.Chunked);
            }
        }

        static bool IsPassthru(HttpVersion version, int code, ICharSequence httpMethod)
        {
            switch (code)
            {
                case 204:
                case 304:
                    return true;
                case 200 when httpMethod.Equals(ZeroLengthConnect):
                    return true;
                default:
                    if (code < 200) { return true; }
                    break;
            }
            return httpMethod.Equals(ZeroLengthHead) || version.Equals(HttpVersion.Http10);
            //return code < 200 || code == 204 || code == 304
            //  || (ReferenceEquals(httpMethod, ZeroLengthHead) || ReferenceEquals(httpMethod, ZeroLengthConnect) && code == 200)
            //  || ReferenceEquals(version, HttpVersion.Http10);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static void EnsureHeaders(IHttpObject msg)
        {
            if (!(msg is IHttpResponse))
            {
                ThrowHelper.ThrowCodecException_EnsureHeaders(msg);
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static void EnsureContent(IHttpObject msg)
        {
            if (!(msg is IHttpContent))
            {
                ThrowHelper.ThrowCodecException_EnsureContent(msg);
            }
        }

        bool EncodeContent(IHttpContent c, IList<object> output)
        {
            IByteBuffer content = c.Content;

            Encode(content, output);

            if (c is ILastHttpContent last)
            {
                FinishEncode(output);

                // Generate an additional chunk if the decoder produced
                // the last product on closure,
                HttpHeaders headers = last.TrailingHeaders;
                if (headers.IsEmpty)
                {
                    output.Add(EmptyLastHttpContent.Default);
                }
                else
                {
                    output.Add(new ComposedLastHttpContent(headers, DecoderResult.Success));
                }
                return true;
            }
            return false;
        }

        protected abstract Result BeginEncode(IHttpResponse httpResponse, ICharSequence acceptEncoding);

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            CleanupSafely(context);
            base.HandlerRemoved(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            CleanupSafely(context);
            base.ChannelInactive(context);
        }

        void Cleanup()
        {
            if (_encoder is object)
            {
                // Clean-up the previous encoder if not cleaned up correctly.
                _ = _encoder.FinishAndReleaseAll();
                _encoder = null;
            }
        }

        void CleanupSafely(IChannelHandlerContext ctx)
        {
            try
            {
                Cleanup();
            }
            catch (Exception cause)
            {
                // If cleanup throws any error we need to propagate it through the pipeline
                // so we don't fail to propagate pipeline events.
                _ = ctx.FireExceptionCaught(cause);
            }
        }

        void Encode(IByteBuffer buf, IList<object> output)
        {
            // call retain here as it will call release after its written to the channel
            _ = _encoder.WriteOutbound(buf.Retain());
            FetchEncoderOutput(output);
        }

        void FinishEncode(IList<object> output)
        {
            if (_encoder.Finish())
            {
                FetchEncoderOutput(output);
            }
            _encoder = null;
        }

        void FetchEncoderOutput(ICollection<object> output)
        {
            while (true)
            {
                var buf = _encoder.ReadOutbound<IByteBuffer>();
                if (buf is null)
                {
                    break;
                }
                if (!buf.IsReadable())
                {
                    _ = buf.Release();
                    continue;
                }
                output.Add(new DefaultHttpContent(buf));
            }
        }

        public sealed class Result
        {
            public Result(ICharSequence targetContentEncoding, EmbeddedChannel contentEncoder)
            {
                if (targetContentEncoding is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.targetContentEncoding); }
                if (contentEncoder is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.contentEncoder); }

                TargetContentEncoding = targetContentEncoding;
                ContentEncoder = contentEncoder;
            }

            public ICharSequence TargetContentEncoding { get; }

            public EmbeddedChannel ContentEncoder { get; }
        }
    }
}
