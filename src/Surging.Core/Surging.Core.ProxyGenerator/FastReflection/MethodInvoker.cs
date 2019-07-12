using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Surging.Core.ProxyGenerator.FastReflection
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IMethodInvoker" />
    /// </summary>
    public interface IMethodInvoker
    {
        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <param name="parameters">The parameters<see cref="object[]"/></param>
        /// <returns>The <see cref="object"/></returns>
        object Invoke(object instance, params object[] parameters);

        #endregion 方法
    }

    #endregion 接口

    /// <summary>
    /// Defines the <see cref="MethodInvoker" />
    /// </summary>
    public class MethodInvoker : IMethodInvoker
    {
        #region 字段

        /// <summary>
        /// Defines the m_invoker
        /// </summary>
        private Func<object, object[], object> m_invoker;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodInvoker"/> class.
        /// </summary>
        /// <param name="methodInfo">The methodInfo<see cref="MethodInfo"/></param>
        public MethodInvoker(MethodInfo methodInfo)
        {
            this.MethodInfo = methodInfo;
            this.m_invoker = CreateInvokeDelegate(methodInfo);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the MethodInfo
        /// </summary>
        public MethodInfo MethodInfo { get; private set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <param name="parameters">The parameters<see cref="object[]"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object Invoke(object instance, params object[] parameters)
        {
            return this.m_invoker(instance, parameters);
        }

        /// <summary>
        /// The CreateInvokeDelegate
        /// </summary>
        /// <param name="methodInfo">The methodInfo<see cref="MethodInfo"/></param>
        /// <returns>The <see cref="Func{object, object[], object}"/></returns>
        private static Func<object, object[], object> CreateInvokeDelegate(MethodInfo methodInfo)
        {
            // Target: ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)

            // parameters to execute
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            // build parameter list
            var parameterExpressions = new List<Expression>();
            var paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                // (Ti)parameters[i]
                BinaryExpression valueObj = Expression.ArrayIndex(
                    parametersParameter, Expression.Constant(i));
                UnaryExpression valueCast = Expression.Convert(
                    valueObj, paramInfos[i].ParameterType);

                parameterExpressions.Add(valueCast);
            }

            // non-instance for static method, or ((TInstance)instance)
            var instanceCast = methodInfo.IsStatic ? null :
                Expression.Convert(instanceParameter, methodInfo.ReflectedType);

            // static invoke or ((TInstance)instance).Method
            var methodCall = Expression.Call(instanceCast, methodInfo, parameterExpressions);

            // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
            if (methodCall.Type == typeof(void))
            {
                var lambda = Expression.Lambda<Action<object, object[]>>(
                        methodCall, instanceParameter, parametersParameter);

                Action<object, object[]> execute = lambda.Compile();
                return (instance, parameters) =>
                {
                    execute(instance, parameters);
                    return null;
                };
            }
            else
            {
                var castMethodCall = Expression.Convert(methodCall, typeof(object));
                var lambda = Expression.Lambda<Func<object, object[], object>>(
                    castMethodCall, instanceParameter, parametersParameter);

                return lambda.Compile();
            }
        }

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="instance">The instance<see cref="object"/></param>
        /// <param name="parameters">The parameters<see cref="object[]"/></param>
        /// <returns>The <see cref="object"/></returns>
        object IMethodInvoker.Invoke(object instance, params object[] parameters)
        {
            return this.Invoke(instance, parameters);
        }

        #endregion 方法
    }
}