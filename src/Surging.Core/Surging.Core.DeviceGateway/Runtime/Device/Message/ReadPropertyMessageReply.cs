using Surging.Core.DeviceGateway.Runtime.Device.Message.Function;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message
{
    public class ReadPropertyMessageReply : CommonDeviceMessageReply<ReadPropertyMessageReply>
    {
        public override MessageType MessageType { get; set; } = MessageType.READ_PROPERTY_REPLY;
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string,object> Properties { get; set; } = new Dictionary<string, object>();

      

        public void FromJson(JsonElement jsonObject)
        {
            base.FromJson(jsonObject); 
            this.Properties = jsonObject.GetProperty("Properties").Deserialize<Dictionary<string, object>>()??new Dictionary<string, object>();
        }


        public ReadPropertyMessageReply Success(Dictionary<string, object> properties)
        {
            this.Properties = properties;
            IsSuccess = true;
            return this;

        }

        public ReadPropertyMessageReply Success(string deviceId,
                                                         string messageId,
                                                         Dictionary<string, object> properties)
        {
            Properties = properties;
            IsSuccess = true;
            DeviceId = deviceId;
            MessageId = messageId;
            return this;
        }

        public ReadPropertyMessageReply Failure(string deviceId,
                                                         string messageId,
                                                         string message)
        { 
            Message = message;
            IsSuccess = true;
            DeviceId = deviceId;
            MessageId = messageId;
            return this;
        }
    }
}
 