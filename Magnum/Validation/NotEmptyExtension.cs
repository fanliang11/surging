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
namespace Magnum.Validation
{
	using System.Collections.Generic;
	using Conditions;
	using Impl;


	public static class NotEmptyExtension
	{
		public static ValidatorConfigurator<string> NotEmpty(this ValidatorConfigurator<string> configurator)
		{
			var notEmptyConfigurator = new NotEmptyConfigurator();

			configurator.AddConfigurator(notEmptyConfigurator);

			return configurator;
		}

		public static ValidatorConfigurator<IList<T>> NotEmpty<T>(this ValidatorConfigurator<IList<T>> configurator)
		{
			var notEmptyConfigurator = new NotEmptyListConfigurator<T>();

			configurator.AddConfigurator(notEmptyConfigurator);

			return configurator;
		}
	}
}