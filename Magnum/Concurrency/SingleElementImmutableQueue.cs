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
	using System.Collections.Generic;


	public class SingleElementImmutableQueue<T> :
		ImmutableQueue<T>
	{
		readonly T _head;

		public SingleElementImmutableQueue(T head)
			: base(1)
		{
			_head = head;
		}

		public override bool IsEmpty
		{
			get { return false; }
		}

		public override T First
		{
			get { return _head; }
		}

		public override IEnumerator<T> GetEnumerator()
		{
			yield return _head;
		}

		public override ImmutableQueue<T> AddFirst(T element)
		{
			return new MultipleElementImmutableQueue<T>(ImmutableList<T>.EmptyList.Add(_head).Add(element),
			                                            ImmutableList<T>.EmptyList);
		}

		public override ImmutableQueue<T> AddLast(T element)
		{
			return new MultipleElementImmutableQueue<T>(ImmutableList<T>.EmptyList.Add(element).Add(_head),
			                                            ImmutableList<T>.EmptyList);
		}

		public override ImmutableQueue<T> RemoveFirst(out T element)
		{
			element = _head;

			return EmptyQueue;
		}
	}
}