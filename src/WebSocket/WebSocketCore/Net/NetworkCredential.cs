/*
 * NetworkCredential.cs
 *
 * The MIT License
 *
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

using System;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Provides the credentials for the password-based authentication.
    /// </summary>
    public class NetworkCredential
    {
        #region 字段

        /// <summary>
        /// Defines the _noRoles
        /// </summary>
        private static readonly string[] _noRoles;

        /// <summary>
        /// Defines the _domain
        /// </summary>
        private string _domain;

        /// <summary>
        /// Defines the _password
        /// </summary>
        private string _password;

        /// <summary>
        /// Defines the _roles
        /// </summary>
        private string[] _roles;

        /// <summary>
        /// Defines the _username
        /// </summary>
        private string _username;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkCredential"/> class.
        /// </summary>
        /// <param name="username">The username<see cref="string"/></param>
        /// <param name="password">The password<see cref="string"/></param>
        /// <param name="domain">The domain<see cref="string"/></param>
        /// <param name="roles">The roles<see cref="string[]"/></param>
        public NetworkCredential(
      string username, string password, string domain, params string[] roles
    )
        {
            if (username == null)
                throw new ArgumentNullException("username");

            if (username.Length == 0)
                throw new ArgumentException("An empty string.", "username");

            _username = username;
            _password = password;
            _domain = domain;
            _roles = roles;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkCredential"/> class.
        /// </summary>
        /// <param name="username">The username<see cref="string"/></param>
        /// <param name="password">The password<see cref="string"/></param>
        public NetworkCredential(string username, string password)
      : this(username, password, null, null)
        {
        }

        /// <summary>
        /// Initializes static members of the <see cref="NetworkCredential"/> class.
        /// </summary>
        static NetworkCredential()
        {
            _noRoles = new string[0];
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Domain
        /// Gets the domain associated with the credentials.
        /// </summary>
        public string Domain
        {
            get
            {
                return _domain ?? String.Empty;
            }

            internal set
            {
                _domain = value;
            }
        }

        /// <summary>
        /// Gets or sets the Password
        /// Gets the password for the username associated with the credentials.
        /// </summary>
        public string Password
        {
            get
            {
                return _password ?? String.Empty;
            }

            internal set
            {
                _password = value;
            }
        }

        /// <summary>
        /// Gets or sets the Roles
        /// Gets the roles associated with the credentials.
        /// </summary>
        public string[] Roles
        {
            get
            {
                return _roles ?? _noRoles;
            }

            internal set
            {
                _roles = value;
            }
        }

        /// <summary>
        /// Gets or sets the Username
        /// Gets the username associated with the credentials.
        /// </summary>
        public string Username
        {
            get
            {
                return _username;
            }

            internal set
            {
                _username = value;
            }
        }

        #endregion 属性
    }
}