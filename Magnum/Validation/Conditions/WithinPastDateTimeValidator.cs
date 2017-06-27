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
	using Impl;


	class WithinPastDateTimeValidator :
		Validator<DateTime>
	{
		readonly TimeSpan _period;
		string _periodText;

		public WithinPastDateTimeValidator(TimeSpan period)
		{
			_period = period;
			_periodText = _period.ToFriendlyString();
		}

		public IEnumerable<Violation> Validate(DateTime value)
		{
			DateTime now = value.Kind == DateTimeKind.Utc ? DateTime.UtcNow.Date : DateTime.Now.Date;
			DateTime past = now - _period;

			if (value.Date < past || value.Date > now)
				yield return new ValidatorViolation<DateTime>("must be within the past " + _periodText);
		}
	}
}