using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Core.Caching;
using Surging.Core.Caching.Configurations;
using Surging.Core.Codec.MessagePack;
using Surging.Core.Consul;
using Surging.Core.Consul.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.DependencyResolution;
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
using System.Diagnostics;
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
                .UseProxy()
                .UseStartup<Startup>()
                .Build();

            using (host.Run())
            {
                Startup.Test(ServiceLocator.GetService<IServiceProxyFactory>());
                //Startup.TestRabbitMq(ServiceLocator.GetService<IServiceProxyFactory>());
                // Startup.TestForRoutePath(ServiceLocator.GetService<IServiceProxyProvider>());
                /// test Parallel 
                //var connectionCount = 300000;
                //StartRequest(connectionCount);
                //Console.ReadLine();
            }
        }

        private static void StartRequest(int connectionCount)
        {
            // var service = ServiceLocator.GetService<IServiceProxyFactory>(); 
            var sw = new Stopwatch();
            sw.Start();
            var userProxy = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<IUserService>("User");
            ServiceResolver.Current.Register("User", userProxy);
            var service = ServiceLocator.GetService<IServiceProxyFactory>();
            userProxy = ServiceResolver.Current.GetService<IUserService>("User");
            sw.Stop();
            Console.WriteLine($"代理所花{sw.ElapsedMilliseconds}ms");
            ThreadPool.SetMinThreads(100, 100);
            Parallel.For(0, connectionCount / 6000, new ParallelOptions() { MaxDegreeOfParallelism = 50 }, async u =>
               {
                   for (var i = 0; i < 6000; i++)
                       await Test(userProxy, connectionCount);
               });
        }

        public static async Task Test(IUserService userProxy,int connectionCount)
        {
            var a =await userProxy.GetDictionary();
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
