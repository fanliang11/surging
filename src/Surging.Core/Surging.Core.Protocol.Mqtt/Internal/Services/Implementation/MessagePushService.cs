using DotNetty.Buffers;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Surging.Core.Protocol.Mqtt.Internal.Channel;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Runtime;
using Surging.Core.Protocol.Mqtt.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Services.Implementation
{
   public class MessagePushService
    {
        private readonly ScanRunnable _scanRunnable;
        public MessagePushService(ScanRunnable scanRunnable)
        {
            _scanRunnable = scanRunnable;
        }
        public void WriteWillMsg(MqttChannel mqttChannel, MqttWillMessage willMeaasge)
        {
            switch (willMeaasge.Qos)
            {
                case 0: 
                    SendQos0Msg(mqttChannel.Channel, willMeaasge.Topic,  Encoding.Default.GetBytes(willMeaasge.WillMessage));
                    break;
                case 1: // qos1
                    SendQosConfirmMsg(QualityOfService.AtLeastOnce, mqttChannel, willMeaasge.Topic, Encoding.Default.GetBytes(willMeaasge.WillMessage));
                    break;
                case 2: // qos2
                    SendQosConfirmMsg(QualityOfService.ExactlyOnce, mqttChannel, willMeaasge.Topic, Encoding.Default.GetBytes(willMeaasge.WillMessage));
                    break;
            } 
             
        }

        public void SendQosConfirmMsg(QualityOfService qos, MqttChannel mqttChannel, string topic, byte[] bytes)
        {
            if (mqttChannel.IsLogin())
            {
                int messageId = MessageIdGenerater.GenerateId();
                switch (qos)
                {
                    case QualityOfService.AtLeastOnce:
                        mqttChannel.AddMqttMessage(messageId, SendQos1Msg(mqttChannel.Channel, topic, false, bytes, messageId));
                        break;
                    case QualityOfService.ExactlyOnce:
                        mqttChannel.AddMqttMessage(messageId, SendQos2Msg(mqttChannel.Channel, topic, false, bytes, messageId));
                        break;
                }
            }

        }

        private SendMqttMessage SendQos1Msg(IChannel channel, String topic, bool isDup, byte[] byteBuf, int messageId)
        {
            var mqttPublishMessage = new PublishPacket(QualityOfService.AtLeastOnce, false, false);
            mqttPublishMessage.Payload = Unpooled.WrappedBuffer(byteBuf);
            channel.WriteAndFlushAsync(mqttPublishMessage);
             return Enqueue(channel, messageId, topic, byteBuf, (int)QualityOfService.AtLeastOnce, ConfirmStatus.PUB);
        }

        private SendMqttMessage SendQos2Msg(IChannel channel, String topic, bool isDup, byte[] byteBuf, int messageId)
        {
            var mqttPublishMessage = new PublishPacket(QualityOfService.ExactlyOnce, false, false);
            mqttPublishMessage.Payload = Unpooled.WrappedBuffer(byteBuf);
            channel.WriteAndFlushAsync(mqttPublishMessage);
             return Enqueue(channel, messageId, topic, byteBuf, (int)QualityOfService.AtLeastOnce, ConfirmStatus.PUB);
        }
        
        private void SendQos0Msg(IChannel channel, String topic, byte[] byteBuf, int messageId)
        {
            if (channel != null)
            {
                var mqttPublishMessage = new PublishPacket(QualityOfService.AtMostOnce, false, false);
                mqttPublishMessage.Payload = Unpooled.WrappedBuffer(byteBuf);
                channel.WriteAndFlushAsync(mqttPublishMessage);
            }
        }

        public void SendPubBack(IChannel channel, int messageId)
        {
            var mqttPubAckMessage = new PubAckPacket() {
                PacketId = messageId
            };
            channel.WriteAndFlushAsync(mqttPubAckMessage);
        }

        public void SendPubRec(MqttChannel mqttChannel, int messageId)
        {
            var mqttPubAckMessage = new PubRecPacket()
            {
                PacketId = messageId
            };
            var channel = mqttChannel.Channel;
            channel.WriteAndFlushAsync(mqttPubAckMessage);
            var sendMqttMessage = Enqueue(channel, messageId, null, null,1, ConfirmStatus.PUBREC);
            mqttChannel.AddMqttMessage(messageId, sendMqttMessage);
        }

         
        public void SendPubRel(IChannel channel, int messageId)
        {
            var mqttPubAckMessage = new PubRelPacket()
            {
                PacketId = messageId
            }; 
            channel.WriteAndFlushAsync(mqttPubAckMessage); 
        }
         
        protected void SendToPubComp(IChannel channel, int messageId)
        {
            var mqttPubAckMessage = new PubCompPacket()
            {
                PacketId = messageId
            };
            channel.WriteAndFlushAsync(mqttPubAckMessage);
        }


        public void SendQos0Msg(IChannel channel, String topic, byte[] byteBuf)
        {
            if (channel != null)
            {
                SendQos0Msg(channel, topic, byteBuf, 0);
            }
        }

        private SendMqttMessage Enqueue(IChannel channel, int messageId, String topic, byte[] datas, int mqttQoS, ConfirmStatus confirmStatus)
        {
            var  message = new SendMqttMessage
            {
                ByteBuf = datas,
                Channel = channel,
                MessageId = messageId,
                Qos = mqttQoS,
                Time = DateTime.Now.Ticks / 10000,
                ConfirmStatus = confirmStatus,
                Topic = topic
            };
            _scanRunnable.Enqueue(message);
            return message;
        }

    }
}
