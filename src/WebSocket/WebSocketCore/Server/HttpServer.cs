/*
 * HttpServer.cs
 *
 * A simple HTTP server that allows to accept WebSocket handshake requests.
 *
 * The MIT License
 *
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
 * Contributors:
 * - Juan Manuel Lallana <juan.manuel.lallana@gmail.com>
 * - Liryna <liryna.stark@gmail.com>
 * - Rohan Singh <rohan-singh@hotmail.com>
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Provides a simple HTTP server that allows to accept
    /// WebSocket handshake requests.
    /// </summary>
    public class HttpServer
    {
        #region 字段

        /// <summary>
        /// Defines the _address
        /// </summary>
        private System.Net.IPAddress _address;

        /// <summary>
        /// Defines the _docRootPath
        /// </summary>
        private string _docRootPath;

        /// <summary>
        /// Defines the _hostname
        /// </summary>
        private string _hostname;

        /// <summary>
        /// Defines the _listener
        /// </summary>
        private HttpListener _listener;

        /// <summary>
        /// Defines the _log
        /// </summary>
        private Logger _log;

        /// <summary>
        /// Defines the _port
        /// </summary>
        private int _port;

        /// <summary>
        /// Defines the _receiveThread
        /// </summary>
        private Thread _receiveThread;

        /// <summary>
        /// Defines the _secure
        /// </summary>
        private bool _secure;

        /// <summary>
        /// Defines the _services
        /// </summary>
        private WebSocketServiceManager _services;

        /// <summary>
        /// Defines the _state
        /// </summary>
        private volatile ServerState _state;

        /// <summary>
        /// Defines the _sync
        /// </summary>
        private object _sync;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class.
        /// </summary>
        public HttpServer()
        {
            init("*", System.Net.IPAddress.Any, 80, false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class.
        /// </summary>
        /// <param name="port">The port<see cref="int"/></param>
        public HttpServer(int port)
      : this(port, port == 443)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class.
        /// </summary>
        /// <param name="port">The port<see cref="int"/></param>
        /// <param name="secure">The secure<see cref="bool"/></param>
        public HttpServer(int port, bool secure)
        {
            if (!port.IsPortNumber())
            {
                var msg = "Less than 1 or greater than 65535.";
                throw new ArgumentOutOfRangeException("port", msg);
            }

            init("*", System.Net.IPAddress.Any, port, secure);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class.
        /// </summary>
        /// <param name="url">The url<see cref="string"/></param>
        public HttpServer(string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            if (url.Length == 0)
                throw new ArgumentException("An empty string.", "url");

            Uri uri;
            string msg;
            if (!tryCreateUri(url, out uri, out msg))
                throw new ArgumentException(msg, "url");

            var host = uri.GetDnsSafeHost(true);

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

            init(host, addr, uri.Port, uri.Scheme == "https");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class.
        /// </summary>
        /// <param name="address">The address<see cref="System.Net.IPAddress"/></param>
        /// <param name="port">The port<see cref="int"/></param>
        public HttpServer(System.Net.IPAddress address, int port)
      : this(address, port, port == 443)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer"/> class.
        /// </summary>
        /// <param name="address">The address<see cref="System.Net.IPAddress"/></param>
        /// <param name="port">The port<see cref="int"/></param>
        /// <param name="secure">The secure<see cref="bool"/></param>
        public HttpServer(System.Net.IPAddress address, int port, bool secure)
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

            init(address.ToString(true), address, port, secure);
        }

        #endregion 构造函数

        #region 事件

        /// <summary>
        /// Occurs when the server receives an HTTP CONNECT request.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnConnect;

        /// <summary>
        /// Occurs when the server receives an HTTP DELETE request.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnDelete;

        /// <summary>
        /// Occurs when the server receives an HTTP GET request.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnGet;

        /// <summary>
        /// Occurs when the server receives an HTTP HEAD request.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnHead;

        /// <summary>
        /// Occurs when the server receives an HTTP OPTIONS request.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnOptions;

        /// <summary>
        /// Occurs when the server receives an HTTP POST request.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnPost;

        /// <summary>
        /// Occurs when the server receives an HTTP PUT request.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnPut;

        /// <summary>
        /// Occurs when the server receives an HTTP TRACE request.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> OnTrace;

        #endregion 事件

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
        /// Gets or sets the scheme used to authenticate the clients.
        /// </summary>
        public AuthenticationSchemes AuthenticationSchemes
        {
            get
            {
                return _listener.AuthenticationSchemes;
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

                    _listener.AuthenticationSchemes = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the path to the document folder of the server.
        /// </summary>
        public string DocumentRootPath
        {
            get
            {
                return _docRootPath;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Length == 0)
                    throw new ArgumentException("An empty string.", "value");

                value = value.TrimSlashOrBackslashFromEnd();

                string full = null;
                try
                {
                    full = Path.GetFullPath(value);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("An invalid path string.", "value", ex);
                }

                if (value == "/")
                    throw new ArgumentException("An absolute root.", "value");

                if (value == "\\")
                    throw new ArgumentException("An absolute root.", "value");

                if (value.Length == 2 && value[1] == ':')
                    throw new ArgumentException("An absolute root.", "value");

                if (full == "/")
                    throw new ArgumentException("An absolute root.", "value");

                full = full.TrimSlashOrBackslashFromEnd();
                if (full.Length == 2 && full[1] == ':')
                    throw new ArgumentException("An absolute root.", "value");

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

                    _docRootPath = value;
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
                return _listener.Realm;
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

                    _listener.Realm = value;
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
                return _listener.ReuseAddress;
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

                    _listener.ReuseAddress = value;
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

                return _listener.SslConfiguration;
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
                return _listener.UserCredentialsFinder;
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

                    _listener.UserCredentialsFinder = value;
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
        [Obsolete("This method will be removed. Use added one instead.")]
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
        /// Gets the contents of the specified file from the document
        /// folder of the server.
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        [Obsolete("This method will be removed.")]
        public byte[] GetFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Length == 0)
                throw new ArgumentException("An empty string.", "path");

            if (path.IndexOf("..") > -1)
                throw new ArgumentException("It contains '..'.", "path");

            path = createFilePath(path);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
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
        /// Starts receiving incoming requests.
        /// </summary>
        public void Start()
        {
            if (_secure)
            {
                string msg;
                if (!checkCertificate(out msg))
                    throw new InvalidOperationException(msg);
            }

            start();
        }

        /// <summary>
        /// Stops receiving incoming requests and closes each connection.
        /// </summary>
        public void Stop()
        {
            stop(1005, String.Empty);
        }

        /// <summary>
        /// Stops receiving incoming requests and closes each connection.
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
        /// Stops receiving incoming requests and closes each connection.
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
        /// The createListener
        /// </summary>
        /// <param name="hostname">The hostname<see cref="string"/></param>
        /// <param name="port">The port<see cref="int"/></param>
        /// <param name="secure">The secure<see cref="bool"/></param>
        /// <returns>The <see cref="HttpListener"/></returns>
        private static HttpListener createListener(
      string hostname, int port, bool secure
    )
        {
            var lsnr = new HttpListener();

            var schm = secure ? "https" : "http";
            var pref = String.Format("{0}://{1}:{2}/", schm, hostname, port);
            lsnr.Prefixes.Add(pref);

            return lsnr;
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
            result = null;
            message = null;

            var uri = uriString.ToUri();
            if (uri == null)
            {
                message = "An invalid URI string.";
                return false;
            }

            if (!uri.IsAbsoluteUri)
            {
                message = "A relative URI.";
                return false;
            }

            var schm = uri.Scheme;
            if (!(schm == "http" || schm == "https"))
            {
                message = "The scheme part is not 'http' or 'https'.";
                return false;
            }

            if (uri.PathAndQuery != "/")
            {
                message = "It includes either or both path and query components.";
                return false;
            }

            if (uri.Fragment.Length > 0)
            {
                message = "It includes the fragment component.";
                return false;
            }

            if (uri.Port == 0)
            {
                message = "The port part is zero.";
                return false;
            }

            result = uri;
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
                    _services.Stop(1006, String.Empty);
                }
                finally
                {
                    _listener.Abort();
                }
            }
            catch
            {
            }

            _state = ServerState.Stop;
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
        /// The checkCertificate
        /// </summary>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool checkCertificate(out string message)
        {
            message = null;

            var byUser = _listener.SslConfiguration.ServerCertificate != null;

            var path = _listener.CertificateFolderPath;
            var withPort = EndPointListener.CertificateExists(_port, path);

            var both = byUser && withPort;
            if (both)
            {
                _log.Warn("A server certificate associated with the port is used.");
                return true;
            }

            var either = byUser || withPort;
            if (!either)
            {
                message = "There is no server certificate for secure connections.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// The createFilePath
        /// </summary>
        /// <param name="childPath">The childPath<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string createFilePath(string childPath)
        {
            childPath = childPath.TrimStart('/', '\\');
            return new StringBuilder(_docRootPath, 32)
                   .AppendFormat("/{0}", childPath)
                   .ToString()
                   .Replace('\\', '/');
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

            _docRootPath = "./Public";
            _listener = createListener(_hostname, _port, _secure);
            _log = _listener.Log;
            _services = new WebSocketServiceManager(_log);
            _sync = new object();
        }

        /// <summary>
        /// The processRequest
        /// </summary>
        /// <param name="context">The context<see cref="HttpListenerContext"/></param>
        private void processRequest(HttpListenerContext context)
        {
            var method = context.Request.HttpMethod;
            var evt = method == "GET"
                      ? OnGet
                      : method == "HEAD"
                        ? OnHead
                        : method == "POST"
                          ? OnPost
                          : method == "PUT"
                            ? OnPut
                            : method == "DELETE"
                              ? OnDelete
                              : method == "CONNECT"
                                ? OnConnect
                                : method == "OPTIONS"
                                  ? OnOptions
                                  : method == "TRACE"
                                    ? OnTrace
                                    : null;

            if (evt != null)
                evt(this, new HttpRequestEventArgs(context, _docRootPath));
            else
                context.Response.StatusCode = 501; // Not Implemented

            context.Response.Close();
        }

        /// <summary>
        /// The processRequest
        /// </summary>
        /// <param name="context">The context<see cref="HttpListenerWebSocketContext"/></param>
        private void processRequest(HttpListenerWebSocketContext context)
        {
            var uri = context.RequestUri;
            if (uri == null)
            {
                context.Close(HttpStatusCode.BadRequest);
                return;
            }

            var path = uri.AbsolutePath;

            WebSocketServiceHostBase host;
            if (!_services.InternalTryGetServiceHost(path, out host))
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
                HttpListenerContext ctx = null;
                try
                {
                    ctx = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(
                      state =>
                      {
                          try
                          {
                              if (ctx.Request.IsUpgradeRequest("websocket"))
                              {
                                  processRequest(ctx.AcceptWebSocket(null));
                                  return;
                              }

                              processRequest(ctx);
                          }
                          catch (Exception ex)
                          {
                              _log.Fatal(ex.Message);
                              _log.Debug(ex.ToString());

                              ctx.Connection.Close(true);
                          }
                      }
                    );
                }
                catch (HttpListenerException)
                {
                    _log.Info("The underlying listener is stopped.");
                    break;
                }
                catch (InvalidOperationException)
                {
                    _log.Info("The underlying listener is stopped.");
                    break;
                }
                catch (Exception ex)
                {
                    _log.Fatal(ex.Message);
                    _log.Debug(ex.ToString());

                    if (ctx != null)
                        ctx.Connection.Close(true);

                    break;
                }
            }

            if (_state != ServerState.ShuttingDown)
                abort();
        }

        /// <summary>
        /// The start
        /// </summary>
        private void start()
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
                    _services.Stop(code, reason);
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
                        stopReceiving(5000);
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
            _listener.Stop();
            _receiveThread.Join(millisecondsTimeout);
        }

        #endregion 方法
    }
}