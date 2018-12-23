using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.Caching;
using Surging.Core.Caching.Configurations;
using Surging.Core.Codec.MessagePack;
using Surging.Core.Consul;
using Surging.Core.Consul.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.DotNetty;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.EventBusRabbitMQ.Configurations;
using Surging.Core.Log4net;
using Surging.Core.Nlog;
using Surging.Core.ProxyGenerator;
using Surging.Core.ServiceHosting;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Core.System.Intercept;
using Surging.IModuleServices.Common;
using System;
//using Surging.Core.Zookeeper;
//using Surging.Core.Zookeeper.Configurations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Services.Client
{
    public class Program
    {
        private static int _endedConnenctionCount = 0;
        private static DateTime begintime;
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var host = new ServiceHostBuilder()
                .RegisterServices(builder =>
                {
                    builder.AddMicroService(option =>
                    {
                        option.AddClient()
                        .AddCache();
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
                    });
                }) 
                .ConfigureLogging(logger =>
                {
                    logger.AddConfiguration(
                        Core.CPlatform.AppConfig.GetSection("Logging"));
                })
                .Configure(build =>
                build.AddEventBusFile("eventBusSettings.json", optional: false))
                .Configure(build =>
                build.AddCacheFile("cacheSettings.json", optional: false, reloadOnChange: true))
                .Configure(build =>
                build.AddCPlatformFile("${surgingpath}|surgingSettings.json", optional: false, reloadOnChange: true))
                .UseClient()
                .UseStartup<Startup>()
                .Build();

            using (host.Run())
            {
                 Startup.Test(ServiceLocator.GetService<IServiceProxyFactory>());
                //Startup.TestRabbitMq(ServiceLocator.GetService<IServiceProxyFactory>());
                // Startup.TestForRoutePath(ServiceLocator.GetService<IServiceProxyProvider>());
                /// test Parallel
                //var connectionCount = 200000;
                //StartRequest(connectionCount);
                //Console.ReadLine();
            }
        }

        private static void StartRequest(int connectionCount)
        {

            var service = ServiceLocator.GetService<IServiceProxyFactory>();
            var userProxy = service.CreateProxy<IUserService>("User");
            Parallel.For(0, connectionCount /1000, new ParallelOptions() { MaxDegreeOfParallelism = 10 },u =>
             {
                 for (var i = 0; i < 1000; i++)
                     Test(userProxy, connectionCount);
             });
        }

        public static void Test(IUserService userProxy,int connectionCount)
        {
            var a = userProxy.GetDictionary().Result;
            IncreaseSuccessConnection(connectionCount);
        }
        
        private static void IncreaseSuccessConnection(int connectionCount)
        {
            Interlocked.Increment(ref _endedConnenctionCount);
            if (_endedConnenctionCount == 1)
                begintime = DateTime.Now;
            if (_endedConnenctionCount >= connectionCount)
                Console.WriteLine($"结束时间{(DateTime.Now - begintime).TotalMilliseconds}");
        }
    }
}
