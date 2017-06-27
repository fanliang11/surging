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
namespace Magnum.Serialization
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Collections;
	using Reflection;

	public class PropertySerializerCache<T> :
		Cache<string, PropertySerializer<T>>
	{
		private int _count;
		private PropertySerializer<T>[] _each;

		public PropertySerializerCache(PropertyTypeSerializerCache typeSerializerCache)
			: base(GetPropertySerializers(typeSerializerCache))
		{
			_each = this.OrderBy(x => x.Name).ToArray();
			_count = _each.Length;
		}

		public new void Each(Action<PropertySerializer<T>> callback)
		{
			for (int i = 0; i < _count; i++)
			{
				callback(_each[i]);
			}
		}

		private static Dictionary<string, PropertySerializer<T>> GetPropertySerializers(
			PropertyTypeSerializerCache typeSerializerCache)
		{
			return typeof (T).GetAllProperties()
				.Where(x => x.GetGetMethod() != null)
				.Where(x => x.GetSetMethod(true) != null || typeof (T).IsInterface)
				.Select(x => new {x.Name, Serializer = CreatePropertySerializer(x, typeSerializerCache)})
				.ToDictionary(x => x.Name, x => x.Serializer);
		}

		private static PropertySerializer<T> CreatePropertySerializer(PropertyInfo property,
		                                                              PropertyTypeSerializerCache typeSerializerCache)
		{
			var genericTypes = new[] {typeof (T), property.PropertyType};
			var args = new object[] {property, typeSerializerCache};

			return (PropertySerializer<T>) FastActivator.Create(typeof (PropertySerializer<,>), genericTypes, args);
		}
	}
}