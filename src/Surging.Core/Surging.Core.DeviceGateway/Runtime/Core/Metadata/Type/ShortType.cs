using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class ShortType : NumberType<short?>, IConverter<object>
    {
        public readonly string id = "short";
        public readonly string name = "短整型";
        private int _decimalPlace = 0;
        public override string GetId()
        {
            return id;
        }

        public string GetName()
        {
            return name;
        }

        protected override short? CastNumber(decimal? num)
        {
            if (num == null) { return null; }
            short.TryParse(num.ToString(), out short result);
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
