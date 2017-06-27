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
namespace Magnum.ValueProviders
{
	using System;
	using System.Collections.Generic;


	/// <summary>
	///   Makes wrapping another value provider easy, including logging of values as they are utilized
	/// </summary>
	public abstract class ValueProviderDecorator :
		ValueProvider
	{
		static readonly HashSet<string> _obscuredKeys;
		readonly ValueProvider _provider;

		static ValueProviderDecorator()
		{
			_obscuredKeys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
				{
					"password",
					"pw",
				};
		}

		protected ValueProviderDecorator(ValueProvider provider)
		{
			_provider = provider;
		}

		protected abstract string ProviderName { get; }

		public bool GetValue(string key, Func<object, bool> matchingValueAction)
		{
			object returnedValue = null;
			bool result = _provider.GetValue(key, value =>
				{
					returnedValue = ObscureValueIfNecessary(key, value);

					return matchingValueAction(value);
				});

			return result;
		}

		public bool GetValue(string key, Func<object, bool> matchingValueAction, Action missingValueAction)
		{
			object returnedValue = null;
			bool result = _provider.GetValue(key, value =>
				{
					returnedValue = ObscureValueIfNecessary(key, value);

					return matchingValueAction(value);
				}, missingValueAction);

			return result;
		}

		public void GetAll(Action<string, object> valueAction)
		{
			_provider.GetAll(valueAction);
		}

		static string ObscureValueIfNecessary(string key, object value)
		{
			if (value == null)
				return "(null)";

			string text = value.ToString();

			if (_obscuredKeys.Contains(key))
				return new string('*', text.Length);

			if (text.Length > 100)
				return text.Substring(0, 100) + "... (" + text.Length + " bytes)";

			return text;
		}
	}
}