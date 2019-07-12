using DotNetty.Buffers;
using DotNetty.Codecs.Mqtt;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Common.Utilities;
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
using Surging.Core.Protocol.Mqtt.Internal.Runtime;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt
{
    /// <summary>
    /// Defines the <see cref="DotNettyMqttServerMessageListener" />
    /// </summary>
    public class DotNettyMqttServerMessageListener : IMessageListener, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _channelService
        /// </summary>
        private readonly IChannelService _channelService;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<DotNettyMqttServerMessageListener> _logger;

        /// <summary>
        /// Defines the _mqttBehaviorProvider
        /// </summary>
        private readonly IMqttBehaviorProvider _mqttBehaviorProvider;

        /// <summary>
        /// Defines the _channel
        /// </summary>
        private IChannel _channel;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNettyMqttServerMessageListener"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger{DotNettyMqttServerMessageListener}"/></param>
        /// <param name="channelService">The channelService<see cref="IChannelService"/></param>
        /// <param name="mqttBehaviorProvider">The mqttBehaviorProvider<see cref="IMqttBehaviorProvider"/></param>
        public DotNettyMqttServerMessageListener(ILogger<DotNettyMqttServerMessageListener> logger,
            IChannelService channelService,
            IMqttBehaviorProvider mqttBehaviorProvider)
        {
            _logger = logger;
            _channelService = channelService;
            _mqttBehaviorProvider = mqttBehaviorProvider;
        }

        #endregion 构造函数

        #region 事件

        /// <summary>
        /// Defines the Received
        /// </summary>
        public event ReceivedDelegate Received;

        #endregion 事件

        #region 方法

        /// <summary>
        /// The ChannelWrite
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="message">The message<see cref="object"/></param>
        /// <param name="packetType">The packetType<see cref="PacketType"/></param>
        /// <param name="mqttHandlerService">The mqttHandlerService<see cref="ServerMqttHandlerService"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task ChannelWrite(IChannelHandlerContext context, object message, PacketType packetType, ServerMqttHandlerService mqttHandlerService)
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

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The OnReceived
        /// </summary>
        /// <param name="sender">The sender<see cref="IMessageSender"/></param>
        /// <param name="message">The message<see cref="TransportMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        /// <summary>
        /// The StartAsync
        /// </summary>
        /// <param name="endPoint">The endPoint<see cref="EndPoint"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task StartAsync(EndPoint endPoint)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备启动Mqtt服务主机，监听地址：{endPoint}。");
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
            .Option(ChannelOption.SoBacklog, AppConfig.ServerOptions.SoBacklog)
            .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
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
            catch
            {
                _logger.LogError($"mqtt服务主机启动失败，监听地址：{endPoint}。 ");
            }
        }

        #endregion 方法

        /// <summary>
        /// Defines the <see cref="ServerHandler" />
        /// </summary>
        private class ServerHandler : ChannelHandlerAdapter
        {
            #region 字段

            /// <summary>
            /// Defines the _logger
            /// </summary>
            private readonly ILogger _logger;

            /// <summary>
            /// Defines the _readAction
            /// </summary>
            private readonly Action<IChannelHandlerContext, PacketType, object> _readAction;

            #endregion 字段

            #region 构造函数

            /// <summary>
            /// Initializes a new instance of the <see cref="ServerHandler"/> class.
            /// </summary>
            /// <param name="readAction">The readAction<see cref="Action{IChannelHandlerContext,PacketType, object}"/></param>
            /// <param name="logger">The logger<see cref="ILogger"/></param>
            /// <param name="channelService">The channelService<see cref="IChannelService"/></param>
            /// <param name="mqttBehaviorProvider">The mqttBehaviorProvider<see cref="IMqttBehaviorProvider"/></param>
            public ServerHandler(Action<IChannelHandlerContext, PacketType, object> readAction,
                ILogger logger,
                IChannelService channelService,
                IMqttBehaviorProvider mqttBehaviorProvider)
            {
                _readAction = readAction;
                _logger = logger;
            }

            #endregion 构造函数

            #region 方法

            /// <summary>
            /// The ChannelInactive
            /// </summary>
            /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
            public override void ChannelInactive(IChannelHandlerContext context)
            {
                this.SetException(new InvalidOperationException("Channel is closed."));
                base.ChannelInactive(context);
            }

            /// <summary>
            /// The ChannelRead
            /// </summary>
            /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
            /// <param name="message">The message<see cref="object"/></param>
            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var buffer = message as Packet;
                _readAction(context, buffer.PacketType, buffer);
                ReferenceCountUtil.Release(message);
            }

            /// <summary>
            /// The ExceptionCaught
            /// </summary>
            /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
            /// <param name="exception">The exception<see cref="Exception"/></param>
            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                _readAction.Invoke(context, PacketType.DISCONNECT, DisconnectPacket.Instance);
                this.SetException(exception);
            }

            /// <summary>
            /// The SetException
            /// </summary>
            /// <param name="ex">The ex<see cref="Exception"/></param>
            internal void SetException(Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError($"message:{ex.Message},Source:{ex.Source},Trace:{ex.StackTrace}");
            }

            #endregion 方法
        }
    }
}