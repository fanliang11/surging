using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Surging.IModuleServices.Common.Models
{
    [ProtoContract]
    [DataContract]
    public class AuthenticationRequestData
    {
        [ProtoMember(1)]
        [DataMember]
        public string UserName { get; set; }

        [ProtoMember(2)]
        [DataMember]
        public string Password { get; set; }
    }
}
