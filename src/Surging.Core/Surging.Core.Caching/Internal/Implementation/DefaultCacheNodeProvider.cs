using Microsoft.Extensions.Configuration;
using Surging.Core.Caching.Models;
using Surging.Core.Caching.RedisCache;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Internal.Implementation
{
    /// <summary>
    /// 默认缓存节点提供者
    /// </summary>
    public class DefaultCacheNodeProvider : ICacheNodeProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceProvider
        /// </summary>
        private readonly CPlatformContainer _serviceProvider;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCacheNodeProvider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        public DefaultCacheNodeProvider(CPlatformContainer serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetServiceCaches
        /// </summary>
        /// <returns>The <see cref="IEnumerable{ServiceCache}"/></returns>
        public IEnumerable<ServiceCache> GetServiceCaches()
        {
            var cacheWrapperSetting = AppConfig.Configuration.Get<CachingProvider>();
            var bingingSettings = cacheWrapperSetting.CachingSettings;
            var serviceCaches = new List<ServiceCache>();
            foreach (var setting in bingingSettings)
            {
                var context = _serviceProvider.GetInstances<RedisContext>(setting.Id);
                foreach (var type in context.dicHash.Keys)
                {
                    var cacheDescriptor = new CacheDescriptor
                    {
                        Id = $"{setting.Id}.{type.ToString()}",
                        Prefix = setting.Id,
                        Type = type
                    };
                    int.TryParse(context.DefaultExpireTime, out int defaultExpireTime);
                    int.TryParse(context.ConnectTimeout, out int connectTimeout);
                    cacheDescriptor.DefaultExpireTime(defaultExpireTime);
                    cacheDescriptor.ConnectTimeout(connectTimeout);
                    var serviceCache = new ServiceCache
                    {
                        CacheDescriptor = cacheDescriptor,
                        CacheEndpoint = context.dicHash[type].GetNodes()
                    };
                    serviceCaches.Add(serviceCache);
                }
            }
            return serviceCaches;
        }

        #endregion 方法
    }
}