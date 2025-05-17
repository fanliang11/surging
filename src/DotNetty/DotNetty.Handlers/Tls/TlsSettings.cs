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

namespace DotNetty.Handlers.Tls
{
    using System.Security.Authentication;
#if NETCOREAPP_2_0_GREATER || NETSTANDARD_2_0_GREATER
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
#endif

    public abstract class TlsSettings
    {
        protected TlsSettings(SslProtocols enabledProtocols, bool checkCertificateRevocation)
        {
            EnabledProtocols = enabledProtocols;
            CheckCertificateRevocation = checkCertificateRevocation;
        }

        /// <summary>Specifies allowable SSL protocols.</summary>
        public SslProtocols EnabledProtocols { get; }

        /// <summary>Specifies whether the certificate revocation list is checked during authentication.</summary>
        public bool CheckCertificateRevocation { get; }

#if NETCOREAPP_2_0_GREATER || NETSTANDARD_2_0_GREATER
        private static readonly TimeSpan DefaultHandshakeTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan MaximumHandshakeTimeout = TimeSpan.FromMilliseconds(int.MaxValue);

        private TimeSpan _handshakeTimeout = DefaultHandshakeTimeout;

        /// <summary>
        /// Specifies the maximum amount of time allowed for the TLS/SSL handshake. This must be positive and finite. Defaults to 10 seconds.
        /// </summary>
        public TimeSpan HandshakeTimeout
        {
            get => _handshakeTimeout;
            set
            {
                if (value <= TimeSpan.Zero && value != Timeout.InfiniteTimeSpan || value > MaximumHandshakeTimeout)
                {
                    ThrowArgumentOutOfRangeException();
                }
                _handshakeTimeout = value != Timeout.InfiniteTimeSpan ? value : MaximumHandshakeTimeout;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentOutOfRangeException()
        {
            throw GetArgumentOutOfRangeException();

            static ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException("value", "Value must be a positive TimeSpan.");
            }
        }
#endif
    }
}