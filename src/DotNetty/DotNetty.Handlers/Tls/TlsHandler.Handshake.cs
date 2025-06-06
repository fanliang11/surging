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
    using System.Diagnostics;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
#if NETCOREAPP_2_0_GREATER || NETSTANDARD_2_0_GREATER
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
#endif

    partial class TlsHandler
    {
        private static readonly Action<object, object> s_handshakeCompletionCallback = (t, s) => HandleHandshakeCompleted((Task)t, (TlsHandler)s);
        public static readonly AttributeKey<SslStream> SslStreamAttrKey = AttributeKey<SslStream>.ValueOf("SSLSTREAM");

        private bool EnsureAuthenticated(IChannelHandlerContext ctx)
        {
            var oldState = State;
            if (oldState.HasAny(TlsHandlerState.AuthenticationStarted))
            {
                return oldState.Has(TlsHandlerState.Authenticated);
            }

            State = oldState | TlsHandlerState.Authenticating;
            BeginHandshake(ctx);
            return false;
        }

        private bool EnsureAuthenticationCompleted(IChannelHandlerContext ctx)
        {
            var oldState = State;
            if (oldState.HasAny(TlsHandlerState.AuthenticationStarted))
            {
                return oldState.HasAny(TlsHandlerState.AuthenticationCompleted);
            }

            State = oldState | TlsHandlerState.Authenticating;
            BeginHandshake(ctx);
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void BeginHandshake(IChannelHandlerContext ctx)
        {
            if (_isServer)
            {
#if NETCOREAPP_2_0_GREATER || NETSTANDARD_2_0_GREATER
                // Adapt to the SslStream signature
                ServerCertificateSelectionCallback selector = null;
                if (_serverCertificateSelector is object)
                {
                    X509Certificate LocalServerCertificateSelection(object sender, string name)
                    {
                        ctx.GetAttribute(SslStreamAttrKey).Set(_sslStream);
                        return _serverCertificateSelector(ctx, name);
                    }
                    selector = new ServerCertificateSelectionCallback(LocalServerCertificateSelection);
                }

                var sslOptions = new SslServerAuthenticationOptions()
                {
                    ServerCertificate = _serverCertificate,
                    ServerCertificateSelectionCallback = selector,
                    ClientCertificateRequired = _serverSettings.NegotiateClientCertificate,
                    EnabledSslProtocols = _serverSettings.EnabledProtocols,
                    CertificateRevocationCheckMode = _serverSettings.CheckCertificateRevocation ? X509RevocationMode.Online : X509RevocationMode.NoCheck,
                    ApplicationProtocols = _serverSettings.ApplicationProtocols // ?? new List<SslApplicationProtocol>()
                };
                _serverSettings.OnAuthenticate?.Invoke(ctx, _serverSettings, sslOptions);

                var cts = new CancellationTokenSource(_serverSettings.HandshakeTimeout);
                _sslStream.AuthenticateAsServerAsync(sslOptions, cts.Token)
                          .ContinueWith(
#if NET
                                static
#endif
                                (t, s) => HandshakeCompletionCallback(t, s), (this, cts), TaskContinuationOptions.ExecuteSynchronously);
#else
                _sslStream.AuthenticateAsServerAsync(_serverCertificate,
                                                     _serverSettings.NegotiateClientCertificate,
                                                     _serverSettings.EnabledProtocols,
                                                     _serverSettings.CheckCertificateRevocation)
                          .ContinueWith((t, s) => HandshakeCompletionCallback(t, s), this, TaskContinuationOptions.ExecuteSynchronously);
#endif
            }
            else
            {
#if NETCOREAPP_2_0_GREATER || NETSTANDARD_2_0_GREATER
                LocalCertificateSelectionCallback selector = null;
                if (_userCertSelector is object)
                {
                    X509Certificate LocalCertificateSelection(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
                    {
                        ctx.GetAttribute(SslStreamAttrKey).Set(_sslStream);
                        return _userCertSelector(ctx, targetHost, localCertificates, remoteCertificate, acceptableIssuers);
                    }
                    selector = new LocalCertificateSelectionCallback(LocalCertificateSelection);
                }
                var sslOptions = new SslClientAuthenticationOptions()
                {
                    TargetHost = _clientSettings.TargetHost,
                    ClientCertificates = _clientSettings.X509CertificateCollection,
                    EnabledSslProtocols = _clientSettings.EnabledProtocols,
                    CertificateRevocationCheckMode = _clientSettings.CheckCertificateRevocation ? X509RevocationMode.Online : X509RevocationMode.NoCheck,
                    LocalCertificateSelectionCallback = selector,
                    ApplicationProtocols = _clientSettings.ApplicationProtocols
                };
                _clientSettings.OnAuthenticate?.Invoke(ctx, _clientSettings, sslOptions);

                var cts = new CancellationTokenSource(_clientSettings.HandshakeTimeout);
                _sslStream.AuthenticateAsClientAsync(sslOptions, cts.Token)
                          .ContinueWith(
#if NET
                                static
#endif
                                (t, s) => HandshakeCompletionCallback(t, s), (this, cts), TaskContinuationOptions.ExecuteSynchronously);
#else
                _sslStream.AuthenticateAsClientAsync(_clientSettings.TargetHost,
                                                     _clientSettings.X509CertificateCollection,
                                                     _clientSettings.EnabledProtocols,
                                                     _clientSettings.CheckCertificateRevocation)
                          .ContinueWith((t, s) => HandshakeCompletionCallback(t, s), this, TaskContinuationOptions.ExecuteSynchronously);
#endif
            }
        }

        private static void HandshakeCompletionCallback(Task task, object s)
        {
#if NETCOREAPP_2_0_GREATER || NETSTANDARD_2_0_GREATER
            var (self, cts) = ((TlsHandler self, CancellationTokenSource cts))s;
            cts.Dispose();
#else
            var self = (TlsHandler)s;
#endif
            var capturedContext = self.CapturedContext;
            if (capturedContext.Executor.InEventLoop)
            {
                HandleHandshakeCompleted(task, self);
            }
            else
            {
                capturedContext.Executor.Execute(s_handshakeCompletionCallback, task, self);
            }
        }

        private static void HandleHandshakeCompleted(Task task, TlsHandler self)
        {
            var capturedContext = self.CapturedContext;
            var oldState = self.State;

            if (task.IsSuccess())
            {
                Debug.Assert(!oldState.HasAny(TlsHandlerState.AuthenticationCompleted));
                self.State = (oldState | TlsHandlerState.Authenticated) & ~(TlsHandlerState.Authenticating | TlsHandlerState.FlushedBeforeHandshake);
                self._handshakePromise.TryComplete();

                _ = capturedContext.FireUserEventTriggered(TlsHandshakeCompletionEvent.Success);

                if (oldState.Has(TlsHandlerState.ReadRequestedBeforeAuthenticated) && !capturedContext.Channel.Configuration.IsAutoRead)
                {
                    _ = capturedContext.Read();
                }

                if (oldState.Has(TlsHandlerState.FlushedBeforeHandshake))
                {
                    try
                    {
                        self.Wrap(capturedContext);
                        _ = capturedContext.Flush();
                    }
                    catch (Exception cause)
                    {
                        // Fail pending writes.
                        self.HandleFailure(capturedContext, cause, true, false, true);
                    }
                }
            }
            else if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Assert(!oldState.HasAny(TlsHandlerState.Authenticated));
                self.State = (oldState | TlsHandlerState.FailedAuthentication) & ~TlsHandlerState.Authenticating;
                var taskExc = task.Exception;
                var cause = taskExc.Unwrap();
                try
                {
                    if (self._handshakePromise.TrySetException(taskExc))
                    {
                        TlsUtils.NotifyHandshakeFailure(capturedContext, cause, true);
                    }
                }
                finally
                {
                    self._pendingUnencryptedWrites?.ReleaseAndFailAll(cause);
                }
            }
        }
    }
}
