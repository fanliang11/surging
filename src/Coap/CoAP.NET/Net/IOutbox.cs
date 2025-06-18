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

using CoAP.Net;

namespace CoAP
{
    public interface IOutbox
    {
        /// <summary>
        /// Sends the specified request over the connector that the stack is connected to.
        /// </summary>
        void SendRequest(Exchange exchange, Request request);
        /// <summary>
        /// Sends the specified response over the connector that the stack is connected to.
        /// </summary>
        void SendResponse(Exchange exchange, Response response);
        /// <summary>
        /// Sends the specified empty message over the connector that the stack is connected to.
        /// </summary>
        void SendEmptyMessage(Exchange exchange, EmptyMessage message);
    }
}
