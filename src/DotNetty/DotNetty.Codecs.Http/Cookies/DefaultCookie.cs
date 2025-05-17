/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.Cookies
{
    using System;
    using System.Text;

    using static CookieUtil;

    public sealed class DefaultCookie : ICookie
    {
        // Constant for undefined MaxAge attribute value.
        const long UndefinedMaxAge = long.MinValue;

        private readonly string _name;
        private string _value;
        private bool _wrap;
        private string _domain;
        private string _path;
        private long _maxAge = UndefinedMaxAge;
        private bool _secure;
        private bool _httpOnly;
        private SameSite? _sameSite;

        public DefaultCookie(string name, string value)
        {
            if (string.IsNullOrEmpty(name?.Trim())) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            _name = name;
            _value = value;
        }

        public string Name => _name;

        public string Value
        {
            get => _value;
            set
            {
                if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
                _value = value;
            }
        }

        public bool Wrap
        {
            get => _wrap;
            set => _wrap = value;
        }

        public string Domain
        {
            get => _domain;
            set => _domain = ValidateAttributeValue(nameof(_domain), value);
        }

        public string Path
        {
            get => _path;
            set => _path = ValidateAttributeValue(nameof(_path), value);
        }

        public long MaxAge
        {
            get => _maxAge;
            set => _maxAge = value;
        }

        public bool IsSecure
        {
            get => _secure;
            set => _secure = value;
        }

        public bool IsHttpOnly
        {
            get => _httpOnly;
            set => _httpOnly = value;
        }

        /// <summary>
        /// Checks to see if this <see cref="ICookie"/> can be sent along cross-site requests.
        /// For more information, please look
        /// <a href="https://tools.ietf.org/html/draft-ietf-httpbis-rfc6265bis-05">here</a>
        /// </summary>
        public SameSite? SameSite
        {
            get => _sameSite;
            set => _sameSite = value;
        }

        public override int GetHashCode() => _name.GetHashCode();

        public override bool Equals(object obj) => obj is DefaultCookie cookie && Equals(cookie);

        public bool Equals(ICookie other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!string.Equals(_name, other.Name
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                    ))
#else
                    , StringComparison.Ordinal))
#endif
            {
                return false;
            }

            if (_path is null)
            {
                if (other.Path is object)
                {
                    return false;
                }
            }
            else if (other.Path is null)
            {
                return false;
            }
            else if (!string.Equals(_path, other.Path
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                    ))
#else
                    , StringComparison.Ordinal))
#endif
            {
                return false;
            }

            if (_domain is null)
            {
                if (other.Domain is object)
                {
                    return false;
                }
            }
            else
            {
                return string.Equals(_domain, other.Domain, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        public int CompareTo(ICookie other)
        {
            int v = string.Compare(_name, other.Name, StringComparison.Ordinal);
            if (v != 0)
            {
                return v;
            }

            if (_path is null)
            {
                if (other.Path is object)
                {
                    return -1;
                }
            }
            else if (other.Path is null)
            {
                return 1;
            }
            else
            {
                v = string.Compare(_path, other.Path, StringComparison.Ordinal);
                if (v != 0)
                {
                    return v;
                }
            }

            if (_domain is null)
            {
                if (other.Domain is object)
                {
                    return -1;
                }
            }
            else if (other.Domain is null)
            {
                return 1;
            }
            else
            {
                v = string.Compare(_domain, other.Domain, StringComparison.OrdinalIgnoreCase);
                return v;
            }

            return 0;
        }

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;

                case ICookie cookie:
                    return CompareTo(cookie);

                default:
                    return ThrowHelper.FromArgumentException_CompareToCookie();
            }
        }

        public override string ToString()
        {
            StringBuilder buf = StringBuilder();
            _ = buf.Append($"{_name}={Value}");
            if (_domain is object)
            {
                _ = buf.Append($", domain={_domain}");
            }
            if (_path is object)
            {
                _ = buf.Append($", path={_path}");
            }
            if (_maxAge >= 0)
            {
                _ = buf.Append($", maxAge={_maxAge}s");
            }
            if (_secure)
            {
                _ = buf.Append(", secure");
            }
            if (_httpOnly)
            {
                _ = buf.Append(", HTTPOnly");
            }
            if (_sameSite.HasValue)
            {
                _ = buf.Append(", SameSite=").Append(_sameSite.Value);
            }
            return buf.ToString();
        }
    }
}
