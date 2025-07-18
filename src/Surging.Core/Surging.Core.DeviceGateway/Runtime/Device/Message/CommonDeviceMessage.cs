
using Surging.Core.DeviceGateway.Utilities;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message
{
    public abstract class CommonDeviceMessage<T>: IDeviceMessage
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public  ConcurrentDictionary<string, object> Headers { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Code { get; set; } 
        public abstract MessageType MessageType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string MessageId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string DeviceId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? Timestamp { get; set; } = Utility.CurrentTimeMillis();

        public JsonObject ToJson()
        {
            var json =new JsonObject();
            json.Add("messageType", MessageType.ToString());
            json.Add("deviceId", DeviceId);
            json.Add("timestamp", Timestamp); 
            json.Add("messageId", MessageId);
            json.Add("Code", Code);
            json.Add("Headers", JsonSerializer.Serialize(Headers));
            return json;
        }

        public void FromJson(JsonObject jsonObject)
        {
          
            if (Timestamp == 0)
            {
                Timestamp = Utility.CurrentTimeMillis();
            }
           jsonObject.TryGetPropertyValue("headers",out JsonNode? headers);
            if (null != headers)
            {
                foreach (var item in headers.AsObject())
                {
                    Headers.AddOrUpdate(item.Key, item.Value, (m,n) =>
                    {
                        return item.Value;
                    });
                }
            }
        }

        public  string ToString()
        {
            return JsonSerializer.Serialize(ToJson());
        }

        public IDeviceMessage AddHeader(string header, object value)
        {if (Headers == null) Headers = new ConcurrentDictionary<string, object>();
            Headers.AddOrUpdate(header, value, (m, n) => value);
            return this;
        }

        public IDeviceMessage AddHeader(HeaderKey<string> header, object value)
        {
            var headerValue = value ?? header.DefaultValue;
            Headers.AddOrUpdate(header.Key, headerValue, (m, n) => headerValue);
            return this;
        }
    }
}