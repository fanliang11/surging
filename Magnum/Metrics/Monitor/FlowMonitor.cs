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

	public class FlowMonitor :
		MonitorBase
	{
		private long _bytesRead;
		private long _bytesWritten;
		private long _readCount;
		private long _writeCount;

		public FlowMonitor(Type ownerType, string name)
			: base(ownerType, name)
		{
		}

		public long WriteCount
		{
			get { return _writeCount; }
		}

		public long BytesWritten
		{
			get { return _bytesWritten; }
		}

		public long ReadCount
		{
			get { return _readCount; }
		}

		public long BytesRead
		{
			get { return _bytesRead; }
		}

		public void IncrementWrite(long length)
		{
			Interlocked.Increment(ref _writeCount);
			Interlocked.Add(ref _bytesWritten, length);
		}

		public void IncrementRead(long length)
		{
			Interlocked.Increment(ref _readCount);
			Interlocked.Add(ref _bytesRead, length);
		}
	}
}