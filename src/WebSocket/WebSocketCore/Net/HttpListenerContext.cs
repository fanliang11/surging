/*
 * HttpListenerContext.cs
 *
 * This code is derived from HttpListenerContext.cs (System.Net) of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2012-2016 sta.blockhead
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
using System.Security.Principal;
using WebSocketCore.Net.WebSockets;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Provides the access to the HTTP request and response objects used by
    /// the <see cref="HttpListener"/>.
    /// </summary>
    public sealed class HttpListenerContext
    {
        #region 字段

        /// <summary>
        /// Defines the _connection
        /// </summary>
        private HttpConnection _connection;

        /// <summary>
        /// Defines the _error
        /// </summary>
        private string _error;

        /// <summary>
        /// Defines the _errorStatus
        /// </summary>
        private int _errorStatus;

        /// <summary>
        /// Defines the _listener
        /// </summary>
        private HttpListener _listener;

        /// <summary>
        /// Defines the _request
        /// </summary>
        private HttpListenerRequest _request;

        /// <summary>
        /// Defines the _response
        /// </summary>
        private HttpListenerResponse _response;

        /// <summary>
        /// Defines the _user
        /// </summary>
        private IPrincipal _user;

        /// <summary>
        /// Defines the _websocketContext
        /// </summary>
        private HttpListenerWebSocketContext _websocketContext;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerContext"/> class.
        /// </summary>
        /// <param name="connection">The connection<see cref="HttpConnection"/></param>
        internal HttpListenerContext(HttpConnection connection)
        {
            _connection = connection;
            _errorStatus = 400;
            _request = new HttpListenerRequest(this);
            _response = new HttpListenerResponse(this);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the HTTP request object that represents a client request.
        /// </summary>
        public HttpListenerRequest Request
        {
            get
            {
                return _request;
            }
        }

        /// <summary>
        /// Gets the HTTP response object used to send a response to the client.
        /// </summary>
        public HttpListenerResponse Response
        {
            get
            {
                return _response;
            }
        }

        /// <summary>
        /// Gets the client information (identity, authentication, and security roles).
        /// </summary>
        public IPrincipal User
        {
            get
            {
                return _user;
            }
        }

        /// <summary>
        /// Gets the Connection
        /// </summary>
        internal HttpConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        /// <summary>
        /// Gets or sets the ErrorMessage
        /// </summary>
        internal string ErrorMessage
        {
            get
            {
                return _error;
            }

            set
            {
                _error = value;
            }
        }

        /// <summary>
        /// Gets or sets the ErrorStatus
        /// </summary>
        internal int ErrorStatus
        {
            get
            {
                return _errorStatus;
            }

            set
            {
                _errorStatus = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether HasError
        /// </summary>
        internal bool HasError
        {
            get
            {
                return _error != null;
            }
        }

        /// <summary>
        /// Gets or sets the Listener
        /// </summary>
        internal HttpListener Listener
        {
            get
            {
                return _listener;
            }

            set
            {
                _listener = value;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Accepts a WebSocket handshake request.
        /// </summary>
        /// <param name="protocol">The protocol<see cref="string"/></param>
        /// <returns>The <see cref="HttpListenerWebSocketContext"/></returns>
        public HttpListenerWebSocketContext AcceptWebSocket(string protocol)
        {
            if (_websocketContext != null)
                throw new InvalidOperationException("The accepting is already in progress.");

            if (protocol != null)
            {
                if (protocol.Length == 0)
                    throw new ArgumentException("An empty string.", "protocol");

                if (!protocol.IsToken())
                    throw new ArgumentException("Contains an invalid character.", "protocol");
            }

            _websocketContext = new HttpListenerWebSocketContext(this, protocol);
            return _websocketContext;
        }

        /// <summary>
        /// The Authenticate
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        internal bool Authenticate()
        {
            var schm = _listener.SelectAuthenticationScheme(_request);
            if (schm == AuthenticationSchemes.Anonymous)
                return true;

            if (schm == AuthenticationSchemes.None)
            {
                _response.Close(HttpStatusCode.Forbidden);
                return false;
            }

            var realm = _listener.GetRealm();
            var user =
              HttpUtility.CreateUser(
                _request.Headers["Authorization"],
                schm,
                realm,
                _request.HttpMethod,
                _listener.GetUserCredentialsFinder()
              );

            if (user == null || !user.Identity.IsAuthenticated)
            {
                _response.CloseWithAuthChallenge(new AuthenticationChallenge(schm, realm).ToString());
                return false;
            }

            _user = user;
            return true;
        }

        /// <summary>
        /// The Register
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        internal bool Register()
        {
            return _listener.RegisterContext(this);
        }

        /// <summary>
        /// The Unregister
        /// </summary>
        internal void Unregister()
        {
            _listener.UnregisterContext(this);
        }

        #endregion 方法
    }
}