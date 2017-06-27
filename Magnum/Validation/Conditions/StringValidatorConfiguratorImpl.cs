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
	using System;
	using Impl;


	class StringValidatorConfiguratorImpl :
		StringValidatorConfigurator,
		Configurator<string>
	{
		readonly ValidatorConfigurator<string> _configurator;
		readonly string _message;
		readonly Func<string, bool> _validate;

		protected StringValidatorConfiguratorImpl(ValidatorConfigurator<string> configurator, Func<string, bool> validate,
		                                          string message)
		{
			_configurator = configurator;
			_validate = validate;
			_message = message;
		}

		public void Configure(ValidatorBuilder<string> builder)
		{
			var validator = new StringValidator(_validate, _message);

			builder.AddValidator(validator);
		}

		public virtual void ValidateConfiguration()
		{
		}

		public void AddConfigurator(Configurator<string> configurator)
		{
			_configurator.AddConfigurator(configurator);
		}
	}
}