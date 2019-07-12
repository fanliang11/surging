/*
 * HttpListenerPrefix.cs
 *
 * This code is derived from ListenerPrefix.cs (System.Net) of Mono
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
 * - Oleg Mihailik <mihailik@gmail.com>
 */

using System;
using System.Net;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Defines the <see cref="HttpListenerPrefix" />
    /// </summary>
    internal sealed class HttpListenerPrefix
    {
        #region 字段

        /// <summary>
        /// Defines the _host
        /// </summary>
        private string _host;

        /// <summary>
        /// Defines the _listener
        /// </summary>
        private HttpListener _listener;

        /// <summary>
        /// Defines the _original
        /// </summary>
        private string _original;

        /// <summary>
        /// Defines the _path
        /// </summary>
        private string _path;

        /// <summary>
        /// Defines the _port
        /// </summary>
        private string _port;

        /// <summary>
        /// Defines the _prefix
        /// </summary>
        private string _prefix;

        /// <summary>
        /// Defines the _secure
        /// </summary>
        private bool _secure;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerPrefix"/> class.
        /// </summary>
        /// <param name="uriPrefix">The uriPrefix<see cref="string"/></param>
        internal HttpListenerPrefix(string uriPrefix)
        {
            _original = uriPrefix;
            parse(uriPrefix);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Host
        /// </summary>
        public string Host
        {
            get
            {
                return _host;
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
        /// Gets or sets the Listener
        /// </summary>
        public HttpListener Listener
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

        /// <summary>
        /// Gets the Original
        /// </summary>
        public string Original
        {
            get
            {
                return _original;
            }
        }

        /// <summary>
        /// Gets the Path
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        /// <summary>
        /// Gets the Port
        /// </summary>
        public string Port
        {
            get
            {
                return _port;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The CheckPrefix
        /// </summary>
        /// <param name="uriPrefix">The uriPrefix<see cref="string"/></param>
        public static void CheckPrefix(string uriPrefix)
        {
            if (uriPrefix == null)
                throw new ArgumentNullException("uriPrefix");

            var len = uriPrefix.Length;
            if (len == 0)
                throw new ArgumentException("An empty string.", "uriPrefix");

            if (!(uriPrefix.StartsWith("http://") || uriPrefix.StartsWith("https://")))
                throw new ArgumentException("The scheme isn't 'http' or 'https'.", "uriPrefix");

            var startHost = uriPrefix.IndexOf(':') + 3;
            if (startHost >= len)
                throw new ArgumentException("No host is specified.", "uriPrefix");

            if (uriPrefix[startHost] == ':')
                throw new ArgumentException("No host is specified.", "uriPrefix");

            var root = uriPrefix.IndexOf('/', startHost, len - startHost);
            if (root == startHost)
                throw new ArgumentException("No host is specified.", "uriPrefix");

            if (root == -1 || uriPrefix[len - 1] != '/')
                throw new ArgumentException("Ends without '/'.", "uriPrefix");

            if (uriPrefix[root - 1] == ':')
                throw new ArgumentException("No port is specified.", "uriPrefix");

            if (root == len - 2)
                throw new ArgumentException("No path is specified.", "uriPrefix");
        }

        /// <summary>
        /// Determines whether this instance and the specified <see cref="Object"/> have the same value.
        /// </summary>
        /// <param name="obj">The obj<see cref="Object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool Equals(Object obj)
        {
            var pref = obj as HttpListenerPrefix;
            return pref != null && pref._prefix == _prefix;
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public override int GetHashCode()
        {
            return _prefix.GetHashCode();
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return _prefix;
        }

        /// <summary>
        /// The parse
        /// </summary>
        /// <param name="uriPrefix">The uriPrefix<see cref="string"/></param>
        private void parse(string uriPrefix)
        {
            if (uriPrefix.StartsWith("https"))
                _secure = true;

            var len = uriPrefix.Length;
            var startHost = uriPrefix.IndexOf(':') + 3;
            var root = uriPrefix.IndexOf('/', startHost + 1, len - startHost - 1);

            var colon = uriPrefix.LastIndexOf(':', root - 1, root - startHost - 1);
            if (uriPrefix[root - 1] != ']' && colon > startHost)
            {
                _host = uriPrefix.Substring(startHost, colon - startHost);
                _port = uriPrefix.Substring(colon + 1, root - colon - 1);
            }
            else
            {
                _host = uriPrefix.Substring(startHost, root - startHost);
                _port = _secure ? "443" : "80";
            }

            _path = uriPrefix.Substring(root);

            _prefix =
              String.Format("http{0}://{1}:{2}{3}", _secure ? "s" : "", _host, _port, _path);
        }

        #endregion 方法
    }
}