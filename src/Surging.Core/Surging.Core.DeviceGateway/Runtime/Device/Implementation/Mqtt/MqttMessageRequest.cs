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
    public  class MqttMessageRequest
    {
        private string Topic {  get; set; }

        private int QosLevel {  get; set; }
        private object Data { get; set; }
        private int MessageId {  get; set; }
        private bool Will { get; set; }

        private bool Dup { get; set; }

        private bool Retain { get; set; }

        public static  MqttMessageRequest Instance(MqttMessage message, PayloadType type)
        {
            MqttMessageRequest requestMessage = new MqttMessageRequest()
            {
                Topic = message.Topic,
                QosLevel = message.QosLevel,
                Dup = message.Dup,
                Retain = message.Retain,
                MessageId = message.MessageId,
                Will = message.Will,
                Data = PayloadDecoder.Read(message.Payload, type)
            };
            return requestMessage;
        }

    }
}
