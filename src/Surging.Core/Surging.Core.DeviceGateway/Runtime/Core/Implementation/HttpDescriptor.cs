using Surging.Core.CPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class HttpDescriptor: ServiceDescriptor
    {
       public static HttpDescriptor  Instance(string path)
        {
            return new HttpDescriptor() { RoutePath=path, Id=path };
        }
    }
}
