using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Channel
{
    public class MqttChannel
    {
        private volatile IChannel _channel;
        private string _clientId;
        private bool isWill;
        private volatile SubscribeStatus _subscribeStatus;
        private List<String> _topics;
        private volatile SessionStatus _sessionStatus;
        private volatile bool _cleanSession;
        private ConcurrentDictionary<int, MqttMessage> _messages;
        private List<int> _receives;

        public void AddRecevice(int messageId)
        {
            _receives.Add(messageId);
        }

        public bool CheckRecevice(int messageId)
        {
            return _receives.Contains(messageId);
        }

        public bool RemoveRecevice(int messageId)
        {
            return _receives.Remove(messageId);
        }


        public void AddMqttMessage(int messageId, MqttMessage msg)
        {
            _messages.AddOrUpdate(messageId, msg,(id,message)=>msg);
        }


        public MqttMessage GetMqttMessage(int messageId)
        {
            MqttMessage mqttMessage = null;
            _messages.TryGetValue(messageId, out mqttMessage);
            return mqttMessage;
        }


        public void RemoveMqttMessage(int messageId)
        {
            MqttMessage mqttMessage = null;
            _messages.Remove(messageId,out mqttMessage);
        }

        public bool IsLogin()
        {
            bool result = false;
            if (_channel != null)
            {
                AttributeKey<object> _login = AttributeKey<object>.ValueOf("login");
                result= _channel.Active && _channel.HasAttribute(_login);
            }
            return result;
        }

        public async Task Close()
        {
            if (_channel != null)
                await _channel.CloseAsync();
        }

        public bool IsActive()
        {
            return _channel != null && _channel.Active;
        }

        public void AddTopic(params string[] topics)
        {
            _topics.AddRange(topics);
        }
    }
}
