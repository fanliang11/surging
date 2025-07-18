using Jint.Runtime.Interop;
using Newtonsoft.Json.Linq;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using Surging.Core.DeviceGateway.Runtime.Device.Implementation.Mqtt;
using Surging.Core.DeviceGateway.Runtime.Device.Message;
using Surging.Core.DeviceGateway.Runtime.Device.Message.Event;
using Surging.Core.DeviceGateway.Runtime.Device.Message.Function;
using Surging.Core.DeviceGateway.Runtime.Device.Message.Property;
using Surging.Core.DeviceGateway.Utilities;
using System.Reactive.Joins;
using System.Reactive.Linq;
using System.Text.Json;

using ReadPropertyMessage = Surging.Core.DeviceGateway.Runtime.Device.Message.ReadPropertyMessage;
using ReadPropertyMessageReply = Surging.Core.DeviceGateway.Runtime.Device.Message.ReadPropertyMessageReply;

namespace Surging.Core.DeviceGateway.Runtime.Device.MessageCodec
{

    public class TopicMessageCodec
    {
        public static List<TopicMessageCodec> _value = new List<TopicMessageCodec>()
        {
            DeviceOnline,
            ReadProperty,
            ReadReplyProperty,
            ReportProperty,
            WriteProperty,
            WriteReplyProperty,
            Event,
            FunctionCall,
            FunctionCallReply,
             
        };

        public static TopicMessageCodec DeviceOnline => new TopicMessageCodec("/*/device/online", typeof(DeviceOnlineMessage), route => route.GroupName("设备上线").UpStream(true).DownStream(false).HttpMethod("Post").Description("验证设备上线").Example("{\"headers\":{\"token\":\"属性值\"}}"));
        public static TopicMessageCodec ReportProperty => new TopicMessageCodec("/*/properties/report", typeof(ReportPropertyMessage), route => route.GroupName("属性上报").UpStream(true).DownStream(false).HttpMethod("Post").Description("上报物模型属性数据").Example("{\"properties\":[\"属性ID\"]}"));
        public static TopicMessageCodec ReadProperty => new TopicMessageCodec("/*/properties/read", typeof(ReadPropertyMessage), route => route.GroupName("读取属性").UpStream(false).DownStream(true).HttpMethod("Post").Description("下发读取属性指令").Example("{\"messageId\":\"消息ID,回复时需要一致.\",\"properties\":[\"属性ID\"]}"));

        public static TopicMessageCodec ReadReplyProperty => new TopicMessageCodec("/*/properties/read/reply", typeof(ReadPropertyMessageReply), route => route.GroupName("读取属性回复").UpStream(true).DownStream(false).HttpMethod("Post").Description("设备响应下发读取属性指令").Example("{\"messageId\":\"消息ID,回复时需要一致.\",\"properties\":{\"属性ID\":\"属性值\"}}"));
        public static TopicMessageCodec FunctionCall => new TopicMessageCodec("/*/function/call", typeof(FunctionInvokeMessage), route => route.UpStream(false).DownStream(true).GroupName("调用功能").Description("下发功能调用指令")
                   .Example("{\"messageId\":\"消息ID,设备回复时需要一致.\"," +
                                            "\"functionId\":\"功能标识\"," +
                                            "\"inparamters\":[{\"name\":\"参数名\",\"value\":\"参数值\"}]}"));


        public static TopicMessageCodec FunctionCallReply => new TopicMessageCodec("/*/function/call/reply", typeof(FunctionInvokeMessageReply), route => route.UpStream(true).DownStream(false).GroupName("调用功能").Description("设备响应下发功能调用指令")
                   .Example("{\"messageId\":\"消息ID,设备回复时需要一致.\"," +
                                            "\"functionId\":\"功能标识\"," +
                                            "\"outparameters\":\"调用结果，格式与功能输出参数配置一致\""));
        public static TopicMessageCodec WriteProperty => new TopicMessageCodec("/*/properties/write", typeof(WritePropertyMessage), route => route.GroupName("修改属性").UpStream(false).DownStream(true).Description("下发修改属性指令")
                           .Example("{\"properties\":{\"属性ID\":\"属性值\"}}"));

        public static TopicMessageCodec WriteReplyProperty => new TopicMessageCodec("/*/properties/write/reply", typeof(WritePropertyMessageReply), route => route.GroupName("修改属性回复").UpStream(true).DownStream(false).Description("设备响应下发修改属性指令")
                   .Example("{\"properties\":{\"属性ID\":\"属性值\"}}"));

        public   static TopicMessageCodec Event => new TopicMessageCodec("/*/event/*", typeof(EventMessage), route => route.GroupName("事件上报").UpStream(true).DownStream(false).HttpMethod("Post").Description("事件上报").Example("{\"data\":{\"key\":\"value\"}}"));

        public Type MessageType { get; }
        public ServiceDescriptor Route { get; }

       public  string Pattern { get; }
        public TopicMessageCodec(string topic,
                    Type type,
                      Func<ServiceDescriptor, ServiceDescriptor> routeCustom)
        {
            Pattern = topic;
            MessageType = type;
            Route = new ServiceDescriptor() { Id = Pattern, RoutePath = Pattern };
            Route = routeCustom(Route);
        }

        TopicMessageCodec(string topic,
                        Type type)
        {
            Pattern = topic;
            MessageType = type;
            Route = new ServiceDescriptor() { Id = Pattern, RoutePath = Pattern };
        }

       public static TopicMessageCodec FromTopic(string path)
        {
            foreach (var value in _value)
            {
                if (PathUtils.Match(value.Pattern, path))
                {
                    return value;
                }
            }
            return default;
        }

        public TopicPayload DoEncode(JObject mapper, string[] topics, IDeviceMessage message)
        {
            RefactorTopic(topics, message);
            return new TopicPayload
            {
                Topic = topics[0],
            };
        }


       public TopicPayload DoEncode(JObject mapper, IDeviceMessage message)
        {
            string[] topics = new string[] { Pattern, "" };
            return DoEncode(mapper, topics, message);
        }

       public void RefactorTopic(  string[] topics, IDeviceMessage message)
        {
            topics[1] = message.DeviceId;
        }

        public static string[] RemoveProductPath(String topic)
        {
            if (!topic.StartsWith("/"))
            {
                topic = "/" + topic;
            }
           var topicArr = topic.Split("/");
            var topics = new string[topicArr.Length - 1];
            Array.Copy(topicArr, 1, topics,0,topicArr.Length-1);
            topics[0] = "";
            return topics;
        }
        public static IObservable<IDeviceMessage> Dodecode(string topic, byte[] payload)
        {
            var result = Observable.Return<IDeviceMessage>(default);
            var messageCodec = FromTopic(topic);
            if (messageCodec != null)
                result= messageCodec.Decode(payload);
            return result;
        }

        public IObservable<IDeviceMessage> Decode(byte[] payload)
        {
            var result = Observable.Return<IDeviceMessage>(default);
            try
            {
                var deviceMessage = JsonSerializer.Deserialize(payload, MessageType) as IDeviceMessage;
                if (deviceMessage != null)
                    result = Observable.Return(deviceMessage);
                return result;
            }
            catch (JsonException)
            {
                return result;
            }
        }

        public static TopicMessageCodec FromMessage(IDeviceMessage message)
        {
            foreach (var value in _value)
            {
                if (value.MessageType==message.GetType())
                {
                    return value;
                }
            }
            return default;
        }

    }
}
