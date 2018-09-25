using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Support.Attributes;
using Surging.Core.Protocol.WS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("Api/{Service}")]
    public  interface IChatService: IServiceKey
    {
        [Command( ShuntStrategy=AddressSelectorMode.HashAlgorithm)]
        Task SendMessage(string name,string data);
    }
}
