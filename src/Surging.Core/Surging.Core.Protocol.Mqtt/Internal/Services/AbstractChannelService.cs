using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Surging.Core.Protocol.Mqtt.Internal.Channel;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System.Collections;
using System.Linq;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    public abstract class AbstractChannelService : IChannelService
    {
        private readonly IMessagePushService _messagePushService;
        protected static ConcurrentDictionary<string, IEnumerable<MqttChannel>> _topics = new ConcurrentDictionary<string, IEnumerable<MqttChannel>>();
        private readonly ConcurrentDictionary<string, MqttChannel> _mqttChannels = new ConcurrentDictionary<String, MqttChannel>();

        public AbstractChannelService(IMessagePushService messagePushService)
        {
            _messagePushService = messagePushService;
        }

        public abstract void Close(string deviceId);

        public abstract bool Connect(string s, MqttChannel build);

        public bool RemoveChannel(string topic, MqttChannel mqttChannel)
        {
            var result = false;
            if (!string.IsNullOrEmpty(topic) && mqttChannel != null)
            {
                _topics.TryGetValue(topic, out IEnumerable<MqttChannel> mqttChannels);
                var channels = mqttChannels == null ? new List<MqttChannel>() : mqttChannels.ToList();
                channels.Remove(mqttChannel);
                _topics.AddOrUpdate(topic, channels, (key, value) => channels);
                result = true;
            }
            return result;
        }

        public string GetDeviceId(IChannel channel)
        {
            string deviceId = null;
            if (channel != null)
            {
                AttributeKey<string> deviceIdAttrKey = AttributeKey<string>.ValueOf("deviceId");
                deviceId = channel.GetAttribute<string>(deviceIdAttrKey).Get();
            }
            return deviceId;
        }

        public bool AddChannel(string topic, MqttChannel mqttChannel)
        {
            var result = false;
            if (!string.IsNullOrEmpty(topic) && mqttChannel != null)
            {
                _topics.TryGetValue(topic, out IEnumerable<MqttChannel> mqttChannels);
                var channels = mqttChannels==null ? new List<MqttChannel>(): mqttChannels.ToList();
                channels.Add(mqttChannel);
                _topics.AddOrUpdate(topic, channels, (key, value) => channels);
                result = true; 
            }
            return result;
        }

        public MqttChannel GetMqttChannel(string deviceId)
        {
            MqttChannel channel = null;
            if (!string.IsNullOrEmpty(deviceId))
            {
                _mqttChannels.TryGetValue(deviceId, out channel);
            }
            return channel;
        }

        public abstract void Login(IChannel channel, string deviceId, ConnectMessage mqttConnectMessage);

        public abstract void Publish(IChannel channel, PublishMessage mqttPublishMessage);

        public abstract  void Pubrec(IChannel channel, int messageId);

        public abstract void Pubrel(IChannel channel, int messageId);

        public abstract void SendWillMsg(MqttWillMessage willMeaasge);
        public abstract  void Suscribe(string deviceId, params string[] topics);

        public abstract void UnSubscribe(string deviceId, params string[] topics);

    }
}
