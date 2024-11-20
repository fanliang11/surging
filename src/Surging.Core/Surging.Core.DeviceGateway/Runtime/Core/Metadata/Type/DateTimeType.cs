using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class DateTimeType : IDataType, IConverter<DateTime?>, IConverter<object>
    {
        private readonly string _id = "datetime";

        private readonly string _name = "时间";
        private DateTimeKind _kind = DateTimeKind.Local;
        private IFormatProvider _formatProvider = new CultureInfo("zh-CN");

        public DateTime? Convert(object value)
        {
            if (value == null) return null;
            if (value is DateTime)
            {
                return DateTime.SpecifyKind((DateTime)value, _kind);
            }
            if (value is DateTimeOffset)
            {
                return ((DateTimeOffset)value).DateTime;
            }
            if (value is long || value is int)
            {
                return new DateTime((long)value);
            }
            if (value is string)
            {
                if (long.TryParse(value.ToString(), out long result))
                {
                    return new DateTime((long)value);
                }
                if (DateTime.TryParse(value.ToString(), _formatProvider, DateTimeStyles.None, out DateTime dt))
                {
                    return DateTime.SpecifyKind(dt, _kind);
                }
                else
                {
                    throw new ArgumentException("unsupported date format:" + value);
                }
            }
            throw new ArgumentException("unsupported date format:" + value);
        }

        public DateTimeType Kind(DateTimeKind kind)
        {
            _kind = kind;
            return this;
        }

        public DateTimeType Format(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
            return this;
        }

        public object Format(string format, object value)
        {
            var dateValue = Convert(value);
            if (dateValue == DateTime.MinValue)
            {
                return "";
            }
            return string.Format(format, DateTime.SpecifyKind(dateValue.Value, _kind));
        }

        public string GetId()
        {
            return _id;
        }

        public string GetName()
        {
            return _name;
        }

        public bool Validate(object value)
        {
            var dateTime = Convert(value);
            if (dateTime == DateTime.MinValue)
            {
                return false;
            }
            return true;
        }

        object IConverter<object>.Convert(object value)
        {
            return Convert(value);
        }
    }
}
