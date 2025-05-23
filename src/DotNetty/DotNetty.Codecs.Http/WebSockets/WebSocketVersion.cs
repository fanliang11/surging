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

namespace DotNetty.Codecs.Http.WebSockets
{
    using DotNetty.Common.Utilities;

    public sealed class WebSocketVersion
    {
        public static readonly WebSocketVersion Unknown = new WebSocketVersion(string.Empty);

        // http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-00
        // draft-ietf-hybi-thewebsocketprotocol- 00.
        public static readonly WebSocketVersion V00 = new WebSocketVersion("0");

        // http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-07
        // draft-ietf-hybi-thewebsocketprotocol- 07
        public static readonly WebSocketVersion V07 = new WebSocketVersion("7");

        // http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-10
        // draft-ietf-hybi-thewebsocketprotocol- 10
        public static readonly WebSocketVersion V08 = new WebSocketVersion("8");

        // http://tools.ietf.org/html/rfc6455 This was originally
        // http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-17 
        //draft-ietf-hybi-thewebsocketprotocol- 17>
        public static readonly WebSocketVersion V13 = new WebSocketVersion("13");

        private readonly AsciiString _value;

        WebSocketVersion(string value)
        {
            _value = AsciiString.Cached(value);
        }

        public override string ToString() => _value.ToString();

        public AsciiString ToHttpHeaderValue()
        {
            if (this == Unknown) ThrowHelper.ThrowInvalidOperationException_UnknownWebSocketVersion();
            return _value;
        }
    }
}
