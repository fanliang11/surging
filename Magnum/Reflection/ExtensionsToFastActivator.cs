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
	using System.Linq.Expressions;


	public static class ExtensionsToFastActivator
	{
		public static void FastInvoke<T>(this T target, string methodName)
		{
			FastInvoker<T>.Current.FastInvoke(target, methodName);
		}

		public static void FastInvoke<T>(this T target, string methodName, params object[] args)
		{
			FastInvoker<T>.Current.FastInvoke(target, methodName, args);
		}

		public static void FastInvoke<T>(this T target, Type[] genericTypes, string methodName)
		{
			FastInvoker<T>.Current.FastInvoke(target, genericTypes, methodName);
		}

		public static void FastInvoke<T>(this T target, Type[] genericTypes, string methodName, params object[] args)
		{
			FastInvoker<T>.Current.FastInvoke(target, genericTypes, methodName, args);
		}

		public static void FastInvoke<T>(this T target, Expression<Action<T>> expression)
		{
			FastInvoker<T>.Current.FastInvoke(target, expression);
		}

		public static void FastInvoke<T>(this T target, Expression<Action<T>> expression, params object[] args)
		{
			FastInvoker<T>.Current.FastInvoke(target, expression, args);
		}

		public static void FastInvoke<T>(this T target, Type[] genericTypes, Expression<Action<T>> expression)
		{
			FastInvoker<T>.Current.FastInvoke(target, genericTypes, expression);
		}

		public static void FastInvoke<T>(this T target, Type[] genericTypes, Expression<Action<T>> expression,
		                                 params object[] args)
		{
			FastInvoker<T>.Current.FastInvoke(target, genericTypes, expression, args);
		}

		public static TResult FastInvoke<T, TResult>(this T target, string methodName)
		{
			return FastInvoker<T, TResult>.Current.FastInvoke(target, methodName);
		}

		public static TResult FastInvoke<T, TResult>(this T target, string methodName, params object[] args)
		{
			return FastInvoker<T, TResult>.Current.FastInvoke(target, methodName, args);
		}

		public static TResult FastInvoke<T, TResult>(this T target, Type[] genericTypes, string methodName)
		{
			return FastInvoker<T, TResult>.Current.FastInvoke(target, genericTypes, methodName);
		}

		public static TResult FastInvoke<T, TResult>(this T target, Type[] genericTypes, string methodName,
		                                             params object[] args)
		{
			return FastInvoker<T, TResult>.Current.FastInvoke(target, genericTypes, methodName, args);
		}

		public static TResult FastInvoke<T, TResult>(this T target, Expression<Func<T, TResult>> expression)
		{
			return FastInvoker<T, TResult>.Current.FastInvoke(target, expression);
		}

		public static TResult FastInvoke<T, TResult>(this T target, Expression<Func<T, TResult>> expression,
		                                             params object[] args)
		{
			return FastInvoker<T, TResult>.Current.FastInvoke(target, expression, args);
		}

		public static TResult FastInvoke<T, TResult>(this T target, Type[] genericTypes,
		                                             Expression<Func<T, TResult>> expression)
		{
			return FastInvoker<T, TResult>.Current.FastInvoke(target, genericTypes, expression);
		}

		public static TResult FastInvoke<T, TResult>(this T target, Type[] genericTypes,
		                                             Expression<Func<T, TResult>> expression, params object[] args)
		{
			return FastInvoker<T, TResult>.Current.FastInvoke(target, genericTypes, expression, args);
		}
	}
}