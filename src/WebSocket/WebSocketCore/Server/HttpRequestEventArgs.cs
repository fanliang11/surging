/*
 * HttpRequestEventArgs.cs
 *
 * The MIT License
 *
 * Copyright (c) 2012-2017 sta.blockhead
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

using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using WebSocketCore.Net;

namespace WebSocketCore.Server
{
    /// <summary>
    /// Represents the event data for the HTTP request events of
    /// the <see cref="HttpServer"/>.
    /// </summary>
    public class HttpRequestEventArgs : EventArgs
    {
        #region 字段

        /// <summary>
        /// Defines the _context
        /// </summary>
        private HttpListenerContext _context;

        /// <summary>
        /// Defines the _docRootPath
        /// </summary>
        private string _docRootPath;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestEventArgs"/> class.
        /// </summary>
        /// <param name="context">The context<see cref="HttpListenerContext"/></param>
        /// <param name="documentRootPath">The documentRootPath<see cref="string"/></param>
        internal HttpRequestEventArgs(
      HttpListenerContext context, string documentRootPath
    )
        {
            _context = context;
            _docRootPath = documentRootPath;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the request data sent from a client.
        /// </summary>
        public HttpListenerRequest Request
        {
            get
            {
                return _context.Request;
            }
        }

        /// <summary>
        /// Gets the response data to return to the client.
        /// </summary>
        public HttpListenerResponse Response
        {
            get
            {
                return _context.Response;
            }
        }

        /// <summary>
        /// Gets the information for the client.
        /// </summary>
        public IPrincipal User
        {
            get
            {
                return _context.User;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Reads the specified file from the document folder of
        /// the <see cref="HttpServer"/>.
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        public byte[] ReadFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Length == 0)
                throw new ArgumentException("An empty string.", "path");

            if (path.IndexOf("..") > -1)
                throw new ArgumentException("It contains '..'.", "path");

            byte[] contents;
            tryReadFile(createFilePath(path), out contents);

            return contents;
        }

        /// <summary>
        /// Tries to read the specified file from the document folder of
        /// the <see cref="HttpServer"/>.
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="contents">The contents<see cref="byte[]"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool TryReadFile(string path, out byte[] contents)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Length == 0)
                throw new ArgumentException("An empty string.", "path");

            if (path.IndexOf("..") > -1)
                throw new ArgumentException("It contains '..'.", "path");

            return tryReadFile(createFilePath(path), out contents);
        }

        /// <summary>
        /// The tryReadFile
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="contents">The contents<see cref="byte[]"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool tryReadFile(string path, out byte[] contents)
        {
            contents = null;

            if (!File.Exists(path))
                return false;

            try
            {
                contents = File.ReadAllBytes(path);
            }
            catch
            {
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

        #endregion 方法
    }
}