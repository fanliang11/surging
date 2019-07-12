/*
 * EndPointListener.cs
 *
 * This code is derived from EndPointListener.cs (System.Net) of Mono
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
 * - Nicholas Devenish
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Defines the <see cref="EndPointListener" />
    /// </summary>
    internal sealed class EndPointListener
    {
        #region 字段

        /// <summary>
        /// Defines the _defaultCertFolderPath
        /// </summary>
        private static readonly string _defaultCertFolderPath;

        /// <summary>
        /// Defines the _all
        /// </summary>
        private List<HttpListenerPrefix> _all;// host == '+'

        /// <summary>
        /// Defines the _endpoint
        /// </summary>
        private IPEndPoint _endpoint;

        /// <summary>
        /// Defines the _prefixes
        /// </summary>
        private Dictionary<HttpListenerPrefix, HttpListener> _prefixes;

        /// <summary>
        /// Defines the _secure
        /// </summary>
        private bool _secure;

        /// <summary>
        /// Defines the _socket
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// Defines the _sslConfig
        /// </summary>
        private ServerSslConfiguration _sslConfig;

        /// <summary>
        /// Defines the _unhandled
        /// </summary>
        private List<HttpListenerPrefix> _unhandled;// host == '*'

        /// <summary>
        /// Defines the _unregistered
        /// </summary>
        private Dictionary<HttpConnection, HttpConnection> _unregistered;

        /// <summary>
        /// Defines the _unregisteredSync
        /// </summary>
        private object _unregisteredSync;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="EndPointListener"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint<see cref="IPEndPoint"/></param>
        /// <param name="secure">The secure<see cref="bool"/></param>
        /// <param name="certificateFolderPath">The certificateFolderPath<see cref="string"/></param>
        /// <param name="sslConfig">The sslConfig<see cref="ServerSslConfiguration"/></param>
        /// <param name="reuseAddress">The reuseAddress<see cref="bool"/></param>
        internal EndPointListener(
      IPEndPoint endpoint,
      bool secure,
      string certificateFolderPath,
      ServerSslConfiguration sslConfig,
      bool reuseAddress
    )
        {
            if (secure)
            {
                var cert =
                  getCertificate(endpoint.Port, certificateFolderPath, sslConfig.ServerCertificate);

                if (cert == null)
                    throw new ArgumentException("No server certificate could be found.");

                _secure = true;
                _sslConfig = new ServerSslConfiguration(sslConfig);
                _sslConfig.ServerCertificate = cert;
            }

            _endpoint = endpoint;
            _prefixes = new Dictionary<HttpListenerPrefix, HttpListener>();
            _unregistered = new Dictionary<HttpConnection, HttpConnection>();
            _unregisteredSync = ((ICollection)_unregistered).SyncRoot;
            _socket =
              new Socket(endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (reuseAddress)
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _socket.Bind(endpoint);
            _socket.Listen(500);
            _socket.BeginAccept(onAccept, this);
        }

        /// <summary>
        /// Initializes static members of the <see cref="EndPointListener"/> class.
        /// </summary>
        static EndPointListener()
        {
            _defaultCertFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Address
        /// </summary>
        public IPAddress Address
        {
            get
            {
                return _endpoint.Address;
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsSecure
        /// </summary>
        public bool IsSecure
        {
            get
            {
                return _secure;
            }
        }

        /// <summary>
        /// Gets the Port
        /// </summary>
        public int Port
        {
            get
            {
                return _endpoint.Port;
            }
        }

        /// <summary>
        /// Gets the SslConfiguration
        /// </summary>
        public ServerSslConfiguration SslConfiguration
        {
            get
            {
                return _sslConfig;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The AddPrefix
        /// </summary>
        /// <param name="prefix">The prefix<see cref="HttpListenerPrefix"/></param>
        /// <param name="listener">The listener<see cref="HttpListener"/></param>
        public void AddPrefix(HttpListenerPrefix prefix, HttpListener listener)
        {
            List<HttpListenerPrefix> current, future;
            if (prefix.Host == "*")
            {
                do
                {
                    current = _unhandled;
                    future = current != null
                             ? new List<HttpListenerPrefix>(current)
                             : new List<HttpListenerPrefix>();

                    prefix.Listener = listener;
                    addSpecial(future, prefix);
                }
                while (Interlocked.CompareExchange(ref _unhandled, future, current) != current);

                return;
            }

            if (prefix.Host == "+")
            {
                do
                {
                    current = _all;
                    future = current != null
                             ? new List<HttpListenerPrefix>(current)
                             : new List<HttpListenerPrefix>();

                    prefix.Listener = listener;
                    addSpecial(future, prefix);
                }
                while (Interlocked.CompareExchange(ref _all, future, current) != current);

                return;
            }

            Dictionary<HttpListenerPrefix, HttpListener> prefs, prefs2;
            do
            {
                prefs = _prefixes;
                if (prefs.ContainsKey(prefix))
                {
                    if (prefs[prefix] != listener)
                    {
                        throw new HttpListenerException(
                          87, String.Format("There's another listener for {0}.", prefix)
                        );
                    }

                    return;
                }

                prefs2 = new Dictionary<HttpListenerPrefix, HttpListener>(prefs);
                prefs2[prefix] = listener;
            }
            while (Interlocked.CompareExchange(ref _prefixes, prefs2, prefs) != prefs);
        }

        /// <summary>
        /// The Close
        /// </summary>
        public void Close()
        {
            _socket.Close();

            HttpConnection[] conns = null;
            lock (_unregisteredSync)
            {
                if (_unregistered.Count == 0)
                    return;

                var keys = _unregistered.Keys;
                conns = new HttpConnection[keys.Count];
                keys.CopyTo(conns, 0);
                _unregistered.Clear();
            }

            for (var i = conns.Length - 1; i >= 0; i--)
                conns[i].Close(true);
        }

        /// <summary>
        /// The RemovePrefix
        /// </summary>
        /// <param name="prefix">The prefix<see cref="HttpListenerPrefix"/></param>
        /// <param name="listener">The listener<see cref="HttpListener"/></param>
        public void RemovePrefix(HttpListenerPrefix prefix, HttpListener listener)
        {
            List<HttpListenerPrefix> current, future;
            if (prefix.Host == "*")
            {
                do
                {
                    current = _unhandled;
                    if (current == null)
                        break;

                    future = new List<HttpListenerPrefix>(current);
                    if (!removeSpecial(future, prefix))
                        break; // The prefix wasn't found.
                }
                while (Interlocked.CompareExchange(ref _unhandled, future, current) != current);

                leaveIfNoPrefix();
                return;
            }

            if (prefix.Host == "+")
            {
                do
                {
                    current = _all;
                    if (current == null)
                        break;

                    future = new List<HttpListenerPrefix>(current);
                    if (!removeSpecial(future, prefix))
                        break; // The prefix wasn't found.
                }
                while (Interlocked.CompareExchange(ref _all, future, current) != current);

                leaveIfNoPrefix();
                return;
            }

            Dictionary<HttpListenerPrefix, HttpListener> prefs, prefs2;
            do
            {
                prefs = _prefixes;
                if (!prefs.ContainsKey(prefix))
                    break;

                prefs2 = new Dictionary<HttpListenerPrefix, HttpListener>(prefs);
                prefs2.Remove(prefix);
            }
            while (Interlocked.CompareExchange(ref _prefixes, prefs2, prefs) != prefs);

            leaveIfNoPrefix();
        }

        /// <summary>
        /// The CertificateExists
        /// </summary>
        /// <param name="port">The port<see cref="int"/></param>
        /// <param name="folderPath">The folderPath<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal static bool CertificateExists(int port, string folderPath)
        {
            if (folderPath == null || folderPath.Length == 0)
                folderPath = _defaultCertFolderPath;

            var cer = Path.Combine(folderPath, String.Format("{0}.cer", port));
            var key = Path.Combine(folderPath, String.Format("{0}.key", port));

            return File.Exists(cer) && File.Exists(key);
        }

        /// <summary>
        /// The RemoveConnection
        /// </summary>
        /// <param name="connection">The connection<see cref="HttpConnection"/></param>
        internal void RemoveConnection(HttpConnection connection)
        {
            lock (_unregisteredSync)
                _unregistered.Remove(connection);
        }

        /// <summary>
        /// The TrySearchHttpListener
        /// </summary>
        /// <param name="uri">The uri<see cref="Uri"/></param>
        /// <param name="listener">The listener<see cref="HttpListener"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal bool TrySearchHttpListener(Uri uri, out HttpListener listener)
        {
            listener = null;

            if (uri == null)
                return false;

            var host = uri.Host;
            var dns = Uri.CheckHostName(host) == UriHostNameType.Dns;
            var port = uri.Port.ToString();
            var path = HttpUtility.UrlDecode(uri.AbsolutePath);
            var pathSlash = path[path.Length - 1] != '/' ? path + "/" : path;

            if (host != null && host.Length > 0)
            {
                var bestLen = -1;
                foreach (var pref in _prefixes.Keys)
                {
                    if (dns)
                    {
                        var prefHost = pref.Host;
                        if (Uri.CheckHostName(prefHost) == UriHostNameType.Dns && prefHost != host)
                            continue;
                    }

                    if (pref.Port != port)
                        continue;

                    var prefPath = pref.Path;

                    var len = prefPath.Length;
                    if (len < bestLen)
                        continue;

                    if (path.StartsWith(prefPath) || pathSlash.StartsWith(prefPath))
                    {
                        bestLen = len;
                        listener = _prefixes[pref];
                    }
                }

                if (bestLen != -1)
                    return true;
            }

            var prefs = _unhandled;
            listener = searchHttpListenerFromSpecial(path, prefs);
            if (listener == null && pathSlash != path)
                listener = searchHttpListenerFromSpecial(pathSlash, prefs);

            if (listener != null)
                return true;

            prefs = _all;
            listener = searchHttpListenerFromSpecial(path, prefs);
            if (listener == null && pathSlash != path)
                listener = searchHttpListenerFromSpecial(pathSlash, prefs);

            return listener != null;
        }

        /// <summary>
        /// The addSpecial
        /// </summary>
        /// <param name="prefixes">The prefixes<see cref="List{HttpListenerPrefix}"/></param>
        /// <param name="prefix">The prefix<see cref="HttpListenerPrefix"/></param>
        private static void addSpecial(List<HttpListenerPrefix> prefixes, HttpListenerPrefix prefix)
        {
            var path = prefix.Path;
            foreach (var pref in prefixes)
            {
                if (pref.Path == path)
                    throw new HttpListenerException(87, "The prefix is already in use.");
            }

            prefixes.Add(prefix);
        }

        /// <summary>
        /// The createRSAFromFile
        /// </summary>
        /// <param name="filename">The filename<see cref="string"/></param>
        /// <returns>The <see cref="RSACryptoServiceProvider"/></returns>
        private static RSACryptoServiceProvider createRSAFromFile(string filename)
        {
            byte[] pvk = null;
            using (var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                pvk = new byte[fs.Length];
                fs.Read(pvk, 0, pvk.Length);
            }

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportCspBlob(pvk);

            return rsa;
        }

        /// <summary>
        /// The getCertificate
        /// </summary>
        /// <param name="port">The port<see cref="int"/></param>
        /// <param name="folderPath">The folderPath<see cref="string"/></param>
        /// <param name="defaultCertificate">The defaultCertificate<see cref="X509Certificate2"/></param>
        /// <returns>The <see cref="X509Certificate2"/></returns>
        private static X509Certificate2 getCertificate(
      int port, string folderPath, X509Certificate2 defaultCertificate
    )
        {
            if (folderPath == null || folderPath.Length == 0)
                folderPath = _defaultCertFolderPath;

            try
            {
                var cer = Path.Combine(folderPath, String.Format("{0}.cer", port));
                var key = Path.Combine(folderPath, String.Format("{0}.key", port));
                if (File.Exists(cer) && File.Exists(key))
                {
                    var cert = new X509Certificate2(cer);
                    cert.PrivateKey = createRSAFromFile(key);

                    return cert;
                }
            }
            catch
            {
            }

            return defaultCertificate;
        }

        /// <summary>
        /// The onAccept
        /// </summary>
        /// <param name="asyncResult">The asyncResult<see cref="IAsyncResult"/></param>
        private static void onAccept(IAsyncResult asyncResult)
        {
            var lsnr = (EndPointListener)asyncResult.AsyncState;

            Socket sock = null;
            try
            {
                sock = lsnr._socket.EndAccept(asyncResult);
            }
            catch (SocketException)
            {
                // TODO: Should log the error code when this class has a logging.
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            try
            {
                lsnr._socket.BeginAccept(onAccept, lsnr);
            }
            catch
            {
                if (sock != null)
                    sock.Close();

                return;
            }

            if (sock == null)
                return;

            processAccepted(sock, lsnr);
        }

        /// <summary>
        /// The processAccepted
        /// </summary>
        /// <param name="socket">The socket<see cref="Socket"/></param>
        /// <param name="listener">The listener<see cref="EndPointListener"/></param>
        private static void processAccepted(Socket socket, EndPointListener listener)
        {
            HttpConnection conn = null;
            try
            {
                conn = new HttpConnection(socket, listener);
                lock (listener._unregisteredSync)
                    listener._unregistered[conn] = conn;

                conn.BeginReadRequest();
            }
            catch
            {
                if (conn != null)
                {
                    conn.Close(true);
                    return;
                }

                socket.Close();
            }
        }

        /// <summary>
        /// The removeSpecial
        /// </summary>
        /// <param name="prefixes">The prefixes<see cref="List{HttpListenerPrefix}"/></param>
        /// <param name="prefix">The prefix<see cref="HttpListenerPrefix"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool removeSpecial(List<HttpListenerPrefix> prefixes, HttpListenerPrefix prefix)
        {
            var path = prefix.Path;
            var cnt = prefixes.Count;
            for (var i = 0; i < cnt; i++)
            {
                if (prefixes[i].Path == path)
                {
                    prefixes.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The searchHttpListenerFromSpecial
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="prefixes">The prefixes<see cref="List{HttpListenerPrefix}"/></param>
        /// <returns>The <see cref="HttpListener"/></returns>
        private static HttpListener searchHttpListenerFromSpecial(
      string path, List<HttpListenerPrefix> prefixes
    )
        {
            if (prefixes == null)
                return null;

            HttpListener bestMatch = null;

            var bestLen = -1;
            foreach (var pref in prefixes)
            {
                var prefPath = pref.Path;

                var len = prefPath.Length;
                if (len < bestLen)
                    continue;

                if (path.StartsWith(prefPath))
                {
                    bestLen = len;
                    bestMatch = pref.Listener;
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// The leaveIfNoPrefix
        /// </summary>
        private void leaveIfNoPrefix()
        {
            if (_prefixes.Count > 0)
                return;

            var prefs = _unhandled;
            if (prefs != null && prefs.Count > 0)
                return;

            prefs = _all;
            if (prefs != null && prefs.Count > 0)
                return;

            EndPointManager.RemoveEndPoint(_endpoint);
        }

        #endregion 方法
    }
}