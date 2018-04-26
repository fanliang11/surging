

using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System.Threading.Tasks;

namespace Surging.IModuleServices.User
{

    [ServiceBundle("api/{Service}")]
    public interface IManagerService: IServiceKey
    {
        Task<string> SayHello(string name);
    }
}
