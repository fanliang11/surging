/*
 * HttpListenerRequest.cs
 *
 * This code is derived from HttpListenerRequest.cs (System.Net) of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2012-2018 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

/*
 * Authors:
 * - Gonzalo Paniagua Javier <gonzalo@novell.com>
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Represents an incoming request to a <see cref="HttpListener"/> instance.
    /// </summary>
    public sealed class HttpListenerRequest
    {
        #region 字段

        /// <summary>
        /// Defines the _100continue
        /// </summary>
        private static readonly byte[] _100continue;

        /// <summary>
        /// Defines the _acceptTypes
        /// </summary>
        private string[] _acceptTypes;

        /// <summary>
        /// Defines the _chunked
        /// </summary>
        private bool _chunked;

        /// <summary>
        /// Defines the _connection
        /// </summary>
        private HttpConnection _connection;

        /// <summary>
        /// Defines the _contentEncoding
        /// </summary>
        private Encoding _contentEncoding;

        /// <summary>
        /// Defines the _contentLength
        /// </summary>
        private long _contentLength;

        /// <summary>
        /// Defines the _context
        /// </summary>
        private HttpListenerContext _context;

        /// <summary>
        /// Defines the _cookies
        /// </summary>
        private CookieCollection _cookies;

        /// <summary>
        /// Defines the _headers
        /// </summary>
        private WebHeaderCollection _headers;

        /// <summary>
        /// Defines the _httpMethod
        /// </summary>
        private string _httpMethod;

        /// <summary>
        /// Defines the _inputStream
        /// </summary>
        private Stream _inputStream;

        /// <summary>
        /// Defines the _protocolVersion
        /// </summary>
        private Version _protocolVersion;

        /// <summary>
        /// Defines the _queryString
        /// </summary>
        private NameValueCollection _queryString;

        /// <summary>
        /// Defines the _rawUrl
        /// </summary>
        private string _rawUrl;

        /// <summary>
        /// Defines the _requestTraceIdentifier
        /// </summary>
        private Guid _requestTraceIdentifier;

        /// <summary>
        /// Defines the _url
        /// </summary>
        private Uri _url;

        /// <summary>
        /// Defines the _urlReferrer
        /// </summary>
        private Uri _urlReferrer;

        /// <summary>
        /// Defines the _urlSet
        /// </summary>
        private bool _urlSet;

        /// <summary>
        /// Defines the _userHostName
        /// </summary>
        private string _userHostName;

        /// <summary>
        /// Defines the _userLanguages
        /// </summary>
        private string[] _userLanguages;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerRequest"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="HttpListenerContext"/></param>
        internal HttpListenerRequest(HttpListenerContext context)
        {
            _context = context;

            _connection = context.Connection;
            _contentLength = -1;
            _headers = new WebHeaderCollection();
            _requestTraceIdentifier = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes static members of the <see cref="HttpListenerRequest"/> class.
        /// </summary>
        static HttpListenerRequest()
        {
            _100continue = Encoding.ASCII.GetBytes("HTTP/1.1 100 Continue\r\n\r\n");
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the media types that are acceptable for the client.
        /// </summary>
        public string[] AcceptTypes
        {
            get
            {
                var val = _headers["Accept"];
                if (val == null)
                    return null;

                if (_acceptTypes == null)
                {
                    _acceptTypes = val
                                   .SplitHeaderValue(',')
                                   .Trim()
                                   .ToList()
                                   .ToArray();
                }

                return _acceptTypes;
            }
        }

        /// <summary>
        /// Gets the ClientCertificateError
        /// Gets an error code that identifies a problem with the certificate
        /// provided by the client.
        /// </summary>
        public int ClientCertificateError
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets the encoding for the entity body data included in the request.
        /// </summary>
        public Encoding ContentEncoding
        {
            get
            {
                if (_contentEncoding == null)
                    _contentEncoding = getContentEncoding() ?? Encoding.UTF8;

                return _contentEncoding;
            }
        }

        /// <summary>
        /// Gets the length in bytes of the entity body data included in the
        /// request.
        /// </summary>
        public long ContentLength64
        {
            get
            {
                return _contentLength;
            }
        }

        /// <summary>
        /// Gets the media type of the entity body data included in the request.
        /// </summary>
        public string ContentType
        {
            get
            {
                return _headers["Content-Type"];
            }
        }

        /// <summary>
        /// Gets the cookies included in the request.
        /// </summary>
        public CookieCollection Cookies
        {
            get
            {
                if (_cookies == null)
                    _cookies = _headers.GetCookies(false);

                return _cookies;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the request has the entity body data.
        /// </summary>
        public bool HasEntityBody
        {
            get
            {
                return _contentLength > 0 || _chunked;
            }
        }

        /// <summary>
        /// Gets the headers included in the request.
        /// </summary>
        public NameValueCollection Headers
        {
            get
            {
                return _headers;
            }
        }

        /// <summary>
        /// Gets the HTTP method specified by the client.
        /// </summary>
        public string HttpMethod
        {
            get
            {
                return _httpMethod;
            }
        }

        /// <summary>
        /// Gets the InputStream
        /// Gets a stream that contains the entity body data included in
        /// the request.
        /// </summary>
        public Stream InputStream
        {
            get
            {
                if (_inputStream == null)
                    _inputStream = getInputStream() ?? Stream.Null;

                return _inputStream;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the client is authenticated.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return _context.User != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the request is sent from the local
        /// computer.
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return _connection.IsLocal;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a secure connection is used to send
        /// the request.
        /// </summary>
        public bool IsSecureConnection
        {
            get
            {
                return _connection.IsSecure;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the request is a WebSocket handshake
        /// request.
        /// </summary>
        public bool IsWebSocketRequest
        {
            get
            {
                return _httpMethod == "GET"
                       && _protocolVersion > HttpVersion.Version10
                       && _headers.Upgrades("websocket");
            }
        }

        /// <summary>
        /// Gets a value indicating whether a persistent connection is requested.
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                return _headers.KeepsAlive(_protocolVersion);
            }
        }

        /// <summary>
        /// Gets the endpoint to which the request is sent.
        /// </summary>
        public System.Net.IPEndPoint LocalEndPoint
        {
            get
            {
                return _connection.LocalEndPoint;
            }
        }

        /// <summary>
        /// Gets the HTTP version specified by the client.
        /// </summary>
        public Version ProtocolVersion
        {
            get
            {
                return _protocolVersion;
            }
        }

        /// <summary>
        /// Gets the query string included in the request.
        /// </summary>
        public NameValueCollection QueryString
        {
            get
            {
                if (_queryString == null)
                {
                    var url = Url;
                    _queryString = HttpUtility.InternalParseQueryString(
                                     url != null ? url.Query : null,
                                     Encoding.UTF8
                                   );
                }

                return _queryString;
            }
        }

        /// <summary>
        /// Gets the raw URL specified by the client.
        /// </summary>
        public string RawUrl
        {
            get
            {
                return _rawUrl;
            }
        }

        /// <summary>
        /// Gets the endpoint from which the request is sent.
        /// </summary>
        public System.Net.IPEndPoint RemoteEndPoint
        {
            get
            {
                return _connection.RemoteEndPoint;
            }
        }

        /// <summary>
        /// Gets the trace identifier of the request.
        /// </summary>
        public Guid RequestTraceIdentifier
        {
            get
            {
                return _requestTraceIdentifier;
            }
        }

        /// <summary>
        /// Gets the URL requested by the client.
        /// </summary>
        public Uri Url
        {
            get
            {
                if (!_urlSet)
                {
                    _url = HttpUtility.CreateRequestUrl(
                             _rawUrl,
                             _userHostName ?? UserHostAddress,
                             IsWebSocketRequest,
                             IsSecureConnection
                           );

                    _urlSet = true;
                }

                return _url;
            }
        }

        /// <summary>
        /// Gets the URI of the resource from which the requested URL was obtained.
        /// </summary>
        public Uri UrlReferrer
        {
            get
            {
                var val = _headers["Referer"];
                if (val == null)
                    return null;

                if (_urlReferrer == null)
                    _urlReferrer = val.ToUri();

                return _urlReferrer;
            }
        }

        /// <summary>
        /// Gets the user agent from which the request is originated.
        /// </summary>
        public string UserAgent
        {
            get
            {
                return _headers["User-Agent"];
            }
        }

        /// <summary>
        /// Gets the IP address and port number to which the request is sent.
        /// </summary>
        public string UserHostAddress
        {
            get
            {
                return _connection.LocalEndPoint.ToString();
            }
        }

        /// <summary>
        /// Gets the server host name requested by the client.
        /// </summary>
        public string UserHostName
        {
            get
            {
                return _userHostName;
            }
        }

        /// <summary>
        /// Gets the natural languages that are acceptable for the client.
        /// </summary>
        public string[] UserLanguages
        {
            get
            {
                var val = _headers["Accept-Language"];
                if (val == null)
                    return null;

                if (_userLanguages == null)
                    _userLanguages = val.Split(',').Trim().ToList().ToArray();

                return _userLanguages;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Begins getting the certificate provided by the client asynchronously.
        /// </summary>
        /// <param name="requestCallback">The requestCallback<see cref="AsyncCallback"/></param>
        /// <param name="state">The state<see cref="object"/></param>
        /// <returns>The <see cref="IAsyncResult"/></returns>
        public IAsyncResult BeginGetClientCertificate(
      AsyncCallback requestCallback, object state
    )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Ends an asynchronous operation to get the certificate provided by the
        /// client.
        /// </summary>
        /// <param name="asyncResult">The asyncResult<see cref="IAsyncResult"/></param>
        /// <returns>The <see cref="X509Certificate2"/></returns>
        public X509Certificate2 EndGetClientCertificate(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the certificate provided by the client.
        /// </summary>
        /// <returns>The <see cref="X509Certificate2"/></returns>
        public X509Certificate2 GetClientCertificate()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns a string that represents the current instance.
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            var buff = new StringBuilder(64);

            buff
            .AppendFormat(
              "{0} {1} HTTP/{2}\r\n", _httpMethod, _rawUrl, _protocolVersion
            )
            .Append(_headers.ToString());

            return buff.ToString();
        }

        /// <summary>
        /// The AddHeader
        /// </summary>
        /// <param name="headerField">The headerField<see cref="string"/></param>
        internal void AddHeader(string headerField)
        {
            var start = headerField[0];
            if (start == ' ' || start == '\t')
            {
                _context.ErrorMessage = "Invalid header field";
                return;
            }

            var colon = headerField.IndexOf(':');
            if (colon < 1)
            {
                _context.ErrorMessage = "Invalid header field";
                return;
            }

            var name = headerField.Substring(0, colon).Trim();
            if (name.Length == 0 || !name.IsToken())
            {
                _context.ErrorMessage = "Invalid header name";
                return;
            }

            var val = colon < headerField.Length - 1
                      ? headerField.Substring(colon + 1).Trim()
                      : String.Empty;

            _headers.InternalSet(name, val, false);

            var lower = name.ToLower(CultureInfo.InvariantCulture);
            if (lower == "host")
            {
                if (_userHostName != null)
                {
                    _context.ErrorMessage = "Invalid Host header";
                    return;
                }

                if (val.Length == 0)
                {
                    _context.ErrorMessage = "Invalid Host header";
                    return;
                }

                _userHostName = val;
                return;
            }

            if (lower == "content-length")
            {
                if (_contentLength > -1)
                {
                    _context.ErrorMessage = "Invalid Content-Length header";
                    return;
                }

                long len;
                if (!Int64.TryParse(val, out len))
                {
                    _context.ErrorMessage = "Invalid Content-Length header";
                    return;
                }

                if (len < 0)
                {
                    _context.ErrorMessage = "Invalid Content-Length header";
                    return;
                }

                _contentLength = len;
                return;
            }
        }

        /// <summary>
        /// The FinishInitialization
        /// </summary>
        internal void FinishInitialization()
        {
            if (_protocolVersion == HttpVersion.Version10)
            {
                finishInitialization10();
                return;
            }

            if (_userHostName == null)
            {
                _context.ErrorMessage = "Host header required";
                return;
            }

            var transferEnc = _headers["Transfer-Encoding"];
            if (transferEnc != null)
            {
                var comparison = StringComparison.OrdinalIgnoreCase;
                if (!transferEnc.Equals("chunked", comparison))
                {
                    _context.ErrorMessage = String.Empty;
                    _context.ErrorStatus = 501;

                    return;
                }

                _chunked = true;
            }

            if (_httpMethod == "POST" || _httpMethod == "PUT")
            {
                if (_contentLength <= 0 && !_chunked)
                {
                    _context.ErrorMessage = String.Empty;
                    _context.ErrorStatus = 411;

                    return;
                }
            }

            var expect = _headers["Expect"];
            if (expect != null)
            {
                var comparison = StringComparison.OrdinalIgnoreCase;
                if (!expect.Equals("100-continue", comparison))
                {
                    _context.ErrorMessage = "Invalid Expect header";
                    return;
                }

                var output = _connection.GetResponseStream();
                output.InternalWrite(_100continue, 0, _100continue.Length);
            }
        }

        /// <summary>
        /// The FlushInput
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        internal bool FlushInput()
        {
            var input = InputStream;
            if (input == Stream.Null)
                return true;

            var len = 2048;
            if (_contentLength > 0 && _contentLength < len)
                len = (int)_contentLength;

            var buff = new byte[len];

            while (true)
            {
                try
                {
                    var ares = input.BeginRead(buff, 0, len, null, null);
                    if (!ares.IsCompleted)
                    {
                        var timeout = 100;
                        if (!ares.AsyncWaitHandle.WaitOne(timeout))
                            return false;
                    }

                    if (input.EndRead(ares) <= 0)
                        return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// The IsUpgradeRequest
        /// </summary>
        /// <param name="protocol">The protocol<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal bool IsUpgradeRequest(string protocol)
        {
            return _headers.Upgrades(protocol);
        }

        /// <summary>
        /// The SetRequestLine
        /// </summary>
        /// <param name="requestLine">The requestLine<see cref="string"/></param>
        internal void SetRequestLine(string requestLine)
        {
            var parts = requestLine.Split(new[] { ' ' }, 3);
            if (parts.Length < 3)
            {
                _context.ErrorMessage = "Invalid request line (parts)";
                return;
            }

            var method = parts[0];
            if (method.Length == 0)
            {
                _context.ErrorMessage = "Invalid request line (method)";
                return;
            }

            var target = parts[1];
            if (target.Length == 0)
            {
                _context.ErrorMessage = "Invalid request line (target)";
                return;
            }

            var rawVer = parts[2];
            if (rawVer.Length != 8)
            {
                _context.ErrorMessage = "Invalid request line (version)";
                return;
            }

            if (rawVer.IndexOf("HTTP/") != 0)
            {
                _context.ErrorMessage = "Invalid request line (version)";
                return;
            }

            Version ver;
            if (!rawVer.Substring(5).TryCreateVersion(out ver))
            {
                _context.ErrorMessage = "Invalid request line (version)";
                return;
            }

            if (ver.Major < 1)
            {
                _context.ErrorMessage = "Invalid request line (version)";
                return;
            }

            if (!method.IsHttpMethod(ver))
            {
                _context.ErrorMessage = "Invalid request line (method)";
                return;
            }

            _httpMethod = method;
            _rawUrl = target;
            _protocolVersion = ver;
        }

        /// <summary>
        /// The finishInitialization10
        /// </summary>
        private void finishInitialization10()
        {
            var transferEnc = _headers["Transfer-Encoding"];
            if (transferEnc != null)
            {
                _context.ErrorMessage = "Invalid Transfer-Encoding header";
                return;
            }

            if (_httpMethod == "POST")
            {
                if (_contentLength == -1)
                {
                    _context.ErrorMessage = "Content-Length header required";
                    return;
                }

                if (_contentLength == 0)
                {
                    _context.ErrorMessage = "Invalid Content-Length header";
                    return;
                }
            }
        }

        /// <summary>
        /// The getContentEncoding
        /// </summary>
        /// <returns>The <see cref="Encoding"/></returns>
        private Encoding getContentEncoding()
        {
            var val = _headers["Content-Type"];
            if (val == null)
                return null;

            Encoding ret;
            HttpUtility.TryGetEncoding(val, out ret);

            return ret;
        }

        /// <summary>
        /// The getInputStream
        /// </summary>
        /// <returns>The <see cref="RequestStream"/></returns>
        private RequestStream getInputStream()
        {
            return _contentLength > 0 || _chunked
                   ? _connection.GetRequestStream(_contentLength, _chunked)
                   : null;
        }

        #endregion 方法
    }
}