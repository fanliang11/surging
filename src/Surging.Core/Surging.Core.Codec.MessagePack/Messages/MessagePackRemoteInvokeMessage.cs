using MessagePack;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Surging.Core.Codec.MessagePack.Messages
{
    [MessagePackObject]
    public class ParameterItem
    {
        #region Constructor

        public ParameterItem(KeyValuePair<string, object> item)
        {
            Key = item.Key;
            Value = item.Value == null ? null : new DynamicItem(item.Value);
        }

        public ParameterItem()
        {
        }

        public ParameterItem(object [] objs)
        {
            Key = objs[0]?.ToString();
            Value = new DynamicItem(objs[1] as object[]);
        }

        #endregion Constructor

        [Key(0)]
        public string Key { get; set; }

        [Key(1)]
        public DynamicItem Value { get; set; }

        public object[] ToArry()
        {
            var result = new object[]
             {
                Key,
                Value
            };
            return result;
        }
    }

    [MessagePackObject]
    public class MessagePackRemoteInvokeMessage
    {
        public MessagePackRemoteInvokeMessage(RemoteInvokeMessage message)
        {
            ServiceId = message.ServiceId; 
            DecodeJOject = message.DecodeJOject;
            ServiceKey = message.ServiceKey;
            Parameters = message.Parameters?.Select(i => new ParameterItem(i)).ToArray();
            Attachments = message.Attachments?.Select(i => new ParameterItem(i)).ToArray();
        }

        public MessagePackRemoteInvokeMessage(object[] objs)
        {
            var parameters = objs[4] as object[];
            var attachments = objs[5] as object[];
            ServiceId = objs[0]?.ToString();
            Token = objs[1]?.ToString();
            DecodeJOject = objs[2] == null ? false: (bool)objs[2]  ;
            ServiceKey= objs[3]?.ToString();
            Parameters = parameters.Select(p => new ParameterItem(p as object[])).ToArray(); 
            Attachments = attachments.Select(p => new ParameterItem(p as object[])).ToArray();
        }

        public MessagePackRemoteInvokeMessage()
        {
        }

        [Key(0)]
        public string ServiceId { get; set; }

        [Key(1)]
        public string Token { get; set; }

        [Key(2)]
        public bool DecodeJOject { get; set; }

        [Key(3)]
        public string ServiceKey { get; set; }

        [Key(4)]
        public ParameterItem[] Parameters { get; set; }

        [Key(5)]
        public ParameterItem[] Attachments { get; set; }

        public object[] ToArry()
        {
            var result = new object[]
            {
                ServiceId,
                Token,
                DecodeJOject,
                ServiceKey,
                Parameters,
                Attachments
            };
            return result;
        }
        
        public RemoteInvokeMessage GetRemoteInvokeMessage()
        {
            return new RemoteInvokeMessage
            {
                Parameters = Parameters?.ToDictionary(i => i.Key, i => i.Value?.Get()),
                Attachments = Attachments?.ToDictionary(i => i.Key, i => i.Value?.Get()),
                ServiceId = ServiceId,
                DecodeJOject = DecodeJOject,
                ServiceKey = ServiceKey, 
            };
        }
    }
}
