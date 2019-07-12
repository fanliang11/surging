using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt
{
    /// <summary>
    /// Defines the <see cref="MqttHandlerServiceBase" />
    /// </summary>
    public abstract class MqttHandlerServiceBase
    {
        #region 字段

        /// <summary>
        /// Defines the _handler
        /// </summary>
        protected readonly Action<IChannelHandlerContext, object> _handler;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttHandlerServiceBase"/> class.
        /// </summary>
        /// <param name="handler">The handler<see cref="Action{IChannelHandlerContext, object}"/></param>
        public MqttHandlerServiceBase(Action<IChannelHandlerContext, object> handler)
        {
            _handler = handler;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The ConnAck
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="ConnAckPacket"/></param>
        public abstract void ConnAck(IChannelHandlerContext context, ConnAckPacket packet);

        /// <summary>
        /// The Disconnect
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="DisconnectPacket"/></param>
        public abstract void Disconnect(IChannelHandlerContext context, DisconnectPacket packet);

        /// <summary>
        /// The Login
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="ConnectPacket"/></param>
        public abstract void Login(IChannelHandlerContext context, ConnectPacket packet);

        /// <summary>
        /// The PingReq
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PingReqPacket"/></param>
        public abstract void PingReq(IChannelHandlerContext context, PingReqPacket packet);

        /// <summary>
        /// The PingResp
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PingRespPacket"/></param>
        public abstract void PingResp(IChannelHandlerContext context, PingRespPacket packet);

        /// <summary>
        /// The PubAck
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PubAckPacket"/></param>
        public abstract void PubAck(IChannelHandlerContext context, PubAckPacket packet);

        /// <summary>
        /// The PubComp
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PubCompPacket"/></param>
        public abstract void PubComp(IChannelHandlerContext context, PubCompPacket packet);

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PublishPacket"/></param>
        public abstract void Publish(IChannelHandlerContext context, PublishPacket packet);

        /// <summary>
        /// The PubRec
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PubRecPacket"/></param>
        public abstract void PubRec(IChannelHandlerContext context, PubRecPacket packet);

        /// <summary>
        /// The PubRel
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="PubRelPacket"/></param>
        public abstract void PubRel(IChannelHandlerContext context, PubRelPacket packet);

        /// <summary>
        /// The SubAck
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="SubAckPacket"/></param>
        public abstract void SubAck(IChannelHandlerContext context, SubAckPacket packet);

        /// <summary>
        /// The Subscribe
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="SubscribePacket"/></param>
        public abstract void Subscribe(IChannelHandlerContext context, SubscribePacket packet);

        /// <summary>
        /// The UnsubAck
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="UnsubAckPacket"/></param>
        public abstract void UnsubAck(IChannelHandlerContext context, UnsubAckPacket packet);

        /// <summary>
        /// The Unsubscribe
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="packet">The packet<see cref="UnsubscribePacket"/></param>
        public abstract void Unsubscribe(IChannelHandlerContext context, UnsubscribePacket packet);

        #endregion 方法
    }
}