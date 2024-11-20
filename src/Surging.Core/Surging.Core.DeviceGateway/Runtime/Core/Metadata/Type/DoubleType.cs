using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    internal class DoubleType : NumberType<double?>, IConverter<object>
    {
        private readonly string _id = "double";
        private readonly string _name = "双精度浮点数";
        private int _decimalPlace = 2;
        public override string GetId()
        {
            return _id;
        }

        public string GetName()
        {
            return _name;
        }

        protected override double? CastNumber(decimal? num)
        {
            if (num == null) { return null; }
            double.TryParse(num.ToString(), out double result);
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