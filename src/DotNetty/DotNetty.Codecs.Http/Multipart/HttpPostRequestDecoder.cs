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

namespace DotNetty.Codecs.Http.Multipart
{
    using System.Collections.Generic;
    using System.Text;
    using DotNetty.Common.Utilities;

    public class HttpPostRequestDecoder : IInterfaceHttpPostRequestDecoder
    {
        internal static readonly int DefaultDiscardThreshold = 10 * 1024 * 1024;

        readonly IInterfaceHttpPostRequestDecoder _decoder;

        public HttpPostRequestDecoder(IHttpRequest request)
            : this(new DefaultHttpDataFactory(DefaultHttpDataFactory.MinSize), request, HttpConstants.DefaultEncoding)
        {
        }

        public HttpPostRequestDecoder(IHttpDataFactory factory, IHttpRequest request)
            : this(factory, request, HttpConstants.DefaultEncoding)
        {
        }

        internal HttpPostRequestDecoder(IHttpDataFactory factory, IHttpRequest request, Encoding encoding)
        {
            if (factory is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.factory); }
            if (request is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.request); }
            if (encoding is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.encoding); }

            // Fill default values
            if (IsMultipartRequest(request))
            {
                _decoder = new HttpPostMultipartRequestDecoder(factory, request, encoding);
            }
            else
            {
                _decoder = new HttpPostStandardRequestDecoder(factory, request, encoding);
            }
        }

        public static bool IsMultipartRequest(IHttpRequest request)
        {
            if (request.Headers.TryGet(HttpHeaderNames.ContentType, out ICharSequence mimeType))
            {
                if (mimeType.StartsWith(HttpHeaderValues.MultipartFormData))
                {
                    return GetMultipartDataBoundary(mimeType) is object;
                }
            }
            return false;
        }

        // 
        // Check from the request ContentType if this request is a Multipart request.
        // return an array of String if multipartDataBoundary exists with the multipartDataBoundary
        // as first element, charset if any as second (missing if not set), else null
        //
        protected internal static ICharSequence[] GetMultipartDataBoundary(ICharSequence contentType)
        {
            // Check if Post using "multipart/form-data; boundary=--89421926422648 [; charset=xxx]"
            ICharSequence[] headerContentType = SplitHeaderContentType(contentType);
            AsciiString multiPartHeader = HttpHeaderValues.MultipartFormData;
            if (headerContentType[0].RegionMatchesIgnoreCase(0, multiPartHeader, 0, multiPartHeader.Count))
            {
                int mrank;
                int crank;
                AsciiString boundaryHeader = HttpHeaderValues.Boundary;
                if (headerContentType[1].RegionMatchesIgnoreCase(0, boundaryHeader, 0, boundaryHeader.Count))
                {
                    mrank = 1;
                    crank = 2;
                }
                else if (headerContentType[2].RegionMatchesIgnoreCase(0, boundaryHeader, 0, boundaryHeader.Count))
                {
                    mrank = 2;
                    crank = 1;
                }
                else
                {
                    return null;
                }
                ICharSequence boundary = headerContentType[mrank].SubstringAfter(HttpConstants.EqualsSignChar);
                if (boundary is null)
                {
                    ThrowHelper.ThrowErrorDataDecoderException_NeedBoundaryValue();
                }
                if (boundary[0] == HttpConstants.DoubleQuoteChar)
                {
                    ICharSequence bound = CharUtil.Trim(boundary);
                    int index = bound.Count - 1;
                    if (bound[index] == HttpConstants.DoubleQuoteChar)
                    {
                        boundary = bound.SubSequence(1, index);
                    }
                }
                AsciiString charsetHeader = HttpHeaderValues.Charset;
                if (headerContentType[crank].RegionMatchesIgnoreCase(0, charsetHeader, 0, charsetHeader.Count))
                {
                    ICharSequence charset = headerContentType[crank].SubstringAfter(HttpConstants.EqualsSignChar);
                    if (charset is object)
                    {
                        return new[]
                        {
                            new StringCharSequence("--" + boundary.ToString()),
                            charset
                        };
                    }
                }

                return new ICharSequence[]
                {
                    new StringCharSequence("--" + boundary.ToString())
                };
            }

            return null;
        }

        public bool IsMultipart => _decoder.IsMultipart;

        public int DiscardThreshold
        {
            get => _decoder.DiscardThreshold;
            set => _decoder.DiscardThreshold = value;
        }

        public List<IInterfaceHttpData> GetBodyHttpDatas() => _decoder.GetBodyHttpDatas();

        public List<IInterfaceHttpData> GetBodyHttpDatas(string name) => _decoder.GetBodyHttpDatas(name);

        public IInterfaceHttpData GetBodyHttpData(string name) => _decoder.GetBodyHttpData(name);

        public IInterfaceHttpPostRequestDecoder Offer(IHttpContent content) => _decoder.Offer(content);

        public bool HasNext => _decoder.HasNext;

        public IInterfaceHttpData Next() => _decoder.Next();

        public IInterfaceHttpData CurrentPartialHttpData => _decoder.CurrentPartialHttpData;

        public void Destroy() => _decoder.Destroy();

        public void CleanFiles() => _decoder.CleanFiles();

        public void RemoveHttpDataFromClean(IInterfaceHttpData data) => _decoder.RemoveHttpDataFromClean(data);

        static ICharSequence[] SplitHeaderContentType(ICharSequence sb)
        {
            int aStart = HttpPostBodyUtil.FindNonWhitespace(sb, 0);
            int aEnd = sb.IndexOf(HttpConstants.SemicolonChar);
            if (aEnd == -1)
            {
                return new[] { sb, StringCharSequence.Empty, StringCharSequence.Empty };
            }
            int bStart = HttpPostBodyUtil.FindNonWhitespace(sb, aEnd + 1);
            if (sb[aEnd - 1] == HttpConstants.SpaceChar)
            {
                aEnd--;
            }
            int bEnd = sb.IndexOf(HttpConstants.SemicolonChar, bStart);
            if (bEnd == -1)
            {
                bEnd = HttpPostBodyUtil.FindEndOfString(sb);
                return new[] { sb.SubSequence(aStart, aEnd), sb.SubSequence(bStart, bEnd), StringCharSequence.Empty };
            }
            int cStart = HttpPostBodyUtil.FindNonWhitespace(sb, bEnd + 1);
            if (sb[bEnd - 1] == HttpConstants.SpaceChar)
            {
                bEnd--;
            }
            int cEnd = HttpPostBodyUtil.FindEndOfString(sb);
            return new[] { sb.SubSequence(aStart, aEnd), sb.SubSequence(bStart, bEnd), sb.SubSequence(cStart, cEnd) };
        }
    }
}
