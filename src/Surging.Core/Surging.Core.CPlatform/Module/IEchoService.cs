using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support.Attributes;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Module
{
    [ServiceBundle("")]
    public interface IEchoService: IServiceKey
    {
        [Command(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        Task<IpAddressModel> Locate(string key,string routePath);
    }
}
