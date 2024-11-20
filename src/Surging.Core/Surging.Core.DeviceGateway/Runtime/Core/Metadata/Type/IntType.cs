using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class IntType : NumberType<int?>, IConverter<object>
    {
        private readonly string _id = "int";
        private readonly string _name = "整型";
        private int _decimalPlace = 0;
        public override string GetId()
        {
            return _id;
        }

        public string GetName()
        {
            return _name;
        }

        protected override int? CastNumber(decimal? num)
        {
            if (num == null) { return null; }
            int.TryParse(num.ToString(), out int result);
            return result;
        }

        protected override int DefaultDecimalPlace()
        {
            return _decimalPlace;
        }

        object IConverter<object>.Convert(object value)
        {
            return Convert(value);
        }
    }
}
