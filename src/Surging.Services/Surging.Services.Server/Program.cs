using Autofac;
using Surging.Core.Codec.MessagePack;
using Surging.Core.Consul;
using Surging.Core.Consul.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.DotNetty;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.ServiceHosting;
using Surging.Core.ServiceHosting.Internal.Implementation;
using System;
//using Surging.Core.Zookeeper;
//using Surging.Core.Zookeeper.Configurations;
using System.Text;
using Surging.Core.Log4net;
using Surging.Core.ProxyGenerator;

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
                        option.AddServiceRuntime();
                        option.AddRelateService();
                        //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
                        option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));
                        option.UseDotNettyTransport();
                        option.UseRabbitMQTransport();
                        option.AddRabbitMQAdapt();
                        //option.UseProtoBufferCodec();
                        option.UseMessagePackCodec();
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
                    });
                })
                .SubscribeAt()
                .UseLog4net("Configs/log4net.config")
                //.UseServer("127.0.0.1", 98)
                //.UseServer("127.0.0.1", 98，“true”) //自动生成Token
                //.UseServer("127.0.0.1", 98，“123456789”) //固定密码Token
                .UseServer(options=> {
                    options.Ip = "127.0.0.1";
                    options.Port = 98;
                    options.Token = "True";
                    options.ExecutionTimeoutInMilliseconds = 30000;
                    options.MaxConcurrentRequests = 200;
                })
                .UseStartup<Startup>()
                .Build();

            using (host.Run())
            {
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            }
        }
    }
}