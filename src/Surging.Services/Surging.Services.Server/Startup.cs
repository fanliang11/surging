using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.Caching.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Threading;

namespace Surging.Services.Server
{
    public class Startup
    {
        public Startup(IConfigurationBuilder config)
        {
            ThreadPool.SetMinThreads(100, 100);
            Environment.SetEnvironmentVariable("io.netty.allocator.maxOrder", "5");
            Environment.SetEnvironmentVariable("io.netty.allocator.numDirectArenas", "0");
            //According to the server process long time task
            Environment.SetEnvironmentVariable("io.netty.eventexecutor.maxPendingTasks", AppConfig.ServerOptions.MaxPendingTasks.ToString());
            Environment.SetEnvironmentVariable("io.netty.allocator.type", "unpooled");
            Environment.SetEnvironmentVariable("io.netty.allocator.numHeapArenas", "0");
            Environment.SetEnvironmentVariable("io.netty.leakDetection.level", "disabled");
            ConfigureEventBus(config);
          //  ConfigureCache(config);
        }

        public IContainer ConfigureServices(ContainerBuilder builder)
        {
            var services = new ServiceCollection();
            ConfigureLogging(services);
            builder.Populate(services);
            ServiceLocator.Current = builder.Build();
            return ServiceLocator.Current;
        }

        public void Configure(IContainer app)
        {
   
        }

        #region 私有方法
        /// <summary>
        /// 配置日志服务
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureLogging(IServiceCollection services)
        {
           // services.AddLogging();
        }

        private static void ConfigureEventBus(IConfigurationBuilder build)
        {
          //  build
           // .AddEventBusFile("eventBusSettings.json", optional: false);
        }

        /// <summary>
        /// 配置缓存服务
        /// </summary>
        private void ConfigureCache(IConfigurationBuilder build)
        {
            build
              .AddCacheFile("cacheSettings.json", optional: false);
        }
        #endregion

    }
}
