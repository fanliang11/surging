using Autofac;
using Microsoft.Extensions.Configuration;
using Surging.Core.Caching.AddressResolvers;
using Surging.Core.Caching.AddressResolvers.Implementation;
using Surging.Core.Caching.Configurations;
using Surging.Core.Caching.Configurations.Implementation;
using Surging.Core.Caching.HashAlgorithms;
using Surging.Core.Caching.HealthChecks;
using Surging.Core.Caching.HealthChecks.Implementation;
using Surging.Core.Caching.Interfaces;
using Surging.Core.Caching.Internal.Implementation;
using Surging.Core.Caching.Models;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;
using Surging.Core.ServiceHosting.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.Caching
{
    /// <summary>
    /// 容器生成扩展 
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        private const string CacheSectionName = "CachingProvider";

        /// <summary>
        /// 附加缓存注入 
        /// </summary>
        /// <param name="builder">服务构建者</param>
        /// <returns>服务构建者</returns>
        public static IServiceBuilder AddCache(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType(typeof(DefaultHealthCheckService)).As(typeof(IHealthCheckService)).SingleInstance();
            services.RegisterType(typeof(DefaultAddressResolver)).As(typeof(IAddressResolver)).SingleInstance();
            services.RegisterType(typeof(HashAlgorithm)).As(typeof(IHashAlgorithm)).SingleInstance();
            services.RegisterType(typeof(DefaultServiceCacheFactory)).As(typeof(IServiceCacheFactory)).SingleInstance();
            services.RegisterType(typeof(DefaultCacheNodeProvider)).As(typeof(ICacheNodeProvider)).SingleInstance();
            services.RegisterType(typeof(ConfigurationWatchProvider)).As(typeof(IConfigurationWatchProvider)).SingleInstance();
            RegisterConfigInstance(services);
            RegisterLocalInstance("ICacheClient`1", services);
            return builder;
        }

        /// <summary>
        /// 注册本地实例 
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="services"></param>
        private static void RegisterLocalInstance(string typeName, ContainerBuilder services)
        {
            var types = typeof(AppConfig)
                        .Assembly.GetTypes().Where(p => p.GetTypeInfo().GetInterface(typeName) != null);
            foreach (var t in types)
            {
                var attribute = t.GetTypeInfo().GetCustomAttribute<IdentifyCacheAttribute>();
                services.RegisterGeneric(t).Named(attribute.Name.ToString(), typeof(ICacheClient<>)).SingleInstance();
            }
        }

        /// <summary>
        /// 注册配置实例 
        /// </summary>
        /// <param name="services"></param>
        private static void RegisterConfigInstance(ContainerBuilder services)
        {
            var cacheWrapperSetting = AppConfig.Configuration.Get<CachingProvider>();
            var bingingSettings = cacheWrapperSetting.CachingSettings;
            try
            {
                var types =
                     typeof(AppConfig)
                        .Assembly.GetTypes()
                        .Where(
                            p => p.GetTypeInfo().GetInterface("ICacheProvider") != null);
                foreach (var t in types)
                {
                    foreach (var setting in bingingSettings)
                    {
                        var properties = setting.Properties;
                        var args = properties.Select(p => GetTypedPropertyValue(p)).ToArray(); ;
                        var maps =
                            properties.Select(p => p.Maps)
                                .FirstOrDefault(p => p != null && p.Any());
                        var type = Type.GetType(setting.Class, throwOnError: true);
                        services.Register(p => Activator.CreateInstance(type, args)).Named(setting.Id, type).SingleInstance();

                        if (maps == null) continue;
                        if (!maps.Any()) continue;
                        foreach (
                            var mapsetting in
                                maps.Where(mapsetting => t.Name.StartsWith(mapsetting.Name, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            services.Register(p => Activator.CreateInstance(t, new object[] { setting.Id })).Named(string.Format("{0}.{1}", setting.Id, mapsetting.Name), typeof(ICacheProvider)).SingleInstance();
                        }
                    }
                    var attribute = t.GetTypeInfo().GetCustomAttribute<IdentifyCacheAttribute>();
                    if (attribute != null)
                        services.Register(p => Activator.CreateInstance(t)).Named(attribute.Name.ToString(), typeof(ICacheProvider)).SingleInstance();
                }
            }
            catch { }
        }

        /// <summary>
        /// 获取类型的属性值 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static object GetTypedPropertyValue(Property obj)
        {
            var mapCollections = obj.Maps;
            if (mapCollections != null && mapCollections.Any())
            {
                var results = new List<object>();
                foreach (var map in mapCollections)
                {
                    object items = null;
                    if (map.Properties != null) items = map.Properties.Select(p => GetTypedPropertyValue(p)).ToArray();
                    results.Add(new
                    {
                        Name = Convert.ChangeType(obj.Name, typeof(string)),
                        Value = Convert.ChangeType(map.Name, typeof(string)),
                        Items = items
                    });
                }
                return results;
            }
            else if (!string.IsNullOrEmpty(obj.Value))
            {
                return new
                {
                    Name = Convert.ChangeType(obj.Name ?? "", typeof(string)),
                    Value = Convert.ChangeType(obj.Value, typeof(string)),
                };
            }
            else if (!string.IsNullOrEmpty(obj.Ref))
                return Convert.ChangeType(obj.Ref, typeof(string));

            return null;
        }
    }
}
