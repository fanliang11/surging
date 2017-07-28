using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.Zookeeper.Configurations;

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
                configInfo,
              provider.GetRequiredService<ISerializer<byte[]>>(),
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IServiceRouteFactory>(),
                provider.GetRequiredService<ILogger<ZooKeeperServiceRouteManager>>()));
        }
    }
}
