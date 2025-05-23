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
    using System.Collections.Immutable;
    using System.Text;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// Splits an HTTP query string into a path string and key-value parameter pairs.
    /// This decoder is for one time use only.  Create a new instance for each URI:
    /// <para>
    /// <code>QueryStringDecoder decoder = new QueryStringDecoder("/hello?recipient=world&amp;x=1;y=2");</code>
    /// assert decoder.path().equals("/hello");
    /// assert decoder.parameters().get("recipient").get(0).equals("world");
    /// assert decoder.parameters().get("x").get(0).equals("1");
    /// assert decoder.parameters().get("y").get(0).equals("2");
    /// </para>
    ///
    /// This decoder can also decode the content of an HTTP POST request whose
    /// content type is <tt>application/x-www-form-urlencoded</tt>:
    /// <para>
    /// QueryStringDecoder decoder = new QueryStringDecoder("recipient=world&amp;x=1;y=2", false);
    /// ...
    /// </para>
    ///
    /// <h3>HashDOS vulnerability fix</h3>
    ///
    /// As a workaround to the <a href="https://netty.io/s/hashdos">HashDOS</a> vulnerability, the decoder
    /// limits the maximum number of decoded key-value parameter pairs, up to {@literal 1024} by
    /// default, and you can configure it when you construct the decoder by passing an additional
    /// integer parameter.
    /// </summary>
    public class QueryStringDecoder
    {
        private const int DefaultMaxParams = 1024;

        private readonly Encoding _charset;
        private readonly string _uri;
        private readonly int _maxParams;
        private readonly bool _semicolonIsNormalChar;
        private int _pathEndIdx;
        private string _path;
        private IDictionary<string, List<string>> _parameters;

        public QueryStringDecoder(string uri) : this(uri, HttpConstants.DefaultEncoding)
        {
        }

        public QueryStringDecoder(string uri, bool hasPath) : this(uri, HttpConstants.DefaultEncoding, hasPath)
        {
        }

        public QueryStringDecoder(string uri, Encoding charset) : this(uri, charset, true)
        {
        }

        public QueryStringDecoder(string uri, Encoding charset, bool hasPath) : this(uri, charset, hasPath, DefaultMaxParams)
        {
        }

        public QueryStringDecoder(string uri, Encoding charset, bool hasPath, int maxParams) : this(uri, charset, hasPath, maxParams, false)
        {
        }

        public QueryStringDecoder(string uri, Encoding charset, bool hasPath, int maxParams, bool semicolonIsNormalChar)
        {
            if (uri is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.uri); }
            if (charset is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.charset); }
            if ((uint)(maxParams - 1) > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_Positive(maxParams, ExceptionArgument.maxParams); }

            _uri = uri;
            _charset = charset;
            _maxParams = maxParams;
            _semicolonIsNormalChar = semicolonIsNormalChar;

            // -1 means that path end index will be initialized lazily
            _pathEndIdx = hasPath ? -1 : 0;
        }

        public QueryStringDecoder(Uri uri) : this(uri, HttpConstants.DefaultEncoding)
        {
        }

        public QueryStringDecoder(Uri uri, Encoding charset) : this(uri, charset, DefaultMaxParams)
        {
        }

        public QueryStringDecoder(Uri uri, Encoding charset, int maxParams) : this(uri, charset, maxParams, false)
        {
        }

        public QueryStringDecoder(Uri uri, Encoding charset, int maxParams, bool semicolonIsNormalChar)
        {
            if (uri is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.uri); }
            if (charset is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.charset); }
            if ((uint)(maxParams - 1) > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_Positive(maxParams, ExceptionArgument.maxParams); }

            string rawPath = uri.AbsolutePath;
            // Also take care of cut of things like "http://localhost"
            _uri = uri.PathAndQuery;
            _charset = charset;
            _maxParams = maxParams;
            _semicolonIsNormalChar = semicolonIsNormalChar;
            _pathEndIdx = rawPath.Length;
        }

        public override string ToString() => _uri;

        public string Path => _path ??= DecodeComponent(_uri, 0, PathEndIdx(), _charset, true);

        public IDictionary<string, List<string>> Parameters => _parameters ??= DecodeParams(_uri, PathEndIdx(), _charset, _maxParams, _semicolonIsNormalChar);

        public string RawPath() => _uri.Substring(0, PathEndIdx());

        public string RawQuery()
        {
            int start = _pathEndIdx + 1;
            return start < _uri.Length ? _uri.Substring(start) : StringUtil.EmptyString;
        }

        int PathEndIdx()
        {
            if (_pathEndIdx == -1)
            {
                _pathEndIdx = FindPathEndIndex(_uri);
            }
            return _pathEndIdx;
        }

        static IDictionary<string, List<string>> DecodeParams(string s, int from, Encoding charset, int paramsLimit, bool semicolonIsNormalChar)
        {
            int len = s.Length;
            if ((uint)from >= (uint)len)
            {
                return ImmutableDictionary<string, List<string>>.Empty;
            }
            if (s[from] == HttpConstants.QuestionMarkChar)
            {
                from++;
            }
            var parameters = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            int nameStart = from;
            int valueStart = -1;
            int i;
            //loop:
            for (i = from; i < len; i++)
            {
                switch (s[i])
                {
                    case HttpConstants.EqualsSignChar:
                        if (nameStart == i)
                        {
                            nameStart = i + 1;
                        }
                        else if (valueStart < nameStart)
                        {
                            valueStart = i + 1;
                        }
                        break;
                    case HttpConstants.SemicolonChar:
                        if (semicolonIsNormalChar)
                        {
                            continue;
                        }
                        goto case HttpConstants.AmpersandChar;
                    // fall-through
                    case HttpConstants.AmpersandChar:
                        if (AddParam(s, nameStart, valueStart, i, parameters, charset))
                        {
                            paramsLimit--;
                            if (0u >= (uint)paramsLimit)
                            {
                                return parameters;
                            }
                        }
                        nameStart = i + 1;
                        break;
                    case HttpConstants.NumberSignChar:
                        goto loop;
                }
            }
        loop:
            _ = AddParam(s, nameStart, valueStart, i, parameters, charset);
            return parameters;
        }

        static bool AddParam(string s, int nameStart, int valueStart, int valueEnd,
            Dictionary<string, List<string>> parameters, Encoding charset)
        {
            if (nameStart >= valueEnd)
            {
                return false;
            }
            if (valueStart <= nameStart)
            {
                valueStart = valueEnd + 1;
            }
            string name = DecodeComponent(s, nameStart, valueStart - 1, charset, false);
            string value = DecodeComponent(s, valueStart, valueEnd, charset, false);
            if (!parameters.TryGetValue(name, out List<string> values))
            {
                values = new List<string>(1);  // Often there's only 1 value.
                parameters.Add(name, values);
            }
            values.Add(value);
            return true;
        }

        public static string DecodeComponent(string s) => DecodeComponent(s, HttpConstants.DefaultEncoding);

        public static string DecodeComponent(string s, Encoding charset) => s is null
            ? StringUtil.EmptyString : DecodeComponent(s, 0, s.Length, charset, false);

        static string DecodeComponent(string s, int from, int toExcluded, Encoding charset, bool isPath)
        {
            int len = toExcluded - from;
            if (len <= 0)
            {
                return StringUtil.EmptyString;
            }
            int firstEscaped = -1;
            for (int i = from; i < toExcluded; i++)
            {
                char c = s[i];
                if (c == HttpConstants.PercentChar || c == HttpConstants.PlusSignChar && !isPath)
                {
                    firstEscaped = i;
                    break;
                }
            }
            if (firstEscaped == -1)
            {
                return s.Substring(from, len);
            }

            // Each encoded byte takes 3 characters (e.g. "%20")
            int decodedCapacity = (toExcluded - firstEscaped) / 3;
            var byteBuf = new byte[decodedCapacity];
            int idx;
            var strBuf = StringBuilderManager.Allocate(len);
            _ = strBuf.Append(s, from, firstEscaped - from);

            for (int i = firstEscaped; i < toExcluded; i++)
            {
                char c = s[i];
                if (c != HttpConstants.PercentChar)
                {
                    _ = strBuf.Append(c != HttpConstants.PlusSignChar || isPath ? c : StringUtil.Space);
                    continue;
                }

                idx = 0;
                do
                {
                    if (i + 3 > toExcluded)
                    {
                        StringBuilderManager.Free(strBuf); ThrowHelper.ThrowArgumentException_UnterminatedEscapeSeq(i, s);
                    }
                    byteBuf[idx++] = StringUtil.DecodeHexByte(s, i + 1);
                    i += 3;
                }
                while (i < toExcluded && s[i] == HttpConstants.PercentChar);
                i--;

                _ = strBuf.Append(charset.GetString(byteBuf, 0, idx));
            }

            return StringBuilderManager.ReturnAndFree(strBuf);
        }

        static int FindPathEndIndex(string uri)
        {
            int len = uri.Length;
            for (int i = 0; i < len; i++)
            {
                char c = uri[i];
                switch (c)
                {
                    case HttpConstants.QuestionMarkChar:
                    case HttpConstants.NumberSignChar:
                        return i;
                }
            }
            return len;
        }
    }
}
