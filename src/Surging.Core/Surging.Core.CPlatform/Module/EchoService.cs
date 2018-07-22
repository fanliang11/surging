using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.HashAlgorithms;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Routing;
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
        public EchoService(IHashAlgorithm hashAlgorithm, IServiceRouteProvider serviceRouteProvider, CPlatformContainer container)
        {
            _hashAlgorithm = hashAlgorithm;
            _addressSelector =container.GetInstances<IAddressSelector>(AddressSelectorMode.HashAlgorithm.ToString());
            _serviceRouteProvider = serviceRouteProvider;
        }

        public async Task<string> Locate(string routePath,string key)
        {
            var route= await _serviceRouteProvider.SearchRoute(routePath);
            AddressModel result = new IpAddressModel();
            if (route != null)
            {
                 result = await _addressSelector.SelectAsync(new AddressSelectContext()
                {
                    Address = route.Address,
                    Descriptor = route.ServiceDescriptor,
                    HashCode = _hashAlgorithm.Hash(key)
                });
            }
            return result.ToString();
        }
    }
}
