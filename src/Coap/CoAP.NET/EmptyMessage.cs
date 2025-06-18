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

namespace CoAP
{
    /// <summary>
    /// Represents an empty CoAP message. An empty message has either
    /// the <see cref="MessageType"/> ACK or RST.
    /// </summary>
    public class EmptyMessage : Message
    {
        /// <summary>
        /// Instantiates a new empty message.
        /// </summary>
        public EmptyMessage(MessageType type)
            : base(type, CoAP.Code.Empty)
        { }

        /// <summary>
        /// Create a new acknowledgment for the specified message.
        /// </summary>
        /// <param name="message">the message to acknowledge</param>
        /// <returns>the acknowledgment</returns>
        public static EmptyMessage NewACK(Message message)
        {
            EmptyMessage ack = new EmptyMessage(MessageType.ACK);
            ack.ID = message.ID;
            ack.Token = CoapConstants.EmptyToken;
            ack.Destination = message.Source;
            return ack;
        }

        /// <summary>
        /// Create a new reset message for the specified message.
        /// </summary>
        /// <param name="message">the message to reject</param>
        /// <returns>the reset</returns>
        public static EmptyMessage NewRST(Message message)
        {
            EmptyMessage rst = new EmptyMessage(MessageType.RST);
            rst.ID = message.ID;
            rst.Token = CoapConstants.EmptyToken;
            rst.Destination = message.Source;
            return rst;
        }
    }
}
