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
namespace Magnum.Collections
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public struct Tuple<TFirst, TSecond> : IComparable,
		IComparable<Tuple<TFirst, TSecond>>
	{
		private static readonly IComparer<TFirst> firstComparer = Comparer<TFirst>.Default;
		private static readonly IComparer<TSecond> secondComparer = Comparer<TSecond>.Default;

		private static readonly IEqualityComparer<TFirst> firstEqualityComparer = EqualityComparer<TFirst>.Default;
		private static readonly IEqualityComparer<TSecond> secondEqualityComparer = EqualityComparer<TSecond>.Default;

		public TFirst First;
		public TSecond Second;

		public Tuple(TFirst first, TSecond second)
		{
			First = first;
			Second = second;
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj is Tuple<TFirst, TSecond>)
				return CompareTo((Tuple<TFirst, TSecond>) obj);
			else
				throw new ArgumentException(Strings.BadComparandType, "obj");
		}

		public int CompareTo(Tuple<TFirst, TSecond> other)
		{
			try
			{
				int firstCompare = firstComparer.Compare(First, other.First);
				
				return firstCompare != 0 ? firstCompare : secondComparer.Compare(Second, other.Second);
			}
			catch (ArgumentException)
			{
				if (!typeof (IComparable<TFirst>).IsAssignableFrom(typeof (TFirst)) &&
				    !typeof (IComparable).IsAssignableFrom(typeof (TFirst)))
				{
					throw new NotSupportedException(string.Format(Strings.UncomparableType, typeof (TFirst).FullName));
				}

				if (!typeof (IComparable<TSecond>).IsAssignableFrom(typeof (TSecond)) &&
				    !typeof (IComparable).IsAssignableFrom(typeof (TSecond)))
				{
					throw new NotSupportedException(string.Format(Strings.UncomparableType, typeof (TSecond).FullName));
				}

				throw;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is Tuple<TFirst, TSecond>)
			{
				Tuple<TFirst, TSecond> other = (Tuple<TFirst, TSecond>) obj;

				return Equals(other);
			}

			return false;
		}

		public bool Equals(Tuple<TFirst, TSecond> other)
		{
			return firstEqualityComparer.Equals(First, other.First) &&
			       secondEqualityComparer.Equals(Second, other.Second);
		}

		public override int GetHashCode()
		{
			// Build the hash code from the hash codes of First and Second. 
			int hashFirst = (First == null) ? 0x61E04917 : First.GetHashCode();
			int hashSecond = (Second == null) ? 0x198ED6A3 : Second.GetHashCode();
			return hashFirst ^ hashSecond;
		}

		public override string ToString()
		{
			return string.Format("First: {0}, Second: {1}",
				(First == null) ? "null" : First.ToString(),
				(Second == null) ? "null" : Second.ToString());
		}

		public static bool operator ==(Tuple<TFirst, TSecond> pair1, Tuple<TFirst, TSecond> pair2)
		{
			return pair1.Equals(pair2);
		}

		public static bool operator !=(Tuple<TFirst, TSecond> pair1, Tuple<TFirst, TSecond> pair2)
		{
			return ! pair1.Equals(pair2);
		}
	}
}