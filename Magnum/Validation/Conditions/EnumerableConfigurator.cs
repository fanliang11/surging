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
	using System.Collections.Generic;
	using Impl;


	public class EnumerableConfigurator<T, TElement> :
		Configurator<T>
		where T : class, IEnumerable<TElement>
	{
		readonly Configurator<TElement> _configurator;

		public EnumerableConfigurator(Configurator<TElement> configurator)
		{
			_configurator = configurator;
		}

		public void Configure(ValidatorBuilder<T> builder)
		{
			var elementBuilder = new TypeValidatorBuilder<TElement>();

			_configurator.Configure(elementBuilder);

			Validator<TElement> elementValidator = elementBuilder.Build("");

			var nestedValidator = new NestedValidator<TElement>(elementValidator);

			var validator = new EnumerableValidator<T, TElement>(nestedValidator);

			builder.AddValidator(validator);
		}

		public void ValidateConfiguration()
		{
			if (_configurator == null)
				throw new ValidationException("The element configurator cannot be null");
		}
	}
}