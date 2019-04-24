using MessagePack;
using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

        #endregion Constructor

        [Key(0)]
        public string Key { get; set; }

        [Key(1)]
        public DynamicItem Value { get; set; }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
