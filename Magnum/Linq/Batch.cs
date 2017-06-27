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
namespace Magnum.Linq
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Monads;

	public static class LinqBatchExtensions
	{
		public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
		{
			return Batch(source, size, x => x);
		}

		public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source, int size, Func<IEnumerable<TSource>, TResult> resultSelector)
		{
			Guard.AgainstNull(source, "source");
			Guard.GreaterThan(0, size, "size");
			Guard.AgainstNull(resultSelector, "resultSelector");

			return BatchImpl(source, size, resultSelector);
		}

		private static IEnumerable<TResult> BatchImpl<TSource, TResult>(this IEnumerable<TSource> source, int size, Func<IEnumerable<TSource>, TResult> resultSelector)
		{
			Debug.Assert(source != null);
			Debug.Assert(size > 0);
			Debug.Assert(resultSelector != null);

			TSource[] items = null;
			int count = 0;

			foreach (var item in source)
			{
				if (items == null)
					items = new TSource[size];

				items[count++] = item;

				if (count != size)
					continue;

				yield return resultSelector(items);

				items = null;
				count = 0;
			}

			if (items != null && count > 0)
				yield return resultSelector(items.Take(count));
		}
	}
}