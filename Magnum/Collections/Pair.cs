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
	/// Stores a pair of objects within a single struct. This struct is useful to use as the
	/// T of a collection, or as the TKey or TValue of a dictionary.
	/// </summary>
	[Serializable]
	public struct Pair<TFirst, TSecond> : IComparable,
		IComparable<Pair<TFirst, TSecond>>
	{
		/// <summary>
		/// Comparers for the first and second type that are used to compare
		/// values.
		/// </summary>
		private static readonly IComparer<TFirst> firstComparer = Comparer<TFirst>.Default;

		private static readonly IEqualityComparer<TFirst> firstEqualityComparer = EqualityComparer<TFirst>.Default;
		private static readonly IComparer<TSecond> secondComparer = Comparer<TSecond>.Default;
		private static readonly IEqualityComparer<TSecond> secondEqualityComparer = EqualityComparer<TSecond>.Default;

		/// <summary>
		/// The first element of the pair.
		/// </summary>
		public TFirst First;

		/// <summary>
		/// The second element of the pair.
		/// </summary>
		public TSecond Second;

		/// <summary>
		/// Creates a new pair with given first and second elements.
		/// </summary>
		/// <param name="first">The first element of the pair.</param>
		/// <param name="second">The second element of the pair.</param>
		public Pair(TFirst first, TSecond second)
		{
			this.First = first;
			this.Second = second;
		}

		/// <summary>
		/// Creates a new pair using elements from a KeyValuePair structure. The
		/// First element gets the Key, and the Second elements gets the Value.
		/// </summary>
		/// <param name="keyAndValue">The KeyValuePair to initialize the Pair with .</param>
		public Pair(KeyValuePair<TFirst, TSecond> keyAndValue)
		{
			this.First = keyAndValue.Key;
			this.Second = keyAndValue.Value;
		}

		/// <summary>
		/// <para> Compares this pair to another pair of the some type. The pairs are compared by using
		/// the IComparable&lt;T&gt; or IComparable interface on TFirst and TSecond. The pairs
		/// are compared by their first elements first, if their first elements are equal, then they
		/// are compared by their second elements.</para>
		/// <para>If either TFirst or TSecond does not implement IComparable&lt;T&gt; or IComparable, then
		/// an NotSupportedException is thrown, because the pairs cannot be compared.</para>
		/// </summary>
		/// <param name="obj">The pair to compare to.</param>
		/// <returns>An integer indicating how this pair compares to <paramref name="obj"/>. Less
		/// than zero indicates this pair is less than <paramref name="obj"/>. Zero indicate this pair is
		/// equals to <paramref name="obj"/>. Greater than zero indicates this pair is greater than
		/// <paramref name="obj"/>.</returns>
		/// <exception cref="ArgumentException"><paramref name="obj"/> is not of the correct type.</exception>
		/// <exception cref="NotSupportedException">Either FirstSecond or TSecond is not comparable
		/// via the IComparable&lt;T&gt; or IComparable interfaces.</exception>
		int IComparable.CompareTo(object obj)
		{
			if (obj is Pair<TFirst, TSecond>)
				return CompareTo((Pair<TFirst, TSecond>) obj);
			else
				throw new ArgumentException(Strings.BadComparandType, "obj");
		}

		/// <summary>
		/// <para> Compares this pair to another pair of the some type. The pairs are compared by using
		/// the IComparable&lt;T&gt; or IComparable interface on TFirst and TSecond. The pairs
		/// are compared by their first elements first, if their first elements are equal, then they
		/// are compared by their second elements.</para>
		/// <para>If either TFirst or TSecond does not implement IComparable&lt;T&gt; or IComparable, then
		/// an NotSupportedException is thrown, because the pairs cannot be compared.</para>
		/// </summary>
		/// <param name="other">The pair to compare to.</param>
		/// <returns>An integer indicating how this pair compares to <paramref name="other"/>. Less
		/// than zero indicates this pair is less than <paramref name="other"/>. Zero indicate this pair is
		/// equals to <paramref name="other"/>. Greater than zero indicates this pair is greater than
		/// <paramref name="other"/>.</returns>
		/// <exception cref="NotSupportedException">Either FirstSecond or TSecond is not comparable
		/// via the IComparable&lt;T&gt; or IComparable interfaces.</exception>
		public int CompareTo(Pair<TFirst, TSecond> other)
		{
			try
			{
				int firstCompare = firstComparer.Compare(First, other.First);
				if (firstCompare != 0)
					return firstCompare;
				else
					return secondComparer.Compare(Second, other.Second);
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
				else
					throw; // Hmmm. Unclear why we got the ArgumentException. 
			}
		}

		/// <summary>
		/// Determines if this pair is equal to another object. The pair is equal to another object 
		/// if that object is a Pair, both element types are the same, and the first and second elements
		/// both compare equal using object.Equals.
		/// </summary>
		/// <param name="obj">Object to compare for equality.</param>
		/// <returns>True if the objects are equal. False if the objects are not equal.</returns>
		public override bool Equals(object obj)
		{
			if (obj != null && obj is Pair<TFirst, TSecond>)
			{
				Pair<TFirst, TSecond> other = (Pair<TFirst, TSecond>) obj;

				return Equals(other);
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Determines if this pair is equal to another pair. The pair is equal if  the first and second elements
		/// both compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
		/// </summary>
		/// <param name="other">Pair to compare with for equality.</param>
		/// <returns>True if the pairs are equal. False if the pairs are not equal.</returns>
		public bool Equals(Pair<TFirst, TSecond> other)
		{
			return firstEqualityComparer.Equals(First, other.First) && secondEqualityComparer.Equals(Second, other.Second);
		}

		/// <summary>
		/// Returns a hash code for the pair, suitable for use in a hash-table or other hashed collection.
		/// Two pairs that compare equal (using Equals) will have the same hash code. The hash code for
		/// the pair is derived by combining the hash codes for each of the two elements of the pair.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			// Build the hash code from the hash codes of First and Second. 
			int hashFirst = (First == null) ? 0x61E04917 : First.GetHashCode();
			int hashSecond = (Second == null) ? 0x198ED6A3 : Second.GetHashCode();
			return hashFirst ^ hashSecond;
		}

		/// <summary>
		/// Returns a string representation of the pair. The string representation of the pair is
		/// of the form:
		/// <c>First: {0}, Second: {1}</c>
		/// where {0} is the result of First.ToString(), and {1} is the result of Second.ToString() (or
		/// "null" if they are null.)
		/// </summary>
		/// <returns> The string representation of the pair.</returns>
		public override string ToString()
		{
			return string.Format("First: {0}, Second: {1}", (First == null) ? "null" : First.ToString(), (Second == null) ? "null" : Second.ToString());
		}

		/// <summary>
		/// Converts this Pair to a KeyValuePair. The Key part of the KeyValuePair gets
		/// the First element, and the Value part of the KeyValuePair gets the Second 
		/// elements.
		/// </summary>
		/// <returns>The KeyValuePair created from this Pair.</returns>
		public KeyValuePair<TFirst, TSecond> ToKeyValuePair()
		{
			return new KeyValuePair<TFirst, TSecond>(this.First, this.Second);
		}

		/// <summary>
		/// Determines if two pairs are equal. Two pairs are equal if  the first and second elements
		/// both compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
		/// </summary>
		/// <param name="pair1">First pair to compare.</param>
		/// <param name="pair2">Second pair to compare.</param>
		/// <returns>True if the pairs are equal. False if the pairs are not equal.</returns>
		public static bool operator ==(Pair<TFirst, TSecond> pair1, Pair<TFirst, TSecond> pair2)
		{
			return firstEqualityComparer.Equals(pair1.First, pair2.First) && secondEqualityComparer.Equals(pair1.Second, pair2.Second);
		}

		/// <summary>
		/// Determines if two pairs are not equal. Two pairs are equal if  the first and second elements
		/// both compare equal using IComparable&lt;T&gt;.Equals or object.Equals.
		/// </summary>
		/// <param name="pair1">First pair to compare.</param>
		/// <param name="pair2">Second pair to compare.</param>
		/// <returns>True if the pairs are not equal. False if the pairs are equal.</returns>
		public static bool operator !=(Pair<TFirst, TSecond> pair1, Pair<TFirst, TSecond> pair2)
		{
			return !(pair1 == pair2);
		}

		/// <summary>
		/// Converts a Pair to a KeyValuePair. The Key part of the KeyValuePair gets
		/// the First element, and the Value part of the KeyValuePair gets the Second 
		/// elements.
		/// </summary>
		/// <param name="pair">Pair to convert.</param>
		/// <returns>The KeyValuePair created from <paramref name="pair"/>.</returns>
		public static explicit operator KeyValuePair<TFirst, TSecond>(Pair<TFirst, TSecond> pair)
		{
			return new KeyValuePair<TFirst, TSecond>(pair.First, pair.Second);
		}

		/// <summary>
		/// Converts a KeyValuePair structure into a Pair. The
		/// First element gets the Key, and the Second element gets the Value.
		/// </summary>
		/// <param name="keyAndValue">The KeyValuePair to convert.</param>
		/// <returns>The Pair created by converted the KeyValuePair into a Pair.</returns>
		public static explicit operator Pair<TFirst, TSecond>(KeyValuePair<TFirst, TSecond> keyAndValue)
		{
			return new Pair<TFirst, TSecond>(keyAndValue);
		}
	}
}