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

namespace DotNetty.Codecs.Http.WebSockets
{
    /// <summary>Indicates the WebSocket frame type.</summary>
    /// <remarks>
    /// The values of this enumeration are defined in <see
    /// href="http://tools.ietf.org/html/rfc6455#section-5.2"> Section 5.2</see> of RFC 6455.
    /// </remarks>
    internal enum Opcode : byte
    {
        /// <summary>Equivalent to numeric value 0. Indicates continuation frame.</summary>
        Cont = 0x0,

        /// <summary>Equivalent to numeric value 1. Indicates text frame.</summary>
        Text = 0x1,

        /// <summary>Equivalent to numeric value 2. Indicates binary frame.</summary>
        Binary = 0x2,

        /// <summary>Equivalent to numeric value 8. Indicates connection close frame.</summary>
        Close = 0x8,

        /// <summary>Equivalent to numeric value 9. Indicates ping frame.</summary>
        Ping = 0x9,

        /// <summary>Equivalent to numeric value 10. Indicates pong frame.</summary>
        Pong = 0xa
    }
}