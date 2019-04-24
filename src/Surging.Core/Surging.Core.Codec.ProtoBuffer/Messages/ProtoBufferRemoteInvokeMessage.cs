using ProtoBuf;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer.Messages
{
    [ProtoContract]
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

        #endregion Constructor

        [ProtoMember(1)]
        public string Key { get; set; }

        [ProtoMember(2)]
        public DynamicItem Value { get; set; }
    }

    [ProtoContract]
    public class ProtoBufferRemoteInvokeMessage
    {
        public ProtoBufferRemoteInvokeMessage(RemoteInvokeMessage message)
        {
            ServiceId = message.ServiceId; 
            DecodeJOject = message.DecodeJOject;
            ServiceKey = message.ServiceKey;
            Parameters = message.Parameters?.Select(i => new ParameterItem(i)).ToArray();
            Attachments = message.Attachments?.Select(i => new ParameterItem(i)).ToArray();

        }

        public ProtoBufferRemoteInvokeMessage()
        {
        }

        [ProtoMember(1)]
        public string ServiceId { get; set; }

        [ProtoMember(2)]
        public string Token { get; set; }

        [ProtoMember(3)]
        public bool DecodeJOject{ get; set; }

        [ProtoMember(4)]
        public string ServiceKey { get; set; }

        [ProtoMember(5)]
        public ParameterItem[] Parameters { get; set; }

        [ProtoMember(6)]
        public ParameterItem[] Attachments { get; set; }


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
