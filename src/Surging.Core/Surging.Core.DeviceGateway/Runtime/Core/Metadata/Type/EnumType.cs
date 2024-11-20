using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type.EnumType;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class EnumType : IDataType, IConverter<object>
    {
        public readonly string _id = "enum";

        private readonly string _name = "枚举值";
        private volatile   List<Element> _elements=new List<Element>(); 

        public static EnumType Instance { get; } = new EnumType();
        private IDataType _valueType;
        private bool _isMulti;
        public object Convert(object value)
        {
            if (value != null)
            {
                foreach (Element ele in _elements)
                {
                    if (Match(value, ele))
                    {
                        //类型完全相同,则使用原始值作为对象.
                        var actValue = value.ToString().Equals(ele.Value) ? value : ele.Value;
                        if (_valueType is IConverter<object>)
                        {
                            actValue = ((IConverter<object>)_valueType).Convert(actValue);
                        }
                        return actValue;
                    }
                }
            }
            return null;
        }

        public EnumType IsMulti(bool isMulti)
        {
            _isMulti = isMulti;
            return this;
        }

        public EnumType AddElement(Element element)
        { 
            _elements.Add(element);
            return this;
        }

        public object Format(string format, object value)
        {
            if (value == null) return null;
            if (_elements == null)
            {
                return value.ToString();
            }
            if (_isMulti)
            {
                var formatObj = ToArray(value).Select(p => Format0(p)).ToList();
                         
                if (value is string) {
                    return string.Join(",", formatObj);
                }
                return formatObj;
            }
            return Format0(value);
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
            if (_elements == null)
            {
                return false;
            }
            object _value;
            if (_isMulti)
            {
                _value = ConvertMulti(value);
            }
            else
            {
                _value = Convert(value);
            }
            if (_value == null)
            {
                return false;
            }
            return true;
        }

        public Object ConvertMulti(object value)
        {
            var result = new List<object>();
            var objects = ToArray(value);
            if (objects.Length>0)
            {
                foreach (object obj in objects)
                {
                    var _v = Convert(obj);
                    if (_v != null)
                    {
                        result.Add(_v);
                    }
                }
            } 
            return result;
        }

        private string? Format0(object value)
        {
            var result = _elements
                .Where(p => value.ToString().Equals(p.Value))
                .FirstOrDefault();
            if(result != null)
                return result.Text==null? result.Value:result.Text;
            return null;
        }

        private object[] ToArray(Object value)
        {
            var values = new object[] { };
            if (value is Collection<object>)
            {
                ((Collection<object>)value).CopyTo(values, 0);

            }
            if (value is string)
            {
                var _string = value.ToString();
                values = _string.Split(",");
            }
            return values;
        }

        private bool Match(object value, Element ele)
        {
            var strVal = value.ToString();
            return string.Equals(ele.Value, strVal,StringComparison.InvariantCultureIgnoreCase) || string.Equals(ele.Text, strVal, StringComparison.InvariantCultureIgnoreCase);
        }

        public   class Element
        {
            public string Value { get; }

            public string Text { get;}

            public  string? Description {  get; }

            public Element(string value, string text ):this(value,text,null)
            { 
            }
            public Element(string value, string text,string? description)
            {
                Value = value;
                Text = text;
                Description = description;
            }

            public Element(Dictionary<String, String> dic):
                this(dic.GetValueOrDefault("value",""),
                    dic.GetValueOrDefault("text",""),
                    dic.GetValueOrDefault("description"))
            {
                
            }

            public Dictionary<String, Object> ToDictionary()
            {
                var map = new Dictionary<String, Object>();
                map.Add("value", Value);
                map.Add("text", Text);
                map.Add("description", Description); 
                return map;
            }
        }
    }
}
