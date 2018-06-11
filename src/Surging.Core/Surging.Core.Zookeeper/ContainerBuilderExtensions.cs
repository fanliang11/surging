using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.Zookeeper.Configurations;
using System;

namespace Surging.Core.Zookeeper
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 设置共享文件路由管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="configInfo">ZooKeeper设置信息。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseZooKeeperRouteManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseRouteManager(provider =>
             new ZooKeeperServiceRouteManager(
                GetConfigInfo(configInfo),
              provider.GetRequiredService<ISerializer<byte[]>>(),
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IServiceRouteFactory>(),
                provider.GetRequiredService<ILogger<ZooKeeperServiceRouteManager>>()));
        }

        /// <summary>
        /// 设置服务命令管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="configInfo">ZooKeeper设置信息。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseZooKeeperCommandManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseCommandManager(provider =>
            {
                var result = new ZookeeperServiceCommandManager(
                    GetConfigInfo(configInfo),
                  provider.GetRequiredService<ISerializer<byte[]>>(),
                    provider.GetRequiredService<ISerializer<string>>(),
                  provider.GetRequiredService<IServiceRouteManager>(),
                    provider.GetRequiredService<IServiceEntryManager>(),
                    provider.GetRequiredService<ILogger<ZookeeperServiceCommandManager>>());
                return result;
            });
        }

        public static IServiceBuilder UseZooKeeperServiceSubscribeManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseSubscribeManager(provider =>
            {
                var result = new ZooKeeperServiceSubscribeManager(
                    GetConfigInfo(configInfo),
                  provider.GetRequiredService<ISerializer<byte[]>>(),
                    provider.GetRequiredService<ISerializer<string>>(),
                    provider.GetRequiredService<IServiceSubscriberFactory>(),
                    provider.GetRequiredService<ILogger<ZooKeeperServiceSubscribeManager>>());
                return result;
            });
        }

        public static IServiceBuilder UseZooKeeperCacheManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseCacheManager(provider =>
             new ZookeeperServiceCacheManager(
               GetConfigInfo(configInfo),
              provider.GetRequiredService<ISerializer<byte[]>>(),
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IServiceCacheFactory>(),
                provider.GetRequiredService<ILogger<ZookeeperServiceCacheManager>>()));
        }


        public static IServiceBuilder UseZooKeeperManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseZooKeeperRouteManager(configInfo)
                .UseZooKeeperCacheManager(configInfo)
                .UseZooKeeperServiceSubscribeManager(configInfo)
                .UseZooKeeperCommandManager(configInfo);
        }

        public static IServiceBuilder UseZooKeeperManager(this IServiceBuilder builder)
        {
            var configInfo = new ConfigInfo(null);
            return builder.UseZooKeeperRouteManager(configInfo)
                .UseZooKeeperCacheManager(configInfo)
                .UseZooKeeperServiceSubscribeManager(configInfo)
                .UseZooKeeperCommandManager(configInfo);
        }


        private static ConfigInfo GetConfigInfo(ConfigInfo config)
        {

            if (AppConfig.Configuration != null)
            {
                var sessionTimeout = config.SessionTimeout.TotalSeconds;
                Double.TryParse(AppConfig.Configuration["SessionTimeout"], out sessionTimeout);
                config = new ConfigInfo(
                    AppConfig.Configuration["ConnectionString"],
                    TimeSpan.FromSeconds(sessionTimeout),
                    AppConfig.Configuration["RoutePath"] ?? config.RoutePath,
                    AppConfig.Configuration["SubscriberPath"] ?? config.SubscriberPath,
                    AppConfig.Configuration["CommandPath"] ?? config.CommandPath,
                    AppConfig.Configuration["CachePath"] ?? config.CachePath,
                    AppConfig.Configuration["ChRoot"] ?? config.ChRoot,
                    AppConfig.Configuration["ReloadOnChange"] != null ? bool.Parse(AppConfig.Configuration["ReloadOnChange"]) :
                    config.ReloadOnChange
                   );
            }
            return config;
        }
    }
}
