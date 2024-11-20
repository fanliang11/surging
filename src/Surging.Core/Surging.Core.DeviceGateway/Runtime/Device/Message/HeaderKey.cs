using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Message
{
    public  class HeaderKey<T>
    {
        public HeaderKey(string key, T value)
        {
            Key = key;
            DefaultValue = value;
        }

        public string Key { get; set; }
        public T DefaultValue { get; set; }
    }
}
