using Surging.Core.CPlatform.Routing;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing
{
    public interface IServiceRouteProvider
    {
        Task<ServiceRoute> Locate(string serviceId);

        ValueTask<ServiceRoute> GetRouteByPath(string path);

        Task<ServiceRoute> SearchRoute(string path);

        Task RegisterRoutes(decimal processorTime);

        ValueTask<ServiceRoute> GetRouteByPathRegex(string path);
    }
}
