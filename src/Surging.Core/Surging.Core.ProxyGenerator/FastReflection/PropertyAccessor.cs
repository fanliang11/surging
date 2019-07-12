using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Surging.Core.ProxyGenerator.FastReflection
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IPropertyAccessor" />
    /// </summary>
    public interface IPropertyAccessor
    {
        #region 方法

        /// <summary>
        /// The GetValue
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <returns>The <see cref="object"/></returns>
        object GetValue(object instance);

        /// <summary>
        /// The SetValue
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        void SetValue(object instance, object value);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// Defines the <see cref="PropertyAccessor" />
    /// </summary>
    public class PropertyAccessor : IPropertyAccessor
    {
        #region 字段

        /// <summary>
        /// Defines the m_getter
        /// </summary>
        private Func<object, object> m_getter;

        /// <summary>
        /// Defines the m_setMethodInvoker
        /// </summary>
        private MethodInvoker m_setMethodInvoker;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAccessor"/> class.
        /// </summary>
        /// <param name="propertyInfo">The propertyInfo<see cref="PropertyInfo"/></param>
        public PropertyAccessor(PropertyInfo propertyInfo)
        {
            this.PropertyInfo = propertyInfo;
            this.InitializeGet(propertyInfo);
            this.InitializeSet(propertyInfo);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the PropertyInfo
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The GetValue
        /// </summary>
        /// <param name="o">The o<see cref="object"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object GetValue(object o)
        {
            if (this.m_getter == null)
            {
                throw new NotSupportedException("Get method is not defined for this property.");
            }

            return this.m_getter(o);
        }

        /// <summary>
        /// The SetValue
        /// </summary>
        /// <param name="o">The o<see cref="object"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        public void SetValue(object o, object value)
        {
            if (this.m_setMethodInvoker == null)
            {
                throw new NotSupportedException("Set method is not defined for this property.");
            }

            this.m_setMethodInvoker.Invoke(o, new object[] { value });
        }

        /// <summary>
        /// The InitializeGet
        /// </summary>
        /// <param name="propertyInfo">The propertyInfo<see cref="PropertyInfo"/></param>
        private void InitializeGet(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead) return;

            // Target: (object)(((TInstance)instance).Property)

            // preparing parameter, object type
            var instance = Expression.Parameter(typeof(object), "instance");

            // non-instance for static method, or ((TInstance)instance)
            var instanceCast = propertyInfo.GetGetMethod(true).IsStatic ? null :
                Expression.Convert(instance, propertyInfo.ReflectedType);

            // ((TInstance)instance).Property
            var propertyAccess = Expression.Property(instanceCast, propertyInfo);

            // (object)(((TInstance)instance).Property)
            var castPropertyValue = Expression.Convert(propertyAccess, typeof(object));

            // Lambda expression
            var lambda = Expression.Lambda<Func<object, object>>(castPropertyValue, instance);

            this.m_getter = lambda.Compile();
        }

        /// <summary>
        /// The InitializeSet
        /// </summary>
        /// <param name="propertyInfo">The propertyInfo<see cref="PropertyInfo"/></param>
        private void InitializeSet(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite) return;
            this.m_setMethodInvoker = new MethodInvoker(propertyInfo.GetSetMethod(true));
        }

        /// <summary>
        /// The GetValue
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <returns>The <see cref="object"/></returns>
        object IPropertyAccessor.GetValue(object instance)
        {
            return this.GetValue(instance);
        }

        /// <summary>
        /// The SetValue
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        void IPropertyAccessor.SetValue(object instance, object value)
        {
            this.SetValue(instance, value);
        }

        #endregion 方法
    }
}