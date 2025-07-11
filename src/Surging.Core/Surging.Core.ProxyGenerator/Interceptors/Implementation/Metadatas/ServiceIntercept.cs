using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation.Metadatas
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public abstract  class ServiceIntercept : ServiceDescriptorAttribute
    {
        protected abstract string MetadataId { get; set; }
    }
}
