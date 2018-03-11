using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.System;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    public class ServiceCacheProvider : ServiceBase, IServiceCacheProvider
    {
        private readonly ISerializer<string> _serializer;
        public ServiceCacheProvider(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }
        public async Task<IEnumerable<CacheDescriptor>> GetServiceDescriptorAsync()
        {
            return await GetService<IServiceCacheManager>().GetCacheDescriptorAsync();
        }

        public async Task<IEnumerable<CacheEndpoint>> GetCacheEndpointAsync(string cacheId)
        {
            return await GetService<IServiceCacheManager>().GetCacheEndpointAsync(cacheId);
        }

        public async Task<CacheEndpoint> GetCacheEndpointAsync(string cacheId, string endpoint)
        {
            return await GetService<IServiceCacheManager>().GetCacheEndpointAsync(cacheId, endpoint);
        }

        public async Task SetCacheEndpointByEndpoint(string cacheId, string endpoint, CacheEndpoint cacheEndpoint)
        {
            var model = await GetService<IServiceCacheManager>().GetAsync(cacheId);

            var cacheEndpoints = model.CacheEndpoint.Where(p => p.ToString() != cacheEndpoint.ToString()).ToList();
            cacheEndpoints.Add(cacheEndpoint);
            model.CacheEndpoint = cacheEndpoints;
            var caches = new ServiceCache[] { model };
            var descriptors = caches.Where(cache => cache != null).Select(cache => new ServiceCacheDescriptor
            {
                AddressDescriptors = cache.CacheEndpoint?.Select(address => new CacheEndpointDescriptor
                {
                    Type = address.GetType().FullName,
                    Value = _serializer.Serialize(address)
                }) ?? Enumerable.Empty<CacheEndpointDescriptor>(),
                CacheDescriptor = cache.CacheDescriptor
            });
            await GetService<IServiceCacheManager>().SetCachesAsync(descriptors);
        }


        public async Task DelCacheEndpointAsync(string cacheId, string endpoint)
        {
            var model = await GetService<IServiceCacheManager>().GetAsync(cacheId);
            var cacheEndpoints = model.CacheEndpoint.Where(p => p.ToString() != endpoint).ToList();
            model.CacheEndpoint = cacheEndpoints;
            var caches = new ServiceCache[] { model };
            var descriptors = caches.Where(cache => cache != null).Select(cache => new ServiceCacheDescriptor
            {
                AddressDescriptors = cache.CacheEndpoint?.Select(address => new CacheEndpointDescriptor
                {
                    Type = address.GetType().FullName,
                    Value = _serializer.Serialize(address)
                }) ?? Enumerable.Empty<CacheEndpointDescriptor>(),
                CacheDescriptor = cache.CacheDescriptor
            });
            await GetService<IServiceCacheManager>().SetCachesAsync(descriptors);
        }
    }
}
