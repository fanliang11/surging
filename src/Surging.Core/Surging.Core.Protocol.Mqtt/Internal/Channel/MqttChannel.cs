﻿using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Internal.Channel
{
    public class MqttChannel
    {
        public IChannel Channel { get; set; }
        public string ClientId { get; set; }
        public bool IsWill { get; set; }
        public SubscribeStatus SubscribeStatus { get; set; }
        public List<string> Topics { get; set; }
        public SessionStatus SessionStatus { get; set; }
        public int KeepAliveInSeconds { get; set; }
        public DateTime PingReqTime { get; set; } = DateTime.Now;

        public bool CleanSession { get; set; }
        public ConcurrentDictionary<int, SendMqttMessage> Messages { get; set; }
    
        public void AddMqttMessage(int messageId, SendMqttMessage msg)
        {
            Messages.AddOrUpdate(messageId, msg,(id,message)=>msg);
        }
        
        public SendMqttMessage GetMqttMessage(int messageId)
        {
            SendMqttMessage mqttMessage = null;
            Messages.TryGetValue(messageId, out mqttMessage);
            return mqttMessage;
        }


        public void RemoveMqttMessage(int messageId)
        {
            SendMqttMessage mqttMessage = null;
            Messages.Remove(messageId,out mqttMessage);
        }

        public bool IsLogin()
        {
            bool result = false;
            if (Channel != null)
            {
                AttributeKey<string> _login = AttributeKey<string>.ValueOf("login");
                result= Channel.Active && Channel.HasAttribute(_login);
            }
            return result;
        }

        public async Task Close()
        {
            if (Channel != null)
                await Channel.CloseAsync();
        }

       
        public async Task<bool> IsOnine()
        {
            //如果保持连接的值非零，并且服务端在2倍的保持连接时间内没有收到客户端的报文，需要断开客户端的连接
            bool isOnline= (DateTime.Now - PingReqTime).TotalSeconds <= (this.KeepAliveInSeconds*2) && SessionStatus== SessionStatus.OPEN;
            if(!isOnline)
            {
               await Close();
            }
            return isOnline;
        }

        public bool IsActive()
        {
            return Channel != null && Channel.Active;
        }
        
        public void AddTopic(params string[] topics)
        { 
            Topics.AddRange(topics.Except(Topics));
        }
    }
}
