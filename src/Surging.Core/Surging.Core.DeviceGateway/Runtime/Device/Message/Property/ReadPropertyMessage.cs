using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message.Property
{
    public class ReadPropertyMessage : RespondDeviceMessage<IDeviceMessageReply>
    {
        public override MessageType MessageType { get; set; } = MessageType.READ_PROPERTY;

        public ConcurrentDictionary<string, object> Properties { get; set; } = new ConcurrentDictionary<string, object>();

        public ReadPropertyMessage AddProperties(string key, string value)
        {
            Properties.AddOrUpdate(key, value, (k, v) => value);
            return this;
        }

        public override ReadPropertyMessageReply NewReply()
        {
            return (ReadPropertyMessageReply)new ReadPropertyMessageReply().From(this);
        }
    }
}
