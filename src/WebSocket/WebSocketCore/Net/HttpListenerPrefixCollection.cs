/*
 * HttpListenerPrefixCollection.cs
 *
 * This code is derived from HttpListenerPrefixCollection.cs (System.Net) of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
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
 * Authors:
 * - Gonzalo Paniagua Javier <gonzalo@novell.com>
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Provides the collection used to store the URI prefixes for the <see cref="HttpListener"/>.
    /// </summary>
    public class HttpListenerPrefixCollection : ICollection<string>, IEnumerable<string>, IEnumerable
    {
        #region 字段

        /// <summary>
        /// Defines the _listener
        /// </summary>
        private HttpListener _listener;

        /// <summary>
        /// Defines the _prefixes
        /// </summary>
        private List<string> _prefixes;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerPrefixCollection"/> class.
        /// </summary>
        /// <param name="listener">The listener<see cref="HttpListener"/></param>
        internal HttpListenerPrefixCollection(HttpListener listener)
        {
            _listener = listener;
            _prefixes = new List<string>();
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the number of prefixes in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return _prefixes.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the access to the collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the access to the collection is synchronized.
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Adds the specified <paramref name="uriPrefix"/> to the collection.
        /// </summary>
        /// <param name="uriPrefix">The uriPrefix<see cref="string"/></param>
        public void Add(string uriPrefix)
        {
            _listener.CheckDisposed();
            HttpListenerPrefix.CheckPrefix(uriPrefix);
            if (_prefixes.Contains(uriPrefix))
                return;

            _prefixes.Add(uriPrefix);
            if (_listener.IsListening)
                EndPointManager.AddPrefix(uriPrefix, _listener);
        }

        /// <summary>
        /// Removes all URI prefixes from the collection.
        /// </summary>
        public void Clear()
        {
            _listener.CheckDisposed();
            _prefixes.Clear();
            if (_listener.IsListening)
                EndPointManager.RemoveListener(_listener);
        }

        /// <summary>
        /// Returns a value indicating whether the collection contains the specified
        /// <paramref name="uriPrefix"/>.
        /// </summary>
        /// <param name="uriPrefix">The uriPrefix<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool Contains(string uriPrefix)
        {
            _listener.CheckDisposed();
            if (uriPrefix == null)
                throw new ArgumentNullException("uriPrefix");

            return _prefixes.Contains(uriPrefix);
        }

        /// <summary>
        /// Copies the contents of the collection to the specified <see cref="Array"/>.
        /// </summary>
        /// <param name="array">The array<see cref="Array"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        public void CopyTo(Array array, int offset)
        {
            _listener.CheckDisposed();
            ((ICollection)_prefixes).CopyTo(array, offset);
        }

        /// <summary>
        /// Copies the contents of the collection to the specified array of <see cref="string"/>.
        /// </summary>
        /// <param name="array">The array<see cref="string[]"/></param>
        /// <param name="offset">The offset<see cref="int"/></param>
        public void CopyTo(string[] array, int offset)
        {
            _listener.CheckDisposed();
            _prefixes.CopyTo(array, offset);
        }

        /// <summary>
        /// Gets the enumerator used to iterate through the <see cref="HttpListenerPrefixCollection"/>.
        /// </summary>
        /// <returns>The <see cref="IEnumerator{string}"/></returns>
        public IEnumerator<string> GetEnumerator()
        {
            return _prefixes.GetEnumerator();
        }

        /// <summary>
        /// Removes the specified <paramref name="uriPrefix"/> from the collection.
        /// </summary>
        /// <param name="uriPrefix">The uriPrefix<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool Remove(string uriPrefix)
        {
            _listener.CheckDisposed();
            if (uriPrefix == null)
                throw new ArgumentNullException("uriPrefix");

            var ret = _prefixes.Remove(uriPrefix);
            if (ret && _listener.IsListening)
                EndPointManager.RemovePrefix(uriPrefix, _listener);

            return ret;
        }

        /// <summary>
        /// Gets the enumerator used to iterate through the <see cref="HttpListenerPrefixCollection"/>.
        /// </summary>
        /// <returns>The <see cref="IEnumerator"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _prefixes.GetEnumerator();
        }

        #endregion 方法
    }
}