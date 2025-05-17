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
    using DotNetty.Common.Utilities;

    public static class HttpUtil
    {
        const int IndexNotFound = -1;
        const uint NIndexNotFound = unchecked((uint)IndexNotFound);

        static readonly AsciiString CharsetEquals = new AsciiString(HttpHeaderValues.Charset + "=");
        static readonly AsciiString Semicolon = AsciiString.Cached(";");

        public static ReadOnlySpan<byte> Http11Bytes => new byte[] { (byte)'H', (byte)'T', (byte)'T', (byte)'P', (byte)'/', (byte)'1', (byte)'.', (byte)'1' }; // "HTTP/1.1"

        ///// <summary>
        ///// Determine if a uri is in origin-form according to
        ///// <a href="https://tools.ietf.org/html/rfc7230#section-5.3">rfc7230, 5.3</a>.
        ///// </summary>
        ///// <param name="uri"></param>
        ///// <returns></returns>
        //public static bool IsOriginForm(Uri uri)
        //{
        //    return uri.Scheme is null && /*uri.getSchemeSpecificPart() is null &&*/ uri.PathAndQuery is null &&
        //           uri.Host is null && uri.Authority is null;
        //}

        ///// <summary>
        ///// Determine if a uri is in asterisk-form according to
        ///// <a href="https://tools.ietf.org/html/rfc7230#section-5.3">rfc7230, 5.3</a>.
        ///// </summary>
        ///// <param name="uri"></param>
        ///// <returns></returns>
        //public static bool IsAsteriskForm(Uri uri)
        //{
        //    return string.Equals("*", uri.AbsolutePath, StringComparison.Ordinal) &&
        //           uri.Scheme is null && /*uri.getSchemeSpecificPart() is null &&*/ uri.Host is null && uri.PathAndQuery is null &&
        //           uri.Port == 0 && uri.Authority is null && uri.Query is null &&
        //           uri.Fragment is null;
        //}

        public static bool IsKeepAlive(IHttpMessage message)
        {
            return !message.Headers.ContainsValue(HttpHeaderNames.Connection, HttpHeaderValues.Close, true) &&
                   (message.ProtocolVersion.IsKeepAliveDefault ||
                    message.Headers.ContainsValue(HttpHeaderNames.Connection, HttpHeaderValues.KeepAlive, true));
        }

        public static void SetKeepAlive(IHttpMessage message, bool keepAlive) => SetKeepAlive(message.Headers, message.ProtocolVersion, keepAlive);

        public static void SetKeepAlive(HttpHeaders headers, HttpVersion httpVersion, bool keepAlive)
        {
            if (httpVersion.IsKeepAliveDefault)
            {
                if (keepAlive)
                {
                    _ = headers.Remove(HttpHeaderNames.Connection);
                }
                else
                {
                    _ = headers.Set(HttpHeaderNames.Connection, HttpHeaderValues.Close);
                }
            }
            else
            {
                if (keepAlive)
                {
                    _ = headers.Set(HttpHeaderNames.Connection, HttpHeaderValues.KeepAlive);
                }
                else
                {
                    _ = headers.Remove(HttpHeaderNames.Connection);
                }
            }
        }

        public static long GetContentLength(IHttpMessage message)
        {
            if (message.Headers.TryGet(HttpHeaderNames.ContentLength, out ICharSequence value))
            {
                return CharUtil.ParseLong(value);
            }

            // We know the content length if it's a Web Socket message even if
            // Content-Length header is missing.
            long webSocketContentLength = GetWebSocketContentLength(message);
            if (webSocketContentLength >= 0)
            {
                return webSocketContentLength;
            }

            // Otherwise we don't.
            return ThrowHelper.FromFormatException_HeaderNotFound();
        }

        public static long GetContentLength(IHttpMessage message, long defaultValue)
        {
            if (message.Headers.TryGet(HttpHeaderNames.ContentLength, out ICharSequence value))
            {
                return CharUtil.ParseLong(value);
            }

            // We know the content length if it's a Web Socket message even if
            // Content-Length header is missing.
            long webSocketContentLength = GetWebSocketContentLength(message);
            if (webSocketContentLength >= 0)
            {
                return webSocketContentLength;
            }

            // Otherwise we don't.
            return defaultValue;
        }

        public static int GetContentLength(IHttpMessage message, int defaultValue) =>
            (int)Math.Min(int.MaxValue, GetContentLength(message, (long)defaultValue));

        static int GetWebSocketContentLength(IHttpMessage message)
        {
            // WebSocket messages have constant content-lengths.
            HttpHeaders h = message.Headers;
            switch (message)
            {
                case IHttpRequest req:
                    if (HttpMethod.Get.Equals(req.Method)
                        && h.Contains(HttpHeaderNames.SecWebsocketKey1)
                        && h.Contains(HttpHeaderNames.SecWebsocketKey2))
                    {
                        return 8;
                    }
                    break;

                case IHttpResponse res:
                    if (res.Status.Code == StatusCodes.Status101SwitchingProtocols
                        && h.Contains(HttpHeaderNames.SecWebsocketOrigin)
                        && h.Contains(HttpHeaderNames.SecWebsocketLocation))
                    {
                        return 16;
                    }
                    break;
            }

            // Not a web socket message
            return -1;
        }

        public static void SetContentLength(IHttpMessage message, long length) => message.Headers.Set(HttpHeaderNames.ContentLength, length);

        public static bool IsContentLengthSet(IHttpMessage message) => message.Headers.Contains(HttpHeaderNames.ContentLength);

        public static bool Is100ContinueExpected(IHttpMessage message)
        {
            return IsExpectHeaderValid(message)
              // unquoted tokens in the expect header are case-insensitive, thus 100-continue is case insensitive
              && message.Headers.Contains(HttpHeaderNames.Expect, HttpHeaderValues.Continue, true);
        }

        internal static bool IsUnsupportedExpectation(IHttpMessage message)
        {
            if (!IsExpectHeaderValid(message))
            {
                return false;
            }

            return message.Headers.TryGet(HttpHeaderNames.Expect, out ICharSequence expectValue)
                && !HttpHeaderValues.Continue.ContentEqualsIgnoreCase(expectValue);
        }

        // Expect: 100-continue is for requests only and it works only on HTTP/1.1 or later. Note further that RFC 7231
        // section 5.1.1 says "A server that receives a 100-continue expectation in an HTTP/1.0 request MUST ignore
        // that expectation."
        static bool IsExpectHeaderValid(IHttpMessage message) => message is IHttpRequest
            && message.ProtocolVersion.CompareTo(HttpVersion.Http11) >= 0;

        public static void Set100ContinueExpected(IHttpMessage message, bool expected)
        {
            if (expected)
            {
                _ = message.Headers.Set(HttpHeaderNames.Expect, HttpHeaderValues.Continue);
            }
            else
            {
                _ = message.Headers.Remove(HttpHeaderNames.Expect);
            }
        }

        public static bool IsTransferEncodingChunked(IHttpMessage message) => message.Headers.ContainsValue(HttpHeaderNames.TransferEncoding, HttpHeaderValues.Chunked, true);

        public static void SetTransferEncodingChunked(IHttpMessage m, bool chunked)
        {
            if (chunked)
            {
                _ = m.Headers.Set(HttpHeaderNames.TransferEncoding, HttpHeaderValues.Chunked);
                _ = m.Headers.Remove(HttpHeaderNames.ContentLength);
            }
            else
            {
                IList<ICharSequence> encodings = m.Headers.GetAll(HttpHeaderNames.TransferEncoding);
                if (0u >= (uint)encodings.Count)
                {
                    return;
                }
                var values = new List<ICharSequence>(encodings);
                foreach (ICharSequence value in encodings)
                {
                    if (HttpHeaderValues.Chunked.ContentEqualsIgnoreCase(value))
                    {
                        _ = values.Remove(value);
                    }
                }
                if (0u >= (uint)values.Count)
                {
                    _ = m.Headers.Remove(HttpHeaderNames.TransferEncoding);
                }
                else
                {
                    _ = m.Headers.Set(HttpHeaderNames.TransferEncoding, values);
                }
            }
        }

        public static Encoding GetCharset(IHttpMessage message) => GetCharset(message, Encoding.UTF8);

        public static Encoding GetCharset(ICharSequence contentTypeValue) => contentTypeValue is object ? GetCharset(contentTypeValue, Encoding.UTF8) : Encoding.UTF8;

        public static Encoding GetCharset(IHttpMessage message, Encoding defaultCharset)
        {
            return message.Headers.TryGet(HttpHeaderNames.ContentType, out ICharSequence contentTypeValue)
                ? GetCharset(contentTypeValue, defaultCharset)
                : defaultCharset;
        }

        public static Encoding GetCharset(ICharSequence contentTypeValue, Encoding defaultCharset)
        {
            if (contentTypeValue is object)
            {
                ICharSequence charsetCharSequence = GetCharsetAsSequence(contentTypeValue);
                if (charsetCharSequence is object)
                {
                    try
                    {
                        return Encoding.GetEncoding(charsetCharSequence.ToString());
                    }
                    catch (ArgumentException)
                    {
                        // just return the default charset
                    }
                    catch (NotSupportedException) // 平台不支持
                    {
                        // just return the default charset
                    }
                }
            }
            return defaultCharset;
        }

        public static ICharSequence GetCharsetAsSequence(IHttpMessage message)
            => message.Headers.TryGet(HttpHeaderNames.ContentType, out ICharSequence contentTypeValue) ? GetCharsetAsSequence(contentTypeValue) : null;

        public static ICharSequence GetCharsetAsSequence(ICharSequence contentTypeValue)
        {
            if (contentTypeValue is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.contentTypeValue);
            }
            int indexOfCharset = AsciiString.IndexOfIgnoreCaseAscii(contentTypeValue, CharsetEquals, 0);
            if ((uint)indexOfCharset >= NIndexNotFound) { return null; }
            int indexOfEncoding = indexOfCharset + CharsetEquals.Count;
            if (indexOfEncoding < contentTypeValue.Count)
            {
                var charsetCandidate = contentTypeValue.SubSequence(indexOfEncoding, contentTypeValue.Count);
                int indexOfSemicolon = AsciiString.IndexOfIgnoreCaseAscii(charsetCandidate, Semicolon, 0);
                if ((uint)indexOfSemicolon >= NIndexNotFound)
                {
                    return charsetCandidate;
                }

                return charsetCandidate.SubSequence(0, indexOfSemicolon);
            }
            return null;
        }

        public static ICharSequence GetMimeType(IHttpMessage message) =>
            message.Headers.TryGet(HttpHeaderNames.ContentType, out ICharSequence contentTypeValue) ? GetMimeType(contentTypeValue) : null;

        public static ICharSequence GetMimeType(ICharSequence contentTypeValue)
        {
            if (contentTypeValue is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.contentTypeValue);
            }
            int indexOfSemicolon = AsciiString.IndexOfIgnoreCaseAscii(contentTypeValue, Semicolon, 0);
            if ((uint)indexOfSemicolon < NIndexNotFound)
            {
                return contentTypeValue.SubSequence(0, indexOfSemicolon);
            }

            return (uint)contentTypeValue.Count > 0u ? contentTypeValue : null;
        }
    }
}
