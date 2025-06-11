using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("Tcp/{Service}")]
    public interface ITcpService : IServiceKey
    {
    }
}
