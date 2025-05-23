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
    /// Provides methods to serialize outgoing messages to byte arrays.
    /// </summary>
    public interface IMessageEncoder
    {
        /// <summary>
        /// Encodes a request into a bytes array.
        /// </summary>
        /// <param name="request">the request to encode</param>
        /// <returns>the encoded bytes</returns>
        Byte[] Encode(Request request);
        /// <summary>
        /// Encodes a response into a bytes array.
        /// </summary>
        /// <param name="response">the response to encode</param>
        /// <returns>the encoded bytes</returns>
        Byte[] Encode(Response response);
        /// <summary>
        /// Encodes an empty message into a bytes array.
        /// </summary>
        /// <param name="message">the empty message to encode</param>
        /// <returns>the encoded bytes</returns>
        Byte[] Encode(EmptyMessage message);
        /// <summary>
        /// Encodes a CoAP message into a bytes array.
        /// </summary>
        /// <param name="message">the message to encode</param>
        /// <returns>
        /// the encoded bytes, or null if the message can not be encoded,
        /// i.e. the message is not a <see cref="Request"/>, a <see cref="Response"/> or an <see cref="EmptyMessage"/>.
        /// </returns>
        Byte[] Encode(Message message);
    }
}
