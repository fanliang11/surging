using Autofac;
using Surging.Core.Consul;
using Surging.Core.Consul.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.DotNetty;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.ProxyGenerator;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.ServiceHosting;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Core.System.Intercept;
using Surging.Core.System.Ioc;
using Surging.Core.Zookeeper;
//using Surging.Core.Zookeeper.Configurations;
using Surging.Services.Server;
using System.Text;

namespace Surging.Services.Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var host = new ServiceHostBuilder()
                .RegisterServices(option =>
                {
                    option.Initialize();
                    option.RegisterServices();
                    option.RegisterRepositories();
                    option.RegisterModules();
                })
                .RegisterServices(builder =>
                {
                    builder.AddMicroService(option =>
                    {
                        option.AddClient();
                        option.AddClientIntercepted(typeof(CacheProviderInterceptor));
                       //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
                       option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));
                        option.UseDotNettyTransport();
                        option.UseRabbitMQTransport();
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
                    });
                })
                .UseClient()
                .UseStartup<Startup>()
                .Build();

            using (host.Run())
            {
                Startup.Test(ServiceLocator.GetService<IServiceProxyFactory>());
                Startup.TestRabbitMq();
            }
        }
    }
}
