using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery
{
    public interface IServiceCacheProvider
    {
        Task<IEnumerable<CacheDescriptor>> GetServiceDescriptorAsync();

        Task<IEnumerable<CacheEndpoint>> GetCacheEndpointAsync(string cacheId);
    }
}
