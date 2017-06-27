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
namespace Magnum.Validation.Impl
{
	using System.Collections.Generic;
	using System.Linq;


	class TypeValidator<T> :
		Validator<T>
	{
		readonly string _name;
		readonly IList<Validator<T>> _validators;

		public TypeValidator(string name, IEnumerable<Validator<T>> validators)
		{
			_name = name;
			_validators = validators.ToList();
		}

		public IEnumerable<Violation> Validate(T value)
		{
			foreach (var validator in _validators)
			{
				foreach (Violation violation in validator.Validate(value))
					yield return new ValidatorViolation<T>(_name + violation.Key, violation.Message);
			}
		}
	}
}