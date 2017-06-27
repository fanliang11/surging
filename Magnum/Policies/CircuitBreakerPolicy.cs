// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Magnum.Policies
{
	using System;

	public static class CircuitBreakerPolicy
	{
		public static ExceptionPolicy CircuitBreak(this PolicyBuilder<ExceptionHandler> builder, TimeSpan duration, int threshold)
		{
			Guard.AgainstNull(builder, "builder");

			if (duration == default(TimeSpan))
				throw new ArgumentOutOfRangeException("duration", "must be greater than zero");
			if (threshold <= 0)
				throw new ArgumentOutOfRangeException("threshold", "must be greater than zero");

			var breaker = new CircuitBreaker(duration, threshold);

			return new ExceptionPolicy(action => ImplementPolicy(action, builder.Condition, breaker));
		}

		private static void ImplementPolicy(Action action, ExceptionHandler isHandled, CircuitBreaker breaker)
		{
			if (breaker.IsBroken)
				throw breaker.LastException;

			try
			{
				action();
				breaker.Reset();
				return;
			}
			catch (Exception ex)
			{
				if (!isHandled(ex))
					throw;

				breaker.TryBreak(ex);
				throw;
			}
		}
	}
}