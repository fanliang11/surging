using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Surging.Core.Caching.Models;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Cache.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Surging.Core.Caching.HashAlgorithms;
using Microsoft.Extensions.Options;
using Surging.Core.CPlatform.Configurations.Watch;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform;
using Newtonsoft.Json;

namespace Surging.Core.Caching.Configurations.Implementation
{
    public class ConfigurationWatchProvider : ConfigurationWatch, IConfigurationWatchProvider
    {
        #region Field  
        private readonly ILogger<ConfigurationWatchProvider> _logger;
        private readonly IServiceCacheManager _serviceCacheManager;
        private readonly CachingProvider _cachingProvider;
        private Queue<bool> queue = new Queue<bool>();
        #endregion

        public ConfigurationWatchProvider(CPlatformContainer serviceProvider, ILogger<ConfigurationWatchProvider> logger, IServiceCacheManager serviceCacheManager)
        {
           
            if (serviceProvider.IsRegistered<IConfigurationWatchManager>())
                serviceProvider.GetInstances<IConfigurationWatchManager>().Register(this);
            _logger = logger;
            _cachingProvider = AppConfig.Configuration.Get<CachingProvider>();
            _serviceCacheManager = serviceCacheManager;
            _serviceCacheManager.Changed += ServiceCacheManager_Removed;
            _serviceCacheManager.Removed += ServiceCacheManager_Removed;
            _serviceCacheManager.Created += ServiceCacheManager_Add;
        }


        private void ServiceCacheManager_Removed(object sender, ServiceCacheEventArgs e)
        {
            SaveConfiguration(e.Cache);
        }

        private void ServiceCacheManager_Add(object sender, ServiceCacheEventArgs e)
        {
            SaveConfiguration(e.Cache);
        }

        private void SaveConfiguration(ServiceCache cache)
        {
            if (this.queue.Count > 0) this.queue.Dequeue();
            var setting = _cachingProvider.CachingSettings.Where(p => p.Id == cache.CacheDescriptor.Prefix).FirstOrDefault();
            if (setting != null)
            {
                setting.Properties.ForEach(p =>
                {
                    if (p.Maps != null)
                        p.Maps.ForEach(m =>
                    {
                        if (m.Name == cache.CacheDescriptor.Type)
                            m.Properties = cache.CacheEndpoint.Select(n =>
                            {
                                var hashNode = n as ConsistentHashNode;
                                if (!string.IsNullOrEmpty(hashNode.UserName) || !string.IsNullOrEmpty(hashNode.Password))
                                {
                                    return new Property
                                    {
                                        Value = $"{hashNode.UserName}:{hashNode.Password}@{hashNode.Host}:{hashNode.Port}::{hashNode.Db}"
                                    };
                                }
                                return new Property
                                {
                                    Value = $"{hashNode.Host}:{hashNode.Port}::{hashNode.Db}"
                                };

                            }).ToList();
                    });
                });
                this.queue.Enqueue(true);
            }
        }

        public override async Task Process()
        { 
            if (this.queue.Count>0 && this.queue.Dequeue())
            {
                var jsonString = JsonConvert.SerializeObject(_cachingProvider);
                await System.IO.File.WriteAllTextAsync(AppConfig.Path, jsonString);
            }
        }
    }
}
