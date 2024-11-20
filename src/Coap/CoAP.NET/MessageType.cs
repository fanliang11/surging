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

namespace CoAP
{
    /// <summary>
    /// Types of CoAP messages.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Unknown type.
        /// </summary>
        Unknown = -1,
        /// <summary>
        /// Confirmable messages require an acknowledgement.
        /// </summary>
        CON = 0,
        /// <summary>
        /// Non-Confirmable messages do not require an acknowledgement.
        /// </summary>
        NON,
        /// <summary>
        /// Acknowledgement messages acknowledge a specific confirmable message.
        /// </summary>
        ACK,
        /// <summary>
        /// Reset messages indicate that a specific confirmable message was received, but some context is missing to properly process it.
        /// </summary>
        RST
    }
}
