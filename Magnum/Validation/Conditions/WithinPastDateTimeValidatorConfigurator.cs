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


	class WithinPastDateTimeValidatorConfigurator :
		DateTimeValidatorConfiguratorImpl
	{
		readonly TimeSpan _period;

		public WithinPastDateTimeValidatorConfigurator(ValidatorConfigurator<DateTime> configurator, TimeSpan period)
			: base(configurator)
		{
			_period = period;
		}

		public override void Configure(ValidatorBuilder<DateTime> builder)
		{
			var validator = new WithinPastDateTimeValidator(_period);

			builder.AddValidator(validator);
		}

		public override void ValidateConfiguration()
		{
			if (_period < TimeSpan.Zero)
				throw new ValidationException("The time span must be greater than or equal to zero");
		}
	}
}