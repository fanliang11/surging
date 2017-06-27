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

	public class CircuitBreaker
	{
		private readonly int _breakThreshold;
		private readonly TimeSpan _duration;

		private DateTime _brokenUntil;
		private int _count;
		private Exception _lastException;

		public CircuitBreaker(TimeSpan duration, int breakThreshold)
		{
			_duration = duration;
			_breakThreshold = breakThreshold;

			Reset();
		}

		public Exception LastException
		{
			get { return _lastException; }
		}

		public bool IsBroken
		{
			get { return SystemUtil.Now < _brokenUntil; }
		}

		public void Reset()
		{
			_count = 0;
			_brokenUntil = DateTime.MinValue;
			_lastException = new InvalidOperationException("This is the default exception for the circuit breaker, no exception has been thrown.");
		}

		public void TryBreak(Exception ex)
		{
			_lastException = ex;

			_count++;
			if (_count >= _breakThreshold)
			{
				BreakTheCircuit();
			}
		}

		private void BreakTheCircuit()
		{
			_brokenUntil = SystemUtil.Now.Add(_duration);
		}
	}
}