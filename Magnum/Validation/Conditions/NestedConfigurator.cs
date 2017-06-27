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
	using Impl;


	public class NestedConfigurator<T> :
		Configurator<T>
	{
		readonly Validator<T> _nestedValidator;

		public NestedConfigurator(Validator<T> nestedValidator)
		{
			_nestedValidator = nestedValidator;
		}

		public void Configure(ValidatorBuilder<T> builder)
		{
			var validator = new NestedValidator<T>(_nestedValidator);

			builder.AddValidator(validator);
		}

		public void ValidateConfiguration()
		{
			if (_nestedValidator == null)
				throw new ValidationException("Nested validator cannot be null");
		}
	}
}