/*
 * HttpResponse.cs
 *
 * The MIT License
 *
 * Copyright (c) 2012-2014 sta.blockhead
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

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using WebSocketCore.Net;

namespace WebSocketCore
{
    /// <summary>
    /// Defines the <see cref="HttpResponse" />
    /// </summary>
    internal class HttpResponse : HttpBase
    {
        #region 字段

        /// <summary>
        /// Defines the _code
        /// </summary>
        private string _code;

        /// <summary>
        /// Defines the _reason
        /// </summary>
        private string _reason;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponse"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="HttpStatusCode"/></param>
        internal HttpResponse(HttpStatusCode code)
      : this(code, code.GetDescription())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponse"/> class.
        /// </summary>
        /// <param name="code">The code<see cref="HttpStatusCode"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        internal HttpResponse(HttpStatusCode code, string reason)
      : this(((int)code).ToString(), reason, HttpVersion.Version11, new NameValueCollection())
        {
            Headers["Server"] = "websocket-sharp/1.0";
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="HttpResponse"/> class from being created.
        /// </summary>
        /// <param name="code">The code<see cref="string"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        /// <param name="version">The version<see cref="Version"/></param>
        /// <param name="headers">The headers<see cref="NameValueCollection"/></param>
        private HttpResponse(string code, string reason, Version version, NameValueCollection headers)
      : base(version, headers)
        {
            _code = code;
            _reason = reason;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Cookies
        /// </summary>
        public CookieCollection Cookies
        {
            get
            {
                return Headers.GetCookies(true);
            }
        }

        /// <summary>
        /// Gets a value indicating whether HasConnectionClose
        /// </summary>
        public bool HasConnectionClose
        {
            get
            {
                var comparison = StringComparison.OrdinalIgnoreCase;
                return Headers.Contains("Connection", "close", comparison);
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsProxyAuthenticationRequired
        /// </summary>
        public bool IsProxyAuthenticationRequired
        {
            get
            {
                return _code == "407";
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsRedirect
        /// </summary>
        public bool IsRedirect
        {
            get
            {
                return _code == "301" || _code == "302";
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsUnauthorized
        /// </summary>
        public bool IsUnauthorized
        {
            get
            {
                return _code == "401";
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsWebSocketResponse
        /// </summary>
        public bool IsWebSocketResponse
        {
            get
            {
                return ProtocolVersion > HttpVersion.Version10
                       && _code == "101"
                       && Headers.Upgrades("websocket");
            }
        }

        /// <summary>
        /// Gets the Reason
        /// </summary>
        public string Reason
        {
            get
            {
                return _reason;
            }
        }

        /// <summary>
        /// Gets the StatusCode
        /// </summary>
        public string StatusCode
        {
            get
            {
                return _code;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The SetCookies
        /// </summary>
        /// <param name="cookies">The cookies<see cref="CookieCollection"/></param>
        public void SetCookies(CookieCollection cookies)
        {
            if (cookies == null || cookies.Count == 0)
                return;

            var headers = Headers;
            foreach (var cookie in cookies.Sorted)
                headers.Add("Set-Cookie", cookie.ToResponseString());
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            var output = new StringBuilder(64);
            output.AppendFormat("HTTP/{0} {1} {2}{3}", ProtocolVersion, _code, _reason, CrLf);

            var headers = Headers;
            foreach (var key in headers.AllKeys)
                output.AppendFormat("{0}: {1}{2}", key, headers[key], CrLf);

            output.Append(CrLf);

            var entity = EntityBody;
            if (entity.Length > 0)
                output.Append(entity);

            return output.ToString();
        }

        /// <summary>
        /// The CreateCloseResponse
        /// </summary>
        /// <param name="code">The code<see cref="HttpStatusCode"/></param>
        /// <returns>The <see cref="HttpResponse"/></returns>
        internal static HttpResponse CreateCloseResponse(HttpStatusCode code)
        {
            var res = new HttpResponse(code);
            res.Headers["Connection"] = "close";

            return res;
        }

        /// <summary>
        /// The CreateUnauthorizedResponse
        /// </summary>
        /// <param name="challenge">The challenge<see cref="string"/></param>
        /// <returns>The <see cref="HttpResponse"/></returns>
        internal static HttpResponse CreateUnauthorizedResponse(string challenge)
        {
            var res = new HttpResponse(HttpStatusCode.Unauthorized);
            res.Headers["WWW-Authenticate"] = challenge;

            return res;
        }

        /// <summary>
        /// The CreateWebSocketResponse
        /// </summary>
        /// <returns>The <see cref="HttpResponse"/></returns>
        internal static HttpResponse CreateWebSocketResponse()
        {
            var res = new HttpResponse(HttpStatusCode.SwitchingProtocols);

            var headers = res.Headers;
            headers["Upgrade"] = "websocket";
            headers["Connection"] = "Upgrade";

            return res;
        }

        /// <summary>
        /// The Parse
        /// </summary>
        /// <param name="headerParts">The headerParts<see cref="string[]"/></param>
        /// <returns>The <see cref="HttpResponse"/></returns>
        internal static HttpResponse Parse(string[] headerParts)
        {
            var statusLine = headerParts[0].Split(new[] { ' ' }, 3);
            if (statusLine.Length != 3)
                throw new ArgumentException("Invalid status line: " + headerParts[0]);

            var headers = new WebHeaderCollection();
            for (int i = 1; i < headerParts.Length; i++)
                headers.InternalSet(headerParts[i], true);

            return new HttpResponse(
              statusLine[1], statusLine[2], new Version(statusLine[0].Substring(5)), headers);
        }

        /// <summary>
        /// The Read
        /// </summary>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        /// <param name="millisecondsTimeout">The millisecondsTimeout<see cref="int"/></param>
        /// <returns>The <see cref="HttpResponse"/></returns>
        internal static HttpResponse Read(Stream stream, int millisecondsTimeout)
        {
            return Read<HttpResponse>(stream, Parse, millisecondsTimeout);
        }

        #endregion 方法
    }
}