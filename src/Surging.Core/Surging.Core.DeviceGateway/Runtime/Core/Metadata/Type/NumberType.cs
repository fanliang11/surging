using DotNetty.Common.Utilities;
using Microsoft.CodeAnalysis;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public abstract class NumberType<T> : IDataType, IConverter<T>
    {
        private readonly bool ORIGINAL = bool.Parse(EnvironmentHelper.GetEnvironmentVariable("${surging.type.number.convert.original}|true"));
        //最大值
        private decimal? _max;

        //最小值
        private decimal? _min;

        private MidpointRounding _round = MidpointRounding.ToEven;

        private int? _decimalPlace = null;

        protected abstract T CastNumber(decimal? num);

        protected abstract int DefaultDecimalPlace();

        public NumberType<T> Round(MidpointRounding round)
        {
            _round = round;
            return this;
        }

        public int GetDecimalPlace(int defaultVal)
        {
            return _decimalPlace == null ? defaultVal : _decimalPlace.Value;
        }

        public NumberType<T> DecimalPlace(int decimalPlace)
        {
            _decimalPlace = decimalPlace;
            return this;
        }

        public NumberType<T> Max(decimal max)
        {
            _max = max;
            return this;
        }

        public NumberType<T> Min(decimal min)
        {
            _min = min;
            return this;
        }

        public long GetMax(long defaultVal)
        {
            if (_max != null)
            {
                return long.Parse(_max.Value.ToString("N0"));
            }
            return defaultVal;
        }

        public long GetMin(long defaultVal)
        {
            if (_min != null)
            {
                return long.Parse(_min.Value.ToString("N0"));
            }
            return defaultVal;
        }

        public double GetMin(double defaultVal)
        {
            if (_min != null)
            {
                return double.Parse(_min.Value.ToString());
            }
            return defaultVal;
        }

        public double GetMax(double defaultVal)
        {
            if (_max != null)
            {
                return double.Parse(_max.Value.ToString());
            }
            return defaultVal;
        }


        public T Convert(object value)
        {
            if (value is decimal || value is int || value is long)
            {
                long.TryParse(value.ToString(), out long result);
                return CastNumber(result);
            }
            return ConvertNumber(value);
        }

        public object Format(string format, object value)
        {
            decimal? decimalVal = ConvertScaleDecimal(value);
            return decimalVal?.ToString();
        }

        public abstract string GetId();

        public T ConvertNumber(object value)
        {
            //保持原始值
            if (ORIGINAL)
            {
                return ConvertOriginalNumber(value);
            }
            return ConvertScaleNumber(value);
        }

        public T ConvertOriginalNumber(object value)
        {
            return ConvertScaleNumber(value, null, null, CastNumber);
        }

        public T ConvertScaleNumber(object value,
                                          int decimalPlace,
                                          MidpointRounding mode)
        {
            return ConvertScaleNumber(value, decimalPlace, mode, CastNumber);
        }

        public T ConvertScaleNumber(object value)
        {
            return ConvertScaleNumber(value, GetDecimalPlace(DefaultDecimalPlace()), _round);
        }

        public decimal? ConvertScaleDecimal(object value)
        {
            return ConvertScaleDecimal(value, GetDecimalPlace(DefaultDecimalPlace()), _round);
        }
        public bool Validate(object value)
        {
            try
            {
                decimal? decimalVal = ConvertScaleDecimal(value);
                if (decimalVal == null)
                {
                    return false;
                }
                if (_max != null && decimalVal > _max)
                {
                    return false;
                }
                if (_min != null && decimalVal < _min)
                {
                    return false;
                }
                return true;
            }
            catch (FormatException e)
            {
                return false;
            }
        }

        public decimal? ConvertScaleDecimal(object value,
                                          int? decimalPlace,
                                          MidpointRounding? mode)
        {
            if (value == null) return null;
            decimal result = decimal.MinValue;
            if (value is decimal && decimalPlace == null)
            {
                result = (decimal)value;
            }
            if (value is string)
            {
                decimal.TryParse(value.ToString(), out result);
            }

            if (value is DateTime)
            {
                result = new decimal(DateTimeConverter.DateTimeToUnixTimestamp((DateTime)value));
            }
            if (result == decimal.MinValue)
            {
                decimal.TryParse(value.ToString(), out result);

            }
            if (decimalPlace == null)
            {
                return result;
            }
            return Math.Round(result, decimalPlace.Value, mode.Value);
        }

        public T ConvertScaleNumber(object value,
                                          int? decimalPlace,
                                          MidpointRounding? mode,
                                          Func<decimal?, T> mapper)
        {
            return mapper.Invoke(ConvertScaleDecimal(value, decimalPlace, mode));
        }



    }
}
