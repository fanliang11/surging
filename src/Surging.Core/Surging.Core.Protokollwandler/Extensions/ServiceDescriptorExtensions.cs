using Surging.Core.Protokollwandler.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;
using Surging.Core.CPlatform;

namespace Surging.Core.Protokollwandler.Extensions
{
    public static class  ServiceDescriptorExtensions
    {
        public static TransferContract GetTransferContract(this ServiceDescriptor descriptor)
        {
            var metadata = descriptor.GetMetadata<TransferContract>("TContract", null); 
            return metadata;
        }

        public static ServiceDescriptor SetTransferContract(this ServiceDescriptor descriptor, TransferContract contract)
        {
            descriptor.Metadatas["TContract"] = contract;
            return descriptor;
        }
    }
}
