using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Surging.Core.Caching.HashAlgorithms;
using Surging.Core.Caching.Models;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Cache.Implementation;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.Configurations.Watch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Caching.Configurations.Implementation
{
    /// <summary>
    /// 配置watch提供者
    /// </summary>
    public class ConfigurationWatchProvider : ConfigurationWatch, IConfigurationWatchProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _cachingProvider
        /// </summary>
        private readonly CachingProvider _cachingProvider;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<ConfigurationWatchProvider> _logger;

        /// <summary>
        /// Defines the _serviceCacheManager
        /// </summary>
        private readonly IServiceCacheManager _serviceCacheManager;

        /// <summary>
        /// Defines the queue
        /// </summary>
        private Queue<bool> queue = new Queue<bool>();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationWatchProvider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        /// <param name="logger">The logger<see cref="ILogger{ConfigurationWatchProvider}"/></param>
        /// <param name="serviceCacheManager">The serviceCacheManager<see cref="IServiceCacheManager"/></param>
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

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Process
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public override async Task Process()
        {
            if (this.queue.Count > 0 && this.queue.Dequeue())
            {
                var jsonString = JsonConvert.SerializeObject(_cachingProvider);
                await System.IO.File.WriteAllTextAsync(AppConfig.Path, jsonString);
            }
        }

        /// <summary>
        /// The SaveConfiguration
        /// </summary>
        /// <param name="cache">The cache<see cref="ServiceCache"/></param>
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

        /// <summary>
        /// The ServiceCacheManager_Add
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="ServiceCacheEventArgs"/></param>
        private void ServiceCacheManager_Add(object sender, ServiceCacheEventArgs e)
        {
            SaveConfiguration(e.Cache);
        }

        /// <summary>
        /// The ServiceCacheManager_Removed
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="ServiceCacheEventArgs"/></param>
        private void ServiceCacheManager_Removed(object sender, ServiceCacheEventArgs e)
        {
            SaveConfiguration(e.Cache);
        }

        #endregion 方法
    }
}