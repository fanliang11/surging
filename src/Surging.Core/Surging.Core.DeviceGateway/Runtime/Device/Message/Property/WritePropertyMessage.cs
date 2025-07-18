using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message.Property
{
    public class WritePropertyMessage : RespondDeviceMessage<IDeviceMessageReply>
    {
        public override MessageType MessageType { get; set; } = MessageType.READ_PROPERTY;

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public WritePropertyMessage AddProperties(string key, string value)
        {
            if (!Properties.ContainsKey(key))
            {
                Properties.Add(key, value);
            }
            return this;
        }

        public override WritePropertyMessageReply NewReply()
        {
            return (WritePropertyMessageReply)new WritePropertyMessageReply().From(this);
        }
    }
}