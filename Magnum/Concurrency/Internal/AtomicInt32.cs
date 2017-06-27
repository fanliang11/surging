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


	public class AtomicInt32 :
		Atomic<int>
	{
		public AtomicInt32(Int32 initialValue)
		{
			Value = initialValue;
		}

		public override Int32 Set(Func<Int32, Int32> mutator)
		{
			for (;;)
			{
				Int32 originalValue = Value;

				Int32 changedValue = mutator(originalValue);

				Int32 previousValue = Interlocked.CompareExchange(ref Value, changedValue, originalValue);

				// if the value returned is equal to the original value, we made the change
				if (previousValue == originalValue)
					return previousValue;
			}
		}
	}
}