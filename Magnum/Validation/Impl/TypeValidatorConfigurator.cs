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


	public class TypeValidatorConfigurator<T> :
		ValidatorConfigurator<T>
	{
		readonly IList<Configurator<T>> _configurators;

		public TypeValidatorConfigurator()
		{
			_configurators = new List<Configurator<T>>();
		}

		public void AddConfigurator(Configurator<T> configurator)
		{
			_configurators.Add(configurator);
		}

		public Validator<T> CreateValidator()
		{
			ValidateConfigurators();

			var builder = new TypeValidatorBuilder<T>();

			foreach (var configurator in _configurators)
				configurator.Configure(builder);

			return builder.Build(typeof(T).Name);
		}

		void ValidateConfigurators()
		{
			foreach (var configurator in _configurators)
				configurator.ValidateConfiguration();
		}
	}
}