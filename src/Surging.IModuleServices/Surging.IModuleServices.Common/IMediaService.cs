using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.Protocol.WS.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("Api/{Service}")]
    [BehaviorContract(IgnoreExtensions =true,Protocol = "media")]
    public interface IMediaService : IServiceKey
    { 
        Task Push(IEnumerable<byte> data);
    }

}
