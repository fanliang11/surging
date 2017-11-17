using System.Collections.Generic;
using System.Linq;
using Surging.Core.CPlatform.Messages;

namespace Surging.Core.Codec.MessagePack.Messages
{
    public class ParameterItem
    {
        #region Constructor

        public ParameterItem(KeyValuePair<string, object> item)
        {
            Key = item.Key;
            Value = item.Value == null ? null : new DynamicItem(item.Value);
        }

        public ParameterItem()
        { }

        #endregion Constructor

        public string Key { get; set; }

        public DynamicItem Value { get; set; }
    }

    public class MessagePackRemoteInvokeMessage
    {
        public MessagePackRemoteInvokeMessage(RemoteInvokeMessage message)
        {
            ServiceId = message.ServiceId;
            Token = message.Token;
            ServiceKey = message.ServiceKey;
            Parameters = message.Parameters?.Select(i => new ParameterItem(i)).ToArray();
        }

        public MessagePackRemoteInvokeMessage()
        { }

        public string ServiceId { get; set; }

        public string Token { get; set; }

        public string ServiceKey { get; set; }

        public ParameterItem[] Parameters { get; set; }

        public RemoteInvokeMessage GetRemoteInvokeMessage()
        {
            return new RemoteInvokeMessage
            {
                Parameters = Parameters?.ToDictionary(i => i.Key, i => i.Value?.Get()),
                ServiceId = ServiceId
            };
        }
    }
}