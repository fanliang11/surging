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


	public class SingleElementImmutableList<T> :
		ImmutableList<T>
	{
		T _head;

		public SingleElementImmutableList(T head)
			: base(1)
		{
			_head = head;
		}

		public SingleElementImmutableList(T head, int tailCount)
			: base(1 + tailCount)
		{
			_head = head;
		}

		public override T Head
		{
			get { return _head; }
		}

		public override ImmutableList<T> Tail
		{
			get { return EmptyList; }
		}

		public override ImmutableList<T> Add(T element)
		{
			return new MultipleElementImmutableList<T>(element, this);
		}

		public override IEnumerator<T> GetEnumerator()
		{
			yield return Head;
		}
	}
}