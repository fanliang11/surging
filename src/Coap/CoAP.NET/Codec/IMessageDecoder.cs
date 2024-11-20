/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;

namespace CoAP.Codec
{
    /// <summary>
    /// Provides methods to parse incoming byte arrays to messages.
    /// </summary>
    public interface IMessageDecoder
    {
        /// <summary>
        /// Checks if the decoding message is wellformed.
        /// </summary>
        Boolean IsWellFormed { get; }
        /// <summary>
        /// Checks if the decoding message is a reply.
        /// </summary>
        Boolean IsReply { get; }
        /// <summary>
        /// Checks if the decoding message is a request.
        /// </summary>
        Boolean IsRequest { get; }
        /// <summary>
        /// Checks if the decoding message is a response.
        /// </summary>
        Boolean IsResponse { get; }
        /// <summary>
        /// Checks if the decoding message is an empty message.
        /// </summary>
        Boolean IsEmpty { get; }
        /// <summary>
        /// Gets the version of the decoding message.
        /// </summary>
        Int32 Version { get; }
        /// <summary>
        /// Gets the id of the decoding message.
        /// </summary>
        Int32 ID { get; }
        /// <summary>
        /// Decodes as a <see cref="Request"/>.
        /// </summary>
        /// <returns>the decoded request</returns>
        Request DecodeRequest();
        /// <summary>
        /// Decodes as a <see cref="Response"/>.
        /// </summary>
        /// <returns>the decoded response</returns>
        Response DecodeResponse();
        /// <summary>
        /// Decodes as a <see cref="EmptyMessage"/>.
        /// </summary>
        /// <returns>the decoded empty message</returns>
        EmptyMessage DecodeEmptyMessage();
        /// <summary>
        /// Decodes as a CoAP message.
        /// </summary>
        /// <returns>the decoded message, or null if not be recognized.</returns>
        Message Decode();
    }
}
