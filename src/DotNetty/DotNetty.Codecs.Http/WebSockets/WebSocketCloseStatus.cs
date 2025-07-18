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
using System.Runtime.CompilerServices;
using DotNetty.Common.Utilities;

namespace DotNetty.Codecs.Http.WebSockets
{
    /// <summary>
    /// WebSocket status codes specified in RFC-6455.
    /// <para>
    ///
    /// RFC-6455 The WebSocket Protocol, December 2011:
    /// <a href="https://tools.ietf.org/html/rfc6455#section-7.4.1"
    ///         >https://tools.ietf.org/html/rfc6455#section-7.4.1</a>
    ///
    /// WebSocket Protocol Registries, April 2019:
    /// <a href="https://www.iana.org/assignments/websocket/websocket.xhtml#close-code-number"
    ///         >https://www.iana.org/assignments/websocket/websocket.xhtml</a>
    ///
    /// 7.4.1.  Defined Status Codes
    ///
    /// Endpoints MAY use the following pre-defined status codes when sending
    /// a Close frame.
    ///
    /// 1000
    ///
    ///    1000 indicates a normal closure, meaning that the purpose for
    ///    which the connection was established has been fulfilled.
    ///
    /// 1001
    ///
    ///    1001 indicates that an endpoint is "going away", such as a server
    ///    going down or a browser having navigated away from a page.
    ///
    /// 1002
    ///
    ///    1002 indicates that an endpoint is terminating the connection due
    ///    to a protocol error.
    ///
    /// 1003
    ///
    ///    1003 indicates that an endpoint is terminating the connection
    ///    because it has received a type of data it cannot accept (e.g., an
    ///    endpoint that understands only text data MAY send this if it
    ///    receives a binary message).
    ///
    /// 1004
    ///
    ///    Reserved. The specific meaning might be defined in the future.
    ///
    /// 1005
    ///
    ///    1005 is a reserved value and MUST NOT be set as a status code in a
    ///    Close control frame by an endpoint. It is designated for use in
    ///    applications expecting a status code to indicate that no status
    ///    code was actually present.
    ///
    /// 1006
    ///
    ///    1006 is a reserved value and MUST NOT be set as a status code in a
    ///    Close control frame by an endpoint. It is designated for use in
    ///    applications expecting a status code to indicate that the
    ///    connection was closed abnormally, e.g., without sending or
    ///    receiving a Close control frame.
    ///
    /// 1007
    ///
    ///    1007 indicates that an endpoint is terminating the connection
    ///    because it has received data within a message that was not
    ///    consistent with the type of the message (e.g., non-UTF-8 [RFC3629]
    ///    data within a text message).
    ///
    /// 1008
    ///
    ///    1008 indicates that an endpoint is terminating the connection
    ///    because it has received a message that violates its policy. This
    ///    is a generic status code that can be returned when there is no
    ///    other more suitable status code (e.g., 1003 or 1009) or if there
    ///    is a need to hide specific details about the policy.
    ///
    /// 1009
    ///
    ///    1009 indicates that an endpoint is terminating the connection
    ///    because it has received a message that is too big for it to
    ///    process.
    ///
    /// 1010
    ///
    ///    1010 indicates that an endpoint (client) is terminating the
    ///    connection because it has expected the server to negotiate one or
    ///    more extension, but the server didn't return them in the response
    ///    message of the WebSocket handshake. The list of extensions that
    ///    are needed SHOULD appear in the /reason/ part of the Close frame.
    ///    Note that this status code is not used by the server, because it
    ///    can fail the WebSocket handshake instead.
    ///
    /// 1011
    ///
    ///    1011 indicates that a server is terminating the connection because
    ///    it encountered an unexpected condition that prevented it from
    ///    fulfilling the request.
    ///
    /// 1012 (IANA Registry, Non RFC-6455)
    ///
    ///    1012 indicates that the service is restarted. a client may reconnect,
    ///    and if it choses to do, should reconnect using a randomized delay
    ///    of 5 - 30 seconds.
    ///
    /// 1013 (IANA Registry, Non RFC-6455)
    ///
    ///    1013 indicates that the service is experiencing overload. a client
    ///    should only connect to a different IP (when there are multiple for the
    ///    target) or reconnect to the same IP upon user action.
    ///
    /// 1014 (IANA Registry, Non RFC-6455)
    ///
    ///    The server was acting as a gateway or proxy and received an invalid
    ///    response from the upstream server. This is similar to 502 HTTP Status Code.
    ///
    /// 1015
    ///
    ///    1015 is a reserved value and MUST NOT be set as a status code in a
    ///    Close control frame by an endpoint. It is designated for use in
    ///    applications expecting a status code to indicate that the
    ///    connection was closed due to a failure to perform a TLS handshake
    ///    (e.g., the server certificate can't be verified).
    ///
    ///
    /// 7.4.2. Reserved Status Code Ranges
    ///
    /// 0-999
    ///
    ///    Status codes in the range 0-999 are not used.
    ///
    /// 1000-2999
    ///
    ///    Status codes in the range 1000-2999 are reserved for definition by
    ///    this protocol, its future revisions, and extensions specified in a
    ///    permanent and readily available public specification.
    ///
    /// 3000-3999
    ///
    ///    Status codes in the range 3000-3999 are reserved for use by
    ///    libraries, frameworks, and applications. These status codes are
    ///    registered directly with IANA. The interpretation of these codes
    ///    is undefined by this protocol.
    ///
    /// 4000-4999
    ///
    ///    Status codes in the range 4000-4999 are reserved for private use
    ///    and thus can't be registered. Such codes can be used by prior
    ///    agreements between WebSocket applications. The interpretation of
    ///    these codes is undefined by this protocol.
    /// </para>
    /// <para>
    /// While <see cref="WebSocketCloseStatus"/> is enum-like structure, its instances should NOT be compared by reference.
    /// Instead, either <see cref="Equals(object)"/> should be used or direct comparison of <see cref="Code"/> value.
    /// </para>
    /// </summary>
    public sealed class WebSocketCloseStatus : IEquatable<WebSocketCloseStatus>, IComparable<WebSocketCloseStatus>, IComparable
    {
        public static readonly WebSocketCloseStatus NormalClosure =
            new WebSocketCloseStatus(1000, new StringCharSequence("Bye"));

        public static readonly WebSocketCloseStatus EndpointUnavailable =
            new WebSocketCloseStatus(1001, new StringCharSequence("Endpoint unavailable"));

        public static readonly WebSocketCloseStatus ProtocolError =
            new WebSocketCloseStatus(1002, new StringCharSequence("Protocol error"));

        public static readonly WebSocketCloseStatus InvalidMessageType =
            new WebSocketCloseStatus(1003, new StringCharSequence("Invalid message type"));

        public static readonly WebSocketCloseStatus InvalidPayloadData =
            new WebSocketCloseStatus(1007, new StringCharSequence("Invalid payload data"));

        public static readonly WebSocketCloseStatus PolicyViolation =
            new WebSocketCloseStatus(1008, new StringCharSequence("Policy violation"));

        public static readonly WebSocketCloseStatus MessageTooBig =
            new WebSocketCloseStatus(1009, new StringCharSequence("Message too big"));

        public static readonly WebSocketCloseStatus MandatoryExtension =
            new WebSocketCloseStatus(1010, new StringCharSequence("Mandatory extension"));

        public static readonly WebSocketCloseStatus InternalServerError =
            new WebSocketCloseStatus(1011, new StringCharSequence("Internal server error"));

        public static readonly WebSocketCloseStatus ServiceRestart =
            new WebSocketCloseStatus(1012, new StringCharSequence("Service Restart"));

        public static readonly WebSocketCloseStatus TryAgainLater =
            new WebSocketCloseStatus(1013, new StringCharSequence("Try Again Later"));

        public static readonly WebSocketCloseStatus BadGateway =
            new WebSocketCloseStatus(1014, new StringCharSequence("Bad Gateway"));

        // 1004, 1005, 1006, 1015 are reserved and should never be used by user
        //public static readonly WebSocketCloseStatus SPECIFIC_MEANING = register(1004, "...");
        //public static readonly WebSocketCloseStatus EMPTY = register(1005, "Empty");
        //public static readonly WebSocketCloseStatus ABNORMAL_CLOSURE = register(1006, "Abnormal closure");
        //public static readonly WebSocketCloseStatus TLS_HANDSHAKE_FAILED(1015, "TLS handshake failed");

        private readonly int _statusCode;
        private readonly ICharSequence _reasonText;
        private string _text;

        public WebSocketCloseStatus(int statusCode, ICharSequence reasonText)
        {
            if (!IsValidStatusCode(statusCode))
            {
                ThrowHelper.ThrowArgumentException_WebSocket_close_status_code_does_NOT_comply(statusCode);
            }
            _statusCode = statusCode;
            _reasonText = reasonText;
        }

        public int Code => _statusCode;

        public ICharSequence ReasonText => _reasonText;

        public static WebSocketCloseStatus ValueOf(int code)
        {
            return code switch
            {
                1000 => NormalClosure,
                1001 => EndpointUnavailable,
                1002 => ProtocolError,
                1003 => InvalidMessageType,
                1007 => InvalidPayloadData,
                1008 => PolicyViolation,
                1009 => MessageTooBig,
                1010 => MandatoryExtension,
                1011 => InternalServerError,
                1012 => ServiceRestart,
                1013 => TryAgainLater,
                1014 => BadGateway,
                _ => Create(code),
            };
        }

        private static WebSocketCloseStatus Create(int code)
        {
            return new WebSocketCloseStatus(code, new StringCharSequence("Close status #" + code));
        }

        public static bool IsValidStatusCode(int code)
        {
            uint ucode = (uint)code;
            return ucode > SharedConstants.TooBigOrNegative ||
                IsInRangeInclusive(ucode, 1000u, 1003u) ||
                IsInRangeInclusive(ucode, 1007u, 1014u) ||
                3000u <= code;
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private static bool IsInRangeInclusive(uint value, uint lowerBound, uint upperBound)
            => ((value - lowerBound) <= (upperBound - lowerBound));

        public int CompareTo(object obj)
        {
            return CompareTo(obj as WebSocketCloseStatus);
        }

        public int CompareTo(WebSocketCloseStatus other)
        {
            if (ReferenceEquals(this, other)) { return 0; }
            if (other is null) { return 1; }
            return _statusCode - other._statusCode;
        }

        public bool Equals(WebSocketCloseStatus other)
        {
            if (ReferenceEquals(this, other)) { return true; }
            if (other is null) { return false; }

            return _statusCode == other._statusCode;
        }

        public override bool Equals(object obj)
        {
            return obj is WebSocketCloseStatus other && Equals(other);
        }

        public override int GetHashCode() => _statusCode;

        public override string ToString()
        {
            // E.g.: "1000 Bye", "1009 Message too big"
            return _text ??= $"{_statusCode} {_reasonText}";
        }
    }
}
