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
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;

	public static class ExtensionsToExpressions
	{
		public static ParameterExpression ToParameterExpression(this ParameterInfo parameterInfo)
		{
			return Expression.Parameter(parameterInfo.ParameterType, parameterInfo.Name ?? "x");
		}

		public static ParameterExpression ToParameterExpression(this ParameterInfo parameterInfo, string name)
		{
			return Expression.Parameter(parameterInfo.ParameterType, parameterInfo.Name ?? name);
		}

		public static IEnumerable<ParameterExpression> ToParameterExpressions(this IEnumerable<ParameterInfo> parameters)
		{
			return parameters.Select((parameter, index) => ToParameterExpression(parameter, "arg" + index));
		}

		public static IEnumerable<Expression> ToArrayIndexParameters(this IEnumerable<ParameterInfo> parameters, ParameterExpression arguments)
		{
			Func<ParameterInfo, int, Expression> converter = (parameter, index) =>
				{
					BinaryExpression arrayExpression = Expression.ArrayIndex(arguments, Expression.Constant(index));

					if (parameter.ParameterType.IsValueType)
						return Expression.Convert(arrayExpression, parameter.ParameterType);

					return Expression.TypeAs(arrayExpression, parameter.ParameterType);
				};

			return parameters.Select(converter);
		}
	}
}