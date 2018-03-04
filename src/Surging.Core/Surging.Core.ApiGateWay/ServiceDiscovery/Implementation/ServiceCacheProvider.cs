using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Cache;
using Surging.Core.System;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    public class ServiceCacheProvider :ServiceBase, IServiceCacheProvider
    {
        public ServiceCacheProvider()
        {

        }
        public async Task<IEnumerable<CacheDescriptor>> GetServiceDescriptorAsync()
        {
            return await GetService<IServiceCacheManager>().GetCacheDescriptorAsync();
        }

        public async Task<IEnumerable<CacheEndpoint>> GetCacheEndpointAsync(string cacheId)
        {
            return await GetService<IServiceCacheManager>().GetCacheEndpointAsync(cacheId);
        }
    }
}
