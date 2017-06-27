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

	public static class LinqSingleExtensions
	{
		public static T SingleOr<T>(this IEnumerable<T> source, Func<T> onMissing)
		{
			Guard.AgainstNull(source, "source");
			Guard.AgainstNull(onMissing, "onMissing");

			using (IEnumerator<T> iterator = source.GetEnumerator())
			{
				if (!iterator.MoveNext())
					return onMissing();

				T first = iterator.Current;

				if (iterator.MoveNext())
					throw new InvalidOperationException("Sequence contains more than one element");

				return first;
			}
		}
	}
}