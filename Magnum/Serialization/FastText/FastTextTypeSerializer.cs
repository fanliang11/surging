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
namespace Magnum.Serialization.FastText
{
	using System;


	public interface FastTextTypeSerializer
	{
		void Serialize<T>(T obj, Action<string> output);
		T Deserialize<T>(string text);
	}


	public class FastTextTypeSerializer<T> :
		FastTextTypeSerializer,
		TypeSerializer<T>
	{
		readonly TypeReader<T> _deserializer;
		readonly TypeWriter<object> _serializer;
		readonly TypeSerializer<T> _typeSerializer;

		public FastTextTypeSerializer(TypeSerializer<T> typeSerializer)
		{
			_typeSerializer = typeSerializer;

			TypeWriter<T> serialize = typeSerializer.GetWriter();
			_serializer = (value, output) => { serialize((T)value, output); };

			_deserializer = typeSerializer.GetReader();
		}

		public void Serialize<TObject>(TObject obj, Action<string> output)
		{
			_serializer(obj, output);
		}

		public TResult Deserialize<TResult>(string text)
		{
			return (TResult)(object)_deserializer(text);
		}

		public TypeReader<T> GetReader()
		{
			return _typeSerializer.GetReader();
		}

		public TypeWriter<T> GetWriter()
		{
			return _typeSerializer.GetWriter();
		}
	}
}