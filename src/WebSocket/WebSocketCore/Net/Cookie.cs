/*
 * Cookie.cs
 *
 * This code is derived from System.Net.Cookie.cs of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2004,2009 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2012-2014 sta.blockhead
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
 * - Lawrence Pit <loz@cable.a2000.nl>
 * - Gonzalo Paniagua Javier <gonzalo@ximian.com>
 * - Daniel Nauck <dna@mono-project.de>
 * - Sebastien Pouliot <sebastien@ximian.com>
 */

using System;
using System.Globalization;
using System.Text;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Provides a set of methods and properties used to manage an HTTP Cookie.
    /// </summary>
    [Serializable]
    public sealed class Cookie
    {
        #region 字段

        /// <summary>
        /// Defines the _reservedCharsForName
        /// </summary>
        private static readonly char[] _reservedCharsForName;

        /// <summary>
        /// Defines the _reservedCharsForValue
        /// </summary>
        private static readonly char[] _reservedCharsForValue;

        /// <summary>
        /// Defines the _comment
        /// </summary>
        private string _comment;

        /// <summary>
        /// Defines the _commentUri
        /// </summary>
        private Uri _commentUri;

        /// <summary>
        /// Defines the _discard
        /// </summary>
        private bool _discard;

        /// <summary>
        /// Defines the _domain
        /// </summary>
        private string _domain;

        /// <summary>
        /// Defines the _expires
        /// </summary>
        private DateTime _expires;

        /// <summary>
        /// Defines the _httpOnly
        /// </summary>
        private bool _httpOnly;

        /// <summary>
        /// Defines the _name
        /// </summary>
        private string _name;

        /// <summary>
        /// Defines the _path
        /// </summary>
        private string _path;

        /// <summary>
        /// Defines the _port
        /// </summary>
        private string _port;

        /// <summary>
        /// Defines the _ports
        /// </summary>
        private int[] _ports;

        /// <summary>
        /// Defines the _secure
        /// </summary>
        private bool _secure;

        /// <summary>
        /// Defines the _timestamp
        /// </summary>
        private DateTime _timestamp;

        /// <summary>
        /// Defines the _value
        /// </summary>
        private string _value;

        /// <summary>
        /// Defines the _version
        /// </summary>
        private int _version;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Cookie"/> class.
        /// </summary>
        public Cookie()
        {
            _comment = String.Empty;
            _domain = String.Empty;
            _expires = DateTime.MinValue;
            _name = String.Empty;
            _path = String.Empty;
            _port = String.Empty;
            _ports = new int[0];
            _timestamp = DateTime.Now;
            _value = String.Empty;
            _version = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cookie"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        public Cookie(string name, string value)
      : this()
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cookie"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        public Cookie(string name, string value, string path)
      : this(name, value)
        {
            Path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cookie"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="domain">The domain<see cref="string"/></param>
        public Cookie(string name, string value, string path, string domain)
      : this(name, value, path)
        {
            Domain = domain;
        }

        /// <summary>
        /// Initializes static members of the <see cref="Cookie"/> class.
        /// </summary>
        static Cookie()
        {
            _reservedCharsForName = new[] { ' ', '=', ';', ',', '\n', '\r', '\t' };
            _reservedCharsForValue = new[] { ';', ',' };
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the value of the Comment attribute of the cookie.
        /// </summary>
        public string Comment
        {
            get
            {
                return _comment;
            }

            set
            {
                _comment = value ?? String.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the value of the CommentURL attribute of the cookie.
        /// </summary>
        public Uri CommentUri
        {
            get
            {
                return _commentUri;
            }

            set
            {
                _commentUri = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the client discards the cookie unconditionally
        /// when the client terminates.
        /// </summary>
        public bool Discard
        {
            get
            {
                return _discard;
            }

            set
            {
                _discard = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the Domain attribute of the cookie.
        /// </summary>
        public string Domain
        {
            get
            {
                return _domain;
            }

            set
            {
                if (value.IsNullOrEmpty())
                {
                    _domain = String.Empty;
                    ExactDomain = true;
                }
                else
                {
                    _domain = value;
                    ExactDomain = value[0] != '.';
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cookie has expired.
        /// </summary>
        public bool Expired
        {
            get
            {
                return _expires != DateTime.MinValue && _expires <= DateTime.Now;
            }

            set
            {
                _expires = value ? DateTime.Now : DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets or sets the value of the Expires attribute of the cookie.
        /// </summary>
        public DateTime Expires
        {
            get
            {
                return _expires;
            }

            set
            {
                _expires = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether non-HTTP APIs can access the cookie.
        /// </summary>
        public bool HttpOnly
        {
            get
            {
                return _httpOnly;
            }

            set
            {
                _httpOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets the Name of the cookie.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                string msg;
                if (!canSetName(value, out msg))
                    throw new CookieException(msg);

                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the Path attribute of the cookie.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }

            set
            {
                _path = value ?? String.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the value of the Port attribute of the cookie.
        /// </summary>
        public string Port
        {
            get
            {
                return _port;
            }

            set
            {
                if (value.IsNullOrEmpty())
                {
                    _port = String.Empty;
                    _ports = new int[0];

                    return;
                }

                if (!value.IsEnclosedIn('"'))
                    throw new CookieException(
                      "The value specified for the Port attribute isn't enclosed in double quotes.");

                string err;
                if (!tryCreatePorts(value, out _ports, out err))
                    throw new CookieException(
                      String.Format(
                        "The value specified for the Port attribute contains an invalid value: {0}", err));

                _port = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the security level of the cookie is secure.
        /// </summary>
        public bool Secure
        {
            get
            {
                return _secure;
            }

            set
            {
                _secure = value;
            }
        }

        /// <summary>
        /// Gets the time when the cookie was issued.
        /// </summary>
        public DateTime TimeStamp
        {
            get
            {
                return _timestamp;
            }
        }

        /// <summary>
        /// Gets or sets the Value of the cookie.
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }

            set
            {
                string msg;
                if (!canSetValue(value, out msg))
                    throw new CookieException(msg);

                _value = value.Length > 0 ? value : "\"\"";
            }
        }

        /// <summary>
        /// Gets or sets the value of the Version attribute of the cookie.
        /// </summary>
        public int Version
        {
            get
            {
                return _version;
            }

            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("value", "Not 0 or 1.");

                _version = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ExactDomain
        /// </summary>
        internal bool ExactDomain { get; set; }

        /// <summary>
        /// Gets the MaxAge
        /// </summary>
        internal int MaxAge
        {
            get
            {
                if (_expires == DateTime.MinValue)
                    return 0;

                var expires = _expires.Kind != DateTimeKind.Local
                              ? _expires.ToLocalTime()
                              : _expires;

                var span = expires - DateTime.Now;
                return span > TimeSpan.Zero
                       ? (int)span.TotalSeconds
                       : 0;
            }
        }

        /// <summary>
        /// Gets the Ports
        /// </summary>
        internal int[] Ports
        {
            get
            {
                return _ports;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current
        /// <see cref="Cookie"/>.
        /// </summary>
        /// <param name="comparand">The comparand<see cref="Object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool Equals(Object comparand)
        {
            var cookie = comparand as Cookie;
            return cookie != null &&
                   _name.Equals(cookie.Name, StringComparison.InvariantCultureIgnoreCase) &&
                   _value.Equals(cookie.Value, StringComparison.InvariantCulture) &&
                   _path.Equals(cookie.Path, StringComparison.InvariantCulture) &&
                   _domain.Equals(cookie.Domain, StringComparison.InvariantCultureIgnoreCase) &&
                   _version == cookie.Version;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="Cookie"/> object.
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public override int GetHashCode()
        {
            return hash(
              StringComparer.InvariantCultureIgnoreCase.GetHashCode(_name),
              _value.GetHashCode(),
              _path.GetHashCode(),
              StringComparer.InvariantCultureIgnoreCase.GetHashCode(_domain),
              _version);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="Cookie"/>.
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            // i.e., only used for clients
            // See para 4.2.2 of RFC 2109 and para 3.3.4 of RFC 2965
            // See also bug #316017
            return ToRequestString(null);
        }

        // From client to server
        /// <summary>
        /// The ToRequestString
        /// </summary>
        /// <param name="uri">The uri<see cref="Uri"/></param>
        /// <returns>The <see cref="string"/></returns>
        internal string ToRequestString(Uri uri)
        {
            if (_name.Length == 0)
                return String.Empty;

            if (_version == 0)
                return String.Format("{0}={1}", _name, _value);

            var output = new StringBuilder(64);
            output.AppendFormat("$Version={0}; {1}={2}", _version, _name, _value);

            if (!_path.IsNullOrEmpty())
                output.AppendFormat("; $Path={0}", _path);
            else if (uri != null)
                output.AppendFormat("; $Path={0}", uri.GetAbsolutePath());
            else
                output.Append("; $Path=/");

            var appendDomain = uri == null || uri.Host != _domain;
            if (appendDomain && !_domain.IsNullOrEmpty())
                output.AppendFormat("; $Domain={0}", _domain);

            if (!_port.IsNullOrEmpty())
            {
                if (_port == "\"\"")
                    output.Append("; $Port");
                else
                    output.AppendFormat("; $Port={0}", _port);
            }

            return output.ToString();
        }

        // From server to client
        /// <summary>
        /// The ToResponseString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        internal string ToResponseString()
        {
            return _name.Length > 0
                   ? (_version == 0 ? toResponseStringVersion0() : toResponseStringVersion1())
                   : String.Empty;
        }

        /// <summary>
        /// The canSetName
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool canSetName(string name, out string message)
        {
            if (name.IsNullOrEmpty())
            {
                message = "The value specified for the Name is null or empty.";
                return false;
            }

            if (name[0] == '$' || name.Contains(_reservedCharsForName))
            {
                message = "The value specified for the Name contains an invalid character.";
                return false;
            }

            message = String.Empty;
            return true;
        }

        /// <summary>
        /// The canSetValue
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool canSetValue(string value, out string message)
        {
            if (value == null)
            {
                message = "The value specified for the Value is null.";
                return false;
            }

            if (value.Contains(_reservedCharsForValue) && !value.IsEnclosedIn('"'))
            {
                message = "The value specified for the Value contains an invalid character.";
                return false;
            }

            message = String.Empty;
            return true;
        }

        /// <summary>
        /// The hash
        /// </summary>
        /// <param name="i">The i<see cref="int"/></param>
        /// <param name="j">The j<see cref="int"/></param>
        /// <param name="k">The k<see cref="int"/></param>
        /// <param name="l">The l<see cref="int"/></param>
        /// <param name="m">The m<see cref="int"/></param>
        /// <returns>The <see cref="int"/></returns>
        private static int hash(int i, int j, int k, int l, int m)
        {
            return i ^
                   (j << 13 | j >> 19) ^
                   (k << 26 | k >> 6) ^
                   (l << 7 | l >> 25) ^
                   (m << 20 | m >> 12);
        }

        /// <summary>
        /// The tryCreatePorts
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="result">The result<see cref="int[]"/></param>
        /// <param name="parseError">The parseError<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool tryCreatePorts(string value, out int[] result, out string parseError)
        {
            var ports = value.Trim('"').Split(',');
            var len = ports.Length;
            var res = new int[len];
            for (var i = 0; i < len; i++)
            {
                res[i] = Int32.MinValue;

                var port = ports[i].Trim();
                if (port.Length == 0)
                    continue;

                if (!Int32.TryParse(port, out res[i]))
                {
                    result = new int[0];
                    parseError = port;

                    return false;
                }
            }

            result = res;
            parseError = String.Empty;

            return true;
        }

        /// <summary>
        /// The toResponseStringVersion0
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        private string toResponseStringVersion0()
        {
            var output = new StringBuilder(64);
            output.AppendFormat("{0}={1}", _name, _value);

            if (_expires != DateTime.MinValue)
                output.AppendFormat(
                  "; Expires={0}",
                  _expires.ToUniversalTime().ToString(
                    "ddd, dd'-'MMM'-'yyyy HH':'mm':'ss 'GMT'",
                    CultureInfo.CreateSpecificCulture("en-US")));

            if (!_path.IsNullOrEmpty())
                output.AppendFormat("; Path={0}", _path);

            if (!_domain.IsNullOrEmpty())
                output.AppendFormat("; Domain={0}", _domain);

            if (_secure)
                output.Append("; Secure");

            if (_httpOnly)
                output.Append("; HttpOnly");

            return output.ToString();
        }

        /// <summary>
        /// The toResponseStringVersion1
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        private string toResponseStringVersion1()
        {
            var output = new StringBuilder(64);
            output.AppendFormat("{0}={1}; Version={2}", _name, _value, _version);

            if (_expires != DateTime.MinValue)
                output.AppendFormat("; Max-Age={0}", MaxAge);

            if (!_path.IsNullOrEmpty())
                output.AppendFormat("; Path={0}", _path);

            if (!_domain.IsNullOrEmpty())
                output.AppendFormat("; Domain={0}", _domain);

            if (!_port.IsNullOrEmpty())
            {
                if (_port == "\"\"")
                    output.Append("; Port");
                else
                    output.AppendFormat("; Port={0}", _port);
            }

            if (!_comment.IsNullOrEmpty())
                output.AppendFormat("; Comment={0}", _comment.UrlEncode());

            if (_commentUri != null)
            {
                var url = _commentUri.OriginalString;
                output.AppendFormat("; CommentURL={0}", url.IsToken() ? url : url.Quote());
            }

            if (_discard)
                output.Append("; Discard");

            if (_secure)
                output.Append("; Secure");

            return output.ToString();
        }

        #endregion 方法
    }
}