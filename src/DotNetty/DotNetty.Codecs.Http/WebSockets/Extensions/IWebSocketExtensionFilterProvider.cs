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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.Http.WebSockets.Extensions
{
    /// <summary>
    /// Extension filter provider that is responsible to provide filters for a certain <see cref="IWebSocketExtension"/> extension.
    /// </summary>
    public interface IWebSocketExtensionFilterProvider
    {
        /// <summary>
        /// Returns the extension filter for <see cref="WebSocketExtensionEncoder"/> encoder.
        /// </summary>
        IWebSocketExtensionFilter EncoderFilter { get; }

        /// <summary>
        /// Returns the extension filter for <see cref="WebSocketExtensionDecoder"/> decoder.
        /// </summary>
        IWebSocketExtensionFilter DecoderFilter { get; }
    }
}
