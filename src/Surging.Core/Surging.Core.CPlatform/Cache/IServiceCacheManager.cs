using Surging.Core.CPlatform.Cache.Implementation;
using Surging.Core.CPlatform.Routing.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Cache
{  
    public interface IServiceCacheManager
    {
        event EventHandler<ServiceCacheEventArgs> Created;
        
        event EventHandler<ServiceCacheEventArgs> Removed;
        
        event EventHandler<ServiceCacheChangedEventArgs> Changed;
        
        Task<IEnumerable<ServiceCache>> GetCachesAsync();
        
        Task SetCachesAsync(IEnumerable<ServiceCache> caches);
        
        Task RemveAddressAsync(IEnumerable<CacheEndpoint> endpoints);
        
        Task ClearAsync();
    }
    
    public static class ServiceRouteManagerExtensions
    {
        public static async Task<ServiceCache> GetAsync(this IServiceCacheManager serviceCacheManager, string cacheId)
        {
            return (await serviceCacheManager.GetCachesAsync()).SingleOrDefault(i => i.CacheDescriptor.Id == cacheId);
        }
    }
}

