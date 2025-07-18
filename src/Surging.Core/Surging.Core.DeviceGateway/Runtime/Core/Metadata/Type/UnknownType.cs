using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class UnknownType : IDataType
    {
        private readonly string id = "unknown";
        private readonly string name = "未知类型";

        public object Format(string format, object value)
        {
            return value?.ToString();
        }

        public string GetId()
        {
            return id;
        }

        public string GetName()
        {
            return name;
        }

        public bool Validate(object value)
        {
            return true;
        }
    }
}
