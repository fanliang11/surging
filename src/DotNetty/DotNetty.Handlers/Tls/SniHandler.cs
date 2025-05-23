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
    using System.Globalization;
    using System.IO;
    using System.Net.Security;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Codecs;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Transport.Channels;

    public sealed class SniHandler : ByteToMessageDecoder
    {
        // Maximal number of ssl records to inspect before fallback to the default server TLS setting (aligned with netty) 
        private const int MAX_SSL_RECORDS = 4;
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance(typeof(SniHandler));
        private static readonly CultureInfo UnitedStatesCultureInfo = new CultureInfo("en-US");

        private readonly Func<Stream, SslStream> _sslStreamFactory;
        private readonly ServerTlsSniSettings _serverTlsSniSettings;

        private bool _handshakeFailed;
        private bool _suppressRead;
        private bool _readPending;
        private IByteBuffer _handshakeBuffer;

        public SniHandler(ServerTlsSniSettings settings)
            : this(stream => new SslStream(stream, true), settings)
        {
        }

        public SniHandler(Func<Stream, SslStream> sslStreamFactory, ServerTlsSniSettings settings)
        {
            if (settings is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.settings); }
            if (sslStreamFactory is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.sslStreamFactory); }
            _sslStreamFactory = sslStreamFactory;
            _serverTlsSniSettings = settings;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            if (!_suppressRead && !_handshakeFailed)
            {
                Exception error = null;
                try
                {
                    int readerIndex = input.ReaderIndex;
                    int readableBytes = input.ReadableBytes;
                    int handshakeLength = -1;

                    // Check if we have enough data to determine the record type and length.
                    while (readableBytes >= TlsUtils.SSL_RECORD_HEADER_LENGTH)
                    {
                        int contentType = input.GetByte(readerIndex);
                        // tls, but not handshake command
                        switch (contentType)
                        {
                            case TlsUtils.SSL_CONTENT_TYPE_CHANGE_CIPHER_SPEC:
                            // fall-through
                            case TlsUtils.SSL_CONTENT_TYPE_ALERT:
                                int len = TlsUtils.GetEncryptedPacketLength(input, readerIndex);

                                // Not an SSL/TLS packet
                                if (len == TlsUtils.NOT_ENCRYPTED)
                                {
                                    _handshakeFailed = true;
                                    var e = new NotSslRecordException(
                                        "not an SSL/TLS record: " + ByteBufferUtil.HexDump(input));
                                    _ = input.SkipBytes(input.ReadableBytes);
                                    _ = context.FireUserEventTriggered(new SniCompletionEvent(e));
                                    TlsUtils.NotifyHandshakeFailure(context, e, true);
                                    throw e;
                                }
                                if (len == TlsUtils.NOT_ENOUGH_DATA)
                                {
                                    // Not enough data
                                    return;
                                }
                                // SNI can't be present in an ALERT or CHANGE_CIPHER_SPEC record, so we'll fall back and
                                // assume no SNI is present. Let's let the actual TLS implementation sort this out.
                                // Just select the default SslContext
                                goto SelectDefault;

                            case TlsUtils.SSL_CONTENT_TYPE_HANDSHAKE:
                                int majorVersion = input.GetByte(readerIndex + 1);

                                // SSLv3 or TLS
                                if (majorVersion == 3)
                                {
                                    int packetLength = input.GetUnsignedShort(readerIndex + 3) + TlsUtils.SSL_RECORD_HEADER_LENGTH;

                                    if (readableBytes < packetLength)
                                    {
                                        // client hello incomplete; try again to decode once more data is ready.
                                        return;
                                    }
                                    else if (packetLength == TlsUtils.SSL_RECORD_HEADER_LENGTH)
                                    {
                                        goto SelectDefault;
                                    }


                                    int endOffset = readerIndex + packetLength;

                                    // Let's check if we already parsed the handshake length or not.
                                    if (handshakeLength == -1)
                                    {
                                        if (readerIndex + 4 > endOffset)
                                        {
                                            // Need more data to read HandshakeType and handshakeLength (4 bytes)
                                            return;
                                        }

                                        int handshakeType = input.GetByte(readerIndex + TlsUtils.SSL_RECORD_HEADER_LENGTH);

                                        // Check if this is a clientHello(1)
                                        // See https://tools.ietf.org/html/rfc5246#section-7.4
                                        if (handshakeType != 1)
                                        {
                                            goto SelectDefault;
                                        }

                                        // Read the length of the handshake as it may arrive in fragments
                                        // See https://tools.ietf.org/html/rfc5246#section-7.4
                                        handshakeLength = input.GetUnsignedMedium(readerIndex + TlsUtils.SSL_RECORD_HEADER_LENGTH + 1);

                                        // Consume handshakeType and handshakeLength (this sums up as 4 bytes)
                                        readerIndex += 4;
                                        packetLength -= 4;

                                        if (handshakeLength + 4 + TlsUtils.SSL_RECORD_HEADER_LENGTH <= packetLength)
                                        {
                                            // We have everything we need in one packet.
                                            // Skip the record header
                                            readerIndex += TlsUtils.SSL_RECORD_HEADER_LENGTH;
                                            Select(context, ExtractSniHostname(input, readerIndex, readerIndex + handshakeLength));
                                            return;
                                        }
                                        else
                                        {
                                            if (_handshakeBuffer is null)
                                            {
                                                _handshakeBuffer = context.Allocator.Buffer(handshakeLength);
                                            }
                                            else
                                            {
                                                // Clear the buffer so we can aggregate into it again.
                                                _ = _handshakeBuffer.Clear();
                                            }
                                        }
                                    }

                                    // Combine the encapsulated data in one buffer but not include the SSL_RECORD_HEADER
                                    _ = _handshakeBuffer.WriteBytes(input, readerIndex + TlsUtils.SSL_RECORD_HEADER_LENGTH,
                                            packetLength - TlsUtils.SSL_RECORD_HEADER_LENGTH);
                                    readerIndex += packetLength;
                                    readableBytes -= packetLength;
                                    if (handshakeLength <= _handshakeBuffer.ReadableBytes)
                                    {
                                        Select(context, ExtractSniHostname(_handshakeBuffer, 0, handshakeLength));
                                        return;
                                    }
                                }
                                break;

                            default:
                                //not tls, ssl or application data, do not try sni
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    error = e;

                    // unexpected encoding, ignore sni and use default
                    if (Logger.WarnEnabled)
                    {
                        Logger.UnexpectedClientHelloPacket(input, e);
                    }
                }

            SelectDefault:
                if (_serverTlsSniSettings.DefaultServerHostName is object)
                {
                    // Just select the default server TLS setting
                    Select(context, _serverTlsSniSettings.DefaultServerHostName);
                }
                else
                {
                    ReleaseHandshakeBuffer();

                    _handshakeFailed = true;
                    var e = new DecoderException($"failed to get the server TLS setting {error}");
                    TlsUtils.NotifyHandshakeFailure(context, e, true);
                    throw e;
                }
            }
        }

        private static string ExtractSniHostname(IByteBuffer input, int offset, int endOffset)
        {
            // See https://tools.ietf.org/html/rfc5246#section-7.4.1.2
            //
            // Decode the ssl client hello packet.
            //
            // struct {
            //    ProtocolVersion client_version;
            //    Random random;
            //    SessionID session_id;
            //    CipherSuite cipher_suites<2..2^16-2>;
            //    CompressionMethod compression_methods<1..2^8-1>;
            //    select (extensions_present) {
            //        case false:
            //            struct {};
            //        case true:
            //            Extension extensions<0..2^16-1>;
            //    };
            // } ClientHello;
            //

            // We have to skip bytes until SessionID (which sum to 34 bytes in this case).
            offset += 34;

            if (endOffset - offset >= 6)
            {
                int sessionIdLength = input.GetByte(offset);
                offset += sessionIdLength + 1;

                int cipherSuitesLength = input.GetUnsignedShort(offset);
                offset += cipherSuitesLength + 2;

                int compressionMethodLength = input.GetByte(offset);
                offset += compressionMethodLength + 1;

                int extensionsLength = input.GetUnsignedShort(offset);
                offset += 2;
                int extensionsLimit = offset + extensionsLength;

                // Extensions should never exceed the record boundary.
                if (extensionsLimit <= endOffset)
                {
                    while (extensionsLimit - offset >= 4)
                    {
                        int extensionType = input.GetUnsignedShort(offset);
                        offset += 2;

                        int extensionLength = input.GetUnsignedShort(offset);
                        offset += 2;

                        if (extensionsLimit - offset < extensionLength)
                        {
                            break;
                        }

                        // SNI
                        // See https://tools.ietf.org/html/rfc6066#page-6
                        if (0u >= (uint)extensionType)
                        {
                            offset += 2;
                            if (extensionsLimit - offset < 3)
                            {
                                break;
                            }

                            int serverNameType = input.GetByte(offset);
                            offset++;

                            if (0u >= (uint)serverNameType)
                            {
                                int serverNameLength = input.GetUnsignedShort(offset);
                                offset += 2;

                                if ((uint)(serverNameLength - 1) > SharedConstants.TooBigOrNegative/*serverNameLength <= 0*/ ||
                                    extensionsLimit - offset < serverNameLength)
                                {
                                    break;
                                }

                                string hostname = input.ToString(offset, serverNameLength, Encoding.UTF8);
                                var idn = new IdnMapping() { AllowUnassigned = true };
                                return idn.GetAscii(hostname).ToLower(UnitedStatesCultureInfo);
                            }
                            else
                            {
                                // invalid enum value
                                break;
                            }
                        }

                        offset += extensionLength;
                    }
                }
            }
            return null;
        }

        private void ReleaseHandshakeBuffer()
        {
            var handshakeBuffer = _handshakeBuffer;
            if (handshakeBuffer is object)
            {
                _ = handshakeBuffer.Release();
                _handshakeBuffer = null;
            }
        }

        private async void Select(IChannelHandlerContext context, string hostName)
        {
            if (hostName is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.hostName); }

            ReleaseHandshakeBuffer();

            _suppressRead = true;
            try
            {
                var serverTlsSetting = await _serverTlsSniSettings.ServerTlsSettingMap(hostName);
                ReplaceHandler(context, serverTlsSetting);
            }
            catch (Exception ex)
            {
                ExceptionCaught(context, new DecoderException($"failed to get the server TLS setting for {hostName}, {ex}"));
            }
            finally
            {
                _suppressRead = false;
                if (_readPending)
                {
                    _readPending = false;
                    _ = context.Read();
                }
            }
        }

        private void ReplaceHandler(IChannelHandlerContext context, ServerTlsSettings serverTlsSetting)
        {
            if (serverTlsSetting is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.serverTlsSetting); }
            var tlsHandler = new TlsHandler(_sslStreamFactory, serverTlsSetting);
            _ = context.Pipeline.Replace(this, nameof(TlsHandler), tlsHandler);
        }

        protected override void HandlerRemovedInternal(IChannelHandlerContext context)
        {
            ReleaseHandshakeBuffer();
            base.HandlerRemovedInternal(context);
        }

        public override void Read(IChannelHandlerContext context)
        {
            if (_suppressRead)
            {
                _readPending = true;
            }
            else
            {
                base.Read(context);
            }
        }
    }
}
