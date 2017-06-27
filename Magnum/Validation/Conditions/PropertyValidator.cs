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
	using System.Collections.Generic;
	using System.Linq.Expressions;


	public class PropertyValidator<T, TProperty> :
		Validator<T>
		where T : class
	{
		readonly Func<T, TProperty> _propertyAccessor;
		readonly Expression<Func<T, TProperty>> _propertyExpression;
		readonly Validator<TProperty> _valueValidator;

		public PropertyValidator(Expression<Func<T, TProperty>> propertyExpression, Validator<TProperty> valueValidator)
		{
			_valueValidator = valueValidator;
			_propertyExpression = propertyExpression;

			_propertyAccessor = _propertyExpression.Compile();
		}

		public IEnumerable<Violation> Validate(T value)
		{
			if (value == null)
				yield break;

			TProperty propertyValue = _propertyAccessor(value);

			foreach (Violation violation in _valueValidator.Validate(propertyValue))
				yield return violation;
		}
	}
}