using Surging.Core.CPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public static  class ServiceDescriptorExtensions
    {
        
        public static string ContentType(this ServiceDescriptor descriptor)
        {
            return descriptor.GetMetadata<string>("ContentType");
        }

        public static ServiceDescriptor ContentType(this ServiceDescriptor descriptor, string contentType)
        {
            descriptor.Metadatas["ContentType"] = contentType;
            return descriptor;
        }

        public static string Path(this ServiceDescriptor descriptor)
        {
            return descriptor.GetMetadata<string>("Path");
        }

        public static ServiceDescriptor Path(this ServiceDescriptor descriptor, string path)
        {
            descriptor.Metadatas["Path"] = path;
            return descriptor;
        }

        public static string Description(this ServiceDescriptor descriptor)
        {
            return descriptor.GetMetadata<string>("Description");
        }

        public static ServiceDescriptor Description(this ServiceDescriptor descriptor, string description)
        {
            descriptor.Metadatas["Description"] = description;
            return descriptor;
        }

        public static string Example(this ServiceDescriptor descriptor)
        {
            return descriptor.GetMetadata<string>("Example");
        }

        public static ServiceDescriptor Example(this ServiceDescriptor descriptor, string example)
        {
            descriptor.Metadatas["Example"] = example;
            return descriptor;
        }
    }
}
