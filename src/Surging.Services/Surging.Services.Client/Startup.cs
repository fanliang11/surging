using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Surging.Core.Caching.Configurations;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.EventBusRabbitMQ.Configurations;
using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.Common.Models.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Surging.Services.Client
{
    /// <summary>
    /// Defines the <see cref="Startup" />
    /// </summary>
    public class Startup
    {
        #region 字段

        /// <summary>
        /// Defines the _builder
        /// </summary>
        private ContainerBuilder _builder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="config">The config<see cref="IConfigurationBuilder"/></param>
        public Startup(IConfigurationBuilder config)
        {
            ConfigureEventBus(config);
            ConfigureCache(config);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="serviceProxyFactory"></param>
        public static void Test(IServiceProxyFactory serviceProxyFactory)
        {
            Task.Run(async () =>
            {
                RpcContext.GetContext().SetAttachment("xid", 124);
                var userProxy = serviceProxyFactory.CreateProxy<IUserService>("User");
                var e = userProxy.SetSex(Sex.Woman).GetAwaiter().GetResult();
                var v = userProxy.GetUserId("fanly").GetAwaiter().GetResult();
                var fa = userProxy.GetUserName(1).GetAwaiter().GetResult();
                userProxy.Try().GetAwaiter().GetResult();
                var v1 = userProxy.GetUserLastSignInTime(1).Result;
                var things = userProxy.GetAllThings().Result;
                var apiResult = userProxy.GetApiResult().GetAwaiter().GetResult();
                userProxy.PublishThroughEventBusAsync(new UserEvent
                {
                    UserId = 1,
                    Name = "fanly"
                }).Wait();

                userProxy.PublishThroughEventBusAsync(new UserEvent
                {
                    UserId = 1,
                    Name = "fanly"
                }).Wait();

                var r = await userProxy.GetDictionary();
                var serviceProxyProvider = ServiceLocator.GetService<IServiceProxyProvider>();

                do
                {
                    Console.WriteLine("正在循环 1w次调用 GetUser.....");

                    //1w次调用
                    var watch = Stopwatch.StartNew();
                    for (var i = 0; i < 10000; i++)
                    {
                        //var a = userProxy.GetDictionary().Result;
                        var a = await userProxy.GetDictionary();
                        //var result = serviceProxyProvider.Invoke<object>(new Dictionary<string, object>(), "api/user/GetDictionary", "User").Result;
                    }
                    watch.Stop();
                    Console.WriteLine($"1w次调用结束，执行时间：{watch.ElapsedMilliseconds}ms");
                    Console.WriteLine("Press any key to continue, q to exit the loop...");
                    var key = Console.ReadLine();
                    if (key.ToLower() == "q")
                        break;
                } while (true);
            }).Wait();
        }

        /// <summary>
        /// The TestForRoutePath
        /// </summary>
        /// <param name="serviceProxyProvider">The serviceProxyProvider<see cref="IServiceProxyProvider"/></param>
        public static void TestForRoutePath(IServiceProxyProvider serviceProxyProvider)
        {
            Dictionary<string, object> model = new Dictionary<string, object>();
            model.Add("user", JsonConvert.SerializeObject(new
            {
                Name = "fanly",
                Age = 18,
                UserId = 1,
                Sex = "Man"
            }));
            string path = "api/user/getuser";
            string serviceKey = "User";

            var userProxy = serviceProxyProvider.Invoke<object>(model, path, serviceKey);
            var s = userProxy.Result;
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// The TestRabbitMq
        /// </summary>
        /// <param name="serviceProxyFactory">The serviceProxyFactory<see cref="IServiceProxyFactory"/></param>
        public static void TestRabbitMq(IServiceProxyFactory serviceProxyFactory)
        {
            serviceProxyFactory.CreateProxy<IUserService>("User").PublishThroughEventBusAsync(new UserEvent()
            {
                Age = 18,
                Name = "fanly",
                UserId = 1
            });
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

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
            _builder = builder;
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
            services.AddLogging();
        }

        #endregion 方法
    }
}