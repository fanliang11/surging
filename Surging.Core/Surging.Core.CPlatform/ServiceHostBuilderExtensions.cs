
using Autofac;
using Surging.Core.CPlatform.Support;
using System.Linq;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Address;
using System.Threading.Tasks;
using Surging.Core.ServiceHosting.Internal;
using Surging.Core.CPlatform.Runtime.Server;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Surging.Core.CPlatform
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseServer(this IServiceHostBuilder hostBuilder,string ip,int port)
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<IServiceCommandManager>().SetServiceCommandsAsync();
                var serviceEntryManager = mapper.Resolve<IServiceEntryManager>();
                var addressDescriptors = serviceEntryManager.GetEntries().Select(i =>
                new ServiceRoute
                {
                    Address = new[] { new IpAddressModel { Ip = ip, Port = port } },
                    ServiceDescriptor = i.Descriptor
                }).ToList();
                mapper.Resolve<IServiceRouteManager>().SetRoutesAsync(addressDescriptors);
                var serviceHost = mapper.Resolve<Runtime.Server.IServiceHost>();
                Task.Factory.StartNew(async () =>
                {
                    await serviceHost.StartAsync(new IPEndPoint(IPAddress.Parse(ip), port));
                }).Wait();
            });
        }

        public static IServiceHostBuilder UseClient(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
               
            });
        }
    }
}
