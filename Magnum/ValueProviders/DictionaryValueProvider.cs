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
namespace Magnum.ValueProviders
{
	using System;
	using System.Collections.Generic;
	using Extensions;

	public class DictionaryValueProvider :
		ValueProvider
	{
		private readonly IDictionary<string, object> _values;

		public DictionaryValueProvider(IDictionary<string, object> dictionary)
		{
			_values = dictionary;
		}

		public bool GetValue(string key, Func<object, bool> matchingValueAction)
		{
			object result;
			bool found = _values.TryGetValue(key, out result);
			if (found)
			{
				return matchingValueAction(result);
			}

			return false;
		}

		public bool GetValue(string key, Func<object, bool> matchingValueAction, Action missingValueAction)
		{
			if (GetValue(key, matchingValueAction))
				return true;

			missingValueAction();
			return false;
		}

		public void GetAll(Action<string, object> valueAction)
		{
			_values.Each(x => valueAction(x.Key, x.Value));
		}
	}
}