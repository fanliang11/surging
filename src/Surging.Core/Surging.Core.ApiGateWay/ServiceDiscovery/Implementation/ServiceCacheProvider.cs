using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery.Implementation
{
    /// <summary>
    /// 服务缓存提供者
    /// </summary>
    public class ServiceCacheProvider : ServiceBase, IServiceCacheProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _serializer
        /// </summary>
        private readonly ISerializer<string> _serializer;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCacheProvider"/> class.
        /// </summary>
        /// <param name="serializer">The serializer<see cref="ISerializer{string}"/></param>
        public ServiceCacheProvider(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The DelCacheEndpointAsync
        /// </summary>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <param name="endpoint">The endpoint<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        /// <summary>
        /// The GetCacheEndpointAsync
        /// </summary>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{CacheEndpoint}}"/></returns>
        public async Task<IEnumerable<CacheEndpoint>> GetCacheEndpointAsync(string cacheId)
        {
            return await GetService<IServiceCacheManager>().GetCacheEndpointAsync(cacheId);
        }

        /// <summary>
        /// The GetCacheEndpointAsync
        /// </summary>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <param name="endpoint">The endpoint<see cref="string"/></param>
        /// <returns>The <see cref="Task{CacheEndpoint}"/></returns>
        public async Task<CacheEndpoint> GetCacheEndpointAsync(string cacheId, string endpoint)
        {
            return await GetService<IServiceCacheManager>().GetCacheEndpointAsync(cacheId, endpoint);
        }

        /// <summary>
        /// The GetServiceDescriptorAsync
        /// </summary>
        /// <returns>The <see cref="Task{IEnumerable{CacheDescriptor}}"/></returns>
        public async Task<IEnumerable<CacheDescriptor>> GetServiceDescriptorAsync()
        {
            return await GetService<IServiceCacheManager>().GetCacheDescriptorAsync();
        }

        /// <summary>
        /// The SetCacheEndpointByEndpoint
        /// </summary>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <param name="endpoint">The endpoint<see cref="string"/></param>
        /// <param name="cacheEndpoint">The cacheEndpoint<see cref="CacheEndpoint"/></param>
        /// <returns>The <see cref="Task"/></returns>
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

        #endregion 方法
    }
}