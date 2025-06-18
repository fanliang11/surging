using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.Protokollwandler.Metadatas;
using Surging.IModuleServices.BrokerService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.BrokerService
{
    [ServiceBundle("api/{Service}/{Method}")]
    public interface IWebService:IServiceKey
    {
          [TransferContract(Name = "webservice", RoutePath = "/WebService1.asmx/SayHello", Type = TransferContractType.SoapWebService)]
        //[TransferContract(Name = "restservice", RoutePath = "/api/values/SayHello", Type = TransferContractType.Rest)]
        [HttpPost(true), HttpPut(true), HttpDelete(true), HttpGet(true)]

        Task SayHello(List<User> familyMember);

        [TransferContract(Name = "restservice", RoutePath = "/api/values/Say", Type = TransferContractType.Rest)]
        Task Say(string name);
    }
}
