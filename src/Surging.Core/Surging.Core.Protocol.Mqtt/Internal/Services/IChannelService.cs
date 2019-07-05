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
        /// <summary>
        /// 获取mqtt通道
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        MqttChannel GetMqttChannel(string deviceId);
        /// <summary>
        /// 获取设备是否连接
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="mqttChannel"></param>
        /// <returns></returns>
        Task<bool> Connect(string deviceId, MqttChannel mqttChannel);
        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="topics">主题列表</param>
        /// <returns></returns>
        Task Suscribe(String deviceId, params string[] topics);
        Task Login(IChannel channel, string deviceId, ConnectMessage mqttConnectMessage);
        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="mqttPublishMessage"></param>
        /// <returns></returns>
        Task Publish(IChannel channel, PublishPacket mqttPublishMessage);
        ValueTask PingReq(IChannel channel);
        Task Publish(string deviceId, MqttWillMessage willMessage);
        Task RemotePublishMessage(string deviceId, MqttWillMessage willMessage);
        Task Close(string deviceId, bool isDisconnect);
        ValueTask<bool> GetDeviceIsOnine(string deviceId);
        Task SendWillMsg(MqttWillMessage willMeaasge);
        ValueTask<string> GetDeviceId(IChannel channel);
        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="topics"></param>
        /// <returns></returns>
        Task UnSubscribe(string deviceId, params string[] topics);
        Task Pubrel(IChannel channel, int messageId);
        Task Pubrec(MqttChannel channel, int messageId);
    }
}
