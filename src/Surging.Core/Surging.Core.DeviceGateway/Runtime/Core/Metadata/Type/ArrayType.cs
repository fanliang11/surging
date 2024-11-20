using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    internal class ArrayType : IDataType, IConverter<List<object>?>, IConverter<object>
    {
        private readonly string _id = "array";

        private readonly string _name = "数组";

        private IDataType _elementType;
        public string GetId()
        {
            return _id;
        }

        public string GetName()
        {
            return _name;
        }

        public ArrayType ElementType(IDataType elementType)
        {
            _elementType = elementType;
            return this;
        }

        public List<object>? Convert(object value)
        {
            if (value == null) return null;
            if (value is Collection)
            {
                return ((Collection<object>)value).Select(p =>
                {
                    if (_elementType is IConverter<object>)
                        return ((IConverter<object>)_elementType).Convert(p);
                    return p;
                }).ToList();
            }
            if (value is string)
            {
                return JsonSerializer.Deserialize<List<object>>(value.ToString());
            }
            return new List<object>() { value };
        }

        public object Format(string format, object value)
        {

            if (_elementType != null && value is Collection)
            {
                var collection = (Collection<object>)value;
                return collection.Select(p => _elementType.Format(format, value)).ToList();
            }

            return JsonSerializer.Serialize(value);
        }



        public bool Validate(object value)
        {
            var list = Convert(value);
            if (_elementType != null && value is Collection)
            {
                foreach (var item in list)
                {
                    var result = _elementType.Validate(item);
                    if (!result)
                    {
                        return result;
                    }
                };
            }
            return true;
        }

        object IConverter<object>.Convert(object value)
        {
            return Convert(value);
        }
    }
}
