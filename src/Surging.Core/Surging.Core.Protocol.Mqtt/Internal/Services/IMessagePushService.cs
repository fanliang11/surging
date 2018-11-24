using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Surging.Core.Protocol.Mqtt.Internal.Channel;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    public interface IMessagePushService
    {
        void WriteWillMsg(MqttChannel mqttChannel, MqttWillMessage willMeaasge);

        void SendQosConfirmMsg(QualityOfService qos, MqttChannel mqttChannel, string topic, byte[] bytes);

        void SendPubBack(IChannel channel, int messageId);

        void SendPubRec(MqttChannel mqttChannel, int messageId);

        void SendPubRel(IChannel channel, int messageId);

        void SendToPubComp(IChannel channel, int messageId);

        void SendQos0Msg(IChannel channel, String topic, byte[] byteBuf);
    }
}
