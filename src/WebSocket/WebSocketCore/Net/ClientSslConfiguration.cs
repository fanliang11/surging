/*
 * ClientSslConfiguration.cs
 *
 * The MIT License
 *
 * Copyright (c) 2014 liryna
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

/*
 * Authors:
 * - Liryna <liryna.stark@gmail.com>
 */

using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace WebSocketCore.Net
{
    /// <summary>
    /// Stores the parameters for the <see cref="SslStream"/> used by clients.
    /// </summary>
    public class ClientSslConfiguration
    {
        #region 字段

        /// <summary>
        /// Defines the _checkCertRevocation
        /// </summary>
        private bool _checkCertRevocation;

        /// <summary>
        /// Defines the _clientCerts
        /// </summary>
        private X509CertificateCollection _clientCerts;

        /// <summary>
        /// Defines the _clientCertSelectionCallback
        /// </summary>
        private LocalCertificateSelectionCallback _clientCertSelectionCallback;

        /// <summary>
        /// Defines the _enabledSslProtocols
        /// </summary>
        private SslProtocols _enabledSslProtocols;

        /// <summary>
        /// Defines the _serverCertValidationCallback
        /// </summary>
        private RemoteCertificateValidationCallback _serverCertValidationCallback;

        /// <summary>
        /// Defines the _targetHost
        /// </summary>
        private string _targetHost;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSslConfiguration"/> class.
        /// </summary>
        public ClientSslConfiguration()
        {
            _enabledSslProtocols = SslProtocols.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSslConfiguration"/> class.
        /// </summary>
        /// <param name="configuration">The configuration<see cref="ClientSslConfiguration"/></param>
        public ClientSslConfiguration(ClientSslConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _checkCertRevocation = configuration._checkCertRevocation;
            _clientCertSelectionCallback = configuration._clientCertSelectionCallback;
            _clientCerts = configuration._clientCerts;
            _enabledSslProtocols = configuration._enabledSslProtocols;
            _serverCertValidationCallback = configuration._serverCertValidationCallback;
            _targetHost = configuration._targetHost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSslConfiguration"/> class.
        /// </summary>
        /// <param name="targetHost">The targetHost<see cref="string"/></param>
        public ClientSslConfiguration(string targetHost)
        {
            _targetHost = targetHost;
            _enabledSslProtocols = SslProtocols.Default;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets a value indicating whether the certificate revocation
        /// list is checked during authentication.
        /// </summary>
        public bool CheckCertificateRevocation
        {
            get
            {
                return _checkCertRevocation;
            }

            set
            {
                _checkCertRevocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the certificates from which to select one to
        /// supply to the server.
        /// </summary>
        public X509CertificateCollection ClientCertificates
        {
            get
            {
                return _clientCerts;
            }

            set
            {
                _clientCerts = value;
            }
        }

        /// <summary>
        /// Gets or sets the callback used to select the certificate to
        /// supply to the server.
        /// </summary>
        public LocalCertificateSelectionCallback ClientCertificateSelectionCallback
        {
            get
            {
                if (_clientCertSelectionCallback == null)
                    _clientCertSelectionCallback = defaultSelectClientCertificate;

                return _clientCertSelectionCallback;
            }

            set
            {
                _clientCertSelectionCallback = value;
            }
        }

        /// <summary>
        /// Gets or sets the protocols used for authentication.
        /// </summary>
        public SslProtocols EnabledSslProtocols
        {
            get
            {
                return _enabledSslProtocols;
            }

            set
            {
                _enabledSslProtocols = value;
            }
        }

        /// <summary>
        /// Gets or sets the callback used to validate the certificate
        /// supplied by the server.
        /// </summary>
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get
            {
                if (_serverCertValidationCallback == null)
                    _serverCertValidationCallback = defaultValidateServerCertificate;

                return _serverCertValidationCallback;
            }

            set
            {
                _serverCertValidationCallback = value;
            }
        }

        /// <summary>
        /// Gets or sets the target host server name.
        /// </summary>
        public string TargetHost
        {
            get
            {
                return _targetHost;
            }

            set
            {
                _targetHost = value;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The defaultSelectClientCertificate
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="targetHost">The targetHost<see cref="string"/></param>
        /// <param name="clientCertificates">The clientCertificates<see cref="X509CertificateCollection"/></param>
        /// <param name="serverCertificate">The serverCertificate<see cref="X509Certificate"/></param>
        /// <param name="acceptableIssuers">The acceptableIssuers<see cref="string[]"/></param>
        /// <returns>The <see cref="X509Certificate"/></returns>
        private static X509Certificate defaultSelectClientCertificate(
      object sender,
      string targetHost,
      X509CertificateCollection clientCertificates,
      X509Certificate serverCertificate,
      string[] acceptableIssuers
    )
        {
            return null;
        }

        /// <summary>
        /// The defaultValidateServerCertificate
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="certificate">The certificate<see cref="X509Certificate"/></param>
        /// <param name="chain">The chain<see cref="X509Chain"/></param>
        /// <param name="sslPolicyErrors">The sslPolicyErrors<see cref="SslPolicyErrors"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool defaultValidateServerCertificate(
      object sender,
      X509Certificate certificate,
      X509Chain chain,
      SslPolicyErrors sslPolicyErrors
    )
        {
            return true;
        }

        #endregion 方法
    }
}