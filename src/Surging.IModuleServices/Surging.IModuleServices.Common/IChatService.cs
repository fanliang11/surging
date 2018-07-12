using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.Protocol.WS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("Api/{Service}")]
    public  interface IChatService: IServiceKey
    {
    }
}
