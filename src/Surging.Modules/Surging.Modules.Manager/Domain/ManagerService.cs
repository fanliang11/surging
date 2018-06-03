using System.Threading.Tasks;
using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.User;

namespace Surging.Modules.Manager.Domain
{
    public class ManagerService : ProxyServiceBase, IIdentityService
    {
        public Task<string> SayHello(string name)
        {
              return Task.FromResult($"{name} say:hello");
        }
    }
}
