using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using System.Linq;
using DotNetty.Common.Utilities;

namespace Surging.Core.Protocol.Mqtt.Implementation
{
    public  class ServerMqttHandlerService : MqttHandlerServiceBase
    {
        private readonly ILogger _logger;
        public ServerMqttHandlerService(Action<IChannelHandlerContext, MqttMessage> handler,
            ILogger logger) : base(handler)
        {
            _logger = logger;
        }

        public override void ConnAck(IChannelHandlerContext context, ConnAckPacket packet)
        {
            _handler(context, new ConnAckMessage());
        }

        public override void Connect(IChannelHandlerContext context, ConnectPacket packet)
        {
            _handler(context, new ConnectMessage());
        }

        public override void Disconnect(IChannelHandlerContext context, DisconnectPacket packet)
        {
            _handler(context, new DisconnectMessage());
        }

        public override void PingReq(IChannelHandlerContext context, PingReqPacket packet)
        {
            var channel = context.Channel;
            if (channel.Open && channel.Active && channel.IsWritable)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("收到来自：【" + context.Channel.RemoteAddress.ToString() + "】心跳");
                _handler(context, new PingReqMessage());
            }
        }

        public override void PingResp(IChannelHandlerContext context, PingRespPacket packet)
        {
            _handler(context, new PingRespMessage());
        }

        public override void PubAck(IChannelHandlerContext context, PubAckPacket packet)
        {
            _handler(context, new PubAckMessage());
        }

        public override void PubComp(IChannelHandlerContext context, PubCompPacket packet)
        {
            _handler(context, new PubCompMessage());
        }

        public override void Publish(IChannelHandlerContext context, PublishPacket packet)
        {
            _handler(context, new PublishMessage());
        }

        public override void PubRec(IChannelHandlerContext context, PubRecPacket packet)
        {
            _handler(context, new PubRecMessage());
        }

        public override void PubRel(IChannelHandlerContext context, PubRelPacket packet)
        {
            _handler(context, new PubRelMessage());
        }

        public override void SubAck(IChannelHandlerContext context, SubAckPacket packet)
        {
            _handler(context, new SubAckMessage());
        }

        public override void Subscribe(IChannelHandlerContext context, SubscribePacket packet)
        { 
            _handler(context, new SubscribeMessage(packet.PacketId, packet.Requests
                .Select(p=> 
            new SubscriptionRequestData(p.TopicFilter,(int)p.QualityOfService) ).ToArray()));
        }

        public override void UnsubAck(IChannelHandlerContext context, UnsubAckPacket packet)
        {
            _handler(context, new UnsubAckMessage());
        }

        public override void Unsubscribe(IChannelHandlerContext context, UnsubscribePacket packet)
        {
            _handler(context, new UnsubscribeMessage(packet.PacketId,packet.TopicFilters.ToArray()));
        }
    }
}
