using DotNetty.Codecs.Mqtt.Packets;
using Surging.Core.CPlatform.Messages;
using Surging.Core.Protocol.Mqtt.Internal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Interceptors
{
    public class MqttInvocation : IMqttInvocation
    {
        public MqttBehavior Behavior { get; set; }

        public EndPoint RemoteAddress { get; set; }

        public string NetworkId { get; set; }

        public PacketType PacketType { get; set; }

        public List<string> Topic { get; set; }
        public object Result { get ; set; }

        public object Message {  get; set; }

        public async virtual Task<bool> Proceed()
        { 
            Behavior.NetworkId.OnNext(NetworkId);
            return await Behavior.CallInvoke(this);
        }
    }
}
