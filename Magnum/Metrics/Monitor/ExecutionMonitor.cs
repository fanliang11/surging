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
	using System.Threading;

	public class ExecutionMonitor :
		MonitorBase
	{
		private long _completed;
		private long _failed;
		private long _started;

		public ExecutionMonitor(Type ownerType, string name)
			: base(ownerType, name)
		{
		}

		public long Completed
		{
			get { return _completed; }
		}

		public long Started
		{
			get { return _started; }
		}

		public long Failed
		{
			get { return _failed; }
		}

		public long Executing
		{
			get { return _started - _failed - _completed; }
		}

		public void IncrementStarted()
		{
			Interlocked.Increment(ref _started);
		}

		public void IncrementCompleted()
		{
			Interlocked.Increment(ref _completed);
		}

		public void IncrementFailed()
		{
			Interlocked.Increment(ref _failed);
		}
	}
}