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
	using System.Reflection;

	public class FastTextPropertySerializer<T> :
		TypeSerializer<T>
	{
		private string _name;
		private TypeReader<T> _reader;
		private TypeWriter<T> _writer;

		public FastTextPropertySerializer(TypeSerializer<T> serializer, PropertyInfo property)
		{
			_writer = serializer.GetWriter();
			_reader = serializer.GetReader();

			_name = property.Name;
		}

		public TypeReader<T> GetReader()
		{
			return value =>
				{
					return _reader(value);
				};
		}

		public TypeWriter<T> GetWriter()
		{
			return (value, output) =>
				{
					Action<string> write = null;
					write = text =>
						{
							output(_name);
							output(FastTextParser.MapSeparatorString);
							output(text);

							write = s => output(s);
						};
					_writer(value, text => write(text));
				};
		}
	}
}