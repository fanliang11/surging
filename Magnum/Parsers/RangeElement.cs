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

	public class RangeElement :
		IRangeElement
	{
		public RangeElement(string begin, string end)
		{
			if (begin.CompareTo(end) > 0)
			{
				string temp = begin;
				begin = end;
				end = temp;
			}

			Begin = new GreaterThanElement(begin);
			End = new LessThanElement(end);
		}

		public GreaterThanElement Begin { get; private set; }
		public LessThanElement End { get; private set; }

		public bool Includes(IRangeElement element)
		{
			if (element == null)
				return false;

			if (Begin.Includes(element) && End.Includes(element))
				return true;

			return false;
		}

		public Expression<Func<T, bool>> GetQueryExpression<T, V>(Expression<Func<T, V>> memberExpression)
		{
			Expression<Func<T, bool>> begin = Begin.GetQueryExpression(memberExpression);
			Expression<Func<T, bool>> end = End.GetQueryExpression(memberExpression);

			BinaryExpression range = Expression.MakeBinary(ExpressionType.AndAlso, begin.Body, end.Body);

			return Expression.Lambda<Func<T, bool>>(range, new[] {begin.Parameters[0]});
		}

		public override string ToString()
		{
			return string.Format("{0}-{1}", Begin.ToString().Trim('-'), End.ToString().Trim('-'));
		}

		public bool Intersection(RangeElement other, out RangeElement intersection)
		{
			intersection = null;

			if (other == null)
				return false;

			// before
			if (Begin.Begin.CompareTo(other.End.End) > 0)
				return false;

			// after
			if (End.End.CompareTo(other.Begin.Begin) < 0)
				return false;

			string begin = (Begin.Begin.CompareTo(other.Begin.Begin) < 0) ? other.Begin.Begin : Begin.Begin;
			string end = (End.End.CompareTo(other.End.End) > 0) ? other.End.End : End.End;
			intersection = new RangeElement(begin, end);
			return true;
		}

		public bool Union(RangeElement other, out RangeElement combined)
		{
			combined = null;

			if (other == null)
				return false;

			if (Begin.Begin.CompareTo(other.Begin.Begin) <= 0 && End.End.CompareTo(other.Begin.Begin) >= 0)
			{
				combined = new RangeElement(Begin.Begin, (End.End.CompareTo(other.End.End) >= 0) ? End.End : other.End.End);
				return true;
			}

			if (End.End.CompareTo(other.End.End) >= 0 && Begin.Begin.CompareTo(other.End.End) <= 0)
			{
				combined = new RangeElement((Begin.Begin.CompareTo(other.Begin.Begin) <= 0) ? Begin.Begin : other.Begin.Begin, End.End);
				return true;
			}

			return false;
		}

		public bool Equals(RangeElement other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Begin, Begin) && Equals(other.End, End);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (RangeElement)) return false;
			return Equals((RangeElement) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Begin != null ? Begin.GetHashCode() : 0)*397) ^ (End != null ? End.GetHashCode() : 0);
			}
		}
	}
}