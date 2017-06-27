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

	/// <summary>
	/// Stores a triple of objects within a single struct. This struct is useful to use as the
	/// T of a collection, or as the TKey or TValue of a dictionary.
	/// </summary>
	[Serializable]
	public struct Triple<TFirst, TSecond, TThird> : IComparable,
		IComparable<Triple<TFirst, TSecond, TThird>>
	{
		/// <summary>
		/// Comparers for the first and second type that are used to compare
		/// values.
		/// </summary>
		private static readonly IComparer<TFirst> firstComparer = Comparer<TFirst>.Default;

		private static readonly IEqualityComparer<TFirst> firstEqualityComparer = EqualityComparer<TFirst>.Default;
		private static readonly IComparer<TSecond> secondComparer = Comparer<TSecond>.Default;
		private static readonly IEqualityComparer<TSecond> secondEqualityComparer = EqualityComparer<TSecond>.Default;
		private static readonly IComparer<TThird> thirdComparer = Comparer<TThird>.Default;
		private static readonly IEqualityComparer<TThird> thirdEqualityComparer = EqualityComparer<TThird>.Default;

		/// <summary>
		/// The first element of the triple.
		/// </summary>
		public TFirst First;

		/// <summary>
		/// The second element of the triple.
		/// </summary>
		public TSecond Second;

		/// <summary>
		/// The thrid element of the triple.
		/// </summary>
		public TThird Third;

		/// <summary>
		/// Creates a new triple with given elements.
		/// </summary>
		/// <param name="first">The first element of the triple.</param>
		/// <param name="second">The second element of the triple.</param>
		/// <param name="third">The third element of the triple.</param>
		public Triple(TFirst first, TSecond second, TThird third)
		{
			this.First = first;
			this.Second = second;
			this.Third = third;
		}

		/// <summary>
		/// <para> Compares this triple to another triple of the some type. The triples are compared by using
		/// the IComparable&lt;T&gt; or IComparable interface on TFirst, TSecond, and TThird. The triples
		/// are compared by their first elements first, if their first elements are equal, then they
		/// are compared by their second elements. If their second elements are also equal, then they
		/// are compared by their third elements.</para>
		/// <para>If TFirst, TSecond, or TThird does not implement IComparable&lt;T&gt; or IComparable, then
		/// an NotSupportedException is thrown, because the triples cannot be compared.</para>
		/// </summary>
		/// <param name="obj">The triple to compare to.</param>
		/// <returns>An integer indicating how this triple compares to <paramref name="obj"/>. Less
		/// than zero indicates this triple is less than <paramref name="obj"/>. Zero indicate this triple is
		/// equals to <paramref name="obj"/>. Greater than zero indicates this triple is greater than
		/// <paramref name="obj"/>.</returns>
		/// <exception cref="ArgumentException"><paramref name="obj"/> is not of the correct type.</exception>
		/// <exception cref="NotSupportedException">Either FirstSecond, TSecond, or TThird is not comparable
		/// via the IComparable&lt;T&gt; or IComparable interfaces.</exception>
		int IComparable.CompareTo(object obj)
		{
			if (obj is Triple<TFirst, TSecond, TThird>)
				return CompareTo((Triple<TFirst, TSecond, TThird>) obj);
			else
				throw new ArgumentException(Strings.BadComparandType, "obj");
		}

		/// <summary>
		/// <para> Compares this triple to another triple of the some type. The triples are compared by using
		/// the IComparable&lt;T&gt; or IComparable interface on TFirst, TSecond, and TThird. The triples
		/// are compared by their first elements first, if their first elements are equal, then they
		/// are compared by their second elements. If their second elements are also equal, then they
		/// are compared by their third elements.</para>
		/// <para>If TFirst, TSecond, or TThird does not implement IComparable&lt;T&gt; or IComparable, then
		/// an NotSupportedException is thrown, because the triples cannot be compared.</para>
		/// </summary>
		/// <param name="other">The triple to compare to.</param>
		/// <returns>An integer indicating how this triple compares to <paramref name="other"/>. Less
		/// than zero indicates this triple is less than <paramref name="other"/>. Zero indicate this triple is
		/// equals to <paramref name="other"/>. Greater than zero indicates this triple is greater than
		/// <paramref name="other"/>.</returns>
		/// <exception cref="NotSupportedException">Either FirstSecond, TSecond, or TThird is not comparable
		/// via the IComparable&lt;T&gt; or IComparable interfaces.</exception>
		public int CompareTo(Triple<TFirst, TSecond, TThird> other)
		{
			try
			{
				int firstCompare = firstComparer.Compare(First, other.First);
				if (firstCompare != 0)
					return firstCompare;
				int secondCompare = secondComparer.Compare(Second, other.Second);
				if (secondCompare != 0)
					return secondCompare;
				else
					return thirdComparer.Compare(Third, other.Third);
			}
			catch (ArgumentException)
			{
				// Determine which type caused the problem for a better error message.
				if (!typeof (IComparable<TFirst>).IsAssignableFrom(typeof (TFirst)) &&
				    !typeof (IComparable).IsAssignableFrom(typeof (TFirst)))
				{
					throw new NotSupportedException(string.Format(Strings.UncomparableType, typeof (TFirst).FullName));
				}
				else if (!typeof (IComparable<TSecond>).IsAssignableFrom(typeof (TSecond)) &&
				         !typeof (IComparable).IsAssignableFrom(typeof (TSecond)))
				{
					throw new NotSupportedException(string.Format(Strings.UncomparableType, typeof (TSecond).FullName));
				}
				else if (!typeof (IComparable<TThird>).IsAssignableFrom(typeof (TThird)) &&
				         !typeof (IComparable).IsAssignableFrom(typeof (TThird)))
				{
					throw new NotSupportedException(string.Format(Strings.UncomparableType, typeof (TThird).FullName));
				}
				else
					throw; // Hmmm. Unclear why we got the ArgumentException. 
			}
		}

		/// <summary>
		/// Determines if this triple is equal to another object. The triple is equal to another object 
		/// if that object is a Triple, all element types are the same, and the all three elements
		/// compare equal using object.Equals.
		/// </summary>
		/// <param name="obj">Object to compare for equality.</param>
		/// <returns>True if the objects are equal. False if the objects are not equal.</returns>
		public override bool Equals(object obj)
		{
			if (obj != null && obj is Triple<TFirst, TSecond, TThird>)
			{
				Triple<TFirst, TSecond, TThird> other = (Triple<TFirst, TSecond, TThird>) obj;

				return Equals(other);
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Determines if this triple is equal to another triple. Two triples are equal if the all three elements
		/// compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
		/// </summary>
		/// <param name="other">Triple to compare with for equality.</param>
		/// <returns>True if the triples are equal. False if the triples are not equal.</returns>
		public bool Equals(Triple<TFirst, TSecond, TThird> other)
		{
			return firstEqualityComparer.Equals(First, other.First) &&
			       secondEqualityComparer.Equals(Second, other.Second) &&
			       thirdEqualityComparer.Equals(Third, other.Third);
		}

		/// <summary>
		/// Returns a hash code for the triple, suitable for use in a hash-table or other hashed collection.
		/// Two triples that compare equal (using Equals) will have the same hash code. The hash code for
		/// the triple is derived by combining the hash codes for each of the two elements of the triple.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			// Build the hash code from the hash codes of First and Second. 
			int hashFirst = (First == null) ? 0x61E04917 : First.GetHashCode();
			int hashSecond = (Second == null) ? 0x198ED6A3 : Second.GetHashCode();
			int hashThird = (Third == null) ? 0x40FC1877 : Third.GetHashCode();
			return hashFirst ^ hashSecond ^ hashThird;
		}

		/// <summary>
		/// Returns a string representation of the triple. The string representation of the triple is
		/// of the form:
		/// <c>First: {0}, Second: {1}, Third: {2}</c>
		/// where {0} is the result of First.ToString(), {1} is the result of Second.ToString(), and
		/// {2} is the result of Third.ToString() (or "null" if they are null.)
		/// </summary>
		/// <returns> The string representation of the triple.</returns>
		public override string ToString()
		{
			return string.Format("First: {0}, Second: {1}, Third: {2}",
				(First == null) ? "null" : First.ToString(),
				(Second == null) ? "null" : Second.ToString(),
				(Third == null) ? "null" : Third.ToString());
		}

		/// <summary>
		/// Determines if two triples are equal. Two triples are equal if the all three elements
		/// compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
		/// </summary>
		/// <param name="pair1">First triple to compare.</param>
		/// <param name="pair2">Second triple to compare.</param>
		/// <returns>True if the triples are equal. False if the triples are not equal.</returns>
		public static bool operator ==(Triple<TFirst, TSecond, TThird> pair1, Triple<TFirst, TSecond, TThird> pair2)
		{
			return pair1.Equals(pair2);
		}

		/// <summary>
		/// Determines if two triples are not equal. Two triples are equal if the all three elements
		/// compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
		/// </summary>
		/// <param name="pair1">First triple to compare.</param>
		/// <param name="pair2">Second triple to compare.</param>
		/// <returns>True if the triples are not equal. False if the triples are equal.</returns>
		public static bool operator !=(Triple<TFirst, TSecond, TThird> pair1, Triple<TFirst, TSecond, TThird> pair2)
		{
			return ! pair1.Equals(pair2);
		}
	}
}