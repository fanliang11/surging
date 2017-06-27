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

	public static class LinqMergeExtensions
	{
		public static IEnumerable<TResult> Merge<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
		{
			Guard.AgainstNull(first, "first");
			Guard.AgainstNull(second, "second");
			Guard.AgainstNull(resultSelector, "resultSelector");

			using (var e1 = first.GetEnumerator())
			using (var e2 = second.GetEnumerator())
				while (e1.MoveNext())
				{
					if (!e2.MoveNext())
						yield break;

					yield return resultSelector(e1.Current, e2.Current);
				}
		}


		public static IEnumerable<TResult> MergeBalanced<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
		{
			Guard.AgainstNull(first, "first");
			Guard.AgainstNull(second, "second");
			Guard.AgainstNull(resultSelector, "resultSelector");

			using (var e1 = first.GetEnumerator())
			using (var e2 = second.GetEnumerator())
			{
				while (e1.MoveNext())
				{
					if (!e2.MoveNext())
						throw new InvalidOperationException("Second sequence ran out before first");

					yield return resultSelector(e1.Current, e2.Current);
				}
				if (e2.MoveNext())
					throw new InvalidOperationException("First sequence ran out before second");
			}
		}

		public static IEnumerable<TResult> MergePadded<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
		{
			Guard.AgainstNull(first, "first");
			Guard.AgainstNull(second, "second");
			Guard.AgainstNull(resultSelector, "resultSelector");

			using (var e1 = first.GetEnumerator())
			using (var e2 = second.GetEnumerator())
			{
				while (e1.MoveNext())
				{
					if (e2.MoveNext())
					{
						yield return resultSelector(e1.Current, e2.Current);
					}
					else
					{
						do
						{
							yield return resultSelector(e1.Current, default(TSecond));
						} while (e1.MoveNext());
						yield break;
					}
				}
				if (e2.MoveNext())
				{
					do
					{
						yield return resultSelector(default(TFirst), e2.Current);
					} while (e2.MoveNext());
					yield break;
				}
			}
		}
	}
}