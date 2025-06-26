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
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Codecs.Http.WebSockets.Extensions.Compression
{
    using System;
    using System.Collections.Generic;
    using DotNetty.Codecs.Compression;

    using static PerMessageDeflateServerExtensionHandshaker;

    /// <summary>
    /// <a href="http://tools.ietf.org/html/draft-ietf-hybi-permessage-compression-18">permessage-deflate</a>
    /// handshake implementation.
    /// </summary>
    public sealed class PerMessageDeflateClientExtensionHandshaker : IWebSocketClientExtensionHandshaker
    {
        private readonly int _compressionLevel;
        private readonly bool _allowClientWindowSize;
        private readonly int _requestedServerWindowSize;
        private readonly bool _allowClientNoContext;
        private readonly bool _requestedServerNoContext;
        private readonly IWebSocketExtensionFilterProvider _extensionFilterProvider;

        /// <summary>Constructor with default configuration.</summary>
        public PerMessageDeflateClientExtensionHandshaker()
            : this(6, ZlibCodecFactory.IsSupportingWindowSizeAndMemLevel, MaxWindowSize, false, false)
        {
        }

        /// <summary>Constructor with custom configuration.</summary>
        /// <param name="compressionLevel">Compression level between 0 and 9 (default is 6).</param>
        /// <param name="allowClientWindowSize">allows WebSocket server to customize the client inflater window size
        /// (default is false).</param>
        /// <param name="requestedServerWindowSize">indicates the requested sever window size to use if server inflater is customizable.</param>
        /// <param name="allowClientNoContext">allows WebSocket server to activate client_no_context_takeover
        /// (default is false).</param>
        /// <param name="requestedServerNoContext">indicates if client needs to activate server_no_context_takeover
        /// if server is compatible with (default is false).</param>
        public PerMessageDeflateClientExtensionHandshaker(int compressionLevel,
            bool allowClientWindowSize, int requestedServerWindowSize,
            bool allowClientNoContext, bool requestedServerNoContext)
            : this(compressionLevel, allowClientWindowSize, requestedServerWindowSize,
                 allowClientNoContext, requestedServerNoContext, WebSocketExtensionFilterProvider.Default)
        {
        }

        /// <summary>Constructor with custom configuration.</summary>
        /// <param name="compressionLevel">Compression level between 0 and 9 (default is 6).</param>
        /// <param name="allowClientWindowSize">allows WebSocket server to customize the client inflater window size
        /// (default is false).</param>
        /// <param name="requestedServerWindowSize">indicates the requested sever window size to use if server inflater is customizable.</param>
        /// <param name="allowClientNoContext">allows WebSocket server to activate client_no_context_takeover
        /// (default is false).</param>
        /// <param name="requestedServerNoContext">indicates if client needs to activate server_no_context_takeover
        /// if server is compatible with (default is false).</param>
        /// <param name="extensionFilterProvider">provides client extension filters for per message deflate encoder and decoder.</param>
        public PerMessageDeflateClientExtensionHandshaker(int compressionLevel,
            bool allowClientWindowSize, int requestedServerWindowSize,
            bool allowClientNoContext, bool requestedServerNoContext,
            IWebSocketExtensionFilterProvider extensionFilterProvider)
        {
            uint urequestedServerWindowSize = (uint)requestedServerWindowSize;
            if (urequestedServerWindowSize > (uint)MaxWindowSize || urequestedServerWindowSize < (uint)MinWindowSize)
            {
                ThrowHelper.ThrowArgumentException_WindowSize(ExceptionArgument.requestedServerWindowSize, requestedServerWindowSize);
            }
            if (/*compressionLevel < 0 || */(uint)compressionLevel > 9u)
            {
                ThrowHelper.ThrowArgumentException_CompressionLevel(compressionLevel);
            }
            if (extensionFilterProvider is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.extensionFilterProvider); }

            _compressionLevel = compressionLevel;
            _allowClientWindowSize = allowClientWindowSize;
            _requestedServerWindowSize = requestedServerWindowSize;
            _allowClientNoContext = allowClientNoContext;
            _requestedServerNoContext = requestedServerNoContext;
            _extensionFilterProvider = extensionFilterProvider;
        }

        public WebSocketExtensionData NewRequestData()
        {
            var parameters = new Dictionary<string, string>(4, StringComparer.Ordinal);
            if (_requestedServerWindowSize != MaxWindowSize)
            {
                parameters.Add(ServerNoContext, null);
            }
            if (_allowClientNoContext)
            {
                parameters.Add(ClientNoContext, null);
            }
            if (_requestedServerWindowSize != MaxWindowSize)
            {
                parameters.Add(ServerMaxWindow, Convert.ToString(_requestedServerWindowSize));
            }
            if (_allowClientWindowSize)
            {
                parameters.Add(ClientMaxWindow, null);
            }
            return new WebSocketExtensionData(PerMessageDeflateExtension, parameters);
        }

        public IWebSocketClientExtension HandshakeExtension(WebSocketExtensionData extensionData)
        {
            if (!IsPerMessageDeflateExtension(extensionData.Name)) { return null; }

            bool succeed = true;
            int clientWindowSize = MaxWindowSize;
            int serverWindowSize = MaxWindowSize;
            bool serverNoContext = false;
            bool clientNoContext = false;

            foreach (KeyValuePair<string, string> parameter in extensionData.Parameters)
            {
                var parameterKey = parameter.Key;
                switch (parameterKey)
                {
                    case ClientMaxWindow:
                        // allowed client_window_size_bits
                        if (_allowClientWindowSize)
                        {
                            clientWindowSize = int.Parse(parameter.Value);
                        }
                        else
                        {
                            succeed = false;
                        }
                        break;

                    case ServerMaxWindow:
                        // acknowledged server_window_size_bits
                        serverWindowSize = int.Parse(parameter.Value);
                        uint uclientWindowSize0 = (uint)clientWindowSize;
                        if (uclientWindowSize0 > (uint)MaxWindowSize || uclientWindowSize0 < (uint)MinWindowSize)
                        {
                            succeed = false;
                        }
                        break;

                    case ClientNoContext:
                        // allowed client_no_context_takeover
                        if (_allowClientNoContext)
                        {
                            clientNoContext = true;
                        }
                        else
                        {
                            succeed = false;
                        }
                        break;

                    case ServerNoContext:
                        // acknowledged server_no_context_takeover
                        if (_requestedServerNoContext)
                        {
                            serverNoContext = true;
                        }
                        else
                        {
                            succeed = false;
                        }
                        break;

                    default:
                        if (string.Equals(ClientMaxWindow, parameterKey, StringComparison.OrdinalIgnoreCase))
                        {
                            // allowed client_window_size_bits
                            if (_allowClientWindowSize)
                            {
                                clientWindowSize = int.Parse(parameter.Value);
                            }
                            else
                            {
                                succeed = false;
                            }
                        }
                        else if (string.Equals(ServerMaxWindow, parameterKey, StringComparison.OrdinalIgnoreCase))
                        {
                            // acknowledged server_window_size_bits
                            serverWindowSize = int.Parse(parameter.Value);
                            uint uclientWindowSize = (uint)clientWindowSize;
                            if (uclientWindowSize > (uint)MaxWindowSize || uclientWindowSize < (uint)MinWindowSize)
                            {
                                succeed = false;
                            }
                        }
                        else if (string.Equals(ClientNoContext, parameterKey, StringComparison.OrdinalIgnoreCase))
                        {
                            // allowed client_no_context_takeover
                            if (_allowClientNoContext)
                            {
                                clientNoContext = true;
                            }
                            else
                            {
                                succeed = false;
                            }
                        }
                        else if (string.Equals(ServerNoContext, parameterKey, StringComparison.OrdinalIgnoreCase))
                        {
                            // acknowledged server_no_context_takeover
                            if (_requestedServerNoContext)
                            {
                                serverNoContext = true;
                            }
                            else
                            {
                                succeed = false;
                            }
                        }
                        else
                        {
                            // unknown parameter
                            succeed = false;
                        }
                        break;
                }

                if (!succeed)
                {
                    break;
                }
            }

            if ((_requestedServerNoContext && !serverNoContext)
                || _requestedServerWindowSize != serverWindowSize)
            {
                succeed = false;
            }

            if (succeed)
            {
                return new WebSocketPermessageDeflateExtension(serverNoContext, serverWindowSize,
                    clientNoContext, clientWindowSize, _compressionLevel, _extensionFilterProvider);
            }
            else
            {
                return null;
            }
        }

        sealed class WebSocketPermessageDeflateExtension : IWebSocketClientExtension
        {
            private readonly bool _serverNoContext;
            private readonly int _serverWindowSize;
            private readonly bool _clientNoContext;
            private readonly int _clientWindowSize;
            private readonly int _compressionLevel;
            private readonly IWebSocketExtensionFilterProvider _extensionFilterProvider;

            public int Rsv => WebSocketRsv.Rsv1;

            public WebSocketPermessageDeflateExtension(bool serverNoContext, int serverWindowSize,
                bool clientNoContext, int clientWindowSize, int compressionLevel,
                IWebSocketExtensionFilterProvider extensionFilterProvider)
            {
                _serverNoContext = serverNoContext;
                _serverWindowSize = serverWindowSize;
                _clientNoContext = clientNoContext;
                _clientWindowSize = clientWindowSize;
                _compressionLevel = compressionLevel;
                _extensionFilterProvider = extensionFilterProvider;
            }

            public WebSocketExtensionEncoder NewExtensionEncoder()
                => new PerMessageDeflateEncoder(_compressionLevel, _clientWindowSize, _clientNoContext, _extensionFilterProvider.EncoderFilter);

            public WebSocketExtensionDecoder NewExtensionDecoder()
                => new PerMessageDeflateDecoder(_serverNoContext, _extensionFilterProvider.DecoderFilter);
        }
    }
}
