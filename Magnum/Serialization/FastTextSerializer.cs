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
	using System.IO;
	using System.Text;
	using FastText;


	public class FastTextSerializer :
		Serializer
	{
		static readonly TypeSerializerCache _typeSerializerCache;

		[ThreadStatic]
		static FastTextTypeSerializerCache _typeSerializers;

		static FastTextSerializer()
		{
			_typeSerializerCache = new TypeSerializerCacheImpl();
		}

		public void Serialize<T>(T obj, TextWriter writer)
		{
			FastTextTypeSerializer serializer = GetTypeSerializer(typeof(T));

			serializer.Serialize(obj, writer.Write);
		}

		public string Serialize<T>(T obj)
		{
			var sb = new StringBuilder(4096);
			using (var writer = new StringWriter(sb))
				Serialize(obj, writer);

			return sb.ToString();
		}

		public T Deserialize<T>(string text)
		{
			FastTextTypeSerializer serializer = GetTypeSerializer(typeof(T));

			return serializer.Deserialize<T>(text);
		}

		public T Deserialize<T>(TextReader reader)
		{
			FastTextTypeSerializer serializer = GetTypeSerializer(typeof(T));

			return serializer.Deserialize<T>(reader.ReadToEnd());
		}

		static FastTextTypeSerializer GetTypeSerializer(Type type)
		{
			if (_typeSerializers == null)
				_typeSerializers = new FastTextTypeSerializerCache(GetTypeSerializerCache());

			return _typeSerializers[type];
		}

		static TypeSerializerCache GetTypeSerializerCache()
		{
			return _typeSerializerCache;
		}
	}
}