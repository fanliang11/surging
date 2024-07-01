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
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Services.Implementation
{
   public class MessagePushService:IMessagePushService
    {
        private readonly ScanRunnable _scanRunnable;
        public MessagePushService(ScanRunnable scanRunnable)
        {
            _scanRunnable = scanRunnable;
        }
        public async Task WriteWillMsg(MqttChannel mqttChannel, MqttWillMessage willMeaasge)
        {
            switch (willMeaasge.Qos)
            {
                case 0: 
                    await SendQos0Msg(mqttChannel.Channel, willMeaasge.Topic,  Encoding.Default.GetBytes(willMeaasge.WillMessage));
                    break;
                case 1: // qos1
                    await SendQosConfirmMsg(QualityOfService.AtLeastOnce, mqttChannel, willMeaasge.Topic, Encoding.Default.GetBytes(willMeaasge.WillMessage));
                    break;
                case 2: // qos2
                    await SendQosConfirmMsg(QualityOfService.ExactlyOnce, mqttChannel, willMeaasge.Topic, Encoding.Default.GetBytes(willMeaasge.WillMessage));
                    break;
            } 
             
        }

        public async Task SendQosConfirmMsg(QualityOfService qos, MqttChannel mqttChannel, string topic, byte[] bytes)
        {
            if (mqttChannel.IsLogin())
            {
                int messageId = MessageIdGenerater.GenerateId();
                switch (qos)
                {
                    case QualityOfService.AtLeastOnce:
                        mqttChannel.AddMqttMessage(messageId, await SendQos1Msg(mqttChannel.Channel, topic, false, bytes, messageId));
                        break;
                    case QualityOfService.ExactlyOnce:
                        mqttChannel.AddMqttMessage(messageId,await SendQos2Msg(mqttChannel.Channel, topic, false, bytes, messageId));
                        break;
                }
            }

        }

        private async Task<SendMqttMessage> SendQos1Msg(IChannel channel, String topic, bool isDup, byte[] byteBuf, int messageId)
        {
            var mqttPublishMessage = new PublishPacket(QualityOfService.AtMostOnce, true, true);
            mqttPublishMessage.TopicName = topic;
            mqttPublishMessage.PacketId = messageId;
            mqttPublishMessage.Payload = Unpooled.WrappedBuffer(byteBuf);
            await channel.WriteAndFlushAsync(mqttPublishMessage);
            return Enqueue(channel, messageId, topic, byteBuf, (int)QualityOfService.AtLeastOnce, ConfirmStatus.PUB);
        }

        private async Task<SendMqttMessage> SendQos2Msg(IChannel channel, String topic, bool isDup, byte[] byteBuf, int messageId)
        {
            var mqttPublishMessage = new PublishPacket(QualityOfService.AtMostOnce, true, true);
            mqttPublishMessage.TopicName = topic;
            mqttPublishMessage.PacketId = messageId;
            mqttPublishMessage.Payload = Unpooled.WrappedBuffer(byteBuf);
            await channel.WriteAndFlushAsync(mqttPublishMessage);
            return Enqueue(channel, messageId, topic, byteBuf, (int)QualityOfService.ExactlyOnce, ConfirmStatus.PUB);
        }
        
        private async Task SendQos0Msg(IChannel channel, String topic, byte[] byteBuf, int messageId)
        {
            if (channel != null)
            {
                var mqttPublishMessage = new PublishPacket(QualityOfService.AtMostOnce, true, true);
                mqttPublishMessage.TopicName = topic;
                mqttPublishMessage.Payload = Unpooled.WrappedBuffer(byteBuf);
                await channel.WriteAndFlushAsync(mqttPublishMessage);
            }
        }

        public async Task SendPubBack(IChannel channel, int messageId)
        {
            var mqttPubAckMessage = new PubAckPacket() {
                PacketId = messageId
            };
           await channel.WriteAndFlushAsync(mqttPubAckMessage);
        }

        public async Task SendPubRec(MqttChannel mqttChannel, int messageId)
        {
            var mqttPubAckMessage = new PubRecPacket()
            {
                PacketId = messageId
            };
            var channel = mqttChannel.Channel;
            await channel.WriteAndFlushAsync(mqttPubAckMessage);
            var sendMqttMessage = Enqueue(channel, messageId, null, null, 1, ConfirmStatus.PUBREC);
            mqttChannel.AddMqttMessage(messageId, sendMqttMessage);
        }
        
        public async Task SendPubRel(IChannel channel, int messageId)
        {
            var mqttPubAckMessage = new PubRelPacket()
            {
                PacketId = messageId
            }; 
           await channel.WriteAndFlushAsync(mqttPubAckMessage); 
        }
         
        public async Task SendToPubComp(IChannel channel, int messageId)
        {
            var mqttPubAckMessage = new PubCompPacket()
            {
                PacketId = messageId
            };
           await channel.WriteAndFlushAsync(mqttPubAckMessage);
        }


        public async Task SendQos0Msg(IChannel channel, String topic, byte[] byteBuf)
        {
            if (channel != null)
            {
               await SendQos0Msg(channel, topic, byteBuf, 0);
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
