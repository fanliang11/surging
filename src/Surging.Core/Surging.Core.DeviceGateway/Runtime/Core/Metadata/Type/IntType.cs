using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class IntType : NumberType<int?>, IConverter<object>
    {
        public readonly string id = "int";
        public readonly string name = "整型";
        public int decimalPlace = 0;
        public override string GetId()
        {
            return id;
        }

        public string GetName()
        {
            return name;
        }

        protected override int? CastNumber(decimal? num)
        {
            if (num == null) { return null; }
            int.TryParse(num.ToString(), out int result);
            return result;
        }

        protected override int DefaultDecimalPlace()
        {
            return decimalPlace;
        }

        object IConverter<object>.Convert(object value)
        {
            return Convert(value);
        }
    }
}
