using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.Consul.Configurations;
using Surging.Core.Consul.WatcherProvider;
using Surging.Core.Consul.WatcherProvider.Implementation;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Consul
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 设置共享文件路由管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="configInfo">ZooKeeper设置信息。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseConsulRouteManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseRouteManager(provider =>
             new ConsulServiceRouteManager(
                configInfo,
              provider.GetRequiredService<ISerializer<byte[]>>(),
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IClientWatchManager>(),
                provider.GetRequiredService<IServiceRouteFactory>(),
                provider.GetRequiredService<ILogger<ConsulServiceRouteManager>>()));
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
                    configInfo,
                  provider.GetRequiredService<ISerializer<byte[]>>(),
                    provider.GetRequiredService<ISerializer<string>>(),
                    provider.GetRequiredService<IClientWatchManager>(),
                    provider.GetRequiredService<IServiceEntryManager>(),
                    provider.GetRequiredService<ILogger<ConsulServiceCommandManager>>());
                return result;
            });
        }

        public static IServiceBuilder UseConsulServiceSubscribeManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseSubscribeManager(provider =>
            {
                var result = new ConsulServiceSubscribeManager(
                    configInfo,
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
            }).As<IClientWatchManager>();
            return builder;
        }

        public static IServiceBuilder UseConsulManager(this IServiceBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseConsulRouteManager(configInfo)
                .UseConsulServiceSubscribeManager(configInfo)
               .UseConsulCommandManager(configInfo).UseConsulWatch(configInfo);
        }
    }
}
