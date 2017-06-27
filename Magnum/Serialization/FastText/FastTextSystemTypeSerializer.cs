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
namespace Magnum.Serialization.FastText
{
	using System;
	using TypeSerializers;


	public class FastTextSystemTypeSerializer :
		TypeSerializer<Type>
	{
		readonly TypeSerializer<string> _stringSerializer = new FastTextStringSerializer();
		readonly TypeSerializer<Type> _typeSerializer = new SystemTypeSerializer();

		public TypeReader<Type> GetReader()
		{
			TypeReader<Type> typeReader = _typeSerializer.GetReader();
			TypeReader<string> stringReader = _stringSerializer.GetReader();

			return value =>
				{
					string typeString = stringReader(value);

					Type typeValue = typeReader(typeString);

					return typeValue;
				};
		}

		public TypeWriter<Type> GetWriter()
		{
			TypeWriter<Type> typeWriter = _typeSerializer.GetWriter();
			TypeWriter<string> stringWriter = _stringSerializer.GetWriter();

			return (value, output) =>
				{
					if (value == null)
						return;

					typeWriter(value, x => stringWriter(x, output));
				};
		}
	}
}