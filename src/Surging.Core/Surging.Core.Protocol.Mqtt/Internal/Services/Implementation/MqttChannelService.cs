using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Transport.Channels;
using Surging.Core.Protocol.Mqtt.Internal.Channel;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System.Linq;
using System.Collections.Concurrent;
using DotNetty.Codecs.Mqtt.Packets;
using Microsoft.Extensions.Logging;
using DotNetty.Buffers;

namespace Surging.Core.Protocol.Mqtt.Internal.Services.Implementation
{
    public class MqttChannelService : AbstractChannelService
    { 
        private readonly IMessagePushService _messagePushService;
        private readonly IClientSessionService _clientSessionService;
        private readonly ILogger _logger;
        public MqttChannelService(IMessagePushService messagePushService, IClientSessionService clientSessionService, ILogger logger) : base(messagePushService)
        {
            _messagePushService = messagePushService;
            _clientSessionService = clientSessionService;
            _logger = logger;
        }

        public override void Close(string deviceId)
        {
            throw new NotImplementedException();
        }

        public override bool Connect(string deviceId, MqttChannel build)
        {
            throw new NotImplementedException();
        }

        public override void Login(IChannel channel, string deviceId, ConnectMessage mqttConnectMessage)
        {
            throw new NotImplementedException();
        }

        public override void Publish(IChannel channel, PublishPacket mqttPublishMessage)
        {
            MqttChannel mqttChannel = GetMqttChannel(this.GetDeviceId(channel));
            var buffer = mqttPublishMessage.Payload;
            byte[] bytes = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(bytes);
            int messageId = mqttPublishMessage.PacketId;
            if (channel.HasAttribute(LoginAttrKey) && mqttChannel != null)
            {
                bool isRetain;
                switch (mqttPublishMessage.QualityOfService)
                {
                    case QualityOfService.AtLeastOnce:
                        _messagePushService.SendPubBack(channel, messageId);
                        break;
                    case QualityOfService.ExactlyOnce:
                        Pubrec(mqttChannel, messageId);
                        break;
                }
                if ((isRetain = mqttPublishMessage.RetainRequested) && mqttPublishMessage.QualityOfService != QualityOfService.AtMostOnce)
                {
                    SaveRetain(mqttPublishMessage.TopicName,
                            new RetainMessage
                            {
                                ByteBuf = bytes,
                                QoS = (int)mqttPublishMessage.QualityOfService
                            }, false);
                }
                else if (mqttPublishMessage.RetainRequested && mqttPublishMessage.QualityOfService == QualityOfService.AtMostOnce)
                {
                    SaveRetain(mqttPublishMessage.TopicName,
                           new RetainMessage
                           {
                               ByteBuf = bytes,
                               QoS = (int)mqttPublishMessage.QualityOfService
                           }, true);
                }
                if (!mqttChannel.CheckRecevice(messageId))
                {
                    PushMessage(mqttPublishMessage.TopicName, (int)mqttPublishMessage.QualityOfService, bytes, isRetain);
                    mqttChannel.AddRecevice(messageId);
                }
            }
        }

        private void PushMessage(string topic, int qos, byte[] bytes, bool isRetain)
        {
            Topics.TryGetValue(topic, out IEnumerable<MqttChannel> mqttChannels);
            if (mqttChannels.Any())
            {
                foreach (var mqttChannel in mqttChannels)
                {
                    switch (mqttChannel.SessionStatus)
                    {
                        case SessionStatus.OPEN:
                            {
                                if (mqttChannel.IsActive())
                                {
                                    SendMessage(mqttChannel, qos,
                                       topic, bytes);
                                }
                                else
                                {
                                    if (!mqttChannel.CleanSession && !isRetain)
                                    {
                                        _clientSessionService.SaveMessage(mqttChannel.ClientId,
                                               new SessionMessage
                                               {
                                                   Message = bytes,
                                                   QoS = qos,
                                                   Topic = topic
                                               });
                                        break;
                                    }
                                }
                            }
                            break;
                        case SessionStatus.CLOSE:
                            _clientSessionService.SaveMessage(mqttChannel.ClientId,
                                             new SessionMessage
                                             {
                                                 Message = bytes,
                                                 QoS = qos,
                                                 Topic = topic
                                             });
                            break;
                    }
                }
            }
        }

        public override void Pubrec(MqttChannel channel, int messageId)
        {
            _messagePushService.SendPubRec(channel, messageId);
        }

        public override void Pubrel(IChannel channel, int messageId)
        {
            if (MqttChannels.TryGetValue(this.GetDeviceId(channel), out MqttChannel mqttChannel))
            {
                if (mqttChannel.IsLogin())
                {
                    mqttChannel.RemoveMqttMessage(messageId);
                    _messagePushService.SendToPubComp(channel, messageId);
                }
            }
        }

        public override void SendWillMsg(MqttWillMessage willMeaasge)
        {
            Topics.TryGetValue(willMeaasge.Topic, out IEnumerable<MqttChannel> mqttChannels);
            if (mqttChannels.Any())
            {
                foreach (var mqttChannel in mqttChannels)
                {
                    switch (mqttChannel.SessionStatus)
                    {
                        case SessionStatus.CLOSE:
                            _clientSessionService.SaveMessage(mqttChannel.ClientId, new SessionMessage
                            {
                                Message = Encoding.UTF8.GetBytes(willMeaasge.WillMessage),
                                QoS = willMeaasge.Qos,
                                Topic = willMeaasge.Topic
                            });
                            break;
                        case SessionStatus.OPEN:
                            _messagePushService.WriteWillMsg(mqttChannel, willMeaasge);
                            break;

                    }
                }
            }
        }

        public override void Suscribe(string deviceId, params string[] topics)
        {
            MqttChannels.TryGetValue(deviceId, out MqttChannel mqttChannel);
            mqttChannel.SubscribeStatus = SubscribeStatus.Yes;
            mqttChannel.AddTopic(topics);
            if (mqttChannel.IsLogin())
            {
                foreach (var topic in topics)
                {
                    this.AddChannel(topic, mqttChannel);
                    this.SendRetain(topic, mqttChannel);
                }
            }
        }

        public override void UnSubscribe(string deviceId, params string[] topics)
        {
            if (MqttChannels.TryGetValue(deviceId, out MqttChannel mqttChannel))
            {
                foreach (var topic in topics)
                {
                    RemoveChannel(topic, mqttChannel);
                }
            }
        }

        public void SendRetain(string topic, MqttChannel mqttChannel)
        {
            Retain.TryGetValue(topic, out ConcurrentQueue<RetainMessage> retainMessages);
            if (!retainMessages.IsEmpty)
            {
                var count = retainMessages.Count;
                for (int i = 0; i < count; i++)
                {
                    if (retainMessages.TryDequeue(out RetainMessage retainMessage))
                    {
                        SendMessage(mqttChannel, retainMessage.QoS, topic, retainMessage.ByteBuf);
                    }
                }
            }
        }

        private void SaveRetain(String topic, RetainMessage retainMessage, bool isClean)
        {
            Retain.TryGetValue(topic, out ConcurrentQueue<RetainMessage> retainMessages);
            if (!retainMessages.IsEmpty && isClean)
            {
                retainMessages.Clear();
            } 
            retainMessages.Enqueue(retainMessage);
            Retain.AddOrUpdate(topic, retainMessages,(key,value)=> retainMessages);
        }

        private void SendMessage(MqttChannel mqttChannel,int qos, string topic,byte [] byteBuf)
        {
            switch (qos)
            {
                case 0:
                    _messagePushService.SendQos0Msg(mqttChannel.Channel, topic, byteBuf);
                    break;
                case 1:
                    _messagePushService.SendQosConfirmMsg(QualityOfService.AtLeastOnce, mqttChannel, topic, byteBuf);
                    break;
                case 2:
                    _messagePushService.SendQosConfirmMsg(QualityOfService.ExactlyOnce, mqttChannel, topic, byteBuf);
                    break;
            }
        }
    }
}
