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


	public class EnumerableValidator<T, TElement> :
		Validator<T>
		where T : class, IEnumerable<TElement>
	{
		readonly Validator<TElement> _elementValidator;

		public EnumerableValidator(Validator<TElement> elementValidator)
		{
			_elementValidator = elementValidator;
		}

		public IEnumerable<Violation> Validate(T value)
		{
			if (value == null)
				yield break;

			IEnumerable<Violation<T>> violations = value
				.SelectMany((item, index) => _elementValidator
				                             	.Validate(item)
				                             	.Select(x => TranslateViolation(index, x)));

			foreach (var violation in violations)
				yield return violation;
		}

		static Violation<T> TranslateViolation(int index, Violation x)
		{
			string key = "[" + index + "]" + x.Key;

			return new ValidatorViolation<T>(key, x.Message);
		}
	}
}