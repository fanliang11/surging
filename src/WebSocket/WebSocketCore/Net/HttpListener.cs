/*
 * HttpListener.cs
 *
 * This code is derived from HttpListener.cs (System.Net) of Mono
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

/*
 * Contributors:
 * - Liryna <liryna.stark@gmail.com>
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;

// TODO: Logging.
namespace WebSocketCore.Net
{
    /// <summary>
    /// Provides a simple, programmatically controlled HTTP listener.
    /// </summary>
    public sealed class HttpListener : IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _defaultRealm
        /// </summary>
        private static readonly string _defaultRealm;

        /// <summary>
        /// Defines the _authSchemes
        /// </summary>
        private AuthenticationSchemes _authSchemes;

        /// <summary>
        /// Defines the _authSchemeSelector
        /// </summary>
        private Func<HttpListenerRequest, AuthenticationSchemes> _authSchemeSelector;

        /// <summary>
        /// Defines the _certFolderPath
        /// </summary>
        private string _certFolderPath;

        /// <summary>
        /// Defines the _connections
        /// </summary>
        private Dictionary<HttpConnection, HttpConnection> _connections;

        /// <summary>
        /// Defines the _connectionsSync
        /// </summary>
        private object _connectionsSync;

        /// <summary>
        /// Defines the _ctxQueue
        /// </summary>
        private List<HttpListenerContext> _ctxQueue;

        /// <summary>
        /// Defines the _ctxQueueSync
        /// </summary>
        private object _ctxQueueSync;

        /// <summary>
        /// Defines the _ctxRegistry
        /// </summary>
        private Dictionary<HttpListenerContext, HttpListenerContext> _ctxRegistry;

        /// <summary>
        /// Defines the _ctxRegistrySync
        /// </summary>
        private object _ctxRegistrySync;

        /// <summary>
        /// Defines the _disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Defines the _ignoreWriteExceptions
        /// </summary>
        private bool _ignoreWriteExceptions;

        /// <summary>
        /// Defines the _listening
        /// </summary>
        private volatile bool _listening;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private Logger _logger;

        /// <summary>
        /// Defines the _prefixes
        /// </summary>
        private HttpListenerPrefixCollection _prefixes;

        /// <summary>
        /// Defines the _realm
        /// </summary>
        private string _realm;

        /// <summary>
        /// Defines the _reuseAddress
        /// </summary>
        private bool _reuseAddress;

        /// <summary>
        /// Defines the _sslConfig
        /// </summary>
        private ServerSslConfiguration _sslConfig;

        /// <summary>
        /// Defines the _userCredFinder
        /// </summary>
        private Func<IIdentity, NetworkCredential> _userCredFinder;

        /// <summary>
        /// Defines the _waitQueue
        /// </summary>
        private List<HttpListenerAsyncResult> _waitQueue;

        /// <summary>
        /// Defines the _waitQueueSync
        /// </summary>
        private object _waitQueueSync;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListener"/> class.
        /// </summary>
        public HttpListener()
        {
            _authSchemes = AuthenticationSchemes.Anonymous;

            _connections = new Dictionary<HttpConnection, HttpConnection>();
            _connectionsSync = ((ICollection)_connections).SyncRoot;

            _ctxQueue = new List<HttpListenerContext>();
            _ctxQueueSync = ((ICollection)_ctxQueue).SyncRoot;

            _ctxRegistry = new Dictionary<HttpListenerContext, HttpListenerContext>();
            _ctxRegistrySync = ((ICollection)_ctxRegistry).SyncRoot;

            _logger = new Logger();

            _prefixes = new HttpListenerPrefixCollection(this);

            _waitQueue = new List<HttpListenerAsyncResult>();
            _waitQueueSync = ((ICollection)_waitQueue).SyncRoot;
        }

        /// <summary>
        /// Initializes static members of the <see cref="HttpListener"/> class.
        /// </summary>
        static HttpListener()
        {
            _defaultRealm = "SECRET AREA";
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets a value indicating whether the listener can be used with the current operating system.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the scheme used to authenticate the clients.
        /// </summary>
        public AuthenticationSchemes AuthenticationSchemes
        {
            get
            {
                CheckDisposed();
                return _authSchemes;
            }

            set
            {
                CheckDisposed();
                _authSchemes = value;
            }
        }

        /// <summary>
        /// Gets or sets the delegate called to select the scheme used to authenticate the clients.
        /// </summary>
        public Func<HttpListenerRequest, AuthenticationSchemes> AuthenticationSchemeSelector
        {
            get
            {
                CheckDisposed();
                return _authSchemeSelector;
            }

            set
            {
                CheckDisposed();
                _authSchemeSelector = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to the folder in which stores the certificate files used to
        /// authenticate the server on the secure connection.
        /// </summary>
        public string CertificateFolderPath
        {
            get
            {
                CheckDisposed();
                return _certFolderPath;
            }

            set
            {
                CheckDisposed();
                _certFolderPath = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the listener returns exceptions that occur when
        /// sending the response to the client.
        /// </summary>
        public bool IgnoreWriteExceptions
        {
            get
            {
                CheckDisposed();
                return _ignoreWriteExceptions;
            }

            set
            {
                CheckDisposed();
                _ignoreWriteExceptions = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the listener has been started.
        /// </summary>
        public bool IsListening
        {
            get
            {
                return _listening;
            }
        }

        /// <summary>
        /// Gets the logging functions.
        /// </summary>
        public Logger Log
        {
            get
            {
                return _logger;
            }
        }

        /// <summary>
        /// Gets the URI prefixes handled by the listener.
        /// </summary>
        public HttpListenerPrefixCollection Prefixes
        {
            get
            {
                CheckDisposed();
                return _prefixes;
            }
        }

        /// <summary>
        /// Gets or sets the name of the realm associated with the listener.
        /// </summary>
        public string Realm
        {
            get
            {
                CheckDisposed();
                return _realm;
            }

            set
            {
                CheckDisposed();
                _realm = value;
            }
        }

        /// <summary>
        /// Gets or sets the SSL configuration used to authenticate the server and
        /// optionally the client for secure connection.
        /// </summary>
        public ServerSslConfiguration SslConfiguration
        {
            get
            {
                CheckDisposed();
                return _sslConfig ?? (_sslConfig = new ServerSslConfiguration());
            }

            set
            {
                CheckDisposed();
                _sslConfig = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether when NTLM authentication is used,
        /// the authentication information of first request is used to authenticate
        /// additional requests on the same connection.
        /// </summary>
        public bool UnsafeConnectionNtlmAuthentication
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets the delegate called to find the credentials for an identity used to
        /// authenticate a client.
        /// </summary>
        public Func<IIdentity, NetworkCredential> UserCredentialsFinder
        {
            get
            {
                CheckDisposed();
                return _userCredFinder;
            }

            set
            {
                CheckDisposed();
                _userCredFinder = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsDisposed
        /// </summary>
        internal bool IsDisposed
        {
            get
            {
                return _disposed;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ReuseAddress
        /// </summary>
        internal bool ReuseAddress
        {
            get
            {
                return _reuseAddress;
            }

            set
            {
                _reuseAddress = value;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Shuts down the listener immediately.
        /// </summary>
        public void Abort()
        {
            if (_disposed)
                return;

            close(true);
        }

        /// <summary>
        /// Begins getting an incoming request asynchronously.
        /// </summary>
        /// <param name="callback">The callback<see cref="AsyncCallback"/></param>
        /// <param name="state">The state<see cref="Object"/></param>
        /// <returns>The <see cref="IAsyncResult"/></returns>
        public IAsyncResult BeginGetContext(AsyncCallback callback, Object state)
        {
            CheckDisposed();
            if (_prefixes.Count == 0)
                throw new InvalidOperationException("The listener has no URI prefix on which listens.");

            if (!_listening)
                throw new InvalidOperationException("The listener hasn't been started.");

            return BeginGetContext(new HttpListenerAsyncResult(callback, state));
        }

        /// <summary>
        /// Shuts down the listener.
        /// </summary>
        public void Close()
        {
            if (_disposed)
                return;

            close(false);
        }

        /// <summary>
        /// Ends an asynchronous operation to get an incoming request.
        /// </summary>
        /// <param name="asyncResult">The asyncResult<see cref="IAsyncResult"/></param>
        /// <returns>The <see cref="HttpListenerContext"/></returns>
        public HttpListenerContext EndGetContext(IAsyncResult asyncResult)
        {
            CheckDisposed();
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            var ares = asyncResult as HttpListenerAsyncResult;
            if (ares == null)
                throw new ArgumentException("A wrong IAsyncResult.", "asyncResult");

            if (ares.EndCalled)
                throw new InvalidOperationException("This IAsyncResult cannot be reused.");

            ares.EndCalled = true;
            if (!ares.IsCompleted)
                ares.AsyncWaitHandle.WaitOne();

            return ares.GetContext(); // This may throw an exception.
        }

        /// <summary>
        /// Gets an incoming request.
        /// </summary>
        /// <returns>The <see cref="HttpListenerContext"/></returns>
        public HttpListenerContext GetContext()
        {
            CheckDisposed();
            if (_prefixes.Count == 0)
                throw new InvalidOperationException("The listener has no URI prefix on which listens.");

            if (!_listening)
                throw new InvalidOperationException("The listener hasn't been started.");

            var ares = BeginGetContext(new HttpListenerAsyncResult(null, null));
            ares.InGet = true;

            return EndGetContext(ares);
        }

        /// <summary>
        /// Starts receiving incoming requests.
        /// </summary>
        public void Start()
        {
            CheckDisposed();
            if (_listening)
                return;

            EndPointManager.AddListener(this);
            _listening = true;
        }

        /// <summary>
        /// Stops receiving incoming requests.
        /// </summary>
        public void Stop()
        {
            CheckDisposed();
            if (!_listening)
                return;

            _listening = false;
            EndPointManager.RemoveListener(this);

            lock (_ctxRegistrySync)
                cleanupContextQueue(true);

            cleanupContextRegistry();
            cleanupConnections();
            cleanupWaitQueue(new HttpListenerException(995, "The listener is stopped."));
        }

        /// <summary>
        /// The AddConnection
        /// </summary>
        /// <param name="connection">The connection<see cref="HttpConnection"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal bool AddConnection(HttpConnection connection)
        {
            if (!_listening)
                return false;

            lock (_connectionsSync)
            {
                if (!_listening)
                    return false;

                _connections[connection] = connection;
                return true;
            }
        }

        /// <summary>
        /// The BeginGetContext
        /// </summary>
        /// <param name="asyncResult">The asyncResult<see cref="HttpListenerAsyncResult"/></param>
        /// <returns>The <see cref="HttpListenerAsyncResult"/></returns>
        internal HttpListenerAsyncResult BeginGetContext(HttpListenerAsyncResult asyncResult)
        {
            lock (_ctxRegistrySync)
            {
                if (!_listening)
                    throw new HttpListenerException(995);

                var ctx = getContextFromQueue();
                if (ctx == null)
                    _waitQueue.Add(asyncResult);
                else
                    asyncResult.Complete(ctx, true);

                return asyncResult;
            }
        }

        /// <summary>
        /// The CheckDisposed
        /// </summary>
        internal void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().ToString());
        }

        /// <summary>
        /// The GetRealm
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        internal string GetRealm()
        {
            var realm = _realm;
            return realm != null && realm.Length > 0 ? realm : _defaultRealm;
        }

        /// <summary>
        /// The GetUserCredentialsFinder
        /// </summary>
        /// <returns>The <see cref="Func{IIdentity, NetworkCredential}"/></returns>
        internal Func<IIdentity, NetworkCredential> GetUserCredentialsFinder()
        {
            return _userCredFinder;
        }

        /// <summary>
        /// The RegisterContext
        /// </summary>
        /// <param name="context">The context<see cref="HttpListenerContext"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal bool RegisterContext(HttpListenerContext context)
        {
            if (!_listening)
                return false;

            lock (_ctxRegistrySync)
            {
                if (!_listening)
                    return false;

                _ctxRegistry[context] = context;

                var ares = getAsyncResultFromQueue();
                if (ares == null)
                    _ctxQueue.Add(context);
                else
                    ares.Complete(context);

                return true;
            }
        }

        /// <summary>
        /// The RemoveConnection
        /// </summary>
        /// <param name="connection">The connection<see cref="HttpConnection"/></param>
        internal void RemoveConnection(HttpConnection connection)
        {
            lock (_connectionsSync)
                _connections.Remove(connection);
        }

        /// <summary>
        /// The SelectAuthenticationScheme
        /// </summary>
        /// <param name="request">The request<see cref="HttpListenerRequest"/></param>
        /// <returns>The <see cref="AuthenticationSchemes"/></returns>
        internal AuthenticationSchemes SelectAuthenticationScheme(HttpListenerRequest request)
        {
            var selector = _authSchemeSelector;
            if (selector == null)
                return _authSchemes;

            try
            {
                return selector(request);
            }
            catch
            {
                return AuthenticationSchemes.None;
            }
        }

        /// <summary>
        /// The UnregisterContext
        /// </summary>
        /// <param name="context">The context<see cref="HttpListenerContext"/></param>
        internal void UnregisterContext(HttpListenerContext context)
        {
            lock (_ctxRegistrySync)
                _ctxRegistry.Remove(context);
        }

        /// <summary>
        /// The cleanupConnections
        /// </summary>
        private void cleanupConnections()
        {
            HttpConnection[] conns = null;
            lock (_connectionsSync)
            {
                if (_connections.Count == 0)
                    return;

                // Need to copy this since closing will call the RemoveConnection method.
                var keys = _connections.Keys;
                conns = new HttpConnection[keys.Count];
                keys.CopyTo(conns, 0);
                _connections.Clear();
            }

            for (var i = conns.Length - 1; i >= 0; i--)
                conns[i].Close(true);
        }

        /// <summary>
        /// The cleanupContextQueue
        /// </summary>
        /// <param name="sendServiceUnavailable">The sendServiceUnavailable<see cref="bool"/></param>
        private void cleanupContextQueue(bool sendServiceUnavailable)
        {
            HttpListenerContext[] ctxs = null;
            lock (_ctxQueueSync)
            {
                if (_ctxQueue.Count == 0)
                    return;

                ctxs = _ctxQueue.ToArray();
                _ctxQueue.Clear();
            }

            if (!sendServiceUnavailable)
                return;

            foreach (var ctx in ctxs)
            {
                var res = ctx.Response;
                res.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                res.Close();
            }
        }

        /// <summary>
        /// The cleanupContextRegistry
        /// </summary>
        private void cleanupContextRegistry()
        {
            HttpListenerContext[] ctxs = null;
            lock (_ctxRegistrySync)
            {
                if (_ctxRegistry.Count == 0)
                    return;

                // Need to copy this since closing will call the UnregisterContext method.
                var keys = _ctxRegistry.Keys;
                ctxs = new HttpListenerContext[keys.Count];
                keys.CopyTo(ctxs, 0);
                _ctxRegistry.Clear();
            }

            for (var i = ctxs.Length - 1; i >= 0; i--)
                ctxs[i].Connection.Close(true);
        }

        /// <summary>
        /// The cleanupWaitQueue
        /// </summary>
        /// <param name="exception">The exception<see cref="Exception"/></param>
        private void cleanupWaitQueue(Exception exception)
        {
            HttpListenerAsyncResult[] aress = null;
            lock (_waitQueueSync)
            {
                if (_waitQueue.Count == 0)
                    return;

                aress = _waitQueue.ToArray();
                _waitQueue.Clear();
            }

            foreach (var ares in aress)
                ares.Complete(exception);
        }

        /// <summary>
        /// The close
        /// </summary>
        /// <param name="force">The force<see cref="bool"/></param>
        private void close(bool force)
        {
            if (_listening)
            {
                _listening = false;
                EndPointManager.RemoveListener(this);
            }

            lock (_ctxRegistrySync)
                cleanupContextQueue(!force);

            cleanupContextRegistry();
            cleanupConnections();
            cleanupWaitQueue(new ObjectDisposedException(GetType().ToString()));

            _disposed = true;
        }

        /// <summary>
        /// The getAsyncResultFromQueue
        /// </summary>
        /// <returns>The <see cref="HttpListenerAsyncResult"/></returns>
        private HttpListenerAsyncResult getAsyncResultFromQueue()
        {
            if (_waitQueue.Count == 0)
                return null;

            var ares = _waitQueue[0];
            _waitQueue.RemoveAt(0);

            return ares;
        }

        /// <summary>
        /// The getContextFromQueue
        /// </summary>
        /// <returns>The <see cref="HttpListenerContext"/></returns>
        private HttpListenerContext getContextFromQueue()
        {
            if (_ctxQueue.Count == 0)
                return null;

            var ctx = _ctxQueue[0];
            _ctxQueue.RemoveAt(0);

            return ctx;
        }

        /// <summary>
        /// Releases all resources used by the listener.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (_disposed)
                return;

            close(true);
        }

        #endregion 方法
    }
}