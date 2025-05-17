using DotNetty.Common.Utilities;
using Newtonsoft.Json.Linq;
using Surging.Core.CPlatform.Codecs.Core;
using Surging.Core.CPlatform.Codecs.Message;
using Surging.Core.DeviceGateway.Runtime.Core;
using Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http;
using Surging.Core.DeviceGateway.Runtime.Device.Implementation.Mqtt;
using Surging.Core.DeviceGateway.Runtime.Device.Message;
using Surging.Core.DeviceGateway.Runtime.Device.MessageCodec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation
{
    public class MqttDeviceMessageCodec : DeviceMessageCodec
    {
        private readonly MessageTransport _transport;
        private readonly JObject _object;
        

    public MqttDeviceMessageCodec(MessageTransport transport)
        {
            _transport = transport; 
        }

        public MqttDeviceMessageCodec() : this(MessageTransport.Mqtt)
        {
        }



         
    public override MessageTransport SupportTransport { get { return _transport; } }

        public override IObservable<MqttMessage> Encode(MessageEncodeContext context)
        {
          
            return Observable.Empty<MqttMessage>();
        }

        public override  IObservable<IDeviceMessage> Decode(MessageDecodeContext context)
        {
            var result = Observable.Return<IDeviceMessage>(default);
            MqttMessage message = (MqttMessage)context.GetMessage();
            byte[] payload = new byte[message.Payload.ReadableBytes];
            message.Payload.ReadBytes(payload);
            message.Payload.SetReaderIndex(0);
            TopicMessageCodec
                  .Dodecode(string.Join("/", TopicMessageCodec.RemoveProductPath(message.Topic)), payload).Subscribe(deviceMessage =>
                  {
                      if(deviceMessage != null) 
                      deviceMessage.DeviceId = message.ClientId;
                      result=result.Publish(deviceMessage);
                  }); 
            return result;
        }
    }
}
