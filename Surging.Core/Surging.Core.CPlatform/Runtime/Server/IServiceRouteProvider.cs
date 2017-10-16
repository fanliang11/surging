using Surging.Core.CPlatform.Routing;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Server
{
    public interface IServiceRouteProvider
    {
        Task<ServiceRoute> Locate(string serviceId);
    }
}
