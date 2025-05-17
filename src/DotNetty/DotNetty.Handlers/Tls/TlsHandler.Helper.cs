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
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Security.Cryptography.X509Certificates;
    using DotNetty.Buffers;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Transport.Channels;

    partial class TlsHandler
    {
        private static readonly IInternalLogger s_logger = InternalLoggerFactory.GetInstance<TlsHandler>();
        private static readonly Exception s_sslStreamClosedException = new IOException("SSLStream closed already");

        public static TlsHandler Client(string targetHost, bool allowAnyServerCertificate = false)
        {
            var tlsSettings = new ClientTlsSettings(targetHost);
            if (allowAnyServerCertificate) { _ = tlsSettings.AllowAnyServerCertificate(); }
            return new(tlsSettings);
        }

        public static TlsHandler Client(string targetHost, X509Certificate clientCertificate)
            => new(new ClientTlsSettings(targetHost, new List<X509Certificate> { clientCertificate }));

        public static TlsHandler Server(X509Certificate certificate, bool allowAnyClientCertificate = false)
        {
            var tlsSettings = new ServerTlsSettings(certificate);
            if (allowAnyClientCertificate) { _ = tlsSettings.AllowAnyClientCertificate(); }
            return new(tlsSettings);
        }

        private static SslStream CreateSslStream(TlsSettings settings, Stream stream)
        {
            if (settings is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.settings); }

            if (settings is ServerTlsSettings serverSettings)
            {
                // Enable client certificate function only if ClientCertificateRequired is true in the configuration
                if (serverSettings.ClientCertificateMode == ClientCertificateMode.NoCertificate)
                {
                    return new SslStream(stream, leaveInnerStreamOpen: true);
                }

#if NETFRAMEWORK
                // SSL 版本 2 协议不支持客户端证书
                if (serverSettings.EnabledProtocols == System.Security.Authentication.SslProtocols.Ssl2)
                {
                    return new SslStream(stream, leaveInnerStreamOpen: true);
                }
#endif

                return new SslStream(stream,
                    leaveInnerStreamOpen: true,
                    userCertificateValidationCallback: (sender, certificate, chain, sslPolicyErrors) => ClientCertificateValidation(certificate, chain, sslPolicyErrors, serverSettings));
            }
            else if (settings is ClientTlsSettings clientSettings)
            {
                return new SslStream(stream,
                    leaveInnerStreamOpen: true,
                    userCertificateValidationCallback: (sender, certificate, chain, sslPolicyErrors) => ServerCertificateValidation(sender, certificate, chain, sslPolicyErrors, clientSettings)
#if !(NETCOREAPP_2_0_GREATER || NETSTANDARD_2_0_GREATER)
                    , userCertificateSelectionCallback: clientSettings.UserCertSelector is null ? null : new LocalCertificateSelectionCallback((sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) =>
                    {
                        return clientSettings.UserCertSelector(sender as SslStream, targetHost, localCertificates, remoteCertificate, acceptableIssuers);
                    })
#endif
                    );
            }
            else
            {
                return new SslStream(stream, leaveInnerStreamOpen: true);
            }
        }

        #region ** ClientCertificateValidation **

        private static bool ClientCertificateValidation(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors, ServerTlsSettings serverSettings)
        {
            if (certificate is null)
            {
                return serverSettings.ClientCertificateMode != ClientCertificateMode.RequireCertificate;
            }

            var clientCertificateValidationFunc = serverSettings.ClientCertificateValidation;
            if (clientCertificateValidationFunc is null)
            {
                if (sslPolicyErrors != SslPolicyErrors.None) { return false; }
            }

            var certificate2 = ConvertToX509Certificate2(certificate);
            if (certificate2 is null) { return false; }

            if (clientCertificateValidationFunc is object)
            {
                if (!clientCertificateValidationFunc(certificate2, chain, sslPolicyErrors))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region ** ServerCertificateValidation **

        /// <summary>Validates the remote certificate.</summary>
        /// <remarks>Code take from SuperSocket.ClientEngine(See https://github.com/kerryjiang/SuperSocket.ClientEngine/blob/b46a0ededbd6249f4e28b8d77f55dea3fa23283e/Core/SslStreamTcpSession.cs#L101). </remarks>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <param name="clientSettings"></param>
        /// <returns></returns>
        private static bool ServerCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors, ClientTlsSettings clientSettings)
        {
            var certificateValidation = clientSettings.ServerCertificateValidation;
            if (certificateValidation is object) { return certificateValidation(certificate, chain, sslPolicyErrors); }

            var callback = ServicePointManager.ServerCertificateValidationCallback;
            if (callback is object) { return callback(sender, certificate, chain, sslPolicyErrors); }

            if (sslPolicyErrors == SslPolicyErrors.None) { return true; }

            if (clientSettings.AllowNameMismatchCertificate)
            {
                sslPolicyErrors &= (~SslPolicyErrors.RemoteCertificateNameMismatch);
            }

            if (clientSettings.AllowCertificateChainErrors)
            {
                sslPolicyErrors &= (~SslPolicyErrors.RemoteCertificateChainErrors);
            }

            if (sslPolicyErrors == SslPolicyErrors.None) { return true; }

            if (!clientSettings.AllowUnstrustedCertificate)
            {
                s_logger.Warn(sslPolicyErrors.ToString());
                return false;
            }

            // not only a remote certificate error
            if (sslPolicyErrors != SslPolicyErrors.None && sslPolicyErrors != SslPolicyErrors.RemoteCertificateChainErrors)
            {
                s_logger.Warn(sslPolicyErrors.ToString());
                return false;
            }

            if (chain is object && chain.ChainStatus is object)
            {
                foreach (X509ChainStatus status in chain.ChainStatus)
                {
                    if ((certificate.Subject == certificate.Issuer) &&
                        (status.Status == X509ChainStatusFlags.UntrustedRoot))
                    {
                        // Self-signed certificates with an untrusted root are valid. 
                        continue;
                    }
                    else
                    {
                        if (status.Status != X509ChainStatusFlags.NoError)
                        {
                            s_logger.Warn(sslPolicyErrors.ToString());
                            // If there are any other errors in the certificate chain, the certificate is invalid,
                            // so the method returns false.
                            return false;
                        }
                    }
                }
            }

            // When processing reaches this line, the only errors in the certificate chain are 
            // untrusted root errors for self-signed certificates. These certificates are valid
            // for default Exchange server installations, so return true.
            return true;
        }

        #endregion

        #region ** ConvertToX509Certificate2 **

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static X509Certificate2 ConvertToX509Certificate2(X509Certificate certificate) => certificate switch
        {
            null => null,
            X509Certificate2 cert2 => cert2,
            _ => new X509Certificate2(certificate),
        };

        #endregion

        #region ** enum Framing **

        private enum Framing
        {
            Unknown = 0,    // Initial before any frame is processd.
            BeforeSSL3,     // SSlv2
            SinceSSL3,      // SSlv3 & TLS
            Unified,        // Intermediate on first frame until response is processes.
            Invalid         // Somthing is wrong.
        }

        #endregion

        #region ** enum ContentType **

        // SSL3/TLS protocol frames definitions.
        private enum ContentType : byte
        {
            ChangeCipherSpec = 20,
            Alert = 21,
            Handshake = 22,
            AppData = 23
        }

        #endregion

        #region ** DetectFraming **

        [MethodImpl(MethodImplOptions.NoInlining)]
        private Framing DetectFraming(IByteBuffer input)
        {
            if (input.IsSingleIoBuffer)
            {
                return DetectFraming(input.UnreadSpan);
            }
            else
            {
                return DetectFraming(input, input.ReaderIndex);
            }
        }

        // code take from https://github.com/dotnet/runtime/blob/83a4d3cc02fb04fce17b24fc09b3cdf77a12ba51/src/libraries/System.Net.Security/src/System/Net/Security/SslStream.Implementation.cs#L1245
        // We need at least 5 bytes to determine what we have.
        private Framing DetectFraming(in ReadOnlySpan<byte> bytes)
        {
            /* PCTv1.0 Hello starts with
             * RECORD_LENGTH_MSB  (ignore)
             * RECORD_LENGTH_LSB  (ignore)
             * PCT1_CLIENT_HELLO  (must be equal)
             * PCT1_CLIENT_VERSION_MSB (if version greater than PCTv1)
             * PCT1_CLIENT_VERSION_LSB (if version greater than PCTv1)
             *
             * ... PCT hello ...
             */

            /* Microsoft Unihello starts with
             * RECORD_LENGTH_MSB  (ignore)
             * RECORD_LENGTH_LSB  (ignore)
             * SSL2_CLIENT_HELLO  (must be equal)
             * SSL2_CLIENT_VERSION_MSB (if version greater than SSLv2) ( or v3)
             * SSL2_CLIENT_VERSION_LSB (if version greater than SSLv2) ( or v3)
             *
             * ... SSLv2 Compatible Hello ...
             */

            /* SSLv2 CLIENT_HELLO starts with
             * RECORD_LENGTH_MSB  (ignore)
             * RECORD_LENGTH_LSB  (ignore)
             * SSL2_CLIENT_HELLO  (must be equal)
             * SSL2_CLIENT_VERSION_MSB (if version greater than SSLv2) ( or v3)
             * SSL2_CLIENT_VERSION_LSB (if version greater than SSLv2) ( or v3)
             *
             * ... SSLv2 CLIENT_HELLO ...
             */

            /* SSLv2 SERVER_HELLO starts with
             * RECORD_LENGTH_MSB  (ignore)
             * RECORD_LENGTH_LSB  (ignore)
             * SSL2_SERVER_HELLO  (must be equal)
             * SSL2_SESSION_ID_HIT (ignore)
             * SSL2_CERTIFICATE_TYPE (ignore)
             * SSL2_CLIENT_VERSION_MSB (if version greater than SSLv2) ( or v3)
             * SSL2_CLIENT_VERSION_LSB (if version greater than SSLv2) ( or v3)
             *
             * ... SSLv2 SERVER_HELLO ...
             */

            /* SSLv3 Type 2 Hello starts with
              * RECORD_LENGTH_MSB  (ignore)
              * RECORD_LENGTH_LSB  (ignore)
              * SSL2_CLIENT_HELLO  (must be equal)
              * SSL2_CLIENT_VERSION_MSB (if version greater than SSLv3)
              * SSL2_CLIENT_VERSION_LSB (if version greater than SSLv3)
              *
              * ... SSLv2 Compatible Hello ...
              */

            /* SSLv3 Type 3 Hello starts with
             * 22 (HANDSHAKE MESSAGE)
             * VERSION MSB
             * VERSION LSB
             * RECORD_LENGTH_MSB  (ignore)
             * RECORD_LENGTH_LSB  (ignore)
             * HS TYPE (CLIENT_HELLO)
             * 3 bytes HS record length
             * HS Version
             * HS Version
             */

            /* SSLv2 message codes
             * SSL_MT_ERROR                0
             * SSL_MT_CLIENT_HELLO         1
             * SSL_MT_CLIENT_MASTER_KEY    2
             * SSL_MT_CLIENT_FINISHED      3
             * SSL_MT_SERVER_HELLO         4
             * SSL_MT_SERVER_VERIFY        5
             * SSL_MT_SERVER_FINISHED      6
             * SSL_MT_REQUEST_CERTIFICATE  7
             * SSL_MT_CLIENT_CERTIFICATE   8
             */

            int version = -1;

            // If the first byte is SSL3 HandShake, then check if we have a SSLv3 Type3 client hello.
            if (bytes[0] == (byte)ContentType.Handshake || bytes[0] == (byte)ContentType.AppData
                || bytes[0] == (byte)ContentType.Alert)
            {
                if (bytes.Length < 3)
                {
                    return Framing.Invalid;
                }

                version = (bytes[1] << 8) | bytes[2];
                if (version < 0x300 || version >= 0x500)
                {
                    return Framing.Invalid;
                }

                //
                // This is an SSL3 Framing
                //
                return Framing.SinceSSL3;
            }

            if (bytes.Length < 3)
            {
                return Framing.Invalid;
            }

            if (bytes[2] > 8)
            {
                return Framing.Invalid;
            }

            if (bytes[2] == 0x1)  // SSL_MT_CLIENT_HELLO
            {
                if (bytes.Length >= 5)
                {
                    version = (bytes[3] << 8) | bytes[4];
                }
            }
            else if (bytes[2] == 0x4) // SSL_MT_SERVER_HELLO
            {
                if (bytes.Length >= 7)
                {
                    version = (bytes[5] << 8) | bytes[6];
                }
            }

            if (version != -1)
            {
                // If this is the first packet, the client may start with an SSL2 packet
                // but stating that the version is 3.x, so check the full range.
                // For the subsequent packets we assume that an SSL2 packet should have a 2.x version.
                if (_framing == Framing.Unknown)
                {
                    if (version != 0x0002 && (version < 0x200 || version >= 0x500))
                    {
                        return Framing.Invalid;
                    }
                }
                else
                {
                    if (version != 0x0002)
                    {
                        return Framing.Invalid;
                    }
                }
            }

            // When server has replied the framing is already fixed depending on the prior client packet
            if (!_isServer || _framing == Framing.Unified)
            {
                return Framing.BeforeSSL3;
            }

            return Framing.Unified; // Will use Ssl2 just for this frame.
        }

        private Framing DetectFraming(IByteBuffer input, int offset)
        {
            int version = -1;

            var first = input.GetByte(offset);
            var second = input.GetByte(offset + 1);
            var third = input.GetByte(offset + 2);

            // If the first byte is SSL3 HandShake, then check if we have a SSLv3 Type3 client hello.
            if (first == (byte)ContentType.Handshake || first == (byte)ContentType.AppData
                || first == (byte)ContentType.Alert)
            {
                if (input.ReadableBytes < 3)
                {
                    return Framing.Invalid;
                }

                version = (second << 8) | third;
                if (version < 0x300 || version >= 0x500)
                {
                    return Framing.Invalid;
                }

                //
                // This is an SSL3 Framing
                //
                return Framing.SinceSSL3;
            }

            if (input.ReadableBytes < 3)
            {
                return Framing.Invalid;
            }

            if (third > 8)
            {
                return Framing.Invalid;
            }

            if (third == 0x1)  // SSL_MT_CLIENT_HELLO
            {
                if (input.ReadableBytes >= 5)
                {
                    version = (input.GetByte(offset + 3) << 8) | input.GetByte(offset + 4);
                }
            }
            else if (third == 0x4) // SSL_MT_SERVER_HELLO
            {
                if (input.ReadableBytes >= 7)
                {
                    version = (input.GetByte(offset + 5) << 8) | input.GetByte(offset + 6);
                }
            }

            if (version != -1)
            {
                // If this is the first packet, the client may start with an SSL2 packet
                // but stating that the version is 3.x, so check the full range.
                // For the subsequent packets we assume that an SSL2 packet should have a 2.x version.
                if (_framing == Framing.Unknown)
                {
                    if (version != 0x0002 && (version < 0x200 || version >= 0x500))
                    {
                        return Framing.Invalid;
                    }
                }
                else
                {
                    if (version != 0x0002)
                    {
                        return Framing.Invalid;
                    }
                }
            }

            // When server has replied the framing is already fixed depending on the prior client packet
            if (!_isServer || _framing == Framing.Unified)
            {
                return Framing.BeforeSSL3;
            }

            return Framing.Unified; // Will use Ssl2 just for this frame.
        }

        #endregion

        #region ** GetFrameSize **

        // Returns TLS Frame size.
        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private static int GetFrameSize(Framing framing, IByteBuffer buffer)
        {
            if (buffer.IsSingleIoBuffer)
            {
                return GetFrameSize(framing, buffer.UnreadSpan);
            }
            else
            {
                return GetFrameSize(framing, buffer, buffer.ReaderIndex);
            }
        }

        // code take from https://github.com/dotnet/runtime/blob/83a4d3cc02fb04fce17b24fc09b3cdf77a12ba51/src/libraries/System.Net.Security/src/System/Net/Security/SslStream.Implementation.cs#L1404
        private static int GetFrameSize(Framing framing, in ReadOnlySpan<byte> buffer)
        {
            int payloadSize = -1;
            switch (framing)
            {
                case Framing.Unified:
                case Framing.BeforeSSL3:
                    // Note: Cannot detect version mismatch for <= SSL2

                    if ((buffer[0] & 0x80) != 0)
                    {
                        // Two bytes
                        payloadSize = (((buffer[0] & 0x7f) << 8) | buffer[1]) + 2;
                    }
                    else
                    {
                        // Three bytes
                        payloadSize = (((buffer[0] & 0x3f) << 8) | buffer[1]) + 3;
                    }

                    break;
                case Framing.SinceSSL3:
                    payloadSize = ((buffer[3] << 8) | buffer[4]) + 5;
                    break;
            }

            return payloadSize;
        }

        private static int GetFrameSize(Framing framing, IByteBuffer buffer, int offset)
        {
            int payloadSize = -1;
            switch (framing)
            {
                case Framing.Unified:
                case Framing.BeforeSSL3:
                    // Note: Cannot detect version mismatch for <= SSL2
                    var first = buffer.GetByte(offset);
                    var second = buffer.GetByte(offset + 1);
                    if ((first & 0x80) != 0)
                    {
                        // Two bytes
                        payloadSize = (((first & 0x7f) << 8) | second) + 2;
                    }
                    else
                    {
                        // Three bytes
                        payloadSize = (((first & 0x3f) << 8) | second) + 3;
                    }

                    break;
                case Framing.SinceSSL3:
                    payloadSize = ((buffer.GetByte(offset + 3) << 8) | buffer.GetByte(offset + 4)) + 5;
                    break;
            }

            return payloadSize;
        }

        #endregion

        #region ** class SslHandlerCoalescingBufferQueue **

        /// <summary>
        /// Each call to SSL_write will introduce about ~100 bytes of overhead. This coalescing queue attempts to increase
        /// goodput by aggregating the plaintext in chunks of <see cref="v_wrapDataSize"/>. If many small chunks are written
        /// this can increase goodput, decrease the amount of calls to SSL_write, and decrease overall encryption operations.
        /// </summary>
        private sealed class SslHandlerCoalescingBufferQueue : AbstractCoalescingBufferQueue
        {
            private readonly TlsHandler _owner;

            public SslHandlerCoalescingBufferQueue(TlsHandler owner, IChannel channel, int initSize)
                : base(channel, initSize)
            {
                _owner = owner;
            }

            protected override IByteBuffer Compose(IByteBufferAllocator alloc, IByteBuffer cumulation, IByteBuffer next)
            {
                int wrapDataSize = _owner.v_wrapDataSize;
                if (cumulation is CompositeByteBuffer composite)
                {
                    int numComponents = composite.NumComponents;
                    if (0u >= (uint)numComponents ||
                        !AttemptCopyToCumulation(composite.InternalComponent(numComponents - 1), next, wrapDataSize))
                    {
                        composite.AddComponent(true, next);
                    }
                    return composite;
                }
                return AttemptCopyToCumulation(cumulation, next, wrapDataSize)
                    ? cumulation
                    : CopyAndCompose(alloc, cumulation, next);
            }

            protected override IByteBuffer ComposeFirst(IByteBufferAllocator allocator, IByteBuffer first)
            {
                if (first is CompositeByteBuffer composite)
                {
                    first = allocator.DirectBuffer(composite.ReadableBytes);
                    try
                    {
                        first.WriteBytes(composite);
                    }
                    catch (Exception cause)
                    {
                        first.Release();
                        ExceptionDispatchInfo.Capture(cause).Throw();
                    }
                    composite.Release();
                }
                return first;
            }

            protected override IByteBuffer RemoveEmptyValue()
            {
                return null;
            }

            private static bool AttemptCopyToCumulation(IByteBuffer cumulation, IByteBuffer next, int wrapDataSize)
            {
                int inReadableBytes = next.ReadableBytes;
                int cumulationCapacity = cumulation.Capacity;
                if (wrapDataSize - cumulation.ReadableBytes >= inReadableBytes &&
                    // Avoid using the same buffer if next's data would make cumulation exceed the wrapDataSize.
                    // Only copy if there is enough space available and the capacity is large enough, and attempt to
                    // resize if the capacity is small.
                    ((cumulation.IsWritable(inReadableBytes) && cumulationCapacity >= wrapDataSize) ||
                    (cumulationCapacity < wrapDataSize && ByteBufferUtil.EnsureWritableSuccess(cumulation.EnsureWritable(inReadableBytes, false)))))
                {
                    cumulation.WriteBytes(next);
                    next.Release();
                    return true;
                }
                return false;
            }
        }

        #endregion


    }
}
