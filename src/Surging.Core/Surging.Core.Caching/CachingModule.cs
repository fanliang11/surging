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
using Surging.Core.CPlatform.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Surging.Core.Caching
{
    /// <summary>
    /// Defines the <see cref="CachingModule" />
    /// </summary>
    public class CachingModule : EnginePartModule
    {
        #region 方法

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="AppModuleContext"/></param>
        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            base.Initialize(context);
            var serviceCacheProvider = serviceProvider.GetInstances<ICacheNodeProvider>();
            var addressDescriptors = serviceCacheProvider.GetServiceCaches().ToList();
            serviceProvider.GetInstances<IServiceCacheManager>().SetCachesAsync(addressDescriptors);
            serviceProvider.GetInstances<IConfigurationWatchProvider>();
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.RegisterType(typeof(DefaultHealthCheckService)).As(typeof(IHealthCheckService)).SingleInstance();
            builder.RegisterType(typeof(DefaultAddressResolver)).As(typeof(IAddressResolver)).SingleInstance();
            builder.RegisterType(typeof(HashAlgorithm)).As(typeof(IHashAlgorithm)).SingleInstance();
            builder.RegisterType(typeof(DefaultServiceCacheFactory)).As(typeof(IServiceCacheFactory)).SingleInstance();
            builder.RegisterType(typeof(DefaultCacheNodeProvider)).As(typeof(ICacheNodeProvider)).SingleInstance();
            builder.RegisterType(typeof(ConfigurationWatchProvider)).As(typeof(IConfigurationWatchProvider)).SingleInstance();
            RegisterConfigInstance(builder);
            RegisterLocalInstance("ICacheClient`1", builder);
        }

        /// <summary>
        /// The GetTypedPropertyValue
        /// </summary>
        /// <param name="obj">The obj<see cref="Property"/></param>
        /// <returns>The <see cref="object"/></returns>
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

        /// <summary>
        /// The RegisterConfigInstance
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        private static void RegisterConfigInstance(ContainerBuilderWrapper builder)
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
                        builder.Register(p => Activator.CreateInstance(type, args)).Named(setting.Id, type).SingleInstance();

                        if (maps == null) continue;
                        if (!maps.Any()) continue;
                        foreach (
                            var mapsetting in
                                maps.Where(mapsetting => t.Name.StartsWith(mapsetting.Name, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            builder.Register(p => Activator.CreateInstance(t, new object[] { setting.Id })).Named(string.Format("{0}.{1}", setting.Id, mapsetting.Name), typeof(ICacheProvider)).SingleInstance();
                        }
                    }
                    var attribute = t.GetTypeInfo().GetCustomAttribute<IdentifyCacheAttribute>();
                    if (attribute != null)
                        builder.Register(p => Activator.CreateInstance(t)).Named(attribute.Name.ToString(), typeof(ICacheProvider)).SingleInstance();
                }
            }
            catch { }
        }

        /// <summary>
        /// The RegisterLocalInstance
        /// </summary>
        /// <param name="typeName">The typeName<see cref="string"/></param>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        private static void RegisterLocalInstance(string typeName, ContainerBuilderWrapper builder)
        {
            var types = typeof(AppConfig)
                        .Assembly.GetTypes().Where(p => p.GetTypeInfo().GetInterface(typeName) != null);
            foreach (var t in types)
            {
                var attribute = t.GetTypeInfo().GetCustomAttribute<IdentifyCacheAttribute>();
                builder.RegisterGeneric(t).Named(attribute.Name.ToString(), typeof(ICacheClient<>)).SingleInstance();
            }
        }

        #endregion 方法
    }
}