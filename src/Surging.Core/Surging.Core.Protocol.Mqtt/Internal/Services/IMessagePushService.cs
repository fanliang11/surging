using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Transport.Channels;
using Surging.Core.Protocol.Mqtt.Internal.Channel;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Services
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IMessagePushService" />
    /// </summary>
    public interface IMessagePushService
    {
        #region 方法

        /// <summary>
        /// The SendPubBack
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SendPubBack(IChannel channel, int messageId);

        /// <summary>
        /// The SendPubRec
        /// </summary>
        /// <param name="mqttChannel">The mqttChannel<see cref="MqttChannel"/></param>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SendPubRec(MqttChannel mqttChannel, int messageId);

        /// <summary>
        /// The SendPubRel
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SendPubRel(IChannel channel, int messageId);

        /// <summary>
        /// The SendQos0Msg
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="topic">The topic<see cref="String"/></param>
        /// <param name="byteBuf">The byteBuf<see cref="byte[]"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SendQos0Msg(IChannel channel, String topic, byte[] byteBuf);

        /// <summary>
        /// The SendQosConfirmMsg
        /// </summary>
        /// <param name="qos">The qos<see cref="QualityOfService"/></param>
        /// <param name="mqttChannel">The mqttChannel<see cref="MqttChannel"/></param>
        /// <param name="topic">The topic<see cref="string"/></param>
        /// <param name="bytes">The bytes<see cref="byte[]"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SendQosConfirmMsg(QualityOfService qos, MqttChannel mqttChannel, string topic, byte[] bytes);

        /// <summary>
        /// The SendToPubComp
        /// </summary>
        /// <param name="channel">The channel<see cref="IChannel"/></param>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SendToPubComp(IChannel channel, int messageId);

        /// <summary>
        /// The WriteWillMsg
        /// </summary>
        /// <param name="mqttChannel">The mqttChannel<see cref="MqttChannel"/></param>
        /// <param name="willMeaasge">The willMeaasge<see cref="MqttWillMessage"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task WriteWillMsg(MqttChannel mqttChannel, MqttWillMessage willMeaasge);

        #endregion 方法
    }

    #endregion 接口
}