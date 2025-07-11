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
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    public abstract class HttpObjectEncoder<T> : MessageToMessageEncoder<object> where T : IHttpMessage
    {
        const float HeadersWeightNew = 1 / 5f;
        const float HeadersWeightHistorical = 1 - HeadersWeightNew;
        const float TrailersWeightNew = HeadersWeightNew;
        const float TrailersWeightHistorical = HeadersWeightHistorical;

        const int StInit = 0;
        const int StContentNonChunk = 1;
        const int StContentChunk = 2;
        const int StContentAlwaysEmpty = 3;

        int state = StInit;

        // Used to calculate an exponential moving average of the encoded size of the initial line and the headers for
        // a guess for future buffer allocations.
        float headersEncodedSizeAccumulator = 256;

        // Used to calculate an exponential moving average of the encoded size of the trailers for
        // a guess for future buffer allocations.
        float trailersEncodedSizeAccumulator = 256;

        protected override void Encode(IChannelHandlerContext context, object message, List<object> output)
        {
            IByteBuffer buf = null;
            if (message is IHttpMessage)
            {
                if (this.state != StInit)
                {
                    ThrowHelper.ThrowInvalidOperationException_UnexpectedMsg(message, this.state);
                }

                var m = (T)message;

                buf = context.Allocator.Buffer((int)this.headersEncodedSizeAccumulator);
                // Encode the message.
                this.EncodeInitialLine(buf, m);
                this.state = this.IsContentAlwaysEmpty(m) ? StContentAlwaysEmpty
                    : HttpUtil.IsTransferEncodingChunked(m) ? StContentChunk : StContentNonChunk;

                this.SanitizeHeadersBeforeEncode(m, this.state == StContentAlwaysEmpty);

                this.EncodeHeaders(m.Headers, buf);
                _ = buf.WriteShort(HttpConstants.CrlfShort);

                this.headersEncodedSizeAccumulator = HeadersWeightNew * PadSizeForAccumulation(buf.ReadableBytes) 
                    + HeadersWeightHistorical * this.headersEncodedSizeAccumulator;
            }

            // Bypass the encoder in case of an empty buffer, so that the following idiom works:
            //
            //     ch.write(Unpooled.EMPTY_BUFFER).addListener(ChannelFutureListener.CLOSE);
            //
            // See https://github.com/netty/netty/issues/2983 for more information.
            if (message is IByteBuffer potentialEmptyBuf)
            {
                if (!potentialEmptyBuf.IsReadable())
                {
                    output.Add(potentialEmptyBuf.Retain());
                    return;
                }
            }

            switch (message)
            {
                case IHttpContent _:
                case IByteBuffer _:
                case IFileRegion _:
                    switch (this.state)
                    {
                        case StInit:
                            ThrowHelper.ThrowInvalidOperationException_UnexpectedMsg(message); break;
                        case StContentNonChunk:
                            long contentLength = ContentLength(message);
                            if (contentLength > 0)
                            {
                                if (buf is object && buf.WritableBytes >= contentLength && message is IHttpContent)
                                {
                                    // merge into other buffer for performance reasons
                                    _ = buf.WriteBytes(((IHttpContent)message).Content);
                                    output.Add(buf);
                                }
                                else
                                {
                                    if (buf is object)
                                    {
                                        output.Add(buf);
                                    }
                                    output.Add(EncodeAndRetain(message));
                                }

                                if (message is ILastHttpContent)
                                {
                                    this.state = StInit;
                                }
                                break;
                            }

                            goto case StContentAlwaysEmpty; // fall-through!
                        case StContentAlwaysEmpty:
                            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                            if (buf is object)
                            {
                                // We allocated a buffer so add it now.
                                output.Add(buf);
                            }
                            else
                            {
                                // Need to produce some output otherwise an
                                // IllegalStateException will be thrown as we did not write anything
                                // Its ok to just write an EMPTY_BUFFER as if there are reference count issues these will be
                                // propagated as the caller of the encode(...) method will release the original
                                // buffer.
                                // Writing an empty buffer will not actually write anything on the wire, so if there is a user
                                // error with msg it will not be visible externally
                                output.Add(Unpooled.Empty);
                            }

                            break;
                        case StContentChunk:
                            if (buf is object)
                            {
                                // We allocated a buffer so add it now.
                                output.Add(buf);
                            }
                            this.EncodeChunkedContent(context, message, ContentLength(message), output);

                            break;
                        default:
                            ThrowHelper.ThrowEncoderException_UnexpectedState(this.state, message); break;
                    }

                    if (message is ILastHttpContent)
                    {
                        this.state = StInit;
                    }
                    break;

                default:
                    output.Add(buf);
                    break;
            }
        }

        protected void EncodeHeaders(HttpHeaders headers, IByteBuffer buf)
        {
            foreach (HeaderEntry<AsciiString, ICharSequence> header in headers)
            {
                HttpHeadersEncoder.EncoderHeader(header.Key, header.Value, buf);
            }
        }

        void EncodeChunkedContent(IChannelHandlerContext context, object message, long contentLength, ICollection<object> output)
        {
            if (contentLength > 0)
            {
                var lengthHex = new AsciiString(Convert.ToString(contentLength, 16), Encoding.ASCII);
                IByteBuffer buf = context.Allocator.Buffer(lengthHex.Count + 2);
                _ = buf.WriteCharSequence(lengthHex, Encoding.ASCII);
                _ = buf.WriteShort(HttpConstants.CrlfShort);
                output.Add(buf);
                output.Add(EncodeAndRetain(message));
                output.Add(HttpConstants.CrlfBuf.Duplicate());
            }

            if (message is ILastHttpContent content)
            {
                HttpHeaders headers = content.TrailingHeaders;
                if (headers.IsEmpty)
                {
                    output.Add(HttpConstants.ZeroCrlfCrlfBuf.Duplicate());
                }
                else
                {
                    IByteBuffer buf = context.Allocator.Buffer((int)this.trailersEncodedSizeAccumulator);
                    _ = buf.WriteMedium(HttpConstants.ZeroCrlfMedium);
                    this.EncodeHeaders(headers, buf);
                    _ = buf.WriteShort(HttpConstants.CrlfShort);
                    this.trailersEncodedSizeAccumulator = TrailersWeightNew * PadSizeForAccumulation(buf.ReadableBytes) 
                        + TrailersWeightHistorical * this.trailersEncodedSizeAccumulator;
                    output.Add(buf);
                }
            }
            else if (0ul >= (ulong)contentLength)
            {
                // Need to produce some output otherwise an
                // IllegalstateException will be thrown
                output.Add(EncodeAndRetain(message));
            }
        }

        // Allows to sanitize headers of the message before encoding these.
        protected virtual void SanitizeHeadersBeforeEncode(T msg, bool isAlwaysEmpty)
        {
            // noop
        }

        protected virtual bool IsContentAlwaysEmpty(T msg) => false;

        /// <inheritdoc />
        public override bool AcceptOutboundMessage(object msg)
        {
            switch (msg)
            {
                case IHttpObject _:
                case IByteBuffer _:
                case IFileRegion _:
                    return true;
                default:
                    return false;
            }
        }

        static object EncodeAndRetain(object message)
        {
            switch (message)
            {
                case IByteBuffer buffer:
                    return buffer.Retain();
                case IHttpContent content:
                    return content.Content.Retain();
                case IFileRegion region:
                    return region.Retain();
                default:
                    ThrowHelper.ThrowInvalidOperationException_UnexpectedMsg(message); return null;
            }
        }

        static long ContentLength(object message)
        {
            switch (message)
            {
                case IHttpContent content:
                    return content.Content.ReadableBytes;
                case IByteBuffer buffer:
                    return buffer.ReadableBytes;
                case IFileRegion region:
                    return region.Count;
                default:
                    ThrowHelper.ThrowInvalidOperationException_UnexpectedMsg(message); return default;
            }
        }

        // Add some additional overhead to the buffer. The rational is that it is better to slightly over allocate and waste
        // some memory, rather than under allocate and require a resize/copy.
        // @param readableBytes The readable bytes in the buffer.
        // @return The {@code readableBytes} with some additional padding.
        static int PadSizeForAccumulation(int readableBytes) => (readableBytes << 2) / 3;

        protected internal abstract void EncodeInitialLine(IByteBuffer buf, T message);
    }
}
