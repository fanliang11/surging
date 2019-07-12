using DotNetty.Buffers;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Ids;
using Surging.Core.Protocol.Mqtt.Internal.Channel;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Services.Implementation
{
    /// <summary>
    /// Defines the <see cref="MqttChannelService" />
    /// </summary>
    public class MqttChannelService : AbstractChannelService
    {
        #region 字段

        /// <summary>
        /// Defines the _clientSessionService
        /// </summary>
        private readonly IClientSessionService _clientSessionService;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<MqttChannelService> _logger;

        /// <summary>
        /// Defines the _messagePushService
        /// </summary>
        private readonly IMessagePushService _messagePushService;

        /// <summary>
        /// Defines the _willService
        /// </summary>
        private readonly IWillService _willService;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttChannelService"/> class.
        /// </summary>
        /// <param name="messagePushService">The messagePushService<see cref="IMessagePushService"/></param>
        /// <param name="clientSessionService">The clientSessionService<see cref="IClientSessionService"/></param>
        /// <param name="logger">The logger<see cref="ILogger{MqttChannelService}"/></param>
        /// <param name="willService">The willService<see cref="IWillService"/></param>
        /// <param name="mqttBrokerEntryManger">The mqttBrokerEntryManger<see cref="IMqttBrokerEntryManger"/></param>
        /// <param name="mqttRemoteInvokeService">The mqttRemoteInvokeService<see cref="IMqttRemoteInvokeService"/></param>
        /// <param name="serviceIdGenerator">The serviceIdGenerator<see cref="IServiceIdGenerator"/></param>
        public MqttChannelService(IMessagePushService messagePushService, IClientSessionService clientSessionService,
            ILogger<MqttChannelService> logger, IWillService willService,
            IMqttBrokerEntryManger mqttBrokerEntryManger,
            IMqttRemoteInvokeService mqttRemoteInvokeService,
            IServiceIdGenerator serviceIdGenerator) :
            base(messagePushService,
                mqttBrokerEntryManger,
                mqttRemoteInvokeService,
                serviceIdGenerator)
        {
            _messagePushService = messagePushService;
            _clientSessionService = clientSessionService;
            _logger = logger;
            _willService = willService;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Close
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="isDisconnect">The isDisconnect<see cref="bool"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Close(string deviceId, bool isDisconnect)
        {
            if (!string.IsNullOrEmpty(deviceId))
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
                    mqttChannel.Topics.ForEach(async topic => { await BrokerCancellationReg(topic); });
                    if (mqttChannel.SubscribeStatus == SubscribeStatus.Yes)
                    {
                        RemoveSubTopic(mqttChannel);
                    }
                }
                if (mqttChannel.IsWill)
                {
                    if (!isDisconnect)
                    {
                        await _willService.SendWillMessage(deviceId);
                    }
                }
            }
        }

        /// <summary>
        /// The Connect
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="channel">The channel<see cref="MqttChannel"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public override async Task<bool> Connect(string deviceId, MqttChannel channel)
        {
            var mqttChannel = GetMqttChannel(deviceId);
            if (mqttChannel != null)
            {
                if (await mqttChannel.IsOnine()) return false;
                else
                {
                    if (mqttChannel.SubscribeStatus == SubscribeStatus.Yes)
                    {
                        var topics = RemoveSubTopic(mqttChannel);
                        foreach (var topic in topics)
                        {
                            Topics.TryGetValue(topic, out IEnumerable<MqttChannel> comparisonValue);
                            var newValue = comparisonValue.Concat(new[] { channel });
                            Topics.AddOrUpdate(topic, newValue, (key, value) => newValue);
                        }
                    }
                }
            }
            MqttChannels.AddOrUpdate(deviceId, channel, (k, v) => channel);
            return true;
        }

        /// <summary>
        /// The Login
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="mqttConnectMessage">The mqttConnectMessage<see cref="ConnectMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Login(IChannel channel, string deviceId, ConnectMessage mqttConnectMessage)
        {
            channel.GetAttribute(LoginAttrKey).Set("login");
            channel.GetAttribute(DeviceIdAttrKey).Set(deviceId);
            await Init(channel, mqttConnectMessage);
        }

        /// <summary>
        /// The PingReq
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <returns>The <see cref="ValueTask"/></returns>
        public override async ValueTask PingReq(IChannel channel)
        {
            if (MqttChannels.TryGetValue(await this.GetDeviceId(channel), out MqttChannel mqttChannel))
            {
                if (mqttChannel.IsLogin())
                {
                    mqttChannel.PingReqTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="mqttPublishMessage">The mqttPublishMessage<see cref="PublishPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Publish(IChannel channel, PublishPacket mqttPublishMessage)
        {
            MqttChannel mqttChannel = GetMqttChannel(await this.GetDeviceId(channel));
            var buffer = mqttPublishMessage.Payload;
            byte[] bytes = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(bytes);
            int messageId = mqttPublishMessage.PacketId;
            if (channel.HasAttribute(LoginAttrKey) && mqttChannel != null)
            {
                bool isRetain = mqttPublishMessage.RetainRequested;
                switch (mqttPublishMessage.QualityOfService)
                {
                    case QualityOfService.AtLeastOnce:
                        await _messagePushService.SendPubBack(channel, messageId);
                        break;

                    case QualityOfService.ExactlyOnce:
                        await Pubrec(mqttChannel, messageId);
                        break;
                }
                if (isRetain)
                {
                    SaveRetain(mqttPublishMessage.TopicName,
                           new RetainMessage
                           {
                               ByteBuf = bytes,
                               QoS = (int)mqttPublishMessage.QualityOfService
                           }, mqttPublishMessage.QualityOfService == QualityOfService.AtMostOnce ? true : false);
                }
                await PushMessage(mqttPublishMessage.TopicName, (int)mqttPublishMessage.QualityOfService, bytes, isRetain);
                await RemotePublishMessage("", new MqttWillMessage
                {
                    Qos = (int)mqttPublishMessage.QualityOfService,
                    Topic = mqttPublishMessage.TopicName,
                    WillMessage = Encoding.Default.GetString(bytes),
                    WillRetain = mqttPublishMessage.RetainRequested
                });
            }
        }

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="willMessage">The willMessage<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Publish(string deviceId, MqttWillMessage willMessage)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                var mqttChannel = GetMqttChannel(deviceId);
                if (mqttChannel != null && await mqttChannel.IsOnine())
                {
                    await _messagePushService.WriteWillMsg(mqttChannel, willMessage);
                }
            }
            else { await SendWillMsg(willMessage); }
            if (willMessage.WillRetain)
                SaveRetain(willMessage.Topic, new RetainMessage
                {
                    ByteBuf = Encoding.UTF8.GetBytes(willMessage.WillMessage),
                    QoS = willMessage.Qos
                }, willMessage.Qos == 0 ? true : false);
        }

        /// <summary>
        /// The Pubrec
        /// </summary>
        /// <param name="channel">The channel<see cref="MqttChannel"/></param>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Pubrec(MqttChannel channel, int messageId)
        {
            await _messagePushService.SendPubRec(channel, messageId);
        }

        /// <summary>
        /// The Pubrel
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Pubrel(IChannel channel, int messageId)
        {
            if (MqttChannels.TryGetValue(await this.GetDeviceId(channel), out MqttChannel mqttChannel))
            {
                if (mqttChannel.IsLogin())
                {
                    mqttChannel.RemoveMqttMessage(messageId);
                    await _messagePushService.SendToPubComp(channel, messageId);
                }
            }
        }

        /// <summary>
        /// The RemoveSubTopic
        /// </summary>
        /// <param name="mqttChannel">The mqttChannel<see cref="MqttChannel"/></param>
        /// <returns>The <see cref="IEnumerable{String}"/></returns>
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

        /// <summary>
        /// The SendRetain
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="mqttChannel">The mqttChannel<see cref="MqttChannel"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendRetain(string topic, MqttChannel mqttChannel)
        {
            Retain.TryGetValue(topic, out ConcurrentQueue<RetainMessage> retainMessages);
            if (retainMessages != null && !retainMessages.IsEmpty)
            {
                var messages = retainMessages.GetEnumerator();
                while (messages.MoveNext())
                {
                    var retainMessage = messages.Current;
                    await SendMessage(mqttChannel, retainMessage.QoS, topic, retainMessage.ByteBuf);
                };
            }
        }

        /// <summary>
        /// The SendWillMsg
        /// </summary>
        /// <param name="willMeaasge">The willMeaasge<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task SendWillMsg(MqttWillMessage willMeaasge)
        {
            Topics.TryGetValue(willMeaasge.Topic, out IEnumerable<MqttChannel> mqttChannels);
            if (mqttChannels != null && mqttChannels.Any())
            {
                foreach (var mqttChannel in mqttChannels)
                {
                    if (!await mqttChannel.IsOnine())
                    {
                        _clientSessionService.SaveMessage(mqttChannel.ClientId, new SessionMessage
                        {
                            Message = Encoding.UTF8.GetBytes(willMeaasge.WillMessage),
                            QoS = willMeaasge.Qos,
                            Topic = willMeaasge.Topic
                        });
                    }
                    else
                    {
                        await _messagePushService.WriteWillMsg(mqttChannel, willMeaasge);
                    }
                }
            }
        }

        /// <summary>
        /// The Suscribe
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="topics">The topics<see cref="string[]"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Suscribe(string deviceId, params string[] topics)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                MqttChannels.TryGetValue(deviceId, out MqttChannel mqttChannel);
                mqttChannel.SubscribeStatus = SubscribeStatus.Yes;
                mqttChannel.AddTopic(topics);
                if (mqttChannel.IsLogin())
                {
                    foreach (var topic in topics)
                    {
                        this.AddChannel(topic, mqttChannel);
                        await RegisterMqttBroker(topic);
                        await this.SendRetain(topic, mqttChannel);
                    }
                }
            }
        }

        /// <summary>
        /// The UnSubscribe
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="topics">The topics<see cref="string[]"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task UnSubscribe(string deviceId, params string[] topics)
        {
            if (MqttChannels.TryGetValue(deviceId, out MqttChannel mqttChannel))
            {
                foreach (var topic in topics)
                {
                    RemoveChannel(topic, mqttChannel);
                    await BrokerCancellationReg(topic);
                }
            }
        }

        /// <summary>
        /// The Init
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="mqttConnectMessage">The mqttConnectMessage<see cref="ConnectMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task Init(IChannel channel, ConnectMessage mqttConnectMessage)
        {
            String deviceId = await GetDeviceId(channel);
            MqttChannel mqttChannel = new MqttChannel()
            {
                Channel = channel,
                KeepAliveInSeconds = mqttConnectMessage.KeepAliveInSeconds,
                CleanSession = mqttConnectMessage.CleanSession,
                ClientId = mqttConnectMessage.ClientId,
                SessionStatus = SessionStatus.OPEN,
                IsWill = mqttConnectMessage.HasWill,
                SubscribeStatus = SubscribeStatus.No,
                Messages = new ConcurrentDictionary<int, SendMqttMessage>(),
                Topics = new List<string>()
            };
            if (await Connect(deviceId, mqttChannel))
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
                    if (!mqttConnectMessage.WillRetain && mqttConnectMessage.WillQualityOfService != 0)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                            _logger.LogError($"WillRetain 设置为false,WillQos必须设置为AtMostOnce");
                        return;
                    }
                }
                await channel.WriteAndFlushAsync(new ConnAckPacket
                {
                    ReturnCode = ConnectReturnCode.Accepted,
                    SessionPresent = !mqttConnectMessage.CleanSession
                });
                var sessionMessages = _clientSessionService.GetMessages(mqttConnectMessage.ClientId);
                if (sessionMessages != null && !sessionMessages.IsEmpty)
                {
                    for (; sessionMessages.TryDequeue(out SessionMessage sessionMessage) && sessionMessage != null;)
                    {
                        switch (sessionMessage.QoS)
                        {
                            case 0:
                                await _messagePushService.SendQos0Msg(channel, sessionMessage.Topic, sessionMessage.Message);
                                break;

                            case 1:
                                await _messagePushService.SendQosConfirmMsg(QualityOfService.AtLeastOnce, GetMqttChannel(deviceId), sessionMessage.Topic, sessionMessage.Message);
                                break;

                            case 2:
                                await _messagePushService.SendQosConfirmMsg(QualityOfService.ExactlyOnce, GetMqttChannel(deviceId), sessionMessage.Topic, sessionMessage.Message);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The PushMessage
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="qos">The qos<see cref="int"/></param>
        /// <param name="bytes">The bytes<see cref="byte[]"/></param>
        /// <param name="isRetain">The isRetain<see cref="bool"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task PushMessage(string topic, int qos, byte[] bytes, bool isRetain)
        {
            Topics.TryGetValue(topic, out IEnumerable<MqttChannel> mqttChannels);
            if (mqttChannels != null && mqttChannels.Any())
            {
                foreach (var mqttChannel in mqttChannels)
                {
                    if (await mqttChannel.IsOnine())
                    {
                        if (mqttChannel.IsActive())
                        {
                            await SendMessage(mqttChannel, qos,
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
                    else
                    {
                        _clientSessionService.SaveMessage(mqttChannel.ClientId,
                                         new SessionMessage
                                         {
                                             Message = bytes,
                                             QoS = qos,
                                             Topic = topic
                                         });
                    }
                }
            }
        }

        /// <summary>
        /// The SaveRetain
        /// </summary>
        /// <param name="topic">The topic<see cref="String"/></param>
        /// <param name="retainMessage">The retainMessage<see cref="RetainMessage"/></param>
        /// <param name="isClean">The isClean<see cref="bool"/></param>
        private void SaveRetain(String topic, RetainMessage retainMessage, bool isClean)
        {
            Retain.TryGetValue(topic, out ConcurrentQueue<RetainMessage> retainMessages);
            if (retainMessages == null) retainMessages = new ConcurrentQueue<RetainMessage>();
            if (!retainMessages.IsEmpty && isClean)
            {
                retainMessages.Clear();
            }
            retainMessages.Enqueue(retainMessage);
            Retain.AddOrUpdate(topic, retainMessages, (key, value) => retainMessages);
        }

        /// <summary>
        /// The SendMessage
        /// </summary>
        /// <param name="mqttChannel">The mqttChannel<see cref="MqttChannel"/></param>
        /// <param name="qos">The qos<see cref="int"/></param>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="byteBuf">The byteBuf<see cref="byte []"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task SendMessage(MqttChannel mqttChannel, int qos, string topic, byte[] byteBuf)
        {
            switch (qos)
            {
                case 0:
                    await _messagePushService.SendQos0Msg(mqttChannel.Channel, topic, byteBuf);
                    break;

                case 1:
                    await _messagePushService.SendQosConfirmMsg(QualityOfService.AtLeastOnce, mqttChannel, topic, byteBuf);
                    break;

                case 2:
                    await _messagePushService.SendQosConfirmMsg(QualityOfService.ExactlyOnce, mqttChannel, topic, byteBuf);
                    break;
            }
        }

        #endregion 方法
    }
}