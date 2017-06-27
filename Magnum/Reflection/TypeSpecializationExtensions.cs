// Copyright 2007-2008 The Apache Software Foundation.
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
	using System.Linq;
	using System.Reflection;
	using Extensions;
	using Linq;

	public static class TypeSpecializationExtensions
	{
		public static Type ToSpecializedType<T>(this T method, object[] args)
			where T : MethodBase
		{
			Guard.AgainstNull(method, "method");

			Type type = method.DeclaringType;
			if (!type.IsGenericType)
				throw new ArgumentException("The argument must be for a generic type", "method");

			Guard.AgainstNull(args, "args");

			Type[] genericArguments = GetGenericTypesFromArguments(method.GetParameters(), type.GetGenericArguments(), args);

			return type.MakeGenericType(genericArguments);
		}

		public static MethodInfo ToSpecializedMethod(this MethodInfo method, object[] args)
		{
			Guard.AgainstNull(method, "method");

			if (!method.IsGenericMethod)
				return method;

			Guard.AgainstNull(args, "args");

			Type[] genericArguments = GetGenericTypesFromArguments(method.GetParameters(), method.GetGenericArguments(), args);

			return method.MakeGenericMethod(genericArguments);
		}

		public static MethodInfo ToSpecializedMethod(this MethodInfo method, Type[] genericTypes, object[] args)
		{
			Guard.AgainstNull(method, "method");

			if (!method.IsGenericMethod)
				return method;

			Guard.AgainstNull(genericTypes, "genericTypes");
			Guard.AgainstNull(args, "args");

			Type[] arguments = method.GetGenericArguments()
				.ApplyGenericTypesToArguments(genericTypes);

			arguments = GetGenericTypesFromArguments(method.GetParameters(), arguments, args);

			method = method.MakeGenericMethod(arguments);

			return method;
		}

		private static Type[] ApplyGenericTypesToArguments(this Type[] arguments, Type[] genericTypes)
		{
			for (int i = 0; i < arguments.Length && i < genericTypes.Length; i++)
			{
				if (genericTypes[i] != null)
					arguments[i] = genericTypes[i];
			}

			return arguments;
		}

		private static Type[] GetGenericTypesFromArguments(ParameterInfo[] parameterInfos, Type[] arguments, object[] args)
		{
			var parameters = parameterInfos
				.Merge(args, (x, y) => new {Parameter = x, Argument = y});

			for (int i = 0; i < arguments.Length; i++)
			{
				Type argumentType = arguments[i];

				if (!argumentType.IsGenericParameter)
					continue;

				parameters
					.Where(arg => arg.Parameter.ParameterType == argumentType && arg.Argument != null)
					.Select(arg => arg.Argument.GetType())
					.Each(type =>
						{
							arguments[i] = type;

							var more = argumentType.GetGenericParameterConstraints()
								.Where(x => x.IsGenericType)
								.Where(x => type.Implements(x.GetGenericTypeDefinition()))
								.SelectMany(x => x.GetGenericArguments()
													.Merge(type.GetGenericTypeDeclarations(x.GetGenericTypeDefinition()), (c, a) => new { Argument = c, Type = a }));

							foreach (var next in more)
							{
								for (int j = 0; j < arguments.Length; j++)
								{
									if (arguments[j] == next.Argument)
										arguments[j] = next.Type;
								}
							}
						});

				foreach (var parameter in parameters.Where(x => x.Parameter.ParameterType.IsGenericType && x.Argument != null))
				{
					var definition = parameter.Parameter.ParameterType.GetGenericTypeDefinition();
					var declaredTypesForGeneric = parameter.Argument.GetType().GetGenericTypeDeclarations(definition);

					var mergeds = parameter.Parameter.ParameterType.GetGenericArguments()
						.Merge(declaredTypesForGeneric, (p, a) => new { ParameterType = p, ArgumentType = a });

					foreach (var merged in mergeds)
					{
						for (int j = 0; j < arguments.Length; j++)
						{
							if (arguments[j] == merged.ParameterType)
								arguments[j] = merged.ArgumentType;
						}
					}
				}
			}

			return arguments;
		}
	}
}