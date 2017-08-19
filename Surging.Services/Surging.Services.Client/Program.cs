using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform;
using Surging.Core.DotNetty;
using Surging.Core.ProxyGenerator;
using Surging.Core.System.Ioc;
using Surging.IModuleServices.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.EventBusRabbitMQ.Configurations;
using Microsoft.Extensions.Configuration;
using Surging.IModuleServices.Common.Models.Events;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing;
using Surging.Core.System.Intercept;
using Surging.IModuleServices.Common.Models;
using Surging.Core.Caching.Configurations;
using Surging.Core.Zookeeper;
using Surging.Core.Zookeeper.Configurations;

namespace Surging.Services.Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();
            var config = new ConfigurationBuilder()
           .SetBasePath(AppContext.BaseDirectory);
            ConfigureEventBus(config);
            ConfigureLogging(services);
            builder.Populate(services);
            ConfigureService(builder);
            ConfigureCache(config);
            ServiceLocator.Current = builder.Build();
            ServiceLocator.GetService<ILoggerFactory>()
                .AddConsole((c, l) => (int)l >= 3);
            Test(RegisterServiceProx(builder));
            TestRabbitMq();
        }

        /// <summary>
        /// 配置事件总线
        /// </summary>
        public static void ConfigureEventBus(IConfigurationBuilder build)
        {
            build
            .AddEventBusFile("eventBusSettings.json", optional: false);
        }
        
        /// <summary>
        /// 配置相关服务
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static void ConfigureService(ContainerBuilder builder)
        {
            builder.Initialize();
            builder.RegisterServices();
            builder.RegisterRepositories();
            builder.RegisterModules();
            builder.AddMicroService(option => {
                option.AddClient();
                option.AddClientIntercepted(typeof(CacheProviderInterceptor));
                option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
                option.UseDotNettyTransport();
                option.UseRabbitMQTransport();
                builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
            });
        }

        /// <summary>
        /// 配置日志服务
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging();
        }

        /// <summary>
        /// 配置缓存服务
        /// </summary>
        public static void ConfigureCache(IConfigurationBuilder build)
        {
            build
              .AddCacheFile("cacheSettings.json", optional: false);
        }

        /// <summary>
        /// 配置服务代理
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IServiceProxyFactory RegisterServiceProx(ContainerBuilder builder)
        {
            var serviceProxyFactory = ServiceLocator.GetService<IServiceProxyFactory>();
            serviceProxyFactory.RegisterProxType(builder.GetInterfaceService().ToArray());
            return serviceProxyFactory;
        }

        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="serviceProxyFactory"></param>
        public static void Test(IServiceProxyFactory serviceProxyFactory)
        {
            Task.Run(async () =>
            {
                var userProxy = serviceProxyFactory.CreateProxy<IUserService>("User");
                await userProxy.GetUserId("user");
                do
                {
                    Console.WriteLine("正在循环 1w次调用 GetUser.....");
                    //1w次调用
                    var watch = Stopwatch.StartNew();
                    for (var i = 0; i < 10000; i++)
                    {
                       await  userProxy.GetUser(new UserModel { UserId = 1 });
                    }
                    watch.Stop();
                    Console.WriteLine($"1w次调用结束，执行时间：{watch.ElapsedMilliseconds}ms");
                    Console.ReadLine();
                } while (true);
            }).Wait();
        }

        public static void TestRabbitMq()
        {
            ServiceLocator.GetService<IUserService>("User").PublishThroughEventBusAsync(new UserEvent()
            {
                Age = "18",
                Name = "fanly",
                UserId = "1"
            });
        }
    }
}