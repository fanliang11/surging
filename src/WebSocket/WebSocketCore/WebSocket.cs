/*
 * WebSocket.cs
 *
 * This code is derived from WebSocket.java
 * (http://github.com/adamac/Java-WebSocket-client).
 *
 * The MIT License
 *
 * Copyright (c) 2009 Adam MacBeth
 * Copyright (c) 2010-2016 sta.blockhead
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
 * - Frank Razenberg <frank@zzattack.org>
 * - David Wood <dpwood@gmail.com>
 * - Liryna <liryna.stark@gmail.com>
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using WebSocketCore.Net;
using WebSocketCore.Net.WebSockets;

namespace WebSocketCore
{
    /// <summary>
    /// Implements the WebSocket interface.
    /// </summary>
    public class WebSocket : IDisposable
    {
        #region 常量

        /// <summary>
        /// Defines the _guid
        /// </summary>
        private const string _guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        /// <summary>
        /// Defines the _version
        /// </summary>
        private const string _version = "13";

        #endregion 常量

        #region 字段

        /// <summary>
        /// Represents the empty array of <see cref="byte"/> used internally.
        /// </summary>
        internal static readonly byte[] EmptyBytes;

        /// <summary>
        /// Represents the length used to determine whether the data should be fragmented in sending.
        /// </summary>
        internal static readonly int FragmentLength;

        /// <summary>
        /// Represents the random number generator used internally.
        /// </summary>
        internal static readonly RandomNumberGenerator RandomNumber;

        /// <summary>
        /// Defines the _maxRetryCountForConnect
        /// </summary>
        private static readonly int _maxRetryCountForConnect;

        /// <summary>
        /// Defines the _authChallenge
        /// </summary>
        private AuthenticationChallenge _authChallenge;

        /// <summary>
        /// Defines the _base64Key
        /// </summary>
        private string _base64Key;

        /// <summary>
        /// Defines the _client
        /// </summary>
        private bool _client;

        /// <summary>
        /// Defines the _closeContext
        /// </summary>
        private Action _closeContext;

        /// <summary>
        /// Defines the _compression
        /// </summary>
        private CompressionMethod _compression;

        /// <summary>
        /// Defines the _context
        /// </summary>
        private WebSocketContext _context;

        /// <summary>
        /// Defines the _cookies
        /// </summary>
        private CookieCollection _cookies;

        /// <summary>
        /// Defines the _credentials
        /// </summary>
        private NetworkCredential _credentials;

        /// <summary>
        /// Defines the _emitOnPing
        /// </summary>
        private bool _emitOnPing;

        /// <summary>
        /// Defines the _enableRedirection
        /// </summary>
        private bool _enableRedirection;

        /// <summary>
        /// Defines the _extensions
        /// </summary>
        private string _extensions;

        /// <summary>
        /// Defines the _extensionsRequested
        /// </summary>
        private bool _extensionsRequested;

        /// <summary>
        /// Defines the _forMessageEventQueue
        /// </summary>
        private object _forMessageEventQueue;

        /// <summary>
        /// Defines the _forPing
        /// </summary>
        private object _forPing;

        /// <summary>
        /// Defines the _forSend
        /// </summary>
        private object _forSend;

        /// <summary>
        /// Defines the _forState
        /// </summary>
        private object _forState;

        /// <summary>
        /// Defines the _fragmentsBuffer
        /// </summary>
        private MemoryStream _fragmentsBuffer;

        /// <summary>
        /// Defines the _fragmentsCompressed
        /// </summary>
        private bool _fragmentsCompressed;

        /// <summary>
        /// Defines the _fragmentsOpcode
        /// </summary>
        private Opcode _fragmentsOpcode;

        /// <summary>
        /// Defines the _handshakeRequestChecker
        /// </summary>
        private Func<WebSocketContext, string> _handshakeRequestChecker;

        /// <summary>
        /// Defines the _ignoreExtensions
        /// </summary>
        private bool _ignoreExtensions;

        /// <summary>
        /// Defines the _inContinuation
        /// </summary>
        private bool _inContinuation;

        /// <summary>
        /// Defines the _inMessage
        /// </summary>
        private volatile bool _inMessage;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private volatile Logger _logger;

        /// <summary>
        /// Defines the _message
        /// </summary>
        private Action<MessageEventArgs> _message;

        /// <summary>
        /// Defines the _messageEventQueue
        /// </summary>
        private Queue<MessageEventArgs> _messageEventQueue;

        /// <summary>
        /// Defines the _nonceCount
        /// </summary>
        private uint _nonceCount;

        /// <summary>
        /// Defines the _origin
        /// </summary>
        private string _origin;

        /// <summary>
        /// Defines the _pongReceived
        /// </summary>
        private ManualResetEvent _pongReceived;

        /// <summary>
        /// Defines the _preAuth
        /// </summary>
        private bool _preAuth;

        /// <summary>
        /// Defines the _protocol
        /// </summary>
        private string _protocol;

        /// <summary>
        /// Defines the _protocols
        /// </summary>
        private string[] _protocols;

        /// <summary>
        /// Defines the _protocolsRequested
        /// </summary>
        private bool _protocolsRequested;

        /// <summary>
        /// Defines the _proxyCredentials
        /// </summary>
        private NetworkCredential _proxyCredentials;

        /// <summary>
        /// Defines the _proxyUri
        /// </summary>
        private Uri _proxyUri;

        /// <summary>
        /// Defines the _readyState
        /// </summary>
        private volatile WebSocketState _readyState;

        /// <summary>
        /// Defines the _receivingExited
        /// </summary>
        private ManualResetEvent _receivingExited;

        /// <summary>
        /// Defines the _retryCountForConnect
        /// </summary>
        private int _retryCountForConnect;

        /// <summary>
        /// Defines the _secure
        /// </summary>
        private bool _secure;

        /// <summary>
        /// Defines the _sslConfig
        /// </summary>
        private ClientSslConfiguration _sslConfig;

        /// <summary>
        /// Defines the _stream
        /// </summary>
        private Stream _stream;

        /// <summary>
        /// Defines the _tcpClient
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        /// Defines the _uri
        /// </summary>
        private Uri _uri;

        /// <summary>
        /// Defines the _waitTime
        /// </summary>
        private TimeSpan _waitTime;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocket"/> class.
        /// </summary>
        /// <param name="url">The url<see cref="string"/></param>
        /// <param name="protocols">The protocols<see cref="string[]"/></param>
        public WebSocket(string url, params string[] protocols)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            if (url.Length == 0)
                throw new ArgumentException("An empty string.", "url");

            string msg;
            if (!url.TryCreateWebSocketUri(out _uri, out msg))
                throw new ArgumentException(msg, "url");

            if (protocols != null && protocols.Length > 0)
            {
                if (!checkProtocols(protocols, out msg))
                    throw new ArgumentException(msg, "protocols");

                _protocols = protocols;
            }

            _base64Key = CreateBase64Key();
            _client = true;
            _logger = new Logger();
            _message = messagec;
            _secure = _uri.Scheme == "wss";
            _waitTime = TimeSpan.FromSeconds(5);

            init();
        }

        // As server
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocket"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="HttpListenerWebSocketContext"/></param>
        /// <param name="protocol">The protocol<see cref="string"/></param>
        internal WebSocket(HttpListenerWebSocketContext context, string protocol)
        {
            _context = context;
            _protocol = protocol;

            _closeContext = context.Close;
            _logger = context.Log;
            _message = messages;
            _secure = context.IsSecureConnection;
            _stream = context.Stream;
            _waitTime = TimeSpan.FromSeconds(1);

            init();
        }

        // As server
        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocket"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="TcpListenerWebSocketContext"/></param>
        /// <param name="protocol">The protocol<see cref="string"/></param>
        internal WebSocket(TcpListenerWebSocketContext context, string protocol)
        {
            _context = context;
            _protocol = protocol;

            _closeContext = context.Close;
            _logger = context.Log;
            _message = messages;
            _secure = context.IsSecureConnection;
            _stream = context.Stream;
            _waitTime = TimeSpan.FromSeconds(1);

            init();
        }

        /// <summary>
        /// Initializes static members of the <see cref="WebSocket"/> class.
        /// </summary>
        static WebSocket()
        {
            _maxRetryCountForConnect = 10;
            EmptyBytes = new byte[0];
            FragmentLength = 1016;
            RandomNumber = new RNGCryptoServiceProvider();
        }

        #endregion 构造函数

        #region 事件

        /// <summary>
        /// Occurs when the WebSocket connection has been closed.
        /// </summary>
        public event EventHandler<CloseEventArgs> OnClose;

        /// <summary>
        /// Occurs when the <see cref="WebSocket"/> gets an error.
        /// </summary>
        public event EventHandler<ErrorEventArgs> OnError;

        /// <summary>
        /// Occurs when the <see cref="WebSocket"/> receives a message.
        /// </summary>
        public event EventHandler<MessageEventArgs> OnMessage;

        /// <summary>
        /// Occurs when the WebSocket connection has been established.
        /// </summary>
        public event EventHandler OnOpen;

        #endregion 事件

        #region 属性

        /// <summary>
        /// Gets or sets the compression method used to compress a message.
        /// </summary>
        public CompressionMethod Compression
        {
            get
            {
                return _compression;
            }

            set
            {
                string msg = null;

                if (!_client)
                {
                    msg = "This instance is not a client.";
                    throw new InvalidOperationException(msg);
                }

                if (!canSet(out msg))
                {
                    _logger.Warn(msg);
                    return;
                }

                lock (_forState)
                {
                    if (!canSet(out msg))
                    {
                        _logger.Warn(msg);
                        return;
                    }

                    _compression = value;
                }
            }
        }

        /// <summary>
        /// Gets the HTTP cookies included in the handshake request/response.
        /// </summary>
        public IEnumerable<Cookie> Cookies
        {
            get
            {
                lock (_cookies.SyncRoot)
                {
                    foreach (Cookie cookie in _cookies)
                        yield return cookie;
                }
            }
        }

        /// <summary>
        /// Gets the credentials for the HTTP authentication (Basic/Digest).
        /// </summary>
        public NetworkCredential Credentials
        {
            get
            {
                return _credentials;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="OnMessage"/> event
        /// is emitted when a ping is received.
        /// </summary>
        public bool EmitOnPing
        {
            get
            {
                return _emitOnPing;
            }

            set
            {
                _emitOnPing = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the URL redirection for
        /// the handshake request is allowed.
        /// </summary>
        public bool EnableRedirection
        {
            get
            {
                return _enableRedirection;
            }

            set
            {
                string msg = null;

                if (!_client)
                {
                    msg = "This instance is not a client.";
                    throw new InvalidOperationException(msg);
                }

                if (!canSet(out msg))
                {
                    _logger.Warn(msg);
                    return;
                }

                lock (_forState)
                {
                    if (!canSet(out msg))
                    {
                        _logger.Warn(msg);
                        return;
                    }

                    _enableRedirection = value;
                }
            }
        }

        /// <summary>
        /// Gets the extensions selected by server.
        /// </summary>
        public string Extensions
        {
            get
            {
                return _extensions ?? String.Empty;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the connection is alive.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return ping(EmptyBytes);
            }
        }

        /// <summary>
        /// Gets a value indicating whether a secure connection is used.
        /// </summary>
        public bool IsSecure
        {
            get
            {
                return _secure;
            }
        }

        /// <summary>
        /// Gets or sets the Log
        /// Gets the logging function.
        /// </summary>
        public Logger Log
        {
            get
            {
                return _logger;
            }

            internal set
            {
                _logger = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the HTTP Origin header to send with
        /// the handshake request.
        /// </summary>
        public string Origin
        {
            get
            {
                return _origin;
            }

            set
            {
                string msg = null;

                if (!_client)
                {
                    msg = "This instance is not a client.";
                    throw new InvalidOperationException(msg);
                }

                if (!value.IsNullOrEmpty())
                {
                    Uri uri;
                    if (!Uri.TryCreate(value, UriKind.Absolute, out uri))
                    {
                        msg = "Not an absolute URI string.";
                        throw new ArgumentException(msg, "value");
                    }

                    if (uri.Segments.Length > 1)
                    {
                        msg = "It includes the path segments.";
                        throw new ArgumentException(msg, "value");
                    }
                }

                if (!canSet(out msg))
                {
                    _logger.Warn(msg);
                    return;
                }

                lock (_forState)
                {
                    if (!canSet(out msg))
                    {
                        _logger.Warn(msg);
                        return;
                    }

                    _origin = !value.IsNullOrEmpty() ? value.TrimEnd('/') : value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Protocol
        /// Gets the name of subprotocol selected by the server.
        /// </summary>
        public string Protocol
        {
            get
            {
                return _protocol ?? String.Empty;
            }

            internal set
            {
                _protocol = value;
            }
        }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        public WebSocketState ReadyState
        {
            get
            {
                return _readyState;
            }
        }

        /// <summary>
        /// Gets the configuration for secure connection.
        /// </summary>
        public ClientSslConfiguration SslConfiguration
        {
            get
            {
                if (!_client)
                {
                    var msg = "This instance is not a client.";
                    throw new InvalidOperationException(msg);
                }

                if (!_secure)
                {
                    var msg = "This instance does not use a secure connection.";
                    throw new InvalidOperationException(msg);
                }

                return getSslConfiguration();
            }
        }

        /// <summary>
        /// Gets the URL to which to connect.
        /// </summary>
        public Uri Url
        {
            get
            {
                return _client ? _uri : _context.RequestUri;
            }
        }

        /// <summary>
        /// Gets or sets the time to wait for the response to the ping or close.
        /// </summary>
        public TimeSpan WaitTime
        {
            get
            {
                return _waitTime;
            }

            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("value", "Zero or less.");

                string msg;
                if (!canSet(out msg))
                {
                    _logger.Warn(msg);
                    return;
                }

                lock (_forState)
                {
                    if (!canSet(out msg))
                    {
                        _logger.Warn(msg);
                        return;
                    }

                    _waitTime = value;
                }
            }
        }

        /// <summary>
        /// Gets the CookieCollection
        /// </summary>
        internal CookieCollection CookieCollection
        {
            get
            {
                return _cookies;
            }
        }

        // As server
        /// <summary>
        /// Gets or sets the CustomHandshakeRequestChecker
        /// </summary>
        internal Func<WebSocketContext, string> CustomHandshakeRequestChecker
        {
            get
            {
                return _handshakeRequestChecker;
            }

            set
            {
                _handshakeRequestChecker = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether HasMessage
        /// </summary>
        internal bool HasMessage
        {
            get
            {
                lock (_forMessageEventQueue)
                    return _messageEventQueue.Count > 0;
            }
        }

        // As server
        /// <summary>
        /// Gets or sets a value indicating whether IgnoreExtensions
        /// </summary>
        internal bool IgnoreExtensions
        {
            get
            {
                return _ignoreExtensions;
            }

            set
            {
                _ignoreExtensions = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsConnected
        /// </summary>
        internal bool IsConnected
        {
            get
            {
                return _readyState == WebSocketState.Open || _readyState == WebSocketState.Closing;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Accepts the handshake request.
        /// </summary>
        public void Accept()
        {
            if (_client)
            {
                var msg = "This instance is a client.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closing)
            {
                var msg = "The close process is in progress.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closed)
            {
                var msg = "The connection has already been closed.";
                throw new InvalidOperationException(msg);
            }

            if (accept())
                open();
        }

        /// <summary>
        /// Accepts the handshake request asynchronously.
        /// </summary>
        public void AcceptAsync()
        {
            if (_client)
            {
                var msg = "This instance is a client.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closing)
            {
                var msg = "The close process is in progress.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closed)
            {
                var msg = "The connection has already been closed.";
                throw new InvalidOperationException(msg);
            }

            Func<bool> acceptor = accept;
            acceptor.BeginInvoke(
              ar =>
              {
                  if (acceptor.EndInvoke(ar))
                      open();
              },
              null
            );
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close()
        {
            close(1005, String.Empty);
        }

        /// <summary>
        /// Closes the connection with the specified <paramref name="code"/>.
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        public void Close(CloseStatusCode code)
        {
            if (_client && code == CloseStatusCode.ServerError)
            {
                var msg = "ServerError cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == CloseStatusCode.MandatoryExtension)
            {
                var msg = "MandatoryExtension cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            close((ushort)code, String.Empty);
        }

        /// <summary>
        /// Closes the connection with the specified <paramref name="code"/> and
        /// <paramref name="reason"/>.
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        public void Close(CloseStatusCode code, string reason)
        {
            if (_client && code == CloseStatusCode.ServerError)
            {
                var msg = "ServerError cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == CloseStatusCode.MandatoryExtension)
            {
                var msg = "MandatoryExtension cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (reason.IsNullOrEmpty())
            {
                close((ushort)code, String.Empty);
                return;
            }

            if (code == CloseStatusCode.NoStatus)
            {
                var msg = "NoStatus cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            byte[] bytes;
            if (!reason.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "reason");
            }

            if (bytes.Length > 123)
            {
                var msg = "Its size is greater than 123 bytes.";
                throw new ArgumentOutOfRangeException("reason", msg);
            }

            close((ushort)code, reason);
        }

        /// <summary>
        /// Closes the connection with the specified <paramref name="code"/>.
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        public void Close(ushort code)
        {
            if (!code.IsCloseStatusCode())
            {
                var msg = "Less than 1000 or greater than 4999.";
                throw new ArgumentOutOfRangeException("code", msg);
            }

            if (_client && code == 1011)
            {
                var msg = "1011 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == 1010)
            {
                var msg = "1010 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            close(code, String.Empty);
        }

        /// <summary>
        /// Closes the connection with the specified <paramref name="code"/> and
        /// <paramref name="reason"/>.
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        public void Close(ushort code, string reason)
        {
            if (!code.IsCloseStatusCode())
            {
                var msg = "Less than 1000 or greater than 4999.";
                throw new ArgumentOutOfRangeException("code", msg);
            }

            if (_client && code == 1011)
            {
                var msg = "1011 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == 1010)
            {
                var msg = "1010 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (reason.IsNullOrEmpty())
            {
                close(code, String.Empty);
                return;
            }

            if (code == 1005)
            {
                var msg = "1005 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            byte[] bytes;
            if (!reason.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "reason");
            }

            if (bytes.Length > 123)
            {
                var msg = "Its size is greater than 123 bytes.";
                throw new ArgumentOutOfRangeException("reason", msg);
            }

            close(code, reason);
        }

        /// <summary>
        /// Closes the connection asynchronously.
        /// </summary>
        public void CloseAsync()
        {
            closeAsync(1005, String.Empty);
        }

        /// <summary>
        /// Closes the connection asynchronously with the specified
        /// <paramref name="code"/>.
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        public void CloseAsync(CloseStatusCode code)
        {
            if (_client && code == CloseStatusCode.ServerError)
            {
                var msg = "ServerError cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == CloseStatusCode.MandatoryExtension)
            {
                var msg = "MandatoryExtension cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            closeAsync((ushort)code, String.Empty);
        }

        /// <summary>
        /// Closes the connection asynchronously with the specified
        /// <paramref name="code"/> and <paramref name="reason"/>.
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        public void CloseAsync(CloseStatusCode code, string reason)
        {
            if (_client && code == CloseStatusCode.ServerError)
            {
                var msg = "ServerError cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == CloseStatusCode.MandatoryExtension)
            {
                var msg = "MandatoryExtension cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (reason.IsNullOrEmpty())
            {
                closeAsync((ushort)code, String.Empty);
                return;
            }

            if (code == CloseStatusCode.NoStatus)
            {
                var msg = "NoStatus cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            byte[] bytes;
            if (!reason.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "reason");
            }

            if (bytes.Length > 123)
            {
                var msg = "Its size is greater than 123 bytes.";
                throw new ArgumentOutOfRangeException("reason", msg);
            }

            closeAsync((ushort)code, reason);
        }

        /// <summary>
        /// Closes the connection asynchronously with the specified
        /// <paramref name="code"/>.
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        public void CloseAsync(ushort code)
        {
            if (!code.IsCloseStatusCode())
            {
                var msg = "Less than 1000 or greater than 4999.";
                throw new ArgumentOutOfRangeException("code", msg);
            }

            if (_client && code == 1011)
            {
                var msg = "1011 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == 1010)
            {
                var msg = "1010 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            closeAsync(code, String.Empty);
        }

        /// <summary>
        /// Closes the connection asynchronously with the specified
        /// <paramref name="code"/> and <paramref name="reason"/>.
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        public void CloseAsync(ushort code, string reason)
        {
            if (!code.IsCloseStatusCode())
            {
                var msg = "Less than 1000 or greater than 4999.";
                throw new ArgumentOutOfRangeException("code", msg);
            }

            if (_client && code == 1011)
            {
                var msg = "1011 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!_client && code == 1010)
            {
                var msg = "1010 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (reason.IsNullOrEmpty())
            {
                closeAsync(code, String.Empty);
                return;
            }

            if (code == 1005)
            {
                var msg = "1005 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            byte[] bytes;
            if (!reason.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "reason");
            }

            if (bytes.Length > 123)
            {
                var msg = "Its size is greater than 123 bytes.";
                throw new ArgumentOutOfRangeException("reason", msg);
            }

            closeAsync(code, reason);
        }

        /// <summary>
        /// Establishes a connection.
        /// </summary>
        public void Connect()
        {
            if (!_client)
            {
                var msg = "This instance is not a client.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closing)
            {
                var msg = "The close process is in progress.";
                throw new InvalidOperationException(msg);
            }

            if (_retryCountForConnect > _maxRetryCountForConnect)
            {
                var msg = "A series of reconnecting has failed.";
                throw new InvalidOperationException(msg);
            }

            if (connect())
                open();
        }

        /// <summary>
        /// Establishes a connection asynchronously.
        /// </summary>
        public void ConnectAsync()
        {
            if (!_client)
            {
                var msg = "This instance is not a client.";
                throw new InvalidOperationException(msg);
            }

            if (_readyState == WebSocketState.Closing)
            {
                var msg = "The close process is in progress.";
                throw new InvalidOperationException(msg);
            }

            if (_retryCountForConnect > _maxRetryCountForConnect)
            {
                var msg = "A series of reconnecting has failed.";
                throw new InvalidOperationException(msg);
            }

            Func<bool> connector = connect;
            connector.BeginInvoke(
              ar =>
              {
                  if (connector.EndInvoke(ar))
                      open();
              },
              null
            );
        }

        /// <summary>
        /// Sends a ping using the WebSocket connection.
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool Ping()
        {
            return ping(EmptyBytes);
        }

        /// <summary>
        /// Sends a ping with <paramref name="message"/> using the WebSocket
        /// connection.
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool Ping(string message)
        {
            if (message.IsNullOrEmpty())
                return ping(EmptyBytes);

            byte[] bytes;
            if (!message.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "message");
            }

            if (bytes.Length > 125)
            {
                var msg = "Its size is greater than 125 bytes.";
                throw new ArgumentOutOfRangeException("message", msg);
            }

            return ping(bytes);
        }

        /// <summary>
        /// Sends the specified data using the WebSocket connection.
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        public void Send(byte[] data)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (data == null)
                throw new ArgumentNullException("data");

            send(Opcode.Binary, new MemoryStream(data));
        }

        /// <summary>
        /// Sends the specified file using the WebSocket connection.
        /// </summary>
        /// <param name="fileInfo">The fileInfo<see cref="FileInfo"/></param>
        public void Send(FileInfo fileInfo)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");

            if (!fileInfo.Exists)
            {
                var msg = "The file does not exist.";
                throw new ArgumentException(msg, "fileInfo");
            }

            FileStream stream;
            if (!fileInfo.TryOpenRead(out stream))
            {
                var msg = "The file could not be opened.";
                throw new ArgumentException(msg, "fileInfo");
            }

            send(Opcode.Binary, stream);
        }

        /// <summary>
        /// Sends the data from the specified stream using the WebSocket connection.
        /// </summary>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        /// <param name="length">The length<see cref="int"/></param>
        public void Send(Stream stream, int length)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanRead)
            {
                var msg = "It cannot be read.";
                throw new ArgumentException(msg, "stream");
            }

            if (length < 1)
            {
                var msg = "Less than 1.";
                throw new ArgumentException(msg, "length");
            }

            var bytes = stream.ReadBytes(length);

            var len = bytes.Length;
            if (len == 0)
            {
                var msg = "No data could be read from it.";
                throw new ArgumentException(msg, "stream");
            }

            if (len < length)
            {
                _logger.Warn(
                  String.Format(
                    "Only {0} byte(s) of data could be read from the stream.",
                    len
                  )
                );
            }

            send(Opcode.Binary, new MemoryStream(bytes));
        }

        /// <summary>
        /// Sends the specified data using the WebSocket connection.
        /// </summary>
        /// <param name="data">The data<see cref="string"/></param>
        public void Send(string data)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (data == null)
                throw new ArgumentNullException("data");

            byte[] bytes;
            if (!data.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "data");
            }

            send(Opcode.Text, new MemoryStream(bytes));
        }

        /// <summary>
        /// Sends the specified data asynchronously using the WebSocket connection.
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <param name="completed">The completed<see cref="Action{bool}"/></param>
        public void SendAsync(byte[] data, Action<bool> completed)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (data == null)
                throw new ArgumentNullException("data");

            sendAsync(Opcode.Binary, new MemoryStream(data), completed);
        }

        /// <summary>
        /// Sends the specified file asynchronously using the WebSocket connection.
        /// </summary>
        /// <param name="fileInfo">The fileInfo<see cref="FileInfo"/></param>
        /// <param name="completed">The completed<see cref="Action{bool}"/></param>
        public void SendAsync(FileInfo fileInfo, Action<bool> completed)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");

            if (!fileInfo.Exists)
            {
                var msg = "The file does not exist.";
                throw new ArgumentException(msg, "fileInfo");
            }

            FileStream stream;
            if (!fileInfo.TryOpenRead(out stream))
            {
                var msg = "The file could not be opened.";
                throw new ArgumentException(msg, "fileInfo");
            }

            sendAsync(Opcode.Binary, stream, completed);
        }

        /// <summary>
        /// Sends the data from the specified stream asynchronously using
        /// the WebSocket connection.
        /// </summary>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        /// <param name="length">The length<see cref="int"/></param>
        /// <param name="completed">The completed<see cref="Action{bool}"/></param>
        public void SendAsync(Stream stream, int length, Action<bool> completed)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanRead)
            {
                var msg = "It cannot be read.";
                throw new ArgumentException(msg, "stream");
            }

            if (length < 1)
            {
                var msg = "Less than 1.";
                throw new ArgumentException(msg, "length");
            }

            var bytes = stream.ReadBytes(length);

            var len = bytes.Length;
            if (len == 0)
            {
                var msg = "No data could be read from it.";
                throw new ArgumentException(msg, "stream");
            }

            if (len < length)
            {
                _logger.Warn(
                  String.Format(
                    "Only {0} byte(s) of data could be read from the stream.",
                    len
                  )
                );
            }

            sendAsync(Opcode.Binary, new MemoryStream(bytes), completed);
        }

        /// <summary>
        /// Sends the specified data asynchronously using the WebSocket connection.
        /// </summary>
        /// <param name="data">The data<see cref="string"/></param>
        /// <param name="completed">The completed<see cref="Action{bool}"/></param>
        public void SendAsync(string data, Action<bool> completed)
        {
            if (_readyState != WebSocketState.Open)
            {
                var msg = "The current state of the connection is not Open.";
                throw new InvalidOperationException(msg);
            }

            if (data == null)
                throw new ArgumentNullException("data");

            byte[] bytes;
            if (!data.TryGetUTF8EncodedBytes(out bytes))
            {
                var msg = "It could not be UTF-8-encoded.";
                throw new ArgumentException(msg, "data");
            }

            sendAsync(Opcode.Text, new MemoryStream(bytes), completed);
        }

        /// <summary>
        /// Sets an HTTP cookie to send with the handshake request.
        /// </summary>
        /// <param name="cookie">The cookie<see cref="Cookie"/></param>
        public void SetCookie(Cookie cookie)
        {
            string msg = null;

            if (!_client)
            {
                msg = "This instance is not a client.";
                throw new InvalidOperationException(msg);
            }

            if (cookie == null)
                throw new ArgumentNullException("cookie");

            if (!canSet(out msg))
            {
                _logger.Warn(msg);
                return;
            }

            lock (_forState)
            {
                if (!canSet(out msg))
                {
                    _logger.Warn(msg);
                    return;
                }

                lock (_cookies.SyncRoot)
                    _cookies.SetOrRemove(cookie);
            }
        }

        /// <summary>
        /// Sets the credentials for the HTTP authentication (Basic/Digest).
        /// </summary>
        /// <param name="username">The username<see cref="string"/></param>
        /// <param name="password">The password<see cref="string"/></param>
        /// <param name="preAuth">The preAuth<see cref="bool"/></param>
        public void SetCredentials(string username, string password, bool preAuth)
        {
            string msg = null;

            if (!_client)
            {
                msg = "This instance is not a client.";
                throw new InvalidOperationException(msg);
            }

            if (!username.IsNullOrEmpty())
            {
                if (username.Contains(':') || !username.IsText())
                {
                    msg = "It contains an invalid character.";
                    throw new ArgumentException(msg, "username");
                }
            }

            if (!password.IsNullOrEmpty())
            {
                if (!password.IsText())
                {
                    msg = "It contains an invalid character.";
                    throw new ArgumentException(msg, "password");
                }
            }

            if (!canSet(out msg))
            {
                _logger.Warn(msg);
                return;
            }

            lock (_forState)
            {
                if (!canSet(out msg))
                {
                    _logger.Warn(msg);
                    return;
                }

                if (username.IsNullOrEmpty())
                {
                    _credentials = null;
                    _preAuth = false;

                    return;
                }

                _credentials = new NetworkCredential(
                                 username, password, _uri.PathAndQuery
                               );

                _preAuth = preAuth;
            }
        }

        /// <summary>
        /// Sets the URL of the HTTP proxy server through which to connect and
        /// the credentials for the HTTP proxy authentication (Basic/Digest).
        /// </summary>
        /// <param name="url">The url<see cref="string"/></param>
        /// <param name="username">The username<see cref="string"/></param>
        /// <param name="password">The password<see cref="string"/></param>
        public void SetProxy(string url, string username, string password)
        {
            string msg = null;

            if (!_client)
            {
                msg = "This instance is not a client.";
                throw new InvalidOperationException(msg);
            }

            Uri uri = null;

            if (!url.IsNullOrEmpty())
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    msg = "Not an absolute URI string.";
                    throw new ArgumentException(msg, "url");
                }

                if (uri.Scheme != "http")
                {
                    msg = "The scheme part is not http.";
                    throw new ArgumentException(msg, "url");
                }

                if (uri.Segments.Length > 1)
                {
                    msg = "It includes the path segments.";
                    throw new ArgumentException(msg, "url");
                }
            }

            if (!username.IsNullOrEmpty())
            {
                if (username.Contains(':') || !username.IsText())
                {
                    msg = "It contains an invalid character.";
                    throw new ArgumentException(msg, "username");
                }
            }

            if (!password.IsNullOrEmpty())
            {
                if (!password.IsText())
                {
                    msg = "It contains an invalid character.";
                    throw new ArgumentException(msg, "password");
                }
            }

            if (!canSet(out msg))
            {
                _logger.Warn(msg);
                return;
            }

            lock (_forState)
            {
                if (!canSet(out msg))
                {
                    _logger.Warn(msg);
                    return;
                }

                if (url.IsNullOrEmpty())
                {
                    _proxyUri = null;
                    _proxyCredentials = null;

                    return;
                }

                _proxyUri = uri;
                _proxyCredentials = !username.IsNullOrEmpty()
                                    ? new NetworkCredential(
                                        username,
                                        password,
                                        String.Format(
                                          "{0}:{1}", _uri.DnsSafeHost, _uri.Port
                                        )
                                      )
                                    : null;
            }
        }

        // As client
        /// <summary>
        /// The CreateBase64Key
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        internal static string CreateBase64Key()
        {
            var src = new byte[16];
            RandomNumber.GetBytes(src);

            return Convert.ToBase64String(src);
        }

        /// <summary>
        /// The CreateResponseKey
        /// </summary>
        /// <param name="base64Key">The base64Key<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        internal static string CreateResponseKey(string base64Key)
        {
            var buff = new StringBuilder(base64Key, 64);
            buff.Append(_guid);
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            var src = sha1.ComputeHash(buff.ToString().UTF8Encode());

            return Convert.ToBase64String(src);
        }

        // As server
        /// <summary>
        /// The Close
        /// </summary>
        /// <param name="response">The response<see cref="HttpResponse"/></param>
        internal void Close(HttpResponse response)
        {
            _readyState = WebSocketState.Closing;

            sendHttpResponse(response);
            releaseServerResources();

            _readyState = WebSocketState.Closed;
        }

        // As server
        /// <summary>
        /// The Close
        /// </summary>
        /// <param name="code">The code<see cref="HttpStatusCode"/></param>
        internal void Close(HttpStatusCode code)
        {
            Close(createHandshakeFailureResponse(code));
        }

        // As server
        /// <summary>
        /// The Close
        /// </summary>
        /// <param name="payloadData">The payloadData<see cref="PayloadData"/></param>
        /// <param name="frameAsBytes">The frameAsBytes<see cref="byte[]"/></param>
        internal void Close(PayloadData payloadData, byte[] frameAsBytes)
        {
            lock (_forState)
            {
                if (_readyState == WebSocketState.Closing)
                {
                    _logger.Info("The closing is already in progress.");
                    return;
                }

                if (_readyState == WebSocketState.Closed)
                {
                    _logger.Info("The connection has already been closed.");
                    return;
                }

                _readyState = WebSocketState.Closing;
            }

            _logger.Trace("Begin closing the connection.");

            var sent = frameAsBytes != null && sendBytes(frameAsBytes);
            var received = sent && _receivingExited != null
                           ? _receivingExited.WaitOne(_waitTime)
                           : false;

            var res = sent && received;

            _logger.Debug(
              String.Format(
                "Was clean?: {0}\n  sent: {1}\n  received: {2}", res, sent, received
              )
            );

            releaseServerResources();
            releaseCommonResources();

            _logger.Trace("End closing the connection.");

            _readyState = WebSocketState.Closed;

            var e = new CloseEventArgs(payloadData);
            e.WasClean = res;

            try
            {
                OnClose.Emit(this, e);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
        }

        // As server
        /// <summary>
        /// The InternalAccept
        /// </summary>
        internal void InternalAccept()
        {
            try
            {
                if (!acceptHandshake())
                    return;
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex.Message);
                _logger.Debug(ex.ToString());

                var msg = "An exception has occurred while attempting to accept.";
                fatal(msg, ex);

                return;
            }

            _readyState = WebSocketState.Open;

            open();
        }

        // As server
        /// <summary>
        /// The Ping
        /// </summary>
        /// <param name="frameAsBytes">The frameAsBytes<see cref="byte[]"/></param>
        /// <param name="timeout">The timeout<see cref="TimeSpan"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal bool Ping(byte[] frameAsBytes, TimeSpan timeout)
        {
            if (_readyState != WebSocketState.Open)
                return false;

            var pongReceived = _pongReceived;
            if (pongReceived == null)
                return false;

            lock (_forPing)
            {
                try
                {
                    pongReceived.Reset();

                    lock (_forState)
                    {
                        if (_readyState != WebSocketState.Open)
                            return false;

                        if (!sendBytes(frameAsBytes))
                            return false;
                    }

                    return pongReceived.WaitOne(timeout);
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }
            }
        }

        // As server
        /// <summary>
        /// The Send
        /// </summary>
        /// <param name="opcode">The opcode<see cref="Opcode"/></param>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <param name="cache">The cache<see cref="Dictionary{CompressionMethod, byte[]}"/></param>
        internal void Send(
      Opcode opcode, byte[] data, Dictionary<CompressionMethod, byte[]> cache
    )
        {
            lock (_forSend)
            {
                lock (_forState)
                {
                    if (_readyState != WebSocketState.Open)
                    {
                        _logger.Error("The connection is closing.");
                        return;
                    }

                    byte[] found;
                    if (!cache.TryGetValue(_compression, out found))
                    {
                        found = new WebSocketFrame(
                                  Fin.Final,
                                  opcode,
                                  data.Compress(_compression),
                                  _compression != CompressionMethod.None,
                                  false
                                )
                                .ToArray();

                        cache.Add(_compression, found);
                    }

                    sendBytes(found);
                }
            }
        }

        // As server
        /// <summary>
        /// The Send
        /// </summary>
        /// <param name="opcode">The opcode<see cref="Opcode"/></param>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        /// <param name="cache">The cache<see cref="Dictionary{CompressionMethod, Stream}"/></param>
        internal void Send(
      Opcode opcode, Stream stream, Dictionary<CompressionMethod, Stream> cache
    )
        {
            lock (_forSend)
            {
                Stream found;
                if (!cache.TryGetValue(_compression, out found))
                {
                    found = stream.Compress(_compression);
                    cache.Add(_compression, found);
                }
                else
                {
                    found.Position = 0;
                }

                send(opcode, found, _compression != CompressionMethod.None);
            }
        }

        /// <summary>
        /// The checkProtocols
        /// </summary>
        /// <param name="protocols">The protocols<see cref="string[]"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool checkProtocols(string[] protocols, out string message)
        {
            message = null;

            Func<string, bool> cond = protocol => protocol.IsNullOrEmpty()
                                                  || !protocol.IsToken();

            if (protocols.Contains(cond))
            {
                message = "It contains a value that is not a token.";
                return false;
            }

            if (protocols.ContainsTwice())
            {
                message = "It contains a value twice.";
                return false;
            }

            return true;
        }

        // As server
        /// <summary>
        /// The accept
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        private bool accept()
        {
            if (_readyState == WebSocketState.Open)
            {
                var msg = "The handshake request has already been accepted.";
                _logger.Warn(msg);

                return false;
            }

            lock (_forState)
            {
                if (_readyState == WebSocketState.Open)
                {
                    var msg = "The handshake request has already been accepted.";
                    _logger.Warn(msg);

                    return false;
                }

                if (_readyState == WebSocketState.Closing)
                {
                    var msg = "The close process has set in.";
                    _logger.Error(msg);

                    msg = "An interruption has occurred while attempting to accept.";
                    error(msg, null);

                    return false;
                }

                if (_readyState == WebSocketState.Closed)
                {
                    var msg = "The connection has been closed.";
                    _logger.Error(msg);

                    msg = "An interruption has occurred while attempting to accept.";
                    error(msg, null);

                    return false;
                }

                try
                {
                    if (!acceptHandshake())
                        return false;
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex.Message);
                    _logger.Debug(ex.ToString());

                    var msg = "An exception has occurred while attempting to accept.";
                    fatal(msg, ex);

                    return false;
                }

                _readyState = WebSocketState.Open;
                return true;
            }
        }

        // As server
        /// <summary>
        /// The acceptHandshake
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        private bool acceptHandshake()
        {
            _logger.Debug(
              String.Format(
                "A handshake request from {0}:\n{1}", _context.UserEndPoint, _context
              )
            );

            string msg;
            if (!checkHandshakeRequest(_context, out msg))
            {
                _logger.Error(msg);

                refuseHandshake(
                  CloseStatusCode.ProtocolError,
                  "A handshake error has occurred while attempting to accept."
                );

                return false;
            }

            if (!customCheckHandshakeRequest(_context, out msg))
            {
                _logger.Error(msg);

                refuseHandshake(
                  CloseStatusCode.PolicyViolation,
                  "A handshake error has occurred while attempting to accept."
                );

                return false;
            }

            _base64Key = _context.Headers["Sec-WebSocket-Key"];

            if (_protocol != null)
            {
                var vals = _context.SecWebSocketProtocols;
                processSecWebSocketProtocolClientHeader(vals);
            }

            if (!_ignoreExtensions)
            {
                var val = _context.Headers["Sec-WebSocket-Extensions"];
                processSecWebSocketExtensionsClientHeader(val);
            }

            return sendHttpResponse(createHandshakeResponse());
        }

        /// <summary>
        /// The canSet
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool canSet(out string message)
        {
            message = null;

            if (_readyState == WebSocketState.Open)
            {
                message = "The connection has already been established.";
                return false;
            }

            if (_readyState == WebSocketState.Closing)
            {
                message = "The connection is closing.";
                return false;
            }

            return true;
        }

        // As server
        /// <summary>
        /// The checkHandshakeRequest
        /// </summary>
        /// <param name="context">The context<see cref="WebSocketContext"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool checkHandshakeRequest(
      WebSocketContext context, out string message
    )
        {
            message = null;

            if (!context.IsWebSocketRequest)
            {
                message = "Not a handshake request.";
                return false;
            }

            if (context.RequestUri == null)
            {
                message = "It specifies an invalid Request-URI.";
                return false;
            }

            var headers = context.Headers;

            var key = headers["Sec-WebSocket-Key"];
            if (key == null)
            {
                message = "It includes no Sec-WebSocket-Key header.";
                return false;
            }

            if (key.Length == 0)
            {
                message = "It includes an invalid Sec-WebSocket-Key header.";
                return false;
            }

            var version = headers["Sec-WebSocket-Version"];
            if (version == null)
            {
                message = "It includes no Sec-WebSocket-Version header.";
                return false;
            }

            if (version != _version)
            {
                message = "It includes an invalid Sec-WebSocket-Version header.";
                return false;
            }

            var protocol = headers["Sec-WebSocket-Protocol"];
            if (protocol != null && protocol.Length == 0)
            {
                message = "It includes an invalid Sec-WebSocket-Protocol header.";
                return false;
            }

            if (!_ignoreExtensions)
            {
                var extensions = headers["Sec-WebSocket-Extensions"];
                if (extensions != null && extensions.Length == 0)
                {
                    message = "It includes an invalid Sec-WebSocket-Extensions header.";
                    return false;
                }
            }

            return true;
        }

        // As client
        /// <summary>
        /// The checkHandshakeResponse
        /// </summary>
        /// <param name="response">The response<see cref="HttpResponse"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool checkHandshakeResponse(HttpResponse response, out string message)
        {
            message = null;

            if (response.IsRedirect)
            {
                message = "Indicates the redirection.";
                return false;
            }

            if (response.IsUnauthorized)
            {
                message = "Requires the authentication.";
                return false;
            }

            if (!response.IsWebSocketResponse)
            {
                message = "Not a WebSocket handshake response.";
                return false;
            }

            var headers = response.Headers;
            if (!validateSecWebSocketAcceptHeader(headers["Sec-WebSocket-Accept"]))
            {
                message = "Includes no Sec-WebSocket-Accept header, or it has an invalid value.";
                return false;
            }

            if (!validateSecWebSocketProtocolServerHeader(headers["Sec-WebSocket-Protocol"]))
            {
                message = "Includes no Sec-WebSocket-Protocol header, or it has an invalid value.";
                return false;
            }

            if (!validateSecWebSocketExtensionsServerHeader(headers["Sec-WebSocket-Extensions"]))
            {
                message = "Includes an invalid Sec-WebSocket-Extensions header.";
                return false;
            }

            if (!validateSecWebSocketVersionServerHeader(headers["Sec-WebSocket-Version"]))
            {
                message = "Includes an invalid Sec-WebSocket-Version header.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// The checkReceivedFrame
        /// </summary>
        /// <param name="frame">The frame<see cref="WebSocketFrame"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool checkReceivedFrame(WebSocketFrame frame, out string message)
        {
            message = null;

            var masked = frame.IsMasked;
            if (_client && masked)
            {
                message = "A frame from the server is masked.";
                return false;
            }

            if (!_client && !masked)
            {
                message = "A frame from a client is not masked.";
                return false;
            }

            if (_inContinuation && frame.IsData)
            {
                message = "A data frame has been received while receiving continuation frames.";
                return false;
            }

            if (frame.IsCompressed && _compression == CompressionMethod.None)
            {
                message = "A compressed frame has been received without any agreement for it.";
                return false;
            }

            if (frame.Rsv2 == Rsv.On)
            {
                message = "The RSV2 of a frame is non-zero without any negotiation for it.";
                return false;
            }

            if (frame.Rsv3 == Rsv.On)
            {
                message = "The RSV3 of a frame is non-zero without any negotiation for it.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// The close
        /// </summary>
        /// <param name="payloadData">The payloadData<see cref="PayloadData"/></param>
        /// <param name="send">The send<see cref="bool"/></param>
        /// <param name="receive">The receive<see cref="bool"/></param>
        /// <param name="received">The received<see cref="bool"/></param>
        private void close(
      PayloadData payloadData, bool send, bool receive, bool received
    )
        {
            lock (_forState)
            {
                if (_readyState == WebSocketState.Closing)
                {
                    _logger.Info("The closing is already in progress.");
                    return;
                }

                if (_readyState == WebSocketState.Closed)
                {
                    _logger.Info("The connection has already been closed.");
                    return;
                }

                send = send && _readyState == WebSocketState.Open;
                receive = send && receive;

                _readyState = WebSocketState.Closing;
            }

            _logger.Trace("Begin closing the connection.");

            var res = closeHandshake(payloadData, send, receive, received);
            releaseResources();

            _logger.Trace("End closing the connection.");

            _readyState = WebSocketState.Closed;

            var e = new CloseEventArgs(payloadData);
            e.WasClean = res;

            try
            {
                OnClose.Emit(this, e);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                error("An error has occurred during the OnClose event.", ex);
            }
        }

        /// <summary>
        /// The close
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        private void close(ushort code, string reason)
        {
            if (_readyState == WebSocketState.Closing)
            {
                _logger.Info("The closing is already in progress.");
                return;
            }

            if (_readyState == WebSocketState.Closed)
            {
                _logger.Info("The connection has already been closed.");
                return;
            }

            if (code == 1005)
            { // == no status
                close(PayloadData.Empty, true, true, false);
                return;
            }

            var send = !code.IsReserved();
            close(new PayloadData(code, reason), send, send, false);
        }

        /// <summary>
        /// The closeAsync
        /// </summary>
        /// <param name="payloadData">The payloadData<see cref="PayloadData"/></param>
        /// <param name="send">The send<see cref="bool"/></param>
        /// <param name="receive">The receive<see cref="bool"/></param>
        /// <param name="received">The received<see cref="bool"/></param>
        private void closeAsync(
      PayloadData payloadData, bool send, bool receive, bool received
    )
        {
            Action<PayloadData, bool, bool, bool> closer = close;
            closer.BeginInvoke(
              payloadData, send, receive, received, ar => closer.EndInvoke(ar), null
            );
        }

        /// <summary>
        /// The closeAsync
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        private void closeAsync(ushort code, string reason)
        {
            if (_readyState == WebSocketState.Closing)
            {
                _logger.Info("The closing is already in progress.");
                return;
            }

            if (_readyState == WebSocketState.Closed)
            {
                _logger.Info("The connection has already been closed.");
                return;
            }

            if (code == 1005)
            { // == no status
                closeAsync(PayloadData.Empty, true, true, false);
                return;
            }

            var send = !code.IsReserved();
            closeAsync(new PayloadData(code, reason), send, send, false);
        }

        /// <summary>
        /// The closeHandshake
        /// </summary>
        /// <param name="payloadData">The payloadData<see cref="PayloadData"/></param>
        /// <param name="send">The send<see cref="bool"/></param>
        /// <param name="receive">The receive<see cref="bool"/></param>
        /// <param name="received">The received<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool closeHandshake(
      PayloadData payloadData, bool send, bool receive, bool received
    )
        {
            var sent = false;
            if (send)
            {
                var frame = WebSocketFrame.CreateCloseFrame(payloadData, _client);
                sent = sendBytes(frame.ToArray());

                if (_client)
                    frame.Unmask();
            }

            var wait = !received && sent && receive && _receivingExited != null;
            if (wait)
                received = _receivingExited.WaitOne(_waitTime);

            var ret = sent && received;

            _logger.Debug(
              String.Format(
                "Was clean?: {0}\n  sent: {1}\n  received: {2}", ret, sent, received
              )
            );

            return ret;
        }

        /// <summary>
        /// The closeHandshake
        /// </summary>
        /// <param name="frameAsBytes">The frameAsBytes<see cref="byte[]"/></param>
        /// <param name="receive">The receive<see cref="bool"/></param>
        /// <param name="received">The received<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool closeHandshake(byte[] frameAsBytes, bool receive, bool received)
        {
            var sent = frameAsBytes != null && sendBytes(frameAsBytes);

            var wait = !received && sent && receive && _receivingExited != null;
            if (wait)
                received = _receivingExited.WaitOne(_waitTime);

            var ret = sent && received;

            _logger.Debug(
              String.Format(
                "Was clean?: {0}\n  sent: {1}\n  received: {2}", ret, sent, received
              )
            );

            return ret;
        }

        // As client
        /// <summary>
        /// The connect
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        private bool connect()
        {
            if (_readyState == WebSocketState.Open)
            {
                var msg = "The connection has already been established.";
                _logger.Warn(msg);

                return false;
            }

            lock (_forState)
            {
                if (_readyState == WebSocketState.Open)
                {
                    var msg = "The connection has already been established.";
                    _logger.Warn(msg);

                    return false;
                }

                if (_readyState == WebSocketState.Closing)
                {
                    var msg = "The close process has set in.";
                    _logger.Error(msg);

                    msg = "An interruption has occurred while attempting to connect.";
                    error(msg, null);

                    return false;
                }

                if (_retryCountForConnect > _maxRetryCountForConnect)
                {
                    var msg = "An opportunity for reconnecting has been lost.";
                    _logger.Error(msg);

                    msg = "An interruption has occurred while attempting to connect.";
                    error(msg, null);

                    return false;
                }

                _readyState = WebSocketState.Connecting;

                try
                {
                    doHandshake();
                }
                catch (Exception ex)
                {
                    _retryCountForConnect++;

                    _logger.Fatal(ex.Message);
                    _logger.Debug(ex.ToString());

                    var msg = "An exception has occurred while attempting to connect.";
                    fatal(msg, ex);

                    return false;
                }

                _retryCountForConnect = 1;
                _readyState = WebSocketState.Open;

                return true;
            }
        }

        // As client
        /// <summary>
        /// The createExtensions
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        private string createExtensions()
        {
            var buff = new StringBuilder(80);

            if (_compression != CompressionMethod.None)
            {
                var str = _compression.ToExtensionString(
                  "server_no_context_takeover", "client_no_context_takeover");

                buff.AppendFormat("{0}, ", str);
            }

            var len = buff.Length;
            if (len > 2)
            {
                buff.Length = len - 2;
                return buff.ToString();
            }

            return null;
        }

        // As server
        /// <summary>
        /// The createHandshakeFailureResponse
        /// </summary>
        /// <param name="code">The code<see cref="HttpStatusCode"/></param>
        /// <returns>The <see cref="HttpResponse"/></returns>
        private HttpResponse createHandshakeFailureResponse(HttpStatusCode code)
        {
            var ret = HttpResponse.CreateCloseResponse(code);
            ret.Headers["Sec-WebSocket-Version"] = _version;

            return ret;
        }

        // As client
        /// <summary>
        /// The createHandshakeRequest
        /// </summary>
        /// <returns>The <see cref="HttpRequest"/></returns>
        private HttpRequest createHandshakeRequest()
        {
            var ret = HttpRequest.CreateWebSocketRequest(_uri);

            var headers = ret.Headers;
            if (!_origin.IsNullOrEmpty())
                headers["Origin"] = _origin;

            headers["Sec-WebSocket-Key"] = _base64Key;

            _protocolsRequested = _protocols != null;
            if (_protocolsRequested)
                headers["Sec-WebSocket-Protocol"] = _protocols.ToString(", ");

            _extensionsRequested = _compression != CompressionMethod.None;
            if (_extensionsRequested)
                headers["Sec-WebSocket-Extensions"] = createExtensions();

            headers["Sec-WebSocket-Version"] = _version;

            AuthenticationResponse authRes = null;
            if (_authChallenge != null && _credentials != null)
            {
                authRes = new AuthenticationResponse(_authChallenge, _credentials, _nonceCount);
                _nonceCount = authRes.NonceCount;
            }
            else if (_preAuth)
            {
                authRes = new AuthenticationResponse(_credentials);
            }

            if (authRes != null)
                headers["Authorization"] = authRes.ToString();

            if (_cookies.Count > 0)
                ret.SetCookies(_cookies);

            return ret;
        }

        // As server
        /// <summary>
        /// The createHandshakeResponse
        /// </summary>
        /// <returns>The <see cref="HttpResponse"/></returns>
        private HttpResponse createHandshakeResponse()
        {
            var ret = HttpResponse.CreateWebSocketResponse();

            var headers = ret.Headers;
            headers["Sec-WebSocket-Accept"] = CreateResponseKey(_base64Key);

            if (_protocol != null)
                headers["Sec-WebSocket-Protocol"] = _protocol;

            if (_extensions != null)
                headers["Sec-WebSocket-Extensions"] = _extensions;

            if (_cookies.Count > 0)
                ret.SetCookies(_cookies);

            return ret;
        }

        // As server
        /// <summary>
        /// The customCheckHandshakeRequest
        /// </summary>
        /// <param name="context">The context<see cref="WebSocketContext"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool customCheckHandshakeRequest(
      WebSocketContext context, out string message
    )
        {
            message = null;

            if (_handshakeRequestChecker == null)
                return true;

            message = _handshakeRequestChecker(context);
            return message == null;
        }

        /// <summary>
        /// The dequeueFromMessageEventQueue
        /// </summary>
        /// <returns>The <see cref="MessageEventArgs"/></returns>
        private MessageEventArgs dequeueFromMessageEventQueue()
        {
            lock (_forMessageEventQueue)
                return _messageEventQueue.Count > 0 ? _messageEventQueue.Dequeue() : null;
        }

        // As client
        /// <summary>
        /// The doHandshake
        /// </summary>
        private void doHandshake()
        {
            setClientStream();
            var res = sendHandshakeRequest();

            string msg;
            if (!checkHandshakeResponse(res, out msg))
                throw new WebSocketException(CloseStatusCode.ProtocolError, msg);

            if (_protocolsRequested)
                _protocol = res.Headers["Sec-WebSocket-Protocol"];

            if (_extensionsRequested)
                processSecWebSocketExtensionsServerHeader(res.Headers["Sec-WebSocket-Extensions"]);

            processCookies(res.Cookies);
        }

        /// <summary>
        /// The enqueueToMessageEventQueue
        /// </summary>
        /// <param name="e">The e<see cref="MessageEventArgs"/></param>
        private void enqueueToMessageEventQueue(MessageEventArgs e)
        {
            lock (_forMessageEventQueue)
                _messageEventQueue.Enqueue(e);
        }

        /// <summary>
        /// The error
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="exception">The exception<see cref="Exception"/></param>
        private void error(string message, Exception exception)
        {
            try
            {
                OnError.Emit(this, new ErrorEventArgs(message, exception));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Debug(ex.ToString());
            }
        }

        /// <summary>
        /// The fatal
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        private void fatal(string message, CloseStatusCode code)
        {
            fatal(message, (ushort)code);
        }

        /// <summary>
        /// The fatal
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="exception">The exception<see cref="Exception"/></param>
        private void fatal(string message, Exception exception)
        {
            var code = exception is WebSocketException
                       ? ((WebSocketException)exception).Code
                       : CloseStatusCode.Abnormal;

            fatal(message, (ushort)code);
        }

        /// <summary>
        /// The fatal
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="code">The code<see cref="ushort"/></param>
        private void fatal(string message, ushort code)
        {
            var payload = new PayloadData(code, message);
            close(payload, !code.IsReserved(), false, false);
        }

        /// <summary>
        /// The getSslConfiguration
        /// </summary>
        /// <returns>The <see cref="ClientSslConfiguration"/></returns>
        private ClientSslConfiguration getSslConfiguration()
        {
            if (_sslConfig == null)
                _sslConfig = new ClientSslConfiguration(_uri.DnsSafeHost);

            return _sslConfig;
        }

        /// <summary>
        /// The init
        /// </summary>
        private void init()
        {
            _compression = CompressionMethod.None;
            _cookies = new CookieCollection();
            _forPing = new object();
            _forSend = new object();
            _forState = new object();
            _messageEventQueue = new Queue<MessageEventArgs>();
            _forMessageEventQueue = ((ICollection)_messageEventQueue).SyncRoot;
            _readyState = WebSocketState.Connecting;
        }

        /// <summary>
        /// The message
        /// </summary>
        private void message()
        {
            MessageEventArgs e = null;
            lock (_forMessageEventQueue)
            {
                if (_inMessage || _messageEventQueue.Count == 0 || _readyState != WebSocketState.Open)
                    return;

                _inMessage = true;
                e = _messageEventQueue.Dequeue();
            }

            _message(e);
        }

        /// <summary>
        /// The messagec
        /// </summary>
        /// <param name="e">The e<see cref="MessageEventArgs"/></param>
        private void messagec(MessageEventArgs e)
        {
            do
            {
                try
                {
                    OnMessage.Emit(this, e);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                    error("An error has occurred during an OnMessage event.", ex);
                }

                lock (_forMessageEventQueue)
                {
                    if (_messageEventQueue.Count == 0 || _readyState != WebSocketState.Open)
                    {
                        _inMessage = false;
                        break;
                    }

                    e = _messageEventQueue.Dequeue();
                }
            }
            while (true);
        }

        /// <summary>
        /// The messages
        /// </summary>
        /// <param name="e">The e<see cref="MessageEventArgs"/></param>
        private void messages(MessageEventArgs e)
        {
            try
            {
                OnMessage.Emit(this, e);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                error("An error has occurred during an OnMessage event.", ex);
            }

            lock (_forMessageEventQueue)
            {
                if (_messageEventQueue.Count == 0 || _readyState != WebSocketState.Open)
                {
                    _inMessage = false;
                    return;
                }

                e = _messageEventQueue.Dequeue();
            }

            ThreadPool.QueueUserWorkItem(state => messages(e));
        }

        /// <summary>
        /// The open
        /// </summary>
        private void open()
        {
            _inMessage = true;
            startReceiving();
            try
            {
                OnOpen.Emit(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                error("An error has occurred during the OnOpen event.", ex);
            }

            MessageEventArgs e = null;
            lock (_forMessageEventQueue)
            {
                if (_messageEventQueue.Count == 0 || _readyState != WebSocketState.Open)
                {
                    _inMessage = false;
                    return;
                }

                e = _messageEventQueue.Dequeue();
            }

            _message.BeginInvoke(e, ar => _message.EndInvoke(ar), null);
        }

        /// <summary>
        /// The ping
        /// </summary>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool ping(byte[] data)
        {
            if (_readyState != WebSocketState.Open)
                return false;

            var pongReceived = _pongReceived;
            if (pongReceived == null)
                return false;

            lock (_forPing)
            {
                try
                {
                    pongReceived.Reset();
                    if (!send(Fin.Final, Opcode.Ping, data, false))
                        return false;

                    return pongReceived.WaitOne(_waitTime);
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// The processCloseFrame
        /// </summary>
        /// <param name="frame">The frame<see cref="WebSocketFrame"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool processCloseFrame(WebSocketFrame frame)
        {
            var payload = frame.PayloadData;
            close(payload, !payload.HasReservedCode, false, true);

            return false;
        }

        // As client
        /// <summary>
        /// The processCookies
        /// </summary>
        /// <param name="cookies">The cookies<see cref="CookieCollection"/></param>
        private void processCookies(CookieCollection cookies)
        {
            if (cookies.Count == 0)
                return;

            _cookies.SetOrRemove(cookies);
        }

        /// <summary>
        /// The processDataFrame
        /// </summary>
        /// <param name="frame">The frame<see cref="WebSocketFrame"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool processDataFrame(WebSocketFrame frame)
        {
            enqueueToMessageEventQueue(
              frame.IsCompressed
              ? new MessageEventArgs(
                  frame.Opcode, frame.PayloadData.ApplicationData.Decompress(_compression))
              : new MessageEventArgs(frame));

            return true;
        }

        /// <summary>
        /// The processFragmentFrame
        /// </summary>
        /// <param name="frame">The frame<see cref="WebSocketFrame"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool processFragmentFrame(WebSocketFrame frame)
        {
            if (!_inContinuation)
            {
                // Must process first fragment.
                if (frame.IsContinuation)
                    return true;

                _fragmentsOpcode = frame.Opcode;
                _fragmentsCompressed = frame.IsCompressed;
                _fragmentsBuffer = new MemoryStream();
                _inContinuation = true;
            }

            _fragmentsBuffer.WriteBytes(frame.PayloadData.ApplicationData, 1024);
            if (frame.IsFinal)
            {
                using (_fragmentsBuffer)
                {
                    var data = _fragmentsCompressed
                               ? _fragmentsBuffer.DecompressToArray(_compression)
                               : _fragmentsBuffer.ToArray();

                    enqueueToMessageEventQueue(new MessageEventArgs(_fragmentsOpcode, data));
                }

                _fragmentsBuffer = null;
                _inContinuation = false;
            }

            return true;
        }

        /// <summary>
        /// The processPingFrame
        /// </summary>
        /// <param name="frame">The frame<see cref="WebSocketFrame"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool processPingFrame(WebSocketFrame frame)
        {
            _logger.Trace("A ping was received.");

            var pong = WebSocketFrame.CreatePongFrame(frame.PayloadData, _client);

            lock (_forState)
            {
                if (_readyState != WebSocketState.Open)
                {
                    _logger.Error("The connection is closing.");
                    return true;
                }

                if (!sendBytes(pong.ToArray()))
                    return false;
            }

            _logger.Trace("A pong to this ping has been sent.");

            if (_emitOnPing)
            {
                if (_client)
                    pong.Unmask();

                enqueueToMessageEventQueue(new MessageEventArgs(frame));
            }

            return true;
        }

        /// <summary>
        /// The processPongFrame
        /// </summary>
        /// <param name="frame">The frame<see cref="WebSocketFrame"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool processPongFrame(WebSocketFrame frame)
        {
            _logger.Trace("A pong was received.");

            try
            {
                _pongReceived.Set();
            }
            catch (NullReferenceException ex)
            {
                _logger.Error(ex.Message);
                _logger.Debug(ex.ToString());

                return false;
            }
            catch (ObjectDisposedException ex)
            {
                _logger.Error(ex.Message);
                _logger.Debug(ex.ToString());

                return false;
            }

            _logger.Trace("It has been signaled.");

            return true;
        }

        /// <summary>
        /// The processReceivedFrame
        /// </summary>
        /// <param name="frame">The frame<see cref="WebSocketFrame"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool processReceivedFrame(WebSocketFrame frame)
        {
            string msg;
            if (!checkReceivedFrame(frame, out msg))
                throw new WebSocketException(CloseStatusCode.ProtocolError, msg);

            frame.Unmask();
            return frame.IsFragment
                   ? processFragmentFrame(frame)
                   : frame.IsData
                     ? processDataFrame(frame)
                     : frame.IsPing
                       ? processPingFrame(frame)
                       : frame.IsPong
                         ? processPongFrame(frame)
                         : frame.IsClose
                           ? processCloseFrame(frame)
                           : processUnsupportedFrame(frame);
        }

        // As server
        /// <summary>
        /// The processSecWebSocketExtensionsClientHeader
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        private void processSecWebSocketExtensionsClientHeader(string value)
        {
            if (value == null)
                return;

            var buff = new StringBuilder(80);
            var comp = false;

            foreach (var elm in value.SplitHeaderValue(','))
            {
                var extension = elm.Trim();
                if (extension.Length == 0)
                    continue;

                if (!comp)
                {
                    if (extension.IsCompressionExtension(CompressionMethod.Deflate))
                    {
                        _compression = CompressionMethod.Deflate;

                        buff.AppendFormat(
                          "{0}, ",
                          _compression.ToExtensionString(
                            "client_no_context_takeover", "server_no_context_takeover"
                          )
                        );

                        comp = true;
                    }
                }
            }

            var len = buff.Length;
            if (len <= 2)
                return;

            buff.Length = len - 2;
            _extensions = buff.ToString();
        }

        // As client
        /// <summary>
        /// The processSecWebSocketExtensionsServerHeader
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        private void processSecWebSocketExtensionsServerHeader(string value)
        {
            if (value == null)
            {
                _compression = CompressionMethod.None;
                return;
            }

            _extensions = value;
        }

        // As server
        /// <summary>
        /// The processSecWebSocketProtocolClientHeader
        /// </summary>
        /// <param name="values">The values<see cref="IEnumerable{string}"/></param>
        private void processSecWebSocketProtocolClientHeader(
      IEnumerable<string> values
    )
        {
            if (values.Contains(val => val == _protocol))
                return;

            _protocol = null;
        }

        /// <summary>
        /// The processUnsupportedFrame
        /// </summary>
        /// <param name="frame">The frame<see cref="WebSocketFrame"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool processUnsupportedFrame(WebSocketFrame frame)
        {
            _logger.Fatal("An unsupported frame:" + frame.PrintToString(false));
            fatal("There is no way to handle it.", CloseStatusCode.PolicyViolation);

            return false;
        }

        // As server
        /// <summary>
        /// The refuseHandshake
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        private void refuseHandshake(CloseStatusCode code, string reason)
        {
            _readyState = WebSocketState.Closing;

            var res = createHandshakeFailureResponse(HttpStatusCode.BadRequest);
            sendHttpResponse(res);

            releaseServerResources();

            _readyState = WebSocketState.Closed;

            var e = new CloseEventArgs(code, reason);

            try
            {
                OnClose.Emit(this, e);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Debug(ex.ToString());
            }
        }

        // As client
        /// <summary>
        /// The releaseClientResources
        /// </summary>
        private void releaseClientResources()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;
            }
        }

        /// <summary>
        /// The releaseCommonResources
        /// </summary>
        private void releaseCommonResources()
        {
            if (_fragmentsBuffer != null)
            {
                _fragmentsBuffer.Dispose();
                _fragmentsBuffer = null;
                _inContinuation = false;
            }

            if (_pongReceived != null)
            {
                _pongReceived.Close();
                _pongReceived = null;
            }

            if (_receivingExited != null)
            {
                _receivingExited.Close();
                _receivingExited = null;
            }
        }

        /// <summary>
        /// The releaseResources
        /// </summary>
        private void releaseResources()
        {
            if (_client)
                releaseClientResources();
            else
                releaseServerResources();

            releaseCommonResources();
        }

        // As server
        /// <summary>
        /// The releaseServerResources
        /// </summary>
        private void releaseServerResources()
        {
            if (_closeContext == null)
                return;

            _closeContext();
            _closeContext = null;
            _stream = null;
            _context = null;
        }

        /// <summary>
        /// The send
        /// </summary>
        /// <param name="fin">The fin<see cref="Fin"/></param>
        /// <param name="opcode">The opcode<see cref="Opcode"/></param>
        /// <param name="data">The data<see cref="byte[]"/></param>
        /// <param name="compressed">The compressed<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool send(Fin fin, Opcode opcode, byte[] data, bool compressed)
        {
            lock (_forState)
            {
                if (_readyState != WebSocketState.Open)
                {
                    _logger.Error("The connection is closing.");
                    return false;
                }

                var frame = new WebSocketFrame(fin, opcode, data, compressed, _client);
                return sendBytes(frame.ToArray());
            }
        }

        /// <summary>
        /// The send
        /// </summary>
        /// <param name="opcode">The opcode<see cref="Opcode"/></param>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool send(Opcode opcode, Stream stream)
        {
            lock (_forSend)
            {
                var src = stream;
                var compressed = false;
                var sent = false;
                try
                {
                    if (_compression != CompressionMethod.None)
                    {
                        stream = stream.Compress(_compression);
                        compressed = true;
                    }

                    sent = send(opcode, stream, compressed);
                    if (!sent)
                        error("A send has been interrupted.", null);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                    error("An error has occurred during a send.", ex);
                }
                finally
                {
                    if (compressed)
                        stream.Dispose();

                    src.Dispose();
                }

                return sent;
            }
        }

        /// <summary>
        /// The send
        /// </summary>
        /// <param name="opcode">The opcode<see cref="Opcode"/></param>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        /// <param name="compressed">The compressed<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool send(Opcode opcode, Stream stream, bool compressed)
        {
            var len = stream.Length;
            if (len == 0)
                return send(Fin.Final, opcode, EmptyBytes, false);

            var quo = len / FragmentLength;
            var rem = (int)(len % FragmentLength);

            byte[] buff = null;
            if (quo == 0)
            {
                buff = new byte[rem];
                return stream.Read(buff, 0, rem) == rem
                       && send(Fin.Final, opcode, buff, compressed);
            }

            if (quo == 1 && rem == 0)
            {
                buff = new byte[FragmentLength];
                return stream.Read(buff, 0, FragmentLength) == FragmentLength
                       && send(Fin.Final, opcode, buff, compressed);
            }

            /* Send fragments */

            // Begin
            buff = new byte[FragmentLength];
            var sent = stream.Read(buff, 0, FragmentLength) == FragmentLength
                       && send(Fin.More, opcode, buff, compressed);

            if (!sent)
                return false;

            var n = rem == 0 ? quo - 2 : quo - 1;
            for (long i = 0; i < n; i++)
            {
                sent = stream.Read(buff, 0, FragmentLength) == FragmentLength
                       && send(Fin.More, Opcode.Cont, buff, false);

                if (!sent)
                    return false;
            }

            // End
            if (rem == 0)
                rem = FragmentLength;
            else
                buff = new byte[rem];

            return stream.Read(buff, 0, rem) == rem
                   && send(Fin.Final, Opcode.Cont, buff, false);
        }

        /// <summary>
        /// The sendAsync
        /// </summary>
        /// <param name="opcode">The opcode<see cref="Opcode"/></param>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        /// <param name="completed">The completed<see cref="Action{bool}"/></param>
        private void sendAsync(Opcode opcode, Stream stream, Action<bool> completed)
        {
            Func<Opcode, Stream, bool> sender = send;
            sender.BeginInvoke(
              opcode,
              stream,
              ar =>
              {
                  try
                  {
                      var sent = sender.EndInvoke(ar);
                      if (completed != null)
                          completed(sent);
                  }
                  catch (Exception ex)
                  {
                      _logger.Error(ex.ToString());
                      error(
                  "An error has occurred during the callback for an async send.",
                  ex
                );
                  }
              },
              null
            );
        }

        /// <summary>
        /// The sendBytes
        /// </summary>
        /// <param name="bytes">The bytes<see cref="byte[]"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool sendBytes(byte[] bytes)
        {
            try
            {
                _stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Debug(ex.ToString());

                return false;
            }

            return true;
        }

        // As client
        /// <summary>
        /// The sendHandshakeRequest
        /// </summary>
        /// <returns>The <see cref="HttpResponse"/></returns>
        private HttpResponse sendHandshakeRequest()
        {
            var req = createHandshakeRequest();
            var res = sendHttpRequest(req, 90000);
            if (res.IsUnauthorized)
            {
                var chal = res.Headers["WWW-Authenticate"];
                _logger.Warn(String.Format("Received an authentication requirement for '{0}'.", chal));
                if (chal.IsNullOrEmpty())
                {
                    _logger.Error("No authentication challenge is specified.");
                    return res;
                }

                _authChallenge = AuthenticationChallenge.Parse(chal);
                if (_authChallenge == null)
                {
                    _logger.Error("An invalid authentication challenge is specified.");
                    return res;
                }

                if (_credentials != null &&
                    (!_preAuth || _authChallenge.Scheme == AuthenticationSchemes.Digest))
                {
                    if (res.HasConnectionClose)
                    {
                        releaseClientResources();
                        setClientStream();
                    }

                    var authRes = new AuthenticationResponse(_authChallenge, _credentials, _nonceCount);
                    _nonceCount = authRes.NonceCount;
                    req.Headers["Authorization"] = authRes.ToString();
                    res = sendHttpRequest(req, 15000);
                }
            }

            if (res.IsRedirect)
            {
                var url = res.Headers["Location"];
                _logger.Warn(String.Format("Received a redirection to '{0}'.", url));
                if (_enableRedirection)
                {
                    if (url.IsNullOrEmpty())
                    {
                        _logger.Error("No url to redirect is located.");
                        return res;
                    }

                    Uri uri;
                    string msg;
                    if (!url.TryCreateWebSocketUri(out uri, out msg))
                    {
                        _logger.Error("An invalid url to redirect is located: " + msg);
                        return res;
                    }

                    releaseClientResources();

                    _uri = uri;
                    _secure = uri.Scheme == "wss";

                    setClientStream();
                    return sendHandshakeRequest();
                }
            }

            return res;
        }

        // As client
        /// <summary>
        /// The sendHttpRequest
        /// </summary>
        /// <param name="request">The request<see cref="HttpRequest"/></param>
        /// <param name="millisecondsTimeout">The millisecondsTimeout<see cref="int"/></param>
        /// <returns>The <see cref="HttpResponse"/></returns>
        private HttpResponse sendHttpRequest(HttpRequest request, int millisecondsTimeout)
        {
            _logger.Debug("A request to the server:\n" + request.ToString());
            var res = request.GetResponse(_stream, millisecondsTimeout);
            _logger.Debug("A response to this request:\n" + res.ToString());

            return res;
        }

        // As server
        /// <summary>
        /// The sendHttpResponse
        /// </summary>
        /// <param name="response">The response<see cref="HttpResponse"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool sendHttpResponse(HttpResponse response)
        {
            _logger.Debug(
              String.Format(
                "A response to {0}:\n{1}", _context.UserEndPoint, response
              )
            );

            return sendBytes(response.ToByteArray());
        }

        // As client
        /// <summary>
        /// The sendProxyConnectRequest
        /// </summary>
        private void sendProxyConnectRequest()
        {
            var req = HttpRequest.CreateConnectRequest(_uri);
            var res = sendHttpRequest(req, 90000);
            if (res.IsProxyAuthenticationRequired)
            {
                var chal = res.Headers["Proxy-Authenticate"];
                _logger.Warn(
                  String.Format("Received a proxy authentication requirement for '{0}'.", chal));

                if (chal.IsNullOrEmpty())
                    throw new WebSocketException("No proxy authentication challenge is specified.");

                var authChal = AuthenticationChallenge.Parse(chal);
                if (authChal == null)
                    throw new WebSocketException("An invalid proxy authentication challenge is specified.");

                if (_proxyCredentials != null)
                {
                    if (res.HasConnectionClose)
                    {
                        releaseClientResources();
                        _tcpClient = new TcpClient(_proxyUri.DnsSafeHost, _proxyUri.Port);
                        _stream = _tcpClient.GetStream();
                    }

                    var authRes = new AuthenticationResponse(authChal, _proxyCredentials, 0);
                    req.Headers["Proxy-Authorization"] = authRes.ToString();
                    res = sendHttpRequest(req, 15000);
                }

                if (res.IsProxyAuthenticationRequired)
                    throw new WebSocketException("A proxy authentication is required.");
            }

            if (res.StatusCode[0] != '2')
                throw new WebSocketException(
                  "The proxy has failed a connection to the requested host and port.");
        }

        // As client
        /// <summary>
        /// The setClientStream
        /// </summary>
        private void setClientStream()
        {
            if (_proxyUri != null)
            {
                _tcpClient = new TcpClient(_proxyUri.DnsSafeHost, _proxyUri.Port);
                _stream = _tcpClient.GetStream();
                sendProxyConnectRequest();
            }
            else
            {
                _tcpClient = new TcpClient(_uri.DnsSafeHost, _uri.Port);
                _stream = _tcpClient.GetStream();
            }

            if (_secure)
            {
                var conf = getSslConfiguration();
                var host = conf.TargetHost;
                if (host != _uri.DnsSafeHost)
                    throw new WebSocketException(
                      CloseStatusCode.TlsHandshakeFailure, "An invalid host name is specified.");

                try
                {
                    var sslStream = new SslStream(
                      _stream,
                      false,
                      conf.ServerCertificateValidationCallback,
                      conf.ClientCertificateSelectionCallback);

                    sslStream.AuthenticateAsClient(
                      host,
                      conf.ClientCertificates,
                      conf.EnabledSslProtocols,
                      conf.CheckCertificateRevocation);

                    _stream = sslStream;
                }
                catch (Exception ex)
                {
                    throw new WebSocketException(CloseStatusCode.TlsHandshakeFailure, ex);
                }
            }
        }

        /// <summary>
        /// The startReceiving
        /// </summary>
        private void startReceiving()
        {
            if (_messageEventQueue.Count > 0)
                _messageEventQueue.Clear();

            _pongReceived = new ManualResetEvent(false);
            _receivingExited = new ManualResetEvent(false);

            Action receive = null;
            receive =
              () =>
                WebSocketFrame.ReadFrameAsync(
                  _stream,
                  false,
                  frame =>
                  {
                      if (!processReceivedFrame(frame) || _readyState == WebSocketState.Closed)
                      {
                          var exited = _receivingExited;
                          if (exited != null)
                              exited.Set();

                          return;
                      }

                      // Receive next asap because the Ping or Close needs a response to it.
                      receive();

                      if (_inMessage || !HasMessage || _readyState != WebSocketState.Open)
                          return;

                      message();
                  },
                  ex =>
                  {
                      _logger.Fatal(ex.ToString());
                      fatal("An exception has occurred while receiving.", ex);
                  }
                );

            receive();
        }

        // As client
        /// <summary>
        /// The validateSecWebSocketAcceptHeader
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool validateSecWebSocketAcceptHeader(string value)
        {
            return value != null && value == CreateResponseKey(_base64Key);
        }

        // As client
        /// <summary>
        /// The validateSecWebSocketExtensionsServerHeader
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool validateSecWebSocketExtensionsServerHeader(string value)
        {
            if (value == null)
                return true;

            if (value.Length == 0)
                return false;

            if (!_extensionsRequested)
                return false;

            var comp = _compression != CompressionMethod.None;
            foreach (var e in value.SplitHeaderValue(','))
            {
                var ext = e.Trim();
                if (comp && ext.IsCompressionExtension(_compression))
                {
                    if (!ext.Contains("server_no_context_takeover"))
                    {
                        _logger.Error("The server hasn't sent back 'server_no_context_takeover'.");
                        return false;
                    }

                    if (!ext.Contains("client_no_context_takeover"))
                        _logger.Warn("The server hasn't sent back 'client_no_context_takeover'.");

                    var method = _compression.ToExtensionString();
                    var invalid =
                      ext.SplitHeaderValue(';').Contains(
                        t =>
                        {
                            t = t.Trim();
                            return t != method
                         && t != "server_no_context_takeover"
                         && t != "client_no_context_takeover";
                        }
                      );

                    if (invalid)
                        return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        // As client
        /// <summary>
        /// The validateSecWebSocketProtocolServerHeader
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool validateSecWebSocketProtocolServerHeader(string value)
        {
            if (value == null)
                return !_protocolsRequested;

            if (value.Length == 0)
                return false;

            return _protocolsRequested && _protocols.Contains(p => p == value);
        }

        // As client
        /// <summary>
        /// The validateSecWebSocketVersionServerHeader
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool validateSecWebSocketVersionServerHeader(string value)
        {
            return value == null || value == _version;
        }

        /// <summary>
        /// Closes the connection and releases all associated resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            close(1001, String.Empty);
        }

        #endregion 方法
    }
}