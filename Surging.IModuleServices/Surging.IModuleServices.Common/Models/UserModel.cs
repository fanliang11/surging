using ProtoBuf;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;
using Surging.Core.System.Intercept;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models
{
    [ProtoContract]
    public class UserModel
    {

        [ProtoMember(1)]
        [CacheKey(1)]
        public int UserId { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public int Age { get; set; }

    }
}
