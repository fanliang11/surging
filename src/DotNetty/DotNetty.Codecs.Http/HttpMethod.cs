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
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    public sealed class HttpMethod : IEquatable<HttpMethod>, IComparable<HttpMethod>, IComparable
    {
        /// <summary>
        /// The OPTIONS method represents a request for information about the communication options
        /// available on the request/response chain identified by the Request-URI. This method allows
        /// the client to determine the options and/or requirements associated with a resource, or the
        /// capabilities of a server, without implying a resource action or initiating a resource retrieval.
        /// </summary>
        public static readonly HttpMethod Options = new HttpMethod("OPTIONS");

        /// <summary>
        /// The GET method means retrieve whatever information (in the form of an entity) is identified
        /// by the Request-URI.  If the Request-URI refers to a data-producing process, it is the
        /// produced data which shall be returned as the entity in the response and not the source text
        /// of the process, unless that text happens to be the output of the process.
        /// </summary>
        public static readonly HttpMethod Get = new HttpMethod("GET");

        /// <summary>
        /// The HEAD method is identical to GET except that the server MUST NOT return a message-body in the response.
        /// </summary>
        public static readonly HttpMethod Head = new HttpMethod("HEAD");

        /// <summary>
        /// The POST method is used to request that the origin server accept the entity enclosed in the
        /// request as a new subordinate of the resource identified by the Request-URI in the
        /// Request-Line.
        /// </summary>
        public static readonly HttpMethod Post = new HttpMethod("POST");

        /// <summary>
        /// The PUT method requests that the enclosed entity be stored under the supplied Request-URI.
        /// </summary>
        public static readonly HttpMethod Put = new HttpMethod("PUT");

        /// <summary>
        /// The PATCH method requests that a set of changes described in the
        /// request entity be applied to the resource identified by the Request-URI.
        /// </summary>
        public static readonly HttpMethod Patch = new HttpMethod("PATCH");

        /// <summary>
        /// The DELETE method requests that the origin server delete the resource identified by the Request-URI.
        /// </summary>
        public static readonly HttpMethod Delete = new HttpMethod("DELETE");

        /// <summary>
        /// The TRACE method is used to invoke a remote, application-layer loop- back of the request message.
        /// </summary>
        public static readonly HttpMethod Trace = new HttpMethod("TRACE");

        /// <summary>
        /// This specification reserves the method name CONNECT for use with a proxy that can dynamically
        /// switch to being a tunnel
        /// </summary>
        public static readonly HttpMethod Connect = new HttpMethod("CONNECT");

        const byte CByte = (byte)'C';
        const byte DByte = (byte)'D';
        const byte GByte = (byte)'G';
        const byte HByte = (byte)'H';
        const byte OByte = (byte)'O';
        const byte PByte = (byte)'P';
        const byte UByte = (byte)'U';
        const byte AByte = (byte)'A';
        const byte TByte = (byte)'T';

        static readonly CachedReadConcurrentDictionary<string, HttpMethod> s_methodCache =
            new CachedReadConcurrentDictionary<string, HttpMethod>(StringComparer.Ordinal);
        static readonly Func<string, HttpMethod> s_convertToHttpMethodFunc = n => ConvertToHttpMethod(n);

        // HashMap
        static readonly Dictionary<string, HttpMethod> MethodMap;

        static HttpMethod()
        {
            MethodMap = new Dictionary<string, HttpMethod>(StringComparer.Ordinal)
            {
                { Options.ToString(), Options },
                { Get.ToString(), Get },
                { Head.ToString(), Head },
                { Post.ToString(), Post },
                { Put.ToString(), Put },
                { Patch.ToString(), Patch },
                { Delete.ToString(), Delete },
                { Trace.ToString(), Trace },
                { Connect.ToString(), Connect },
            };
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        public static HttpMethod ValueOf(AsciiString name)
        {
            if (name is object)
            {
                HttpMethod result = ValueOfInline(name.Array);
                if (result is object)
                {
                    return result;
                }

                // Fall back to slow path
                var methodName = name.ToString();
                if (MethodMap.TryGetValue(methodName, out result))
                {
                    return result;
                }

                return s_methodCache.GetOrAdd(methodName, s_convertToHttpMethodFunc);
            }
            // Really slow path and error handling
            return new HttpMethod(name?.ToString());
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static HttpMethod ValueOfInline(byte[] bytes)
        {
            if ((uint)bytes.Length <= 2u)
            {
                return null;
            }

            HttpMethod match = null;
            int i = 0;
            switch (bytes[i++])
            {
                case CByte:
                    match = Connect;
                    break;
                case DByte:
                    match = Delete;
                    break;
                case GByte:
                    match = Get;
                    break;
                case HByte:
                    match = Head;
                    break;
                case OByte:
                    match = Options;
                    break;
                case PByte:
                    switch (bytes[i++])
                    {
                        case OByte:
                            match = Post;
                            break;
                        case UByte:
                            match = Put;
                            break;
                        case AByte:
                            match = Patch;
                            break;
                    }
                    break;
                case TByte:
                    match = Trace;
                    break;
            }
            if (match is object)
            {
                byte[] array = match.name.Array;
                if ((uint)bytes.Length == (uint)array.Length)
                {
                    for (; (uint)i < (uint)bytes.Length; i++)
                    {
                        if (bytes[i] != array[i])
                        {
                            match = null;
                            break;
                        }
                    }
                }
                else
                {
                    match = null;
                }
            }
            return match;
        }

        readonly AsciiString name;

        /// <summary>
        /// Creates a new HTTP method with the specified name.  You will not need to
        /// create a new method unless you are implementing a protocol derived from
        /// HTTP, such as
        /// http://en.wikipedia.org/wiki/Real_Time_Streaming_Protocol and
        /// http://en.wikipedia.org/wiki/Internet_Content_Adaptation_Protocol
        /// </summary>
        /// <param name="name"></param>
        public HttpMethod(string name)
        {
            if (name is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }

            name = name.Trim();
            if (string.IsNullOrEmpty(name))
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name);
            }

            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (CharUtil.IsISOControl(c) || char.IsWhiteSpace(c))
                {
                    ThrowHelper.ThrowArgumentException_InvalidMethodName(c, name);
                }
            }

            this.name = AsciiString.Cached(name);
        }

        public string Name => this.name.ToString();

        public AsciiString AsciiName => this.name;

        public override int GetHashCode() => this.name.GetHashCode();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) { return true; }

            return obj is HttpMethod other && this.name.Equals(other.name);
        }

        public bool Equals(HttpMethod other)
        {
            if (ReferenceEquals(this, other)) { return true; }
            return other is object && this.name.Equals(other.name);
        }

        public override string ToString() => this.name.ToString();

        public int CompareTo(object obj) => this.CompareTo(obj as HttpMethod);

        public int CompareTo(HttpMethod other)
        {
            if (ReferenceEquals(this, other)) { return 0; }
            if (other is null) { return 1; }
            return this.name.CompareTo(other.name);
        }

        private static HttpMethod ConvertToHttpMethod(string name)
        {
            if (MethodMap.TryGetValue(name, out var result))
            {
                return result;
            }

            return new HttpMethod(name);
        }
    }
}
