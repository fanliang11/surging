
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

namespace Surging.Core.CPlatform
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseServer(this IServiceHostBuilder hostBuilder, string ip, int port, string token="True")
        {
            return hostBuilder.MapServices(mapper =>
            {
                BuildServiceEngine(mapper);
                mapper.Resolve<IServiceCommandManager>().SetServiceCommandsAsync();
                var serviceEntryManager = mapper.Resolve<IServiceEntryManager>();
                string serviceToken = mapper.Resolve<IServiceTokenGenerator>().GeneratorToken(token);
                int _port = AppConfig.ServerOptions.Port==0? port: AppConfig.ServerOptions.Port;
                string _ip = AppConfig.ServerOptions.Ip??ip;
                _port = AppConfig.ServerOptions.IpEndpoint?.Port ?? _port;
                _ip = AppConfig.ServerOptions.IpEndpoint?.Address.ToString() ?? _ip;
               

                if (_ip.IndexOf(".") < 0 || _ip == "" || _ip == "0.0.0.0")
                {
                    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface adapter in nics)
                    {
                        if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet && (_ip == "" || _ip == "0.0.0.0" || _ip == adapter.Name))
                        {
                            IPInterfaceProperties ipxx = adapter.GetIPProperties();
                            UnicastIPAddressInformationCollection ipCollection = ipxx.UnicastAddresses;
                            foreach (UnicastIPAddressInformation ipadd in ipCollection)
                            {
                                if (ipadd.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    _ip = ipadd.Address.ToString();
                                }
                            }
                        }
                    }
                }
                var mappingIp= AppConfig.ServerOptions.MappingIP ?? _ip;
                var mappingPort = AppConfig.ServerOptions.MappingPort;
                if (mappingPort == 0)
                    mappingPort = _port;
                new ServiceRouteWatch(mapper.Resolve<CPlatformContainer>(),  () =>
                {
                    var addressDescriptors = serviceEntryManager.GetEntries().Select(i =>
                    new ServiceRoute
                    {
                        Address = new[] { new IpAddressModel { Ip = mappingIp, Port = mappingPort, ProcessorTime = Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds, Token = serviceToken } },
                        ServiceDescriptor = i.Descriptor
                    }).ToList();
                    mapper.Resolve<IServiceRouteManager>().SetRoutesAsync(addressDescriptors);
                });

                mapper.Resolve<IModuleProvider>().Initialize();
                var serviceHost = mapper.Resolve<Runtime.Server.IServiceHost>();
                Task.Factory.StartNew(async () =>
                {
                    await serviceHost.StartAsync(new IPEndPoint(IPAddress.Parse(_ip), _port));
                }).Wait();
            });
        }

        public static IServiceHostBuilder UseServer(this IServiceHostBuilder hostBuilder, Action<SurgingServerOptions> options)
        {
            var serverOptions = new SurgingServerOptions();
            options.Invoke(serverOptions);
            AppConfig.ServerOptions = serverOptions;
            return hostBuilder.UseServer(serverOptions.Ip,serverOptions.Port,serverOptions.Token);
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

            });
        }

        public static void BuildServiceEngine(IContainer container)
        {
            if(container.IsRegistered<IServiceEngine>())
            {
                using (var soap = container.BeginLifetimeScope(
                  builder =>
                  {
                      container.Resolve<IServiceEngineBuilder>().Build(builder);
                  })) {}
            }
        }
    }
}
