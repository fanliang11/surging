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

	public class ObjectSerializer<T> :
		TypeSerializer<T>
		where T : class
	{
		private readonly PropertySerializerCache<T> _properties;

		public ObjectSerializer(PropertyTypeSerializerCache typeSerializerCache)
		{
			ObjectType = typeof (T);
			if (!ObjectType.IsClass && !ObjectType.IsInterface)
				throw new ArgumentException("Only classes and interfaces can be serialized by an object serializer, not: "
				                            + ObjectType.FullName);

			_properties = new PropertySerializerCache<T>(typeSerializerCache);
		}

		public Type ObjectType { get; private set; }

		public PropertySerializerCache<T> Properties
		{
			get { return _properties; }
		}

		public virtual TypeReader<T> GetReader()
		{
			// TODO
			return text => default(T);
		}

		public virtual TypeWriter<T> GetWriter()
		{
			return (value, output) =>
				{
					if (value == null)
						return;

					_properties.Each(serializer => { serializer.Write(value, output); });
				};
		}
	}
}