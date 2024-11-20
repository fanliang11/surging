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
using CoAP.Threading;

namespace CoAP.Stack
{
    /// <summary>
    /// A partial implementation of a layer.
    /// </summary>
    public class AbstractLayer : ILayer
    {
        private IExecutor _executor;

        /// <inheritdoc/>
        public IExecutor Executor
        {
            get { return _executor; }
            set { _executor = value; }
        }

        /// <inheritdoc/>
        public virtual void SendRequest(INextLayer nextLayer, Exchange exchange, Request request)
        {
            nextLayer.SendRequest(exchange, request);
        }

        /// <inheritdoc/>
        public virtual void SendResponse(INextLayer nextLayer, Exchange exchange, Response response)
        {
            nextLayer.SendResponse(exchange, response);
        }

        /// <inheritdoc/>
        public virtual void SendEmptyMessage(INextLayer nextLayer, Exchange exchange, EmptyMessage message)
        {
            nextLayer.SendEmptyMessage(exchange, message);
        }

        /// <inheritdoc/>
        public virtual void ReceiveRequest(INextLayer nextLayer, Exchange exchange, Request request)
        {
            nextLayer.ReceiveRequest(exchange, request);
        }

        /// <inheritdoc/>
        public virtual void ReceiveResponse(INextLayer nextLayer, Exchange exchange, Response response)
        {
            nextLayer.ReceiveResponse(exchange, response);
        }

        /// <inheritdoc/>
        public virtual void ReceiveEmptyMessage(INextLayer nextLayer, Exchange exchange, EmptyMessage message)
        {
            nextLayer.ReceiveEmptyMessage(exchange, message);
        }
    }
}
