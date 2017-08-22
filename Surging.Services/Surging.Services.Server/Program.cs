using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.Caching.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.EventBus;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Support;
using Surging.Core.DotNetty;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.EventBusRabbitMQ.Configurations;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.ServiceHosting;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Core.System.Ioc;
using Surging.Core.Zookeeper;
using Surging.Core.Zookeeper.Configurations;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
                .UseStartup<Startup>()
                .Build();
        }
    }
}