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

	public class AccessorValueProvider :
		ValueProvider
	{
		private readonly Func<IEnumerable<string>> _getKeys;
		private readonly Func<string, object> _getValue;

		public AccessorValueProvider(Func<string, object> getValue, Func<IEnumerable<string>> getKeys)
		{
			_getValue = getValue;
			_getKeys = getKeys;
		}

		public bool GetValue(string key, Func<object, bool> matchingValueAction)
		{
			object result = _getValue(key);
			if (result != null)
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
			_getKeys().Each(key => valueAction(key, _getValue(key)));
		}
	}
}