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
namespace Magnum.Concurrency
{
	using System;
	using Internal;


	/// <summary>
	/// The factory to create atomic instances/references
	/// </summary>
	public static class Atomic
	{
		public static Atomic<int> Create(int initialValue)
		{
			return new AtomicInt32(initialValue);
		}

		public static Atomic<long> Create(long initialValue)
		{
			return new AtomicInt64(initialValue);
		}

		public static Atomic<double> Create(double initialValue)
		{
			return new AtomicDouble(initialValue);
		}

		public static Atomic<float> Create(float initialValue)
		{
			return new AtomicFloat(initialValue);
		}

		public static Atomic<T> Create<T>(T initialValue)
			where T : class
		{
			return new AtomicReference<T>(initialValue);
		}

		public static Atomic<object> Create(object initialValue)
		{
			return new AtomicObject(initialValue);
		}
	}

	/// <summary>
	/// An atomic value that can be modified using a mutator function.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class Atomic<T>
	{
		public T Value;

		/// <summary>
		/// Sets the value to the result of the mutator. Each attempt to set the value
		/// will call the mutator, which could result in multiple calls with different
		/// values on each call.
		/// </summary>
		/// <param name="mutator">A function that takes in the current value and returns the new value</param>
		/// <returns>The value that was replaced</returns>
		public abstract T Set(Func<T, T> mutator);
	}
}