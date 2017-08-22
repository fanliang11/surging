using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform;
using Surging.Core.DotNetty;
using Surging.Core.ProxyGenerator;
using Surging.Core.System.Ioc;
using Surging.IModuleServices.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.EventBusRabbitMQ.Configurations;
using Microsoft.Extensions.Configuration;
using Surging.IModuleServices.Common.Models.Events;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing;
using Surging.Core.System.Intercept;
using Surging.IModuleServices.Common.Models;
using Surging.Core.Caching.Configurations;
using Surging.Core.Zookeeper;
using Surging.Core.Zookeeper.Configurations;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Services.Server;
using Surging.Core.ServiceHosting;

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
                .UseStartup<Startup>()
                .Build();
        }
    }
}