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
    /// Represents a layer in the stack.
    /// </summary>
    public interface ILayer
    {
        /// <summary>
        /// Gets or set the executor to schedule tasks.
        /// </summary>
        IExecutor Executor { get; set; }
        /// <summary>
        /// Filters a request sending event.
        /// </summary>
        /// <param name="nextLayer">the next layer</param>
        /// <param name="exchange">the exchange associated</param>
        /// <param name="request">the request to send</param>
        void SendRequest(INextLayer nextLayer, Exchange exchange, Request request);
        /// <summary>
        /// Filters a response sending event.
        /// </summary>
        /// <param name="nextLayer">the next layer</param>
        /// <param name="exchange">the exchange associated</param>
        /// <param name="response">the response to send</param>
        void SendResponse(INextLayer nextLayer, Exchange exchange, Response response);
        /// <summary>
        /// Filters an empty message sending event.
        /// </summary>
        /// <param name="nextLayer">the next layer</param>
        /// <param name="exchange">the exchange associated</param>
        /// <param name="message">the empty message to send</param>
        void SendEmptyMessage(INextLayer nextLayer, Exchange exchange, EmptyMessage message);
        /// <summary>
        /// Filters a request receiving event.
        /// </summary>
        /// <param name="nextLayer">the next layer</param>
        /// <param name="exchange">the exchange associated</param>
        /// <param name="request">the request to receive</param>
        void ReceiveRequest(INextLayer nextLayer, Exchange exchange, Request request);
        /// <summary>
        /// Filters a response receiving event.
        /// </summary>
        /// <param name="nextLayer">the next layer</param>
        /// <param name="exchange">the exchange associated</param>
        /// <param name="response">the response to receive</param>
        void ReceiveResponse(INextLayer nextLayer, Exchange exchange, Response response);
        /// <summary>
        /// Filters an empty message receiving event.
        /// </summary>
        /// <param name="nextLayer">the next layer</param>
        /// <param name="exchange">the exchange associated</param>
        /// <param name="message">the empty message to receive</param>
        void ReceiveEmptyMessage(INextLayer nextLayer, Exchange exchange, EmptyMessage message);
    }

    /// <summary>
    /// Represent a next layer in the stack.
    /// </summary>
    public interface INextLayer
    {
        /// <summary>
        /// Sends a request to next layer.
        /// </summary>
        void SendRequest(Exchange exchange, Request request);
        /// <summary>
        /// Sends a response to next layer.
        /// </summary>
        void SendResponse(Exchange exchange, Response response);
        /// <summary>
        /// Sends an empty message to next layer.
        /// </summary>
        void SendEmptyMessage(Exchange exchange, EmptyMessage message);
        /// <summary>
        /// Receives a request to next layer.
        /// </summary>
        void ReceiveRequest(Exchange exchange, Request request);
        /// <summary>
        /// Receives a response to next layer.
        /// </summary>
        void ReceiveResponse(Exchange exchange, Response response);
        /// <summary>
        /// Receives an empty message to next layer.
        /// </summary>
        void ReceiveEmptyMessage(Exchange exchange, EmptyMessage message);
    }
}
