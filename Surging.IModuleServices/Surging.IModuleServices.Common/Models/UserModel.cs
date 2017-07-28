using ProtoBuf;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;
using Surging.Core.System.Intercept;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models
{
    public class UserModel
    {

        [CacheKey(1)]
        public int UserId { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

    }
}
