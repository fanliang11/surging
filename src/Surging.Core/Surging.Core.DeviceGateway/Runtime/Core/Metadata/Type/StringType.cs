using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class StringType : IDataType, IConverter<string?>, IConverter<object>
    {
        public readonly string id = "string";

        public readonly string name = "字符串";

        public static  StringType Instance { get; }= new StringType();
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
            return string.IsNullOrEmpty(value?.ToString());
        }

        public object Format(string format, object value)
        {
            return string.Format(format, value);
        }

        public string? Convert(object value)
        {
            return value == null ? null : value.ToString();
        }

        object IConverter<object>.Convert(object value)
        {
            return Convert(value);
        }
    }
}
