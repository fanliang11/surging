using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message.Property
{
    public class WritePropertyMessageReply : CommonDeviceMessageReply<ReadPropertyMessageReply>
    {
        public override MessageType MessageType { get; set; } = MessageType.READ_PROPERTY_REPLY;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public WritePropertyMessageReply AddProperties(string key, string value)
        {
            if (!this.Properties.ContainsKey(key))
            {
                Properties.Add(key, value);
            }
            return this;
        }


        public WritePropertyMessageReply Success(Dictionary<string, object> properties)
        {
            foreach (var item in properties)
            {
                if (!this.Properties.ContainsKey(item.Key))
                {
                    Properties.Add(item.Key, item.Value);
                }
            }
            IsSuccess = true;
            return this;

        }
    }
}
