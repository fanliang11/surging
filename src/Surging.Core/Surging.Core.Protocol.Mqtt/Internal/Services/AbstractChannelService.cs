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
using DotNetty.Codecs.Mqtt.Packets;
using System.Threading.Tasks;
using Surging.Core.Protocol.Mqtt.Internal.Runtime;
using System.Net;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Ids;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    public abstract class AbstractChannelService : IChannelService
    {
        private readonly AttributeKey<string> _loginAttrKey = AttributeKey<string>.ValueOf("login");
        private readonly AttributeKey<string> _deviceIdAttrKey = AttributeKey<string>.ValueOf("deviceId");
        private readonly IMessagePushService _messagePushService;
        private readonly ConcurrentDictionary<string, IEnumerable<MqttChannel>> _topics = new ConcurrentDictionary<string, IEnumerable<MqttChannel>>();
        private readonly ConcurrentDictionary<string, MqttChannel> _mqttChannels = new ConcurrentDictionary<String, MqttChannel>();
        protected readonly  ConcurrentDictionary<String, ConcurrentQueue<RetainMessage>> _retain = new ConcurrentDictionary<String, ConcurrentQueue<RetainMessage>>();
        private readonly IMqttBrokerEntryManger _mqttBrokerEntryManger;
        private readonly IMqttRemoteInvokeService _mqttRemoteInvokeService;
        private readonly string _publishServiceId;

        public AbstractChannelService(IMessagePushService messagePushService,
            IMqttBrokerEntryManger mqttBrokerEntryManger,
            IMqttRemoteInvokeService mqttRemoteInvokeService,
            IServiceIdGenerator serviceIdGenerator
            )
        {
            _messagePushService = messagePushService;
            _mqttBrokerEntryManger = mqttBrokerEntryManger;
            _mqttRemoteInvokeService = mqttRemoteInvokeService;
            _publishServiceId= serviceIdGenerator.GenerateServiceId(typeof(IMqttRomtePublishService).GetMethod("Publish"));
        }

        public ConcurrentDictionary<string, MqttChannel> MqttChannels { get {
                return _mqttChannels;
            }
        }

        public AttributeKey<string> DeviceIdAttrKey
        {
            get
            {
                return _deviceIdAttrKey;
            }
        }

        public AttributeKey<string> LoginAttrKey
        {
            get
            {
                return _loginAttrKey;
            }
        }

        public ConcurrentDictionary<string, IEnumerable<MqttChannel>> Topics
        {
            get
            {
                return _topics;
            }
        }

        public ConcurrentDictionary<String, ConcurrentQueue<RetainMessage>> Retain
        {
            get
            {
                return _retain;
            }
        }

        public abstract Task Close(string deviceId, bool isDisconnect);

        public abstract bool Connect(string deviceId, MqttChannel build);

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

        public async ValueTask<string> GetDeviceId(IChannel channel)
        {
            string deviceId = null;
            if (channel != null)
            { 
                deviceId = channel.GetAttribute<string>(DeviceIdAttrKey).Get();
            }
            return await new ValueTask<string>(deviceId);
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

        protected async Task RegisterMqttBroker(string topic)
        {
            var addresses = await _mqttBrokerEntryManger.GetMqttBrokerAddress(topic);
            var host = NetUtils.GetHostAddress();
            if (addresses==null || !addresses.Any(p => p.ToString() == host.ToString()))
                await _mqttBrokerEntryManger.Register(topic, host);
        }

        protected async Task  BrokerCancellationReg(string topic)
        {
            if (Topics.ContainsKey(topic))
            {
                if (Topics["topic"].Count() == 0)
                    await _mqttBrokerEntryManger.CancellationReg(topic, NetUtils.GetHostAddress());
            }
            else
            {
                await _mqttBrokerEntryManger.CancellationReg(topic, NetUtils.GetHostAddress());
            }
        }

        public async Task RemotePublishMessage(string deviceId, MqttWillMessage willMessage)
        {
            await _mqttRemoteInvokeService.InvokeAsync(new MqttRemoteInvokeContext
            {
                topic = willMessage.Topic,
                InvokeMessage = new RemoteInvokeMessage
                {
                    ServiceId = _publishServiceId,
                    Parameters = new Dictionary<string, object>() {
                           {"deviceId",deviceId},
                           { "message",willMessage}
                       }
                },

            }, AppConfig.ServerOptions.ExecutionTimeoutInMilliseconds);
        }

        public abstract Task Login(IChannel channel, string deviceId, ConnectMessage mqttConnectMessage);

        public abstract Task Publish(IChannel channel, PublishPacket mqttPublishMessage);

        public abstract Task Pubrec(MqttChannel channel, int messageId);

        public abstract ValueTask PingReq(IChannel channel);

        public abstract Task Pubrel(IChannel channel, int messageId);

        public abstract Task SendWillMsg(MqttWillMessage willMeaasge);
        public abstract Task Suscribe(string deviceId, params string[] topics);

        public abstract Task UnSubscribe(string deviceId, params string[] topics);

        public abstract Task Publish(string deviceId, MqttWillMessage willMessage);

        public ValueTask<bool> GetDeviceIsOnine(string deviceId)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(deviceId))
            {
                MqttChannels.TryGetValue(deviceId, out MqttChannel mqttChannel);
                result = mqttChannel==null?false: mqttChannel.IsOnine();
            }
            return new ValueTask<bool>(result);
        }
    }
}
