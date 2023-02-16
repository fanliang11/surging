using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("api/{Service}/{Method}")]
    [ServiceContract]
    public  interface IWebServiceService : IServiceKey
    {
        [OperationContract]
        Task<string> SayHello(string name);
    }
}
