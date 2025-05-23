using DotNetty.Codecs.Mqtt.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Mqtt.Interceptors
{
    public class MqttServiceContext
    {
       public EndPoint? RemoteAddress {  get;  }

      public  PacketType PacketType { get; }

        public object Message { get;  }

        public MqttServiceContext(EndPoint remoteAddress, PacketType packetType, object message)
        {
            RemoteAddress = remoteAddress;
            PacketType = packetType;
            Message = message;
        }
    }
}
