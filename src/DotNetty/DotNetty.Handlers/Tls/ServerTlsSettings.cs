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
    using System;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using DotNetty.Transport.Channels;

    public sealed class ServerTlsSettings : TlsSettings
    {
        private static readonly Func<X509Certificate2, X509Chain, SslPolicyErrors, bool> s_clientCertificateValidation;
        private static readonly SslProtocols s_defaultServerProtocol;

        static ServerTlsSettings()
        {
#if NET
            s_defaultServerProtocol = SslProtocols.Tls12;
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                s_defaultServerProtocol = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
            }
            else
            {
                s_defaultServerProtocol = SslProtocols.Tls12 | SslProtocols.Tls11;
            }
#endif
#if NETCOREAPP_3_0_GREATER
            s_defaultServerProtocol |= SslProtocols.Tls13;
#endif
            s_clientCertificateValidation = (_, __, ___) => true;
        }

        public ServerTlsSettings(X509Certificate certificate)
            : this(certificate, ClientCertificateMode.NoCertificate)
        {
        }

        public ServerTlsSettings(X509Certificate certificate, bool negotiateClientCertificate)
            : this(certificate, negotiateClientCertificate, false)
        {
        }

        public ServerTlsSettings(X509Certificate certificate, bool negotiateClientCertificate, bool checkCertificateRevocation)
            : this(certificate, negotiateClientCertificate, checkCertificateRevocation, s_defaultServerProtocol)
        {
        }

        public ServerTlsSettings(X509Certificate certificate, bool negotiateClientCertificate, bool checkCertificateRevocation, SslProtocols enabledProtocols)
          : base(enabledProtocols, checkCertificateRevocation)
        {
            Certificate = certificate;
            NegotiateClientCertificate = negotiateClientCertificate;
            ClientCertificateMode = negotiateClientCertificate ? ClientCertificateMode.AllowCertificate : ClientCertificateMode.NoCertificate;
        }

        public ServerTlsSettings(X509Certificate certificate, ClientCertificateMode clientCertificateMode)
            : this(certificate, clientCertificateMode, false)
        {
        }

        public ServerTlsSettings(X509Certificate certificate, ClientCertificateMode clientCertificateMode, bool checkCertificateRevocation)
            : this(certificate, clientCertificateMode, checkCertificateRevocation, s_defaultServerProtocol)
        {
        }

        public ServerTlsSettings(X509Certificate certificate, ClientCertificateMode clientCertificateMode, bool checkCertificateRevocation, SslProtocols enabledProtocols)
            : base(enabledProtocols, checkCertificateRevocation)
        {
            Certificate = certificate;
            NegotiateClientCertificate = clientCertificateMode != ClientCertificateMode.NoCertificate;
            ClientCertificateMode = clientCertificateMode;
        }

        /// <summary>Specifies the server certificate used to authenticate Tls/Ssl connections.
        /// This is ignored if ServerCertificateSelector is set.</summary>
        public X509Certificate Certificate { get; }

        internal readonly bool NegotiateClientCertificate;

        /// <summary>Specifies the client certificate requirements for a HTTPS connection.
        /// Defaults to <see cref="ClientCertificateMode.NoCertificate"/>.</summary>
        public ClientCertificateMode ClientCertificateMode { get; set; } = ClientCertificateMode.NoCertificate;

        /// <summary>Specifies a callback for additional client certificate validation that will be invoked during authentication.</summary>
        public Func<X509Certificate2, X509Chain, SslPolicyErrors, bool> ClientCertificateValidation { get; set; }

        /// <summary>Overrides the current <see cref="ClientCertificateValidation"/> callback and allows any client certificate.</summary>
        public ServerTlsSettings AllowAnyClientCertificate()
        {
            ClientCertificateValidation = s_clientCertificateValidation;
            return this;
        }

#if NETCOREAPP_2_0_GREATER || NETSTANDARD_2_0_GREATER
        public System.Collections.Generic.List<SslApplicationProtocol> ApplicationProtocols { get; set; }

        /// <summary>A callback that will be invoked to dynamically select a server certificate. This is higher priority than ServerCertificate.
        /// If SNI is not avialable then the name parameter will be null.</summary>
        public Func<IChannelHandlerContext, string, X509Certificate2> ServerCertificateSelector { get; set; }

        public Action<IChannelHandlerContext, ServerTlsSettings, SslServerAuthenticationOptions> OnAuthenticate { get; set; }
#endif
    }
}