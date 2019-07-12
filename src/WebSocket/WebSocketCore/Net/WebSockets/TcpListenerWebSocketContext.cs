/*
 * TcpListenerWebSocketContext.cs
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

/*
 * Contributors:
 * - Liryna <liryna.stark@gmail.com>
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;

namespace WebSocketCore.Net.WebSockets
{
    /// <summary>
    /// Provides the access to the information in a WebSocket handshake request to
    /// a <see cref="TcpListener"/> instance.
    /// </summary>
    internal class TcpListenerWebSocketContext : WebSocketContext
    {
        #region 字段

        /// <summary>
        /// Defines the _log
        /// </summary>
        private Logger _log;

        /// <summary>
        /// Defines the _queryString
        /// </summary>
        private NameValueCollection _queryString;

        /// <summary>
        /// Defines the _request
        /// </summary>
        private HttpRequest _request;

        /// <summary>
        /// Defines the _requestUri
        /// </summary>
        private Uri _requestUri;

        /// <summary>
        /// Defines the _secure
        /// </summary>
        private bool _secure;

        /// <summary>
        /// Defines the _serverEndPoint
        /// </summary>
        private System.Net.EndPoint _serverEndPoint;

        /// <summary>
        /// Defines the _stream
        /// </summary>
        private Stream _stream;

        /// <summary>
        /// Defines the _tcpClient
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        /// Defines the _user
        /// </summary>
        private IPrincipal _user;

        /// <summary>
        /// Defines the _userEndPoint
        /// </summary>
        private System.Net.EndPoint _userEndPoint;

        /// <summary>
        /// Defines the _websocket
        /// </summary>
        private WebSocket _websocket;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpListenerWebSocketContext"/> class.
        /// </summary>
        /// <param name="tcpClient">The tcpClient<see cref="TcpClient"/></param>
        /// <param name="protocol">The protocol<see cref="string"/></param>
        /// <param name="secure">The secure<see cref="bool"/></param>
        /// <param name="sslConfig">The sslConfig<see cref="ServerSslConfiguration"/></param>
        /// <param name="log">The log<see cref="Logger"/></param>
        internal TcpListenerWebSocketContext(
      TcpClient tcpClient,
      string protocol,
      bool secure,
      ServerSslConfiguration sslConfig,
      Logger log
    )
        {
            _tcpClient = tcpClient;
            _secure = secure;
            _log = log;

            var netStream = tcpClient.GetStream();
            if (secure)
            {
                var sslStream = new SslStream(
                                  netStream,
                                  false,
                                  sslConfig.ClientCertificateValidationCallback
                                );

                sslStream.AuthenticateAsServer(
                  sslConfig.ServerCertificate,
                  sslConfig.ClientCertificateRequired,
                  sslConfig.EnabledSslProtocols,
                  sslConfig.CheckCertificateRevocation
                );

                _stream = sslStream;
            }
            else
            {
                _stream = netStream;
            }

            var sock = tcpClient.Client;
            _serverEndPoint = sock.LocalEndPoint;
            _userEndPoint = sock.RemoteEndPoint;

            _request = HttpRequest.Read(_stream, 90000);
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
                return _request.Cookies;
            }
        }

        /// <summary>
        /// Gets the HTTP headers included in the handshake request.
        /// </summary>
        public override NameValueCollection Headers
        {
            get
            {
                return _request.Headers;
            }
        }

        /// <summary>
        /// Gets the value of the Host header included in the handshake request.
        /// </summary>
        public override string Host
        {
            get
            {
                return _request.Headers["Host"];
            }
        }

        /// <summary>
        /// Gets a value indicating whether the client is authenticated.
        /// </summary>
        public override bool IsAuthenticated
        {
            get
            {
                return _user != null;
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
                return UserEndPoint.Address.IsLocal();
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
                return _secure;
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
                return _request.IsWebSocketRequest;
            }
        }

        /// <summary>
        /// Gets the value of the Origin header included in the handshake request.
        /// </summary>
        public override string Origin
        {
            get
            {
                return _request.Headers["Origin"];
            }
        }

        /// <summary>
        /// Gets the query string included in the handshake request.
        /// </summary>
        public override NameValueCollection QueryString
        {
            get
            {
                if (_queryString == null)
                {
                    var uri = RequestUri;
                    _queryString = HttpUtility.InternalParseQueryString(
                                     uri != null ? uri.Query : null,
                                     Encoding.UTF8
                                   );
                }

                return _queryString;
            }
        }

        /// <summary>
        /// Gets the URI requested by the client.
        /// </summary>
        public override Uri RequestUri
        {
            get
            {
                if (_requestUri == null)
                {
                    _requestUri = HttpUtility.CreateRequestUrl(
                                    _request.RequestUri,
                                    _request.Headers["Host"],
                                    _request.IsWebSocketRequest,
                                    _secure
                                  );
                }

                return _requestUri;
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
                return _request.Headers["Sec-WebSocket-Key"];
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
                var val = _request.Headers["Sec-WebSocket-Protocol"];
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
                return _request.Headers["Sec-WebSocket-Version"];
            }
        }

        /// <summary>
        /// Gets the endpoint to which the handshake request is sent.
        /// </summary>
        public override System.Net.IPEndPoint ServerEndPoint
        {
            get
            {
                return (System.Net.IPEndPoint)_serverEndPoint;
            }
        }

        /// <summary>
        /// Gets the client information.
        /// </summary>
        public override IPrincipal User
        {
            get
            {
                return _user;
            }
        }

        /// <summary>
        /// Gets the endpoint from which the handshake request is sent.
        /// </summary>
        public override System.Net.IPEndPoint UserEndPoint
        {
            get
            {
                return (System.Net.IPEndPoint)_userEndPoint;
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
                return _log;
            }
        }

        /// <summary>
        /// Gets the Stream
        /// </summary>
        internal Stream Stream
        {
            get
            {
                return _stream;
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
            return _request.ToString();
        }

        /// <summary>
        /// The Authenticate
        /// </summary>
        /// <param name="scheme">The scheme<see cref="AuthenticationSchemes"/></param>
        /// <param name="realm">The realm<see cref="string"/></param>
        /// <param name="credentialsFinder">The credentialsFinder<see cref="Func{IIdentity, NetworkCredential}"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal bool Authenticate(
      AuthenticationSchemes scheme,
      string realm,
      Func<IIdentity, NetworkCredential> credentialsFinder
    )
        {
            var chal = new AuthenticationChallenge(scheme, realm).ToString();

            var retry = -1;
            Func<bool> auth = null;
            auth =
              () =>
              {
                  retry++;
                  if (retry > 99)
                      return false;

                  var user = HttpUtility.CreateUser(
                         _request.Headers["Authorization"],
                         scheme,
                         realm,
                         _request.HttpMethod,
                         credentialsFinder
                       );

                  if (user != null && user.Identity.IsAuthenticated)
                  {
                      _user = user;
                      return true;
                  }

                  _request = sendAuthenticationChallenge(chal);
                  return auth();
              };

            return auth();
        }

        /// <summary>
        /// The Close
        /// </summary>
        internal void Close()
        {
            _stream.Close();
            _tcpClient.Close();
        }

        /// <summary>
        /// The Close
        /// </summary>
        /// <param name="code">The code<see cref="HttpStatusCode"/></param>
        internal void Close(HttpStatusCode code)
        {
            var res = HttpResponse.CreateCloseResponse(code);
            var bytes = res.ToByteArray();
            _stream.Write(bytes, 0, bytes.Length);

            _stream.Close();
            _tcpClient.Close();
        }

        /// <summary>
        /// The sendAuthenticationChallenge
        /// </summary>
        /// <param name="challenge">The challenge<see cref="string"/></param>
        /// <returns>The <see cref="HttpRequest"/></returns>
        private HttpRequest sendAuthenticationChallenge(string challenge)
        {
            var res = HttpResponse.CreateUnauthorizedResponse(challenge);
            var bytes = res.ToByteArray();
            _stream.Write(bytes, 0, bytes.Length);

            return HttpRequest.Read(_stream, 15000);
        }

        #endregion 方法
    }
}