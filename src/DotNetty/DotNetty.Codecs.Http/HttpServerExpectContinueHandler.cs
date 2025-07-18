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
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    public class HttpServerExpectContinueHandler : ChannelHandlerAdapter
    {
        static readonly IFullHttpResponse ExpectationFailed = new DefaultFullHttpResponse(
            HttpVersion.Http11, HttpResponseStatus.ExpectationFailed, Unpooled.Empty);

        static readonly IFullHttpResponse Accept = new DefaultFullHttpResponse(
            HttpVersion.Http11, HttpResponseStatus.Continue, Unpooled.Empty);

        static HttpServerExpectContinueHandler()
        {
            _ = ExpectationFailed.Headers.Set(HttpHeaderNames.ContentLength, HttpHeaderValues.Zero);
            _ = Accept.Headers.Set(HttpHeaderNames.ContentLength, HttpHeaderValues.Zero);
        }

        protected virtual IHttpResponse AcceptMessage(IHttpRequest request) => (IHttpResponse)Accept.RetainedDuplicate();

        protected virtual IHttpResponse RejectResponse(IHttpRequest request) => (IHttpResponse)ExpectationFailed.RetainedDuplicate();

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IHttpRequest req && HttpUtil.Is100ContinueExpected(req))
            {
                IHttpResponse accept = this.AcceptMessage(req);

                if (accept is null)
                {
                    // the expectation failed so we refuse the request.
                    IHttpResponse rejection = this.RejectResponse(req);
                    _ = ReferenceCountUtil.Release(message);
                    _ = context.WriteAndFlushAsync(rejection).ContinueWith(CloseOnFailureAction, context, TaskContinuationOptions.ExecuteSynchronously);
                    return;
                }

                _ = context.WriteAndFlushAsync(accept).ContinueWith(CloseOnFailureAction, context, TaskContinuationOptions.ExecuteSynchronously);
                _ = req.Headers.Remove(HttpHeaderNames.Expect);
            }
            _ = context.FireChannelRead(message);
        }

        static readonly Action<Task, object> CloseOnFailureAction = (t, s) => CloseOnFailure(t, s);
        static void CloseOnFailure(Task task, object state)
        {
            if (task.IsFaulted)
            {
                var context = (IChannelHandlerContext)state;
                _ = context.CloseAsync();
            }
            //return TaskUtil.Completed;
        }
    }
}
