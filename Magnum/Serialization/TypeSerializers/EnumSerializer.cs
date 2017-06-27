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

	public class EnumSerializer<T> :
		TypeSerializer<T>
	{
		public TypeReader<T> GetReader()
		{
			return value =>
				{
					try
					{
						var result = (T) Enum.Parse(typeof (T), value);
						return result;
					}
					catch (Exception ex)
					{
						throw TypeSerializerException.New(this, value, ex);
					}
				};
		}

		public TypeWriter<T> GetWriter()
		{
			return (value, output) => output(value.ToString());
		}
	}
}