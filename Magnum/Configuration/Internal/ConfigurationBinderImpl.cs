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
namespace Magnum.Configuration.Internal
{
	using System;
	using System.Collections.Generic;
	using Binding;
	using Extensions;
	using ValueProviders;


	public class ConfigurationBinderImpl :
		ConfigurationBinder
	{
		readonly ModelBinder _binder;
		readonly ModelBinderContext _context;
		readonly ValueProvider _provider;

		public ConfigurationBinderImpl(ValueProvider provider)
		{
			_provider = provider;
			_binder = new FastModelBinder();

			_context = new ConfigurationBinderContext(provider);
		}

		public T Bind<T>()
		{
			var obj = _binder.Bind<T>(_context);

			return obj;
		}

        public object Bind(Type typeToBindTo)
        {
            var obj = _binder.Bind(typeToBindTo, _context);

            return obj;
        }

		public object GetValue(string key)
		{
			object resultValue = null;
			_provider.GetValue(key, value =>
				{
					resultValue = value;

					return true;
				});

			return resultValue;
		}

		public T GetValue<T>(string key)
		{
			object value = GetValue(key);
			if (value == null)
				return default(T);

			if (typeof(T).IsAssignableFrom(value.GetType()))
				return (T)value;

			throw new ArgumentException("The value type {0} is not assignable to the requested type {1}"
			                            	.FormatWith(value.GetType().Name, typeof(T).Name));
		}

		public string GetValueAsString(string key)
		{
			object value = GetValue(key);
			if (value == null)
				return null;

			return value.ToString();
		}

		public IDictionary<string, object> GetAll()
		{
			var found = new Dictionary<string, object>();
			_provider.GetAll((s, o) =>
				{
					if (found.ContainsKey(s))
						found[s] = o;
					else
						found.Add(s, o);
				});

			return found;
		}
	}
}