
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

namespace Surging.Core.CPlatform
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseServer(this IServiceHostBuilder hostBuilder, string ip, int port, string token="True")
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<IServiceCommandManager>().SetServiceCommandsAsync();
                var serviceEntryManager = mapper.Resolve<IServiceEntryManager>();
                bool enableToken;
                string serviceToken;
                string _ip = ip;
                if (ip.IndexOf(".") < 0 || ip == "" || ip == "0.0.0.0")
                {
                    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface adapter in nics)
                    {
                        if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet && (ip == "" || ip == "0.0.0.0" || ip == adapter.Name))
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

                if (!bool.TryParse(token,out enableToken))
                {
                      serviceToken= token;
                }
                else
                {
                    if(enableToken) serviceToken = Guid.NewGuid().ToString("N");
                    else serviceToken = null;
                }
                var addressDescriptors = serviceEntryManager.GetEntries().Select(i =>
                new ServiceRoute
                {
                    Address = new[] { new IpAddressModel { Ip = _ip, Port = port, Token= serviceToken } },
                    ServiceDescriptor = i.Descriptor
                }).ToList();
                mapper.Resolve<IServiceRouteManager>().SetRoutesAsync(addressDescriptors);
                var serviceHost = mapper.Resolve<Runtime.Server.IServiceHost>();
                Task.Factory.StartNew(async () =>
                {
                    await serviceHost.StartAsync(new IPEndPoint(IPAddress.Parse(_ip), port));
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
    }
}
