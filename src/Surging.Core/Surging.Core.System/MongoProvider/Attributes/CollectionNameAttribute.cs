using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.MongoProvider.Attributes
{
    /// <summary>
    /// Defines the <see cref="CollectionNameAttribute" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class CollectionNameAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionNameAttribute"/> class.
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        public CollectionNameAttribute(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("参数不能为空", "value");
            }

            Name = value;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Name
        /// </summary>
        public string Name { get; private set; }

        #endregion 属性
    }
}