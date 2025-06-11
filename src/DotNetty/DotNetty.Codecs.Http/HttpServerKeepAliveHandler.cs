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
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    using static HttpUtil;

    public class HttpServerKeepAliveHandler : ChannelDuplexHandler
    {
        static readonly AsciiString MultipartPrefix = new AsciiString("multipart");

        bool persistentConnection = true;
        // Track pending responses to support client pipelining: https://tools.ietf.org/html/rfc7230#section-6.3.2
        int pendingResponses;

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            // read message and track if it was keepAlive
            if (message is IHttpRequest request)
            {
                if (this.persistentConnection)
                {
                    this.pendingResponses += 1;
                    this.persistentConnection = IsKeepAlive(request);
                }
            }
            _ = context.FireChannelRead(message);
        }

        public override void Write(IChannelHandlerContext context, object message, IPromise promise)
        {
            // modify message on way out to add headers if needed
            if (message is IHttpResponse response)
            {
                this.TrackResponse(response);
                // Assume the response writer knows if they can persist or not and sets isKeepAlive on the response
                if (!IsKeepAlive(response) || !IsSelfDefinedMessageLength(response))
                {
                    // No longer keep alive as the client can't tell when the message is done unless we close connection
                    this.pendingResponses = 0;
                    this.persistentConnection = false;
                }
                // Server might think it can keep connection alive, but we should fix response header if we know better
                if (!this.ShouldKeepAlive())
                {
                    SetKeepAlive(response, false);
                }
            }

            if (message is ILastHttpContent && !this.ShouldKeepAlive())
            {
                promise = promise.Unvoid();
                _ = promise.Task.CloseOnComplete(context);
            }
            base.Write(context, message, promise);
        }

        void TrackResponse(IHttpResponse response)
        {
            if (!IsInformational(response))
            {
                this.pendingResponses -= 1;
            }
        }

        bool ShouldKeepAlive() => this.pendingResponses != 0 || this.persistentConnection;

        /// <summary>
        /// Keep-alive only works if the client can detect when the message has ended without relying on the connection being
        /// closed.
        /// https://tools.ietf.org/html/rfc7230#section-6.3
        /// https://tools.ietf.org/html/rfc7230#section-3.3.2
        /// https://tools.ietf.org/html/rfc7230#section-3.3.3
        /// </summary>
        /// <param name="response">The HttpResponse to check</param>
        /// <returns>true if the response has a self defined message length.</returns>
        static bool IsSelfDefinedMessageLength(IHttpResponse response) => 
            IsContentLengthSet(response) || IsTransferEncodingChunked(response) || IsMultipart(response) 
            || IsInformational(response) || response.Status.Code == StatusCodes.Status204NoContent;

        static bool IsInformational(IHttpResponse response) => response.Status.CodeClass == HttpStatusClass.Informational;

        static bool IsMultipart(IHttpResponse response)
        {
            return response.Headers.TryGet(HttpHeaderNames.ContentType, out ICharSequence contentType)
                && contentType.RegionMatchesIgnoreCase(0, MultipartPrefix, 0, MultipartPrefix.Count);
        }
    }
}
