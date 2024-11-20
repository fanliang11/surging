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

namespace CoAP.Stack
{
    /// <summary>
    /// Stack of layers.
    /// </summary>
    public class LayerStack : Chain<LayerStack, ILayer, INextLayer>
    {
        /// <summary>
        /// Instantiates.
        /// </summary>
        public LayerStack()
            : base(
            e => new NextLayer(e),
            () => new StackTopLayer(), () => new StackBottomLayer()
            )
        { }

        /// <summary>
        /// Sends a request into the layer stack.
        /// </summary>
        /// <param name="request">the request to send</param>
        public void SendRequest(Request request)
        {
            Head.Filter.SendRequest(Head.NextFilter, null, request);
        }

        /// <summary>
        /// Sends a response into the layer stack.
        /// </summary>
        /// <param name="exchange">the exchange associated</param>
        /// <param name="response">the response to send</param>
        public void SendResponse(Exchange exchange, Response response)
        {
            Head.Filter.SendResponse(Head.NextFilter, exchange, response);
        }

        /// <summary>
        /// Sends an empty message into the layer stack.
        /// </summary>
        /// <param name="exchange">the exchange associated</param>
        /// <param name="message">the message to send</param>
        public void SendEmptyMessage(Exchange exchange, EmptyMessage message)
        {
            Head.Filter.SendEmptyMessage(Head.NextFilter, exchange, message);
        }

        /// <summary>
        /// Receives a request into the layer stack.
        /// </summary>
        /// <param name="exchange">the exchange associated</param>
        /// <param name="request">the response to receive</param>
        public void ReceiveRequest(Exchange exchange, Request request)
        {
            Tail.Filter.ReceiveRequest(Tail.NextFilter, exchange, request);
        }

        /// <summary>
        /// Receives a response into the layer stack.
        /// </summary>
        /// <param name="exchange">the exchange associated</param>
        /// <param name="response">the response to receive</param>
        public void ReceiveResponse(Exchange exchange, Response response)
        {
            Tail.Filter.ReceiveResponse(Tail.NextFilter, exchange, response);
        }

        /// <summary>
        /// Receives an empty message into the layer stack.
        /// </summary>
        /// <param name="exchange">the exchange associated</param>
        /// <param name="message">the message to receive</param>
        public void ReceiveEmptyMessage(Exchange exchange, EmptyMessage message)
        {
            Tail.Filter.ReceiveEmptyMessage(Tail.NextFilter, exchange, message);
        }

        class StackTopLayer : AbstractLayer
        {
            public override void SendRequest(INextLayer nextLayer, Exchange exchange, Request request)
            {
                if (exchange == null)
                {
                    exchange = new Exchange(request, Origin.Local);
                    exchange.EndPoint = request.EndPoint;
                }
                
                exchange.Request = request;
                base.SendRequest(nextLayer, exchange, request);
            }

            public override void SendResponse(INextLayer nextLayer, Exchange exchange, Response response)
            {
                exchange.Response = response;
                base.SendResponse(nextLayer, exchange, response);
            }

            public override void ReceiveRequest(INextLayer nextLayer, Exchange exchange, Request request)
            {
                // if there is no BlockwiseLayer we still have to set it
                if (exchange.Request == null)
                    exchange.Request = request;
                if (exchange.Deliverer != null)
                    exchange.Deliverer.DeliverRequest(exchange);
            }

            public override void ReceiveResponse(INextLayer nextLayer, Exchange exchange, Response response)
            {
                if (!response.HasOption(OptionType.Observe))
                    exchange.Complete = true;
                if (exchange.Deliverer != null)
                    // notify request that response has arrived
                    exchange.Deliverer.DeliverResponse(exchange, response);
            }

            public override void ReceiveEmptyMessage(INextLayer nextLayer, Exchange exchange, EmptyMessage message)
            {
                // When empty messages reach the top of the CoAP stack we can ignore them. 
            }
        }

        class StackBottomLayer : AbstractLayer
        {
            public override void SendRequest(INextLayer nextLayer, Exchange exchange, Request request)
            {
                exchange.Outbox.SendRequest(exchange, request);
            }

            public override void SendResponse(INextLayer nextLayer, Exchange exchange, Response response)
            {
                exchange.Outbox.SendResponse(exchange, response);
            }

            public override void SendEmptyMessage(INextLayer nextLayer, Exchange exchange, EmptyMessage message)
            {
                exchange.Outbox.SendEmptyMessage(exchange, message);
            }
        }

        class NextLayer : INextLayer
        {
            readonly Entry _entry;

            public NextLayer(Entry entry)
            {
                _entry = entry;
            }

            public void SendRequest(Exchange exchange, Request request)
            {
                _entry.NextEntry.Filter.SendRequest(_entry.NextEntry.NextFilter, exchange, request);
            }

            public void SendResponse(Exchange exchange, Response response)
            {
                _entry.NextEntry.Filter.SendResponse(_entry.NextEntry.NextFilter, exchange, response);
            }

            public void SendEmptyMessage(Exchange exchange, EmptyMessage message)
            {
                _entry.NextEntry.Filter.SendEmptyMessage(_entry.NextEntry.NextFilter, exchange, message);
            }

            public void ReceiveRequest(Exchange exchange, Request request)
            {
                _entry.PrevEntry.Filter.ReceiveRequest(_entry.PrevEntry.NextFilter, exchange, request);
            }

            public void ReceiveResponse(Exchange exchange, Response response)
            {
                _entry.PrevEntry.Filter.ReceiveResponse(_entry.PrevEntry.NextFilter, exchange, response);
            }

            public void ReceiveEmptyMessage(Exchange exchange, EmptyMessage message)
            {
                _entry.PrevEntry.Filter.ReceiveEmptyMessage(_entry.PrevEntry.NextFilter, exchange, message);
            }
        }
    }
}
