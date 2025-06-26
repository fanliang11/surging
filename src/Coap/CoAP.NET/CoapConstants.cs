/*
 * Copyright (c) 2011-2012, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;

namespace CoAP
{
    /// <summary>
    /// Constants defined for CoAP protocol
    /// </summary>
    public static class CoapConstants
    {
        /// <summary>
        /// RFC 7252 CoAP version.
        /// </summary>
        public const Int32 Version = 0x01;
        /// <summary>
        /// The CoAP URI scheme.
        /// </summary>
        public const String UriScheme = "coap";
        /// <summary>
        /// The CoAPS URI scheme.
        /// </summary>
        public const String SecureUriScheme = "coaps";
        /// <summary>
        /// The default CoAP port for normal CoAP communication (not secure).
        /// </summary>
        public const Int32 DefaultPort = 5683;
        /// <summary>
        /// The default CoAP port for secure CoAP communication (coaps).
        /// </summary>
        public const Int32 DefaultSecurePort = 5684;
        /// <summary>
        /// The initial time (ms) for a CoAP message
        /// </summary>
        public const Int32 AckTimeout = 2000;
        /// <summary>
        /// The initial timeout is set
        /// to a random number between RESPONSE_TIMEOUT and (RESPONSE_TIMEOUT *
        /// RESPONSE_RANDOM_FACTOR)
        /// </summary>
        public const Double AckRandomFactor = 1.5D;
        /// <summary>
        /// The max time that a message would be retransmitted
        /// </summary>
        public const Int32 MaxRetransmit = 4;
        /// <summary>
        /// Default block size used for block-wise transfers
        /// </summary>
        public const Int32 DefaultBlockSize = 512;
        public const Int32 MessageCacheSize = 32;
        public const Int32 ReceiveBufferSize = 4096;
        public const Int32 DefaultOverallTimeout = 100000;
        /// <summary>
        /// Default URI for wellknown resource
        /// </summary>
        public const String DefaultWellKnownURI = "/.well-known/core";
        public const Int32 TokenLength = 8;
        public const Int32 DefaultMaxAge = 60;
        /// <summary>
        /// The number of notifications until a CON notification will be used.
        /// </summary>
        public const Int32 ObservingRefreshInterval = 10;

        public static readonly Byte[] EmptyToken = new Byte[0];

        /// <summary>
        /// The lowest value of a request code.
        /// </summary>
        public const Int32 RequestCodeLowerBound = 1;

        /// <summary>
        /// The highest value of a request code.
        /// </summary>
        public const Int32 RequestCodeUpperBound = 31;

        /// <summary>
        /// The lowest value of a response code.
        /// </summary>
        public const Int32 ResponseCodeLowerBound = 64;

        /// <summary>
        /// The highest value of a response code.
        /// </summary>
        public const Int32 ResponseCodeUpperBound = 191;
    }
}
