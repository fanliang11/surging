using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.MongoProvider.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class CollectionNameAttribute : Attribute
    {
        public CollectionNameAttribute(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("参数不能为空", "value");
            }

            Name = value;
        }
        public string Name { get; private set; }
    }
}
