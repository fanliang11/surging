using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Surging.Core.ProxyGenerator.FastReflection
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IConstructorInvoker" />
    /// </summary>
    public interface IConstructorInvoker
    {
        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="parameters">The parameters<see cref="object[]"/></param>
        /// <returns>The <see cref="object"/></returns>
        object Invoke(params object[] parameters);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// Defines the <see cref="ConstructorInvoker" />
    /// </summary>
    public class ConstructorInvoker : IConstructorInvoker
    {
        #region 字段

        /// <summary>
        /// Defines the m_invoker
        /// </summary>
        private Func<object[], object> m_invoker;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorInvoker"/> class.
        /// </summary>
        /// <param name="constructorInfo">The constructorInfo<see cref="ConstructorInfo"/></param>
        public ConstructorInvoker(ConstructorInfo constructorInfo)
        {
            this.ConstructorInfo = constructorInfo;
            this.m_invoker = InitializeInvoker(constructorInfo);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ConstructorInfo
        /// </summary>
        public ConstructorInfo ConstructorInfo { get; private set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="parameters">The parameters<see cref="object[]"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object Invoke(params object[] parameters)
        {
            return this.m_invoker(parameters);
        }

        /// <summary>
        /// The InitializeInvoker
        /// </summary>
        /// <param name="constructorInfo">The constructorInfo<see cref="ConstructorInfo"/></param>
        /// <returns>The <see cref="Func{object[], object}"/></returns>
        private Func<object[], object> InitializeInvoker(ConstructorInfo constructorInfo)
        {
            // Target: (object)new T((T0)parameters[0], (T1)parameters[1], ...)

            // parameters to execute
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            // build parameter list
            var parameterExpressions = new List<Expression>();
            var paramInfos = constructorInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                // (Ti)parameters[i]
                var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                var valueCast = Expression.Convert(valueObj, paramInfos[i].ParameterType);

                parameterExpressions.Add(valueCast);
            }

            // new T((T0)parameters[0], (T1)parameters[1], ...)
            var instanceCreate = Expression.New(constructorInfo, parameterExpressions);

            // (object)new T((T0)parameters[0], (T1)parameters[1], ...)
            var instanceCreateCast = Expression.Convert(instanceCreate, typeof(object));

            var lambda = Expression.Lambda<Func<object[], object>>(instanceCreateCast, parametersParameter);

            return lambda.Compile();
        }

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="parameters">The parameters<see cref="object[]"/></param>
        /// <returns>The <see cref="object"/></returns>
        object IConstructorInvoker.Invoke(params object[] parameters)
        {
            return this.Invoke(parameters);
        }

        #endregion 方法
    }
}