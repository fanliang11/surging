using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using Surging.Core.DeviceGateway.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message
{
    public abstract class CommonDeviceMessageReply<T> : IDeviceMessageReply
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ConcurrentDictionary<string, object> Headers { get; set; }

        public bool IsSuccess { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Code { get; set; }

        public abstract MessageType MessageType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Message { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string MessageId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string DeviceId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? Timestamp { get; set; } = Utility.CurrentTimeMillis();

        public JsonObject ToJson()
        {
            var json = new JsonObject();
            json.Add("messageType", MessageType.ToString());
            json.Add("deviceId", DeviceId);
            json.Add("timestamp", Timestamp);
            json.Add("messageId", MessageId);
            json.Add("Code", Code);
            return json;
        }

        public IDeviceMessageReply Success(bool isSuccess)
        {
            IsSuccess = isSuccess;
            return this;
        }

        public IDeviceMessageReply Failure(Exception e)
        {
            return Failure(StatusCode.FAIL.ToString(), e.Message.ToString());
        }

        public IDeviceMessageReply From(IMessage message)
        {
            this.MessageId = message.MessageId;
            var deviceMessage = message as IDeviceMessage;
            if (deviceMessage != null)
            {
                this.DeviceId = deviceMessage.DeviceId;
            }
            return this;
        }

        public IDeviceMessageReply Failure(StatusCode errorCode)
        {
            FieldInfo field = errorCode.GetType().GetField(errorCode.ToString());
            var display = field?.GetCustomAttribute<DisplayAttribute>();
            return Failure(errorCode.ToString(), display?.Name);
        }

        public IDeviceMessageReply Failure(string errorCode, string msg)
        {
            IsSuccess = false;
            Code = errorCode;
            Message = msg;
            return this;
        }

        public void FromJson(JsonObject jsonObject)
        {

            if (Timestamp == 0)
            {
                Timestamp = Utility.CurrentTimeMillis();
            }
            jsonObject.TryGetPropertyValue("headers", out JsonNode? headers);
            if (null != headers)
            {
                foreach (var item in headers.AsObject())
                {
                    Headers.AddOrUpdate(item.Key, item.Value, (m, n) =>
                    {
                        return item.Value;
                    });
                }
            }
        }

        public IDeviceMessage AddHeader(string header, object value)
        {
            if (header == null) Headers = new ConcurrentDictionary<string, object>();
            Headers.AddOrUpdate(header, value, (m, n) => value);
            return this;
        }

        public IDeviceMessage AddHeader(HeaderKey<string> header, object value)
        {
            var headerValue = value ?? header.DefaultValue;
            Headers.AddOrUpdate(header.Key, headerValue, (m, n) => headerValue);
            return this;
        }

        public string ToString()
        {
            return JsonSerializer.Serialize(ToJson());
        }
    }
}
