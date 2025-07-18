using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class LongType : NumberType<long?>, IConverter<object>
    {
        public readonly string id = "long";
        public readonly string name = "长整型";
        public int decimalPlace = 0;
        public override string GetId()
        {
            return id;
        }

        public string GetName()
        {
            return name;
        }

        protected override long? CastNumber(decimal? num)
        {
            if (num == null) { return null; }
            long.TryParse(num.ToString(), out long result);
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
