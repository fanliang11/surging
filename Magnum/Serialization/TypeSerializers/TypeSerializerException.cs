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
	using System.Runtime.Serialization;
	using Extensions;

	[Serializable]
	public class TypeSerializerException :
		FormatException
	{
		public TypeSerializerException()
		{
		}

		public TypeSerializerException(string message)
			: base(message)
		{
		}

		public TypeSerializerException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected TypeSerializerException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public static TypeSerializerException New<T>(TypeSerializer<T> serializer, string value)
		{
			return new TypeSerializerException("Unable to convert {0} to an {1}".FormatWith(value, typeof (T).Name));
		}

		public static TypeSerializerException New<T>(TypeSerializer<T> serializer, string value, Exception innerException)
		{
			return new TypeSerializerException("Unable to convert {0} to an {1}".FormatWith(value, typeof (T).Name),
			                                   innerException);
		}

		public static TypeSerializerException New<T>(string value, Exception innerException)
		{
			return new TypeSerializerException("Unable to convert {0} to an {1}".FormatWith(value, typeof (T).Name),
			                                   innerException);
		}
	}
}