using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.Caching.Configurations;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.EventBusRabbitMQ.Configurations;

namespace Surging.Services.Server
{
    /// <summary>
    /// Defines the <see cref="Startup" />
    /// </summary>
    public class Startup
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="config">The config<see cref="IConfigurationBuilder"/></param>
        public Startup(IConfigurationBuilder config)
        {
            ConfigureEventBus(config);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Configure
        /// </summary>
        /// <param name="app">The app<see cref="IContainer"/></param>
        public void Configure(IContainer app)
        {
        }

        /// <summary>
        /// The ConfigureServices
        /// </summary>
        /// <param name="builder">The builder<see cref="ContainerBuilder"/></param>
        /// <returns>The <see cref="IContainer"/></returns>
        public IContainer ConfigureServices(ContainerBuilder builder)
        {
            var services = new ServiceCollection();
            ConfigureLogging(services);
            builder.Populate(services);
            ServiceLocator.Current = builder.Build();
            return ServiceLocator.Current;
        }

        /// <summary>
        /// The ConfigureEventBus
        /// </summary>
        /// <param name="build">The build<see cref="IConfigurationBuilder"/></param>
        private static void ConfigureEventBus(IConfigurationBuilder build)
        {
            build
            .AddEventBusFile("eventBusSettings.json", optional: false);
        }

        /// <summary>
        /// 配置缓存服务
        /// </summary>
        /// <param name="build">The build<see cref="IConfigurationBuilder"/></param>
        private void ConfigureCache(IConfigurationBuilder build)
        {
            build
              .AddCacheFile("cacheSettings.json", optional: false);
        }

        /// <summary>
        /// 配置日志服务
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureLogging(IServiceCollection services)
        {
        }

        #endregion 方法
    }
}