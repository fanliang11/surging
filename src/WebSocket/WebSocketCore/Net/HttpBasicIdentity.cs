/*
 * HttpBasicIdentity.cs
 *
 * This code is derived from HttpListenerBasicIdentity.cs (System.Net) of
 * Mono (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2014-2017 sta.blockhead
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
using System.Security.Principal;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Holds the username and password from an HTTP Basic authentication attempt.
    /// </summary>
    public class HttpBasicIdentity : GenericIdentity
    {
        #region 字段

        /// <summary>
        /// Defines the _password
        /// </summary>
        private string _password;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpBasicIdentity"/> class.
        /// </summary>
        /// <param name="username">The username<see cref="string"/></param>
        /// <param name="password">The password<see cref="string"/></param>
        internal HttpBasicIdentity(string username, string password)
      : base(username, "Basic")
        {
            _password = password;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the password from a basic authentication attempt.
        /// </summary>
        public virtual string Password
        {
            get
            {
                return _password;
            }
        }

        #endregion 属性
    }
}