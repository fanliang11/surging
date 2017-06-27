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
namespace Magnum.Parsers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;

	public static class ExtensionsToRangeParser
	{
		private static readonly MethodInfo _startsWith;
		private static readonly MethodInfo _compareTo;

		static ExtensionsToRangeParser()
		{
			_startsWith = typeof (string)
				.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.Name == "StartsWith")
				.Where(x => x.GetParameters().Count() == 1)
				.Where(x => x.GetParameters().Where(p => p.ParameterType == typeof (string)).Count() == 1)
				.Single();

			_compareTo = typeof (string)
				.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.Name == "CompareTo")
				.Where(x => x.GetParameters().Where(p => p.ParameterType == typeof (string)).Count() == 1)
				.Single();
		}

		public static string ToRangeString(this IEnumerable<IRangeElement> elements)
		{
			return string.Join(";", elements.Select(x => x.ToString()).ToArray());
		}

		public static bool Includes(this IEnumerable<IRangeElement> elements, IRangeElement find)
		{
			foreach (IRangeElement element in elements)
			{
				if (element.Includes(find) && !ReferenceEquals(element, find))
					return true;
			}

			return false;
		}

		public static IEnumerable<IRangeElement> CombineOverlappingRanges(this IEnumerable<IRangeElement> elements)
		{
			var ranges = new List<RangeElement>();
			var openRanges = new List<IRangeElement>();

			foreach (IRangeElement element in elements)
			{
				var range = element as RangeElement;
				if (range != null)
				{
					ranges.Add(range);
					continue;
				}

				if (element is GreaterThanElement || element is LessThanElement)
				{
					openRanges.Add(element);
					continue;
				}

				yield return element;
			}

			for (int i = 0; i < ranges.Count; i++)
			{
				for (int j = i + 1; j < ranges.Count;)
				{
					RangeElement combined;
					if (ranges[i].Union(ranges[j], out combined))
					{
						ranges[i] = combined;
						ranges.Remove(ranges[j]);
						continue;
					}

					j++;
				}

				yield return ranges[i];
			}

			for (int i = 0; i < openRanges.Count;)
			{
				bool removed = false;
				for (int j = i + 1; j < openRanges.Count; j++)
				{
					if(openRanges[i].Overlaps(openRanges[j]))
					{
						openRanges.Remove(openRanges[j]);
						openRanges.Remove(openRanges[i]);
						removed = true;
						break;
					}
				}

				if(removed)
					continue;

				i++;
			}

			foreach (IRangeElement element in openRanges)
				yield return element;
		}

		public static bool Overlaps(this IRangeElement element, IRangeElement other)
		{
			if (element is LessThanElement && other is GreaterThanElement)
			{
				return (((LessThanElement) element).End.CompareTo(((GreaterThanElement) other).Begin) >= 0);
			}

			if (element is GreaterThanElement && other is LessThanElement)
			{
				return (((LessThanElement)other).End.CompareTo(((GreaterThanElement)element).Begin) >= 0);
			}

			return false;
		}

		public static IEnumerable<IRangeElement> RestrictTo(this IEnumerable<IRangeElement> requested, IEnumerable<IRangeElement> restriction)
		{
			int requestedCount = 0;
			foreach (IRangeElement element in requested)
			{
				requestedCount++;

				if (!restriction.Includes(element))
				{
					var range = element as RangeElement;
					if (range != null)
					{
						RangeElement intersection = null;
						restriction
							.Where(x => x is RangeElement)
							.Where(x => ((RangeElement) x).Intersection(range, out intersection))
							.FirstOrDefault();

						if(intersection != null)
						{
							yield return intersection;
							continue;
						}

						var includedResults = restriction
							.Where(x => x is StartsWithElement)
							.Where(range.Includes);

						foreach (IRangeElement includedResult in includedResults)
							yield return includedResult;
					}

					continue;
				}

				yield return element;
			}

			if(requestedCount==0)
				foreach (IRangeElement element in restriction)
					yield return element;
		}

		public static IEnumerable<IRangeElement> Optimize(this IEnumerable<IRangeElement> elements)
		{
			var results = new List<IRangeElement>();

			foreach (IRangeElement element in elements)
			{
				if (results.Contains(element))
					continue;

				if (results.Includes(element))
					continue;

				results.Add(element);
			}

			foreach (IRangeElement result in results)
			{
				if (!results.Includes(result))
					yield return result;
			}
		}

		public static Expression<Func<T, bool>> ToQueryExpression<T, V>(this IEnumerable<IRangeElement> elements, Expression<Func<T, V>> memberExpression)
		{
			Expression<Func<T, bool>> result = null;

			foreach (IRangeElement element in elements)
			{
				Expression<Func<T, bool>> expression = element.GetQueryExpression(memberExpression);

				if (result == null)
					result = expression;
				else
				{
					var binary = Expression.MakeBinary(ExpressionType.OrElse, result.Body, expression.Body);

					result = Expression.Lambda<Func<T, bool>>(binary, new[] {result.Parameters[0]});
				}
			}

			return result ?? (x => true);
		}

		public static IQueryable<T> WhereInRange<T, V>(this IQueryable<T> elements, Expression<Func<T, V>> memberExpression, IEnumerable<IRangeElement> rangeElements)
		{
			return elements.Where(rangeElements.ToQueryExpression(memberExpression));
		}

		public static IEnumerable<T> WhereInRange<T, V>(this IEnumerable<T> elements, Expression<Func<T, V>> memberExpression, IEnumerable<IRangeElement> rangeElements)
		{
			return elements.Where(rangeElements.ToQueryExpression(memberExpression).Compile());
		}

		internal static Expression<Func<T, bool>> ToCompareToExpression<T>(this Expression<Func<T, string>> memberExpression, string value, ExpressionType comparisonType)
		{
			var member = memberExpression.Body as MemberExpression;
			if (member == null)
				throw new InvalidOperationException("Only member expressions are allowed");

			var argument = Expression.Constant(value);
			var zero = Expression.Constant(0);

			var call = Expression.Call(member, _compareTo, new[] {argument});

			var compare = Expression.MakeBinary(comparisonType, call, zero);

			return Expression.Lambda<Func<T, bool>>(compare, new[] {memberExpression.Parameters[0]});
		}

		internal static Expression<Func<T, bool>> ToStartsWithExpression<T>(this Expression<Func<T, string>> memberExpression, string value)
		{
			var member = memberExpression.Body as MemberExpression;
			if (member == null)
				throw new InvalidOperationException("Only member expressions are allowed");

			var argument = Expression.Constant(value);

			var call = Expression.Call(member, _startsWith, new[] {argument});

			return Expression.Lambda<Func<T, bool>>(call, new[] {memberExpression.Parameters[0]});
		}

		internal static Expression<Func<T, bool>> ToBinaryExpression<T, V>(this Expression<Func<T, V>> memberExpression, string value, ExpressionType comparisonType)
		{
			var member = memberExpression.Body as MemberExpression;
			if (member == null)
				throw new InvalidOperationException("Only member expressions are allowed");

			var argument = Expression.Constant(value.ConvertTo<V>(), typeof (V));

			var compare = Expression.MakeBinary(comparisonType, member, argument);

			return Expression.Lambda<Func<T, bool>>(compare, new[] {memberExpression.Parameters[0]});
		}

		internal static object ConvertTo<T>(this string value)
		{
			if (typeof (T) == typeof (int))
				return int.Parse(value);

			throw new InvalidOperationException("The type " + typeof (T).Name + " is not supported");
		}
	}
}