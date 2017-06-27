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

	public static class LinqIndexExtensions
	{
		public static IEnumerable<TResult> Index<TFirst, TResult>(this IEnumerable<TFirst> first, Func<TFirst, int, TResult> resultSelector)
		{
			return first.Merge(Index(), resultSelector);
		}

		public static IEnumerable<TResult> Index<TFirst, TResult>(this IEnumerable<TFirst> first, int start, Func<TFirst, int, TResult> resultSelector)
		{
			return first.Merge(Index(start), resultSelector);
		}

		public static IEnumerable<int> Index()
		{
			return GenerateByIndexImpl(0, x => x);
		}

		public static IEnumerable<int> Index(int start)
		{
			return GenerateByIndexImpl(start, x => x);
		}

		public static IEnumerable<TResult> Index<TResult>(Func<int, TResult> selector)
		{
			Guard.AgainstNull(selector, "selector");

			return GenerateByIndexImpl(0, selector);
		}

		public static IEnumerable<TResult> Index<TResult>(int start, Func<int, TResult> selector)
		{
			Guard.AgainstNull(selector, "selector");

			return GenerateByIndexImpl(start, selector);
		}

		private static IEnumerable<TResult> GenerateByIndexImpl<TResult>(int start, Func<int, TResult> selector)
		{
			for (int i = start; i < int.MaxValue; i++)
			{
				yield return selector(i);
			}

			yield return selector(int.MaxValue);
		}
	}
}