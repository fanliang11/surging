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
using CoAP.Net;

namespace CoAP.Stack
{
    /// <summary>
    /// Doesn't do much yet except for setting a simple token. Notice that empty
    /// tokens must be represented as byte array of length 0 (not null).
    /// </summary>
    public class TokenLayer : AbstractLayer
    {
        private Int32 _counter;

        /// <summary>
        /// Constructs a new token layer.
        /// </summary>
        public TokenLayer(ICoapConfig config)
        {
            if (config.UseRandomTokenStart)
                _counter = new Random().Next();
        }

        /// <inheritdoc/>
        public override void SendRequest(INextLayer nextLayer, Exchange exchange, Request request)
        {
            if (request.Token == null)
                request.Token = NewToken();
            base.SendRequest(nextLayer, exchange, request);
        }

        /// <inheritdoc/>
        public override void SendResponse(INextLayer nextLayer, Exchange exchange, Response response)
        {
            // A response must have the same token as the request it belongs to. If
            // the token is empty, we must use a byte array of length 0.
            if (response.Token == null)
                response.Token = exchange.CurrentRequest.Token;
            base.SendResponse(nextLayer, exchange, response);
        }

        /// <inheritdoc/>
        public override void ReceiveRequest(INextLayer nextLayer, Exchange exchange, Request request)
        {
            if (exchange.CurrentRequest.Token == null)
                throw new InvalidOperationException("Received requests's token cannot be null, use byte[0] for empty tokens");
            base.ReceiveRequest(nextLayer, exchange, request);
        }

        /// <inheritdoc/>
        public override void ReceiveResponse(INextLayer nextLayer, Exchange exchange, Response response)
        {
            if (response.Token == null)
                throw new InvalidOperationException("Received response's token cannot be null, use byte[0] for empty tokens");
            base.ReceiveResponse(nextLayer, exchange, response);
        }

        private Byte[] NewToken()
        {
            UInt32 token = (UInt32)System.Threading.Interlocked.Increment(ref _counter);
            return new Byte[]
            { 
                (Byte)(token >> 24), (Byte)(token >> 16),
                (Byte)(token >> 8), (Byte)token
            };
        }
    }
}
