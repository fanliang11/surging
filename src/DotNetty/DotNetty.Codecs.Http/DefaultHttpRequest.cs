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
    using DotNetty.Common.Internal;
    
    public class DefaultHttpRequest : DefaultHttpMessage, IHttpRequest
    {
        const int HashCodePrime = 31;

        HttpMethod method;
        string uri;

        public DefaultHttpRequest(HttpVersion httpVersion, HttpMethod method, string uri) 
            : this(httpVersion, method, uri, true)
        {
        }

        public DefaultHttpRequest(HttpVersion version, HttpMethod method, string uri, bool validateHeaders)
            : base(version, validateHeaders, false)
        {
            if (method is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.method); }
            if (uri is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.uri); }

            this.method = method;
            this.uri = uri;
        }

        public DefaultHttpRequest(HttpVersion version, HttpMethod method, string uri, HttpHeaders headers) 
            : base(version, headers)
        {
            if (method is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.method); }
            if (uri is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.uri); }

            this.method = method;
            this.uri = uri;
        }

        public HttpMethod Method => this.method;

        public string Uri => this.uri;

        public IHttpRequest SetMethod(HttpMethod value)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
            this.method = value;
            return this;
        }

        public IHttpRequest SetUri(string value)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
            this.uri = value;
            return this;
        }

        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode()
        {
            int result = 1;
            result = HashCodePrime * result + this.method.GetHashCode();
            result = HashCodePrime * result + this.uri.GetHashCode();
            result = HashCodePrime * result + base.GetHashCode();

            return result;
        }
        // ReSharper restore NonReadonlyMemberInGetHashCode

        public override bool Equals(object obj)
        {
            if (obj is DefaultHttpRequest other)
            {
                return this.method.Equals(other.method)
                    && string.Equals(this.uri, other.uri, StringComparison.OrdinalIgnoreCase)
                    && base.Equals(obj);
            }

            return false;
        }

        public override string ToString() => StringBuilderManager.ReturnAndFree(HttpMessageUtil.AppendRequest(StringBuilderManager.Allocate(256), this));
    }
}
