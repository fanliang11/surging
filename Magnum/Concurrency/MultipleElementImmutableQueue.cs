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
	using System.Linq;


	public class MultipleElementImmutableQueue<T> :
		ImmutableQueue<T>
	{
		readonly ImmutableList<T> _input;
		readonly ImmutableList<T> _output;

		public MultipleElementImmutableQueue(ImmutableList<T> output, ImmutableList<T> input)
			: base(output.Count + input.Count)
		{
			_output = output;
			_input = input;
		}

		public override bool IsEmpty
		{
			get { return false; }
		}

		public override T First
		{
			get { return _output.Head; }
		}

		public override IEnumerator<T> GetEnumerator()
		{
			IEnumerable<T> input = _input;
			IEnumerable<T> output = _output;

			return output.Concat(input.Reverse()).GetEnumerator();
		}

		public override ImmutableQueue<T> AddFirst(T element)
		{
			return new MultipleElementImmutableQueue<T>(_output.Add(element), _input);
		}

		public override ImmutableQueue<T> AddLast(T element)
		{
			return new MultipleElementImmutableQueue<T>(_output, _input.Add(element));
		}

		public override ImmutableQueue<T> RemoveFirst(out T element)
		{
			element = _output.Head;

			if (Count == 2)
				return new SingleElementImmutableQueue<T>(_output.Tail.Head);

			if (_output.Count > 2)
				return new MultipleElementImmutableQueue<T>(_output.Tail, _input);

			return new MultipleElementImmutableQueue<T>(_input.Reverse().Add(_output.Tail.Head), ImmutableList<T>.EmptyList);
		}
	}
}