using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.BrokerService;
using Surging.IModuleServices.BrokerService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.BrokerService.Domain
{
    public class WebService : ProxyServiceBase, IWebService
    {
        public Task SayHello(List<User> familyMember)
        {
            return Task.CompletedTask;
        }

        public Task Say(string name)
        {
            return Task.CompletedTask;
        }
    }
}
