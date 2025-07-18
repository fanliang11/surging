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
using CoAP.Channel;
using CoAP.Codec; 
using CoAP.Stack;
using CoAP.Threading;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Utilities;

namespace CoAP.Net
{
    /// <summary>
    /// EndPoint encapsulates the stack that executes the CoAP protocol.
    /// </summary>
    public partial class CoAPEndPoint : IEndPoint, IOutbox
    {
       private readonly ILogger log ;

        readonly ICoapConfig _config;
        readonly IChannel _channel;
        readonly CoapStack _coapStack;
        private IMessageDeliverer _deliverer;
        private IMatcher _matcher;
        private Int32 _running;
        private System.Net.EndPoint _localEP;
        private IExecutor _executor;

        /// <inheritdoc/>
        public event EventHandler<MessageEventArgs<Request>> SendingRequest;
        /// <inheritdoc/>
        public event EventHandler<MessageEventArgs<Response>> SendingResponse;
        /// <inheritdoc/>
        public event EventHandler<MessageEventArgs<EmptyMessage>> SendingEmptyMessage;
        /// <inheritdoc/>
        public event EventHandler<MessageEventArgs<Request>> ReceivingRequest;
        /// <inheritdoc/>
        public event EventHandler<MessageEventArgs<Response>> ReceivingResponse;
        /// <inheritdoc/>
        public event EventHandler<MessageEventArgs<EmptyMessage>> ReceivingEmptyMessage;

        /// <summary>
        /// Instantiates a new endpoint.
        /// </summary>
        public CoAPEndPoint()
            : this(0, CoapConfig.Default)
        { }

        /// <summary>
        /// Instantiates a new endpoint with the specified configuration.
        /// </summary>
        public CoAPEndPoint(ICoapConfig config)
            : this(0, config)
        { }

        /// <summary>
        /// Instantiates a new endpoint with the specified port.
        /// </summary>
        public CoAPEndPoint(Int32 port)
            : this(port, CoapConfig.Default)
        { }

        /// <summary>
        /// Instantiates a new endpoint with the
        /// specified <see cref="System.Net.EndPoint"/>.
        /// </summary>
        public CoAPEndPoint(System.Net.EndPoint localEP)
            : this(localEP, CoapConfig.Default)
        { }

        /// <summary>
        /// Instantiates a new endpoint with the
        /// specified port and configuration.
        /// </summary>
        public CoAPEndPoint(Int32 port, ICoapConfig config)
            : this(NewUDPChannel(port, config), config)
        { }

        /// <summary>
        /// Instantiates a new endpoint with the
        /// specified <see cref="System.Net.EndPoint"/> and configuration.
        /// </summary>
        public CoAPEndPoint(System.Net.EndPoint localEP, ICoapConfig config)
            : this(NewUDPChannel(localEP, config), config)
        { }

        /// <summary>
        /// Instantiates a new endpoint with the
        /// specified channel and configuration.
        /// </summary>
        public CoAPEndPoint(IChannel channel, ICoapConfig config)
        {
            log=ServiceLocator.GetService<ILogger<CoAPEndPoint>>();
            _config = config;
            _channel = channel;
            _matcher = new Matcher(config);
            _coapStack = new CoapStack(config);
            _channel.DataReceived += ReceiveData;
        }

        /// <inheritdoc/>
        public ICoapConfig Config
        {
            get { return _config; }
        }

        public IExecutor Executor
        {
            get { return _executor; }
            set
            {
                _executor = value ?? Executors.NoThreading;
                _coapStack.Executor = _executor;
            }
        }

        /// <inheritdoc/>
        public System.Net.EndPoint LocalEndPoint
        {
            get { return _localEP; }
        }

        /// <inheritdoc/>
        public IMessageDeliverer MessageDeliverer
        {
            set { _deliverer = value; }
            get
            {
                if (_deliverer == null)
                    _deliverer = new ClientMessageDeliverer();
                return _deliverer;
            }
        }

        /// <inheritdoc/>
        public IOutbox Outbox
        {
            get { return this; }
        }

        /// <inheritdoc/>
        public Boolean Running
        {
            get { return _running > 0; }
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (System.Threading.Interlocked.CompareExchange(ref _running, 1, 0) > 0)
                return;

            if (_executor == null)
                Executor = Executors.Default;

            _localEP = _channel.LocalEndPoint;
            try
            {
                _matcher.Start();
                _channel.Start();
                _localEP = _channel.LocalEndPoint;
            }
            catch
            {
                if (log.IsEnabled(LogLevel.Warning))
                    log.LogWarning("Cannot start endpoint at " + _localEP);
                Stop();
                throw;
            }
            if (log.IsEnabled(LogLevel.Debug))
                log.LogDebug("Starting endpoint bound to " + _localEP);
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (System.Threading.Interlocked.Exchange(ref _running, 0) == 0)
                return;
           if (log.IsEnabled(LogLevel.Debug))
                 log.LogDebug("Stopping endpoint bound to " + _localEP);
            _channel.Stop();
            _matcher.Stop();
            _matcher.Clear();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _matcher.Clear();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Running)
                Stop();
            _channel.Dispose();
            IDisposable d = _matcher as IDisposable;
            if (d != null)
                d.Dispose();
        }

        /// <inheritdoc/>
        public void SendRequest(Request request)
        {
            _executor.Start(() => _coapStack.SendRequest(request));
        }

        /// <inheritdoc/>
        public void SendResponse(Exchange exchange, Response response)
        {
            _executor.Start(() => _coapStack.SendResponse(exchange, response));
        }

        /// <inheritdoc/>
        public void SendEmptyMessage(Exchange exchange, EmptyMessage message)
        {
            _executor.Start(() => _coapStack.SendEmptyMessage(exchange, message));
        }

        private void ReceiveData(Object sender, DataReceivedEventArgs e)
        {
            _executor.Start(() => ReceiveData(e));
        }

        private void ReceiveData(DataReceivedEventArgs e)
        {
            IMessageDecoder decoder = Spec.NewMessageDecoder(e.Data);
            if (decoder.IsRequest)
            {
                Request request;
                try
                {
                    request = decoder.DecodeRequest();
                }
                catch (Exception)
                {
                    if (decoder.IsReply)
                    {
                        if (log.IsEnabled(LogLevel.Warning))
                            log.LogWarning("Message format error caused by " + e.EndPoint);
                    }
                    else
                    {
                        // manually build RST from raw information
                        EmptyMessage rst = new EmptyMessage(MessageType.RST);
                        rst.Destination = e.EndPoint;
                        rst.ID = decoder.ID;

                        Fire(SendingEmptyMessage, rst);

                        _channel.Send(Serialize(rst), rst.Destination);

                        if (log.IsEnabled(LogLevel.Warning))
                            log.LogWarning("Message format error caused by " + e.EndPoint + " and reseted.");
                    }
                    return;
                }

                request.Source = e.EndPoint;

                Fire(ReceivingRequest, request);

                if (!request.IsCancelled)
                {
                    Exchange exchange = _matcher.ReceiveRequest(request);
                    if (exchange != null)
                    {
                        exchange.EndPoint = this;
                        _coapStack.ReceiveRequest(exchange, request);
                    }
                }
            }
            else if (decoder.IsResponse)
            {
                Response response = decoder.DecodeResponse();
                response.Source = e.EndPoint;

                Fire(ReceivingResponse, response);

                if (!response.IsCancelled)
                {
                    Exchange exchange = _matcher.ReceiveResponse(response);
                    if (exchange != null)
                    {
                        response.RTT = (DateTime.Now - exchange.Timestamp).TotalMilliseconds;
                        exchange.EndPoint = this;
                        _coapStack.ReceiveResponse(exchange, response);
                    }
                    else if (response.Type != MessageType.ACK)
                    {
                       if (log.IsEnabled(LogLevel.Debug))
                             log.LogDebug("Rejecting unmatchable response from " + e.EndPoint);
                        Reject(response);
                    }
                }
            }
            else if (decoder.IsEmpty)
            {
                EmptyMessage message = decoder.DecodeEmptyMessage();
                message.Source = e.EndPoint;

                Fire(ReceivingEmptyMessage, message);

                if (!message.IsCancelled)
                {
                    // CoAP Ping
                    if (message.Type == MessageType.CON || message.Type == MessageType.NON)
                    {
                       if (log.IsEnabled(LogLevel.Debug))
                             log.LogDebug("Responding to ping by " + e.EndPoint);
                        Reject(message);
                    }
                    else
                    {
                        Exchange exchange = _matcher.ReceiveEmptyMessage(message);
                        if (exchange != null)
                        {
                            exchange.EndPoint = this;
                            _coapStack.ReceiveEmptyMessage(exchange, message);
                        }
                    }
                }
            }
            else if (log.IsEnabled(LogLevel.Debug))
            {
                 log.LogDebug("Silently ignoring non-CoAP message from " + e.EndPoint);
            }
        }

        private void Reject(Message message)
        {
            EmptyMessage rst = EmptyMessage.NewRST(message);

            Fire(SendingEmptyMessage, rst);

            if (!rst.IsCancelled)
                _channel.Send(Serialize(rst), rst.Destination);
        }

        private Byte[] Serialize(EmptyMessage message)
        {
            Byte[] bytes = message.Bytes;
            if (bytes == null)
            {
                bytes = Spec.NewMessageEncoder().Encode(message);
                message.Bytes = bytes;
            }
            return bytes;
        }

        private Byte[] Serialize(Request request)
        {
            Byte[] bytes = request.Bytes;
            if (bytes == null)
            {
                bytes = Spec.NewMessageEncoder().Encode(request);
                request.Bytes = bytes;
            }
            return bytes;
        }

        private Byte[] Serialize(Response response)
        {
            Byte[] bytes = response.Bytes;
            if (bytes == null)
            {
                bytes = Spec.NewMessageEncoder().Encode(response);
                response.Bytes = bytes;
            }
            return bytes;
        }

        private void Fire<T>(EventHandler<MessageEventArgs<T>> handler, T msg) where T : Message
        {
            if (handler != null)
                handler(this, new MessageEventArgs<T>(msg));
        }

        static IChannel NewUDPChannel(Int32 port, ICoapConfig config)
        {
            UDPChannel channel = new UDPChannel(port);
            channel.ReceiveBufferSize = config.ChannelReceiveBufferSize;
            channel.SendBufferSize = config.ChannelSendBufferSize;
            channel.ReceivePacketSize = config.ChannelReceivePacketSize;
            return channel;
        }

        static IChannel NewUDPChannel(System.Net.EndPoint localEP, ICoapConfig config)
        { 
            UDPChannel channel = new UDPChannel(localEP);
            channel.ReceiveBufferSize = config.ChannelReceiveBufferSize;
            channel.SendBufferSize = config.ChannelSendBufferSize;
            channel.ReceivePacketSize = config.ChannelReceivePacketSize;
            return channel; 
        }

        void IOutbox.SendRequest(Exchange exchange, Request request)
        {
            _matcher.SendRequest(exchange, request);

            Fire(SendingRequest, request);

            if (!request.IsCancelled)
                _channel.Send(Serialize(request), request.Destination);
        }

        void IOutbox.SendResponse(Exchange exchange, Response response)
        {
            _matcher.SendResponse(exchange, response);

            Fire(SendingResponse, response);

            if (!response.IsCancelled)
                _channel.Send(Serialize(response), response.Destination);
        }

        void IOutbox.SendEmptyMessage(Exchange exchange, EmptyMessage message)
        {
            _matcher.SendEmptyMessage(exchange, message);

            Fire(SendingEmptyMessage, message);

            if (!message.IsCancelled)
                _channel.Send(Serialize(message), message.Destination);
        }
    }
}
