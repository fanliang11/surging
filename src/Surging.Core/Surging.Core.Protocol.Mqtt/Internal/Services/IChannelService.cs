using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Surging.Core.Protocol.Mqtt.Internal.Channel;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    public interface IChannelService
    {
        MqttChannel GetMqttChannel(string deviceId);
        bool Connect(string deviceId, MqttChannel build);
        void Suscribe(String deviceId, params string[] topics);
        void Login(IChannel channel, string deviceId, ConnectMessage mqttConnectMessage);
        void Publish(IChannel channel, PublishPacket mqttPublishMessage);
        Task Close(string deviceId, bool isDisconnect);
        void SendWillMsg(MqttWillMessage willMeaasge);
        string GetDeviceId(IChannel channel);
        void UnSubscribe(string deviceId, params string[] topics);
        void Pubrel(IChannel channel, int messageId);
        void Pubrec(MqttChannel channel, int messageId);
    }
}
