using Surging.Core.CPlatform.Cache.Implementation;
using Surging.Core.CPlatform.Routing.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Cache
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceCacheManager" />
    /// </summary>
    public interface IServiceCacheManager
    {
        #region 事件

        /// <summary>
        /// Defines the Changed
        /// </summary>
        event EventHandler<ServiceCacheChangedEventArgs> Changed;

        /// <summary>
        /// Defines the Created
        /// </summary>
        event EventHandler<ServiceCacheEventArgs> Created;

        /// <summary>
        /// Defines the Removed
        /// </summary>
        event EventHandler<ServiceCacheEventArgs> Removed;

        #endregion 事件

        #region 方法

        /// <summary>
        /// The ClearAsync
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        Task ClearAsync();

        /// <summary>
        /// The GetCachesAsync
        /// </summary>
        /// <returns>The <see cref="Task{IEnumerable{ServiceCache}}"/></returns>
        Task<IEnumerable<ServiceCache>> GetCachesAsync();

        /// <summary>
        /// The RemveAddressAsync
        /// </summary>
        /// <param name="endpoints">The endpoints<see cref="IEnumerable{CacheEndpoint}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task RemveAddressAsync(IEnumerable<CacheEndpoint> endpoints);

        /// <summary>
        /// The SetCachesAsync
        /// </summary>
        /// <param name="caches">The caches<see cref="IEnumerable{ServiceCache}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SetCachesAsync(IEnumerable<ServiceCache> caches);

        /// <summary>
        /// The SetCachesAsync
        /// </summary>
        /// <param name="cacheDescriptors">The cacheDescriptors<see cref="IEnumerable{ServiceCacheDescriptor}"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SetCachesAsync(IEnumerable<ServiceCacheDescriptor> cacheDescriptors);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// 服务命令管理者扩展方法。
    /// </summary>
    public static class ServiceRouteManagerExtensions
    {
        #region 方法

        /// <summary>
        /// The GetAsync
        /// </summary>
        /// <param name="serviceCacheManager">The serviceCacheManager<see cref="IServiceCacheManager"/></param>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <returns>The <see cref="Task{ServiceCache}"/></returns>
        public static async Task<ServiceCache> GetAsync(this IServiceCacheManager serviceCacheManager, string cacheId)
        {
            return (await serviceCacheManager.GetCachesAsync()).SingleOrDefault(i => i.CacheDescriptor.Id == cacheId);
        }

        /// <summary>
        /// The GetCacheDescriptorAsync
        /// </summary>
        /// <param name="serviceCacheManager">The serviceCacheManager<see cref="IServiceCacheManager"/></param>
        /// <returns>The <see cref="Task{IEnumerable{CacheDescriptor}}"/></returns>
        public static async Task<IEnumerable<CacheDescriptor>> GetCacheDescriptorAsync(this IServiceCacheManager serviceCacheManager)
        {
            var caches = await serviceCacheManager.GetCachesAsync();
            return caches.Select(p => p.CacheDescriptor);
        }

        /// <summary>
        /// The GetCacheEndpointAsync
        /// </summary>
        /// <param name="serviceCacheManager">The serviceCacheManager<see cref="IServiceCacheManager"/></param>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{CacheEndpoint}}"/></returns>
        public static async Task<IEnumerable<CacheEndpoint>> GetCacheEndpointAsync(this IServiceCacheManager serviceCacheManager, string cacheId)
        {
            var caches = await serviceCacheManager.GetCachesAsync();
            return caches.Where(p => p.CacheDescriptor.Id == cacheId).Select(p => p.CacheEndpoint).FirstOrDefault();
        }

        /// <summary>
        /// The GetCacheEndpointAsync
        /// </summary>
        /// <param name="serviceCacheManager">The serviceCacheManager<see cref="IServiceCacheManager"/></param>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <param name="endpoint">The endpoint<see cref="string"/></param>
        /// <returns>The <see cref="Task{CacheEndpoint}"/></returns>
        public static async Task<CacheEndpoint> GetCacheEndpointAsync(this IServiceCacheManager serviceCacheManager, string cacheId, string endpoint)
        {
            var caches = await serviceCacheManager.GetCachesAsync();
            var cache = caches.Where(p => p.CacheDescriptor.Id == cacheId).Select(p => p.CacheEndpoint).FirstOrDefault();
            return cache.Where(p => p.ToString() == endpoint).FirstOrDefault();
        }

        #endregion 方法
    }
}