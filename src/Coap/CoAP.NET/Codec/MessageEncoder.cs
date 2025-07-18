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
    /// Base class for message encoders.
    /// </summary>
    public abstract class MessageEncoder : IMessageEncoder
    {
        /// <inheritdoc/>
        public Byte[] Encode(Request request)
        {
            DatagramWriter writer = new DatagramWriter();
            Serialize(writer, request, request.Code);
            return writer.ToByteArray();
        }

        /// <inheritdoc/>
        public Byte[] Encode(Response response)
        {
            DatagramWriter writer = new DatagramWriter();
            Serialize(writer, response, response.Code);
            return writer.ToByteArray();
        }

        /// <inheritdoc/>
        public Byte[] Encode(EmptyMessage message)
        {
            DatagramWriter writer = new DatagramWriter();
            Serialize(writer, message, Code.Empty);
            return writer.ToByteArray();
        }

        /// <inheritdoc/>
        public Byte[] Encode(Message message)
        {
            if (message.IsRequest)
                return Encode((Request)message);
            else if (message.IsResponse)
                return Encode((Response)message);
            else if (message is EmptyMessage)
                return Encode((EmptyMessage)message);
            else
                return null;
        }

        /// <summary>
        /// Serializes a message.
        /// </summary>
        /// <param name="writer">the writer</param>
        /// <param name="message">the message to write</param>
        /// <param name="code">the code</param>
        protected abstract void Serialize(DatagramWriter writer, Message message, Int32 code);
    }
}
