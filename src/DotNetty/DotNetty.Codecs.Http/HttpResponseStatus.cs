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
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal;
    using DotNetty.Common.Utilities;

    /// <summary>
    /// The response code and its description of HTTP or its derived protocols, such as
    /// RTSP (http://en.wikipedia.org/wiki/Real_Time_Streaming_Protocol) and 
    /// ICAP (http://en.wikipedia.org/wiki/Internet_Content_Adaptation_Protocol) 
    /// </summary>
    public class HttpResponseStatus : IEquatable<HttpResponseStatus>, IComparable<HttpResponseStatus>, IComparable
    {
        /// <summary>100 Continue</summary>
        public static readonly HttpResponseStatus Continue = NewStatus(100, "Continue");

        /// <summary>101 Switching Protocols</summary>
        public static readonly HttpResponseStatus SwitchingProtocols = NewStatus(101, "Switching Protocols");

        /// <summary>102 Processing (WebDAV, RFC2518)</summary>
        public static readonly HttpResponseStatus Processing = NewStatus(102, "Processing");

        /// <summary>200 OK</summary>
        public static readonly HttpResponseStatus OK = NewStatus(200, "OK");

        /// <summary>201 Created</summary>
        public static readonly HttpResponseStatus Created = NewStatus(201, "Created");

        /// <summary>202 Accepted</summary>
        public static readonly HttpResponseStatus Accepted = NewStatus(202, "Accepted");

        /// <summary>203 Non-Authoritative Information (since HTTP/1.1)</summary>
        public static readonly HttpResponseStatus NonAuthoritativeInformation = NewStatus(203, "Non-Authoritative Information");

        /// <summary>204 No Content</summary>
        public static readonly HttpResponseStatus NoContent = NewStatus(204, "No Content");

        /// <summary>205 Reset Content</summary>
        public static readonly HttpResponseStatus ResetContent = NewStatus(205, "Reset Content");

        /// <summary>206 Partial Content</summary>
        public static readonly HttpResponseStatus PartialContent = NewStatus(206, "Partial Content");

        /// <summary>207 Multi-Status (WebDAV, RFC2518)</summary>
        public static readonly HttpResponseStatus MultiStatus = NewStatus(207, "Multi-Status");

        /// <summary>300 Multiple Choices</summary>
        public static readonly HttpResponseStatus MultipleChoices = NewStatus(300, "Multiple Choices");

        /// <summary>301 Moved Permanently</summary>
        public static readonly HttpResponseStatus MovedPermanently = NewStatus(301, "Moved Permanently");

        /// <summary>302 Found</summary>
        public static readonly HttpResponseStatus Found = NewStatus(302, "Found");

        /// <summary>303 See Other (since HTTP/1.1)</summary>
        public static readonly HttpResponseStatus SeeOther = NewStatus(303, "See Other");

        /// <summary>304 Not Modified</summary>
        public static readonly HttpResponseStatus NotModified = NewStatus(304, "Not Modified");

        /// <summary>305 Use Proxy (since HTTP/1.1)</summary>
        public static readonly HttpResponseStatus UseProxy = NewStatus(305, "Use Proxy");

        /// <summary>307 Temporary Redirect (since HTTP/1.1)</summary>
        public static readonly HttpResponseStatus TemporaryRedirect = NewStatus(307, "Temporary Redirect");

        /// <summary>308 Permanent Redirect (RFC7538)</summary>
        public static readonly HttpResponseStatus PermanentRedirect = NewStatus(308, "Permanent Redirect");

        /// <summary>400 Bad Request</summary>
        public static readonly HttpResponseStatus BadRequest = NewStatus(400, "Bad Request");

        /// <summary>401 Unauthorized</summary>
        public static readonly HttpResponseStatus Unauthorized = NewStatus(401, "Unauthorized");

        /// <summary>402 Payment Required</summary>
        public static readonly HttpResponseStatus PaymentRequired = NewStatus(402, "Payment Required");

        /// <summary>403 Forbidden</summary>
        public static readonly HttpResponseStatus Forbidden = NewStatus(403, "Forbidden");

        /// <summary>404 Not Found</summary>
        public static readonly HttpResponseStatus NotFound = NewStatus(404, "Not Found");

        /// <summary>405 Method Not Allowed</summary>
        public static readonly HttpResponseStatus MethodNotAllowed = NewStatus(405, "Method Not Allowed");

        /// <summary>406 Not Acceptable</summary>
        public static readonly HttpResponseStatus NotAcceptable = NewStatus(406, "Not Acceptable");

        /// <summary>407 Proxy Authentication Required</summary>
        public static readonly HttpResponseStatus ProxyAuthenticationRequired = NewStatus(407, "Proxy Authentication Required");

        /// <summary>408 Request Timeout</summary>
        public static readonly HttpResponseStatus RequestTimeout = NewStatus(408, "Request Timeout");

        /// <summary>409 Conflict</summary>
        public static readonly HttpResponseStatus Conflict = NewStatus(409, "Conflict");

        /// <summary>410 Gone</summary>
        public static readonly HttpResponseStatus Gone = NewStatus(410, "Gone");

        /// <summary>411 Length Required</summary>
        public static readonly HttpResponseStatus LengthRequired = NewStatus(411, "Length Required");

        /// <summary>412 Precondition Failed</summary>
        public static readonly HttpResponseStatus PreconditionFailed = NewStatus(412, "Precondition Failed");

        /// <summary>413 Request Entity Too Large</summary>
        public static readonly HttpResponseStatus RequestEntityTooLarge = NewStatus(413, "Request Entity Too Large");

        /// <summary>414 Request-URI Too Long</summary>
        public static readonly HttpResponseStatus RequestUriTooLong = NewStatus(414, "Request-URI Too Long");

        /// <summary>415 Unsupported Media Type</summary>
        public static readonly HttpResponseStatus UnsupportedMediaType = NewStatus(415, "Unsupported Media Type");

        /// <summary>416 Requested Range Not Satisfiable</summary>
        public static readonly HttpResponseStatus RequestedRangeNotSatisfiable = NewStatus(416, "Requested Range Not Satisfiable");

        /// <summary>417 Expectation Failed</summary>
        public static readonly HttpResponseStatus ExpectationFailed = NewStatus(417, "Expectation Failed");

        /// <summary>421 Misdirected Request
        /// <para><a href="https://tools.ietf.org/html/rfc7540#section-9.1.2">421 (Misdirected Request) Status Code</a></para>
        /// </summary>
        public static readonly HttpResponseStatus MisdirectedRequest = NewStatus(421, "Misdirected Request");

        /// <summary>422 Unprocessable Entity (WebDAV, RFC4918)</summary>
        public static readonly HttpResponseStatus UnprocessableEntity = NewStatus(422, "Unprocessable Entity");

        /// <summary>423 Locked (WebDAV, RFC4918)</summary>
        public static readonly HttpResponseStatus Locked = NewStatus(423, "Locked");

        /// <summary>424 Failed Dependency (WebDAV, RFC4918)</summary>
        public static readonly HttpResponseStatus FailedDependency = NewStatus(424, "Failed Dependency");

        /// <summary>425 Unordered Collection (WebDAV, RFC3648)</summary>
        public static readonly HttpResponseStatus UnorderedCollection = NewStatus(425, "Unordered Collection");

        /// <summary>426 Upgrade Required (RFC2817)</summary>
        public static readonly HttpResponseStatus UpgradeRequired = NewStatus(426, "Upgrade Required");

        /// <summary>428 Precondition Required (RFC6585)</summary>
        public static readonly HttpResponseStatus PreconditionRequired = NewStatus(428, "Precondition Required");

        /// <summary>429 Too Many Requests (RFC6585)</summary>
        public static readonly HttpResponseStatus TooManyRequests = NewStatus(429, "Too Many Requests");

        /// <summary>431 Request Header Fields Too Large (RFC6585)</summary>
        public static readonly HttpResponseStatus RequestHeaderFieldsTooLarge = NewStatus(431, "Request Header Fields Too Large");

        /// <summary>500 Internal Server Error</summary>
        public static readonly HttpResponseStatus InternalServerError = NewStatus(500, "Internal Server Error");

        /// <summary>501 Not Implemented</summary>
        public static readonly HttpResponseStatus NotImplemented = NewStatus(501, "Not Implemented");

        /// <summary>502 Bad Gateway</summary>
        public static readonly HttpResponseStatus BadGateway = NewStatus(502, "Bad Gateway");

        /// <summary>503 Service Unavailable</summary>
        public static readonly HttpResponseStatus ServiceUnavailable = NewStatus(503, "Service Unavailable");

        /// <summary>504 Gateway Timeout</summary>
        public static readonly HttpResponseStatus GatewayTimeout = NewStatus(504, "Gateway Timeout");

        /// <summary>505 HTTP Version Not Supported</summary>
        public static readonly HttpResponseStatus HttpVersionNotSupported = NewStatus(505, "HTTP Version Not Supported");

        /// <summary>506 Variant Also Negotiates (RFC2295)</summary>
        public static readonly HttpResponseStatus VariantAlsoNegotiates = NewStatus(506, "Variant Also Negotiates");

        /// <summary>507 Insufficient Storage (WebDAV, RFC4918)</summary>
        public static readonly HttpResponseStatus InsufficientStorage = NewStatus(507, "Insufficient Storage");

        /// <summary>510 Not Extended (RFC2774)</summary>
        public static readonly HttpResponseStatus NotExtended = NewStatus(510, "Not Extended");

        /// <summary>511 Network Authentication Required (RFC6585)</summary>
        public static readonly HttpResponseStatus NetworkAuthenticationRequired = NewStatus(511, "Network Authentication Required");

        static HttpResponseStatus NewStatus(int statusCode, string reasonPhrase) => new HttpResponseStatus(statusCode, new AsciiString(reasonPhrase), true);

        /// <summary>
        /// Returns the <see cref="HttpResponseStatus"/> represented by the specified code.
        /// If the specified code is a standard HTTP getStatus code, a cached instance
        /// will be returned.  Otherwise, a new instance will be returned.
        /// </summary>
        /// <param name="code">The response code value</param>
        /// <returns>the <see cref="HttpResponseStatus"/> represented by the specified <paramref name="code"/>.</returns>
        public static HttpResponseStatus ValueOf(int code) => ValueOf0(code) ?? new HttpResponseStatus(code);

        static HttpResponseStatus ValueOf0(int code)
        {
            return code switch
            {
                100 => Continue,
                101 => SwitchingProtocols,
                102 => Processing,
                200 => OK,
                201 => Created,
                202 => Accepted,
                203 => NonAuthoritativeInformation,
                204 => NoContent,
                205 => ResetContent,
                206 => PartialContent,
                207 => MultiStatus,
                300 => MultipleChoices,
                301 => MovedPermanently,
                302 => Found,
                303 => SeeOther,
                304 => NotModified,
                305 => UseProxy,
                307 => TemporaryRedirect,
                308 => PermanentRedirect,
                400 => BadRequest,
                401 => Unauthorized,
                402 => PaymentRequired,
                403 => Forbidden,
                404 => NotFound,
                405 => MethodNotAllowed,
                406 => NotAcceptable,
                407 => ProxyAuthenticationRequired,
                408 => RequestTimeout,
                409 => Conflict,
                410 => Gone,
                411 => LengthRequired,
                412 => PreconditionFailed,
                413 => RequestEntityTooLarge,
                414 => RequestUriTooLong,
                415 => UnsupportedMediaType,
                416 => RequestedRangeNotSatisfiable,
                417 => ExpectationFailed,
                421 => MisdirectedRequest,
                422 => UnprocessableEntity,
                423 => Locked,
                424 => FailedDependency,
                425 => UnorderedCollection,
                426 => UpgradeRequired,
                428 => PreconditionRequired,
                429 => TooManyRequests,
                431 => RequestHeaderFieldsTooLarge,
                500 => InternalServerError,
                501 => NotImplemented,
                502 => BadGateway,
                503 => ServiceUnavailable,
                504 => GatewayTimeout,
                505 => HttpVersionNotSupported,
                506 => VariantAlsoNegotiates,
                507 => InsufficientStorage,
                510 => NotExtended,
                511 => NetworkAuthenticationRequired,
                _ => null,
            };
        }

        /// <summary>
        /// Returns the <see cref="HttpResponseStatus"/> represented by the specified <paramref name="code"/> and <paramref name="reasonPhrase"/>.
        /// If the specified code is a standard HTTP status <paramref name="code"/> and <paramref name="reasonPhrase"/>, a cached instance
        /// will be returned. Otherwise, a new instance will be returned.
        /// </summary>
        /// <param name="code">The response code value.</param>
        /// <param name="reasonPhrase">The response code reason phrase.</param>
        /// <returns>the <see cref="HttpResponseStatus"/> represented by the specified <paramref name="code"/> and <paramref name="reasonPhrase"/>.</returns>
        public static HttpResponseStatus ValueOf(int code, AsciiString reasonPhrase)
        {
            HttpResponseStatus responseStatus = ValueOf0(code);
            return responseStatus is object && responseStatus.ReasonPhrase.ContentEquals(reasonPhrase)
                ? responseStatus
                : new HttpResponseStatus(code, reasonPhrase);
        }

        /// <summary>
        /// Parses the specified HTTP status line into a <see cref="HttpResponseStatus"/>. The expected formats of the line are:
        /// <para><see cref="Code"/> (e.g. 200)</para>
        /// <para><see cref="Code"/> <see cref="ReasonPhrase"/> (e.g. 404 Not Found)</para>
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static HttpResponseStatus ParseLine(ICharSequence line) => line is AsciiString asciiString ? ParseLine(asciiString) : ParseLine(line.ToString());

        /// <summary>
        /// Parses the specified HTTP status line into a <see cref="HttpResponseStatus"/>. The expected formats of the line are:
        /// <para><see cref="Code"/> (e.g. 200)</para>
        /// <para><see cref="Code"/> <see cref="ReasonPhrase"/> (e.g. 404 Not Found)</para>
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static HttpResponseStatus ParseLine(string line)
        {
            try
            {
                int space = line.IndexOf(' ');
                return space == -1
                    ? ValueOf(int.Parse(line))
                    : ValueOf(int.Parse(line.Substring(0, space)), new AsciiString(line.Substring(space + 1)));
            }
            catch (Exception e)
            {
                return ThrowHelper.FromArgumentException_ParseLine(line, e);
            }
        }

        /// <summary>
        /// Parses the specified HTTP status line into a <see cref="HttpResponseStatus"/>. The expected formats of the line are:
        /// <para><see cref="Code"/> (e.g. 200)</para>
        /// <para><see cref="Code"/> <see cref="ReasonPhrase"/> (e.g. 404 Not Found)</para>
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static HttpResponseStatus ParseLine(AsciiString line)
        {
            try
            {
                //int space = line.ForEachByte(ByteProcessor.FindAsciiSpace);
                const char Space = ' ';
                int space = line.IndexOf(Space, 0);
                return space == -1
                    ? ValueOf(line.ParseInt())
                    : ValueOf(line.ParseInt(0, space), (AsciiString)line.SubSequence(space + 1));
            }
            catch (Exception e)
            {
                return ThrowHelper.FromArgumentException_ParseLine(line, e);
            }
        }


        readonly int code;
        readonly AsciiString codeAsText;
        readonly HttpStatusClass codeClass;

        readonly AsciiString reasonPhrase;
        readonly byte[] bytes;

        /// <summary>
        /// Creates a new instance with the specified <paramref name="code"/> and the auto-generated default reason phrase.
        /// </summary>
        /// <param name="code"></param>
        HttpResponseStatus(int code)
            : this(code, new AsciiString($"{HttpStatusClass.ValueOf(code).DefaultReasonPhrase} ({code})"), false)
        {
        }

        /// <summary>
        /// Creates a new instance with the specified <paramref name="code"/> and its <paramref name="reasonPhrase"/>.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reasonPhrase"></param>
        public HttpResponseStatus(int code, AsciiString reasonPhrase)
            : this(code, reasonPhrase, false)
        {
        }

        private HttpResponseStatus(int code, AsciiString reasonPhrase, bool bytes)
        {
            if ((uint)code > SharedConstants.TooBigOrNegative) // < 0
            {
                ThrowHelper.ThrowArgumentException_InvalidResponseCode(code);
            }
            if (reasonPhrase is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.reasonPhrase);
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < reasonPhrase.Count; i++)
            {
                char c = reasonPhrase[i];
                // Check prohibited characters.
                switch (c)
                {
                    case '\n':
                    case '\r':
                        ThrowHelper.ThrowArgumentException_ReasonPhrase(reasonPhrase); break;
                }
            }

            this.code = code;
            this.codeAsText = new AsciiString(code.ToString(System.Globalization.CultureInfo.InvariantCulture));
            this.reasonPhrase = reasonPhrase;
            this.bytes = bytes ? Encoding.ASCII.GetBytes($"{code} {reasonPhrase}") : null;
            this.codeClass = HttpStatusClass.ValueOf(code);
        }

        /// <summary>
        /// Returns the code of this <see cref="HttpResponseStatus"/>.
        /// </summary>
        public int Code => this.code;

        /// <summary>
        /// Returns the status code as <see cref="AsciiString"/>.
        /// </summary>
        public AsciiString CodeAsText => this.codeAsText;

        /// <summary>
        /// Returns the reason phrase of this <see cref="HttpResponseStatus"/>.
        /// </summary>
        public AsciiString ReasonPhrase => this.reasonPhrase;

        /// <summary>
        /// Returns the class of this <see cref="HttpResponseStatus"/>
        /// </summary>
        public HttpStatusClass CodeClass => this.codeClass;

        /// <inheritdoc />
        public override int GetHashCode() => this.code;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) { return true; }
            return obj is HttpResponseStatus other && this.code == other.code;
        }

        /// <summary>
        /// Equality of <see cref="HttpResponseStatus"/> only depends on <see cref="Code"/>.
        /// The reason phrase is not considered for equality.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(HttpResponseStatus other)
        {
            if (ReferenceEquals(this, other)) { return true; }
            return other is object && this.code == other.code;
        }

        public int CompareTo(object obj) => CompareTo(obj as HttpResponseStatus);

        public int CompareTo(HttpResponseStatus other)
        {
            if (ReferenceEquals(this, other)) { return 0; }
            if (other is null) { return 1; }
            return this.code - other.code;
        }

        /// <inheritdoc />
        public override string ToString() =>
            StringBuilderManager.ReturnAndFree(StringBuilderManager.Allocate(this.ReasonPhrase.Count + 4)
            .Append(this.codeAsText)
            .Append(' ')
            .Append(this.ReasonPhrase));

        internal void Encode(IByteBuffer buf)
        {
            if (this.bytes is null)
            {
                ByteBufferUtil.Copy(this.codeAsText, buf);
                _ = buf.WriteByte(HttpConstants.HorizontalSpace);
                _ = buf.WriteCharSequence(this.reasonPhrase, Encoding.ASCII);
            }
            else
            {
                _ = buf.WriteBytes(this.bytes);
            }
        }
    }
}
