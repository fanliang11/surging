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
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    /// Utilities for TLS packets.
    static class TlsUtils
    {
        /// <summary>
        /// <a href="https://tools.ietf.org/html/rfc5246#section-6.2">2^14</a> which is the maximum sized plaintext chunk
        /// allowed by the TLS RFC.
        /// </summary>
        public const int MAX_PLAINTEXT_LENGTH = 16 * 1024; // 2^14
        private const int MAX_COMPRESSED_LENGTH = MAX_PLAINTEXT_LENGTH + 1024;
        private const int MAX_CIPHERTEXT_LENGTH = MAX_COMPRESSED_LENGTH + 1024;
        private const int GMSSL_PROTOCOL_VERSION = 0x101;

        // Header (5) + Data (2^14) + Compression (1024) + Encryption (1024) + MAC (20) + Padding (256)
        public const int MAX_ENCRYPTED_PACKET_LENGTH = MAX_CIPHERTEXT_LENGTH + 5 + 20 + 256;


        // change cipher spec
        public const int SSL_CONTENT_TYPE_CHANGE_CIPHER_SPEC = 20;

        // alert
        public const int SSL_CONTENT_TYPE_ALERT = 21;

        // handshake
        public const int SSL_CONTENT_TYPE_HANDSHAKE = 22;

        // application data
        public const int SSL_CONTENT_TYPE_APPLICATION_DATA = 23;

        // HeartBeat Extension
        public const int SSL_CONTENT_TYPE_EXTENSION_HEARTBEAT = 24;

        // the length of the ssl record header (in bytes)
        public const int SSL_RECORD_HEADER_LENGTH = 5;

        // Not enough data in buffer to parse the record length
        public const int NOT_ENOUGH_DATA = -1;

        // data is not encrypted
        public const int NOT_ENCRYPTED = -2;

        /// <summary>
        ///     Return how much bytes can be read out of the encrypted data. Be aware that this method will not increase
        ///     the readerIndex of the given <see cref="IByteBuffer"/>.
        /// </summary>
        /// <param name="buffer">
        ///     The <see cref="IByteBuffer"/> to read from. Be aware that it must have at least
        ///     <see cref="SSL_RECORD_HEADER_LENGTH"/> bytes to read,
        ///     otherwise it will throw an <see cref="ArgumentException"/>.
        /// </param>
        /// <param name="offset">Offset to record start.</param>
        /// <returns>
        ///     The length of the encrypted packet that is included in the buffer. This will
        ///     return <c>-1</c> if the given <see cref="IByteBuffer"/> is not encrypted at all.
        /// </returns>
        public static int GetEncryptedPacketLength(IByteBuffer buffer, int offset)
        {
            int packetLength = 0;

            // SSLv3 or TLS - Check ContentType
            bool tls;
            switch (buffer.GetByte(offset))
            {
                case SSL_CONTENT_TYPE_CHANGE_CIPHER_SPEC:
                case SSL_CONTENT_TYPE_ALERT:
                case SSL_CONTENT_TYPE_HANDSHAKE:
                case SSL_CONTENT_TYPE_APPLICATION_DATA:
                case SSL_CONTENT_TYPE_EXTENSION_HEARTBEAT:
                    tls = true;
                    break;

                default:
                    // SSLv2 or bad data
                    tls = false;
                    break;
            }

            if (tls)
            {
                // SSLv3 or TLS or GMSSLv1.0 or GMSSLv1.1 - Check ProtocolVersion
                int majorVersion = buffer.GetByte(offset + 1);
                if (majorVersion == 3 || buffer.GetShort(offset + 1) == GMSSL_PROTOCOL_VERSION)
                {
                    // SSLv3 or TLS or GMSSLv1.0 or GMSSLv1.1
                    packetLength = buffer.GetUnsignedShort(offset + 3) + SSL_RECORD_HEADER_LENGTH;
                    if ((uint)packetLength <= SSL_RECORD_HEADER_LENGTH)
                    {
                        // Neither SSLv3 or TLSv1 (i.e. SSLv2 or bad data)
                        tls = false;
                    }
                }
                else
                {
                    // Neither SSLv3 or TLSv1 (i.e. SSLv2 or bad data)
                    tls = false;
                }
            }

            if (!tls)
            {
                // SSLv2 or bad data - Check the version
                uint uHeaderLength = (buffer.GetByte(offset) & 0x80) != 0 ? 2u : 3u;
                uint uMajorVersion = buffer.GetByte(offset + (int)uHeaderLength + 1);
                if (uMajorVersion == 2u || uMajorVersion == 3u)
                {
                    // SSLv2
                    packetLength = uHeaderLength == 2u ?
                            (buffer.GetShort(offset) & 0x7FFF) + 2 : (buffer.GetShort(offset) & 0x3FFF) + 3;
                    if (uHeaderLength >= (uint)packetLength)
                    {
                        return NOT_ENOUGH_DATA;
                    }
                }
                else
                {
                    return NOT_ENCRYPTED;
                }
            }

            return packetLength;
        }

        public static void NotifyHandshakeFailure(IChannelHandlerContext ctx, Exception cause, bool notify)
        {
            // We have may haven written some parts of data before an exception was thrown so ensure we always flush.
            // See https://github.com/netty/netty/issues/3900#issuecomment-172481830
            try
            {
                ctx.Flush();
            }
            catch { }
            if (notify)
            {
                ctx.FireUserEventTriggered(new TlsHandshakeCompletionEvent(cause));
            }
            ctx.CloseAsync().Ignore();
        }
    }
}