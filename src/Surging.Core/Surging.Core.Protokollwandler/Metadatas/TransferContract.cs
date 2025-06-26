using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.Protokollwandler.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protokollwandler.Metadatas
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class TransferContract : ServiceDescriptorAttribute
    {
        public  string Name { get; set; }

        public string RoutePath { get; set; }

        public TransferContractType Type { get; set; }

        public override void Apply(ServiceDescriptor descriptor)
        {
            descriptor.SetTransferContract(this);
        }
    }
}

