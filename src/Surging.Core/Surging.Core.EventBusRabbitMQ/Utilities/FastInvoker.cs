using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ.Utilities
{
    /// <summary>
    /// Defines the <see cref="FastInvoker{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FastInvoker<T>
    {
        #region 字段

        /// <summary>
        /// Defines the _current
        /// </summary>
        [ThreadStatic]
        internal static FastInvoker<T> _current;

        #endregion 字段

        #region 属性

        /// <summary>
        /// Gets the Current
        /// </summary>
        public static FastInvoker<T> Current
        {
            get
            {
                if (_current == null)
                    _current = new FastInvoker<T>();
                return _current;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The FastInvoke
        /// </summary>
        /// <param name="target">The target<see cref="T"/></param>
        /// <param name="expression">The expression<see cref="Expression{Action{T}}"/></param>
        public void FastInvoke(T target, Expression<Action<T>> expression)
        {
            var call = expression.Body as MethodCallExpression;
            if (call == null)
                throw new ArgumentException("只支持方法调用表达式。 ", "expression");
            Action<T> invoker = GetInvoker(() => call.Method);
            invoker(target);
        }

        /// <summary>
        /// The FastInvoke
        /// </summary>
        /// <param name="target">The target<see cref="T"/></param>
        /// <param name="genericTypes">The genericTypes<see cref="Type[]"/></param>
        /// <param name="expression">The expression<see cref="Expression{Action{T}}"/></param>
        public void FastInvoke(T target, Type[] genericTypes, Expression<Action<T>> expression)
        {
            var call = expression.Body as MethodCallExpression;
            if (call == null)
                throw new ArgumentException("只支持方法调用表达式", "expression");

            MethodInfo method = call.Method;
            Action<T> invoker = GetInvoker(() =>
           {
               if (method.IsGenericMethod)
                   return GetGenericMethodFromTypes(method.GetGenericMethodDefinition(), genericTypes);
               return method;
           });
            invoker(target);
        }

        /// <summary>
        /// The GetGenericMethodFromTypes
        /// </summary>
        /// <param name="method">The method<see cref="MethodInfo"/></param>
        /// <param name="genericTypes">The genericTypes<see cref="Type[]"/></param>
        /// <returns>The <see cref="MethodInfo"/></returns>
        internal MethodInfo GetGenericMethodFromTypes(MethodInfo method, Type[] genericTypes)
        {
            if (!method.IsGenericMethod)
                throw new ArgumentException("不能为非泛型方法指定泛型类型。: " + method.Name);
            Type[] genericArguments = method.GetGenericArguments();
            if (genericArguments.Length != genericTypes.Length)
            {
                throw new ArgumentException("传递的泛型参数的数目错误" + genericTypes.Length
                                            + " (needed " + genericArguments.Length + ")");
            }
            method = method.GetGenericMethodDefinition().MakeGenericMethod(genericTypes);
            return method;
        }

        /// <summary>
        /// The GetInvoker
        /// </summary>
        /// <param name="getMethodInfo">The getMethodInfo<see cref="Func{MethodInfo}"/></param>
        /// <returns>The <see cref="Action{T}"/></returns>
        internal Action<T> GetInvoker(Func<MethodInfo> getMethodInfo)
        {
            MethodInfo method = getMethodInfo();

            ParameterExpression instanceParameter = Expression.Parameter(typeof(T), "target");

            MethodCallExpression call = Expression.Call(instanceParameter, method);

            return Expression.Lambda<Action<T>>(call, new[] { instanceParameter }).Compile();
        }

        #endregion 方法
    }
}