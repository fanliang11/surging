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
using System.Text;
using Surging.Core.Log4net;
using Surging.Core.ProxyGenerator;

namespace Centa.HostService.Org
{
    public class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var host = new ServiceHostBuilder()
                   //.RegisterServices(provider =>
                   //{
                   //    //这里写自己的注册
                   //})
                .RegisterServices(builder =>
                {
                    builder.AddMicroService(option =>
                    {
                        option.AddServiceRuntime();
                        option.AddRelateService();
                        option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));
                        option.UseDotNettyTransport();
                        option.UseRabbitMQTransport();
                        option.AddRabbitMQAdapt();
                        //option.UseMessagePackCodec();
                        option.UseJsonCodec(); 
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
                    });
                })
                .SubscribeAt()
                .UseLog4net("Configs/log4net.config")
                .UseServer(options =>
                               {
                                   options.Ip = "127.0.0.1";
                                   options.Port = 10242;
                                   options.Token = "True";
                                   options.ExecutionTimeoutInMilliseconds = 30000;
                                   options.MaxConcurrentRequests = 200;
                               })
                .UseProxy()
                .UseStartup<Startup>()
                .Build();

            using (host.Run())
            {
                Console.Title = "组织管理";
                Console.WriteLine($"组织管理——服务端启动成功，{DateTime.Now}。");
                Console.ReadLine();
            }
        }
    }
}
