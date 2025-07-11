using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    internal class DoubleType : NumberType<double?>, IConverter<object>
    {
        private readonly string id = "double";
        private readonly string name = "双精度浮点数";
        private int decimalPlace = 2;
        public override string GetId()
        {
            return id;
        }

        public string GetName()
        {
            return name;
        }

        protected override double? CastNumber(decimal? num)
        {
            if (num == null) { return null; }
            double.TryParse(num.ToString(), out double result);
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