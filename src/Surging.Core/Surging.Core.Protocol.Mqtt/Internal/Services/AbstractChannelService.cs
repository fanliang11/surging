using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Ids;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.Mqtt.Internal.Channel;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Runtime;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    /// <summary>
    /// Defines the <see cref="AbstractChannelService" />
    /// </summary>
    public abstract class AbstractChannelService : IChannelService
    {
        #region 字段

        /// <summary>
        /// Defines the _retain
        /// </summary>
        protected readonly ConcurrentDictionary<String, ConcurrentQueue<RetainMessage>> _retain = new ConcurrentDictionary<String, ConcurrentQueue<RetainMessage>>();

        /// <summary>
        /// Defines the _deviceIdAttrKey
        /// </summary>
        private readonly AttributeKey<string> _deviceIdAttrKey = AttributeKey<string>.ValueOf("deviceId");

        /// <summary>
        /// Defines the _loginAttrKey
        /// </summary>
        private readonly AttributeKey<string> _loginAttrKey = AttributeKey<string>.ValueOf("login");

        /// <summary>
        /// Defines the _messagePushService
        /// </summary>
        private readonly IMessagePushService _messagePushService;

        /// <summary>
        /// Defines the _mqttBrokerEntryManger
        /// </summary>
        private readonly IMqttBrokerEntryManger _mqttBrokerEntryManger;

        /// <summary>
        /// Defines the _mqttChannels
        /// </summary>
        private readonly ConcurrentDictionary<string, MqttChannel> _mqttChannels = new ConcurrentDictionary<String, MqttChannel>();

        /// <summary>
        /// Defines the _mqttRemoteInvokeService
        /// </summary>
        private readonly IMqttRemoteInvokeService _mqttRemoteInvokeService;

        /// <summary>
        /// Defines the _publishServiceId
        /// </summary>
        private readonly string _publishServiceId;

        /// <summary>
        /// Defines the _topics
        /// </summary>
        private readonly ConcurrentDictionary<string, IEnumerable<MqttChannel>> _topics = new ConcurrentDictionary<string, IEnumerable<MqttChannel>>();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractChannelService"/> class.
        /// </summary>
        /// <param name="messagePushService">The messagePushService<see cref="IMessagePushService"/></param>
        /// <param name="mqttBrokerEntryManger">The mqttBrokerEntryManger<see cref="IMqttBrokerEntryManger"/></param>
        /// <param name="mqttRemoteInvokeService">The mqttRemoteInvokeService<see cref="IMqttRemoteInvokeService"/></param>
        /// <param name="serviceIdGenerator">The serviceIdGenerator<see cref="IServiceIdGenerator"/></param>
        public AbstractChannelService(IMessagePushService messagePushService,
            IMqttBrokerEntryManger mqttBrokerEntryManger,
            IMqttRemoteInvokeService mqttRemoteInvokeService,
            IServiceIdGenerator serviceIdGenerator
            )
        {
            _messagePushService = messagePushService;
            _mqttBrokerEntryManger = mqttBrokerEntryManger;
            _mqttRemoteInvokeService = mqttRemoteInvokeService;
            _publishServiceId = serviceIdGenerator.GenerateServiceId(typeof(IMqttRomtePublishService).GetMethod("Publish"));
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the DeviceIdAttrKey
        /// </summary>
        public AttributeKey<string> DeviceIdAttrKey
        {
            get
            {
                return _deviceIdAttrKey;
            }
        }

        /// <summary>
        /// Gets the LoginAttrKey
        /// </summary>
        public AttributeKey<string> LoginAttrKey
        {
            get
            {
                return _loginAttrKey;
            }
        }

        /// <summary>
        /// Gets the MqttChannels
        /// </summary>
        public ConcurrentDictionary<string, MqttChannel> MqttChannels
        {
            get
            {
                return _mqttChannels;
            }
        }

        /// <summary>
        /// Gets the Retain
        /// </summary>
        public ConcurrentDictionary<String, ConcurrentQueue<RetainMessage>> Retain
        {
            get
            {
                return _retain;
            }
        }

        /// <summary>
        /// Gets the Topics
        /// </summary>
        public ConcurrentDictionary<string, IEnumerable<MqttChannel>> Topics
        {
            get
            {
                return _topics;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The AddChannel
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="mqttChannel">The mqttChannel<see cref="MqttChannel"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool AddChannel(string topic, MqttChannel mqttChannel)
        {
            var result = false;
            if (!string.IsNullOrEmpty(topic) && mqttChannel != null)
            {
                _topics.TryGetValue(topic, out IEnumerable<MqttChannel> mqttChannels);
                var channels = mqttChannels == null ? new List<MqttChannel>() : mqttChannels.ToList();
                channels.Add(mqttChannel);
                _topics.AddOrUpdate(topic, channels, (key, value) => channels);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// The Close
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="isDisconnect">The isDisconnect<see cref="bool"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Close(string deviceId, bool isDisconnect);

        /// <summary>
        /// The Connect
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="build">The build<see cref="MqttChannel"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public abstract Task<bool> Connect(string deviceId, MqttChannel build);

        /// <summary>
        /// The GetDeviceId
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <returns>The <see cref="ValueTask{string}"/></returns>
        public async ValueTask<string> GetDeviceId(IChannel channel)
        {
            string deviceId = null;
            if (channel != null)
            {
                deviceId = channel.GetAttribute<string>(DeviceIdAttrKey).Get();
            }
            return await new ValueTask<string>(deviceId);
        }

        /// <summary>
        /// The GetDeviceIsOnine
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <returns>The <see cref="ValueTask{bool}"/></returns>
        public async ValueTask<bool> GetDeviceIsOnine(string deviceId)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(deviceId))
            {
                MqttChannels.TryGetValue(deviceId, out MqttChannel mqttChannel);
                result = mqttChannel == null ? false : await mqttChannel.IsOnine();
            }
            return result;
        }

        /// <summary>
        /// The GetMqttChannel
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <returns>The <see cref="MqttChannel"/></returns>
        public MqttChannel GetMqttChannel(string deviceId)
        {
            MqttChannel channel = null;
            if (!string.IsNullOrEmpty(deviceId))
            {
                _mqttChannels.TryGetValue(deviceId, out channel);
            }
            return channel;
        }

        /// <summary>
        /// The Login
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="mqttConnectMessage">The mqttConnectMessage<see cref="ConnectMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Login(IChannel channel, string deviceId, ConnectMessage mqttConnectMessage);

        /// <summary>
        /// The PingReq
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <returns>The <see cref="ValueTask"/></returns>
        public abstract ValueTask PingReq(IChannel channel);

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="mqttPublishMessage">The mqttPublishMessage<see cref="PublishPacket"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Publish(IChannel channel, PublishPacket mqttPublishMessage);

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="willMessage">The willMessage<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Publish(string deviceId, MqttWillMessage willMessage);

        /// <summary>
        /// The Pubrec
        /// </summary>
        /// <param name="channel">The channel<see cref="MqttChannel"/></param>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Pubrec(MqttChannel channel, int messageId);

        /// <summary>
        /// The Pubrel
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Pubrel(IChannel channel, int messageId);

        /// <summary>
        /// The RemotePublishMessage
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="willMessage">The willMessage<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        /// <summary>
        /// The RemoveChannel
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="mqttChannel">The mqttChannel<see cref="MqttChannel"/></param>
        /// <returns>The <see cref="bool"/></returns>
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

        /// <summary>
        /// The SendWillMsg
        /// </summary>
        /// <param name="willMeaasge">The willMeaasge<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task SendWillMsg(MqttWillMessage willMeaasge);

        /// <summary>
        /// The Suscribe
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="topics">The topics<see cref="string[]"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task Suscribe(string deviceId, params string[] topics);

        /// <summary>
        /// The UnSubscribe
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="topics">The topics<see cref="string[]"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public abstract Task UnSubscribe(string deviceId, params string[] topics);

        /// <summary>
        /// The BrokerCancellationReg
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        protected async Task BrokerCancellationReg(string topic)
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

        /// <summary>
        /// The RegisterMqttBroker
        /// </summary>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        protected async Task RegisterMqttBroker(string topic)
        {
            var addresses = await _mqttBrokerEntryManger.GetMqttBrokerAddress(topic);
            var host = NetUtils.GetHostAddress();
            if (addresses == null || !addresses.Any(p => p.ToString() == host.ToString()))
                await _mqttBrokerEntryManger.Register(topic, host);
        }

        #endregion 方法
    }
}