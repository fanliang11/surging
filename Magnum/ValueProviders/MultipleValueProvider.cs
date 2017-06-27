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
	using System.Linq;
	using Extensions;

	/// <summary>
	///   Requests a value from an ordered list of value providers
	/// </summary>
	public class MultipleValueProvider :
		ValueProvider
	{
		private readonly IList<ValueProvider> _providers;

		public MultipleValueProvider(IEnumerable<ValueProvider> providers)
		{
			_providers = new List<ValueProvider>(providers);
		}

		public bool GetValue(string key, Func<object, bool> matchingValueAction)
		{
			return _providers.Any(x => x.GetValue(key, matchingValueAction));
		}

		public bool GetValue(string key, Func<object, bool> matchingValueAction, Action missingValueAction)
		{
			if (_providers.Any(x => x.GetValue(key, matchingValueAction)))
				return true;

			missingValueAction();
			return false;
		}

		public void GetAll(Action<string, object> valueAction)
		{
			_providers.Each(provider => provider.GetAll(valueAction));
		}
	}
}