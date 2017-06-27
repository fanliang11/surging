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
	using System.Linq;
	using Collections;
	using CommandLineParser;


	public class CommandLineValueProvider :
		ValueProviderDecorator
	{
		public CommandLineValueProvider(string commandLine)
			: base(CreateDictionaryProvider(commandLine))
		{
		}

		/// <summary>
		/// Uses the default command-line for the process
		/// </summary>
		public CommandLineValueProvider()
			: base(CreateDictionaryProvider())
		{
		}

		protected override string ProviderName
		{
			get { return "command-line"; }
		}

		static ValueProvider CreateDictionaryProvider()
		{
			return CreateDictionaryProvider(CommandLine.Parse());
		}

		static ValueProvider CreateDictionaryProvider(string commandLine)
		{
			return CreateDictionaryProvider(new MonadicCommandLineParser().Parse(commandLine));
		}

		static ValueProvider CreateDictionaryProvider(IEnumerable<ICommandLineElement> elements)
		{
			Dictionary<string, object> dictionary = elements.Where(x => x is IDefinitionElement)
				.Cast<IDefinitionElement>()
				.Select(x => new Magnum.Collections.Tuple<string, object>(x.Key, x.Value))
				.Union(elements.Where(x => x is ISwitchElement)
				       	.Cast<ISwitchElement>()
				       	.Select(x => new Magnum.Collections.Tuple<string, object>(x.Key, x.Value)))
				.ToDictionary(x => x.First, x => x.Second, StringComparer.InvariantCultureIgnoreCase);

			var provider = new DictionaryValueProvider(dictionary);

			return provider;
		}
	}
}
