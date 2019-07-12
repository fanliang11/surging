/*
 * HttpListenerWebSocketContext.cs
 *
 * The MIT License
 *
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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;

namespace WebSocketCore.Net.WebSockets
{
    /// <summary>
    /// Provides the access to the information in a WebSocket handshake request to
    /// a <see cref="HttpListener"/> instance.
    /// </summary>
    public class HttpListenerWebSocketContext : WebSocketContext
    {
        #region 字段

        /// <summary>
        /// Defines the _context
        /// </summary>
        private HttpListenerContext _context;

        /// <summary>
        /// Defines the _websocket
        /// </summary>
        private WebSocket _websocket;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerWebSocketContext"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="HttpListenerContext"/></param>
        /// <param name="protocol">The protocol<see cref="string"/></param>
        internal HttpListenerWebSocketContext(
      HttpListenerContext context, string protocol
    )
        {
            _context = context;
            _websocket = new WebSocket(this, protocol);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the HTTP cookies included in the handshake request.
        /// </summary>
        public override CookieCollection CookieCollection
        {
            get
            {
                return _context.Request.Cookies;
            }
        }

        /// <summary>
        /// Gets the HTTP headers included in the handshake request.
        /// </summary>
        public override NameValueCollection Headers
        {
            get
            {
                return _context.Request.Headers;
            }
        }

        /// <summary>
        /// Gets the value of the Host header included in the handshake request.
        /// </summary>
        public override string Host
        {
            get
            {
                return _context.Request.UserHostName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the client is authenticated.
        /// </summary>
        public override bool IsAuthenticated
        {
            get
            {
                return _context.Request.IsAuthenticated;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the handshake request is sent from
        /// the local computer.
        /// </summary>
        public override bool IsLocal
        {
            get
            {
                return _context.Request.IsLocal;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a secure connection is used to send
        /// the handshake request.
        /// </summary>
        public override bool IsSecureConnection
        {
            get
            {
                return _context.Request.IsSecureConnection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the request is a WebSocket handshake
        /// request.
        /// </summary>
        public override bool IsWebSocketRequest
        {
            get
            {
                return _context.Request.IsWebSocketRequest;
            }
        }

        /// <summary>
        /// Gets the value of the Origin header included in the handshake request.
        /// </summary>
        public override string Origin
        {
            get
            {
                return _context.Request.Headers["Origin"];
            }
        }

        /// <summary>
        /// Gets the query string included in the handshake request.
        /// </summary>
        public override NameValueCollection QueryString
        {
            get
            {
                return _context.Request.QueryString;
            }
        }

        /// <summary>
        /// Gets the URI requested by the client.
        /// </summary>
        public override Uri RequestUri
        {
            get
            {
                return _context.Request.Url;
            }
        }

        /// <summary>
        /// Gets the value of the Sec-WebSocket-Key header included in
        /// the handshake request.
        /// </summary>
        public override string SecWebSocketKey
        {
            get
            {
                return _context.Request.Headers["Sec-WebSocket-Key"];
            }
        }

        /// <summary>
        /// Gets the names of the subprotocols from the Sec-WebSocket-Protocol
        /// header included in the handshake request.
        /// </summary>
        public override IEnumerable<string> SecWebSocketProtocols
        {
            get
            {
                var val = _context.Request.Headers["Sec-WebSocket-Protocol"];
                if (val == null || val.Length == 0)
                    yield break;

                foreach (var elm in val.Split(','))
                {
                    var protocol = elm.Trim();
                    if (protocol.Length == 0)
                        continue;

                    yield return protocol;
                }
            }
        }

        /// <summary>
        /// Gets the value of the Sec-WebSocket-Version header included in
        /// the handshake request.
        /// </summary>
        public override string SecWebSocketVersion
        {
            get
            {
                return _context.Request.Headers["Sec-WebSocket-Version"];
            }
        }

        /// <summary>
        /// Gets the endpoint to which the handshake request is sent.
        /// </summary>
        public override System.Net.IPEndPoint ServerEndPoint
        {
            get
            {
                return _context.Request.LocalEndPoint;
            }
        }

        /// <summary>
        /// Gets the client information.
        /// </summary>
        public override IPrincipal User
        {
            get
            {
                return _context.User;
            }
        }

        /// <summary>
        /// Gets the endpoint from which the handshake request is sent.
        /// </summary>
        public override System.Net.IPEndPoint UserEndPoint
        {
            get
            {
                return _context.Request.RemoteEndPoint;
            }
        }

        /// <summary>
        /// Gets the WebSocket instance used for two-way communication between
        /// the client and server.
        /// </summary>
        public override WebSocket WebSocket
        {
            get
            {
                return _websocket;
            }
        }

        /// <summary>
        /// Gets the Log
        /// </summary>
        internal Logger Log
        {
            get
            {
                return _context.Listener.Log;
            }
        }

        /// <summary>
        /// Gets the Stream
        /// </summary>
        internal Stream Stream
        {
            get
            {
                return _context.Connection.Stream;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Returns a string that represents the current instance.
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return _context.Request.ToString();
        }

        /// <summary>
        /// The Close
        /// </summary>
        internal void Close()
        {
            _context.Connection.Close(true);
        }

        /// <summary>
        /// The Close
        /// </summary>
        /// <param name="code">The code<see cref="HttpStatusCode"/></param>
        internal void Close(HttpStatusCode code)
        {
            _context.Response.Close(code);
        }

        #endregion 方法
    }
}