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
namespace Magnum.Concurrency.Internal
{
	using System;
	using System.Threading;


	/// <summary>
	/// Maintains a reference to an immutable object, allowing a mutator function to 
	/// change the reference pointed to in an atomic fashion
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class AtomicReference<T> :
		Atomic<T>
		where T : class
	{
		public AtomicReference(T initialValue)
		{
			Value = initialValue;
		}

		/// <summary>
		/// Applies a change method to the value, passing the current value to the mutator
		/// and attempting to set the value to the returned value. if the value has not been
		/// changed by another thread, the operation completes, otherwise it is attempted 
		/// again by calling the mutator function with the new value.
		/// </summary>
		/// <param name="mutator">A function that, given the current value, returns the changed value</param>
		/// <returns>The previous value</returns>
		public override T Set(Func<T, T> mutator)
		{
			for (;;)
			{
				T originalValue = Value;

				T changedValue = mutator(originalValue);

				T previousValue = Interlocked.CompareExchange(ref Value, changedValue, originalValue);

				// if the value returned is equal to the original value, we made the change
				if (previousValue == originalValue)
					return previousValue;
			}
		}
	}
}