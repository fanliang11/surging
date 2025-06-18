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
using CoAP.Util;

namespace CoAP.Net
{
    public class ClientMessageDeliverer : IMessageDeliverer
    {
        /// <inheritdoc/>
        public void DeliverRequest(Exchange exchange)
        {
            exchange.SendReject();
        }

        /// <inheritdoc/>
        public void DeliverResponse(Exchange exchange, Response response)
        {
            if (exchange == null)
                throw ThrowHelper.ArgumentNull("exchange");
            if (response == null)
                throw ThrowHelper.ArgumentNull("response");
            if (exchange.Request == null)
                throw new ArgumentException("Request should not be empty.", "exchange");
            exchange.Request.Response = response;
        }
    }
}
