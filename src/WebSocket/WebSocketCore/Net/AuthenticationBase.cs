/*
 * AuthenticationBase.cs
 *
 * The MIT License
 *
 * Copyright (c) 2014 sta.blockhead
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
using System.Text;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Defines the <see cref="AuthenticationBase" />
    /// </summary>
    internal abstract class AuthenticationBase
    {
        #region 字段

        /// <summary>
        /// Defines the Parameters
        /// </summary>
        internal NameValueCollection Parameters;

        /// <summary>
        /// Defines the _scheme
        /// </summary>
        private AuthenticationSchemes _scheme;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationBase"/> class.
        /// </summary>
        /// <param name="scheme">The scheme<see cref="AuthenticationSchemes"/></param>
        /// <param name="parameters">The parameters<see cref="NameValueCollection"/></param>
        protected AuthenticationBase(AuthenticationSchemes scheme, NameValueCollection parameters)
        {
            _scheme = scheme;
            Parameters = parameters;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Algorithm
        /// </summary>
        public string Algorithm
        {
            get
            {
                return Parameters["algorithm"];
            }
        }

        /// <summary>
        /// Gets the Nonce
        /// </summary>
        public string Nonce
        {
            get
            {
                return Parameters["nonce"];
            }
        }

        /// <summary>
        /// Gets the Opaque
        /// </summary>
        public string Opaque
        {
            get
            {
                return Parameters["opaque"];
            }
        }

        /// <summary>
        /// Gets the Qop
        /// </summary>
        public string Qop
        {
            get
            {
                return Parameters["qop"];
            }
        }

        /// <summary>
        /// Gets the Realm
        /// </summary>
        public string Realm
        {
            get
            {
                return Parameters["realm"];
            }
        }

        /// <summary>
        /// Gets the Scheme
        /// </summary>
        public AuthenticationSchemes Scheme
        {
            get
            {
                return _scheme;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return _scheme == AuthenticationSchemes.Basic
                   ? ToBasicString()
                   : _scheme == AuthenticationSchemes.Digest
                     ? ToDigestString()
                     : String.Empty;
        }

        /// <summary>
        /// The CreateNonceValue
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        internal static string CreateNonceValue()
        {
            var src = new byte[16];
            var rand = new Random();
            rand.NextBytes(src);

            var res = new StringBuilder(32);
            foreach (var b in src)
                res.Append(b.ToString("x2"));

            return res.ToString();
        }

        /// <summary>
        /// The ParseParameters
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="NameValueCollection"/></returns>
        internal static NameValueCollection ParseParameters(string value)
        {
            var res = new NameValueCollection();
            foreach (var param in value.SplitHeaderValue(','))
            {
                var i = param.IndexOf('=');
                var name = i > 0 ? param.Substring(0, i).Trim() : null;
                var val = i < 0
                          ? param.Trim().Trim('"')
                          : i < param.Length - 1
                            ? param.Substring(i + 1).Trim().Trim('"')
                            : String.Empty;

                res.Add(name, val);
            }

            return res;
        }

        /// <summary>
        /// The ToBasicString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        internal abstract string ToBasicString();

        /// <summary>
        /// The ToDigestString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        internal abstract string ToDigestString();

        #endregion 方法
    }
}