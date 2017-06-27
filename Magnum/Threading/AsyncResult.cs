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
namespace Magnum.Threading
{
	using System;
	using System.Threading;

	public class AsyncResult :
		IAsyncResult
	{
		private readonly AsyncCallback _callback;
		private readonly object _state;
		private volatile bool _completed;
		private ManualResetEvent _completedEvent = new ManualResetEvent(false);

		public AsyncResult()
		{
			_state = null;
		}

		public AsyncResult(AsyncCallback callback, object state)
		{
			_callback = callback;
			_state = state;
		}

		public Exception Exception { get; private set; }

		public bool IsCompleted
		{
			get { return _completed; }
		}

		public WaitHandle AsyncWaitHandle
		{
			get { return _completedEvent; }
		}

		public object AsyncState
		{
			get { return _state; }
		}

		public bool CompletedSynchronously
		{
			get { return false; }
		}

		public void SetAsCompleted()
		{
			_completed = true;
			_completedEvent.Set();

			if (_callback != null)
				_callback(this);
		}

		public void SetAsCompleted(Exception exception)
		{
			Exception = exception;

			SetAsCompleted();
		}

		public void EndInvoke()
		{
			if (!IsCompleted)
				_completedEvent.WaitOne();

			if (_completedEvent != null)
			{
				_completedEvent.Close();
				_completedEvent = null;
			}

			if (Exception != null)
				throw Exception;
		}
	}
}