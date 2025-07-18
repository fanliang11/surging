using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata
{
    public abstract class ConfigPropertyMetadata : ConfigVersionSupport
    {
        public abstract string Code { get; }

        public abstract string Name { get; }

        public abstract string Description { get;  }

        public abstract IDataType ValueType{get;  }

    }
}
