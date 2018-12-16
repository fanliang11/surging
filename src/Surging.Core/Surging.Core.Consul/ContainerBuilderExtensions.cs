using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.Consul.Configurations;
using Surging.Core.Consul.WatcherProvider;
using Surging.Core.Consul.WatcherProvider.Implementation;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Serialization;
using System;
using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Mqtt;

namespace Surging.Core.Consul
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 设置服务路由管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="configInfo">ZooKeeper设置信息。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseConsulRouteManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseRouteManager(provider =>
             new ConsulServiceRouteManager(
                 GetConfigInfo(configInfo),
              provider.GetRequiredService<ISerializer<byte[]>>(),
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IClientWatchManager>(),
                provider.GetRequiredService<IServiceRouteFactory>(),
                provider.GetRequiredService<ILogger<ConsulServiceRouteManager>>(),
                 provider.GetRequiredService<IServiceHeartbeatManager>()));
        }

        public static IServiceBuilder UseConsulCacheManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseCacheManager(provider =>
             new ConsulServiceCacheManager(
                 GetConfigInfo(configInfo),
              provider.GetRequiredService<ISerializer<byte[]>>(),
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IClientWatchManager>(),
                provider.GetRequiredService<IServiceCacheFactory>(),
                provider.GetRequiredService<ILogger<ConsulServiceCacheManager>>()));
        }

        /// <summary>
        /// 设置服务命令管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="configInfo">ZooKeeper设置信息。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseConsulCommandManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseCommandManager(provider =>
            {
                var result = new ConsulServiceCommandManager(
                     GetConfigInfo(configInfo),
                  provider.GetRequiredService<ISerializer<byte[]>>(),
                    provider.GetRequiredService<ISerializer<string>>(),
                    provider.GetRequiredService <IServiceRouteManager>(),
                    provider.GetRequiredService<IClientWatchManager>(),
                    provider.GetRequiredService<IServiceEntryManager>(),
                    provider.GetRequiredService<ILogger<ConsulServiceCommandManager>>(),
                      provider.GetRequiredService<IServiceHeartbeatManager>());
                return result;
            });
        }

        public static IServiceBuilder UseConsulMqttRouteManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseMqttRouteManager(provider =>
             new ConsulMqttServiceRouteManager(
                 GetConfigInfo(configInfo),
              provider.GetRequiredService<ISerializer<byte[]>>(),
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IClientWatchManager>(),
                provider.GetRequiredService<IMqttServiceFactory>(),
                provider.GetRequiredService<ILogger<ConsulMqttServiceRouteManager>>(),
                 provider.GetRequiredService<IServiceHeartbeatManager>()));
        }

        public static IServiceBuilder UseConsulServiceSubscribeManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseSubscribeManager(provider =>
            {
                var result = new ConsulServiceSubscribeManager(
                    GetConfigInfo(configInfo),
                    provider.GetRequiredService<ISerializer<byte[]>>(),
                    provider.GetRequiredService<ISerializer<string>>(),
                    provider.GetRequiredService<IClientWatchManager>(),
                    provider.GetRequiredService<IServiceSubscriberFactory>(),
                    provider.GetRequiredService<ILogger<ConsulServiceSubscribeManager>>());
                return result;
            });
        }

        /// <summary>
        /// 设置使用基于Consul的Watch机制
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IServiceBuilder UseConsulWatch(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            builder.Services.Register(provider =>
            {
                return new ClientWatchManager(configInfo);
            }).As<IClientWatchManager>().SingleInstance();
            return builder;
        }

        [Obsolete]
        public static IServiceBuilder UseConsulManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseConsulRouteManager(configInfo)
                .UseConsulServiceSubscribeManager(configInfo)
               .UseConsulCommandManager(configInfo)
               .UseConsulCacheManager(configInfo)
               .UseConsulWatch(configInfo)
               .UseConsulMqttRouteManager(configInfo);
        }

        [Obsolete]
        public static IServiceBuilder UseConsulManager(this IServiceBuilder builder)
        {
            var configInfo = new ConfigInfo(null);
            return builder.UseConsulRouteManager(configInfo)
                .UseConsulServiceSubscribeManager(configInfo)
               .UseConsulCommandManager(configInfo)
               .UseConsulCacheManager(configInfo).UseConsulWatch(configInfo)
               .UseConsulMqttRouteManager(configInfo);
        }


        private static ConfigInfo GetConfigInfo(ConfigInfo config)
        {
            ConsulOption option = null;
            var section = CPlatform.AppConfig.GetSection("Consul");
            if (section.Exists())
                option = section.Get<ConsulOption>();
            else if(AppConfig.Configuration!=null)
                option = AppConfig.Configuration.Get<ConsulOption>();
            if (option != null)
            {
                var sessionTimeout = config.SessionTimeout.TotalSeconds;
                Double.TryParse(option.SessionTimeout, out sessionTimeout);
                config = new ConfigInfo(
                   option.ConnectionString,
                    TimeSpan.FromSeconds(sessionTimeout),
                    option.RoutePath ?? config.RoutePath,
                    option.SubscriberPath ?? config.SubscriberPath,
                    option.CommandPath  ?? config.CommandPath,
                    option.CachePath ?? config.CachePath,
                    option.MqttRoutePath ?? config.MqttRoutePath,
                   option.ReloadOnChange != null ? bool.Parse(option.ReloadOnChange) :
                    config.ReloadOnChange,
                    option.EnableChildrenMonitor != null ? bool.Parse(option.EnableChildrenMonitor) :
                    config.EnableChildrenMonitor
                   );
            }
            return config;
        }
    }
}
