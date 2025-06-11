using Surging.Core.DeviceGateway.Runtime.Device.Message.Function;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message.Property
{
    internal class ReportPropertyMessage : CommonDeviceMessage<IDeviceMessage>
    {
        public override MessageType MessageType { get; set; } = MessageType.READ_PROPERTY;

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public void FromJson(JsonObject jsonObject)
        {
            base.FromJson(jsonObject);
            this.Properties = jsonObject["Properties"].GetValue<Dictionary<string, object>>();
        }

        public ReportPropertyMessage AddInput(string name, object value)
        {
            Properties.Add(name, value);
            return this;
        }

        public ReportPropertyMessage AddProperties(Dictionary<string, object> properties)
        {
            foreach (var prop in properties)
            {
                Properties.Add(prop.Key,prop.Value);
            }
            return this;
        }
    }
}
