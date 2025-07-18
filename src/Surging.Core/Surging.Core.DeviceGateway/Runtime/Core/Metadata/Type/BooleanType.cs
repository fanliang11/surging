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
        public readonly string id = "boolean";

        public readonly string name = "布尔值";

        public string trueText = "是";

        public string falseText = "否";

        public string trueValue = "true";

        public string falseValue = "false";
        public BooleanType TrueText(string trueText)
        {
            trueText = trueText;
            return this;
        }

        public BooleanType FalseText(string falseText)
        {
            falseText = falseText;
            return this;
        }

        public BooleanType TrueValue(string trueValue)
        {
            trueValue = trueValue;
            return this;
        }

        public BooleanType FalseValue(string falseValue)
        {
            falseText = falseValue;
            return this;
        }

        public string GetId()
        {
            return id;
        }

        public string GetName()
        {
            return name;
        }

        public bool? Convert(object value)
        {
            if (value == null) return null;
            if (value is bool)
            {
                return (bool)value;
            }

            var stringVal = value.ToString().Trim();
            if (stringVal.Equals(trueValue) || stringVal.Equals(trueText))
            {
                return true;
            }

            if (stringVal.Equals(falseValue) || stringVal.Equals(falseText))
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
                return trueText;
            }
            if (false.Equals(trueOrFalse))
            {
                return falseText;
            }
            return "unknown:" + value;
        }

        object IConverter<object>.Convert(object value)
        {
            return Convert(value);
        }
    }
}
