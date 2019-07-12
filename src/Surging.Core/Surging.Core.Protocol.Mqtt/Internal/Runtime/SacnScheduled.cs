using DotNetty.Buffers;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    /// <summary>
    /// Defines the <see cref="SacnScheduled" />
    /// </summary>
    public class SacnScheduled : ScanRunnable
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="SacnScheduled"/> class.
        /// </summary>
        public SacnScheduled()
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Execute
        /// </summary>
        /// <param name="poll">The poll<see cref="SendMqttMessage"/></param>
        public override void Execute(SendMqttMessage poll)
        {
            if (CheckTime(poll.Time) && poll.Channel.Active)
            {
                poll.Time = DateTime.Now.Ticks / 10000;
                switch (poll.ConfirmStatus)
                {
                    case ConfirmStatus.PUB:
                        PubMessage(poll.Channel, poll);
                        break;

                    case ConfirmStatus.PUBREL:
                        PubRelAck(poll);
                        break;

                    case ConfirmStatus.PUBREC:
                        PubRecAck(poll);
                        break;
                }
            }
        }

        /// <summary>
        /// The PubRelAck
        /// </summary>
        /// <param name="mqttMessage">The mqttMessage<see cref="SendMqttMessage"/></param>
        protected void PubRelAck(SendMqttMessage mqttMessage)
        {
            PubRelPacket mqttPubAckMessage = new PubRelPacket()
            {
                PacketId = mqttMessage.MessageId,
            };
            mqttMessage.Channel.WriteAndFlushAsync(mqttPubAckMessage);
        }

        /// <summary>
        /// The CheckTime
        /// </summary>
        /// <param name="time">The time<see cref="long"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private bool CheckTime(long time)
        {
            return DateTime.Now.Millisecond - time >= 10 * 1000;
        }

        /// <summary>
        /// The PubMessage
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="mqttMessage">The mqttMessage<see cref="SendMqttMessage"/></param>
        private void PubMessage(IChannel channel, SendMqttMessage mqttMessage)
        {
            PublishPacket mqttPublishMessage = new PublishPacket((QualityOfService)mqttMessage.Qos, true, mqttMessage.Retain)
            {
                TopicName = mqttMessage.Topic,
                PacketId = mqttMessage.MessageId,
                Payload = Unpooled.WrappedBuffer(mqttMessage.ByteBuf)
            };
            channel.WriteAndFlushAsync(mqttPublishMessage);
        }

        /// <summary>
        /// The PubRecAck
        /// </summary>
        /// <param name="mqttMessage">The mqttMessage<see cref="SendMqttMessage"/></param>
        private void PubRecAck(SendMqttMessage mqttMessage)
        {
            PubRecPacket mqttPubAckMessage = new PubRecPacket()
            {
                PacketId = mqttMessage.MessageId,
            };
            mqttMessage.Channel.WriteAndFlushAsync(mqttPubAckMessage);
        }

        #endregion 方法
    }
}