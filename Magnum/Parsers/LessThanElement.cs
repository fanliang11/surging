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
	using System.ComponentModel;
	using System.Linq.Expressions;

	public class LessThanElement :
		IRangeElement
	{
		public LessThanElement(string end)
		{
			End = end;
		}

		public string End { get; private set; }

		public bool Includes(IRangeElement element)
		{
			if (element == null)
				return false;

			if (element is StartsWithElement)
				return ((StartsWithElement) element).Start.CompareTo(End) <= 0;

			if (element is RangeElement)
				return ((RangeElement) element).End.End.CompareTo(End) <= 0;

			if (element is LessThanElement)
				return ((LessThanElement) element).End.CompareTo(End) <= 0;

			if(element is GreaterThanElement)
				return false;

			return false;
		}

		public Expression<Func<T, bool>> GetQueryExpression<T,V>(Expression<Func<T, V>> memberExpression)
		{
			if (typeof(V) == typeof(string))
			{
				var stringMemberExpression = memberExpression as Expression<Func<T, string>>;

				return stringMemberExpression.ToCompareToExpression(GetEndForQuery(), ExpressionType.LessThan);
			}

			return memberExpression.ToBinaryExpression(End, ExpressionType.LessThanOrEqual);
		}

		public override string ToString()
		{
			return string.Format("-{0}", End);
		}

		public bool Equals(LessThanElement other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.End, End);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (LessThanElement)) return false;
			return Equals((LessThanElement) obj);
		}

		public override int GetHashCode()
		{
			return (End != null ? End.GetHashCode() : 0);
		}

		private string GetEndForQuery()
		{
			return End + new string('z', 64);
		}
	}
}