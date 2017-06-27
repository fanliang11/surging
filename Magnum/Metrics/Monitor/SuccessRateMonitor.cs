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
namespace Magnum.Metrics.Monitor
{
	using System;
	using Extensions;

	public class SuccessRateMonitor :
		MonitorBase
	{
		private readonly long _itemThreshold;
		private readonly ExecutionMonitor _monitor;
		private readonly long _timeThreshold;
		private long _previousCompleted;
		private long _previousFailed;
		private long _previousTicks;

		public SuccessRateMonitor(Type monitorType, string name, ExecutionMonitor monitor)
			: this(monitorType, name, monitor, 100, 5.Seconds())
		{
		}

		public SuccessRateMonitor(Type monitorType, string name, ExecutionMonitor monitor, long itemThreshold, TimeSpan timeThreshold)
			: base(monitorType, name)
		{
			_monitor = monitor;

			InitializePreviousValues();

			_itemThreshold = itemThreshold;
			_timeThreshold = timeThreshold.Ticks;
		}

		public long SuccessRate
		{
			get { return GetSuccessRate(); }
		}

		private long GetSuccessRate()
		{
			long completed = _monitor.Completed;
			long failed = _monitor.Failed;

			long rate = GetCurrentSuccessRate(completed, failed);
			if (rate == -1)
				return rate;

			long newItems = (completed - _previousCompleted) + (failed - _previousFailed);
			long now = SystemUtil.Now.Ticks;
			long elapsedTime = now - _previousTicks;

			if (newItems >= _itemThreshold || elapsedTime >= _timeThreshold)
			{
				_previousCompleted = completed;
				_previousFailed = failed;
				_previousTicks = now;
			}

			return rate;
		}

		private long GetCurrentSuccessRate(long completed, long failed)
		{
			try
			{
				long justCompleted = completed - _previousCompleted;
				long justFailed = failed - _previousFailed;

				if (completed < 0 || failed < 0 || justCompleted < 0 || justFailed < 0)
					return -1;

				if (justCompleted + justFailed == 0)
					return 100;

				return CalculateSuccessRate(justCompleted, justFailed);
			}
			catch
			{
				return -1;
			}
		}

		private void InitializePreviousValues()
		{
			_previousCompleted = _monitor.Completed;
			_previousFailed = _monitor.Failed;

			_previousTicks = SystemUtil.Now.Ticks;
		}

		private static long CalculateSuccessRate(long completed, long failed)
		{
			return (long) Math.Round(100 - (100*(double) failed)/(completed + failed));
		}
	}
}