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
    public class DefaultCacheNodeProvider : ICacheNodeProvider
    {
        private readonly CPlatformContainer _serviceProvider;
        public DefaultCacheNodeProvider(CPlatformContainer serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

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
    }
}
