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
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;

namespace CoAP.Stack
{
    /// <summary>
    /// The reliability layer
    /// </summary>
    public class ReliabilityLayer : AbstractLayer
    {
        private  readonly ILogger log;
        static readonly Object TransmissionContextKey = "TransmissionContext";

        private readonly Random _rand = new Random();
        private ICoapConfig _config;

        /// <summary>
        /// Constructs a new reliability layer.
        /// </summary>
        public ReliabilityLayer(ICoapConfig config)
        {
            log = ServiceLocator.GetService<ILogger<ReliabilityLayer>>();
            _config = config;
        }

        /// <summary>
        /// // Schedules a retransmission for confirmable messages.
        /// </summary>
        public override void SendRequest(INextLayer nextLayer, Exchange exchange, Request request)
        {
            if (request.Type == MessageType.Unknown)
                request.Type = MessageType.CON;

            if (request.Type == MessageType.CON)
            {
               if (log.IsEnabled(LogLevel.Debug))
                     log.LogDebug("Scheduling retransmission for " + request);
                PrepareRetransmission(exchange, request, ctx => SendRequest(nextLayer, exchange, request));
            }

            base.SendRequest(nextLayer, exchange, request);
        }

        /// <summary>
        /// Makes sure that the response type is correct. The response type for a NON
	    /// can be NON or CON. The response type for a CON should either be an ACK
	    /// with a piggy-backed response or, if an empty ACK has already be sent, a
        /// CON or NON with a separate response.
        /// </summary>
        public override void SendResponse(INextLayer nextLayer, Exchange exchange, Response response)
        {
            MessageType mt = response.Type;
            if (mt == MessageType.Unknown)
            {
                MessageType reqType = exchange.CurrentRequest.Type;
                if (reqType == MessageType.CON)
                {
                    if (exchange.CurrentRequest.IsAcknowledged)
                    {
                        // send separate response
                        response.Type = MessageType.CON;
                    }
                    else
                    {
                        exchange.CurrentRequest.IsAcknowledged = true;
                        // send piggy-backed response
                        response.Type = MessageType.ACK;
                        response.ID = exchange.CurrentRequest.ID;
                    }
                }
                else
                {
                    // send NON response
                    response.Type = MessageType.NON;
                }
            }
            else if (mt == MessageType.ACK || mt == MessageType.RST)
            {
                response.ID = exchange.CurrentRequest.ID;
            }

            if (response.Type == MessageType.CON)
            {
               if (log.IsEnabled(LogLevel.Debug))
                     log.LogDebug("Scheduling retransmission for " + response);
                PrepareRetransmission(exchange, response, ctx => SendResponse(nextLayer, exchange, response));
            }

            base.SendResponse(nextLayer, exchange, response);
        }

        /// <summary>
        /// When we receive a duplicate of a request, we stop it here and do not
	    /// forward it to the upper layer. If the server has already sent a response,
	    /// we send it again. If the request has only been acknowledged (but the ACK
	    /// has gone lost or not reached the client yet), we resent the ACK. If the
        /// request has neither been responded, acknowledged or rejected yet, the
	    /// server has not yet decided what to do with the request and we cannot do
	    /// anything.
        /// </summary>
        public override void ReceiveRequest(INextLayer nextLayer, Exchange exchange, Request request)
        {
            if (request.Duplicate)
            {
                // Request is a duplicate, so resend ACK, RST or response
                if (exchange.CurrentResponse != null)
                {
                   if (log.IsEnabled(LogLevel.Debug))
                         log.LogDebug("Respond with the current response to the duplicate request");
                    base.SendResponse(nextLayer, exchange, exchange.CurrentResponse);
                }
                else if (exchange.CurrentRequest != null)
                {
                    if (exchange.CurrentRequest.IsAcknowledged)
                    {
                       if (log.IsEnabled(LogLevel.Debug))
                             log.LogDebug("The duplicate request was acknowledged but no response computed yet. Retransmit ACK.");
                        EmptyMessage ack = EmptyMessage.NewACK(request);
                        SendEmptyMessage(nextLayer, exchange, ack);
                    }
                    else if (exchange.CurrentRequest.IsRejected)
                    {
                       if (log.IsEnabled(LogLevel.Debug))
                             log.LogDebug("The duplicate request was rejected. Reject again.");
                        EmptyMessage rst = EmptyMessage.NewRST(request);
                        SendEmptyMessage(nextLayer, exchange, rst);
                    }
                    else
                    {
                       if (log.IsEnabled(LogLevel.Debug))
                             log.LogDebug("The server has not yet decided what to do with the request. We ignore the duplicate.");
                        // The server has not yet decided, whether to acknowledge or
                        // reject the request. We know for sure that the server has
                        // received the request though and can drop this duplicate here.
                    }
                }
                else
                {
                    // Lost the current request. The server has not yet decided what to do.
                }
            }
            else
            {
                // Request is not a duplicate
                exchange.CurrentRequest = request;
                base.ReceiveRequest(nextLayer, exchange, request);
            }
        }

        /// <summary>
        /// When we receive a Confirmable response, we acknowledge it and it also
	    /// counts as acknowledgment for the request. If the response is a duplicate,
        /// we stop it here and do not forward it to the upper layer.
        /// </summary>
        public override void ReceiveResponse(INextLayer nextLayer, Exchange exchange, Response response)
        {
            TransmissionContext ctx = (TransmissionContext)exchange.Remove(TransmissionContextKey);
            if (ctx != null)
            {
                exchange.CurrentRequest.IsAcknowledged = true;
                ctx.Cancel();
            }

            if (response.Type == MessageType.CON && !exchange.Request.IsCancelled)
            {
               if (log.IsEnabled(LogLevel.Debug))
                     log.LogDebug("Response is confirmable, send ACK.");
                EmptyMessage ack = EmptyMessage.NewACK(response);
                SendEmptyMessage(nextLayer, exchange, ack);
            }

            if (response.Duplicate)
            {
               if (log.IsEnabled(LogLevel.Debug))
                     log.LogDebug("Response is duplicate, ignore it.");
            }
            else
            {
                base.ReceiveResponse(nextLayer, exchange, response);
            }
        }

        /// <summary>
        /// If we receive an ACK or RST, we mark the outgoing request or response
        /// as acknowledged or rejected respectively and cancel its retransmission.
        /// </summary>
        public override void ReceiveEmptyMessage(INextLayer nextLayer, Exchange exchange, EmptyMessage message)
        {
            switch (message.Type)
            {
                case MessageType.ACK:
                    if (exchange.Origin == Origin.Local)
                        exchange.CurrentRequest.IsAcknowledged = true;
                    else
                        exchange.CurrentResponse.IsAcknowledged = true;
                    break;
                case MessageType.RST:
                    if (exchange.Origin == Origin.Local)
                        exchange.CurrentRequest.IsRejected = true;
                    else
                        exchange.CurrentResponse.IsRejected = true;
                    break;
                default:
                    if (log.IsEnabled(LogLevel.Warning))
                        log.LogWarning("Empty messgae was not ACK nor RST: " + message);
                    break;
            }

            TransmissionContext ctx = (TransmissionContext)exchange.Remove(TransmissionContextKey);
            if (ctx != null)
                ctx.Cancel();

            base.ReceiveEmptyMessage(nextLayer, exchange, message);
        }

        private void PrepareRetransmission(Exchange exchange, Message msg, Action<TransmissionContext> retransmit)
        {
            TransmissionContext ctx = exchange.GetOrAdd<TransmissionContext>(
                TransmissionContextKey, _ => new TransmissionContext(_config, exchange, msg, retransmit));
            
            if (ctx.FailedTransmissionCount > 0)
            {
                ctx.CurrentTimeout = (Int32)(ctx.CurrentTimeout * _config.AckTimeoutScale);
            }
            else if (ctx.CurrentTimeout == 0)
            {
                ctx.CurrentTimeout = InitialTimeout(_config.AckTimeout, _config.AckRandomFactor);
            }

           if (log.IsEnabled(LogLevel.Debug))
                 log.LogDebug("Send request, failed transmissions: " + ctx.FailedTransmissionCount);

            ctx.Start();
        }

        private Int32 InitialTimeout(Int32 initialTimeout, Double factor)
        {
            return (Int32)(initialTimeout + initialTimeout * (factor - 1D) * _rand.NextDouble());
        }

        class TransmissionContext : IDisposable
        {
            readonly ICoapConfig _config;
            readonly Exchange _exchange;
            readonly Message _message;
            private Int32 _currentTimeout;
            private Int32 _failedTransmissionCount;
            private System.Timers.Timer _timer;
            private Action<TransmissionContext> _retransmit;

            public TransmissionContext(ICoapConfig config, Exchange exchange, Message message, Action<TransmissionContext> retransmit)
            {
                _config = config;
                _exchange = exchange;
                _message = message;
                _retransmit = retransmit;
                _currentTimeout = message.AckTimeout;
                _timer = new System.Timers.Timer();
                _timer.AutoReset = false;
                _timer.Elapsed += timer_Elapsed;
            }

            public Int32 FailedTransmissionCount
            {
                get { return _failedTransmissionCount; }
                set { _failedTransmissionCount = value; }
            }

            public Int32 CurrentTimeout
            {
                get { return _currentTimeout; }
                set { _currentTimeout = value; }
            }

            public void Start()
            {
                _timer.Stop();

                if (_currentTimeout > 0)
                {
                    _timer.Interval = _currentTimeout;
                    _timer.Start();
                }
            }

            public void Cancel()
            {
                System.Timers.Timer t = System.Threading.Interlocked.Exchange(ref _timer, null);

                // avoid race condition of multiple responses (e.g., notifications)
                if (t == null)
                    return;

                try
                {
                    t.Stop();
                    t.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // ignore
                }

                
            }

            public void Dispose()
            {
                Cancel();
            }

          private  void timer_Elapsed(Object sender, System.Timers.ElapsedEventArgs e)
            {
                /*
			     * Do not retransmit a message if it has been acknowledged,
			     * rejected, canceled or already been retransmitted for the maximum
			     * number of times.
			     */
                Int32 failedCount = ++_failedTransmissionCount;

                if (_message.IsAcknowledged)
                {
                    
                    return;
                }
                else if (_message.IsRejected)
                {
                   
                    return;
                }
                else if (_message.IsCancelled)
                {
 
                    return;
                }
                else if (failedCount <= (_message.MaxRetransmit != 0 ? _message.MaxRetransmit : _config.MaxRetransmit))
                {
                   
                    _message.FireRetransmitting();

                    // message might have canceled
                    if (!_message.IsCancelled)
                        _retransmit(this);
                }
                else
                { 
                    _exchange.TimedOut = true;
                    _message.IsTimedOut = true;
                    _exchange.Remove(TransmissionContextKey);
                    Cancel();
                }
            }
        }
    }
}
