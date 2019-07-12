/*
 * ServerSslConfiguration.cs
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
    /// Stores the parameters for the <see cref="SslStream"/> used by servers.
    /// </summary>
    public class ServerSslConfiguration
    {
        #region 字段

        /// <summary>
        /// Defines the _checkCertRevocation
        /// </summary>
        private bool _checkCertRevocation;

        /// <summary>
        /// Defines the _clientCertRequired
        /// </summary>
        private bool _clientCertRequired;

        /// <summary>
        /// Defines the _clientCertValidationCallback
        /// </summary>
        private RemoteCertificateValidationCallback _clientCertValidationCallback;

        /// <summary>
        /// Defines the _enabledSslProtocols
        /// </summary>
        private SslProtocols _enabledSslProtocols;

        /// <summary>
        /// Defines the _serverCert
        /// </summary>
        private X509Certificate2 _serverCert;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSslConfiguration"/> class.
        /// </summary>
        public ServerSslConfiguration()
        {
            _enabledSslProtocols = SslProtocols.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSslConfiguration"/> class.
        /// </summary>
        /// <param name="configuration">The configuration<see cref="ServerSslConfiguration"/></param>
        public ServerSslConfiguration(ServerSslConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _checkCertRevocation = configuration._checkCertRevocation;
            _clientCertRequired = configuration._clientCertRequired;
            _clientCertValidationCallback = configuration._clientCertValidationCallback;
            _enabledSslProtocols = configuration._enabledSslProtocols;
            _serverCert = configuration._serverCert;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSslConfiguration"/> class.
        /// </summary>
        /// <param name="serverCertificate">The serverCertificate<see cref="X509Certificate2"/></param>
        public ServerSslConfiguration(X509Certificate2 serverCertificate)
        {
            _serverCert = serverCertificate;
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
        /// Gets or sets a value indicating whether the client is asked for
        /// a certificate for authentication.
        /// </summary>
        public bool ClientCertificateRequired
        {
            get
            {
                return _clientCertRequired;
            }

            set
            {
                _clientCertRequired = value;
            }
        }

        /// <summary>
        /// Gets or sets the callback used to validate the certificate
        /// supplied by the client.
        /// </summary>
        public RemoteCertificateValidationCallback ClientCertificateValidationCallback
        {
            get
            {
                if (_clientCertValidationCallback == null)
                    _clientCertValidationCallback = defaultValidateClientCertificate;

                return _clientCertValidationCallback;
            }

            set
            {
                _clientCertValidationCallback = value;
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
        /// Gets or sets the certificate used to authenticate the server.
        /// </summary>
        public X509Certificate2 ServerCertificate
        {
            get
            {
                return _serverCert;
            }

            set
            {
                _serverCert = value;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The defaultValidateClientCertificate
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="certificate">The certificate<see cref="X509Certificate"/></param>
        /// <param name="chain">The chain<see cref="X509Chain"/></param>
        /// <param name="sslPolicyErrors">The sslPolicyErrors<see cref="SslPolicyErrors"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool defaultValidateClientCertificate(
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