using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Core.System.Ioc;
using Surging.Core.CPlatform;
using Surging.Core.ProxyGenerator;
using Surging.Core.System.Intercept;
using Surging.Core.Consul.Configurations;
using Surging.Core.DotNetty;
using Surging.Core.Consul;
using Autofac;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.ServiceHosting;
using Surging.Core.ServiceHosting.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using System;

namespace Surging.ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseUrls("http://localhost:729")
                .UseKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections =100;
                    options.Limits.MaxConcurrentUpgradedConnections = 100;
                    options.Limits.MaxRequestBodySize = 10 * 1024; 
                    options.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();
            host.Run();
          
        }
    }
}
