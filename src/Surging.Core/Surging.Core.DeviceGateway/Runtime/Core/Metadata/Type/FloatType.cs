using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class FloatType : NumberType<float?>, IConverter<object>
    {
        private readonly string _id = "float";
        private readonly string _name = "单精度浮点数";
        private int _decimalPlace = 2;
        public override string GetId()
        {
            return _id;
        }

        public string GetName()
        {
            return _name;
        }

        protected override float? CastNumber(decimal? num)
        {
            if (num == null) { return null; }
            float.TryParse(num.ToString(), out float result);
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
