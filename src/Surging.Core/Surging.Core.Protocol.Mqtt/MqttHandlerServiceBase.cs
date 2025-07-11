using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt
{
   public abstract class MqttHandlerServiceBase
    {
        protected readonly Action<IChannelHandlerContext, object> _handler;
        public MqttHandlerServiceBase( Action<IChannelHandlerContext, object> handler)
        {
            _handler = handler;
        }

        public abstract void Login(IChannelHandlerContext context, ConnectPacket packet);

        public abstract void ConnAck(IChannelHandlerContext context, ConnAckPacket packet);

        public abstract void Disconnect(IChannelHandlerContext context, DisconnectPacket packet);

        public abstract void PingReq(IChannelHandlerContext context, PingReqPacket packet);

        public abstract void PingResp(IChannelHandlerContext context, PingRespPacket packet);

        public abstract void PubAck(IChannelHandlerContext context, PubAckPacket packet);

        public abstract void PubComp(IChannelHandlerContext context, PubCompPacket packet);

        public abstract void PubRec(IChannelHandlerContext context, PubRecPacket packet);

        public abstract void PubRel(IChannelHandlerContext context, PubRelPacket packet);

        public abstract void Publish(IChannelHandlerContext context, PublishPacket packet);

        public abstract void SubAck(IChannelHandlerContext context, SubAckPacket packet);

        public abstract void Subscribe(IChannelHandlerContext context, SubscribePacket packet);

        public abstract void UnsubAck(IChannelHandlerContext context, UnsubAckPacket packet);

        public abstract void Unsubscribe(IChannelHandlerContext context, UnsubscribePacket packet);

    }
}
