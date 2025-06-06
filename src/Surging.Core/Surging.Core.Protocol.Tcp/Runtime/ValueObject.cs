using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Runtime
{
    public class ValueObject
    {
        private readonly Dictionary<string, object> _config;
        public ValueObject(Dictionary<string, object> config) {

            _config = config;
        }

        public T GetVariableValue<T>(string patrVariableName, T defaultValue =default)
        {
            object outValue;
            if (_config.TryGetValue(patrVariableName, out outValue))
            { 
                return (T)Convert.ChangeType(outValue, typeof(T));
            }
            else
            {
                return defaultValue;
            }
        }
 
    }
}
