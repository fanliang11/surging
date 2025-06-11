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
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.Cors
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    using static Common.Utilities.ReferenceCountUtil;

    public class CorsHandler : ChannelDuplexHandler
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<CorsHandler>();

        internal static readonly AsciiString AnyOrigin = new AsciiString("*");
        internal static readonly AsciiString NullOrigin = new AsciiString("null");

        CorsConfig config;
        IHttpRequest request;
        readonly IList<CorsConfig> configList;
        bool isShortCircuit;

        public CorsHandler(CorsConfig config)
            : this(config is object ? new List<CorsConfig>(new[] { config }) : null, config.IsShortCircuit)
        {
        }

        public CorsHandler(IList<CorsConfig> configList, bool isShortCircuit)
        {
            if (configList is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.configList); }
            if (0u >= (uint)configList.Count) { ThrowHelper.ThrowArgumentException_Positive(ExceptionArgument.configList); }
            this.configList = configList;
            this.isShortCircuit = isShortCircuit;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IHttpRequest req)
            {
                this.request = req;
                var origin = request.Headers.Get(HttpHeaderNames.Origin, null);
                this.config = GetForOrigin(origin);
                if (IsPreflightRequest(req))
                {
                    this.HandlePreflight(context, req);
                    return;
                }
                if (this.isShortCircuit && !(origin is null || this.config is object))
                {
                    Forbidden(context, req);
                    return;
                }
            }
            _ = context.FireChannelRead(message);
        }

        void HandlePreflight(IChannelHandlerContext ctx, IHttpRequest req)
        {
            var response = new DefaultFullHttpResponse(req.ProtocolVersion, HttpResponseStatus.OK, true, true);
            if (this.SetOrigin(response))
            {
                this.SetAllowMethods(response);
                this.SetAllowHeaders(response);
                this.SetAllowCredentials(response);
                this.SetMaxAge(response);
                this.SetPreflightHeaders(response);
            }
            if (!response.Headers.Contains(HttpHeaderNames.ContentLength))
            {
                _ = response.Headers.Set(HttpHeaderNames.ContentLength, HttpHeaderValues.Zero);
            }

            _ = Release(req);
            Respond(ctx, req, response);
        }

        void SetPreflightHeaders(IHttpResponse response) => response.Headers.Add(this.config.PreflightResponseHeaders());

        private CorsConfig GetForOrigin(ICharSequence requestOrigin)
        {
            foreach (var corsConfig in this.configList)
            {
                if (corsConfig.IsAnyOriginSupported)
                {
                    return corsConfig;
                }
                if (corsConfig.Origins.Contains(requestOrigin))
                {
                    return corsConfig;
                }
                if (corsConfig.IsNullOriginAllowed || NullOrigin.Equals(requestOrigin))
                {
                    return corsConfig;
                }
            }
            return null;
        }

        bool SetOrigin(IHttpResponse response)
        {
            if (!this.request.Headers.TryGet(HttpHeaderNames.Origin, out ICharSequence origin) || this.config is null)
            {
                return false;
            }
            if (NullOrigin.ContentEquals(origin) && this.config.IsNullOriginAllowed)
            {
                SetNullOrigin(response);
                return true;
            }
            if (this.config.IsAnyOriginSupported)
            {
                if (this.config.IsCredentialsAllowed)
                {
                    this.EchoRequestOrigin(response);
                    SetVaryHeader(response);
                }
                else
                {
                    SetAnyOrigin(response);
                }
                return true;
            }
            if (this.config.Origins.Contains(origin))
            {
                SetOrigin(response, origin);
                SetVaryHeader(response);
                return true;
            }
#if DEBUG
            if (Logger.DebugEnabled) Logger.RequestOriginWasNotAmongTheConfiguredOrigins(origin, this.config);
#endif

            return false;
        }

        void EchoRequestOrigin(IHttpResponse response) => SetOrigin(response, this.request.Headers.Get(HttpHeaderNames.Origin, null));

        static void SetVaryHeader(IHttpResponse response) => response.Headers.Set(HttpHeaderNames.Vary, HttpHeaderNames.Origin);

        static void SetAnyOrigin(IHttpResponse response) => SetOrigin(response, AnyOrigin);

        static void SetNullOrigin(IHttpResponse response) => SetOrigin(response, NullOrigin);

        static void SetOrigin(IHttpResponse response, ICharSequence origin) => response.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, origin);

        void SetAllowCredentials(IHttpResponse response)
        {
            if (this.config.IsCredentialsAllowed
                && !AsciiString.ContentEquals(response.Headers.Get(HttpHeaderNames.AccessControlAllowOrigin, null), AnyOrigin))
            {
                _ = response.Headers.Set(HttpHeaderNames.AccessControlAllowCredentials, new AsciiString("true"));
            }
        }

        static bool IsPreflightRequest(IHttpRequest request)
        {
            HttpHeaders headers = request.Headers;
            return HttpMethod.Options.Equals(request.Method)
                && headers.Contains(HttpHeaderNames.Origin)
                && headers.Contains(HttpHeaderNames.AccessControlRequestMethod);
        }

        void SetExposeHeaders(IHttpResponse response)
        {
            ISet<ICharSequence> headers = this.config.ExposedHeaders();
            if ((uint)headers.Count > 0u)
            {
                _ = response.Headers.Set(HttpHeaderNames.AccessControlExposeHeaders, headers);
            }
        }

        void SetAllowMethods(IHttpResponse response) => response.Headers.Set(HttpHeaderNames.AccessControlAllowMethods, this.config.AllowedRequestMethods());

        void SetAllowHeaders(IHttpResponse response) => response.Headers.Set(HttpHeaderNames.AccessControlAllowHeaders, this.config.AllowedRequestHeaders());

        void SetMaxAge(IHttpResponse response) => response.Headers.Set(HttpHeaderNames.AccessControlMaxAge, this.config.MaxAge);

        public override void Write(IChannelHandlerContext context, object message, IPromise promise)
        {
            if (this.config is object && this.config.IsCorsSupportEnabled && message is IHttpResponse response)
            {
                if (this.SetOrigin(response))
                {
                    this.SetAllowCredentials(response);
                    this.SetExposeHeaders(response);
                }
            }
            _ = context.WriteAsync(message, promise);
        }

        static void Forbidden(IChannelHandlerContext ctx, IHttpRequest request)
        {
            var response = new DefaultFullHttpResponse(request.ProtocolVersion, HttpResponseStatus.Forbidden);
            _ = response.Headers.Set(HttpHeaderNames.ContentLength, HttpHeaderValues.Zero);
            _ = Release(request);
            Respond(ctx, request, response);
        }

        static void Respond(IChannelHandlerContext ctx, IHttpRequest request, IHttpResponse response)
        {
            bool keepAlive = HttpUtil.IsKeepAlive(request);

            HttpUtil.SetKeepAlive(response, keepAlive);

            Task task = ctx.WriteAndFlushAsync(response);
            if (!keepAlive)
            {
                _ = task.CloseOnComplete(ctx);
            }
        }
    }
}
