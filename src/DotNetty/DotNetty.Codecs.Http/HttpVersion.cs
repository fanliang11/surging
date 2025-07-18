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
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    public class HttpVersion : IEquatable<HttpVersion>, IComparable<HttpVersion>, IComparable
    {
        const byte OneByte = (byte)'1';
        const byte ZeroByte = (byte)'0';

        static readonly Regex VersionPattern = new Regex("^(\\S+)/(\\d+)\\.(\\d+)$", RegexOptions.Compiled);

        internal static readonly AsciiString Http10String = new AsciiString("HTTP/1.0");
        internal static readonly AsciiString Http11String = new AsciiString("HTTP/1.1");

        public static readonly HttpVersion Http10 = new HttpVersion("HTTP", 1, 0, false, true);
        public static readonly HttpVersion Http11 = new HttpVersion("HTTP", 1, 1, true, true);

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static HttpVersion ValueOf(AsciiString text)
        {
            if (text is null)
            {
                ThrowHelper.ThrowArgumentException_NullText();
            }

            // ReSharper disable once PossibleNullReferenceException
            HttpVersion version = ValueOfInline(text.Array);
            if (version is object)
            {
                return version;
            }

            // Fall back to slow path
            text = text.Trim();

            if (0u >= (uint)text.Count)
            {
                ThrowHelper.ThrowArgumentException_EmptyText();
            }

            // Try to match without convert to uppercase first as this is what 99% of all clients
            // will send anyway. Also there is a change to the RFC to make it clear that it is
            // expected to be case-sensitive
            //
            // See:
            // * http://trac.tools.ietf.org/wg/httpbis/trac/ticket/1
            // * http://trac.tools.ietf.org/wg/httpbis/trac/wiki
            //
            return Version0(text) ?? new HttpVersion(text.ToString(), true);
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        static HttpVersion ValueOfInline(byte[] bytes)
        {
            if ((uint)bytes.Length != 8u) return null;

            var http11Bytes = HttpUtil.Http11Bytes;

            if (bytes[0] != http11Bytes[0]) return null;
            if (bytes[1] != http11Bytes[1]) return null;
            if (bytes[2] != http11Bytes[2]) return null;
            if (bytes[3] != http11Bytes[3]) return null;
            if (bytes[4] != http11Bytes[4]) return null;
            if (bytes[5] != http11Bytes[5]) return null;
            if (bytes[6] != http11Bytes[6]) return null;
            return (bytes[7]) switch
            {
                OneByte => Http11,
                ZeroByte => Http10,
                _ => null,
            };
        }

        static HttpVersion Version0(AsciiString text)
        {
            if (Http11String.Equals(text))
            {
                return Http11;
            }
            if (Http10String.Equals(text))
            {
                return Http10;
            }

            return null;
        }

        readonly string protocolName;
        readonly int majorVersion;
        readonly int minorVersion;
        readonly AsciiString text;
        readonly bool keepAliveDefault;
        readonly byte[] bytes;

        public HttpVersion(string text, bool keepAliveDefault)
        {
            if (text is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.text); }

            text = text.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(text))
            {
                ThrowHelper.ThrowArgumentException_Empty(ExceptionArgument.text);
            }

            Match match = VersionPattern.Match(text);
            if (!match.Success)
            {
                ThrowHelper.ThrowArgumentException_InvalidVersion(text);
            }

            this.protocolName = match.Groups[1].Value;
            this.majorVersion = int.Parse(match.Groups[2].Value);
            this.minorVersion = int.Parse(match.Groups[3].Value);
            this.text = new AsciiString($"{this.ProtocolName}/{this.MajorVersion}.{this.MinorVersion}");
            this.keepAliveDefault = keepAliveDefault;
            this.bytes = null;
        }

        HttpVersion(string protocolName, int majorVersion, int minorVersion, bool keepAliveDefault, bool bytes)
        {
            if (protocolName is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.protocolName);
            }

            protocolName = protocolName.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(protocolName))
            {
                ThrowHelper.ThrowArgumentException_Empty(ExceptionArgument.protocolName);
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < protocolName.Length; i++)
            {
                char c = protocolName[i];
                if (CharUtil.IsISOControl(c) || char.IsWhiteSpace(c))
                {
                    ThrowHelper.ThrowArgumentException_InvalidProtocolName(c);
                }
            }

            if ((uint)majorVersion > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentException_PositiveOrZero(majorVersion, ExceptionArgument.majorVersion);
            }
            if ((uint)minorVersion > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentException_PositiveOrZero(minorVersion, ExceptionArgument.minorVersion);
            }

            this.protocolName = protocolName;
            this.majorVersion = majorVersion;
            this.minorVersion = minorVersion;
            this.text = new AsciiString(protocolName + '/' + majorVersion + '.' + minorVersion);
            this.keepAliveDefault = keepAliveDefault;

            this.bytes = bytes ? this.text.Array : null;
        }

        public string ProtocolName => this.protocolName;

        public int MajorVersion => this.majorVersion;

        public int MinorVersion => this.minorVersion;

        public AsciiString Text => this.text;

        public bool IsKeepAliveDefault => this.keepAliveDefault;

        public override string ToString() => this.text.ToString();

        public override int GetHashCode() => (this.protocolName.GetHashCode() * 31 + this.majorVersion) * 31 + this.minorVersion;

        public override bool Equals(object obj)
        {
            if (obj is HttpVersion that)
            {
                return this.minorVersion == that.minorVersion
                    && this.majorVersion == that.majorVersion
                    && string.Equals(this.protocolName, that.protocolName
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                        );
#else
                        , StringComparison.Ordinal);
#endif
            }

            return false;
        }

        public bool Equals(HttpVersion other)
        {
            if (ReferenceEquals(this, other)) { return true; }

            return other is object
                && this.minorVersion == other.minorVersion
                && this.majorVersion == other.majorVersion
                && string.Equals(this.protocolName, other.protocolName
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
                    );
#else
                    , StringComparison.Ordinal);
#endif
        }

        public int CompareTo(HttpVersion other)
        {
            int v = string.CompareOrdinal(this.protocolName, other.protocolName);
            if (v != 0)
            {
                return v;
            }

            v = this.majorVersion - other.majorVersion;
            if (v != 0)
            {
                return v;
            }

            return this.minorVersion - other.minorVersion;
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            if (obj is HttpVersion httpVersion)
            {
                return this.CompareTo(httpVersion);
            }

            return ThrowHelper.FromArgumentException_CompareToHttpVersion();
        }

        internal void Encode(IByteBuffer buf)
        {
            if (this.bytes is null)
            {
                _ = buf.WriteCharSequence(this.text, Encoding.ASCII);
            }
            else
            {
                _ = buf.WriteBytes(this.bytes);
            }
        }
    }
}
