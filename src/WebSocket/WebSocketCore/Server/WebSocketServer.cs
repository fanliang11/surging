/*
 * WebSocketServer.cs
 *
 * The MIT License
 *
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
 * Contributors:
 * - Juan Manuel Lallana <juan.manuel.lallana@gmail.com>
 * - Jonas Hovgaard <j@jhovgaard.dk>
 * - Liryna <liryna.stark@gmail.com>
 * - Rohan Singh <rohan-singh@hotmail.com>
 */

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using WebSocketCore.Net;
using WebSocketCore.Net.WebSockets;

namespace WebSocketCore.Server
{
    /// <summary>
    /// Provides a WebSocket protocol server.
    /// </summary>
    public class WebSocketServer
    {
        #region 字段

        /// <summary>
        /// Defines the _defaultRealm
        /// </summary>
        private static readonly string _defaultRealm;

        /// <summary>
        /// Defines the _address
        /// </summary>
        private System.Net.IPAddress _address;

        /// <summary>
        /// Defines the _allowForwardedRequest
        /// </summary>
        private bool _allowForwardedRequest;

        /// <summary>
        /// Defines the _authSchemes
        /// </summary>
        private AuthenticationSchemes _authSchemes;

        /// <summary>
        /// Defines the _dnsStyle
        /// </summary>
        private bool _dnsStyle;

        /// <summary>
        /// Defines the _hostname
        /// </summary>
        private string _hostname;

        /// <summary>
        /// Defines the _listener
        /// </summary>
        private TcpListener _listener;

        /// <summary>
        /// Defines the _log
        /// </summary>
        private Logger _log;

        /// <summary>
        /// Defines the _port
        /// </summary>
        private int _port;

        /// <summary>
        /// Defines the _realm
        /// </summary>
        private string _realm;

        /// <summary>
        /// Defines the _realmInUse
        /// </summary>
        private string _realmInUse;

        /// <summary>
        /// Defines the _receiveThread
        /// </summary>
        private Thread _receiveThread;

        /// <summary>
        /// Defines the _reuseAddress
        /// </summary>
        private bool _reuseAddress;

        /// <summary>
        /// Defines the _secure
        /// </summary>
        private bool _secure;

        /// <summary>
        /// Defines the _services
        /// </summary>
        private WebSocketServiceManager _services;

        /// <summary>
        /// Defines the _sslConfig
        /// </summary>
        private ServerSslConfiguration _sslConfig;

        /// <summary>
        /// Defines the _sslConfigInUse
        /// </summary>
        private ServerSslConfiguration _sslConfigInUse;

        /// <summary>
        /// Defines the _state
        /// </summary>
        private volatile ServerState _state;

        /// <summary>
        /// Defines the _sync
        /// </summary>
        private object _sync;

        /// <summary>
        /// Defines the _userCredFinder
        /// </summary>
        private Func<IIdentity, NetworkCredential> _userCredFinder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer"/> class.
        /// </summary>
        public WebSocketServer()
        {
            var addr = System.Net.IPAddress.Any;
            init(addr.ToString(), addr, 80, false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer"/> class.
        /// </summary>
        /// <param name="port">The port<see cref="int"/></param>
        public WebSocketServer(int port)
      : this(port, port == 443)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer"/> class.
        /// </summary>
        /// <param name="port">The port<see cref="int"/></param>
        /// <param name="secure">The secure<see cref="bool"/></param>
        public WebSocketServer(int port, bool secure)
        {
            if (!port.IsPortNumber())
            {
                var msg = "Less than 1 or greater than 65535.";
                throw new ArgumentOutOfRangeException("port", msg);
            }

            var addr = System.Net.IPAddress.Any;
            init(addr.ToString(), addr, port, secure);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer"/> class.
        /// </summary>
        /// <param name="url">The url<see cref="string"/></param>
        public WebSocketServer(string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            if (url.Length == 0)
                throw new ArgumentException("An empty string.", "url");

            Uri uri;
            string msg;
            if (!tryCreateUri(url, out uri, out msg))
                throw new ArgumentException(msg, "url");

            var host = uri.DnsSafeHost;

            var addr = host.ToIPAddress();
            if (addr == null)
            {
                msg = "The host part could not be converted to an IP address.";
                throw new ArgumentException(msg, "url");
            }

            if (!addr.IsLocal())
            {
                msg = "The IP address of the host is not a local IP address.";
                throw new ArgumentException(msg, "url");
            }

            init(host, addr, uri.Port, uri.Scheme == "wss");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer"/> class.
        /// </summary>
        /// <param name="address">The address<see cref="System.Net.IPAddress"/></param>
        /// <param name="port">The port<see cref="int"/></param>
        public WebSocketServer(System.Net.IPAddress address, int port)
      : this(address, port, port == 443)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServer"/> class.
        /// </summary>
        /// <param name="address">The address<see cref="System.Net.IPAddress"/></param>
        /// <param name="port">The port<see cref="int"/></param>
        /// <param name="secure">The secure<see cref="bool"/></param>
        public WebSocketServer(System.Net.IPAddress address, int port, bool secure)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            if (!address.IsLocal())
                throw new ArgumentException("Not a local IP address.", "address");

            if (!port.IsPortNumber())
            {
                var msg = "Less than 1 or greater than 65535.";
                throw new ArgumentOutOfRangeException("port", msg);
            }

            init(address.ToString(), address, port, secure);
        }

        /// <summary>
        /// Initializes static members of the <see cref="WebSocketServer"/> class.
        /// </summary>
        static WebSocketServer()
        {
            _defaultRealm = "SECRET AREA";
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the IP address of the server.
        /// </summary>
        public System.Net.IPAddress Address
        {
            get
            {
                return _address;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server accepts every
        /// handshake request without checking the request URI.
        /// </summary>
        public bool AllowForwardedRequest
        {
            get
            {
                return _allowForwardedRequest;
            }

            set
            {
                string msg;
                if (!canSet(out msg))
                {
                    _log.Warn(msg);
                    return;
                }

                lock (_sync)
                {
                    if (!canSet(out msg))
                    {
                        _log.Warn(msg);
                        return;
                    }

                    _allowForwardedRequest = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the scheme used to authenticate the clients.
        /// </summary>
        public AuthenticationSchemes AuthenticationSchemes
        {
            get
            {
                return _authSchemes;
            }

            set
            {
                string msg;
                if (!canSet(out msg))
                {
                    _log.Warn(msg);
                    return;
                }

                lock (_sync)
                {
                    if (!canSet(out msg))
                    {
                        _log.Warn(msg);
                        return;
                    }

                    _authSchemes = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the server has started.
        /// </summary>
        public bool IsListening
        {
            get
            {
                return _state == ServerState.Start;
            }
        }

        /// <summary>
        /// Gets a value indicating whether secure connections are provided.
        /// </summary>
        public bool IsSecure
        {
            get
            {
                return _secure;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server cleans up
        /// the inactive sessions periodically.
        /// </summary>
        public bool KeepClean
        {
            get
            {
                return _services.KeepClean;
            }

            set
            {
                _services.KeepClean = value;
            }
        }

        /// <summary>
        /// Gets the logging function for the server.
        /// </summary>
        public Logger Log
        {
            get
            {
                return _log;
            }
        }

        /// <summary>
        /// Gets the port of the server.
        /// </summary>
        public int Port
        {
            get
            {
                return _port;
            }
        }

        /// <summary>
        /// Gets or sets the realm used for authentication.
        /// </summary>
        public string Realm
        {
            get
            {
                return _realm;
            }

            set
            {
                string msg;
                if (!canSet(out msg))
                {
                    _log.Warn(msg);
                    return;
                }

                lock (_sync)
                {
                    if (!canSet(out msg))
                    {
                        _log.Warn(msg);
                        return;
                    }

                    _realm = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server is allowed to
        /// be bound to an address that is already in use.
        /// </summary>
        public bool ReuseAddress
        {
            get
            {
                return _reuseAddress;
            }

            set
            {
                string msg;
                if (!canSet(out msg))
                {
                    _log.Warn(msg);
                    return;
                }

                lock (_sync)
                {
                    if (!canSet(out msg))
                    {
                        _log.Warn(msg);
                        return;
                    }

                    _reuseAddress = value;
                }
            }
        }

        /// <summary>
        /// Gets the configuration for secure connections.
        /// </summary>
        public ServerSslConfiguration SslConfiguration
        {
            get
            {
                if (!_secure)
                {
                    var msg = "This instance does not provide secure connections.";
                    throw new InvalidOperationException(msg);
                }

                return getSslConfiguration();
            }
        }

        /// <summary>
        /// Gets or sets the delegate used to find the credentials
        /// for an identity.
        /// </summary>
        public Func<IIdentity, NetworkCredential> UserCredentialsFinder
        {
            get
            {
                return _userCredFinder;
            }

            set
            {
                string msg;
                if (!canSet(out msg))
                {
                    _log.Warn(msg);
                    return;
                }

                lock (_sync)
                {
                    if (!canSet(out msg))
                    {
                        _log.Warn(msg);
                        return;
                    }

                    _userCredFinder = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the time to wait for the response to the WebSocket Ping or
        /// Close.
        /// </summary>
        public TimeSpan WaitTime
        {
            get
            {
                return _services.WaitTime;
            }

            set
            {
                _services.WaitTime = value;
            }
        }

        /// <summary>
        /// Gets the management function for the WebSocket services
        /// provided by the server.
        /// </summary>
        public WebSocketServiceManager WebSocketServices
        {
            get
            {
                return _services;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Adds a WebSocket service with the specified behavior,
        /// <paramref name="path"/>, and <paramref name="initializer"/>.
        /// </summary>
        /// <typeparam name="TBehaviorWithNew"></typeparam>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="initializer">The initializer<see cref="Action{TBehaviorWithNew}"/></param>
        public void AddWebSocketService<TBehaviorWithNew>(
      string path, Action<TBehaviorWithNew> initializer
    )
      where TBehaviorWithNew : WebSocketBehavior, new()
        {
            _services.AddService<TBehaviorWithNew>(path, initializer);
        }

        /// <summary>
        /// Adds a WebSocket service with the specified behavior,
        /// <paramref name="path"/>, and <paramref name="creator"/>.
        /// </summary>
        /// <typeparam name="TBehavior"></typeparam>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="creator">The creator<see cref="Func{TBehavior}"/></param>
        public void AddWebSocketService<TBehavior>(
      string path, Func<TBehavior> creator
    )
      where TBehavior : WebSocketBehavior
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (creator == null)
                throw new ArgumentNullException("creator");

            if (path.Length == 0)
                throw new ArgumentException("An empty string.", "path");

            if (path[0] != '/')
                throw new ArgumentException("Not an absolute path.", "path");

            if (path.IndexOfAny(new[] { '?', '#' }) > -1)
            {
                var msg = "It includes either or both query and fragment components.";
                throw new ArgumentException(msg, "path");
            }

            _services.Add<TBehavior>(path, creator);
        }

        /// <summary>
        /// Adds a WebSocket service with the specified behavior and
        /// <paramref name="path"/>.
        /// </summary>
        /// <typeparam name="TBehaviorWithNew"></typeparam>
        /// <param name="path">The path<see cref="string"/></param>
        public void AddWebSocketService<TBehaviorWithNew>(string path)
      where TBehaviorWithNew : WebSocketBehavior, new()
        {
            _services.AddService<TBehaviorWithNew>(path, null);
        }

        /// <summary>
        /// The AddWebSocketService
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="webSocketBehavior">The webSocketBehavior<see cref="WebSocketBehavior"/></param>
        public void AddWebSocketService(string path, WebSocketBehavior webSocketBehavior)
        {
            _services.AddService(path, webSocketBehavior);
        }

        /// <summary>
        /// Removes a WebSocket service with the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool RemoveWebSocketService(string path)
        {
            return _services.RemoveService(path);
        }

        /// <summary>
        /// Starts receiving incoming handshake requests.
        /// </summary>
        public void Start()
        {
            ServerSslConfiguration sslConfig = null;

            if (_secure)
            {
                sslConfig = new ServerSslConfiguration(getSslConfiguration());

                string msg;
                if (!checkSslConfiguration(sslConfig, out msg))
                    throw new InvalidOperationException(msg);
            }

            start(sslConfig);
        }

        /// <summary>
        /// Stops receiving incoming handshake requests and closes
        /// each connection.
        /// </summary>
        public void Stop()
        {
            stop(1005, String.Empty);
        }

        /// <summary>
        /// Stops receiving incoming handshake requests and closes each
        /// connection with the specified <paramref name="code"/> and
        /// <paramref name="reason"/>.
        /// </summary>
        /// <param name="code">The code<see cref="CloseStatusCode"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        public void Stop(CloseStatusCode code, string reason)
        {
            if (code == CloseStatusCode.MandatoryExtension)
            {
                var msg = "MandatoryExtension cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!reason.IsNullOrEmpty())
            {
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
            }

            stop((ushort)code, reason);
        }

        /// <summary>
        /// Stops receiving incoming handshake requests and closes each
        /// connection with the specified <paramref name="code"/> and
        /// <paramref name="reason"/>.
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        public void Stop(ushort code, string reason)
        {
            if (!code.IsCloseStatusCode())
            {
                var msg = "Less than 1000 or greater than 4999.";
                throw new ArgumentOutOfRangeException("code", msg);
            }

            if (code == 1010)
            {
                var msg = "1010 cannot be used.";
                throw new ArgumentException(msg, "code");
            }

            if (!reason.IsNullOrEmpty())
            {
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
            }

            stop(code, reason);
        }

        /// <summary>
        /// The checkSslConfiguration
        /// </summary>
        /// <param name="configuration">The configuration<see cref="ServerSslConfiguration"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool checkSslConfiguration(
      ServerSslConfiguration configuration, out string message
    )
        {
            message = null;

            if (configuration.ServerCertificate == null)
            {
                message = "There is no server certificate for secure connections.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// The tryCreateUri
        /// </summary>
        /// <param name="uriString">The uriString<see cref="string"/></param>
        /// <param name="result">The result<see cref="Uri"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool tryCreateUri(
      string uriString, out Uri result, out string message
    )
        {
            if (!uriString.TryCreateWebSocketUri(out result, out message))
                return false;

            if (result.PathAndQuery != "/")
            {
                result = null;
                message = "It includes either or both path and query components.";

                return false;
            }

            return true;
        }

        /// <summary>
        /// The abort
        /// </summary>
        private void abort()
        {
            lock (_sync)
            {
                if (_state != ServerState.Start)
                    return;

                _state = ServerState.ShuttingDown;
            }

            try
            {
                try
                {
                    _listener.Stop();
                }
                finally
                {
                    _services.Stop(1006, String.Empty);
                }
            }
            catch
            {
            }

            _state = ServerState.Stop;
        }

        /// <summary>
        /// The authenticateClient
        /// </summary>
        /// <param name="context">The context<see cref="TcpListenerWebSocketContext"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool authenticateClient(TcpListenerWebSocketContext context)
        {
            if (_authSchemes == AuthenticationSchemes.Anonymous)
                return true;

            if (_authSchemes == AuthenticationSchemes.None)
                return false;

            return context.Authenticate(_authSchemes, _realmInUse, _userCredFinder);
        }

        /// <summary>
        /// The canSet
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool canSet(out string message)
        {
            message = null;

            if (_state == ServerState.Start)
            {
                message = "The server has already started.";
                return false;
            }

            if (_state == ServerState.ShuttingDown)
            {
                message = "The server is shutting down.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// The checkHostNameForRequest
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool checkHostNameForRequest(string name)
        {
            return !_dnsStyle
                   || Uri.CheckHostName(name) != UriHostNameType.Dns
                   || name == _hostname;
        }

        /// <summary>
        /// The getRealm
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        private string getRealm()
        {
            var realm = _realm;
            return realm != null && realm.Length > 0 ? realm : _defaultRealm;
        }

        /// <summary>
        /// The getSslConfiguration
        /// </summary>
        /// <returns>The <see cref="ServerSslConfiguration"/></returns>
        private ServerSslConfiguration getSslConfiguration()
        {
            if (_sslConfig == null)
                _sslConfig = new ServerSslConfiguration();

            return _sslConfig;
        }

        /// <summary>
        /// The init
        /// </summary>
        /// <param name="hostname">The hostname<see cref="string"/></param>
        /// <param name="address">The address<see cref="System.Net.IPAddress"/></param>
        /// <param name="port">The port<see cref="int"/></param>
        /// <param name="secure">The secure<see cref="bool"/></param>
        private void init(
      string hostname, System.Net.IPAddress address, int port, bool secure
    )
        {
            _hostname = hostname;
            _address = address;
            _port = port;
            _secure = secure;

            _authSchemes = AuthenticationSchemes.Anonymous;
            _dnsStyle = Uri.CheckHostName(hostname) == UriHostNameType.Dns;
            _listener = new TcpListener(address, port);
            _log = new Logger();
            _services = new WebSocketServiceManager(_log);
            _sync = new object();
        }

        /// <summary>
        /// The processRequest
        /// </summary>
        /// <param name="context">The context<see cref="TcpListenerWebSocketContext"/></param>
        private void processRequest(TcpListenerWebSocketContext context)
        {
            if (!authenticateClient(context))
            {
                context.Close(HttpStatusCode.Forbidden);
                return;
            }

            var uri = context.RequestUri;
            if (uri == null)
            {
                context.Close(HttpStatusCode.BadRequest);
                return;
            }

            if (!_allowForwardedRequest)
            {
                if (uri.Port != _port)
                {
                    context.Close(HttpStatusCode.BadRequest);
                    return;
                }

                if (!checkHostNameForRequest(uri.DnsSafeHost))
                {
                    context.Close(HttpStatusCode.NotFound);
                    return;
                }
            }

            WebSocketServiceHostBase host;
            if (!_services.InternalTryGetServiceHost(uri.AbsolutePath, out host))
            {
                context.Close(HttpStatusCode.NotImplemented);
                return;
            }

            host.StartSession(context);
        }

        /// <summary>
        /// The receiveRequest
        /// </summary>
        private void receiveRequest()
        {
            while (true)
            {
                TcpClient cl = null;
                try
                {
                    cl = _listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(
                      state =>
                      {
                          try
                          {
                              var ctx = new TcpListenerWebSocketContext(
                                cl, null, _secure, _sslConfigInUse, _log
                              );

                              processRequest(ctx);
                          }
                          catch (Exception ex)
                          {
                              _log.Error(ex.Message);
                              _log.Debug(ex.ToString());

                              cl.Close();
                          }
                      }
                    );
                }
                catch (SocketException ex)
                {
                    if (_state == ServerState.ShuttingDown)
                    {
                        _log.Info("The underlying listener is stopped.");
                        break;
                    }

                    _log.Fatal(ex.Message);
                    _log.Debug(ex.ToString());

                    break;
                }
                catch (Exception ex)
                {
                    _log.Fatal(ex.Message);
                    _log.Debug(ex.ToString());

                    if (cl != null)
                        cl.Close();

                    break;
                }
            }

            if (_state != ServerState.ShuttingDown)
                abort();
        }

        /// <summary>
        /// The start
        /// </summary>
        /// <param name="sslConfig">The sslConfig<see cref="ServerSslConfiguration"/></param>
        private void start(ServerSslConfiguration sslConfig)
        {
            if (_state == ServerState.Start)
            {
                _log.Info("The server has already started.");
                return;
            }

            if (_state == ServerState.ShuttingDown)
            {
                _log.Warn("The server is shutting down.");
                return;
            }

            lock (_sync)
            {
                if (_state == ServerState.Start)
                {
                    _log.Info("The server has already started.");
                    return;
                }

                if (_state == ServerState.ShuttingDown)
                {
                    _log.Warn("The server is shutting down.");
                    return;
                }

                _sslConfigInUse = sslConfig;
                _realmInUse = getRealm();

                _services.Start();
                try
                {
                    startReceiving();
                }
                catch
                {
                    _services.Stop(1011, String.Empty);
                    throw;
                }

                _state = ServerState.Start;
            }
        }

        /// <summary>
        /// The startReceiving
        /// </summary>
        private void startReceiving()
        {
            if (_reuseAddress)
            {
                _listener.Server.SetSocketOption(
                  SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true
                );
            }

            try
            {
                _listener.Start();
            }
            catch (Exception ex)
            {
                var msg = "The underlying listener has failed to start.";
                throw new InvalidOperationException(msg, ex);
            }

            _receiveThread = new Thread(new ThreadStart(receiveRequest));
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        }

        /// <summary>
        /// The stop
        /// </summary>
        /// <param name="code">The code<see cref="ushort"/></param>
        /// <param name="reason">The reason<see cref="string"/></param>
        private void stop(ushort code, string reason)
        {
            if (_state == ServerState.Ready)
            {
                _log.Info("The server is not started.");
                return;
            }

            if (_state == ServerState.ShuttingDown)
            {
                _log.Info("The server is shutting down.");
                return;
            }

            if (_state == ServerState.Stop)
            {
                _log.Info("The server has already stopped.");
                return;
            }

            lock (_sync)
            {
                if (_state == ServerState.ShuttingDown)
                {
                    _log.Info("The server is shutting down.");
                    return;
                }

                if (_state == ServerState.Stop)
                {
                    _log.Info("The server has already stopped.");
                    return;
                }

                _state = ServerState.ShuttingDown;
            }

            try
            {
                var threw = false;
                try
                {
                    stopReceiving(5000);
                }
                catch
                {
                    threw = true;
                    throw;
                }
                finally
                {
                    try
                    {
                        _services.Stop(code, reason);
                    }
                    catch
                    {
                        if (!threw)
                            throw;
                    }
                }
            }
            finally
            {
                _state = ServerState.Stop;
            }
        }

        /// <summary>
        /// The stopReceiving
        /// </summary>
        /// <param name="millisecondsTimeout">The millisecondsTimeout<see cref="int"/></param>
        private void stopReceiving(int millisecondsTimeout)
        {
            try
            {
                _listener.Stop();
            }
            catch (Exception ex)
            {
                var msg = "The underlying listener has failed to stop.";
                throw new InvalidOperationException(msg, ex);
            }

            _receiveThread.Join(millisecondsTimeout);
        }

        #endregion 方法
    }
}