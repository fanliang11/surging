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
namespace Magnum.Validation.Conditions
{
	using System.Text.RegularExpressions;
	using Advanced;
	using Impl;


	public class MatchesConfiguratorImpl :
		Configurator<string>,
		MatchesConfigurator
	{
		readonly ValidatorConfigurator<string> _configurator;
		readonly string _pattern;
		Regex _compiled;
		RegexOptions _options;

		public MatchesConfiguratorImpl(ValidatorConfigurator<string> configurator, string pattern)
		{
			_configurator = configurator;
			_pattern = pattern;
			_options = RegexOptions.Compiled;
		}

		public void Configure(ValidatorBuilder<string> builder)
		{
			var validator = new MatchesValidator(_compiled);

			builder.AddValidator(validator);
		}

		public void ValidateConfiguration()
		{
			_compiled = new Regex(_pattern, _options);
		}

		public void AddConfigurator(Configurator<string> configurator)
		{
			// this passes through since we are not modifying any chained configurations
			_configurator.AddConfigurator(configurator);
		}

		public MatchesConfigurator SingleLine()
		{
			_options = (_options & ~RegexOptions.Multiline) | RegexOptions.Singleline;

			return this;
		}
	}
}