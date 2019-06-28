using Autofac;
using Surging.Core.CPlatform.Support;
using System.Linq;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Address;
using System.Threading.Tasks;
using Surging.Core.ServiceHosting.Internal;
using Surging.Core.CPlatform.Runtime.Server;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Runtime.Client;
using System;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.Module;
using System.Diagnostics;
using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Utilities;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;
using Surging.Core.CPlatform.Transport.Implementation;

namespace Surging.Core.CPlatform
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseServer(this IServiceHostBuilder hostBuilder, string ip, int port, string token = "True")
        {
            return hostBuilder.MapServices(async mapper =>
            {
                BuildServiceEngine(mapper);
                await mapper.Resolve<IServiceCommandManager>().SetServiceCommandsAsync();
                string serviceToken = mapper.Resolve<IServiceTokenGenerator>().GeneratorToken(token);
                int _port = AppConfig.ServerOptions.Port = AppConfig.ServerOptions.Port == 0 ? port : AppConfig.ServerOptions.Port;
                string _ip = AppConfig.ServerOptions.Ip = AppConfig.ServerOptions.Ip ?? ip;
                _port = AppConfig.ServerOptions.Port = AppConfig.ServerOptions.IpEndpoint?.Port ?? _port;
                _ip = AppConfig.ServerOptions.Ip = AppConfig.ServerOptions.IpEndpoint?.Address.ToString() ?? _ip;
                _ip = NetUtils.GetHostAddress(_ip);

                await ConfigureRoute(mapper, serviceToken);
                mapper.Resolve<IModuleProvider>().Initialize();
                var serviceHosts = mapper.Resolve<IList<Runtime.Server.IServiceHost>>();
                Task.Factory.StartNew(async () =>
                {
                    foreach (var serviceHost in serviceHosts)
                        await serviceHost.StartAsync(_ip, _port);
                    mapper.Resolve<IServiceEngineLifetime>().NotifyStarted();
                }).Wait();
            });
        }

        public static IServiceHostBuilder UseServer(this IServiceHostBuilder hostBuilder, Action<SurgingServerOptions> options)
        {
            var serverOptions = new SurgingServerOptions();
            options.Invoke(serverOptions);
            AppConfig.ServerOptions = serverOptions;
            return hostBuilder.UseServer(serverOptions.Ip, serverOptions.Port, serverOptions.Token);
        }

        public static IServiceHostBuilder UseClient(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                var serviceEntryManager = mapper.Resolve<IServiceEntryManager>();
                var addressDescriptors = serviceEntryManager.GetEntries().Select(i =>
                {
                    i.Descriptor.Metadatas = null;
                    return new ServiceSubscriber
                    {
                        Address = new[] { new IpAddressModel {
                             Ip = Dns.GetHostEntry(Dns.GetHostName())
                             .AddressList.FirstOrDefault<IPAddress>
                             (a => a.AddressFamily.ToString().Equals("InterNetwork")).ToString() } },
                        ServiceDescriptor = i.Descriptor
                    };
                }).ToList();
                mapper.Resolve<IServiceSubscribeManager>().SetSubscribersAsync(addressDescriptors);
                mapper.Resolve<IModuleProvider>().Initialize();
            });
        }

        public static void BuildServiceEngine(IContainer container)
        {
            if (container.IsRegistered<IServiceEngine>())
            {
                var builder = new ContainerBuilder();

                container.Resolve<IServiceEngineBuilder>().Build(builder);
                var configBuilder = container.Resolve<IConfigurationBuilder>();
                var appSettingPath = Path.Combine(AppConfig.ServerOptions.RootPath, "appsettings.json");
                configBuilder.AddCPlatformFile("${appsettingspath}|" + appSettingPath, optional: false, reloadOnChange: true);
                builder.Update(container);
            }
        }

        public static async Task ConfigureRoute(IContainer mapper, string serviceToken)
        {
            if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.Tcp ||
             AppConfig.ServerOptions.Protocol == CommunicationProtocol.None)
            {
                var routeProvider = mapper.Resolve<IServiceRouteProvider>();
                if (AppConfig.ServerOptions.EnableRouteWatch)
                    new ServiceRouteWatch(mapper.Resolve<CPlatformContainer>(),
                        async () => await routeProvider.RegisterRoutes(
                        Math.Round(Convert.ToDecimal(Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds), 2, MidpointRounding.AwayFromZero)));
                else
                    await routeProvider.RegisterRoutes(0);
            }
        }

    }
}
