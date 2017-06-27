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

	public class NestedValueProvider :
		ValueProvider
	{
		private readonly ValueProvider _parent;
		private readonly string _prefix;

		public NestedValueProvider(ValueProvider parent, string prefix)
		{
			_parent = parent;
			_prefix = prefix;
		}

		public bool GetValue(string key, Func<object, bool> matchingValueAction)
		{
			return _parent.GetValue(_prefix + key, matchingValueAction);
		}

		public bool GetValue(string key, Func<object, bool> matchingValueAction, Action missingValueAction)
		{
			return _parent.GetValue(_prefix + key, matchingValueAction, missingValueAction);
		}

		public void GetAll(Action<string, object> valueAction)
		{
			_parent.GetAll(valueAction);
		}
	}
}