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
        Task Suscribe(String deviceId, params string[] topics);
        Task Login(IChannel channel, string deviceId, ConnectMessage mqttConnectMessage);
        Task Publish(IChannel channel, PublishPacket mqttPublishMessage);
        ValueTask PingReq(IChannel channel);
        Task Publish(string deviceId, MqttWillMessage willMessage);
        Task RemotePublishMessage(string deviceId, MqttWillMessage willMessage);
        Task Close(string deviceId, bool isDisconnect);
        ValueTask<bool> GetDeviceIsOnine(string deviceId);
        Task SendWillMsg(MqttWillMessage willMeaasge);
        ValueTask<string> GetDeviceId(IChannel channel);
        Task UnSubscribe(string deviceId, params string[] topics);
        Task Pubrel(IChannel channel, int messageId);
        Task Pubrec(MqttChannel channel, int messageId);
    }
}
