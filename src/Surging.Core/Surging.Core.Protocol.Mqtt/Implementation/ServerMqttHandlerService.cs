using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Runtime;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Implementation
{
    /// <summary>
    /// Defines the <see cref="ServerMqttHandlerService" />
    /// </summary>
    public class ServerMqttHandlerService
    {
        #region 字段

        /// <summary>
        /// Defines the _channelService
        /// </summary>
        private readonly IChannelService _channelService;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the _mqttBehaviorProvider
        /// </summary>
        private readonly IMqttBehaviorProvider _mqttBehaviorProvider;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerMqttHandlerService"/> class.
        /// </summary>
        /// <param name="logger">The logger<see cref="ILogger"/></param>
        /// <param name="channelService">The channelService<see cref="IChannelService"/></param>
        /// <param name="mqttBehaviorProvider">The mqttBehaviorProvider<see cref="IMqttBehaviorProvider"/></param>
        public ServerMqttHandlerService(
            ILogger logger, IChannelService channelService, IMqttBehaviorProvider mqttBehaviorProvider)
        {
            _logger = logger;
            _channelService = channelService;
            _mqttBehaviorProvider = mqttBehaviorProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The ConnAck
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="ConnAckPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task ConnAck(IChannelHandlerContext context, ConnAckPacket packet)
        {
            await context.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// The Disconnect
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="DisconnectPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Disconnect(IChannelHandlerContext context, DisconnectPacket packet)
        {
            await _channelService.Close(await _channelService.GetDeviceId(context.Channel), true);
        }

        /// <summary>
        /// The Login
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="ConnectPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Login(IChannelHandlerContext context, ConnectPacket packet)
        {
            string deviceId = packet.ClientId;
            if (string.IsNullOrEmpty(deviceId))
            {
                await ConnAck(context, new ConnAckPacket
                {
                    ReturnCode = ConnectReturnCode.RefusedIdentifierRejected
                });
                return;
            }
            var mqttBehavior = _mqttBehaviorProvider.GetMqttBehavior();
            if (mqttBehavior != null)
            {
                if (packet.HasPassword && packet.HasUsername
                        && await mqttBehavior.Authorized(packet.Username, packet.Password))
                {
                    var mqttChannel = _channelService.GetMqttChannel(deviceId);
                    if (mqttChannel == null || !await mqttChannel.IsOnine())
                    {
                        byte[] bytes = null;
                        if (packet.WillMessage != null)
                        {
                            bytes = new byte[packet.WillMessage.ReadableBytes];
                            packet.WillMessage.ReadBytes(bytes);
                        }
                        await _channelService.Login(context.Channel, deviceId, new ConnectMessage
                        {
                            CleanSession = packet.CleanSession,
                            ClientId = packet.ClientId,
                            Duplicate = packet.Duplicate,
                            HasPassword = packet.HasPassword,
                            HasUsername = packet.HasUsername,
                            HasWill = packet.HasWill,
                            KeepAliveInSeconds = packet.KeepAliveInSeconds,
                            Password = packet.Password,
                            ProtocolLevel = packet.ProtocolLevel,
                            ProtocolName = packet.ProtocolName,
                            Qos = (int)packet.QualityOfService,
                            RetainRequested = packet.RetainRequested,
                            Username = packet.Username,
                            WillMessage = bytes,
                            WillQualityOfService = (int)packet.WillQualityOfService,
                            WillRetain = packet.WillRetain,
                            WillTopic = packet.WillTopicName
                        });
                    }
                }
                else
                {
                    await ConnAck(context, new ConnAckPacket
                    {
                        ReturnCode = ConnectReturnCode.RefusedBadUsernameOrPassword
                    });
                }
            }
            else
            {
                await ConnAck(context, new ConnAckPacket
                {
                    ReturnCode = ConnectReturnCode.RefusedServerUnavailable
                });
            }
        }

        /// <summary>
        /// The PingReq
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PingReqPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task PingReq(IChannelHandlerContext context, PingReqPacket packet)
        {
            var channel = context.Channel;
            if (channel.Open && channel.Active && channel.IsWritable)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("收到来自：【" + context.Channel.RemoteAddress.ToString() + "】心跳");
                await _channelService.PingReq(context.Channel);
                await PingResp(context, PingRespPacket.Instance);
            }
        }

        /// <summary>
        /// The PingResp
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PingRespPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task PingResp(IChannelHandlerContext context, PingRespPacket packet)
        {
            await context.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// The PubAck
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PubAckPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task PubAck(IChannelHandlerContext context, PubAckPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(await _channelService.GetDeviceId(context.Channel));
            var message = mqttChannel.GetMqttMessage(messageId);
            if (message != null)
            {
                message.ConfirmStatus = ConfirmStatus.COMPLETE;
            }
            await context.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// The PubComp
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PubCompPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task PubComp(IChannelHandlerContext context, PubCompPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(await _channelService.GetDeviceId(context.Channel));
            var message = mqttChannel.GetMqttMessage(messageId);
            if (message != null)
            {
                message.ConfirmStatus = ConfirmStatus.COMPLETE;
            }
            await context.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PublishPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Publish(IChannelHandlerContext context, PublishPacket packet)
        {
            await _channelService.Publish(context.Channel, packet);
        }

        /// <summary>
        /// The PubRec
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PubRecPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task PubRec(IChannelHandlerContext context, PubRecPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(await _channelService.GetDeviceId(context.Channel));
            var message = mqttChannel.GetMqttMessage(messageId);
            if (message != null)
            {
                message.ConfirmStatus = ConfirmStatus.PUBREC;
            }
            await _channelService.Pubrec(mqttChannel, messageId);
        }

        /// <summary>
        /// The PubRel
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PubRelPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task PubRel(IChannelHandlerContext context, PubRelPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(await _channelService.GetDeviceId(context.Channel));
            var message = mqttChannel.GetMqttMessage(messageId);
            if (message != null)
            {
                message.ConfirmStatus = ConfirmStatus.PUBREL;
            }
            await _channelService.Pubrel(context.Channel, messageId);
        }

        /// <summary>
        /// The SubAck
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="SubAckPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SubAck(IChannelHandlerContext context, SubAckPacket packet)
        {
            await context.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// The Subscribe
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="SubscribePacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Subscribe(IChannelHandlerContext context, SubscribePacket packet)
        {
            if (packet != null)
            {
                var topics = packet.Requests.Select(p => p.TopicFilter).ToArray();
                await _channelService.Suscribe(await _channelService.GetDeviceId(context.Channel), topics);
                await SubAck(context, SubAckPacket.InResponseTo(packet, QualityOfService.ExactlyOnce
                 ));
            }
        }

        /// <summary>
        /// The UnsubAck
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="UnsubAckPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task UnsubAck(IChannelHandlerContext context, UnsubAckPacket packet)
        {
            await context.WriteAndFlushAsync(packet);
        }

        /// <summary>
        /// The Unsubscribe
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="UnsubscribePacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Unsubscribe(IChannelHandlerContext context, UnsubscribePacket packet)
        {
            string[] topics = packet.TopicFilters.ToArray();
            await _channelService.UnSubscribe(await _channelService.GetDeviceId(context.Channel), topics);
            await UnsubAck(context, UnsubAckPacket.InResponseTo(packet));
        }

        #endregion 方法
    }
}