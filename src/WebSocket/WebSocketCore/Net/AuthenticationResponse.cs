/*
 * AuthenticationResponse.cs
 *
 * ParseBasicCredentials is derived from System.Net.HttpListenerContext.cs of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2013-2014 sta.blockhead
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
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Defines the <see cref="AuthenticationResponse" />
    /// </summary>
    internal class AuthenticationResponse : AuthenticationBase
    {
        #region 字段

        /// <summary>
        /// Defines the _nonceCount
        /// </summary>
        private uint _nonceCount;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResponse"/> class.
        /// </summary>
        /// <param name="challenge">The challenge<see cref="AuthenticationChallenge"/></param>
        /// <param name="credentials">The credentials<see cref="NetworkCredential"/></param>
        /// <param name="nonceCount">The nonceCount<see cref="uint"/></param>
        internal AuthenticationResponse(
      AuthenticationChallenge challenge, NetworkCredential credentials, uint nonceCount)
      : this(challenge.Scheme, challenge.Parameters, credentials, nonceCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResponse"/> class.
        /// </summary>
        /// <param name="scheme">The scheme<see cref="AuthenticationSchemes"/></param>
        /// <param name="parameters">The parameters<see cref="NameValueCollection"/></param>
        /// <param name="credentials">The credentials<see cref="NetworkCredential"/></param>
        /// <param name="nonceCount">The nonceCount<see cref="uint"/></param>
        internal AuthenticationResponse(
      AuthenticationSchemes scheme,
      NameValueCollection parameters,
      NetworkCredential credentials,
      uint nonceCount)
      : base(scheme, parameters)
        {
            Parameters["username"] = credentials.Username;
            Parameters["password"] = credentials.Password;
            Parameters["uri"] = credentials.Domain;
            _nonceCount = nonceCount;
            if (scheme == AuthenticationSchemes.Digest)
                initAsDigest();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResponse"/> class.
        /// </summary>
        /// <param name="credentials">The credentials<see cref="NetworkCredential"/></param>
        internal AuthenticationResponse(NetworkCredential credentials)
      : this(AuthenticationSchemes.Basic, new NameValueCollection(), credentials, 0)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="AuthenticationResponse"/> class from being created.
        /// </summary>
        /// <param name="scheme">The scheme<see cref="AuthenticationSchemes"/></param>
        /// <param name="parameters">The parameters<see cref="NameValueCollection"/></param>
        private AuthenticationResponse(AuthenticationSchemes scheme, NameValueCollection parameters)
      : base(scheme, parameters)
        {
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Cnonce
        /// </summary>
        public string Cnonce
        {
            get
            {
                return Parameters["cnonce"];
            }
        }

        /// <summary>
        /// Gets the Nc
        /// </summary>
        public string Nc
        {
            get
            {
                return Parameters["nc"];
            }
        }

        /// <summary>
        /// Gets the Password
        /// </summary>
        public string Password
        {
            get
            {
                return Parameters["password"];
            }
        }

        /// <summary>
        /// Gets the Response
        /// </summary>
        public string Response
        {
            get
            {
                return Parameters["response"];
            }
        }

        /// <summary>
        /// Gets the Uri
        /// </summary>
        public string Uri
        {
            get
            {
                return Parameters["uri"];
            }
        }

        /// <summary>
        /// Gets the UserName
        /// </summary>
        public string UserName
        {
            get
            {
                return Parameters["username"];
            }
        }

        /// <summary>
        /// Gets the NonceCount
        /// </summary>
        internal uint NonceCount
        {
            get
            {
                return _nonceCount < UInt32.MaxValue
                       ? _nonceCount
                       : 0;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The ToIdentity
        /// </summary>
        /// <returns>The <see cref="IIdentity"/></returns>
        public IIdentity ToIdentity()
        {
            var schm = Scheme;
            return schm == AuthenticationSchemes.Basic
                   ? new HttpBasicIdentity(Parameters["username"], Parameters["password"]) as IIdentity
                   : schm == AuthenticationSchemes.Digest
                     ? new HttpDigestIdentity(Parameters)
                     : null;
        }

        /// <summary>
        /// The CreateRequestDigest
        /// </summary>
        /// <param name="parameters">The parameters<see cref="NameValueCollection"/></param>
        /// <returns>The <see cref="string"/></returns>
        internal static string CreateRequestDigest(NameValueCollection parameters)
        {
            var user = parameters["username"];
            var pass = parameters["password"];
            var realm = parameters["realm"];
            var nonce = parameters["nonce"];
            var uri = parameters["uri"];
            var algo = parameters["algorithm"];
            var qop = parameters["qop"];
            var cnonce = parameters["cnonce"];
            var nc = parameters["nc"];
            var method = parameters["method"];

            var a1 = algo != null && algo.ToLower() == "md5-sess"
                     ? createA1(user, pass, realm, nonce, cnonce)
                     : createA1(user, pass, realm);

            var a2 = qop != null && qop.ToLower() == "auth-int"
                     ? createA2(method, uri, parameters["entity"])
                     : createA2(method, uri);

            var secret = hash(a1);
            var data = qop != null
                       ? String.Format("{0}:{1}:{2}:{3}:{4}", nonce, nc, cnonce, qop, hash(a2))
                       : String.Format("{0}:{1}", nonce, hash(a2));

            return hash(String.Format("{0}:{1}", secret, data));
        }

        /// <summary>
        /// The Parse
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="AuthenticationResponse"/></returns>
        internal static AuthenticationResponse Parse(string value)
        {
            try
            {
                var cred = value.Split(new[] { ' ' }, 2);
                if (cred.Length != 2)
                    return null;

                var schm = cred[0].ToLower();
                return schm == "basic"
                       ? new AuthenticationResponse(
                           AuthenticationSchemes.Basic, ParseBasicCredentials(cred[1]))
                       : schm == "digest"
                         ? new AuthenticationResponse(
                             AuthenticationSchemes.Digest, ParseParameters(cred[1]))
                         : null;
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// The ParseBasicCredentials
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="NameValueCollection"/></returns>
        internal static NameValueCollection ParseBasicCredentials(string value)
        {
            // Decode the basic-credentials (a Base64 encoded string).
            var userPass = Encoding.Default.GetString(Convert.FromBase64String(value));

            // The format is [<domain>\]<username>:<password>.
            var i = userPass.IndexOf(':');
            var user = userPass.Substring(0, i);
            var pass = i < userPass.Length - 1 ? userPass.Substring(i + 1) : String.Empty;

            // Check if 'domain' exists.
            i = user.IndexOf('\\');
            if (i > -1)
                user = user.Substring(i + 1);

            var res = new NameValueCollection();
            res["username"] = user;
            res["password"] = pass;

            return res;
        }

        /// <summary>
        /// The ToBasicString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        internal override string ToBasicString()
        {
            var userPass = String.Format("{0}:{1}", Parameters["username"], Parameters["password"]);
            var cred = Convert.ToBase64String(Encoding.UTF8.GetBytes(userPass));

            return "Basic " + cred;
        }

        /// <summary>
        /// The ToDigestString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        internal override string ToDigestString()
        {
            var output = new StringBuilder(256);
            output.AppendFormat(
              "Digest username=\"{0}\", realm=\"{1}\", nonce=\"{2}\", uri=\"{3}\", response=\"{4}\"",
              Parameters["username"],
              Parameters["realm"],
              Parameters["nonce"],
              Parameters["uri"],
              Parameters["response"]);

            var opaque = Parameters["opaque"];
            if (opaque != null)
                output.AppendFormat(", opaque=\"{0}\"", opaque);

            var algo = Parameters["algorithm"];
            if (algo != null)
                output.AppendFormat(", algorithm={0}", algo);

            var qop = Parameters["qop"];
            if (qop != null)
                output.AppendFormat(
                  ", qop={0}, cnonce=\"{1}\", nc={2}", qop, Parameters["cnonce"], Parameters["nc"]);

            return output.ToString();
        }

        /// <summary>
        /// The createA1
        /// </summary>
        /// <param name="username">The username<see cref="string"/></param>
        /// <param name="password">The password<see cref="string"/></param>
        /// <param name="realm">The realm<see cref="string"/></param>
        /// <param name="nonce">The nonce<see cref="string"/></param>
        /// <param name="cnonce">The cnonce<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string createA1(
      string username, string password, string realm, string nonce, string cnonce)
        {
            return String.Format(
              "{0}:{1}:{2}", hash(createA1(username, password, realm)), nonce, cnonce);
        }

        /// <summary>
        /// The createA1
        /// </summary>
        /// <param name="username">The username<see cref="string"/></param>
        /// <param name="password">The password<see cref="string"/></param>
        /// <param name="realm">The realm<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string createA1(string username, string password, string realm)
        {
            return String.Format("{0}:{1}:{2}", username, realm, password);
        }

        /// <summary>
        /// The createA2
        /// </summary>
        /// <param name="method">The method<see cref="string"/></param>
        /// <param name="uri">The uri<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string createA2(string method, string uri)
        {
            return String.Format("{0}:{1}", method, uri);
        }

        /// <summary>
        /// The createA2
        /// </summary>
        /// <param name="method">The method<see cref="string"/></param>
        /// <param name="uri">The uri<see cref="string"/></param>
        /// <param name="entity">The entity<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string createA2(string method, string uri, string entity)
        {
            return String.Format("{0}:{1}:{2}", method, uri, hash(entity));
        }

        /// <summary>
        /// The hash
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string hash(string value)
        {
            var src = Encoding.UTF8.GetBytes(value);
            var md5 = MD5.Create();
            var hashed = md5.ComputeHash(src);

            var res = new StringBuilder(64);
            foreach (var b in hashed)
                res.Append(b.ToString("x2"));

            return res.ToString();
        }

        /// <summary>
        /// The initAsDigest
        /// </summary>
        private void initAsDigest()
        {
            var qops = Parameters["qop"];
            if (qops != null)
            {
                if (qops.Split(',').Contains(qop => qop.Trim().ToLower() == "auth"))
                {
                    Parameters["qop"] = "auth";
                    Parameters["cnonce"] = CreateNonceValue();
                    Parameters["nc"] = String.Format("{0:x8}", ++_nonceCount);
                }
                else
                {
                    Parameters["qop"] = null;
                }
            }

            Parameters["method"] = "GET";
            Parameters["response"] = CreateRequestDigest(Parameters);
        }

        #endregion 方法
    }
}