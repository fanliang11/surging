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


	public abstract class ImmutableQueue<T> :
		IEnumerable<T>
	{
		static ImmutableQueue<T> _emptyQueue;
		int _count;

		protected ImmutableQueue(int count)
		{
			_count = count;
		}

		public abstract bool IsEmpty { get; }

		public abstract T First { get; }

		public int Count
		{
			get { return _count; }
		}

		public static ImmutableQueue<T> EmptyQueue
		{
			get
			{
				if (_emptyQueue == null)
					_emptyQueue = new EmptyImmutableQueue<T>();

				return _emptyQueue;
			}
		}

		public abstract IEnumerator<T> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public abstract ImmutableQueue<T> AddFirst(T element);

		public abstract ImmutableQueue<T> AddLast(T element);

		public abstract ImmutableQueue<T> RemoveFirst(out T element);
	}
}