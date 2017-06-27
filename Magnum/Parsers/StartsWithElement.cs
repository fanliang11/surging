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
	using System.Linq;
	using System.Linq.Expressions;

	public class StartsWithElement :
		IRangeElement
	{
		public StartsWithElement(string start)
		{
			Start = start;
		}

		public string Start { get; private set; }

		public bool Includes(IRangeElement element)
		{
			if (element == null)
				return false;

			if (element is StartsWithElement)
				return Includes((StartsWithElement) element);

			return false;
		}

		public Expression<Func<T, bool>> GetQueryExpression<T,V>(Expression<Func<T, V>> memberExpression)
		{
			if (typeof(V) == typeof(string))
			{
				var stringMemberExpression = memberExpression as Expression<Func<T, string>>;

				return stringMemberExpression.ToStartsWithExpression(Start);
			}

			return memberExpression.ToBinaryExpression(Start, ExpressionType.Equal);
		}

		public override string ToString()
		{
			return string.Format("{0}", Start);
		}

		public bool Equals(StartsWithElement other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Start, Start);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (StartsWithElement)) return false;
			return Equals((StartsWithElement) obj);
		}

		public override int GetHashCode()
		{
			return (Start != null ? Start.GetHashCode() : 0);
		}

		public IQueryable<T> Where<T>(IQueryable<T> elements, Expression<Func<T, string>> memberExpression)
		{
			Expression<Func<T, bool>> expression = memberExpression.ToStartsWithExpression(Start);

			return elements.Where(expression);
		}

		private bool Includes(StartsWithElement element)
		{
			if (Start.Length > element.Start.Length)
				return false;

			if (element.Start.Substring(0, Start.Length) == Start)
				return true;

			return false;
		}
	}
}