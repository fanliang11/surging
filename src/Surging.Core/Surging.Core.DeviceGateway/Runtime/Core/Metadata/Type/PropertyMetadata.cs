using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    internal class PropertyMetadata
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IDataType DataType { get; set; }

        public PropertyMetadata(string code, string name, IDataType dataType)
        {
            Code = code;
            Name = name;
            DataType = dataType;
        }
    }
}
