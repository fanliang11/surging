using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    internal class LongType : NumberType<long?>, IConverter<object>
    {
        private readonly string _id = "long";
        private readonly string _name = "长整型";
        private int _decimalPlace = 0;
        public override string GetId()
        {
            return _id;
        }

        public string GetName()
        {
            return _name;
        }

        protected override long? CastNumber(decimal? num)
        {
            if (num == null) { return null; }
            long.TryParse(num.ToString(), out long result);
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
