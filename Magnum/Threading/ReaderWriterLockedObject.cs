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

	/// <summary>
	/// Contains an object within a ReaderWriterLockContext
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ReaderWriterLockedObject<T> :
		ILockedObject<T>
	{
		private volatile bool _disposed;
		private ReaderWriterLockContext _lockContext;
		private T _value;

		public ReaderWriterLockedObject(T value)
		{
			_value = value;
			_lockContext = new ReaderWriterLockContext();
		}

		public ReaderWriterLockedObject(T value, LockRecursionPolicy policy)
		{
			_value = value;
			_lockContext = new ReaderWriterLockContext(policy);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void ReadUnlocked(Action<T> action)
		{
			action(_value);
		}

		public void ReadLock(Action<T> action)
		{
			if (_disposed) throw new ObjectDisposedException("ReaderWriterLockedObject");

			_lockContext.ReadLock(x => action(_value));
		}

		public V ReadLock<V>(Func<T, V> action)
		{
			if (_disposed) throw new ObjectDisposedException("ReaderWriterLockedObject");

			return _lockContext.ReadLock(x => action(_value));
		}

		public bool ReadLock(TimeSpan timeout, Action<T> action)
		{
			if (_disposed) throw new ObjectDisposedException("ReaderWriterLockedObject");

			return _lockContext.ReadLock(timeout, x => action(_value));
		}

		public void UpgradeableReadLock(Action<T> action)
		{
			if (_disposed) throw new ObjectDisposedException("ReaderWriterLockedObject");

			_lockContext.UpgradeableReadLock(x => action(_value));
		}

		public V UpgradeableReadLock<V>(Func<T, V> action)
		{
			if (_disposed) throw new ObjectDisposedException("ReaderWriterLockedObject");

			return _lockContext.UpgradeableReadLock(x => action(_value));
		}

		public bool UpgradeableReadLock(TimeSpan timeout, Action<T> action)
		{
			if (_disposed) throw new ObjectDisposedException("ReaderWriterLockedObject");

			return _lockContext.UpgradeableReadLock(timeout, x => action(_value));
		}

		public void WriteLock(Func<T, T> action)
		{
			if (_disposed) throw new ObjectDisposedException("ReaderWriterLockedObject");

			_lockContext.WriteLock(x => _value = action(_value));
		}

		public void WriteLock(Action<T> action)
		{
			if (_disposed) throw new ObjectDisposedException("ReaderWriterLockedObject");

			_lockContext.WriteLock(x => action(_value));
		}

		public V WriteLock<V>(Func<T, V> action)
		{
			if (_disposed) throw new ObjectDisposedException("ReaderWriterLockedObject");

			return _lockContext.WriteLock(x => action(_value));
		}

		public bool WriteLock(TimeSpan timeout, Func<T, T> action)
		{
			if (_disposed) throw new ObjectDisposedException("ReaderWriterLockedObject");

			return _lockContext.WriteLock(timeout, x => _value = action(_value));
		}

		public void WriteLock(TimeSpan timeout, Action<T> action)
		{
			if (_disposed) throw new ObjectDisposedException("ReaderWriterLockedObject");

			_lockContext.WriteLock(timeout, x => action(_value));
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _disposed) return;

			if (_lockContext != null)
				_lockContext.Dispose();

			_lockContext = null;
			_disposed = true;
		}

		~ReaderWriterLockedObject()
		{
			Dispose(false);
		}
	}
}