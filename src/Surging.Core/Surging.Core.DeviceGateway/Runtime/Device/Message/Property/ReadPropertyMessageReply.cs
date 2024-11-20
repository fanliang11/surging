using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message.Property
{
    public class ReadPropertyMessageReply : CommonDeviceMessageReply<ReadPropertyMessageReply>
    {
        public override MessageType MessageType { get; set; } = MessageType.READ_PROPERTY_REPLY;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ConcurrentDictionary<string, object> Properties { get; set; } = new ConcurrentDictionary<string, object>();

        public ReadPropertyMessageReply AddProperties(string key, string value)
        {
            Properties.AddOrUpdate(key, value, (k, v) => value);
            return this;
        }


        public ReadPropertyMessageReply Success(Dictionary<string, object> properties)
        {
            foreach (var item in properties)
            {
                Properties.AddOrUpdate(item.Key, item.Value, (k, v) => item.Value);
            }
            IsSuccess = true;
            return this;

        }
    }
}
