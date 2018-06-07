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
using Surging.Core.EventBusKafka.Configurations;
//using Surging.Core.EventBusKafka;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.Log4net;
using Surging.Core.Nlog;
using Surging.Core.ProxyGenerator;
using Surging.Core.ServiceHosting;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Core.Zookeeper.Configurations;
using System;
//using Surging.Core.Zookeeper;
//using Surging.Core.Zookeeper.Configurations;
using System.Text;

namespace Surging.Services.Server
{
    public class Program
    {
        static void Main(string[] args)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var host = new ServiceHostBuilder()
                .RegisterServices(builder =>
                {
                    builder.AddMicroService(option =>
                    {
                        option.AddServiceRuntime()
                        .AddRelateService()
                        .AddConfigurationWatch()
                        //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
                        .UseConsulManager()
                        .UseDotNettyTransport()
                        .UseRabbitMQTransport()
                        .AddRabbitMQAdapt()
                        .AddCache()
                        .AddServiceEngine(typeof(SurgingServiceEngine))
                        //.UseKafkaMQTransport(kafkaOption =>
                        //{
                        //    kafkaOption.Servers = "127.0.0.1";
                        //    kafkaOption.LogConnectionClose = false;
                        //    kafkaOption.MaxQueueBuffering = 10;
                        //    kafkaOption.MaxSocketBlocking = 10;
                        //    kafkaOption.EnableAutoCommit = false;
                        //})
                        //.AddKafkaMQAdapt()
                        //.UseProtoBufferCodec()
                        .UseMessagePackCodec();
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
                    });
                })
                .SubscribeAt() 
               // .UseLog4net(LogLevel.Error, "Configs/log4net.config")
                .UseNLog(LogLevel.Error, "Configs/NLog.config")
                //.UseServer("127.0.0.1", 98)
                //.UseServer("127.0.0.1", 98，“true”) //自动生成Token
                //.UseServer("127.0.0.1", 98，“123456789”) //固定密码Token
                .UseServer(options =>
                {
                    // options.IpEndpoint = new IPEndPoint(IPAddress.Any, 98);  
                    options.Token = "True";
                    options.ExecutionTimeoutInMilliseconds = 30000;
                    options.MaxConcurrentRequests = 200;
                })
                .UseServiceCache()
                // .Configure(build =>
                //build.AddZookeeperFile("Configs/zookeeper.json", optional: false))
               .Configure(build =>
                build.AddConsulFile("Configs/consul.json", optional: false))
                .Configure(build =>
                build.AddEventBusFile("eventBusSettings.json", optional: false))
                .Configure(build =>
                build.AddCacheFile("cacheSettings.json", optional: false,reloadOnChange:true))
                  .Configure(build =>
                build.AddCPlatformFile("surgingSettings.json", optional: false, reloadOnChange: true))
                .UseProxy()
                .UseStartup<Startup>()
                .Build();

            using (host.Run())
            {
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            }
        }
    }
}