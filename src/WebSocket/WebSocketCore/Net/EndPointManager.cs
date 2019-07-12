/*
 * EndPointManager.cs
 *
 * This code is derived from EndPointManager.cs (System.Net) of Mono
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
 * - Gonzalo Paniagua Javier <gonzalo@ximian.com>
 */

/*
 * Contributors:
 * - Liryna <liryna.stark@gmail.com>
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Defines the <see cref="EndPointManager" />
    /// </summary>
    internal sealed class EndPointManager
    {
        #region 字段

        /// <summary>
        /// Defines the _endpoints
        /// </summary>
        private static readonly Dictionary<IPEndPoint, EndPointListener> _endpoints;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Prevents a default instance of the <see cref="EndPointManager"/> class from being created.
        /// </summary>
        private EndPointManager()
        {
        }

        /// <summary>
        /// Initializes static members of the <see cref="EndPointManager"/> class.
        /// </summary>
        static EndPointManager()
        {
            _endpoints = new Dictionary<IPEndPoint, EndPointListener>();
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The AddListener
        /// </summary>
        /// <param name="listener">The listener<see cref="HttpListener"/></param>
        public static void AddListener(HttpListener listener)
        {
            var added = new List<string>();
            lock (((ICollection)_endpoints).SyncRoot)
            {
                try
                {
                    foreach (var pref in listener.Prefixes)
                    {
                        addPrefix(pref, listener);
                        added.Add(pref);
                    }
                }
                catch
                {
                    foreach (var pref in added)
                        removePrefix(pref, listener);

                    throw;
                }
            }
        }

        /// <summary>
        /// The AddPrefix
        /// </summary>
        /// <param name="uriPrefix">The uriPrefix<see cref="string"/></param>
        /// <param name="listener">The listener<see cref="HttpListener"/></param>
        public static void AddPrefix(string uriPrefix, HttpListener listener)
        {
            lock (((ICollection)_endpoints).SyncRoot)
                addPrefix(uriPrefix, listener);
        }

        /// <summary>
        /// The RemoveListener
        /// </summary>
        /// <param name="listener">The listener<see cref="HttpListener"/></param>
        public static void RemoveListener(HttpListener listener)
        {
            lock (((ICollection)_endpoints).SyncRoot)
            {
                foreach (var pref in listener.Prefixes)
                    removePrefix(pref, listener);
            }
        }

        /// <summary>
        /// The RemovePrefix
        /// </summary>
        /// <param name="uriPrefix">The uriPrefix<see cref="string"/></param>
        /// <param name="listener">The listener<see cref="HttpListener"/></param>
        public static void RemovePrefix(string uriPrefix, HttpListener listener)
        {
            lock (((ICollection)_endpoints).SyncRoot)
                removePrefix(uriPrefix, listener);
        }

        /// <summary>
        /// The RemoveEndPoint
        /// </summary>
        /// <param name="endpoint">The endpoint<see cref="IPEndPoint"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal static bool RemoveEndPoint(IPEndPoint endpoint)
        {
            lock (((ICollection)_endpoints).SyncRoot)
            {
                EndPointListener lsnr;
                if (!_endpoints.TryGetValue(endpoint, out lsnr))
                    return false;

                _endpoints.Remove(endpoint);
                lsnr.Close();

                return true;
            }
        }

        /// <summary>
        /// The addPrefix
        /// </summary>
        /// <param name="uriPrefix">The uriPrefix<see cref="string"/></param>
        /// <param name="listener">The listener<see cref="HttpListener"/></param>
        private static void addPrefix(string uriPrefix, HttpListener listener)
        {
            var pref = new HttpListenerPrefix(uriPrefix);

            var addr = convertToIPAddress(pref.Host);
            if (addr == null)
                throw new HttpListenerException(87, "Includes an invalid host.");

            if (!addr.IsLocal())
                throw new HttpListenerException(87, "Includes an invalid host.");

            int port;
            if (!Int32.TryParse(pref.Port, out port))
                throw new HttpListenerException(87, "Includes an invalid port.");

            if (!port.IsPortNumber())
                throw new HttpListenerException(87, "Includes an invalid port.");

            var path = pref.Path;
            if (path.IndexOf('%') != -1)
                throw new HttpListenerException(87, "Includes an invalid path.");

            if (path.IndexOf("//", StringComparison.Ordinal) != -1)
                throw new HttpListenerException(87, "Includes an invalid path.");

            var endpoint = new IPEndPoint(addr, port);

            EndPointListener lsnr;
            if (_endpoints.TryGetValue(endpoint, out lsnr))
            {
                if (lsnr.IsSecure ^ pref.IsSecure)
                    throw new HttpListenerException(87, "Includes an invalid scheme.");
            }
            else
            {
                lsnr =
                  new EndPointListener(
                    endpoint,
                    pref.IsSecure,
                    listener.CertificateFolderPath,
                    listener.SslConfiguration,
                    listener.ReuseAddress
                  );

                _endpoints.Add(endpoint, lsnr);
            }

            lsnr.AddPrefix(pref, listener);
        }

        /// <summary>
        /// The convertToIPAddress
        /// </summary>
        /// <param name="hostname">The hostname<see cref="string"/></param>
        /// <returns>The <see cref="IPAddress"/></returns>
        private static IPAddress convertToIPAddress(string hostname)
        {
            if (hostname == "*")
                return IPAddress.Any;

            if (hostname == "+")
                return IPAddress.Any;

            return hostname.ToIPAddress();
        }

        /// <summary>
        /// The removePrefix
        /// </summary>
        /// <param name="uriPrefix">The uriPrefix<see cref="string"/></param>
        /// <param name="listener">The listener<see cref="HttpListener"/></param>
        private static void removePrefix(string uriPrefix, HttpListener listener)
        {
            var pref = new HttpListenerPrefix(uriPrefix);

            var addr = convertToIPAddress(pref.Host);
            if (addr == null)
                return;

            if (!addr.IsLocal())
                return;

            int port;
            if (!Int32.TryParse(pref.Port, out port))
                return;

            if (!port.IsPortNumber())
                return;

            var path = pref.Path;
            if (path.IndexOf('%') != -1)
                return;

            if (path.IndexOf("//", StringComparison.Ordinal) != -1)
                return;

            var endpoint = new IPEndPoint(addr, port);

            EndPointListener lsnr;
            if (!_endpoints.TryGetValue(endpoint, out lsnr))
                return;

            if (lsnr.IsSecure ^ pref.IsSecure)
                return;

            lsnr.RemovePrefix(pref, listener);
        }

        #endregion 方法
    }
}