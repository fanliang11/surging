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
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// Configuration for Cross-Origin Resource Sharing (CORS).
    /// </summary>
    public sealed class CorsConfig
    {
        readonly ISet<ICharSequence> _origins;
        readonly bool _anyOrigin;
        readonly bool _enabled;
        readonly ISet<ICharSequence> _exposeHeaders;
        readonly bool _allowCredentials;
        readonly long _maxAge;
        readonly ISet<HttpMethod> _allowedRequestMethods;
        readonly ISet<AsciiString> _allowedRequestHeaders;
        readonly bool _allowNullOrigin;
        readonly IDictionary<AsciiString, ICallable<object>> _preflightHeaders;
        readonly bool _shortCircuit;

        internal CorsConfig(CorsConfigBuilder builder)
        {
            _origins = new HashSet<ICharSequence>(builder.origins, AsciiString.CaseSensitiveHasher);
            _anyOrigin = builder.anyOrigin;
            _enabled = builder.enabled;
            _exposeHeaders = builder.exposeHeaders;
            _allowCredentials = builder.allowCredentials;
            _maxAge = builder.maxAge;
            _allowedRequestMethods = builder.requestMethods;
            _allowedRequestHeaders = builder.requestHeaders;
            _allowNullOrigin = builder.allowNullOrigin;
            _preflightHeaders = builder.preflightHeaders;
            _shortCircuit = builder.shortCircuit;
        }

        /// <summary>
        /// Determines if support for CORS is enabled.
        /// </summary>
        /// <returns><c>true</c> if support for CORS is enabled, false otherwise.</returns>
        public bool IsCorsSupportEnabled => _enabled;

        /// <summary>
        /// Determines whether a wildcard origin, '*', is supported.
        /// </summary>
        /// <returns><c>true</c> if any origin is allowed.</returns>
        public bool IsAnyOriginSupported => _anyOrigin;

        /// <summary>
        /// Returns the allowed origin. This can either be a wildcard or an origin value.
        /// </summary>
        /// <returns>the value that will be used for the CORS response header 'Access-Control-Allow-Origin'</returns>
        public ICharSequence Origin => 0u >= (uint)_origins.Count ? CorsHandler.AnyOrigin : _origins.First();

        /// <summary>
        /// Returns the set of allowed origins.
        /// </summary>
        public ISet<ICharSequence> Origins => _origins;

        /// <summary>
        /// Web browsers may set the 'Origin' request header to 'null' if a resource is loaded
        /// from the local file system.
        /// 
        /// <para>If isNullOriginAllowed is true then the server will response with the wildcard for the
        /// the CORS response header 'Access-Control-Allow-Origin'.</para>
        /// </summary>
        /// <returns><c>true</c> if a 'null' origin should be supported.</returns>
        public bool IsNullOriginAllowed => _allowNullOrigin;

        /// <summary>
        /// Returns a set of headers to be exposed to calling clients.
        /// 
        /// <para>During a simple CORS request only certain response headers are made available by the
        /// browser, for example using:</para>
        /// <code>
        /// xhr.getResponseHeader("Content-Type");
        /// </code>
        /// The headers that are available by default are:
        /// <ul>
        /// <li>Cache-Control</li>
        /// <li>Content-Language</li>
        /// <li>Content-Type</li>
        /// <li>Expires</li>
        /// <li>Last-Modified</li>
        /// <li>Pragma</li>
        /// </ul>
        /// <para>To expose other headers they need to be specified, which is what this method enables by
        /// adding the headers names to the CORS 'Access-Control-Expose-Headers' response header.</para>
        /// </summary>
        /// <returns><see cref="ISet{ICharSequence}"/> a list of the headers to expose.</returns>
        public ISet<ICharSequence> ExposedHeaders() => _exposeHeaders.ToImmutableHashSet();

        /// <summary>
        /// Determines if cookies are supported for CORS requests.
        ///
        /// <para>By default cookies are not included in CORS requests but if isCredentialsAllowed returns
        /// true cookies will be added to CORS requests. Setting this value to true will set the
        /// CORS 'Access-Control-Allow-Credentials' response header to true.</para>
        ///
        /// <para>Please note that cookie support needs to be enabled on the client side as well.
        /// The client needs to opt-in to send cookies by calling:</para>
        /// <code>
        /// xhr.withCredentials = true;
        /// </code>
        /// <para>The default value for 'withCredentials' is false in which case no cookies are sent.
        /// Setting this to true will included cookies in cross origin requests.</para>
        /// </summary>
        /// <returns><c>true</c> if cookies are supported.</returns>
        public bool IsCredentialsAllowed => _allowCredentials;

        /// <summary>
        /// Gets the maxAge setting.
        ///
        /// <para>When making a preflight request the client has to perform two request with can be inefficient.
        /// This setting will set the CORS 'Access-Control-Max-Age' response header and enables the
        /// caching of the preflight response for the specified time. During this time no preflight
        /// request will be made.</para>
        /// </summary>
        /// <returns>the time in seconds that a preflight request may be cached.</returns>
        public long MaxAge => _maxAge;

        /// <summary>
        /// Returns the allowed set of Request Methods. The Http methods that should be returned in the
        /// CORS 'Access-Control-Request-Method' response header.
        /// </summary>
        /// <returns><see cref="ISet{T}"/> of <see cref="HttpMethod"/>s that represent the allowed Request Methods.</returns>
        public ISet<HttpMethod> AllowedRequestMethods() => _allowedRequestMethods.ToImmutableHashSet();

        /// <summary>
        /// Returns the allowed set of Request Headers.
        /// </summary>
        /// <remarks>
        /// The header names returned from this method will be used to set the CORS
        /// 'Access-Control-Allow-Headers' response header.
        /// </remarks>
        /// <returns><see cref="ISet{AsciiString}"/> of strings that represent the allowed Request Headers.</returns>
        public ISet<AsciiString> AllowedRequestHeaders() => _allowedRequestHeaders.ToImmutableHashSet();

        /// <summary>
        /// Returns HTTP response headers that should be added to a CORS preflight response.
        /// </summary>
        /// <returns><see cref="HttpHeaders"/> the HTTP response headers to be added.</returns>
        public HttpHeaders PreflightResponseHeaders()
        {
            if (0u >= (uint)_preflightHeaders.Count)
            {
                return EmptyHttpHeaders.Default;
            }
            HttpHeaders headers = new DefaultHttpHeaders();
            foreach (KeyValuePair<AsciiString, ICallable<object>> entry in _preflightHeaders)
            {
                object value = GetValue(entry.Value);
                if (value is IEnumerable<object> values)
                {
                    _ = headers.Add(entry.Key, values);
                }
                else
                {
                    _ = headers.Add(entry.Key, value);
                }
            }
            return headers;
        }

        /// <summary>
        /// Determines whether a CORS request should be rejected if it's invalid before being
        /// further processing.
        /// </summary>
        /// <remarks>
        /// CORS headers are set after a request is processed. This may not always be desired
        /// and this setting will check that the Origin is valid and if it is not valid no
        /// further processing will take place, and an error will be returned to the calling client.
        /// </remarks>
        /// <returns><c>true</c> if a CORS request should short-circuit upon receiving an invalid Origin header.</returns>
        public bool IsShortCircuit => _shortCircuit;

        static object GetValue(ICallable<object> callable)
        {
            try
            {
                return callable.Call();
            }
            catch (Exception exception)
            {
                return ThrowHelper.FromInvalidOperationException_Cqrs(callable, exception);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = StringBuilderManager.Allocate();
            _ = builder.Append($"{StringUtil.SimpleClassName(this)}")
                .Append($"[enabled = {_enabled}");

            _ = builder.Append(", origins=");
            if (0u >= (uint)Origins.Count)
            {
                _ = builder.Append("*");
            }
            else
            {
                _ = builder.Append("(");
                foreach (ICharSequence value in Origins)
                {
                    _ = builder.Append($"'{value}'");
                }
                _ = builder.Append(")");
            }

            _ = builder.Append(", exposedHeaders=");
            if (0u >= (uint)_exposeHeaders.Count)
            {
                _ = builder.Append("*");
            }
            else
            {
                _ = builder.Append("(");
                foreach (ICharSequence value in _exposeHeaders)
                {
                    _ = builder.Append($"'{value}'");
                }
                _ = builder.Append(")");
            }

            _ = builder.Append($", isCredentialsAllowed={_allowCredentials}");
            _ = builder.Append($", maxAge={_maxAge}");

            _ = builder.Append(", allowedRequestMethods=");
            if (0u >= (uint)_allowedRequestMethods.Count)
            {
                _ = builder.Append("*");
            }
            else
            {
                _ = builder.Append("(");
                foreach (HttpMethod value in _allowedRequestMethods)
                {
                    _ = builder.Append($"'{value}'");
                }
                _ = builder.Append(")");
            }

            _ = builder.Append(", allowedRequestHeaders=");
            if (0u >= (uint)_allowedRequestHeaders.Count)
            {
                _ = builder.Append("*");
            }
            else
            {
                _ = builder.Append("(");
                foreach (AsciiString value in _allowedRequestHeaders)
                {
                    _ = builder.Append($"'{value}'");
                }
                _ = builder.Append(")");
            }

            _ = builder.Append(", preflightHeaders=");
            if (0u >= (uint)_preflightHeaders.Count)
            {
                _ = builder.Append("*");
            }
            else
            {
                _ = builder.Append("(");
                foreach (AsciiString value in _preflightHeaders.Keys)
                {
                    _ = builder.Append($"'{value}'");
                }
                _ = builder.Append(")");
            }

            _ = builder.Append("]");
            return StringBuilderManager.ReturnAndFree(builder);
        }
    }
}
