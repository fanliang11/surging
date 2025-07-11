using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class PasswordType : IDataType, IConverter<string?>, IConverter<object>
    {
        public readonly string id = "password";

        public readonly string name = "密码";
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
