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
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Services.Implementation
{
    public class MqttChannelService : AbstractChannelService
    { 
        private readonly IMessagePushService _messagePushService;
        private readonly IClientSessionService _clientSessionService;
        private readonly ILogger _logger;
        private readonly IWillService _willService;
        public MqttChannelService(IMessagePushService messagePushService, IClientSessionService clientSessionService, 
            ILogger logger, IWillService willService) : base(messagePushService)
        {
            _messagePushService = messagePushService;
            _clientSessionService = clientSessionService;
            _logger = logger;
            _willService = willService;
        }

        public override async Task Close(string deviceId, bool isDisconnect)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                MqttChannels.TryGetValue(deviceId, out MqttChannel mqttChannel);
                if (mqttChannel != null)
                {
                    mqttChannel.SessionStatus = SessionStatus.CLOSE;
                    await mqttChannel.Close(); 
                    mqttChannel.Channel = null;
                }
                if (!mqttChannel.CleanSession)
                {
                    var messages = mqttChannel.Messages;
                    if (messages != null)
                    {
                        foreach (var sendMqttMessage in messages.Values)
                        {
                            if (sendMqttMessage.ConfirmStatus == ConfirmStatus.PUB)
                            {
                                _clientSessionService.SaveMessage(mqttChannel.ClientId, new SessionMessage
                                {
                                    Message = sendMqttMessage.ByteBuf,
                                    QoS = sendMqttMessage.Qos,
                                    Topic = sendMqttMessage.Topic
                                });
                            }
                        }
                    }
                }
                else
                {
                    MqttChannels.TryRemove(deviceId, out MqttChannel channel);
                    if (mqttChannel.SubscribeStatus == SubscribeStatus.Yes)
                    {
                        RemoveSubTopic(mqttChannel);
                    }
                }
                if (mqttChannel.IsWill)
                {     
                    if (!isDisconnect)
                    {  
                        _willService.SendWillMessage(deviceId);
                    }
                }

            }
        }
        
        public override bool Connect(string deviceId, MqttChannel channel)
        {
            var mqttChannel = GetMqttChannel(deviceId);
            if (mqttChannel != null)
            {
                if (mqttChannel.SessionStatus == SessionStatus.OPEN) return false;
                else if (mqttChannel.SessionStatus == SessionStatus.CLOSE)
                {
                    if (mqttChannel.SubscribeStatus == SubscribeStatus.Yes)
                    {
                        var topics = RemoveSubTopic(mqttChannel);
                        foreach (var topic in topics)
                        {
                            Topics.TryGetValue(topic, out IEnumerable<MqttChannel> comparisonValue);
                            var newValue = comparisonValue.Concat(new [] { channel });
                            Topics.AddOrUpdate(topic, newValue, (key, value) => newValue);
                        }
                    }

                }
            }
            MqttChannels.AddOrUpdate(deviceId, channel, (k, v) => channel);
            return true;
        }

        public override void Login(IChannel channel, string deviceId, ConnectMessage mqttConnectMessage)
        {
            channel.GetAttribute(LoginAttrKey).Set("login");
            channel.GetAttribute(DeviceIdAttrKey).Set(deviceId);
            Init(channel,mqttConnectMessage);
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
                bool isRetain= mqttPublishMessage.RetainRequested;
                switch (mqttPublishMessage.QualityOfService)
                {
                    case QualityOfService.AtLeastOnce:
                        _messagePushService.SendPubBack(channel, messageId);
                        break;
                    case QualityOfService.ExactlyOnce:
                        Pubrec(mqttChannel, messageId);
                        break;
                }
                 if (isRetain)
                {
                    SaveRetain(mqttPublishMessage.TopicName,
                           new RetainMessage
                           {
                               ByteBuf = bytes,
                               QoS = (int)mqttPublishMessage.QualityOfService
                           }, mqttPublishMessage.QualityOfService == QualityOfService.AtMostOnce?true:false);
                }
                if (!mqttChannel.CheckRecevice(messageId))
                {
                    PushMessage(mqttPublishMessage.TopicName, (int)mqttPublishMessage.QualityOfService, bytes, isRetain);
                    mqttChannel.AddRecevice(messageId);
                }
            }
        }

        public override void Publish(string deviceId, MqttWillMessage willMessage)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                var mqttChannel = GetMqttChannel(deviceId);
                if (mqttChannel.SessionStatus == SessionStatus.OPEN)
                {
                    _messagePushService.WriteWillMsg(mqttChannel, willMessage);
                }
            }
            else { SendWillMsg(willMessage); }
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

        public IEnumerable<String> RemoveSubTopic(MqttChannel mqttChannel)
        {
            IEnumerable<String> topics = mqttChannel.Topics;
            foreach (var topic in topics)
            {
                Topics.TryGetValue(topic, out IEnumerable<MqttChannel> comparisonValue);
                var newValue = comparisonValue.Where(p => p != mqttChannel);
                Topics.TryUpdate(topic, newValue, comparisonValue);
            }
            return topics;
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

        private void Init(IChannel channel, ConnectMessage mqttConnectMessage)
        {
            String deviceId = GetDeviceId(channel);
            MqttChannel mqttChannel = new MqttChannel()
            {
                Channel = channel,
                CleanSession = mqttConnectMessage.CleanSession,
                ClientId = mqttConnectMessage.ClientId,
                SessionStatus = SessionStatus.OPEN,
                IsWill = mqttConnectMessage.HasWill,
                SubscribeStatus = SubscribeStatus.No,
                Messages = new ConcurrentDictionary<int, SendMqttMessage>(),
                Receives = new List<int>(),
                Topics = new List<string>()
            };
            if (Connect(deviceId, mqttChannel))
            {
                if (mqttConnectMessage.HasWill)
                {
                    if (mqttConnectMessage.WillMessage == null || string.IsNullOrEmpty(mqttConnectMessage.WillTopic))
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                            _logger.LogError($"WillMessage 和 WillTopic不能为空");
                        return;
                    }
                    var willMessage = new MqttWillMessage
                    {
                        Qos = mqttConnectMessage.Qos,
                        WillRetain = mqttConnectMessage.WillRetain,
                        Topic = mqttConnectMessage.WillTopic,
                        WillMessage = Encoding.UTF8.GetString(mqttConnectMessage.WillMessage)

                    };
                    _willService.Add(mqttConnectMessage.ClientId, willMessage);
                }
                else
                {
                    _willService.Remove(mqttConnectMessage.ClientId);
                    if (!mqttConnectMessage.WillRetain && mqttConnectMessage.WillQualityOfService == 0)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                            _logger.LogError($"WillRetain 设置为false,WillQos必须设置为AtMostOnce");
                        return;
                    }
                }
                if (mqttConnectMessage.CleanSession)
                {
                    channel.WriteAndFlushAsync(new ConnAckPacket
                    {
                        ReturnCode = ConnectReturnCode.Accepted,
                        SessionPresent = false
                    });
                }
                else
                {
                    channel.WriteAndFlushAsync(new ConnAckPacket
                    {
                        ReturnCode = ConnectReturnCode.Accepted,
                        SessionPresent = true
                    });
                }
                var sessionMessages = _clientSessionService.GetMessages(mqttConnectMessage.ClientId);
                if (sessionMessages != null && !sessionMessages.IsEmpty)
                {
                    for (; sessionMessages.TryDequeue(out SessionMessage sessionMessage) && sessionMessage != null;)
                    {
                        switch (sessionMessage.QoS)
                        {
                            case 0:
                                _messagePushService.SendQos0Msg(channel, sessionMessage.Topic, sessionMessage.Message);
                                break;
                            case 1:
                                _messagePushService.SendQosConfirmMsg(QualityOfService.AtLeastOnce, GetMqttChannel(deviceId), sessionMessage.Topic, sessionMessage.Message);
                                break;
                            case 2:
                                _messagePushService.SendQosConfirmMsg(QualityOfService.ExactlyOnce, GetMqttChannel(deviceId), sessionMessage.Topic, sessionMessage.Message);
                                break;
                        }
                    }
                }
            }
        }
    }
}
