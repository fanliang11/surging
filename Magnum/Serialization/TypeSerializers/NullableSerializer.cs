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
namespace Magnum.Serialization.TypeSerializers
{
	using System;
	using Extensions;

	public class NullableSerializer<T> :
		TypeSerializer<Nullable<T>>
		where T : struct
	{
		private readonly TypeReader<T> _reader;
		private readonly TypeSerializer<T> _serializer;
		private readonly TypeWriter<T> _writer;

		public NullableSerializer(TypeSerializer<T> serializer)
		{
			_serializer = serializer;
			_reader = _serializer.GetReader();
			_writer = _serializer.GetWriter();
		}

		public TypeReader<T?> GetReader()
		{
			return value =>
				{
					if (value == null || value.Length == 0)
						return default(T?);

					return _reader(value);
				};
		}

		public TypeWriter<T?> GetWriter()
		{
			return (value, output) =>
				{
					if (value.HasValue)
					{
						_writer(value.Value, output);
					}
				};
		}
	}
}