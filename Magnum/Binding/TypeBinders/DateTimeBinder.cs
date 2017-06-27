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
	using System.Xml;

	public class DateTimeBinder :
		ValueTypeBinder<DateTime>
	{
		protected override bool ConvertType(object value, out DateTime result)
		{
			if (base.ConvertType(value, out result))
				return true;

			if (value is DateTimeOffset)
			{
				result = ((DateTimeOffset) value).DateTime;
				return true;
			}

			return false;
		}

		protected override bool ParseType(string text, out DateTime result)
		{
			return DateTime.TryParse(text, out result);
		}

		protected override DateTime UseXmlConvert(string text)
		{
			return XmlConvert.ToDateTime(text, XmlDateTimeSerializationMode.Utc);
		}
	}
}