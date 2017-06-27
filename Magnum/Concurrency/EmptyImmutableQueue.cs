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
	using System.Collections.Generic;


	public class EmptyImmutableQueue<T> :
		ImmutableQueue<T>
	{
		public EmptyImmutableQueue()
			: base(0)
		{
		}

		public override bool IsEmpty
		{
			get { return true; }
		}

		public override T First
		{
			get { throw new InvalidOperationException("The queue is empty"); }
		}

		public override IEnumerator<T> GetEnumerator()
		{
			yield break;
		}

		public override ImmutableQueue<T> AddFirst(T element)
		{
			return new SingleElementImmutableQueue<T>(element);
		}

		public override ImmutableQueue<T> AddLast(T element)
		{
			return new SingleElementImmutableQueue<T>(element);
		}

		public override ImmutableQueue<T> RemoveFirst(out T element)
		{
			throw new InvalidOperationException("The queue is empty");
		}
	}
}