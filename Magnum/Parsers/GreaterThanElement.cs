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
	using System.Linq.Expressions;

	public class GreaterThanElement :
		IRangeElement
	{
		public GreaterThanElement(string begin)
		{
			Begin = begin;
		}

		public string Begin { get; private set; }

		public bool Includes(IRangeElement element)
		{
			if (element == null)
				return false;

			if (element is StartsWithElement)
				return ((StartsWithElement) element).Start.CompareTo(Begin) >= 0;

			if (element is RangeElement)
				return ((RangeElement) element).Begin.Begin.CompareTo(Begin) >= 0;

			if (element is GreaterThanElement)
				return ((GreaterThanElement) element).Begin.CompareTo(Begin) >= 0;

			if(element is LessThanElement)
				return false;

			return false;
		}

		public Expression<Func<T, bool>> GetQueryExpression<T,V>(Expression<Func<T, V>> memberExpression)
		{
			if (typeof(V) == typeof(string))
			{
				var stringMemberExpression = memberExpression as Expression<Func<T, string>>;

				return stringMemberExpression.ToCompareToExpression(Begin, ExpressionType.GreaterThanOrEqual);
			}

			return memberExpression.ToBinaryExpression(Begin, ExpressionType.GreaterThanOrEqual);
		}

		public override string ToString()
		{
			return string.Format("{0}-", Begin);
		}

		public bool Equals(GreaterThanElement other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Begin, Begin);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (GreaterThanElement)) return false;
			return Equals((GreaterThanElement) obj);
		}

		public override int GetHashCode()
		{
			return (Begin != null ? Begin.GetHashCode() : 0);
		}
	}
}