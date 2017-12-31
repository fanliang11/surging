using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Surging.Core.ApiGateWay;
using Surging.Core.Codec.MessagePack;
using Surging.Core.Consul;
using Surging.Core.Consul.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.DotNetty;
using Surging.Core.ProxyGenerator;
using Surging.Core.ServiceHosting;
using Surging.Core.System.Intercept;
using System;
using System.IO;

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
                .ConfigureServices(p =>
                {
                    p.AddAutofac();
                    var builder = new ContainerBuilder();
                    builder.Populate(p);
                    builder.AddMicroService(option =>
                    {
                        option.AddClient();
                        option.AddClientIntercepted(typeof(CacheProviderInterceptor));
                        //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
                        option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));
                        option.UseDotNettyTransport();
                        option.AddApiGateWay();
                        //option.UseProtoBufferCodec();
                        option.UseMessagePackCodec();
                        builder.Register(m => new CPlatformContainer(ServiceLocator.Current));
                    });
                    ServiceLocator.Current = builder.Build();
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
