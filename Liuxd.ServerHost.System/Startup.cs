using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.Caching.Configurations;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Support;
using Surging.Core.ProxyGenerator.Utilitys;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Liuxd.ServerHost.System
{
    public class Startup
    {
        public Startup()
        {
            var config = new ConfigurationBuilder()
           .SetBasePath(AppContext.BaseDirectory);
            ConfigureEventBus(config);
            ConfigureCache(config);
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
            app.Resolve<ILoggerFactory>()
                    .AddConsole((c, l) => (int)l >= 1);
        }

        #region 私有方法
        /// <summary>
        /// 配置日志服务
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging();
        }

        private static void ConfigureEventBus(IConfigurationBuilder build)
        {
           // build
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
