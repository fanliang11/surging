using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using System.Linq;
using Surging.Core.Protocol.Mqtt.Internal.Services;

namespace Surging.Core.Protocol.Mqtt.Implementation
{
    public  class ServerMqttHandlerService : MqttHandlerServiceBase
    {
        private readonly ILogger _logger;
        private readonly IChannelService _channelService;
        public ServerMqttHandlerService(Action<IChannelHandlerContext, object> handler,
            ILogger logger, IChannelService channelService) : base(handler)
        {
            _logger = logger;
            _channelService = channelService;
        }

        public override void ConnAck(IChannelHandlerContext context, ConnAckPacket packet)
        {
            _handler(context, packet);
        }

        public override void Connect(IChannelHandlerContext context, ConnectPacket packet)
        {
            _handler(context, packet);
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
