// Copyright 2007-2010 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Magnum.Reflection
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using Extensions;


	public class FastInvoker<T> :
		FastInvokerBase,
		IFastInvoker<T>
	{
		[ThreadStatic]
		static FastInvoker<T> _current;

		readonly Dictionary<int, Action<T>> _noArgs = new Dictionary<int, Action<T>>();
		readonly Dictionary<int, Action<T, object[]>> _withArgs = new Dictionary<int, Action<T, object[]>>();

		FastInvoker()
			: base(typeof(T))
		{
		}

		public static FastInvoker<T> Current
		{
			get
			{
				if (_current == null)
					_current = new FastInvoker<T>();

				return _current;
			}
		}

		public void FastInvoke(T target, string methodName)
		{
			Action<T> invoker = GetInvoker(methodName);

			invoker(target);
		}

		public Action<T> GetInvoker(string methodName)
		{
			int key = 97*methodName.GetHashCode();

			return GetInvoker(key, () =>
				{
					return MethodNameCache[methodName]
						.MatchingArguments()
						.First();
				});
		}

		public void FastInvoke(T target, string methodName, params object[] args)
		{
			Action<T, object[]> invoker = GetInvoker(methodName, args);

			invoker(target, args);
		}

		public Action<T, object[]> GetInvoker(string methodName, object[] args)
		{
			if (args == null || args.Length == 0)
			{
				var invoker = GetInvoker(methodName);
				return (x, y) => invoker(x);
			}

			int key = GetArgumentHashCode(97*methodName.GetHashCode(), args);

			return GetInvoker(key, () =>
				{
					return MethodNameCache[methodName]
						.MatchingArguments(args)
						.First()
						.ToSpecializedMethod(args);
				}, args);
		}

		public void FastInvoke(T target, Type[] genericTypes, string methodName)
		{
			int key = GetArgumentHashCode(97*methodName.GetHashCode(), genericTypes);

			Action<T> invoker = GetInvoker(key, () =>
				{
					var empty = new object[] {};

					return MethodNameCache[methodName]
						.MatchingArguments()
						.First()
						.ToSpecializedMethod(genericTypes, empty);
				});

			invoker(target);
		}

		public void FastInvoke(T target, Type[] genericTypes, string methodName, params object[] args)
		{
			if (args == null || args.Length == 0)
			{
				FastInvoke(target, genericTypes, methodName);
				return;
			}

			int key = GetArgumentHashCode(97*methodName.GetHashCode(), genericTypes, args);

			Action<T, object[]> invoker = GetInvoker(key, () =>
				{
					return MethodNameCache[methodName]
						.MatchingArguments(args)
						.First()
						.ToSpecializedMethod(genericTypes, args);
				}, args);

			invoker(target, args);
		}

		public void FastInvoke(T target, Expression<Action<T>> expression)
		{
			var call = expression.Body as MethodCallExpression;
			if (call == null)
				throw new ArgumentException("Only method call expressions are supported.", "expression");

			int key = 61*call.Method.GetHashCode();

			Action<T> invoker = GetInvoker(key, () => call.Method);

			invoker(target);
		}

		public void FastInvoke(T target, Expression<Action<T>> expression, params object[] args)
		{
			var call = expression.Body as MethodCallExpression;
			if (call == null)
				throw new ArgumentException("Only method call expressions are supported.", "expression");

			MethodInfo method = call.Method;

			int key = GetArgumentHashCode(61*method.GetHashCode(), args);

			Action<T, object[]> invoker = GetInvoker(key,
			                                         () =>
			                                         	{
			                                         		return method.IsGenericMethod
			                                         		       	? method.GetGenericMethodDefinition().ToSpecializedMethod(args)
			                                         		       	: method;
			                                         	}, args);

			invoker(target, args);
		}

		public void FastInvoke(T target, Type[] genericTypes, Expression<Action<T>> expression)
		{
			var call = expression.Body as MethodCallExpression;
			if (call == null)
				throw new ArgumentException("Only method call expressions are supported.", "expression");

			MethodInfo method = call.Method;

			int key = GetArgumentHashCode(61*method.GetHashCode(), genericTypes);

			Action<T> invoker = GetInvoker(key, () =>
				{
					if (method.IsGenericMethod)
						return GetGenericMethodFromTypes(method.GetGenericMethodDefinition(), genericTypes);

					return method;
				});

			invoker(target);
		}

		public void FastInvoke(T target, Type[] genericTypes, Expression<Action<T>> expression, params object[] args)
		{
			var call = expression.Body as MethodCallExpression;
			if (call == null)
				throw new ArgumentException("Only method call expressions are supported.", "expression");

			MethodInfo method = call.Method;

			int key = GetArgumentHashCode(61*method.GetHashCode(), genericTypes, args);

			Action<T, object[]> invoker = GetInvoker(key, () =>
				{
					if (method.IsGenericMethod)
						return method.GetGenericMethodDefinition().ToSpecializedMethod(genericTypes, args);

					return method.ToSpecializedMethod(genericTypes, args);
				}, args);

			invoker(target, args);
		}

		Action<T> GetInvoker(int key, Func<MethodInfo> getMethodInfo)
		{
			return _noArgs.Retrieve(key, () =>
				{
					MethodInfo method = getMethodInfo();

					ParameterExpression instanceParameter = Expression.Parameter(typeof(T), "target");

					MethodCallExpression call = Expression.Call(instanceParameter, method);

					return Expression.Lambda<Action<T>>(call, new[] {instanceParameter}).Compile();
				});
		}

		Action<T, object[]> GetInvoker(int key, Func<MethodInfo> getMethodInfo, object[] args)
		{
			return _withArgs.Retrieve(key, () =>
				{
					MethodInfo method = getMethodInfo();

					ParameterExpression instanceParameter = Expression.Parameter(typeof(T), "target");
					ParameterExpression argsParameter = Expression.Parameter(typeof(object[]), "args");

					Expression[] parameters = method.GetParameters().ToArrayIndexParameters(argsParameter).ToArray();

					MethodCallExpression call = Expression.Call(instanceParameter, method, parameters);

					return Expression.Lambda<Action<T, object[]>>(call, new[] {instanceParameter, argsParameter}).Compile();
				});
		}
	}
}