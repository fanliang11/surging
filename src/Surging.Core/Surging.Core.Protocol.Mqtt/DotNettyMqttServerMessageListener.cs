﻿using DotNetty.Buffers;
using DotNetty.Codecs.Mqtt;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Diagnostics;
using Surging.Core.CPlatform.EventExecutor;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Network;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.Mqtt.Implementation;
using Surging.Core.Protocol.Mqtt.Interceptors;
using Surging.Core.Protocol.Mqtt.Internal.Channel;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Runtime;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using Surging.Core.ProxyGenerator.Interceptors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt
{
    public class DotNettyMqttServerMessageListener : IMessageListener,INetwork, IDisposable
    {
        #region Field

        private readonly ILogger<DotNettyMqttServerMessageListener> _logger;
        private IChannel _channel;
        private readonly IChannelService _channelService;
        private readonly IMqttBehaviorProvider _mqttBehaviorProvider;
        private readonly DiagnosticListener _diagnosticListener;
        private readonly NetworkProperties _properties;
        private readonly IEventExecutorProvider _eventExecutorProvider;
        public string Id { get; set; }
        #endregion Field

        public event ReceivedDelegate Received;

        #region Constructor
        public DotNettyMqttServerMessageListener(ILogger<DotNettyMqttServerMessageListener> logger,
       IChannelService channelService,
       IEventExecutorProvider eventExecutorProvider,
       IMqttBehaviorProvider mqttBehaviorProvider):this(logger, channelService, eventExecutorProvider, mqttBehaviorProvider, new NetworkProperties())
        {

        }
        public DotNettyMqttServerMessageListener(ILogger<DotNettyMqttServerMessageListener> logger, 
            IChannelService channelService,
             IEventExecutorProvider eventExecutorProvider,
            IMqttBehaviorProvider mqttBehaviorProvider,
             NetworkProperties properties)
        {
            Id = properties?.Id;
            _logger = logger;
            _eventExecutorProvider = eventExecutorProvider;
            _channelService = channelService;
            _mqttBehaviorProvider = mqttBehaviorProvider;
            _diagnosticListener = new DiagnosticListener(DiagnosticListenerExtensions.DiagnosticListenerName);
            _properties = properties;
        }
        #endregion

        public async void Dispose()
        {
            await _channel.CloseAsync();
        }

        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        public async Task StartAsync(EndPoint endPoint)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备启动Mqtt服务主机，监听地址：{endPoint}。");
            IEventLoopGroup bossGroup = _eventExecutorProvider.GetBossEventExecutor();
            IEventLoopGroup workerGroup = _eventExecutorProvider.GetWorkEventExecutor();//Default eventLoopCount is Environment.ProcessorCount * 2
            var bootstrap = new ServerBootstrap();
            if (AppConfig.ServerOptions.Libuv)
            {
                bootstrap.Channel<TcpServerChannel>();
            }
            else
            {
                bootstrap.Channel<TcpServerSocketChannel>(); 
            }
            bootstrap
            .Option(ChannelOption.SoBacklog, AppConfig.ServerOptions.SoBacklog)
            .ChildOption(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
            .Group(bossGroup, workerGroup)
            .Option(ChannelOption.TcpNodelay, true)
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                pipeline.AddLast(MqttEncoder.Instance,
                    new MqttDecoder(true, 256 * 1024), new ServerHandler(async (context, packetType, message) =>
                {
                    var mqttHandlerService = new ServerMqttHandlerService(_logger, _channelService, _mqttBehaviorProvider);
                    await ChannelWrite(context, message, packetType, mqttHandlerService);
                }, _logger, _channelService, _mqttBehaviorProvider));
            }));
            try
            {
                _channel = await bootstrap.BindAsync(endPoint);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"mqtt服务主机启动成功，监听地址：{endPoint}。");
            }
            catch(Exception ex)
            {
                _logger.LogError($"mqtt服务主机启动失败，监听地址：{endPoint}。 ");
            }
        }

        public async Task ChannelWrite(IChannelHandlerContext context,object message, PacketType packetType, ServerMqttHandlerService mqttHandlerService)
        {
            var mqttBehavior=  _mqttBehaviorProvider.GetMqttBehavior();
            var result = true;
            if (packetType != PacketType.PINGREQ && packetType != PacketType.PINGRESP && packetType != PacketType.DISCONNECT)
            {
                var interceptor = ServiceLocator.IsRegistered<IMqttInterceptor>() ? ServiceLocator.GetService<IMqttInterceptor>() : null;
                var mqttChannel = _channelService.GetMqttChannel(await _channelService.GetDeviceId(context.Channel));
                var pubPacket = message as PublishPacket;
                var topics = mqttChannel?.Topics;
                if (packetType == PacketType.PUBLISH)
                    topics = new List<string>() { (message as PublishPacket)?.TopicName };
                else if (packetType == PacketType.SUBSCRIBE)
                    topics = (message as SubscribePacket) ?.Requests.Select(p => p.TopicFilter).ToList();
                result = interceptor != null ? await interceptor.Intercept(new MqttInvocation()
                {
                    Behavior = mqttBehavior,
                    Message = message,
                    NetworkId = Id,
                    PacketType = packetType,
                    RemoteAddress = context.Channel.RemoteAddress,
                    Topic = topics
                }) : true;
            }
            if (result)
            {
                switch (packetType)
                {
                    case PacketType.CONNECT:
                        await mqttHandlerService.Login(context, message as ConnectPacket);
                        break;
                    case PacketType.PUBLISH:
                        await mqttHandlerService.Publish(context, message as PublishPacket);
                        break;
                    case PacketType.PUBACK:
                        await mqttHandlerService.PubAck(context, message as PubAckPacket);
                        break;
                    case PacketType.PUBREC:
                        await mqttHandlerService.PubRec(context, message as PubRecPacket);
                        break;
                    case PacketType.PUBREL:
                        await mqttHandlerService.PubRel(context, message as PubRelPacket);
                        break;
                    case PacketType.PUBCOMP:
                        await mqttHandlerService.PubComp(context, message as PubCompPacket);
                        break;
                    case PacketType.SUBSCRIBE:
                        await mqttHandlerService.Subscribe(context, message as SubscribePacket);
                        break;
                    case PacketType.SUBACK:
                        await mqttHandlerService.SubAck(context, message as SubAckPacket);
                        break;
                    case PacketType.UNSUBSCRIBE:
                        await mqttHandlerService.Unsubscribe(context, message as UnsubscribePacket);
                        break;
                    case PacketType.UNSUBACK:
                        await mqttHandlerService.UnsubAck(context, message as UnsubAckPacket);
                        break;
                    case PacketType.PINGREQ:
                        await mqttHandlerService.PingReq(context, message as PingReqPacket);
                        break;
                    case PacketType.PINGRESP:
                        await mqttHandlerService.PingResp(context, message as PingRespPacket);
                        break;
                    case PacketType.DISCONNECT:
                        await mqttHandlerService.Disconnect(context, message as DisconnectPacket);
                        break;
                }
            }
        }

        private class ServerHandler : ChannelHandlerAdapter
        {
            private readonly Action<IChannelHandlerContext, PacketType, object> _readAction;
            private readonly ILogger _logger; 

            public ServerHandler(Action<IChannelHandlerContext,PacketType, object> readAction, 
                ILogger logger,
                IChannelService channelService,
                IMqttBehaviorProvider mqttBehaviorProvider)  
            {
                _readAction = readAction;
                _logger = logger;
            }
             
            public override void ChannelRead(IChannelHandlerContext context, object message)
            { 
                var buffer = message as Packet;
                _readAction(context, buffer.PacketType, buffer);
                ReferenceCountUtil.Release(message);
            }

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                this.SetException(new InvalidOperationException("Channel is closed."));
                base.ChannelInactive(context);
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception) {
                _readAction.Invoke(context,PacketType.DISCONNECT,DisconnectPacket.Instance);
                this.SetException(exception);
            }

            void SetException(Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError($"message:{ex.Message},Source:{ex.Source},Trace:{ex.StackTrace}");
            }
        }

        private void WirteDiagnosticError(TransportMessage message)
        {
            if (!AppConfig.ServerOptions.DisableDiagnostic)
            {
                var remoteInvokeResultMessage = message.GetContent<RemoteInvokeResultMessage>();
                _diagnosticListener.WriteTransportError(TransportType.Mqtt, new TransportErrorEventData(new DiagnosticMessage
                {
                    Content = message.Content,
                    ContentType = message.ContentType,
                    Id = message.Id
                }, new Exception(remoteInvokeResultMessage.ExceptionMessage)));
            }
        }

        public async Task StartAsync()
        {
            await StartAsync(_properties.CreateSocketAddress());
        }

        NetworkType INetwork.GetType()
        {
            return NetworkType.Mqtt;
        }

        public async void Shutdown()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Mqtt服务主机已停止。");
            await _channel.CloseAsync();
        }

        public bool IsAlive()
        {
            return _channel.Active;
        }

        public bool IsAutoReload()
        {
            return false;
        }
    }
}
