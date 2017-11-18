using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.Caching.Configurations;
using Surging.Core.EventBusRabbitMQ.Configurations;
using Surging.Core.ProxyGenerator;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.System.Ioc;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.Common.Models.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Surging.Services.Client
{
    public class Startup
    {
        private ContainerBuilder _builder;
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
            _builder = builder;
            ServiceLocator.Current = builder.Build();
            return ServiceLocator.Current;
        }

        public void Configure(IContainer app)
        {
            app.Resolve<ILoggerFactory>()
                    .AddConsole((c, l) => (int)l >= 3);
            RegisterServiceProx(_builder);
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
                        var a = userProxy.GetUser(new UserModel { UserId = 1 }).Result;
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
        #endregion

    }
}
