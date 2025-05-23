/*
 * Copyright (c) 2011-2015, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;
using CoAP.Stack;

namespace CoAP.Net
{
    /// <summary>
    /// Represents a communication endpoint multiplexing CoAP message exchanges
    /// between (potentially multiple) clients and servers.
    /// </summary>
    public interface IEndPoint : IDisposable
    {
        /// <summary>
        /// Gets this endpoint's configuration.
        /// </summary>
        ICoapConfig Config { get; }
        /// <summary>
        /// Gets the local <see cref="System.Net.EndPoint"/> this endpoint is associated with.
        /// </summary>
        System.Net.EndPoint LocalEndPoint { get; }
        /// <summary>
        /// Checks if the endpoint has started.
        /// </summary>
        Boolean Running { get; }
        /// <summary>
        /// Gets or sets the message deliverer.
        /// </summary>
        IMessageDeliverer MessageDeliverer { get; set; }
        /// <summary>
        /// Gets the outbox.
        /// </summary>
        IOutbox Outbox { get; }
        /// <summary>
        /// Occurs when a request is about to be sent.
        /// </summary>
        event EventHandler<MessageEventArgs<Request>> SendingRequest;
        /// <summary>
        /// Occurs when a response is about to be sent.
        /// </summary>
        event EventHandler<MessageEventArgs<Response>> SendingResponse;
        /// <summary>
        /// Occurs when a an empty message is about to be sent.
        /// </summary>
        event EventHandler<MessageEventArgs<EmptyMessage>> SendingEmptyMessage;
        /// <summary>
        /// Occurs when a request request has been received.
        /// </summary>
        event EventHandler<MessageEventArgs<Request>> ReceivingRequest;
        /// <summary>
        /// Occurs when a response has been received.
        /// </summary>
        event EventHandler<MessageEventArgs<Response>> ReceivingResponse;
        /// <summary>
        /// Occurs when an empty message has been received.
        /// </summary>
        event EventHandler<MessageEventArgs<EmptyMessage>> ReceivingEmptyMessage;
        /// <summary>
        /// Starts this endpoint and all its components.
        /// </summary>
        void Start();
        /// <summary>
        /// Stops this endpoint and all its components
        /// </summary>
        void Stop();
        void Clear();
        /// <summary>
        /// Sends the specified request.
        /// </summary>
        /// <param name="request"></param>
        void SendRequest(Request request);
        /// <summary>
        /// Sends the specified response.
        /// </summary>
        void SendResponse(Exchange exchange, Response response);
        /// <summary>
        /// Sends the specified empty message.
        /// </summary>
        void SendEmptyMessage(Exchange exchange, EmptyMessage message);
    }
}
