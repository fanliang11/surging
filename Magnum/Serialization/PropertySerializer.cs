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
	using System.Reflection;
	using Reflection;

	public interface PropertySerializer<T>
	{
		string Name { get; }
		void Write(T obj, Action<string> output);
		void Read(T obj, string value);
	}

	public class PropertySerializer<T, TProperty> :
		PropertySerializer<T>
	{
		private readonly FastProperty<T, TProperty> _property;
		private readonly TypeSerializer<TProperty> _serializer;
		private readonly TypeWriter<TProperty> _typeWriter;
		private TypeReader<TProperty> _typeReader;

		public PropertySerializer(PropertyInfo property, PropertyTypeSerializerCache typeSerializerCache)
		{
			_property = new FastProperty<T, TProperty>(property, BindingFlags.NonPublic);

			_serializer = typeSerializerCache.GetTypeSerializer<TProperty>(property);

			_typeWriter = _serializer.GetWriter();
			_typeReader = _serializer.GetReader();
		}

		public string Name
		{
			get { return _property.Property.Name; }
		}

		public void Write(T obj, Action<string> output)
		{
			TProperty value = _property.Get(obj);

			_typeWriter(value, output);
		}

		public void Read(T obj, string value)
		{
			TProperty propertyValue = _typeReader(value);

			_property.Set(obj, propertyValue);
		}
	}
}