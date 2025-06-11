using Surging.Core.CPlatform.Ioc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Grpc.Runtime
{
   public class GrpcServiceEntry
    { 

        public Type Type { get; set; }

        public IServiceBehavior Behavior { get; set; }
    }
}
