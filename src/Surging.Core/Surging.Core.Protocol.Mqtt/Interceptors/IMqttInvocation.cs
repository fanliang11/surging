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
    public interface IMqttInvocation
    {
        Task<bool> Proceed();

        MqttBehavior Behavior { get; }

        EndPoint? RemoteAddress { get; }

        List<string> Topic {  get; }

        object Message {  get; }
        public string NetworkId { get; }
        PacketType PacketType { get; }  

       object Result { get; set; }
    }
}
