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
	using System.Diagnostics;

	public class ProcessMonitor :
		MonitorBase
	{
		public ProcessMonitor(Type ownerType, string name)
			: base(ownerType, name)
		{
		}

		public int ProcessorCount
		{
			get { return Environment.ProcessorCount; }
		}

		public long MemoryUsed
		{
			get { return Process.GetCurrentProcess().WorkingSet64 >> 20; }
		}

		public int ThreadCount
		{
			get { return Process.GetCurrentProcess().Threads.Count; }
		}

		public TimeSpan ProcessorTimeUsed
		{
			get { return Process.GetCurrentProcess().TotalProcessorTime; }
		}
	}
}