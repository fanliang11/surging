using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.HashAlgorithms;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Module
{
    public class EchoService : ServiceBase, IEchoService
    {
        private readonly IHashAlgorithm _hashAlgorithm;
        private readonly IAddressSelector _addressSelector;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly IServiceHeartbeatManager _serviceHeartbeatManager;

        public EchoService(IHashAlgorithm hashAlgorithm, IServiceRouteProvider serviceRouteProvider,
            CPlatformContainer container, IServiceHeartbeatManager serviceHeartbeatManager)
        {
            _hashAlgorithm = hashAlgorithm;
            _addressSelector =container.GetInstances<IAddressSelector>(AddressSelectorMode.HashAlgorithm.ToString());
            _serviceRouteProvider = serviceRouteProvider;

            _serviceHeartbeatManager = serviceHeartbeatManager;
        }

        public async Task<IpAddressModel> Locate(string key,string routePath)
        {
            var route= await _serviceRouteProvider.SearchRoute(routePath);
            AddressModel result = new IpAddressModel();
            if (route != null)
            {
                 result = await _addressSelector.SelectAsync(new AddressSelectContext()
                {
                    Address = route.Address,
                    Descriptor = route.ServiceDescriptor,
                    Item = key,
                });
                _serviceHeartbeatManager.AddWhitelist(route.ServiceDescriptor.Id);
            } 
            var ipAddress = result as IpAddressModel;
            return ipAddress;
        }
    }
}
