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
    #region 接口

    /// <summary>
    /// Defines the <see cref="IChannelService" />
    /// </summary>
    public interface IChannelService
    {
        #region 方法

        /// <summary>
        /// The Close
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="isDisconnect">The isDisconnect<see cref="bool"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task Close(string deviceId, bool isDisconnect);

        /// <summary>
        /// 获取设备是否连接
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="mqttChannel"></param>
        /// <returns></returns>
        Task<bool> Connect(string deviceId, MqttChannel mqttChannel);

        /// <summary>
        /// The GetDeviceId
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <returns>The <see cref="ValueTask{string}"/></returns>
        ValueTask<string> GetDeviceId(IChannel channel);

        /// <summary>
        /// The GetDeviceIsOnine
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <returns>The <see cref="ValueTask{bool}"/></returns>
        ValueTask<bool> GetDeviceIsOnine(string deviceId);

        /// <summary>
        /// 获取mqtt通道
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        MqttChannel GetMqttChannel(string deviceId);

        /// <summary>
        /// The Login
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="mqttConnectMessage">The mqttConnectMessage<see cref="ConnectMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task Login(IChannel channel, string deviceId, ConnectMessage mqttConnectMessage);

        /// <summary>
        /// The PingReq
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <returns>The <see cref="ValueTask"/></returns>
        ValueTask PingReq(IChannel channel);

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="mqttPublishMessage"></param>
        /// <returns></returns>
        Task Publish(IChannel channel, PublishPacket mqttPublishMessage);

        /// <summary>
        /// The Publish
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="willMessage">The willMessage<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task Publish(string deviceId, MqttWillMessage willMessage);

        /// <summary>
        /// The Pubrec
        /// </summary>
        /// <param name="channel">The channel<see cref="MqttChannel"/></param>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task Pubrec(MqttChannel channel, int messageId);

        /// <summary>
        /// The Pubrel
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task Pubrel(IChannel channel, int messageId);

        /// <summary>
        /// The RemotePublishMessage
        /// </summary>
        /// <param name="deviceId">The deviceId<see cref="string"/></param>
        /// <param name="willMessage">The willMessage<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task RemotePublishMessage(string deviceId, MqttWillMessage willMessage);

        /// <summary>
        /// The SendWillMsg
        /// </summary>
        /// <param name="willMeaasge">The willMeaasge<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SendWillMsg(MqttWillMessage willMeaasge);

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="topics">主题列表</param>
        /// <returns></returns>
        Task Suscribe(String deviceId, params string[] topics);

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="topics"></param>
        /// <returns></returns>
        Task UnSubscribe(string deviceId, params string[] topics);

        #endregion 方法
    }

    #endregion 接口
}