using Autofac;
using Surging.Core.CPlatform;
using Surging.Core.DotNetty;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.ServiceHosting;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Core.System.Ioc;
using Surging.Core.Zookeeper;
using Surging.Core.Zookeeper.Configurations;
using System.Text;

namespace Surging.Services.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
             Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var host = new ServiceHostBuilder()
                .RegisterServices(option=> {
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
                        option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
                        option.UseDotNettyTransport();
                        option.UseRabbitMQTransport();
                        option.AddRabbitMQAdapt();
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
                    });
                })
                .UseStartup<Startup>()
                .Build();
        }
    }
}