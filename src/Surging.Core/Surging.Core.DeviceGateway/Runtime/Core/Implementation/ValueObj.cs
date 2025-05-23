using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class ValueObj : object
    {
        private readonly object _value;
        private readonly ITypeConvertibleService _typeConvertibleService;
        public ValueObj(object value){
            _value = value;
            _typeConvertibleService = ServiceLocator.GetService<ITypeConvertibleService>();

        }

        public T Convert<T>()
        {
            if (this != null)
            {
                var result = _typeConvertibleService.Convert(_value, typeof(T));
                return (T)result;
            }
            return default;
        }
    }

    public class ValueObjs:Dictionary<string, object>
    { 
        public ValueObjs()
        {
        }

         public ValueObj GetValue(string key)
        {
            this.TryGetValue(key, out var value);
            return new ValueObj(value);
        }
    }
}
