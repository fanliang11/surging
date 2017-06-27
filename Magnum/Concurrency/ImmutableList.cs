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
	using System.Collections;
	using System.Collections.Generic;


	public abstract class ImmutableList<T> :
		IEnumerable<T>
	{
		static ImmutableList<T> _emptyList;
		readonly int _count;

		public ImmutableList(int count)
		{
			_count = count;
		}

		public int Count
		{
			get { return _count; }
		}

		public bool IsEmpty
		{
			get { return _count == 0; }
		}

		public static ImmutableList<T> EmptyList
		{
			get
			{
				if (_emptyList == null)
					_emptyList = new EmptyImmutableList<T>();

				return _emptyList;
			}
		}

		public abstract T Head { get; }
		public abstract ImmutableList<T> Tail { get; }

		public abstract IEnumerator<T> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public abstract ImmutableList<T> Add(T element);

		public ImmutableList<T> AddMany(IEnumerable<T> elements)
		{
			ImmutableList<T> result = this;
			foreach (T element in elements)
				result = result.Add(element);

			return result;
		}

		public ImmutableList<T> Remove(T element)
		{
			return Remove(element, EmptyList);
		}

		public ImmutableList<T> Remove(T element, ImmutableList<T> accumulated)
		{
			if (_count == 0)
				return accumulated.Reverse();

			if (Head.Equals(element))
				return Tail.AddMany(accumulated);

			return Tail.Remove(element, accumulated.Add(Head));
		}

		public ImmutableList<T> Reverse()
		{
			return Reverse(EmptyList);
		}

		public ImmutableList<T> Reverse(ImmutableList<T> accumulated)
		{
			if (_count == 0)
				return accumulated;

			return Tail.Reverse(accumulated.Add(Head));
		}

		public bool Contains(T element)
		{
			return _count > 0 && (Head.Equals(element) || Tail.Contains(element));
		}
	}
}