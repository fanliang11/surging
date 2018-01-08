using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Surging.Core.EventBusKafka.Utilities
{
    public class FastInvoker<T>
    {
        [ThreadStatic]
        static FastInvoker<T> _current;
        public static FastInvoker<T> Current
        {
            get
            {
                if (_current == null)
                    _current = new FastInvoker<T>();
                return _current;
            }
        }

        public void FastInvoke(T target, Expression<Action<T>> expression)
        {
            var call = expression.Body as MethodCallExpression;
            if (call == null)
                throw new ArgumentException("只支持方法调用表达式。 ", "expression");
            Action<T> invoker = GetInvoker(() => call.Method);
            invoker(target);
        }

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

        MethodInfo GetGenericMethodFromTypes(MethodInfo method, Type[] genericTypes)
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

        Action<T> GetInvoker(Func<MethodInfo> getMethodInfo)
        {
            MethodInfo method = getMethodInfo();

            ParameterExpression instanceParameter = Expression.Parameter(typeof(T), "target");

            MethodCallExpression call = Expression.Call(instanceParameter, method);

            return Expression.Lambda<Action<T>>(call, new[] { instanceParameter }).Compile();

        }
    }
}
 
