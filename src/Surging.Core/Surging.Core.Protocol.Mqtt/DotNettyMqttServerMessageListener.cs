using DotNetty.Buffers;
using DotNetty.Codecs.Mqtt;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport;
using Surging.Core.CPlatform.Transport.Codec;
using Surging.Core.Protocol.Mqtt.Implementation;
using Surging.Core.Protocol.Mqtt.Internal.Channel;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt
{
    public class DotNettyMqttServerMessageListener : IMessageListener, IDisposable
    {
        #region Field

        private readonly ILogger<DotNettyMqttServerMessageListener> _logger;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private IChannel _channel;
        private readonly ISerializer<string> _serializer;

        #endregion Field

        public event ReceivedDelegate Received;

        #region Constructor
        public DotNettyMqttServerMessageListener(ILogger<DotNettyMqttServerMessageListener> logger, 
            ITransportMessageCodecFactory codecFactory,
            ISerializer<string> serializer)
        {
            _logger = logger;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _serializer = serializer;
        }
        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
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
                _logger.LogDebug($"准备启动服务主机，监听地址：{endPoint}。");
            IEventLoopGroup bossGroup = new MultithreadEventLoopGroup(1);
            IEventLoopGroup workerGroup = new MultithreadEventLoopGroup();//Default eventLoopCount is Environment.ProcessorCount * 2
            var bootstrap = new ServerBootstrap();
            if (AppConfig.ServerOptions.Libuv)
            {
                var dispatcher = new DispatcherEventLoopGroup();
                bossGroup = dispatcher;
                workerGroup = new WorkerEventLoopGroup(dispatcher);
                bootstrap.Channel<TcpServerChannel>();
            }
            else
            {
                bossGroup = new MultithreadEventLoopGroup(1);
                workerGroup = new MultithreadEventLoopGroup();
                bootstrap.Channel<TcpServerSocketChannel>();
            }
            bootstrap 
            .Option(ChannelOption.SoBacklog, 100)
            .ChildOption(ChannelOption.RcvbufAllocator,new AdaptiveRecvByteBufAllocator())
            .Group(bossGroup, workerGroup)
            .Option(ChannelOption.TcpNodelay, true)
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                pipeline.AddLast(MqttEncoder.Instance,
                    new MqttDecoder(true, 256 * 1024), new ServerHandler(async (contenxt, message) =>
                { 
                    await contenxt.WriteAsync(message);
                }, _logger, _serializer));
            }));
            try
            {
                _channel = await bootstrap.BindAsync(endPoint);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"mqtt服务主机启动成功，监听地址：{endPoint}。");
            }
            catch
            {
                _logger.LogError($"mqtt服务主机启动失败，监听地址：{endPoint}。 ");
            }
        }

        private class ServerHandler : ChannelHandlerAdapter
        {
            private readonly Action<IChannelHandlerContext, MqttMessage> _readAction;
            private readonly ILogger _logger;
            private readonly ISerializer<string> _serializer;
            private readonly MqttHandlerServiceBase _mqttHandlerService;

            public ServerHandler(Action<IChannelHandlerContext, MqttMessage> readAction, 
                ILogger logger,
                ISerializer<string> serializer)  
            {
                _readAction = readAction;
                _logger = logger;
                _serializer = serializer;
                _mqttHandlerService = new ServerMqttHandlerService(_readAction,logger);
            }
             
            public override void ChannelRead(IChannelHandlerContext context, object message)
            { 
                var buffer = message as Packet;
                switch ( buffer.PacketType)
                {
                    case PacketType.CONNECT:
                        _mqttHandlerService.Connect(context, buffer as ConnectPacket);
                        break;
                    case PacketType.CONNACK:
                        _mqttHandlerService.ConnAck(context, buffer as ConnAckPacket);
                        break;
                    case PacketType.PUBLISH:
                        _mqttHandlerService.Disconnect(context, buffer as DisconnectPacket);
                        break;
                    case PacketType.PUBACK:
                        _mqttHandlerService.PingReq(context, buffer as PingReqPacket);
                        break;
                    case PacketType.PUBREC:
                        _mqttHandlerService.PingResp(context, buffer as PingRespPacket);
                        break;
                    case PacketType.PUBREL:
                        _mqttHandlerService.PubAck(context, buffer as PubAckPacket);
                        break;
                    case PacketType.PUBCOMP:
                        _mqttHandlerService.PubComp(context, buffer as PubCompPacket);
                        break;
                    case PacketType.SUBSCRIBE:
                        _mqttHandlerService.PubRec(context, buffer as PubRecPacket);
                        break;
                    case PacketType.SUBACK:
                        _mqttHandlerService.PubRel(context, buffer as PubRelPacket);
                        break;
                    case PacketType.UNSUBSCRIBE:
                        _mqttHandlerService.Publish(context, buffer as PublishPacket);
                        break;
                    case PacketType.UNSUBACK:
                        _mqttHandlerService.SubAck(context, buffer as SubAckPacket);
                        break;
                    case PacketType.PINGREQ:
                        _mqttHandlerService.Subscribe(context, buffer as SubscribePacket);
                        break;
                    case PacketType.PINGRESP:
                        _mqttHandlerService.UnsubAck(context, buffer as UnsubAckPacket);
                        break;
                    case PacketType.DISCONNECT:
                        _mqttHandlerService.Unsubscribe(context, buffer as UnsubscribePacket);
                        break;
                }
            }

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                this.SetException(new InvalidOperationException("Channel is closed."));
                base.ChannelInactive(context);
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception) => this.SetException(exception);

            void SetException(Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError($"message:{ex.Message},Source:{ex.Source},Trace:{ex.StackTrace}");
            }
        }
    }
}
