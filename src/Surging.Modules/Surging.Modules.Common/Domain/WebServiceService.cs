using Surging.Core.Protocol.WebService.Runtime;
using Surging.IModuleServices.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class WebServiceService : WebServiceBehavior, IWebServiceService
    {
        public Task<string> SayHello(string name)
        {
            return Task.FromResult($"Hello,{name}");
        }
    }
}
