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
	using System.Collections.Generic;
	using System.Threading;

	/// <summary>
	/// Attempts to provide a method of running asynchronous methods (the BeginXXX/EndXXX style)
	/// in a linear manner avoiding all the propogation of those methods up the call stack
	/// 
	/// Slightly influence by Jeffrey Richter's PowerThreading library
	/// http://wintellect.com/PowerThreading.aspx
	/// 
	/// </summary>
	public class AsyncExecutor :
		IAsyncExecutor
	{
		private readonly object _enumeratorLock = new object();
		private readonly ReaderWriterLockedObject<Queue<IAsyncResult>> _resultQueue;
		private readonly SynchronizationContext _syncContext;
		private readonly ReaderWriterLockedObject<int> _waitCount;
		private AsyncResult _asyncResult;
		private volatile bool _cancelled;
		private IEnumerator<int> _enumerator;

		public AsyncExecutor()
		{
			_resultQueue = new ReaderWriterLockedObject<Queue<IAsyncResult>>(new Queue<IAsyncResult>());
			_waitCount = new ReaderWriterLockedObject<int>(0);

			_syncContext = SynchronizationContext.Current;
		}

		public void Cancel()
		{
			_cancelled = true;
		}

		public void Execute(IEnumerator<int> enumerator)
		{
			EndExecute(BeginExecute(enumerator, null, null));
		}

		public IAsyncResult BeginExecute(IEnumerator<int> enumerator, AsyncCallback callback, object state)
		{
			_enumerator = enumerator;
			_asyncResult = new AsyncResult(callback, state);

			ContinueEnumerator(true);

			return _asyncResult;
		}

		public void EndExecute(IAsyncResult asyncResult)
		{
			_asyncResult.EndInvoke();
			_asyncResult = null;
		}

		public AsyncCallback End()
		{
			return EnqueueResultToInbox;
		}

		public IAsyncResult Result()
		{
			return _resultQueue.WriteLock(x => x.Dequeue());
		}

		private void EnqueueResultToInbox(IAsyncResult asyncResult)
		{
			_resultQueue.WriteLock(x => x.Enqueue(asyncResult));

			if (_resultQueue.ReadLock(x => x.Count) == _waitCount.ReadLock(x => x))
			{
				ContinueEnumerator(false);
			}
		}

		private void ContinueEnumerator(bool outsideOfSyncContext)
		{
			if (_syncContext != null && outsideOfSyncContext)
			{
				_syncContext.Post(SyncContextContinueEnumerator, this);
				return;
			}

			Exception caughtException = null;

			lock (_enumeratorLock)
			{
				bool stillGoing = false;
				try
				{
					while ((stillGoing = _enumerator.MoveNext()))
					{
						if (HasBeenCancelled())
							continue;

						int expectedResultCount = _enumerator.Current;
						if (expectedResultCount == 0)
						{
							ThreadPool.QueueUserWorkItem(ThreadPoolContinueEnumerator, this);
							return;
						}

						_waitCount.WriteLock(x => expectedResultCount);
						return;
					}
				}
				catch (Exception ex)
				{
					caughtException = ex;
					throw;
				}
				finally
				{
					if (!stillGoing)
					{
						_enumerator.Dispose();

						if (caughtException != null)
						{
							_asyncResult.SetAsCompleted(caughtException);
						}
						else
						{
							_asyncResult.SetAsCompleted();
						}
					}
				}
			}
		}

		private bool HasBeenCancelled()
		{
			return _cancelled;
		}

		private static void SyncContextContinueEnumerator(object state)
		{
			((AsyncExecutor) state).ContinueEnumerator(false);
		}

		private static void ThreadPoolContinueEnumerator(object state)
		{
			((AsyncExecutor) state).ContinueEnumerator(true);
		}

		public static void Run(Func<IAsyncExecutor, IEnumerator<int>> action)
		{
			AsyncExecutor executor = new AsyncExecutor();

			executor.Execute(action(executor));
		}

		public static IAsyncResult RunAsync(Func<IAsyncExecutor, IEnumerator<int>> action, Action complete)
		{
			AsyncExecutor executor = new AsyncExecutor();

			AsyncCallback callback = x =>
				{
					executor.EndExecute(x);
					complete();
				};

			return executor.BeginExecute(action(executor), callback, null);
		}

		public static IAsyncResult RunAsync<T>(Func<IAsyncExecutor, IEnumerator<int>> action, T state, Action<T> complete)
		{
			AsyncExecutor executor = new AsyncExecutor();

			AsyncCallback callback = x =>
				{
					executor.EndExecute(x);
					complete(state);
				};

			return executor.BeginExecute(action(executor), callback, state);
		}
	}
}