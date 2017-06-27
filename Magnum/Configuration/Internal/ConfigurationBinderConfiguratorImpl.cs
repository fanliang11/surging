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
	using System.IO;
	using System.Linq;
	using ValueProviders;


	public class ConfigurationBinderConfiguratorImpl :
		ConfigurationBinderConfigurator
	{
		readonly IList<Func<ValueProvider>> _providers = new List<Func<ValueProvider>>();

		public void AddJsonFile(string filename)
		{
			_providers.Add(() => new FileValueProvider(filename, stream => new JsonValueProvider(stream)));
		}

		public void AddJson(Stream stream)
		{
			_providers.Add(() => new JsonValueProvider(stream));
		}

		public void AddJson(string text)
		{
			_providers.Add(() => new JsonValueProvider(text));
		}

		public void AddCommandLine(string commandLine)
		{
			_providers.Add(() => new CommandLineValueProvider(commandLine));
		}

		public ConfigurationBinder CreateBinder()
		{
			var providers = new MultipleValueProvider(_providers.Reverse().Select(x => x()));

			return new ConfigurationBinderImpl(providers);
		}
	}
}