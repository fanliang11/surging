using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Module
{
    [ServiceBundle("")]
    public interface IEchoService: IServiceKey
    {
        Task<string> Locate(string routePath, string key);
    }
}
