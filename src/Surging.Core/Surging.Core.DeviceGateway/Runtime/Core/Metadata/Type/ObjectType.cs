﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class ObjectType : IDataType, IConverter<Dictionary<string, object>>, IConverter<object>
    {
        public readonly string id = "object";
        public readonly string name = "对象类型";

        public List<PropertyMetadata> properties = new List<PropertyMetadata>();
        public Dictionary<string, object> Convert(object value)
        {
            return Handle(value, (valueType, data) =>
            {
                if (valueType is IConverter<object>)
                {
                    return ((IConverter<object>)valueType).Convert(data);
                }
                return data;
            });
        }

        public ObjectType AddPropertyMetadata(PropertyMetadata property)
        {
            properties.Add(property);
            return this;
        }

        public List<PropertyMetadata> GetProperties()
        {
            return properties;
        }


        public PropertyMetadata? GetProperty(string key)
        {
            return properties
                    .Where(p => p.Code == key).FirstOrDefault();
        }

        public ObjectType AddProperty(string property, IDataType dataType)
        {
            return AddProperty(property, property, dataType);
        }

        public ObjectType AddProperty(string code, string name, IDataType type)
        {
            var metadata = new PropertyMetadata(code, name, type);
            return AddPropertyMetadata(metadata);
        }
        public Dictionary<string, object> Handle(object value, Func<IDataType, object, object> mapping)
        {
            if (value == null)
            {
                return null;
            }
            if (value is string && ((string)value).StartsWith("{"))
            {
                value = JsonSerializer.Deserialize<Dictionary<string, object>>(value.ToString());
            }

            if (value is Dictionary<string, object>)
            {
                var mapValue = value as Dictionary<string, object>;
                if (properties != null && mapValue != null)
                {
                    foreach (var property in properties)
                    {
                        mapValue.TryGetValue(property.Code, out object data);
                        var valueType = property.DataType;
                        if (data != null)
                        {
                            mapValue.Add(property.Code, mapping.Invoke(valueType, data));
                        }
                    }
                }
                return mapValue;
            }
            return null;
        }

        public object Format(string format, object value)
        {
            return Handle(value, (valueType, data) =>
            {
                return valueType.Format(format, data);
            });
        }

        public string GetId()
        {
            return id;
        }

        public string GetName()
        {
            return name;
        }

        public bool Validate(object value)
        {
            throw new NotImplementedException();
        }

        object IConverter<object>.Convert(object value)
        {
            return Convert(value);
        }
    }
}
