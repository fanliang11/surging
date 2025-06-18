using Surging.Core.CPlatform.Codecs.Message;
using Surging.Core.CPlatform.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation.Mqtt
{
    public class MqttMessageResponse
    {
        public int MessageId { get; set; }

        public object Data {  get; set; }
        public string Topic {  get; set; }

        public int QosLevel {  get; set; }

        public bool Dup {  get; set; }

        public static MqttMessageResponse Instance(MqttMessage message, PayloadType type)
        {
            return  new MqttMessageResponse()
            {
                Topic = message.Topic,
                QosLevel = message.QosLevel,
                Dup = message.Dup, 
                MessageId = message.MessageId, 
                Data = PayloadDecoder.Read(message.Payload, type)
            }; 
        }
    }
}
