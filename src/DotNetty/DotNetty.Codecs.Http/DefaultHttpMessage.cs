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
    public abstract class DefaultHttpMessage : DefaultHttpObject, IHttpMessage
    {
        const int HashCodePrime = 31;
        HttpVersion version;
        readonly HttpHeaders headers;

        protected DefaultHttpMessage(HttpVersion version) : this(version, true, false)
        {
        }

        protected DefaultHttpMessage(HttpVersion version, bool validateHeaders, bool singleFieldHeaders)
        {
            if (version is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.version); }

            this.version = version;
            this.headers = singleFieldHeaders 
                ? new CombinedHttpHeaders(validateHeaders) 
                : new DefaultHttpHeaders(validateHeaders);
        }

        protected DefaultHttpMessage(HttpVersion version, HttpHeaders headers)
        {
            if (version is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.version); }
            if (headers is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.headers); }

            this.version = version;
            this.headers = headers;
        }

        public HttpHeaders Headers => this.headers;

        public HttpVersion ProtocolVersion => this.version;

        public override int GetHashCode()
        {
            int result = 1;
            result = HashCodePrime * result + this.headers.GetHashCode();
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            result = HashCodePrime * result + this.version.GetHashCode();
            result = HashCodePrime * result + base.GetHashCode();
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj is DefaultHttpMessage other)
            {
                return this.headers.Equals(other.headers)
                    && this.version.Equals(other.version)
                    && base.Equals(obj);
            }

            return false;
        }

        public IHttpMessage SetProtocolVersion(HttpVersion value)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
            this.version = value;
            return this;
        }
    }
}
