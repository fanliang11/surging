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
    using System.Runtime.CompilerServices;
    using DotNetty.Codecs.Compression;

    /// <summary>
    /// <a href="http://tools.ietf.org/html/draft-ietf-hybi-permessage-compression-18">permessage-deflate</a>
    /// handshake implementation.
    /// </summary>
    public sealed class PerMessageDeflateServerExtensionHandshaker : IWebSocketServerExtensionHandshaker
    {
        public const int MinWindowSize = 8;
        public const int MaxWindowSize = 15;

        internal const string PerMessageDeflateExtension = "permessage-deflate";
        internal const string ClientMaxWindow = "client_max_window_bits";
        internal const string ServerMaxWindow = "server_max_window_bits";
        internal const string ClientNoContext = "client_no_context_takeover";
        internal const string ServerNoContext = "server_no_context_takeover";

        private readonly int _compressionLevel;
        private readonly bool _allowServerWindowSize;
        private readonly int _preferredClientWindowSize;
        private readonly bool _allowServerNoContext;
        private readonly bool _preferredClientNoContext;
        private readonly IWebSocketExtensionFilterProvider _extensionFilterProvider;

        /// <summary>Constructor with default configuration.</summary>
        public PerMessageDeflateServerExtensionHandshaker()
            : this(6, ZlibCodecFactory.IsSupportingWindowSizeAndMemLevel, MaxWindowSize, false, false)
        {
        }

        /// <summary>Constructor with custom configuration.</summary>
        /// <param name="compressionLevel">Compression level between 0 and 9 (default is 6).</param>
        /// <param name="allowServerWindowSize">allows WebSocket client to customize the server inflater window size
        /// (default is false).</param>
        /// <param name="preferredClientWindowSize">indicates the preferred client window size to use if client inflater is customizable.</param>
        /// <param name="allowServerNoContext">allows WebSocket client to activate server_no_context_takeover
        /// (default is false).</param>
        /// <param name="preferredClientNoContext">indicates if server prefers to activate client_no_context_takeover
        /// if client is compatible with (default is false).</param>
        public PerMessageDeflateServerExtensionHandshaker(int compressionLevel,
            bool allowServerWindowSize, int preferredClientWindowSize,
            bool allowServerNoContext, bool preferredClientNoContext)
            : this(compressionLevel, allowServerWindowSize, preferredClientWindowSize, allowServerNoContext,
                preferredClientNoContext, WebSocketExtensionFilterProvider.Default)
        {
        }

        /// <summary>Constructor with custom configuration.</summary>
        /// <param name="compressionLevel">Compression level between 0 and 9 (default is 6).</param>
        /// <param name="allowServerWindowSize">allows WebSocket client to customize the server inflater window size
        /// (default is false).</param>
        /// <param name="preferredClientWindowSize">indicates the preferred client window size to use if client inflater is customizable.</param>
        /// <param name="allowServerNoContext">allows WebSocket client to activate server_no_context_takeover
        /// (default is false).</param>
        /// <param name="preferredClientNoContext">indicates if server prefers to activate client_no_context_takeover
        /// if client is compatible with (default is false).</param>
        /// <param name="extensionFilterProvider">provides server extension filters for per message deflate encoder and decoder.</param>
        public PerMessageDeflateServerExtensionHandshaker(int compressionLevel,
            bool allowServerWindowSize, int preferredClientWindowSize,
            bool allowServerNoContext, bool preferredClientNoContext,
            IWebSocketExtensionFilterProvider extensionFilterProvider)
        {
            uint upreferredClientWindowSize = (uint)preferredClientWindowSize;
            if (upreferredClientWindowSize > (uint)MaxWindowSize || upreferredClientWindowSize < (uint)MinWindowSize)
            {
                ThrowHelper.ThrowArgumentException_WindowSize(ExceptionArgument.preferredClientWindowSize, preferredClientWindowSize);
            }
            if (/*compressionLevel < 0 || */(uint)compressionLevel > 9u)
            {
                ThrowHelper.ThrowArgumentException_CompressionLevel(compressionLevel);
            }
            if (extensionFilterProvider is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.extensionFilterProvider); }

            _compressionLevel = compressionLevel;
            _allowServerWindowSize = allowServerWindowSize;
            _preferredClientWindowSize = preferredClientWindowSize;
            _allowServerNoContext = allowServerNoContext;
            _preferredClientNoContext = preferredClientNoContext;
            _extensionFilterProvider = extensionFilterProvider;
        }

        public IWebSocketServerExtension HandshakeExtension(WebSocketExtensionData extensionData)
        {
            if (!IsPerMessageDeflateExtension(extensionData.Name)) { return null; }

            bool deflateEnabled = true;
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
                        // use preferred clientWindowSize because client is compatible with customization
                        clientWindowSize = _preferredClientWindowSize;
                        break;

                    case ServerMaxWindow:
                        // use provided windowSize if it is allowed
                        if (_allowServerWindowSize)
                        {
                            serverWindowSize = int.Parse(parameter.Value);
                            uint userverWindowSize = (uint)serverWindowSize;
                            if (userverWindowSize > (uint)MaxWindowSize || userverWindowSize < (uint)MinWindowSize)
                            {
                                deflateEnabled = false;
                            }
                        }
                        else
                        {
                            deflateEnabled = false;
                        }
                        break;

                    case ClientNoContext:
                        // use preferred clientNoContext because client is compatible with customization
                        clientNoContext = _preferredClientNoContext;
                        break;

                    case ServerNoContext:
                        // use server no context if allowed
                        if (_allowServerNoContext)
                        {
                            serverNoContext = true;
                        }
                        else
                        {
                            deflateEnabled = false;
                        }
                        break;

                    default:
                        if (string.Equals(ClientMaxWindow, parameterKey, StringComparison.OrdinalIgnoreCase))
                        {
                            // use preferred clientWindowSize because client is compatible with customization
                            clientWindowSize = _preferredClientWindowSize;
                        }
                        else if (string.Equals(ServerMaxWindow, parameterKey, StringComparison.OrdinalIgnoreCase))
                        {
                            // use provided windowSize if it is allowed
                            if (_allowServerWindowSize)
                            {
                                serverWindowSize = int.Parse(parameter.Value);
                                uint userverWindowSize = (uint)serverWindowSize;
                                if (userverWindowSize > (uint)MaxWindowSize || userverWindowSize < (uint)MinWindowSize)
                                {
                                    deflateEnabled = false;
                                }
                            }
                            else
                            {
                                deflateEnabled = false;
                            }
                        }
                        else if (string.Equals(ClientNoContext, parameterKey, StringComparison.OrdinalIgnoreCase))
                        {
                            // use preferred clientNoContext because client is compatible with customization
                            clientNoContext = _preferredClientNoContext;
                        }
                        else if (string.Equals(ServerNoContext, parameterKey, StringComparison.OrdinalIgnoreCase))
                        {
                            // use server no context if allowed
                            if (_allowServerNoContext)
                            {
                                serverNoContext = true;
                            }
                            else
                            {
                                deflateEnabled = false;
                            }
                        }
                        else
                        {
                            // unknown parameter
                            deflateEnabled = false;
                        }
                        break;
                }

                if (!deflateEnabled)
                {
                    break;
                }
            }

            if (deflateEnabled)
            {
                return new WebSocketPermessageDeflateExtension(_compressionLevel, serverNoContext,
                    serverWindowSize, clientNoContext, clientWindowSize, _extensionFilterProvider);
            }
            else
            {
                return null;
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        internal static bool IsPerMessageDeflateExtension(string name)
        {
#if NETCOREAPP_3_0_GREATER || NETSTANDARD_2_0_GREATER
            if (string.Equals(PerMessageDeflateExtension, name)) { return true; }
#else
            if (string.Equals(PerMessageDeflateExtension, name, StringComparison.Ordinal)) { return true; }
#endif
            if (string.Equals(PerMessageDeflateExtension, name, StringComparison.OrdinalIgnoreCase)) { return true; }
            return false;
        }

        sealed class WebSocketPermessageDeflateExtension : IWebSocketServerExtension
        {
            private readonly int _compressionLevel;
            private readonly bool _serverNoContext;
            private readonly int _serverWindowSize;
            private readonly bool _clientNoContext;
            private readonly int _clientWindowSize;
            private readonly IWebSocketExtensionFilterProvider _extensionFilterProvider;

            public WebSocketPermessageDeflateExtension(int compressionLevel, bool serverNoContext,
                int serverWindowSize, bool clientNoContext, int clientWindowSize,
                IWebSocketExtensionFilterProvider extensionFilterProvider)
            {
                _compressionLevel = compressionLevel;
                _serverNoContext = serverNoContext;
                _serverWindowSize = serverWindowSize;
                _clientNoContext = clientNoContext;
                _clientWindowSize = clientWindowSize;
                _extensionFilterProvider = extensionFilterProvider;
            }

            public int Rsv => WebSocketRsv.Rsv1;

            public WebSocketExtensionEncoder NewExtensionEncoder()
                => new PerMessageDeflateEncoder(_compressionLevel, _serverWindowSize, _serverNoContext, _extensionFilterProvider.EncoderFilter);

            public WebSocketExtensionDecoder NewExtensionDecoder()
                => new PerMessageDeflateDecoder(_clientNoContext, _extensionFilterProvider.DecoderFilter);

            public WebSocketExtensionData NewReponseData()
            {
                var parameters = new Dictionary<string, string>(4, StringComparer.Ordinal);
                if (_serverNoContext)
                {
                    parameters.Add(ServerNoContext, null);
                }
                if (_clientNoContext)
                {
                    parameters.Add(ClientNoContext, null);
                }
                if (_serverWindowSize != MaxWindowSize)
                {
                    parameters.Add(ServerMaxWindow, Convert.ToString(_serverWindowSize));
                }
                if (_clientWindowSize != MaxWindowSize)
                {
                    parameters.Add(ClientMaxWindow, Convert.ToString(_clientWindowSize));
                }
                return new WebSocketExtensionData(PerMessageDeflateExtension, parameters);
            }
        }
    }
}
