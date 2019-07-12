using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.ServiceDiscovery
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IServiceCacheProvider" />
    /// </summary>
    public interface IServiceCacheProvider
    {
        #region 方法

        /// <summary>
        /// The DelCacheEndpointAsync
        /// </summary>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <param name="endpoint">The endpoint<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task DelCacheEndpointAsync(string cacheId, string endpoint);

        /// <summary>
        /// The GetCacheEndpointAsync
        /// </summary>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <returns>The <see cref="Task{IEnumerable{CacheEndpoint}}"/></returns>
        Task<IEnumerable<CacheEndpoint>> GetCacheEndpointAsync(string cacheId);

        /// <summary>
        /// The GetCacheEndpointAsync
        /// </summary>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <param name="endpoint">The endpoint<see cref="string"/></param>
        /// <returns>The <see cref="Task{CacheEndpoint}"/></returns>
        Task<CacheEndpoint> GetCacheEndpointAsync(string cacheId, string endpoint);

        /// <summary>
        /// The GetServiceDescriptorAsync
        /// </summary>
        /// <returns>The <see cref="Task{IEnumerable{CacheDescriptor}}"/></returns>
        Task<IEnumerable<CacheDescriptor>> GetServiceDescriptorAsync();

        /// <summary>
        /// The SetCacheEndpointByEndpoint
        /// </summary>
        /// <param name="cacheId">The cacheId<see cref="string"/></param>
        /// <param name="endpoint">The endpoint<see cref="string"/></param>
        /// <param name="cacheEndpoint">The cacheEndpoint<see cref="CacheEndpoint"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task SetCacheEndpointByEndpoint(string cacheId, string endpoint, CacheEndpoint cacheEndpoint);

        #endregion 方法
    }

    #endregion 接口
}