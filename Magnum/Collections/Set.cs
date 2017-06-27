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
	/// Set&lt;T&gt; is a collection that contains items of type T. 
	/// The item are maintained in a haphazard, unpredictable order, and duplicate items are not allowed.
	/// </summary>
	/// <remarks>
	/// <p>The items are compared in one of two ways. If T implements IComparable&lt;T&gt; 
	/// then the Equals method of that interface will be used to compare items, otherwise the Equals
	/// method from Object will be used. Alternatively, an instance of IComparer&lt;T&gt; can be passed
	/// to the constructor to use to compare items.</p>
	/// <p>Set is implemented as a hash table. Inserting, deleting, and looking up an
	/// an element all are done in approximately constant time, regardless of the number of items in the Set.</p>
	/// <p><see cref="OrderedSet&lt;T&gt;"/> is similar, but uses comparison instead of hashing, and does maintains
	/// the items in sorted order.</p>
	///</remarks>
	///<seealso cref="OrderedSet&lt;T&gt;"/>
	[Serializable]
	public class Set<T> : CollectionBase<T>,
		ICollection<T>,
		ICloneable
	{
		// The comparer used to hash/compare items. 
		private readonly IEqualityComparer<T> equalityComparer;

		// The hash table that actually does the work of storing the items.
		private Hash<T> hash;

		#region Constructors

		/// <summary>
		/// Creates a new Set. The Equals method and GetHashCode method on T
		/// will be used to compare items for equality.
		/// </summary>
		///<remarks>
		/// Items that are null are permitted, and will be sorted before all other items.
		///</remarks>
		public Set()
			:
				this(EqualityComparer<T>.Default)
		{
		}

		/// <summary>
		/// Creates a new Set. The Equals and GetHashCode method of the passed comparer object
		/// will be used to compare items in this set.
		/// </summary>
		/// <param name="equalityComparer">An instance of IEqualityComparer&lt;T&gt; that will be used to compare items.</param>
		public Set(IEqualityComparer<T> equalityComparer)
		{
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			this.equalityComparer = equalityComparer;
			hash = new Hash<T>(equalityComparer);
		}

		/// <summary>
		/// Creates a new Set. The Equals method and GetHashCode method on T
		/// will be used to compare items for equality.
		/// </summary>
		///<remarks>
		/// Items that are null are permitted.
		///</remarks>
		/// <param name="collection">A collection with items to be placed into the Set.</param>
		public Set(IEnumerable<T> collection)
			:
				this(collection, EqualityComparer<T>.Default)
		{
		}

		/// <summary>
		/// Creates a new Set. The Equals and GetHashCode method of the passed comparer object
		/// will be used to compare items in this set. The set is
		/// initialized with all the items in the given collection.
		/// </summary>
		/// <param name="collection">A collection with items to be placed into the Set.</param>
		/// <param name="equalityComparer">An instance of IEqualityComparer&lt;T&gt; that will be used to compare items.</param>
		public Set(IEnumerable<T> collection, IEqualityComparer<T> equalityComparer)
			: this(equalityComparer)
		{
			AddMany(collection);
		}

		/// <summary>
		/// Creates a new Set given a comparer and a tree that contains the data. Used
		/// internally for Clone.
		/// </summary>
		/// <param name="equalityComparer">EqualityComparer for the set.</param>
		/// <param name="hash">Data for the set.</param>
		private Set(IEqualityComparer<T> equalityComparer, Hash<T> hash)
		{
			this.equalityComparer = equalityComparer;
			this.hash = hash;
		}

		#endregion Constructors

		#region Cloning

		/// <summary>
		/// Makes a shallow clone of this set; i.e., if items of the
		/// set are reference types, then they are not cloned. If T is a value type,
		/// then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks>Cloning the set takes time O(N), where N is the number of items in the set.</remarks>
		/// <returns>The cloned set.</returns>
		object ICloneable.Clone()
		{
			return this.Clone();
		}

		/// <summary>
		/// Makes a shallow clone of this set; i.e., if items of the
		/// set are reference types, then they are not cloned. If T is a value type,
		/// then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks>Cloning the set takes time O(N), where N is the number of items in the set.</remarks>
		/// <returns>The cloned set.</returns>
		public Set<T> Clone()
		{
			Set<T> newSet = new Set<T>(equalityComparer, hash.Clone(null));
			return newSet;
		}

		/// <summary>
		/// Makes a deep clone of this set. A new set is created with a clone of
		/// each element of this set, by calling ICloneable.Clone on each element. If T is
		/// a value type, then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks><para>If T is a reference type, it must implement
		/// ICloneable. Otherwise, an InvalidOperationException is thrown.</para>
		/// <para>Cloning the set takes time O(N), where N is the number of items in the set.</para></remarks>
		/// <returns>The cloned set.</returns>
		/// <exception cref="InvalidOperationException">T is a reference type that does not implement ICloneable.</exception>
		public Set<T> CloneContents()
		{
			bool itemIsValueType;
			if (!Util.IsCloneableType(typeof (T), out itemIsValueType))
				throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, typeof (T).FullName));

			Set<T> clone = new Set<T>(equalityComparer);

			// Clone each item, and add it to the new ordered set.
			foreach (T item in this)
			{
				T itemClone;

				if (itemIsValueType)
					itemClone = item;
				else
				{
					if (item == null)
						itemClone = default(T); // Really null, because we know T is a reference type
					else
						itemClone = (T) (((ICloneable) item).Clone());
				}

				clone.Add(itemClone);
			}

			return clone;
		}

		#endregion Cloning

		#region Basic collection containment

		/// <summary>
		/// Returns the IEqualityComparer&lt;T&gt; used to compare items in this set. 
		/// </summary>
		/// <value>If the set was created using a comparer, that comparer is returned. Otherwise
		/// the default comparer for T (EqualityComparer&lt;T&gt;.Default) is returned.</value>
		public IEqualityComparer<T> Comparer
		{
			get { return this.equalityComparer; }
		}

		/// <summary>
		/// Returns the number of items in the set.
		/// </summary>
		/// <remarks>The size of the set is returned in constant time.</remarks>
		/// <value>The number of items in the set.</value>
		public override sealed int Count
		{
			get { return hash.ElementCount; }
		}

		/// <summary>
		/// Returns an enumerator that enumerates all the items in the set. 
		/// The items are enumerated in sorted order.
		/// </summary>
		/// <remarks>
		/// <p>Typically, this method is not called directly. Instead the "foreach" statement is used
		/// to enumerate the items, which uses this method implicitly.</p>
		/// <p>If an item is added to or deleted from the set while it is being enumerated, then 
		/// the enumeration will end with an InvalidOperationException.</p>
		/// <p>Enumerating all the items in the set takes time O(N), where N is the number
		/// of items in the set.</p>
		/// </remarks>
		/// <returns>An enumerator for enumerating all the items in the Set.</returns>		
		public override sealed IEnumerator<T> GetEnumerator()
		{
			return hash.GetEnumerator();
		}

		/// <summary>
		/// Determines if this set contains an item equal to <paramref name="item"/>. The set
		/// is not changed.
		/// </summary>
		/// <remarks>Searching the set for an item takes approximately constant time, regardless of the number of items in the set.</remarks>
		/// <param name="item">The item to search for.</param>
		/// <returns>True if the set contains <paramref name="item"/>. False if the set does not contain <paramref name="item"/>.</returns>
		public override sealed bool Contains(T item)
		{
			T dummy;
			return hash.Find(item, false, out dummy);
		}

		/// <summary>
		/// <para>Determines if this set contains an item equal to <paramref name="item"/>, according to the 
		/// comparison mechanism that was used when the set was created. The set
		/// is not changed.</para>
		/// <para>If the set does contain an item equal to <paramref name="item"/>, then the item from the set is returned.</para>
		/// </summary>
		/// <remarks>Searching the set for an item takes approximately constant time, regardless of the number of items in the set.</remarks>
		/// <example>
		/// In the following example, the set contains strings which are compared in a case-insensitive manner. 
		/// <code>
		/// Set&lt;string&gt; set = new Set&lt;string&gt;(StringComparer.CurrentCultureIgnoreCase);
		/// set.Add("HELLO");
		/// string s;
		/// bool b = set.TryGetItem("Hello", out s);   // b receives true, s receives "HELLO".
		/// </code>
		/// </example>
		/// <param name="item">The item to search for.</param>
		/// <param name="foundItem">Returns the item from the set that was equal to <paramref name="item"/>.</param>
		/// <returns>True if the set contains <paramref name="item"/>. False if the set does not contain <paramref name="item"/>.</returns>
		public bool TryGetItem(T item, out T foundItem)
		{
			return hash.Find(item, false, out foundItem);
		}

		#endregion

		#region Adding elements

		/// <summary>
		/// Adds a new item to the set. If the set already contains an item equal to
		/// <paramref name="item"/>, that item is replaced with <paramref name="item"/>.
		/// </summary>
		/// <remarks>
		/// <para>Equality between items is determined by the comparison instance or delegate used
		/// to create the set.</para>
		/// <para>Adding an item takes approximately constant time, regardless of the number of items in the set.</para></remarks>
		/// <param name="item">The item to add to the set.</param>
		/// <returns>True if the set already contained an item equal to <paramref name="item"/> (which was replaced), false 
		/// otherwise.</returns>
		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		/// <summary>
		/// Adds a new item to the set. If the set already contains an item equal to
		/// <paramref name="item"/>, that item is replaced with <paramref name="item"/>.
		/// </summary>
		/// <remarks>
		/// <para>Equality between items is determined by the comparison instance or delegate used
		/// to create the set.</para>
		/// <para>Adding an item takes approximately constant time, regardless of the number of items in the set.</para></remarks>
		/// <param name="item">The item to add to the set.</param>
		/// <returns>True if the set already contained an item equal to <paramref name="item"/> (which was replaced), false 
		/// otherwise.</returns>
		public new bool Add(T item)
		{
			T dummy;
			return !hash.Insert(item, true, out dummy);
		}

		/// <summary>
		/// Adds all the items in <paramref name="collection"/> to the set. If the set already contains an item equal to
		/// one of the items in <paramref name="collection"/>, that item will be replaced.
		/// </summary>
		/// <remarks>
		/// <para>Equality between items is determined by the comparison instance or delegate used
		/// to create the set.</para>
		/// <para>Adding the collection takes time O(M), where M is the 
		/// number of items in <paramref name="collection"/>.</para></remarks>
		/// <param name="collection">A collection of items to add to the set.</param>
		public void AddMany(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			// If we're adding ourselves, then there is nothing to do.
			if (ReferenceEquals(collection, this))
				return;

			foreach (T item in collection)
				Add(item);
		}

		#endregion Adding elements

		#region Removing elements

		/// <summary>
		/// Searches the set for an item equal to <paramref name="item"/>, and if found,
		/// removes it from the set. If not found, the set is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>Equality between items is determined by the comparison instance or delegate used
		/// to create the set.</para>
		/// <para>Removing an item from the set takes approximately constant time, regardless of the size of the set.</para></remarks>
		/// <param name="item">The item to remove.</param>
		/// <returns>True if <paramref name="item"/> was found and removed. False if <paramref name="item"/> was not in the set.</returns>
		public override sealed bool Remove(T item)
		{
			T dummy;
			return hash.Delete(item, out dummy);
		}

		/// <summary>
		/// Removes all items from the set.
		/// </summary>
		/// <remarks>Clearing the set takes a constant amount of time, regardless of the number of items in it.</remarks>
		public override sealed void Clear()
		{
			hash.StopEnumerations(); // Invalidate any enumerations.

			// The simplest and fastest way is simply to throw away the old tree and create a new one.
			hash = new Hash<T>(equalityComparer);
		}

		/// <summary>
		/// Removes all the items in <paramref name="collection"/> from the set. 
		/// </summary>
		/// <remarks>
		/// <para>Equality between items is determined by the comparison instance or delegate used
		/// to create the set.</para>
		/// <para>Removing the collection takes time O(M), where M is the 
		/// number of items in <paramref name="collection"/>.</para></remarks>
		/// <param name="collection">A collection of items to remove from the set.</param>
		/// <returns>The number of items removed from the set.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public int RemoveMany(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			int count = 0;

			if (collection == this)
			{
				count = Count;
				Clear(); // special case, otherwise we will throw.
			}
			else
			{
				foreach (T item in collection)
				{
					if (Remove(item))
						++count;
				}
			}

			return count;
		}

		#endregion Removing elements

		#region Set operations

		/// <summary>
		/// Determines if this set is a superset of another set. Neither set is modified.
		/// This set is a superset of <paramref name="otherSet"/> if every element in
		/// <paramref name="otherSet"/> is also in this set.
		/// <remarks>IsSupersetOf is computed in time O(M), where M is the size of the 
		/// <paramref name="otherSet"/>.</remarks>
		/// </summary>
		/// <param name="otherSet">Set to compare to.</param>
		/// <returns>True if this is a superset of <paramref name="otherSet"/>.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public bool IsSupersetOf(Set<T> otherSet)
		{
			CheckConsistentComparison(otherSet);

			if (otherSet.Count > this.Count)
				return false; // Can't be a superset of a bigger set

			// Check each item in the other set to make sure it is in this set.
			foreach (T item in otherSet)
			{
				if (!this.Contains(item))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Determines if this set is a proper superset of another set. Neither set is modified.
		/// This set is a proper superset of <paramref name="otherSet"/> if every element in
		/// <paramref name="otherSet"/> is also in this set.
		/// Additionally, this set must have strictly more items than <paramref name="otherSet"/>.
		/// </summary>
		/// <remarks>IsProperSubsetOf is computed in time O(M), where M is the size of
		/// <paramref name="otherSet"/>.</remarks>
		/// <param name="otherSet">Set to compare to.</param>
		/// <returns>True if this is a proper superset of <paramref name="otherSet"/>.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public bool IsProperSupersetOf(Set<T> otherSet)
		{
			CheckConsistentComparison(otherSet);

			if (otherSet.Count >= this.Count)
				return false; // Can't be a proper superset of a bigger or equal set

			return IsSupersetOf(otherSet);
		}

		/// <summary>
		/// Determines if this set is a subset of another set. Neither set is modified.
		/// This set is a subset of <paramref name="otherSet"/> if every element in this set
		/// is also in <paramref name="otherSet"/>.
		/// </summary>
		/// <remarks>IsSubsetOf is computed in time O(N), where N is the size of the this set.</remarks>
		/// <param name="otherSet">Set to compare to.</param>
		/// <returns>True if this is a subset of <paramref name="otherSet"/>.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public bool IsSubsetOf(Set<T> otherSet)
		{
			return otherSet.IsSupersetOf(this);
		}

		/// <summary>
		/// Determines if this set is a proper subset of another set. Neither set is modified.
		/// This set is a subset of <paramref name="otherSet"/> if every element in this set
		/// is also in <paramref name="otherSet"/>. Additionally, this set must have strictly 
		/// fewer items than <paramref name="otherSet"/>.
		/// </summary>
		/// <remarks>IsProperSubsetOf is computed in time O(N), where N is the size of the this set.</remarks>
		/// <param name="otherSet">Set to compare to.</param>
		/// <returns>True if this is a proper subset of <paramref name="otherSet"/>.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public bool IsProperSubsetOf(Set<T> otherSet)
		{
			return otherSet.IsProperSupersetOf(this);
		}

		/// <summary>
		/// Determines if this set is equal to another set. This set is equal to
		/// <paramref name="otherSet"/> if they contain the same items.
		/// </summary>
		/// <remarks>IsEqualTo is computed in time O(N), where N is the number of items in 
		/// this set.</remarks>
		/// <param name="otherSet">Set to compare to</param>
		/// <returns>True if this set is equal to <paramref name="otherSet"/>, false otherwise.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public bool IsEqualTo(Set<T> otherSet)
		{
			CheckConsistentComparison(otherSet);

			// Must be the same size.
			if (otherSet.Count != this.Count)
				return false;

			// Check each item in the other set to make sure it is in this set.
			foreach (T item in otherSet)
			{
				if (!this.Contains(item))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Determines if this set is disjoint from another set. Two sets are disjoint
		/// if no item from one set is equal to any item in the other set.
		/// </summary>
		/// <remarks>
		/// <para>The answer is computed in time O(N), where N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to check disjointness with.</param>
		/// <returns>True if the two sets are disjoint, false otherwise.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public bool IsDisjointFrom(Set<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			Set<T> smaller, larger;
			if (otherSet.Count > this.Count)
			{
				smaller = this;
				larger = otherSet;
			}
			else
			{
				smaller = otherSet;
				larger = this;
			}

			foreach (T item in smaller)
			{
				if (larger.Contains(item))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Computes the union of this set with another set. The union of two sets
		/// is all items that appear in either or both of the sets. This set receives
		/// the union of the two sets, the other set is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>If equal items appear in both sets, the union will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The union of two sets is computed in time O(M + N), where M is the size of the 
		/// larger set, and N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to union with.</param>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public void UnionWith(Set<T> otherSet)
		{
			CheckConsistentComparison(otherSet);

			AddMany(otherSet);
		}

		/// <summary>
		/// Computes the union of this set with another set. The union of two sets
		/// is all items that appear in either or both of the sets. A new set is 
		/// created with the union of the sets and is returned. This set and the other set 
		/// are unchanged.
		/// </summary>
		/// <remarks>
		/// <para>If equal items appear in both sets, the union will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The union of two sets is computed in time O(M + N), where M is the size of the 
		/// one set, and N is the size of the other set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to union with.</param>
		/// <returns>The union of the two sets.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public Set<T> Union(Set<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			Set<T> smaller, larger, result;
			if (otherSet.Count > this.Count)
			{
				smaller = this;
				larger = otherSet;
			}
			else
			{
				smaller = otherSet;
				larger = this;
			}

			result = larger.Clone();
			result.AddMany(smaller);
			return result;
		}

		/// <summary>
		/// Computes the intersection of this set with another set. The intersection of two sets
		/// is all items that appear in both of the sets. This set receives
		/// the intersection of the two sets, the other set is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both sets, the intersection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The intersection of two sets is computed in time O(N), where N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to intersection with.</param>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public void IntersectionWith(Set<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			hash.StopEnumerations();

			Set<T> smaller, larger;
			if (otherSet.Count > this.Count)
			{
				smaller = this;
				larger = otherSet;
			}
			else
			{
				smaller = otherSet;
				larger = this;
			}

			T dummy;
			Hash<T> newHash = new Hash<T>(equalityComparer);

			foreach (T item in smaller)
			{
				if (larger.Contains(item))
					newHash.Insert(item, true, out dummy);
			}

			hash = newHash;
		}

		/// <summary>
		/// Computes the intersection of this set with another set. The intersection of two sets
		/// is all items that appear in both of the sets. A new set is 
		/// created with the intersection of the sets and is returned. This set and the other set 
		/// are unchanged.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both sets, the intersection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The intersection of two sets is computed in time O(N), where N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to intersection with.</param>
		/// <returns>The intersection of the two sets.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public Set<T> Intersection(Set<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			Set<T> smaller, larger, result;
			if (otherSet.Count > this.Count)
			{
				smaller = this;
				larger = otherSet;
			}
			else
			{
				smaller = otherSet;
				larger = this;
			}

			result = new Set<T>(equalityComparer);
			foreach (T item in smaller)
			{
				if (larger.Contains(item))
					result.Add(item);
			}

			return result;
		}

		/// <summary>
		/// Computes the difference of this set with another set. The difference of these two sets
		/// is all items that appear in this set, but not in <paramref name="otherSet"/>. This set receives
		/// the difference of the two sets; the other set is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The difference of two sets is computed in time O(N), where N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to difference with.</param>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public void DifferenceWith(Set<T> otherSet)
		{
			// Difference with myself is nothing. This check is needed because the
			// main algorithm doesn't work correctly otherwise.
			if (this == otherSet)
				Clear();

			CheckConsistentComparison(otherSet);

			if (otherSet.Count < this.Count)
			{
				foreach (T item in otherSet)
				{
					this.Remove(item);
				}
			}
			else
			{
				RemoveAll(delegate(T item)
					{
						return otherSet.Contains(item);
					});
			}
		}

		/// <summary>
		/// Computes the difference of this set with another set. The difference of these two sets
		/// is all items that appear in this set, but not in <paramref name="otherSet"/>. A new set is 
		/// created with the difference of the sets and is returned. This set and the other set 
		/// are unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The difference of two sets is computed in time O(N), where N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to difference with.</param>
		/// <returns>The difference of the two sets.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public Set<T> Difference(Set<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			Set<T> result = this.Clone();
			result.DifferenceWith(otherSet);
			return result;
		}

		/// <summary>
		/// Computes the symmetric difference of this set with another set. The symmetric difference of two sets
		/// is all items that appear in either of the sets, but not both. This set receives
		/// the symmetric difference of the two sets; the other set is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The symmetric difference of two sets is computed in time O(N), where N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to symmetric difference with.</param>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public void SymmetricDifferenceWith(Set<T> otherSet)
		{
			// main algorithm doesn't work correctly otherwise.
			if (this == otherSet)
				Clear();

			CheckConsistentComparison(otherSet);

			if (otherSet.Count > this.Count)
			{
				hash.StopEnumerations();
				Hash<T> newHash = otherSet.hash.Clone(null);
				T dummy;

				foreach (T item in this)
				{
					if (newHash.Find(item, false, out dummy))
						newHash.Delete(item, out dummy);
					else
						newHash.Insert(item, true, out dummy);
				}
				this.hash = newHash;
			}
			else
			{
				foreach (T item in otherSet)
				{
					if (this.Contains(item))
						this.Remove(item);
					else
						this.Add(item);
				}
			}
		}

		/// <summary>
		/// Computes the symmetric difference of this set with another set. The symmetric difference of two sets
		/// is all items that appear in either of the sets, but not both. A new set is 
		/// created with the symmetric difference of the sets and is returned. This set and the other set 
		/// are unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The symmetric difference of two sets is computed in time O(N), where N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to symmetric difference with.</param>
		/// <returns>The symmetric difference of the two sets.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public Set<T> SymmetricDifference(Set<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			Set<T> smaller, larger, result;
			if (otherSet.Count > this.Count)
			{
				smaller = this;
				larger = otherSet;
			}
			else
			{
				smaller = otherSet;
				larger = this;
			}

			result = larger.Clone();
			foreach (T item in smaller)
			{
				if (result.Contains(item))
					result.Remove(item);
				else
					result.Add(item);
			}

			return result;
		}

		/// <summary>
		/// Check that this set and another set were created with the same comparison
		/// mechanism. Throws exception if not compatible.
		/// </summary>
		/// <param name="otherSet">Other set to check comparision mechanism.</param>
		/// <exception cref="InvalidOperationException">If otherSet and this set don't use the same method for comparing items.</exception>
		private void CheckConsistentComparison(Set<T> otherSet)
		{
			if (otherSet == null)
				throw new ArgumentNullException("otherSet");

			if (!Equals(equalityComparer, otherSet.equalityComparer))
				throw new InvalidOperationException(Strings.InconsistentComparisons);
		}

		#endregion Set operations
	}
}