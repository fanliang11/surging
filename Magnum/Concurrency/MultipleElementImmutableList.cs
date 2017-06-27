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


	public class MultipleElementImmutableList<T> :
		SingleElementImmutableList<T>
	{
		ImmutableList<T> _tail;

		public MultipleElementImmutableList(T head, ImmutableList<T> tail)
			: base(head, tail.Count)
		{
			_tail = tail;
		}

		public override ImmutableList<T> Tail
		{
			get { return _tail; }
		}

		public override IEnumerator<T> GetEnumerator()
		{
			yield return Head;

			foreach (T element in Tail)
				yield return element;
		}
	}
}