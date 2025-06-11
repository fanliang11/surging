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
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using DotNetty.Transport.Channels;

    public sealed class ClientTlsSettings : TlsSettings
    {
        private static readonly Func<X509Certificate, X509Chain, SslPolicyErrors, bool> s_serverCertificateValidation = (_, __, ___) => true;

        public ClientTlsSettings(string targetHost)
          : this(targetHost, new List<X509Certificate>())
        {
        }

        public ClientTlsSettings(string targetHost, List<X509Certificate> certificates)
          : this(false, certificates, targetHost)
        {
        }

        public ClientTlsSettings(bool checkCertificateRevocation, List<X509Certificate> certificates, string targetHost)
          : this(
#if NETCOREAPP_3_0_GREATER
            SslProtocols.Tls13 |
#endif
            SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls
            , checkCertificateRevocation, certificates, targetHost)
        {
        }

        public ClientTlsSettings(SslProtocols enabledProtocols, bool checkCertificateRevocation, List<X509Certificate> certificates, string targetHost)
          : base(enabledProtocols, checkCertificateRevocation)
        {
            X509CertificateCollection = new X509CertificateCollection(certificates.ToArray());
            TargetHost = targetHost;
            Certificates = certificates.AsReadOnly();
        }

        internal X509CertificateCollection X509CertificateCollection { get; set; }

        public IReadOnlyCollection<X509Certificate> Certificates { get; }

        public string TargetHost { get; }

        /// <summary>Whether allow untrusted certificate</summary>
        public bool AllowUnstrustedCertificate { get; set; }

        /// <summary>Whether allow the certificate whose name doesn't match current remote endpoint's host name</summary>
        public bool AllowNameMismatchCertificate { get; set; }

        /// <summary>Whether allow the certificate chain errors</summary>
        public bool AllowCertificateChainErrors { get; set; }

        public Func<X509Certificate, X509Chain, SslPolicyErrors, bool> ServerCertificateValidation { get; set; }

        /// <summary>Overrides the current <see cref="ServerCertificateValidation"/> callback and allows any server certificate.</summary>
        public ClientTlsSettings AllowAnyServerCertificate()
        {
            ServerCertificateValidation = s_serverCertificateValidation;
            return this;
        }

#if NETCOREAPP_2_0_GREATER || NETSTANDARD_2_0_GREATER
        public System.Collections.Generic.List<SslApplicationProtocol> ApplicationProtocols { get; set; }

        public Func<IChannelHandlerContext, string, X509CertificateCollection, X509Certificate, string[], X509Certificate2> UserCertSelector { get; set; }

        public Action<IChannelHandlerContext, ClientTlsSettings, SslClientAuthenticationOptions> OnAuthenticate { get; set; }
#else
        public Func<SslStream, string, X509CertificateCollection, X509Certificate, string[], X509Certificate2> UserCertSelector { get; set; }
#endif
    }
}