using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata
{
    public abstract class ConfigMetadata: ConfigVersionSupport
    {
       public abstract ConfigMetadata Copy(params ConfigVersion[] versions);
    }
}
