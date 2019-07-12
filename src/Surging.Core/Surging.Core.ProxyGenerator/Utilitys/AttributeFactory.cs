using Surging.Core.ProxyGenerator.FastReflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Core.ProxyGenerator.Utilitys
{
    /// <summary>
    /// Defines the <see cref="AttributeFactory" />
    /// </summary>
    internal class AttributeFactory
    {
        #region 字段

        /// <summary>
        /// Defines the m_attributeCreator
        /// </summary>
        private Func<object> m_attributeCreator;

        /// <summary>
        /// Defines the m_propertySetters
        /// </summary>
        private List<Action<object>> m_propertySetters;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFactory"/> class.
        /// </summary>
        /// <param name="data">The data<see cref="CustomAttributeData"/></param>
        public AttributeFactory(CustomAttributeData data)
        {
            this.Data = data;

            var ctorInvoker = new ConstructorInvoker(data.Constructor);
            var ctorArgs = data.ConstructorArguments.Select(a => a.Value).ToArray();
            this.m_attributeCreator = () => ctorInvoker.Invoke(ctorArgs);

            this.m_propertySetters = new List<Action<object>>();
            foreach (var arg in data.NamedArguments)
            {
                var property = (PropertyInfo)arg.MemberInfo;
                var propertyAccessor = new PropertyAccessor(property);
                var value = arg.TypedValue.Value;
                this.m_propertySetters.Add(o => propertyAccessor.SetValue(o, value));
            }
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Data
        /// </summary>
        public CustomAttributeData Data { get; private set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Create
        /// </summary>
        /// <returns>The <see cref="Attribute"/></returns>
        public Attribute Create()
        {
            var attribute = this.m_attributeCreator();

            foreach (var setter in this.m_propertySetters)
            {
                setter(attribute);
            }

            return (Attribute)attribute;
        }

        #endregion 方法
    }
}