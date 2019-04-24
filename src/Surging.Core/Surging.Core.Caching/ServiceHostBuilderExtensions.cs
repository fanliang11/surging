using Microsoft.Extensions.Configuration;
using Surging.Core.Caching.Models;
using Surging.Core.ServiceHosting.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using System.Reflection;
using Surging.Core.Caching.Interfaces;
using Surging.Core.CPlatform.Cache;
using Surging.Core.Caching.Configurations;

namespace Surging.Core.Caching
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseServiceCache(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                var serviceCacheProvider = mapper.Resolve<ICacheNodeProvider>();
                var addressDescriptors = serviceCacheProvider.GetServiceCaches().ToList();
                mapper.Resolve<IServiceCacheManager>().SetCachesAsync(addressDescriptors);
                mapper.Resolve<IConfigurationWatchProvider>();
            });
        }
        
    }
}
