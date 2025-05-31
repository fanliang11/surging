using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class BooleanType : IDataType, IConverter<bool?>, IConverter<object>
    {
        private readonly string _id = "boolean";

        private readonly string _name = "布尔值";

        private string _trueText = "是";

        private string _falseText = "否";

        private string _trueValue = "true";

        private string _falseValue = "false";
        public BooleanType TrueText(string trueText)
        {
            _trueText = trueText;
            return this;
        }

        public BooleanType FalseText(string falseText)
        {
            _falseText = falseText;
            return this;
        }

        public BooleanType TrueValue(string trueValue)
        {
            _trueValue = trueValue;
            return this;
        }

        public BooleanType FalseValue(string falseValue)
        {
            _falseText = falseValue;
            return this;
        }

        public string GetId()
        {
            return _id;
        }

        public string GetName()
        {
            return _name;
        }

        public bool? Convert(object value)
        {
            if (value == null) return null;
            if (value is bool)
            {
                return (bool)value;
            }

            var stringVal = value.ToString().Trim();
            if (stringVal.Equals(_trueValue) || stringVal.Equals(_trueText))
            {
                return true;
            }

            if (stringVal.Equals(_falseValue) || stringVal.Equals(_falseText))
            {
                return false;
            }
            return stringVal.Equals("1")
                    || stringVal.Equals("true")
                    || stringVal.Equals("y")
                    || stringVal.Equals("yes")
                    || stringVal.Equals("ok")
                    || stringVal.Equals("是")
                    || stringVal.Equals("正常");
        }


        public bool Validate(object value)
        {

            var trueOrFalse = Convert(value);

            return trueOrFalse == null
                    ? false
                    : true;
        }

        public object Format(string format, object value)
        {
            var trueOrFalse = Convert(value);

            if (true.Equals(trueOrFalse))
            {
                return _trueText;
            }
            if (false.Equals(trueOrFalse))
            {
                return _falseText;
            }
            return "unknown:" + value;
        }

        object IConverter<object>.Convert(object value)
        {
            return Convert(value);
        }
    }
}
