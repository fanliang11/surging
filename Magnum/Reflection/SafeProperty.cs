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


	/// <summary>
	/// Use to safely set a property on an object, including the expansion of any lists as necessary
	/// and creation of reference properties. May not be suitable to all situations, but works great
	/// for deserializing data into an empty object graph.
	/// </summary>
	public class SafeProperty
	{
		readonly Action<object, int, object> _setter;

		SafeProperty(Type type, Action<object, int, object> setter)
		{
			Type = type;
			_setter = setter;
		}

		public Type Type { get; private set; }

		public void Set(object obj, int occurrence, object value)
		{
			_setter(obj, occurrence, value);
		}

		public static SafeProperty Create(Expression expression)
		{
			var visitor = new SafePropertyVisitor(expression);

			return new SafeProperty(visitor.Type, visitor.Setter);
		}
	}
}