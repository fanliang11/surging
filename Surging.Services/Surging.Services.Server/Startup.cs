using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.Caching.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Support;
using Surging.Core.DotNetty;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.EventBusRabbitMQ.Configurations;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.Zookeeper;
using Surging.Core.Zookeeper.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Services.Server
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
            builder.AddMicroService(option =>
            {
                option.AddServiceRuntime();
                option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
                option.UseDotNettyTransport();
                option.UseRabbitMQTransport();
                option.AddRabbitMQAdapt();
                builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
            });
            ServiceLocator.Current = builder.Build();
            return ServiceLocator.Current;
        }

        public void Configure(IContainer app)
        {
            app.Resolve<ILoggerFactory>()
                    .AddConsole((c, l) => (int)l >= 3);
            ConfigureRoutes();
            app.Resolve<ISubscriptionAdapt>().SubscribeAt();
            app.Resolve<IServiceCommandManager>().SetServiceCommandsAsync();
            StartService();
            Console.ReadLine();
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
            build
            .AddEventBusFile("eventBusSettings.json", optional: false);
        }

        /// <summary>
        /// 配置缓存服务
        /// </summary>
        private void ConfigureCache(IConfigurationBuilder build)
        {
            build
              .AddCacheFile("cacheSettings.json", optional: false);
        }

        /// <summary>
        ///添加路由列表， 有利于测试，
        /// </summary>
        private void ConfigureRoutes()
        {
            var serviceEntryManager = ServiceLocator.GetService<IServiceEntryManager>();
            var addressDescriptors = serviceEntryManager.GetEntries().Select(i =>
            new ServiceRoute
            {
                Address = new[] { new IpAddressModel { Ip = "127.0.0.1", Port = 98 } },
                ServiceDescriptor = i.Descriptor
            }).ToList();
            var serviceRouteManager = ServiceLocator.GetService<IServiceRouteManager>();
            serviceRouteManager.SetRoutesAsync(addressDescriptors);
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        private void StartService()
        {
            var serviceHost = ServiceLocator.GetService<IServiceHost>();
            Task.Factory.StartNew(async () =>
            {
                await serviceHost.StartAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 98));
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            }).Wait();
        }
        #endregion

    }
}
