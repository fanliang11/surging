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
    public class SacnScheduled: ScanRunnable
    {

        public SacnScheduled()
        {
        }

        private bool CheckTime(long time)
        {
            return DateTime.Now.Millisecond - time >= 10 * 1000;
        }
         
        public override void Execute(SendMqttMessage poll)
        {
            if (CheckTime(poll.Time) && poll.Channel.Active)
            {
                poll.Time=DateTime.Now.Ticks / 10000;
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

        protected void PubRelAck( SendMqttMessage mqttMessage)
        {
            PubRelPacket mqttPubAckMessage = new PubRelPacket()
            {
                PacketId = mqttMessage.MessageId,
            };
            mqttMessage.Channel.WriteAndFlushAsync(mqttPubAckMessage);
        }

        private void PubRecAck(SendMqttMessage mqttMessage)
        {
            PubRecPacket mqttPubAckMessage = new PubRecPacket()
            {
                PacketId = mqttMessage.MessageId,
            };
            mqttMessage.Channel.WriteAndFlushAsync(mqttPubAckMessage);
        }
    }
}
