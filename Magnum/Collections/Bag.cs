// Copyright 2007-2010 The Apache Software Foundation.
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
	/// Bag&lt;T&gt; is a collection that contains items of type T. 
	/// Unlike a Set, duplicate items (items that compare equal to each other) are allowed in an Bag. 
	/// </summary>
	/// <remarks>
	/// <p>The items are compared in one of two ways. If T implements IComparable&lt;T&gt; 
	/// then the Equals method of that interface will be used to compare items, otherwise the Equals
	/// method from Object will be used. Alternatively, an instance of IComparer&lt;T&gt; can be passed
	/// to the constructor to use to compare items.</p>
	/// <p>Bag is implemented as a hash table. Inserting, deleting, and looking up an
	/// an element all are done in approximately constant time, regardless of the number of items in the bag.</p>
	/// <p>When multiple equal items are stored in the bag, they are stored as a representative item and a count. 
	/// If equal items can be distinguished, this may be noticable. For example, if a case-insensitive
	/// comparer is used with a Bag&lt;string&gt;, and both "hello", and "HELLO" are added to the bag, then the
	/// bag will appear to contain two copies of "hello" (the representative item).</p>
	/// <p><see cref="OrderedBag&lt;T&gt;"/> is similar, but uses comparison instead of hashing, maintain
	/// the items in sorted order, and stores distinct copies of items that compare equal.</p>
	///</remarks>
	///<seealso cref="OrderedBag&lt;T&gt;"/>
	[Serializable]
	public class Bag<T> : CollectionBase<T>,
	                      ICloneable
	{
		// The comparer used to compare KeyValuePairs. Equals and GetHashCode are used.
		readonly IEqualityComparer<KeyValuePair<T, int>> equalityComparer;

		// The comparer used to compare items. Kept just for the Comparer property. 
		readonly IEqualityComparer<T> keyEqualityComparer;

		// The hash that actually does the work of storing the items. Each item is
		// stored as a representative item, and a count.

		// The total number of items stored in the bag.
		int count;
		Hash<KeyValuePair<T, int>> hash;

		/// <summary>
		/// Helper function to create a new KeyValuePair struct with an item and a count.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="count">The number of appearances.</param>
		/// <returns>A new KeyValuePair.</returns>
		static KeyValuePair<T, int> NewPair(T item, int count)
		{
			var pair = new KeyValuePair<T, int>(item, count);
			return pair;
		}

		/// <summary>
		/// Helper function to create a new KeyValuePair struct with a count of zero.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>A new KeyValuePair.</returns>
		static KeyValuePair<T, int> NewPair(T item)
		{
			var pair = new KeyValuePair<T, int>(item, 0);
			return pair;
		}

		#region Constructors

		/// <summary>
		/// Creates a new Bag. 
		/// </summary>
		///<remarks>
		/// Items that are null are permitted.
		///</remarks>
		public Bag()
			:
				this(EqualityComparer<T>.Default)
		{
		}

		/// <summary>
		/// Creates a new Bag. The Equals and GetHashCode methods of the passed comparison object
		/// will be used to compare items in this bag for equality.
		/// </summary>
		/// <param name="equalityComparer">An instance of IEqualityComparer&lt;T&gt; that will be used to compare items.</param>
		public Bag(IEqualityComparer<T> equalityComparer)
		{
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			keyEqualityComparer = equalityComparer;
			this.equalityComparer = Comparers.EqualityComparerKeyValueFromComparerKey<T, int>(equalityComparer);
			hash = new Hash<KeyValuePair<T, int>>(this.equalityComparer);
		}

		/// <summary>
		/// Creates a new Bag. The bag is
		/// initialized with all the items in the given collection.
		/// </summary>
		///<remarks>
		/// Items that are null are permitted.
		///</remarks>
		/// <param name="collection">A collection with items to be placed into the Bag.</param>
		public Bag(IEnumerable<T> collection)
			:
				this(collection, EqualityComparer<T>.Default)
		{
		}

		/// <summary>
		/// Creates a new Bag. The Equals and GetHashCode methods of the passed comparison object
		/// will be used to compare items in this bag. The bag is
		/// initialized with all the items in the given collection.
		/// </summary>
		/// <param name="collection">A collection with items to be placed into the Bag.</param>
		/// <param name="equalityComparer">An instance of IEqualityComparer&lt;T&gt; that will be used to compare items.</param>
		public Bag(IEnumerable<T> collection, IEqualityComparer<T> equalityComparer)
			: this(equalityComparer)
		{
			AddMany(collection);
		}

		/// <summary>
		/// Creates a new Bag given a comparer and a hash that contains the data. Used
		/// internally for Clone.
		/// </summary>
		/// <param name="equalityComparer">IEqualityComparer for the bag.</param>
		/// <param name="keyEqualityComparer">IEqualityComparer for the key.</param>
		/// <param name="hash">Data for the bag.</param>
		/// <param name="count">Size of the bag.</param>
		Bag(IEqualityComparer<KeyValuePair<T, int>> equalityComparer, IEqualityComparer<T> keyEqualityComparer,
		    Hash<KeyValuePair<T, int>> hash, int count)
		{
			this.equalityComparer = equalityComparer;
			this.keyEqualityComparer = keyEqualityComparer;
			this.hash = hash;
			this.count = count;
		}

		#endregion Constructors

		#region Cloning

		/// <summary>
		/// Makes a shallow clone of this bag; i.e., if items of the
		/// bag are reference types, then they are not cloned. If T is a value type,
		/// then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks>Cloning the bag takes time O(N), where N is the number of items in the bag.</remarks>
		/// <returns>The cloned bag.</returns>
		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Makes a shallow clone of this bag; i.e., if items of the
		/// bag are reference types, then they are not cloned. If T is a value type,
		/// then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks>Cloning the bag takes time O(N), where N is the number of unquie items in the bag.</remarks>
		/// <returns>The cloned bag.</returns>
		public Bag<T> Clone()
		{
			var newBag = new Bag<T>(equalityComparer, keyEqualityComparer, hash.Clone(null), count);
			return newBag;
		}

		/// <summary>
		/// Makes a deep clone of this bag. A new bag is created with a clone of
		/// each element of this bag, by calling ICloneable.Clone on each element. If T is
		/// a value type, then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks><para>If T is a reference type, it must implement
		/// ICloneable. Otherwise, an InvalidOperationException is thrown.</para>
		/// <para>Cloning the bag takes time O(N log N), where N is the number of items in the bag.</para></remarks>
		/// <returns>The cloned bag.</returns>
		/// <exception cref="InvalidOperationException">T is a reference type that does not implement ICloneable.</exception>
		public Bag<T> CloneContents()
		{
			bool itemIsValueType;
			if (!Util.IsCloneableType(typeof(T), out itemIsValueType))
				throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, typeof(T).FullName));

			var newHash = new Hash<KeyValuePair<T, int>>(equalityComparer);

			// Clone each item, and add it to the new ordered bag.
			foreach (var pair in hash)
			{
				KeyValuePair<T, int> newPair, dummy;
				T newKey;

				if (!itemIsValueType && pair.Key != null)
					newKey = (T)(((ICloneable)pair.Key).Clone());
				else
					newKey = pair.Key;

				newPair = NewPair(newKey, pair.Value);

				newHash.Insert(newPair, true, out dummy);
			}

			return new Bag<T>(equalityComparer, keyEqualityComparer, newHash, count);
		}

		#endregion Cloning

		#region Basic collection containment

		/// <summary>
		/// Returns the IEqualityComparer&lt;T&gt; used to compare items in this bag. 
		/// </summary>
		/// <value>If the bag was created using a comparer, that comparer is returned. Otherwise
		/// the default comparer for T (EqualityComparer&lt;T&gt;.Default) is returned.</value>
		public IEqualityComparer<T> Comparer
		{
			get { return keyEqualityComparer; }
		}

		/// <summary>
		/// Returns the number of items in the bag.
		/// </summary>
		/// <remarks>The size of the bag is returned in constant time.</remarks>
		/// <value>The number of items in the bag.</value>
		public override sealed int Count
		{
			get { return count; }
		}

		/// <summary>
		/// Returns the number of copies of <paramref name="item"/> in the bag. 
		/// </summary>
		/// <remarks>NumberOfCopies() takes approximately constant time, no matter how many items
		/// are stored in the bag.</remarks>
		/// <param name="item">The item to search for in the bag.</param>
		/// <returns>The number of items in the bag that compare equal to <paramref name="item"/>.</returns>
		public int NumberOfCopies(T item)
		{
			KeyValuePair<T, int> foundPair;
			if (hash.Find(NewPair(item), false, out foundPair))
				return foundPair.Value;
			else
				return 0;
		}

		/// <summary>
		/// Returns the representative item stored in the bag that is equal to
		/// the provided item. Also returns the number of copies of the item in the bag.
		/// </summary>
		/// <param name="item">Item to find in the bag.</param>
		/// <param name="representative">If one or more items equal to <paramref name="item"/> are present in the
		/// bag, returns the representative item. If no items equal to <paramref name="item"/> are stored in the bag, 
		/// returns <paramref name="item"/>.</param>
		/// <returns>The number of items equal to <paramref name="item"/> stored in the bag.</returns>
		public int GetRepresentativeItem(T item, out T representative)
		{
			KeyValuePair<T, int> foundPair;
			if (hash.Find(NewPair(item), false, out foundPair))
			{
				representative = foundPair.Key;
				return foundPair.Value;
			}
			else
			{
				representative = item;
				return 0;
			}
		}

		/// <summary>
		/// Returns an enumerator that enumerates all the items in the bag. 
		/// If an item is present multiple times in the bag, the representative item is yielded by the
		/// enumerator multiple times. The order of enumeration is haphazard and may change.
		/// </summary>
		/// <remarks>
		/// <p>Typically, this method is not called directly. Instead the "foreach" statement is used
		/// to enumerate the items, which uses this method implicitly.</p>
		/// <p>If an item is added to or deleted from the bag while it is being enumerated, then 
		/// the enumeration will end with an InvalidOperationException.</p>
		/// <p>Enumeration all the items in the bag takes time O(N), where N is the number
		/// of items in the bag.</p>
		/// </remarks>
		/// <returns>An enumerator for enumerating all the items in the Bag.</returns>		
		public override sealed IEnumerator<T> GetEnumerator()
		{
			foreach (var pair in hash)
			{
				for (int i = 0; i < pair.Value; ++i)
					yield return pair.Key;
			}
		}

		/// <summary>
		/// Determines if this bag contains an item equal to <paramref name="item"/>. The bag
		/// is not changed.
		/// </summary>
		/// <remarks>Searching the bag for an item takes time O(log N), where N is the number of items in the bag.</remarks>
		/// <param name="item">The item to search for.</param>
		/// <returns>True if the bag contains <paramref name="item"/>. False if the bag does not contain <paramref name="item"/>.</returns>
		public override sealed bool Contains(T item)
		{
			KeyValuePair<T, int> dummy;
			return hash.Find(NewPair(item), false, out dummy);
		}

		/// <summary>
		/// Enumerates all the items in the bag, but enumerates equal items
		/// just once, even if they occur multiple times in the bag.
		/// </summary>
		/// <remarks>If the bag is changed while items are being enumerated, the
		/// enumeration will terminate with an InvalidOperationException.</remarks>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates the unique items.</returns>
		public IEnumerable<T> DistinctItems()
		{
			foreach (var pair in hash)
				yield return pair.Key;
		}

		#endregion

		#region Adding elements

		/// <summary>
		/// Adds a new item to the bag. Since bags can contain duplicate items, the item 
		/// is added even if the bag already contains an item equal to <paramref name="item"/>. In
		/// this case, the count of items for the representative item is increased by one, but the existing
		/// represetative item is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>Adding an item takes approximately constant time, regardless of the number of items in the bag.</para></remarks>
		/// <param name="item">The item to add to the bag.</param>
		public override sealed void Add(T item)
		{
			KeyValuePair<T, int> pair = NewPair(item, 1);
			KeyValuePair<T, int> existing, newPair;
			if (! hash.Insert(pair, false, out existing))
			{
				// The item already existed, so update the count instead.
				newPair = NewPair(existing.Key, existing.Value + 1);
				hash.Insert(newPair, true, out pair);
			}
			++count;
		}

		// CONSIDER: add an example to the documentation below.
		/// <summary>
		/// Adds a new item to the bag. Since bags can contain duplicate items, the item 
		/// is added even if the bag already contains an item equal to <paramref name="item"/>. In
		/// this case (unlike Add), the new item becomes the representative item.
		/// </summary>
		/// <remarks>
		/// <para>Adding an item takes approximately constant time, regardless of the number of items in the bag.</para></remarks>
		/// <param name="item">The item to add to the bag.</param>
		public void AddRepresentative(T item)
		{
			KeyValuePair<T, int> pair = NewPair(item, 1);
			KeyValuePair<T, int> existing, newPair;
			if (!hash.Insert(pair, false, out existing))
			{
				// The item already existed, so update the count instead.
				newPair = NewPair(pair.Key, existing.Value + 1);
				hash.Insert(newPair, true, out pair);
			}
			++count;
		}

		/// <summary>
		/// Changes the number of copies of an existing item in the bag, or adds the indicated number
		/// of copies of the item to the bag. 
		/// </summary>
		/// <remarks>
		/// <para>Changing the number of copies takes approximately constant time, regardless of the number of items in the bag.</para></remarks>
		/// <param name="item">The item to change the number of copies of. This may or may not already be present in the bag.</param>
		/// <param name="numCopies">The new number of copies of the item.</param>
		public void ChangeNumberOfCopies(T item, int numCopies)
		{
			if (numCopies == 0)
				RemoveAllCopies(item);
			else
			{
				KeyValuePair<T, int> dummy, existing, newPair;
				if (hash.Find(NewPair(item), false, out existing))
				{
					count += numCopies - existing.Value;
					newPair = NewPair(existing.Key, numCopies);
				}
				else
				{
					count += numCopies;
					newPair = NewPair(item, numCopies);
				}
				hash.Insert(newPair, true, out dummy);
			}
		}

		/// <summary>
		/// Adds all the items in <paramref name="collection"/> to the bag. 
		/// </summary>
		/// <remarks>
		/// <para>Adding the collection takes time O(M log N), where N is the number of items in the bag, and M is the 
		/// number of items in <paramref name="collection"/>.</para></remarks>
		/// <param name="collection">A collection of items to add to the bag.</param>
		public void AddMany(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			// If we're adding ourselves, we need to copy to a separate array to avoid modification
			// during enumeration.
			if (this == collection)
				collection = ToArray();

			foreach (T item in collection)
				Add(item);
		}

		#endregion Adding elements

		#region Removing elements

		/// <summary>
		/// Searches the bag for one item equal to <paramref name="item"/>, and if found,
		/// removes it from the bag. If not found, the bag is unchanged. 
		/// </summary>
		/// <remarks>
		/// <para>Equality between items is determined by the comparison instance or delegate used
		/// to create the bag.</para>
		/// <para>Removing an item from the bag takes approximated constant time,
		/// regardless of the number of items in the bag.</para></remarks>
		/// <param name="item">The item to remove.</param>
		/// <returns>True if <paramref name="item"/> was found and removed. False if <paramref name="item"/> was not in the bag.</returns>
		public override sealed bool Remove(T item)
		{
			KeyValuePair<T, int> removed, newPair;
			if (hash.Delete(NewPair(item), out removed))
			{
				if (removed.Value > 1)
				{
					// Only want to remove one copied, so add back in with a reduced count.
					KeyValuePair<T, int> dummy;
					newPair = NewPair(removed.Key, removed.Value - 1);
					hash.Insert(newPair, true, out dummy);
				}
				--count;
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Searches the bag for all items equal to <paramref name="item"/>, and 
		/// removes all of them from the bag. If not found, the bag is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>Equality between items is determined by the comparer instance used
		/// to create the bag.</para>
		/// <para>RemoveAllCopies() takes time O(M log N), where N is the total number of items in the bag, and M is
		/// the number of items equal to <paramref name="item"/>.</para></remarks>
		/// <param name="item">The item to remove.</param>
		/// <returns>The number of copies of <paramref name="item"/> that were found and removed. </returns>
		public int RemoveAllCopies(T item)
		{
			KeyValuePair<T, int> removed;
			if (hash.Delete(NewPair(item), out removed))
			{
				count -= removed.Value;
				return removed.Value;
			}
			else
				return 0;
		}

		/// <summary>
		/// Removes all the items in <paramref name="collection"/> from the bag. Items that
		/// are not present in the bag are ignored.
		/// </summary>
		/// <remarks>
		/// <para>Equality between items is determined by the comparer instance used
		/// to create the bag.</para>
		/// <para>Removing the collection takes time O(M), where M is the 
		/// number of items in <paramref name="collection"/>.</para></remarks>
		/// <param name="collection">A collection of items to remove from the bag.</param>
		/// <returns>The number of items removed from the bag.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public int RemoveMany(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			int removeCount = 0;

			if (collection == this)
			{
				removeCount = Count;
				Clear(); // special case, otherwise we will throw.
			}
			else
			{
				foreach (T item in collection)
				{
					if (Remove(item))
						++removeCount;
				}
			}

			return removeCount;
		}

		/// <summary>
		/// Removes all items from the bag.
		/// </summary>
		/// <remarks>Clearing the bag takes a constant amount of time, regardless of the number of items in it.</remarks>
		public override sealed void Clear()
		{
			hash.StopEnumerations(); // Invalidate any enumerations.

			// The simplest and fastest way is simply to throw away the old hash and create a new one.
			hash = new Hash<KeyValuePair<T, int>>(equalityComparer);
			count = 0;
		}

		#endregion Removing elements

		#region Set operations

		/// <summary>
		/// Determines if this bag is equal to another bag. This bag is equal to
		/// <paramref name="otherBag"/> if they contain the same number of 
		/// of copies of equal elements.
		/// </summary>
		/// <remarks>IsSupersetOf is computed in time O(N), where N is the number of unique items in 
		/// this bag.</remarks>
		/// <param name="otherBag">Bag to compare to</param>
		/// <returns>True if this bag is equal to <paramref name="otherBag"/>, false otherwise.</returns>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public bool IsEqualTo(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			// Must be the same size.
			if (otherBag.Count != Count)
				return false;

			// Check each item to make sure it is in this set the same number of times.
			foreach (T item in otherBag.DistinctItems())
			{
				if (NumberOfCopies(item) != otherBag.NumberOfCopies(item))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Determines if this bag is a superset of another bag. Neither bag is modified.
		/// This bag is a superset of <paramref name="otherBag"/> if every element in
		/// <paramref name="otherBag"/> is also in this bag, at least the same number of
		/// times.
		/// </summary>
		/// <remarks>IsSupersetOf is computed in time O(M), where M is the number of unique items in 
		/// <paramref name="otherBag"/>.</remarks>
		/// <param name="otherBag">Bag to compare to.</param>
		/// <returns>True if this is a superset of <paramref name="otherBag"/>.</returns>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public bool IsSupersetOf(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			if (otherBag.Count > Count)
				return false; // Can't be a superset of a bigger set

			// Check each item in the other set to make sure it is in this set.
			foreach (T item in otherBag.DistinctItems())
			{
				if (NumberOfCopies(item) < otherBag.NumberOfCopies(item))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Determines if this bag is a proper superset of another bag. Neither bag is modified.
		/// This bag is a proper superset of <paramref name="otherBag"/> if every element in
		/// <paramref name="otherBag"/> is also in this bag, at least the same number of
		/// times. Additional, this bag must have strictly more items than <paramref name="otherBag"/>.
		/// </summary>
		/// <remarks>IsProperSupersetOf is computed in time O(M), where M is the number of unique items in 
		/// <paramref name="otherBag"/>.</remarks>
		/// <param name="otherBag">Set to compare to.</param>
		/// <returns>True if this is a proper superset of <paramref name="otherBag"/>.</returns>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public bool IsProperSupersetOf(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			if (otherBag.Count >= Count)
				return false; // Can't be a proper superset of a bigger or equal set

			return IsSupersetOf(otherBag);
		}

		/// <summary>
		/// Determines if this bag is a subset of another ba11 items in this bag.
		/// </summary>
		/// <param name="otherBag">Bag to compare to.</param>
		/// <returns>True if this is a subset of <paramref name="otherBag"/>.</returns>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public bool IsSubsetOf(Bag<T> otherBag)
		{
			return otherBag.IsSupersetOf(this);
		}

		/// <summary>
		/// Determines if this bag is a proper subset of another bag. Neither bag is modified.
		/// This bag is a subset of <paramref name="otherBag"/> if every element in this bag
		/// is also in <paramref name="otherBag"/>, at least the same number of
		/// times. Additional, this bag must have strictly fewer items than <paramref name="otherBag"/>.
		/// </summary>
		/// <remarks>IsProperSubsetOf is computed in time O(N), where N is the number of unique items in this bag.</remarks>
		/// <param name="otherBag">Bag to compare to.</param>
		/// <returns>True if this is a proper subset of <paramref name="otherBag"/>.</returns>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public bool IsProperSubsetOf(Bag<T> otherBag)
		{
			return otherBag.IsProperSupersetOf(this);
		}

		/// <summary>
		/// Determines if this bag is disjoint from another bag. Two bags are disjoint
		/// if no item from one set is equal to any item in the other bag.
		/// </summary>
		/// <remarks>
		/// <para>The answer is computed in time O(N), where N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherBag">Bag to check disjointness with.</param>
		/// <returns>True if the two bags are disjoint, false otherwise.</returns>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public bool IsDisjointFrom(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);
			Bag<T> smaller, larger;
			if (otherBag.Count > Count)
			{
				smaller = this;
				larger = otherBag;
			}
			else
			{
				smaller = otherBag;
				larger = this;
			}

			foreach (T item in smaller.DistinctItems())
			{
				if (larger.Contains(item))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Computes the union of this bag with another bag. The union of two bags
		/// is all items from both of the bags. If an item appears X times in one bag,
		/// and Y times in the other bag, the union contains the item Maximum(X,Y) times. This bag receives
		/// the union of the two bags, the other bag is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The union of two bags is computed in time O(M+N), where M and N are the size of the 
		/// two bags.</para>
		/// </remarks>
		/// <param name="otherBag">Bag to union with.</param>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public void UnionWith(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			if (otherBag == this)
				return; // Nothing to do

			int copiesInThis, copiesInOther;

			// Enumerate each of the items in the other bag. Add items that need to be
			// added to this bag.
			foreach (T item in otherBag.DistinctItems())
			{
				copiesInThis = NumberOfCopies(item);
				copiesInOther = otherBag.NumberOfCopies(item);

				if (copiesInOther > copiesInThis)
					ChangeNumberOfCopies(item, copiesInOther);
			}
		}

		/// <summary>
		/// Computes the union of this bag with another bag. The union of two bags
		/// is all items from both of the bags.  If an item appears X times in one bag,
		/// and Y times in the other bag, the union contains the item Maximum(X,Y) times. A new bag is 
		/// created with the union of the bags and is returned. This bag and the other bag 
		/// are unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The union of two bags is computed in time O(M+N), where M and N are the size of the two bags.</para>
		/// </remarks>
		/// <param name="otherBag">Bag to union with.</param>
		/// <returns>The union of the two bags.</returns>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public Bag<T> Union(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			Bag<T> smaller, larger, result;
			if (otherBag.Count > Count)
			{
				smaller = this;
				larger = otherBag;
			}
			else
			{
				smaller = otherBag;
				larger = this;
			}

			result = larger.Clone();
			result.UnionWith(smaller);
			return result;
		}

		/// <summary>
		/// Computes the sum of this bag with another bag. The sum of two bags
		/// is all items from both of the bags. If an item appears X times in one bag,
		/// and Y times in the other bag, the sum contains the item (X+Y) times. This bag receives
		/// the sum of the two bags, the other bag is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The sum of two bags is computed in time O(M), where M is the size of the 
		/// other bag..</para>
		/// </remarks>
		/// <param name="otherBag">Bag to sum with.</param>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public void SumWith(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			if (this == otherBag)
			{
				// Not very efficient, but an uncommon case.
				AddMany(otherBag);
				return;
			}

			int copiesInThis, copiesInOther;

			// Enumerate each of the items in the other bag. Add items that need to be
			// added to this bag.
			foreach (T item in otherBag.DistinctItems())
			{
				copiesInThis = NumberOfCopies(item);
				copiesInOther = otherBag.NumberOfCopies(item);

				ChangeNumberOfCopies(item, copiesInThis + copiesInOther);
			}
		}

		/// <summary>
		/// Computes the sum of this bag with another bag. he sum of two bags
		/// is all items from both of the bags.  If an item appears X times in one bag,
		/// and Y times in the other bag, the sum contains the item (X+Y) times. A new bag is 
		/// created with the sum of the bags and is returned. This bag and the other bag 
		/// are unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The sum of two bags is computed in time O(M + N log M), where M is the size of the 
		/// larger bag, and N is the size of the smaller bag.</para>
		/// </remarks>
		/// <param name="otherBag">Bag to sum with.</param>
		/// <returns>The sum of the two bags.</returns>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public Bag<T> Sum(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			Bag<T> smaller, larger, result;
			if (otherBag.Count > Count)
			{
				smaller = this;
				larger = otherBag;
			}
			else
			{
				smaller = otherBag;
				larger = this;
			}

			result = larger.Clone();
			result.SumWith(smaller);
			return result;
		}

		/// <summary>
		/// Computes the intersection of this bag with another bag. The intersection of two bags
		/// is all items that appear in both of the bags. If an item appears X times in one bag,
		/// and Y times in the other bag, the sum contains the item Minimum(X,Y) times. This bag receives
		/// the intersection of the two bags, the other bag is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both bags, the intersection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The intersection of two bags is computed in time O(N), where N is the size of the smaller bag.</para>
		/// </remarks>
		/// <param name="otherBag">Bag to intersection with.</param>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public void IntersectionWith(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			hash.StopEnumerations();

			Bag<T> smaller, larger;
			if (otherBag.Count > Count)
			{
				smaller = this;
				larger = otherBag;
			}
			else
			{
				smaller = otherBag;
				larger = this;
			}

			KeyValuePair<T, int> dummy;
			var newHash = new Hash<KeyValuePair<T, int>>(equalityComparer);
			int newCount = 0;
			int copiesInSmaller, copiesInLarger, copies;

			// Enumerate each of the items in the smaller bag. Add items that need to be
			// added to the intersection.
			foreach (T item in smaller.DistinctItems())
			{
				copiesInLarger = larger.NumberOfCopies(item);
				copiesInSmaller = smaller.NumberOfCopies(item);
				copies = Math.Min(copiesInLarger, copiesInSmaller);
				if (copies > 0)
				{
					newHash.Insert(NewPair(item, copies), true, out dummy);
					newCount += copies;
				}
			}

			hash = newHash;
			count = newCount;
		}

		/// <summary>
		/// Computes the intersection of this bag with another bag. The intersection of two bags
		/// is all items that appear in both of the bags. If an item appears X times in one bag,
		/// and Y times in the other bag, the intersection contains the item Minimum(X,Y) times. A new bag is 
		/// created with the intersection of the bags and is returned. This bag and the other bag 
		/// are unchanged.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both bags, the intersection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The intersection of two bags is computed in time O(N), where N is the size of the smaller bag.</para>
		/// </remarks>
		/// <param name="otherBag">Bag to intersection with.</param>
		/// <returns>The intersection of the two bags.</returns>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public Bag<T> Intersection(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			Bag<T> smaller, larger, result;
			if (otherBag.Count > Count)
			{
				smaller = this;
				larger = otherBag;
			}
			else
			{
				smaller = otherBag;
				larger = this;
			}

			int copiesInSmaller, copiesInLarger, copies;

			// Enumerate each of the items in the smaller bag. Add items that need to be
			// added to the intersection.
			result = new Bag<T>(keyEqualityComparer);
			foreach (T item in smaller.DistinctItems())
			{
				copiesInLarger = larger.NumberOfCopies(item);
				copiesInSmaller = smaller.NumberOfCopies(item);
				copies = Math.Min(copiesInLarger, copiesInSmaller);
				if (copies > 0)
					result.ChangeNumberOfCopies(item, copies);
			}

			return result;
		}

		/// <summary>
		/// Computes the difference of this bag with another bag. The difference of these two bags
		/// is all items that appear in this bag, but not in <paramref name="otherBag"/>. If an item appears X times in this bag,
		/// and Y times in the other bag, the difference contains the item X - Y times (zero times if Y >= X). This bag receives
		/// the difference of the two bags; the other bag is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The difference of two bags is computed in time O(M), where M is the size of the 
		/// other bag.</para>
		/// </remarks>
		/// <param name="otherBag">Bag to difference with.</param>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public void DifferenceWith(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			if (this == otherBag)
			{
				Clear();
				return;
			}

			int copiesInThis, copiesInOther, copies;

			// Enumerate each of the items in the other bag. Remove items that need to be
			// removed from this bag.
			foreach (T item in otherBag.DistinctItems())
			{
				copiesInThis = NumberOfCopies(item);
				copiesInOther = otherBag.NumberOfCopies(item);
				copies = copiesInThis - copiesInOther;
				if (copies < 0)
					copies = 0;

				ChangeNumberOfCopies(item, copies);
			}
		}

		/// <summary>
		/// Computes the difference of this bag with another bag. The difference of these two bags
		/// is all items that appear in this bag, but not in <paramref name="otherBag"/>. If an item appears X times in this bag,
		/// and Y times in the other bag, the difference contains the item X - Y times (zero times if Y >= X).  A new bag is 
		/// created with the difference of the bags and is returned. This bag and the other bag 
		/// are unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The difference of two bags is computed in time O(M + N), where M and N are the size
		/// of the two bags.</para>
		/// </remarks>
		/// <param name="otherBag">Bag to difference with.</param>
		/// <returns>The difference of the two bags.</returns>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public Bag<T> Difference(Bag<T> otherBag)
		{
			Bag<T> result;

			CheckConsistentComparison(otherBag);

			result = Clone();
			result.DifferenceWith(otherBag);
			return result;
		}

		/// <summary>
		/// Computes the symmetric difference of this bag with another bag. The symmetric difference of two bags
		/// is all items that appear in either of the bags, but not both. If an item appears X times in one bag,
		/// and Y times in the other bag, the symmetric difference contains the item AbsoluteValue(X - Y) times. This bag receives
		/// the symmetric difference of the two bags; the other bag is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The symmetric difference of two bags is computed in time O(M + N), where M is the size of the 
		/// larger bag, and N is the size of the smaller bag.</para>
		/// </remarks>
		/// <param name="otherBag">Bag to symmetric difference with.</param>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public void SymmetricDifferenceWith(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			if (this == otherBag)
			{
				Clear();
				return;
			}

			int copiesInThis, copiesInOther, copies;

			// Enumerate each of the items in the other bag. Add items that need to be
			// added to this bag.
			foreach (T item in otherBag.DistinctItems())
			{
				copiesInThis = NumberOfCopies(item);
				copiesInOther = otherBag.NumberOfCopies(item);
				copies = Math.Abs(copiesInThis - copiesInOther);

				if (copies != copiesInThis)
					ChangeNumberOfCopies(item, copies);
			}
		}

		/// <summary>
		/// Computes the symmetric difference of this bag with another bag. The symmetric difference of two bags
		/// is all items that appear in either of the bags, but not both. If an item appears X times in one bag,
		/// and Y times in the other bag, the symmetric difference contains the item AbsoluteValue(X - Y) times. A new bag is 
		/// created with the symmetric difference of the bags and is returned. This bag and the other bag 
		/// are unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The symmetric difference of two bags is computed in time O(M + N), where M is the size of the 
		/// larger bag, and N is the size of the smaller bag.</para>
		/// </remarks>
		/// <param name="otherBag">Bag to symmetric difference with.</param>
		/// <returns>The symmetric difference of the two bags.</returns>
		/// <exception cref="InvalidOperationException">This bag and <paramref name="otherBag"/> don't use the same method for comparing items.</exception>
		public Bag<T> SymmetricDifference(Bag<T> otherBag)
		{
			CheckConsistentComparison(otherBag);

			Bag<T> smaller, larger, result;
			if (otherBag.Count > Count)
			{
				smaller = this;
				larger = otherBag;
			}
			else
			{
				smaller = otherBag;
				larger = this;
			}

			result = larger.Clone();
			result.SymmetricDifferenceWith(smaller);
			return result;
		}

		/// <summary>
		/// Check that this bag and another bag were created with the same comparison
		/// mechanism. Throws exception if not compatible.
		/// </summary>
		/// <param name="otherBag">Other bag to check comparision mechanism.</param>
		/// <exception cref="InvalidOperationException">If otherBag and this bag don't use the same method for comparing items.</exception>
		void CheckConsistentComparison(Bag<T> otherBag)
		{
			if (otherBag == null)
				throw new ArgumentNullException("otherBag");

			if (!Equals(equalityComparer, otherBag.equalityComparer))
				throw new InvalidOperationException(Strings.InconsistentComparisons);
		}

		#endregion Set operations
	}
}