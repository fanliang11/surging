/*
 * HttpListenerResponse.cs
 *
 * This code is derived from HttpListenerResponse.cs (System.Net) of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2012-2015 sta.blockhead
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

/*
 * Contributors:
 * - Nicholas Devenish
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Provides the access to a response to a request received by the <see cref="HttpListener"/>.
    /// </summary>
    public sealed class HttpListenerResponse : IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _closeConnection
        /// </summary>
        private bool _closeConnection;

        /// <summary>
        /// Defines the _contentEncoding
        /// </summary>
        private Encoding _contentEncoding;

        /// <summary>
        /// Defines the _contentLength
        /// </summary>
        private long _contentLength;

        /// <summary>
        /// Defines the _contentType
        /// </summary>
        private string _contentType;

        /// <summary>
        /// Defines the _context
        /// </summary>
        private HttpListenerContext _context;

        /// <summary>
        /// Defines the _cookies
        /// </summary>
        private CookieCollection _cookies;

        /// <summary>
        /// Defines the _disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Defines the _headers
        /// </summary>
        private WebHeaderCollection _headers;

        /// <summary>
        /// Defines the _headersSent
        /// </summary>
        private bool _headersSent;

        /// <summary>
        /// Defines the _keepAlive
        /// </summary>
        private bool _keepAlive;

        /// <summary>
        /// Defines the _location
        /// </summary>
        private string _location;

        /// <summary>
        /// Defines the _outputStream
        /// </summary>
        private ResponseStream _outputStream;

        /// <summary>
        /// Defines the _sendChunked
        /// </summary>
        private bool _sendChunked;

        /// <summary>
        /// Defines the _statusCode
        /// </summary>
        private int _statusCode;

        /// <summary>
        /// Defines the _statusDescription
        /// </summary>
        private string _statusDescription;

        /// <summary>
        /// Defines the _version
        /// </summary>
        private Version _version;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerResponse"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="HttpListenerContext"/></param>
        internal HttpListenerResponse(HttpListenerContext context)
        {
            _context = context;
            _keepAlive = true;
            _statusCode = 200;
            _statusDescription = "OK";
            _version = HttpVersion.Version11;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the encoding for the entity body data included in the response.
        /// </summary>
        public Encoding ContentEncoding
        {
            get
            {
                return _contentEncoding;
            }

            set
            {
                checkDisposed();
                _contentEncoding = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of bytes in the entity body data included in the response.
        /// </summary>
        public long ContentLength64
        {
            get
            {
                return _contentLength;
            }

            set
            {
                checkDisposedOrHeadersSent();
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Less than zero.", "value");

                _contentLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the media type of the entity body included in the response.
        /// </summary>
        public string ContentType
        {
            get
            {
                return _contentType;
            }

            set
            {
                checkDisposed();
                if (value != null && value.Length == 0)
                    throw new ArgumentException("An empty string.", "value");

                _contentType = value;
            }
        }

        /// <summary>
        /// Gets or sets the cookies sent with the response.
        /// </summary>
        public CookieCollection Cookies
        {
            get
            {
                return _cookies ?? (_cookies = new CookieCollection());
            }

            set
            {
                _cookies = value;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP headers sent to the client.
        /// </summary>
        public WebHeaderCollection Headers
        {
            get
            {
                return _headers ?? (_headers = new WebHeaderCollection(HttpHeaderType.Response, false));
            }

            set
            {
                if (value != null && value.State != HttpHeaderType.Response)
                    throw new InvalidOperationException(
                      "The specified headers aren't valid for a response.");

                _headers = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server requests a persistent connection.
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                return _keepAlive;
            }

            set
            {
                checkDisposedOrHeadersSent();
                _keepAlive = value;
            }
        }

        /// <summary>
        /// Gets the OutputStream
        /// Gets a <see cref="Stream"/> to use to write the entity body data.
        /// </summary>
        public Stream OutputStream
        {
            get
            {
                checkDisposed();
                return _outputStream ?? (_outputStream = _context.Connection.GetResponseStream());
            }
        }

        /// <summary>
        /// Gets or sets the HTTP version used in the response.
        /// </summary>
        public Version ProtocolVersion
        {
            get
            {
                return _version;
            }

            set
            {
                checkDisposedOrHeadersSent();
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Major != 1 || (value.Minor != 0 && value.Minor != 1))
                    throw new ArgumentException("Not 1.0 or 1.1.", "value");

                _version = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL to which the client is redirected to locate a requested resource.
        /// </summary>
        public string RedirectLocation
        {
            get
            {
                return _location;
            }

            set
            {
                checkDisposed();
                if (value == null)
                {
                    _location = null;
                    return;
                }

                Uri uri = null;
                if (!value.MaybeUri() || !Uri.TryCreate(value, UriKind.Absolute, out uri))
                    throw new ArgumentException("Not an absolute URL.", "value");

                _location = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the response uses the chunked transfer encoding.
        /// </summary>
        public bool SendChunked
        {
            get
            {
                return _sendChunked;
            }

            set
            {
                checkDisposedOrHeadersSent();
                _sendChunked = value;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP status code returned to the client.
        /// </summary>
        public int StatusCode
        {
            get
            {
                return _statusCode;
            }

            set
            {
                checkDisposedOrHeadersSent();
                if (value < 100 || value > 999)
                    throw new System.Net.ProtocolViolationException(
                      "A value isn't between 100 and 999 inclusive.");

                _statusCode = value;
                _statusDescription = value.GetStatusDescription();
            }
        }

        /// <summary>
        /// Gets or sets the description of the HTTP status code returned to the client.
        /// </summary>
        public string StatusDescription
        {
            get
            {
                return _statusDescription;
            }

            set
            {
                checkDisposedOrHeadersSent();
                if (value == null || value.Length == 0)
                {
                    _statusDescription = _statusCode.GetStatusDescription();
                    return;
                }

                if (!value.IsText() || value.IndexOfAny(new[] { '\r', '\n' }) > -1)
                    throw new ArgumentException("Contains invalid characters.", "value");

                _statusDescription = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether CloseConnection
        /// </summary>
        internal bool CloseConnection
        {
            get
            {
                return _closeConnection;
            }

            set
            {
                _closeConnection = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether HeadersSent
        /// </summary>
        internal bool HeadersSent
        {
            get
            {
                return _headersSent;
            }

            set
            {
                _headersSent = value;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Closes the connection to the client without returning a response.
        /// </summary>
        public void Abort()
        {
            if (_disposed)
                return;

            close(true);
        }

        /// <summary>
        /// Adds an HTTP header with the specified <paramref name="name"/> and
        /// <paramref name="value"/> to the headers for the response.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        public void AddHeader(string name, string value)
        {
            Headers.Set(name, value);
        }

        /// <summary>
        /// Appends the specified <paramref name="cookie"/> to the cookies sent with the response.
        /// </summary>
        /// <param name="cookie">The cookie<see cref="Cookie"/></param>
        public void AppendCookie(Cookie cookie)
        {
            Cookies.Add(cookie);
        }

        /// <summary>
        /// Appends a <paramref name="value"/> to the specified HTTP header sent with the response.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        public void AppendHeader(string name, string value)
        {
            Headers.Add(name, value);
        }

        /// <summary>
        /// Returns the response to the client and releases the resources used by
        /// this <see cref="HttpListenerResponse"/> instance.
        /// </summary>
        public void Close()
        {
            if (_disposed)
                return;

            close(false);
        }

        /// <summary>
        /// Returns the response with the specified array of <see cref="byte"/> to the client and
        /// releases the resources used by this <see cref="HttpListenerResponse"/> instance.
        /// </summary>
        /// <param name="responseEntity">The responseEntity<see cref="byte[]"/></param>
        /// <param name="willBlock">The willBlock<see cref="bool"/></param>
        public void Close(byte[] responseEntity, bool willBlock)
        {
            checkDisposed();
            if (responseEntity == null)
                throw new ArgumentNullException("responseEntity");

            var len = responseEntity.Length;
            var output = OutputStream;
            if (willBlock)
            {
                output.Write(responseEntity, 0, len);
                close(false);

                return;
            }

            output.BeginWrite(
              responseEntity,
              0,
              len,
              ar =>
              {
                  output.EndWrite(ar);
                  close(false);
              },
              null);
        }

        /// <summary>
        /// Copies some properties from the specified <see cref="HttpListenerResponse"/> to
        /// this response.
        /// </summary>
        /// <param name="templateResponse">The templateResponse<see cref="HttpListenerResponse"/></param>
        public void CopyFrom(HttpListenerResponse templateResponse)
        {
            if (templateResponse == null)
                throw new ArgumentNullException("templateResponse");

            if (templateResponse._headers != null)
            {
                if (_headers != null)
                    _headers.Clear();

                Headers.Add(templateResponse._headers);
            }
            else if (_headers != null)
            {
                _headers = null;
            }

            _contentLength = templateResponse._contentLength;
            _statusCode = templateResponse._statusCode;
            _statusDescription = templateResponse._statusDescription;
            _keepAlive = templateResponse._keepAlive;
            _version = templateResponse._version;
        }

        /// <summary>
        /// Configures the response to redirect the client's request to
        /// the specified <paramref name="url"/>.
        /// </summary>
        /// <param name="url">The url<see cref="string"/></param>
        public void Redirect(string url)
        {
            checkDisposedOrHeadersSent();
            if (url == null)
                throw new ArgumentNullException("url");

            Uri uri = null;
            if (!url.MaybeUri() || !Uri.TryCreate(url, UriKind.Absolute, out uri))
                throw new ArgumentException("Not an absolute URL.", "url");

            _location = url;
            _statusCode = 302;
            _statusDescription = "Found";
        }

        /// <summary>
        /// Adds or updates a <paramref name="cookie"/> in the cookies sent with the response.
        /// </summary>
        /// <param name="cookie">The cookie<see cref="Cookie"/></param>
        public void SetCookie(Cookie cookie)
        {
            if (cookie == null)
                throw new ArgumentNullException("cookie");

            if (!canAddOrUpdate(cookie))
                throw new ArgumentException("Cannot be replaced.", "cookie");

            Cookies.Add(cookie);
        }

        /// <summary>
        /// The WriteHeadersTo
        /// </summary>
        /// <param name="destination">The destination<see cref="MemoryStream"/></param>
        /// <returns>The <see cref="WebHeaderCollection"/></returns>
        internal WebHeaderCollection WriteHeadersTo(MemoryStream destination)
        {
            var headers = new WebHeaderCollection(HttpHeaderType.Response, true);
            if (_headers != null)
                headers.Add(_headers);

            if (_contentType != null)
            {
                var type = _contentType.IndexOf("charset=", StringComparison.Ordinal) == -1 &&
                           _contentEncoding != null
                           ? String.Format("{0}; charset={1}", _contentType, _contentEncoding.WebName)
                           : _contentType;

                headers.InternalSet("Content-Type", type, true);
            }

            if (headers["Server"] == null)
                headers.InternalSet("Server", "websocket-sharp/1.0", true);

            var prov = CultureInfo.InvariantCulture;
            if (headers["Date"] == null)
                headers.InternalSet("Date", DateTime.UtcNow.ToString("r", prov), true);

            if (!_sendChunked)
                headers.InternalSet("Content-Length", _contentLength.ToString(prov), true);
            else
                headers.InternalSet("Transfer-Encoding", "chunked", true);

            /*
             * Apache forces closing the connection for these status codes:
             * - 400 Bad Request
             * - 408 Request Timeout
             * - 411 Length Required
             * - 413 Request Entity Too Large
             * - 414 Request-Uri Too Long
             * - 500 Internal Server Error
             * - 503 Service Unavailable
             */
            var closeConn = !_context.Request.KeepAlive ||
                            !_keepAlive ||
                            _statusCode == 400 ||
                            _statusCode == 408 ||
                            _statusCode == 411 ||
                            _statusCode == 413 ||
                            _statusCode == 414 ||
                            _statusCode == 500 ||
                            _statusCode == 503;

            var reuses = _context.Connection.Reuses;
            if (closeConn || reuses >= 100)
            {
                headers.InternalSet("Connection", "close", true);
            }
            else
            {
                headers.InternalSet(
                  "Keep-Alive", String.Format("timeout=15,max={0}", 100 - reuses), true);

                if (_context.Request.ProtocolVersion < HttpVersion.Version11)
                    headers.InternalSet("Connection", "keep-alive", true);
            }

            if (_location != null)
                headers.InternalSet("Location", _location, true);

            if (_cookies != null)
                foreach (Cookie cookie in _cookies)
                    headers.InternalSet("Set-Cookie", cookie.ToResponseString(), true);

            var enc = _contentEncoding ?? Encoding.Default;
            var writer = new StreamWriter(destination, enc, 256);
            writer.Write("HTTP/{0} {1} {2}\r\n", _version, _statusCode, _statusDescription);
            writer.Write(headers.ToStringMultiValue(true));
            writer.Flush();

            // Assumes that the destination was at position 0.
            destination.Position = enc.GetPreamble().Length;

            return headers;
        }

        /// <summary>
        /// The canAddOrUpdate
        /// </summary>
        /// <param name="cookie">The cookie<see cref="Cookie"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool canAddOrUpdate(Cookie cookie)
        {
            if (_cookies == null || _cookies.Count == 0)
                return true;

            var found = findCookie(cookie).ToList();
            if (found.Count == 0)
                return true;

            var ver = cookie.Version;
            foreach (var c in found)
                if (c.Version == ver)
                    return true;

            return false;
        }

        /// <summary>
        /// The checkDisposed
        /// </summary>
        private void checkDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().ToString());
        }

        /// <summary>
        /// The checkDisposedOrHeadersSent
        /// </summary>
        private void checkDisposedOrHeadersSent()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().ToString());

            if (_headersSent)
                throw new InvalidOperationException("Cannot be changed after the headers are sent.");
        }

        /// <summary>
        /// The close
        /// </summary>
        /// <param name="force">The force<see cref="bool"/></param>
        private void close(bool force)
        {
            _disposed = true;
            _context.Connection.Close(force);
        }

        /// <summary>
        /// The findCookie
        /// </summary>
        /// <param name="cookie">The cookie<see cref="Cookie"/></param>
        /// <returns>The <see cref="IEnumerable{Cookie}"/></returns>
        private IEnumerable<Cookie> findCookie(Cookie cookie)
        {
            var name = cookie.Name;
            var domain = cookie.Domain;
            var path = cookie.Path;
            if (_cookies != null)
                foreach (Cookie c in _cookies)
                    if (c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                        c.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase) &&
                        c.Path.Equals(path, StringComparison.Ordinal))
                        yield return c;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="HttpListenerResponse"/>.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (_disposed)
                return;

            close(true); // Same as the Abort method.
        }

        #endregion 方法
    }
}