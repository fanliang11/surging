﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Mqtt;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Support;
using Surging.Core.Zookeeper.Configurations;
using Surging.Core.Zookeeper.Internal;
using Surging.Core.Zookeeper.Internal.Cluster.HealthChecks;
using Surging.Core.Zookeeper.Internal.Cluster.HealthChecks.Implementation;
using Surging.Core.Zookeeper.Internal.Cluster.Implementation.Selectors;
using Surging.Core.Zookeeper.Internal.Cluster.Implementation.Selectors.Implementation;
using Surging.Core.Zookeeper.Internal.Implementation;
using System;

namespace Surging.Core.Zookeeper
{
    /// <summary>
    /// Defines the <see cref="ZookeeperModule" />
    /// </summary>
    public class ZookeeperModule : EnginePartModule
    {
        #region 方法

        /// <summary>
        /// The Initialize
        /// </summary>
        /// <param name="context">The context<see cref="AppModuleContext"/></param>
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        /// <summary>
        /// The UseCacheManager
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="factory">The factory<see cref="Func{IServiceProvider, IServiceCacheManager}"/></param>
        /// <returns>The <see cref="ContainerBuilderWrapper"/></returns>
        public ContainerBuilderWrapper UseCacheManager(ContainerBuilderWrapper builder, Func<IServiceProvider, IServiceCacheManager> factory)
        {
            builder.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        /// <summary>
        /// The UseCommandManager
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="factory">The factory<see cref="Func{IServiceProvider, IServiceCommandManager}"/></param>
        /// <returns>The <see cref="ContainerBuilderWrapper"/></returns>
        public ContainerBuilderWrapper UseCommandManager(ContainerBuilderWrapper builder, Func<IServiceProvider, IServiceCommandManager> factory)
        {
            builder.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        /// <summary>
        /// The UseHealthCheck
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <returns>The <see cref="ZookeeperModule"/></returns>
        public ZookeeperModule UseHealthCheck(ContainerBuilderWrapper builder)
        {
            builder.RegisterType<DefaultHealthCheckService>().As<IHealthCheckService>().SingleInstance();
            return this;
        }

        /// <summary>
        /// The UseMqttRouteManager
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="factory">The factory<see cref="Func{IServiceProvider, IMqttServiceRouteManager}"/></param>
        /// <returns>The <see cref="ContainerBuilderWrapper"/></returns>
        public ContainerBuilderWrapper UseMqttRouteManager(ContainerBuilderWrapper builder, Func<IServiceProvider, IMqttServiceRouteManager> factory)
        {
            builder.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        /// <summary>
        /// The UseRouteManager
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="factory">The factory<see cref="Func{IServiceProvider, IServiceRouteManager}"/></param>
        /// <returns>The <see cref="ContainerBuilderWrapper"/></returns>
        public ContainerBuilderWrapper UseRouteManager(ContainerBuilderWrapper builder, Func<IServiceProvider, IServiceRouteManager> factory)
        {
            builder.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        /// <summary>
        /// The UseSubscribeManager
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="factory">The factory<see cref="Func{IServiceProvider, IServiceSubscribeManager}"/></param>
        /// <returns>The <see cref="ContainerBuilderWrapper"/></returns>
        public ContainerBuilderWrapper UseSubscribeManager(ContainerBuilderWrapper builder, Func<IServiceProvider, IServiceSubscribeManager> factory)
        {
            builder.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        /// <summary>
        /// The UseZookeeperAddressSelector
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <returns>The <see cref="ZookeeperModule"/></returns>
        public ZookeeperModule UseZookeeperAddressSelector(ContainerBuilderWrapper builder)
        {
            builder.RegisterType<ZookeeperRandomAddressSelector>().As<IZookeeperAddressSelector>().SingleInstance();
            return this;
        }

        /// <summary>
        /// The UseZooKeeperCacheManager
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="configInfo">The configInfo<see cref="ConfigInfo"/></param>
        /// <returns>The <see cref="ZookeeperModule"/></returns>
        public ZookeeperModule UseZooKeeperCacheManager(ContainerBuilderWrapper builder, ConfigInfo configInfo)
        {
            UseCacheManager(builder, provider =>
             new ZookeeperServiceCacheManager(
               GetConfigInfo(configInfo),
              provider.GetRequiredService<ISerializer<byte[]>>(),
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IServiceCacheFactory>(),
                provider.GetRequiredService<ILogger<ZookeeperServiceCacheManager>>(),
                  provider.GetRequiredService<IZookeeperClientProvider>()));
            return this;
        }

        /// <summary>
        /// The UseZookeeperClientProvider
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="configInfo">The configInfo<see cref="ConfigInfo"/></param>
        /// <returns>The <see cref="ZookeeperModule"/></returns>
        public ZookeeperModule UseZookeeperClientProvider(ContainerBuilderWrapper builder, ConfigInfo configInfo)
        {
            UseZooKeeperClientProvider(builder, provider =>
        new DefaultZookeeperClientProvider(
            GetConfigInfo(configInfo),
         provider.GetRequiredService<IHealthCheckService>(),
           provider.GetRequiredService<IZookeeperAddressSelector>(),
           provider.GetRequiredService<ILogger<DefaultZookeeperClientProvider>>()));
            return this;
        }

        /// <summary>
        /// The UseZooKeeperClientProvider
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="factory">The factory<see cref="Func{IServiceProvider, IZookeeperClientProvider}"/></param>
        /// <returns>The <see cref="ContainerBuilderWrapper"/></returns>
        public ContainerBuilderWrapper UseZooKeeperClientProvider(ContainerBuilderWrapper builder, Func<IServiceProvider, IZookeeperClientProvider> factory)
        {
            builder.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        /// <summary>
        /// 设置服务命令管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="configInfo">ZooKeeper设置信息。</param>
        /// <returns>服务构建者。</returns>
        public ZookeeperModule UseZooKeeperCommandManager(ContainerBuilderWrapper builder, ConfigInfo configInfo)
        {
            UseCommandManager(builder, provider =>
           {
               var result = new ZookeeperServiceCommandManager(
                   GetConfigInfo(configInfo),
                 provider.GetRequiredService<ISerializer<byte[]>>(),
                   provider.GetRequiredService<ISerializer<string>>(),
                 provider.GetRequiredService<IServiceRouteManager>(),
                   provider.GetRequiredService<IServiceEntryManager>(),
                   provider.GetRequiredService<ILogger<ZookeeperServiceCommandManager>>(),
                  provider.GetRequiredService<IZookeeperClientProvider>());
               return result;
           });
            return this;
        }

        /// <summary>
        /// The UseZooKeeperMqttRouteManager
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="configInfo">The configInfo<see cref="ConfigInfo"/></param>
        /// <returns>The <see cref="ZookeeperModule"/></returns>
        public ZookeeperModule UseZooKeeperMqttRouteManager(ContainerBuilderWrapper builder, ConfigInfo configInfo)
        {
            UseMqttRouteManager(builder, provider =>
          {
              var result = new ZooKeeperMqttServiceRouteManager(
                   GetConfigInfo(configInfo),
                 provider.GetRequiredService<ISerializer<byte[]>>(),
                   provider.GetRequiredService<ISerializer<string>>(),
                   provider.GetRequiredService<IMqttServiceFactory>(),
                   provider.GetRequiredService<ILogger<ZooKeeperMqttServiceRouteManager>>(),
                  provider.GetRequiredService<IZookeeperClientProvider>());
              return result;
          });
            return this;
        }

        /// <summary>
        /// 设置共享文件路由管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="configInfo">ZooKeeper设置信息。</param>
        /// <returns>服务构建者。</returns>
        public ZookeeperModule UseZooKeeperRouteManager(ContainerBuilderWrapper builder, ConfigInfo configInfo)
        {
            UseRouteManager(builder, provider =>
          new ZooKeeperServiceRouteManager(
             GetConfigInfo(configInfo),
           provider.GetRequiredService<ISerializer<byte[]>>(),
             provider.GetRequiredService<ISerializer<string>>(),
             provider.GetRequiredService<IServiceRouteFactory>(),
             provider.GetRequiredService<ILogger<ZooKeeperServiceRouteManager>>(),
                  provider.GetRequiredService<IZookeeperClientProvider>()));
            return this;
        }

        /// <summary>
        /// The UseZooKeeperServiceSubscribeManager
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilderWrapper"/></param>
        /// <param name="configInfo">The configInfo<see cref="ConfigInfo"/></param>
        /// <returns>The <see cref="ZookeeperModule"/></returns>
        public ZookeeperModule UseZooKeeperServiceSubscribeManager(ContainerBuilderWrapper builder, ConfigInfo configInfo)
        {
            UseSubscribeManager(builder, provider =>
            {
                var result = new ZooKeeperServiceSubscribeManager(
                    GetConfigInfo(configInfo),
                  provider.GetRequiredService<ISerializer<byte[]>>(),
                    provider.GetRequiredService<ISerializer<string>>(),
                    provider.GetRequiredService<IServiceSubscriberFactory>(),
                    provider.GetRequiredService<ILogger<ZooKeeperServiceSubscribeManager>>(),
                  provider.GetRequiredService<IZookeeperClientProvider>());
                return result;
            });
            return this;
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            var configInfo = new ConfigInfo(null);
            UseZookeeperAddressSelector(builder)
            .UseHealthCheck(builder)
            .UseZookeeperClientProvider(builder, configInfo)
            .UseZooKeeperRouteManager(builder, configInfo)
            .UseZooKeeperCacheManager(builder, configInfo)
            .UseZooKeeperMqttRouteManager(builder, configInfo)
            .UseZooKeeperServiceSubscribeManager(builder, configInfo)
            .UseZooKeeperCommandManager(builder, configInfo);
        }

        /// <summary>
        /// The GetConfigInfo
        /// </summary>
        /// <param name="config">The config<see cref="ConfigInfo"/></param>
        /// <returns>The <see cref="ConfigInfo"/></returns>
        private static ConfigInfo GetConfigInfo(ConfigInfo config)
        {
            ZookeeperOption option = null;
            var section = CPlatform.AppConfig.GetSection("Zookeeper");
            if (section.Exists())
                option = section.Get<ZookeeperOption>();
            else if (AppConfig.Configuration != null)
                option = AppConfig.Configuration.Get<ZookeeperOption>();
            if (option != null)
            {
                var sessionTimeout = config.SessionTimeout.TotalSeconds;
                Double.TryParse(option.SessionTimeout, out sessionTimeout);
                config = new ConfigInfo(
                    option.ConnectionString,
                    TimeSpan.FromSeconds(sessionTimeout),
                    option.RoutePath ?? config.RoutePath,
                    option.SubscriberPath ?? config.SubscriberPath,
                    option.CommandPath ?? config.CommandPath,
                    option.CachePath ?? config.CachePath,
                    option.MqttRoutePath ?? config.MqttRoutePath,
                    option.ChRoot ?? config.ChRoot,
                    option.ReloadOnChange != null ? bool.Parse(option.ReloadOnChange) :
                    config.ReloadOnChange,
                   option.EnableChildrenMonitor != null ? bool.Parse(option.EnableChildrenMonitor) :
                    config.EnableChildrenMonitor
                   );
            }
            return config;
        }

        #endregion 方法
    }
}