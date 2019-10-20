using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("api/{Service}/{Method}")]
    public interface IGreeterService : IServiceKey
    {
    }
}
