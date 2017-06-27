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
namespace Magnum.Binding
{
	using System;
	using System.Linq.Expressions;
	using System.Reflection;

	public interface ObjectPropertyBinder
	{
		PropertyInfo Property { get; }
	}

	public class ObjectPropertyBinder<T> :
		ObjectPropertyBinder
	{
		private readonly PropertyInfo _info;
		private readonly Action<T, object> _set;

		public ObjectPropertyBinder(PropertyInfo info)
		{
			_info = info;
			_set = InitializeSet(info);
		}

		public PropertyInfo Property
		{
			get { return _info; }
		}

		public void SetValue(T instance, object value)
		{
			_set(instance, value);
		}

		private static Action<T, object> InitializeSet(PropertyInfo property)
		{
			ParameterExpression instance = Expression.Parameter(typeof (T), "instance");
			ParameterExpression value = Expression.Parameter(typeof (object), "value");

			UnaryExpression valueCast;
			if (property.PropertyType.IsValueType)
				valueCast = Expression.Convert(value, property.PropertyType);
			else
				valueCast = Expression.TypeAs(value, property.PropertyType);

			MethodCallExpression call = Expression.Call(instance, property.GetSetMethod(true), valueCast);

			return Expression.Lambda<Action<T, object>>(call, new[] {instance, value}).Compile();
		}
	}
}