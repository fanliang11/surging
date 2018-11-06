using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Messages;

namespace Surging.Core.Protocol.Mqtt.Implementation
{
    public  class ServerMqttHandlerService : MqttHandlerServiceBase
    {
        public ServerMqttHandlerService(Action<IChannelHandlerContext, TransportMessage> handler) : base(handler)
        {
        }

        public override void ConnAck(IChannelHandlerContext context, ConnAckPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void Connect(IChannelHandlerContext context, ConnectPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void Disconnect(IChannelHandlerContext context, DisconnectPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void PingReq(IChannelHandlerContext context, PingReqPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void PingResp(IChannelHandlerContext context, PingRespPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void PubAck(IChannelHandlerContext context, PubAckPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void PubComp(IChannelHandlerContext context, PubCompPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void Publish(IChannelHandlerContext context, PublishPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void PubRec(IChannelHandlerContext context, PubRecPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void PubRel(IChannelHandlerContext context, PubRelPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void SubAck(IChannelHandlerContext context, SubAckPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void Subscribe(IChannelHandlerContext context, SubscribePacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void UnsubAck(IChannelHandlerContext context, UnsubAckPacket packet)
        {
            _handler(context, new TransportMessage());
        }

        public override void Unsubscribe(IChannelHandlerContext context, UnsubscribePacket packet)
        {
            _handler(context, new TransportMessage());
        }
    }
}
