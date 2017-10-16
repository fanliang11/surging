using Autofac;
using Surging.Core.Consul;
using Surging.Core.Consul.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.DotNetty;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.ServiceHosting;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Core.System.Ioc;
using System;
using System.Text;

namespace Liuxd.ServerHost.System
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Fuck The World!");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var host = new ServiceHostBuilder()
                .RegisterServices(option =>
                {
                    option.Initialize();
                    option.RegisterServices();
                    option.RegisterRepositories();
                    option.RegisterModules();
                    option.RegisterServiceBus();
                })
                .RegisterServices(builder =>
                {
                    builder.AddMicroService(option =>
                    {
                        option.AddServiceRuntime();
                        // option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
                        option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));
                        option.UseDotNettyTransport();
                        //option.UseRabbitMQTransport();
                        //option.AddRabbitMQAdapt();
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
                    });
                })
                //.SubscribeAt()
                .UseServer("127.0.0.1", 99)
                .UseStartup<Startup>()
                .Build();

            using (host.Run())
            {
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
                Console.ReadLine();
            }
        }
    }
}
