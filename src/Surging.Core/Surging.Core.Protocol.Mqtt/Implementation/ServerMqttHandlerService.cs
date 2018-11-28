using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using System.Linq;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using Surging.Core.Protocol.Mqtt.Internal.Runtime;

namespace Surging.Core.Protocol.Mqtt.Implementation
{
    public  class ServerMqttHandlerService : MqttHandlerServiceBase
    {
        private readonly ILogger _logger;
        private readonly IChannelService _channelService;
        private readonly IMqttBehaviorProvider _mqttBehaviorProvider;
        public ServerMqttHandlerService(Action<IChannelHandlerContext, object> handler,
            ILogger logger, IChannelService channelService, IMqttBehaviorProvider mqttBehaviorProvider) : base(handler)
        {
            _logger = logger;
            _channelService = channelService;
            _mqttBehaviorProvider = mqttBehaviorProvider;
        }

        public override void ConnAck(IChannelHandlerContext context, ConnAckPacket packet)
        {
            _handler(context, packet);
        }

        public override void Login(IChannelHandlerContext context, ConnectPacket packet)
        {
            string deviceId = packet.ClientId;
            if (string.IsNullOrEmpty(deviceId))
            {
                ConnAck(context, new ConnAckPacket
                {
                    ReturnCode = ConnectReturnCode.RefusedIdentifierRejected
                });
                return;
            }
            var mqttBehavior = _mqttBehaviorProvider.GetMqttBehavior();
            if (mqttBehavior != null)
            {
                if (packet.HasPassword && packet.HasUsername
                        && mqttBehavior.Authorized(packet.Username, packet.Username))
                {
                    var mqttChannel = _channelService.GetMqttChannel(deviceId);
                    if (mqttChannel == null || mqttChannel.SessionStatus == SessionStatus.CLOSE)
                    {
                        byte[] bytes = new byte[packet.WillMessage.ReadableBytes];
                        packet.WillMessage.ReadBytes(bytes);
                        _channelService.Login(context.Channel, deviceId, new ConnectMessage
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
                    ConnAck(context, new ConnAckPacket
                    {
                        ReturnCode = ConnectReturnCode.RefusedBadUsernameOrPassword
                    });
                }
            }
            else
            {
                ConnAck(context, new ConnAckPacket
                {
                    ReturnCode = ConnectReturnCode.RefusedServerUnavailable
                });
            }
        }

        public override void Disconnect(IChannelHandlerContext context, DisconnectPacket packet)
        {
            _channelService.Close(_channelService.GetDeviceId(context.Channel), true);
        }

        public override void PingReq(IChannelHandlerContext context, PingReqPacket packet)
        {
            var channel = context.Channel;
            if (channel.Open && channel.Active && channel.IsWritable)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("收到来自：【" + context.Channel.RemoteAddress.ToString() + "】心跳");
                _handler(context, packet);
            }
        }

        public override void PingResp(IChannelHandlerContext context, PingRespPacket packet)
        {
            _handler(context, packet);
        }

        public override void PubAck(IChannelHandlerContext context, PubAckPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(_channelService.GetDeviceId(context.Channel));
            var message = mqttChannel.GetMqttMessage(messageId);
            message.ConfirmStatus = ConfirmStatus.COMPLETE;
        }

        public override void PubComp(IChannelHandlerContext context, PubCompPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(_channelService.GetDeviceId(context.Channel));
            var message = mqttChannel.GetMqttMessage(messageId);
            message.ConfirmStatus = ConfirmStatus.COMPLETE;
        }

        public override void Publish(IChannelHandlerContext context, PublishPacket packet)
        {
            _channelService.Publish(context.Channel, packet);
        }

        public override void PubRec(IChannelHandlerContext context, PubRecPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(_channelService.GetDeviceId(context.Channel));
             var message= mqttChannel.GetMqttMessage(messageId);
            message.ConfirmStatus=ConfirmStatus.PUBREL;
            _channelService.Pubrec(mqttChannel, messageId);
        }

        public override void PubRel(IChannelHandlerContext context, PubRelPacket packet)
        {
            int messageId = packet.PacketId;
            var mqttChannel = _channelService.GetMqttChannel(_channelService.GetDeviceId(context.Channel));
            var message = mqttChannel.GetMqttMessage(messageId);
            message.ConfirmStatus = ConfirmStatus.PUBREL;
            _channelService.Pubrec(mqttChannel, messageId);
        }

        public override void SubAck(IChannelHandlerContext context, SubAckPacket packet)
        {
            _handler(context, packet);
        }

        public override void Subscribe(IChannelHandlerContext context, SubscribePacket packet)
        {
            var topics = packet.Requests.Select(p => p.TopicFilter).ToArray();
            _channelService.Suscribe(_channelService.GetDeviceId(context.Channel), topics);
            SubAck(context, SubAckPacket.InResponseTo(packet, QualityOfService.ExactlyOnce
             ));
        }

        public override void UnsubAck(IChannelHandlerContext context, UnsubAckPacket packet)
        {
            _handler(context, packet);
        }

        public override void Unsubscribe(IChannelHandlerContext context, UnsubscribePacket packet)
        {
            string [] topics = packet.TopicFilters.ToArray();
            _channelService.UnSubscribe(_channelService.GetDeviceId(context.Channel), topics);
            UnsubAck(context, UnsubAckPacket.InResponseTo(packet));
        }
    }
}
