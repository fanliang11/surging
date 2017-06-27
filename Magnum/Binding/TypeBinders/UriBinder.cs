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
namespace Magnum.Binding.TypeBinders
{
	using System;
	using System.Runtime.Serialization;

	public class UriBinder :
		ValueTypeBinder<Uri>
	{
		protected override bool ParseType(string text, out Uri result)
		{
			try
			{
				result = new Uri(text);
				return true;
			}
			catch (UriFormatException ex)
			{
				throw new SerializationException("The Uri is in an invalid format: " + text, ex);
			}
		}

		protected override Uri UseXmlConvert(string text)
		{
			throw new NotImplementedException("No other way to convert a Uri");
		}
	}
}