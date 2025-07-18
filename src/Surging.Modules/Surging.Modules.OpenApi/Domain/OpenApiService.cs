using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.OpenApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.OpenApi.Domain
{
    public class OpenApiService : ProxyServiceBase, IOpenApiService
    {
        public Task<bool> Generater(string name)
        {
            return Task.FromResult(true);
        }

        public Task<string> Query(string name)
        {
            return Task.FromResult(name);
        }
    }
}
