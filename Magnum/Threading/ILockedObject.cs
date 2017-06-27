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

	/// <summary>
	/// Interface to a locked object of type T
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ILockedObject<T> :
		IDisposable
	{
		/// <summary>
		/// Access the contained object without any locking
		/// </summary>
		/// <param name="action"></param>
		void ReadUnlocked(Action<T> action);

		/// <summary>
		/// Access the contained object within a read lock
		/// </summary>
		/// <param name="action"></param>
		void ReadLock(Action<T> action);

		V ReadLock<V>(Func<T, V> action);

		/// <summary>
		/// Access the contained object within a read lock if possible before the timeout expires
		/// </summary>
		/// <param name="timeout">The time to wait for a lock before returning false</param>
		/// <param name="action"></param>
		/// <returns>True if the lock was obtained and the action called, otherwise false</returns>
		bool ReadLock(TimeSpan timeout, Action<T> action);

		/// <summary>
		/// Access the contained object within an upgradeable read lock
		/// </summary>
		/// <param name="action"></param>
		void UpgradeableReadLock(Action<T> action);

		V UpgradeableReadLock<V>(Func<T, V> action);
		
		bool UpgradeableReadLock(TimeSpan timeout, Action<T> action);

		/// <summary>
		/// Access the contained object within a write lock
		/// </summary>
		/// <param name="action"></param>
		void WriteLock(Func<T, T> action);
	
		void WriteLock(Action<T> action);

		V WriteLock<V>(Func<T, V> action);
		
		bool WriteLock(TimeSpan timeout, Func<T, T> action);
		void WriteLock(TimeSpan timeout, Action<T> action);
	}
}