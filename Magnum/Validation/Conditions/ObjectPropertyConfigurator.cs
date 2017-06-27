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
	using System.Reflection;
	using Impl;


	public class ObjectPropertyConfigurator<T, TProperty> :
		PropertyConfigurator<T, TProperty>,
		Configurator<T>
		where T : class
	{
		readonly IList<Configurator<TProperty>> _configurators;
		readonly Expression<Func<T, TProperty>> _propertyExpression;

		public ObjectPropertyConfigurator(Expression<Func<T, TProperty>> propertyExpression)
		{
			_propertyExpression = propertyExpression;

			_configurators = new List<Configurator<TProperty>>();
		}

		public void Configure(ValidatorBuilder<T> builder)
		{
			var propertyValidatorBuilder = new TypeValidatorBuilder<TProperty>();
			foreach (var configurator in _configurators)
				configurator.Configure(propertyValidatorBuilder);

			Validator<TProperty> propertyValueValidator =
				propertyValidatorBuilder.Build("." + GetPropertyName(_propertyExpression));

			var validator = new PropertyValidator<T, TProperty>(_propertyExpression, propertyValueValidator);

			builder.AddValidator(validator);
		}

		public void ValidateConfiguration()
		{
			Expression expression = _propertyExpression.Body;
			var me = expression as MemberExpression;
			if (me == null || me.Member.MemberType != MemberTypes.Property)
				throw new ValidationException("A property accessor must be specified: " + _propertyExpression);

			foreach (var configurator in _configurators)
				configurator.ValidateConfiguration();
		}

		public void AddConfigurator(Configurator<TProperty> configurator)
		{
			_configurators.Add(configurator);
		}

		static string GetPropertyName(Expression<Func<T, TProperty>> propertyExpression)
		{
			Expression expression = propertyExpression.Body;
			var me = expression as MemberExpression;

			return me.Member.Name;
		}
	}
}