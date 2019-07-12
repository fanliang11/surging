/*
 * HttpDigestIdentity.cs
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
using System.Collections.Specialized;
using System.Security.Principal;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Holds the username and other parameters from
    /// an HTTP Digest authentication attempt.
    /// </summary>
    public class HttpDigestIdentity : GenericIdentity
    {
        #region 字段

        /// <summary>
        /// Defines the _parameters
        /// </summary>
        private NameValueCollection _parameters;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDigestIdentity"/> class.
        /// </summary>
        /// <param name="parameters">The parameters<see cref="NameValueCollection"/></param>
        internal HttpDigestIdentity(NameValueCollection parameters)
      : base(parameters["username"], "Digest")
        {
            _parameters = parameters;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the algorithm parameter from a digest authentication attempt.
        /// </summary>
        public string Algorithm
        {
            get
            {
                return _parameters["algorithm"];
            }
        }

        /// <summary>
        /// Gets the cnonce parameter from a digest authentication attempt.
        /// </summary>
        public string Cnonce
        {
            get
            {
                return _parameters["cnonce"];
            }
        }

        /// <summary>
        /// Gets the nc parameter from a digest authentication attempt.
        /// </summary>
        public string Nc
        {
            get
            {
                return _parameters["nc"];
            }
        }

        /// <summary>
        /// Gets the nonce parameter from a digest authentication attempt.
        /// </summary>
        public string Nonce
        {
            get
            {
                return _parameters["nonce"];
            }
        }

        /// <summary>
        /// Gets the opaque parameter from a digest authentication attempt.
        /// </summary>
        public string Opaque
        {
            get
            {
                return _parameters["opaque"];
            }
        }

        /// <summary>
        /// Gets the qop parameter from a digest authentication attempt.
        /// </summary>
        public string Qop
        {
            get
            {
                return _parameters["qop"];
            }
        }

        /// <summary>
        /// Gets the realm parameter from a digest authentication attempt.
        /// </summary>
        public string Realm
        {
            get
            {
                return _parameters["realm"];
            }
        }

        /// <summary>
        /// Gets the response parameter from a digest authentication attempt.
        /// </summary>
        public string Response
        {
            get
            {
                return _parameters["response"];
            }
        }

        /// <summary>
        /// Gets the uri parameter from a digest authentication attempt.
        /// </summary>
        public string Uri
        {
            get
            {
                return _parameters["uri"];
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The IsValid
        /// </summary>
        /// <param name="password">The password<see cref="string"/></param>
        /// <param name="realm">The realm<see cref="string"/></param>
        /// <param name="method">The method<see cref="string"/></param>
        /// <param name="entity">The entity<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        internal bool IsValid(
      string password, string realm, string method, string entity
    )
        {
            var copied = new NameValueCollection(_parameters);
            copied["password"] = password;
            copied["realm"] = realm;
            copied["method"] = method;
            copied["entity"] = entity;

            var expected = AuthenticationResponse.CreateRequestDigest(copied);
            return _parameters["response"] == expected;
        }

        #endregion 方法
    }
}