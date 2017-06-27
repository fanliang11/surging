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
namespace Magnum.Serialization
{
	using System;
	using Collections;


	/// <summary>
	///   Wraps the cache for TypeSerializer implementations
	/// </summary>
	public class TypeSerializerCacheImpl :
		TypeSerializerCache
	{
		readonly Cache<Type, TypeSerializer> _defaultSerializers;

		public TypeSerializerCacheImpl()
		{
			_defaultSerializers = new Cache<Type, TypeSerializer>(new TypeSerializerLoader().LoadBuiltInTypeSerializers());
		}

		public TypeSerializer<T> GetTypeSerializer<T>()
		{
			return _defaultSerializers[typeof(T)] as TypeSerializer<T>;
		}

		public void Each(Action<Type, TypeSerializer> action)
		{
			_defaultSerializers.Each(action);
		}
	}
}