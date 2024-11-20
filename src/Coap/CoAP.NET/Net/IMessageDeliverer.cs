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

namespace CoAP.Net
{
    /// <summary>
    /// Provides methods for delivering inbound CoAP messages to an appropriate processor.
    /// </summary>
    public interface IMessageDeliverer
    {
        /// <summary>
        /// Delivers an inbound CoAP request to an appropriate resource.
        /// </summary>
        /// <param name="exchange"> the exchange containing the inbound <see cref="Request"/></param>
        void DeliverRequest(Exchange exchange);
        /// <summary>
        /// Delivers an inbound CoAP response message to its corresponding request.
        /// </summary>
        /// <param name="exchange">the exchange containing the originating CoAP request</param>
        /// <param name="response">the inbound CoAP response message</param>
        void DeliverResponse(Exchange exchange, Response response);
    }
}
