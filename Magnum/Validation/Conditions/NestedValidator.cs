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
	using System.Linq;
	using Impl;


	public class NestedValidator<T> :
		Validator<T>
	{
		readonly Validator<T> _validator;

		public NestedValidator(Validator<T> validator)
		{
			_validator = validator;
		}

		public IEnumerable<Violation> Validate(T value)
		{
			return _validator.Validate(value).Select(TranslateViolation);
		}

		static Violation TranslateViolation(Violation x)
		{
			string key = x.Key.Contains(".") ? x.Key.Substring(x.Key.IndexOf('.')) : "";

			return new ValidatorViolation<T>(key, x.Message);
		}
	}
}