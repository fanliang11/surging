using Jint.Runtime.Interop;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using Surging.Core.DeviceGateway.Runtime.Device.Message;
using Surging.Core.DeviceGateway.Runtime.Device.Message.Event;
using Surging.Core.DeviceGateway.Runtime.Device.Message.Property;
using Surging.Core.DeviceGateway.Utilities;
using System.Reactive.Linq;
using System.Text.Json;

namespace Surging.Core.DeviceGateway.Runtime.Device.MessageCodec
{

    public class TopicMessageCodec
    {
        public static List<TopicMessageCodec> _value = new List<TopicMessageCodec>()
        {
            DeviceOnline,
            ReadProperty,
            ReportProperty,
            WriteProperty,
            Event
        };

        public static TopicMessageCodec DeviceOnline => new TopicMessageCodec("/*/device/online", typeof(DeviceOnlineMessage), route => route.GroupName("设备上线").HttpMethod("Post").Description("验证设备上线").Example("{\"headers\":{\"token\":\"属性值\"}}"));
        public static TopicMessageCodec ReportProperty => new TopicMessageCodec("/*/properties/report", typeof(ReadPropertyMessage), route => route.GroupName("属性上报").HttpMethod("Post").Description("上报物模型属性数据").Example("{\"properties\":{\"属性ID\":\"属性值\"}}"));
        public static TopicMessageCodec ReadProperty => new TopicMessageCodec("/*/properties/read", typeof(ReadPropertyMessage), route => route.GroupName("读取属性").HttpMethod("Post").Description("下方读取属性指令").Example("{\"messageId\":\"消息ID,回复时需要一致.\",\"properties\":[\"属性ID\"]}"));

        public static TopicMessageCodec WriteProperty => new TopicMessageCodec("/*/properties/read", typeof(ReadPropertyMessage), route => route.GroupName("修改属性").Description("下发修改属性指令")
                           .Example("{\"properties\":{\"属性ID\":\"属性值\"}}"));

      public   static TopicMessageCodec Event => new TopicMessageCodec("/*/event/*", typeof(EventMessage), route => route.GroupName("事件上报").HttpMethod("Post").Description("事件上报").Example("{\"data\":{\"key\":\"value\"}}"));

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

        public static IObservable<IDeviceMessage> Dodecode(string topic, byte[] payload)
        {
            var result = Observable.Return<IDeviceMessage>(default);
            var messageCodec = FromTopic(topic);
            if (messageCodec != null)
                result= messageCodec.Decode(payload);
            return result;
        }

        public  IObservable<IDeviceMessage> Decode( byte[] payload)
        {
            var result = Observable.Return<IDeviceMessage>(default);
            var deviceMessage = JsonSerializer.Deserialize(payload, MessageType) as IDeviceMessage;
            if(deviceMessage != null)
                result=Observable.Return(deviceMessage);
            return result;
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
