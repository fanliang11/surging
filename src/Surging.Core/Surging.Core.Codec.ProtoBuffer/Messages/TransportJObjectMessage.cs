using Newtonsoft.Json.Linq;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Codec.ProtoBuffer.Messages
{
    [ProtoContract]
    public class TransportJObjectMessage:JObject
    {
        [ProtoMember(1, DynamicType = true)]
        public object Property { get; set; }
    }
}
