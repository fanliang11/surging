using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message.Function
{
    public class FunctionParameter
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public override string ToString()
        {
            return Name + "(" + Value + ")";
        }

    }
}
