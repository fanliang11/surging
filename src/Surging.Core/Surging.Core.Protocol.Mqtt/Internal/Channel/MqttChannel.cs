using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Channel
{
    /// <summary>
    /// Defines the <see cref="MqttChannel" />
    /// </summary>
    public class MqttChannel
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Channel
        /// </summary>
        public IChannel Channel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether CleanSession
        /// </summary>
        public bool CleanSession { get; set; }

        /// <summary>
        /// Gets or sets the ClientId
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsWill
        /// </summary>
        public bool IsWill { get; set; }

        /// <summary>
        /// Gets or sets the KeepAliveInSeconds
        /// </summary>
        public int KeepAliveInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the Messages
        /// </summary>
        public ConcurrentDictionary<int, SendMqttMessage> Messages { get; set; }

        /// <summary>
        /// Gets or sets the PingReqTime
        /// </summary>
        public DateTime PingReqTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the SessionStatus
        /// </summary>
        public SessionStatus SessionStatus { get; set; }

        /// <summary>
        /// Gets or sets the SubscribeStatus
        /// </summary>
        public SubscribeStatus SubscribeStatus { get; set; }

        /// <summary>
        /// Gets or sets the Topics
        /// </summary>
        public List<string> Topics { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The AddMqttMessage
        /// </summary>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <param name="msg">The msg<see cref="SendMqttMessage"/></param>
        public void AddMqttMessage(int messageId, SendMqttMessage msg)
        {
            Messages.AddOrUpdate(messageId, msg, (id, message) => msg);
        }

        /// <summary>
        /// The AddTopic
        /// </summary>
        /// <param name="topics">The topics<see cref="string[]"/></param>
        public void AddTopic(params string[] topics)
        {
            Topics.AddRange(topics);
        }

        /// <summary>
        /// The Close
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Close()
        {
            if (Channel != null)
                await Channel.CloseAsync();
        }

        /// <summary>
        /// The GetMqttMessage
        /// </summary>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        /// <returns>The <see cref="SendMqttMessage"/></returns>
        public SendMqttMessage GetMqttMessage(int messageId)
        {
            SendMqttMessage mqttMessage = null;
            Messages.TryGetValue(messageId, out mqttMessage);
            return mqttMessage;
        }

        /// <summary>
        /// The IsActive
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsActive()
        {
            return Channel != null && Channel.Active;
        }

        /// <summary>
        /// The IsLogin
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsLogin()
        {
            bool result = false;
            if (Channel != null)
            {
                AttributeKey<string> _login = AttributeKey<string>.ValueOf("login");
                result = Channel.Active && Channel.HasAttribute(_login);
            }
            return result;
        }

        /// <summary>
        /// The IsOnine
        /// </summary>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public async Task<bool> IsOnine()
        {
            //如果保持连接的值非零，并且服务端在2倍的保持连接时间内没有收到客户端的报文，需要断开客户端的连接
            bool isOnline = (DateTime.Now - PingReqTime).TotalSeconds <= (this.KeepAliveInSeconds * 2) && SessionStatus == SessionStatus.OPEN;
            if (!isOnline)
            {
                await Close();
            }
            return isOnline;
        }

        /// <summary>
        /// The RemoveMqttMessage
        /// </summary>
        /// <param name="messageId">The messageId<see cref="int"/></param>
        public void RemoveMqttMessage(int messageId)
        {
            SendMqttMessage mqttMessage = null;
            Messages.Remove(messageId, out mqttMessage);
        }

        #endregion 方法
    }
}