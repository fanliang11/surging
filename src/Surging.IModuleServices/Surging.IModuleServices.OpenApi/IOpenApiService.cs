using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.IModuleServices.OpenApi.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.OpenApi
{
    [ServiceBundle("api/{Method}")]
    public interface IOpenApiService : IServiceKey
    {
        [OpenApi]
        public Task<bool> Generater(string name);
         
        Task<string> Query(string name);
    }
}
