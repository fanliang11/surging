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
	using System.Reflection;
	using Collections;
	using Extensions;


	public abstract class FastInvokerBase
	{
		const BindingFlags _methodBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		protected readonly MultiDictionary<string, MethodInfo> MethodNameCache;

		protected FastInvokerBase(Type type)
		{
			ObjectType = type;

			MethodNameCache = new MultiDictionary<string, MethodInfo>(false);

			type.GetMethods(_methodBindingFlags).Each(method => MethodNameCache.Add(method.Name, method));
		}

		public Type ObjectType { get; private set; }

		protected static int GetArgumentHashCode(int seed, object[] args)
		{
			int key = seed;
			for (int i = 0; i < args.Length; i++)
				key ^= args[i] == null ? 31*i : args[i].GetType().GetHashCode() << i;
			return key;
		}

		protected static int GetArgumentHashCode(int seed, Type[] genericTypes)
		{
			int key = seed;
			for (int i = 0; i < genericTypes.Length; i++)
				key ^= genericTypes[i] == null ? 27*i : genericTypes[i].GetHashCode()*101 << i;
			return key;
		}

		protected static int GetArgumentHashCode(int seed, Type[] genericTypes, object[] args)
		{
			int key = seed;
			for (int i = 0; i < genericTypes.Length; i++)
				key ^= genericTypes[i] == null ? 27*i : genericTypes[i].GetHashCode()*101 << i;
			for (int i = 0; i < args.Length; i++)
				key ^= args[i] == null ? 31*i : args[i].GetType().GetHashCode() << i;
			return key;
		}

		protected static MethodInfo GetGenericMethodFromTypes(MethodInfo method, Type[] genericTypes)
		{
			if (!method.IsGenericMethod)
				throw new ArgumentException("Generic types cannot be specified for a non-generic method: " + method.Name);

			Type[] genericArguments = method.GetGenericArguments();

			if (genericArguments.Length != genericTypes.Length)
			{
				throw new ArgumentException("An incorrect number of generic arguments was specified: " + genericTypes.Length
				                            + " (needed " + genericArguments.Length + ")");
			}

			method = method.GetGenericMethodDefinition().MakeGenericMethod(genericTypes);
			return method;
		}
	}
}