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
namespace Magnum
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public class RangeEnumerator<T> :
		IEnumerable<T>
	{
		readonly bool _ascending;
		readonly Range<T> _range;
		readonly Func<T, T> _step;

		public RangeEnumerator(Range<T> range, Func<T, T> step)
		{
			_range = range;
			_step = step;

			_ascending = range.Comparer.Compare(range.LowerBound, step(range.LowerBound)) < 0;
		}

		public IEnumerator<T> GetEnumerator()
		{
			T first = _ascending ? _range.LowerBound : _range.UpperBound;
			T last = _ascending ? _range.UpperBound : _range.LowerBound;

			T value = first;

			IComparer<T> comparer = _range.Comparer;

			if (_range.IncludeLowerBound)
			{
				if (_range.IncludeUpperBound || comparer.Compare(value, last) < 0)
					yield return value;
			}

			value = _step(value);

			while (comparer.Compare(value, last) < 0)
			{
				yield return value;
				value = _step(value);
			}

			if (_range.IncludeUpperBound && comparer.Compare(value, last) == 0)
				yield return value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}