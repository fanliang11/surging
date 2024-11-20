using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata
{
    public  class ConfigVersion
    {
        public string VersionNum { get; }

        public ConfigVersion(string versionNum) 
        { 
            VersionNum = versionNum; 
        }
    }
}
