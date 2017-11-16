using ProtoBuf;
using Surging.Core.CPlatform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models
{
    [ProtoContract]
    public class IdentityUser:RequestData
    {
        [ProtoMember(1)]
        public string RoleId { get; set; }
    }
}
