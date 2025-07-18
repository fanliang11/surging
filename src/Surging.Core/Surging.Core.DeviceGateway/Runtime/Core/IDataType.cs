using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core
{
    public interface IDataType
    {
       bool Validate(object value);

        object Format(string format, object value);

        string GetId();
    }
}
