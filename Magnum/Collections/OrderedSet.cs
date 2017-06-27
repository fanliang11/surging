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
	/// OrderedSet&lt;T&gt; is a collection that contains items of type T. 
	/// The item are maintained in a sorted order, and duplicate items are not allowed. Each item has
	/// an index in the set: the smallest item has index 0, the next smallest item has index 1,
	/// and so forth.
	/// </summary>
	/// <remarks>
	/// <p>The items are compared in one of three ways. If T implements IComparable&lt;TKey&gt; or IComparable,
	/// then the CompareTo method of that interface will be used to compare items. Alternatively, a comparison
	/// function can be passed in either as a delegate, or as an instance of IComparer&lt;TKey&gt;.</p>
	/// <p>OrderedSet is implemented as a balanced binary tree. Inserting, deleting, and looking up an
	/// an element all are done in log(N) type, where N is the number of keys in the tree.</p>
	/// <p><see cref="Set&lt;T&gt;"/> is similar, but uses hashing instead of comparison, and does not maintain
	/// the items in sorted order.</p>
	///</remarks>
	///<seealso cref="Set&lt;T&gt;"/>
	[Serializable]
	public class OrderedSet<T> : CollectionBase<T>,
		ICollection<T>,
		ICloneable
	{
		// The comparer used to compare items. 
		private readonly IComparer<T> comparer;

		// The red-black tree that actually does the work of storing the items.
		private RedBlackTree<T> tree;

		#region Constructors

		/// <summary>
		/// Creates a new OrderedSet. The T must implement IComparable&lt;T&gt;
		/// or IComparable. 
		/// The CompareTo method of this interface will be used to compare items in this set.
		/// </summary>
		///<remarks>
		/// Items that are null are permitted, and will be sorted before all other items.
		///</remarks>
		/// <exception cref="InvalidOperationException">T does not implement IComparable&lt;TKey&gt;.</exception>
		public OrderedSet()
			:
				this(Comparers.DefaultComparer<T>())
		{
		}

		/// <summary>
		/// Creates a new OrderedSet. The passed delegate will be used to compare items in this set.
		/// </summary>
		/// <param name="comparison">A delegate to a method that will be used to compare items.</param>
		public OrderedSet(Comparison<T> comparison)
			:
				this(Comparers.ComparerFromComparison(comparison))
		{
		}

		/// <summary>
		/// Creates a new OrderedSet. The Compare method of the passed comparison object
		/// will be used to compare items in this set.
		/// </summary>
		/// <remarks>
		/// The GetHashCode and Equals methods of the provided IComparer&lt;T&gt; will never
		/// be called, and need not be implemented.
		/// </remarks>
		/// <param name="comparer">An instance of IComparer&lt;T&gt; that will be used to compare items.</param>
		public OrderedSet(IComparer<T> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			this.comparer = comparer;
			tree = new RedBlackTree<T>(comparer);
		}

		/// <summary>
		/// Creates a new OrderedSet. The T must implement IComparable&lt;T&gt;
		/// or IComparable. 
		/// The CompareTo method of this interface will be used to compare items in this set. The set is
		/// initialized with all the items in the given collection.
		/// </summary>
		///<remarks>
		/// Items that are null are permitted, and will be sorted before all other items.
		///</remarks>
		/// <param name="collection">A collection with items to be placed into the OrderedSet.</param>
		/// <exception cref="InvalidOperationException">T does not implement IComparable&lt;TKey&gt;.</exception>
		public OrderedSet(IEnumerable<T> collection)
			:
				this(collection, Comparers.DefaultComparer<T>())
		{
		}

		/// <summary>
		/// Creates a new OrderedSet. The passed delegate will be used to compare items in this set.
		/// The set is initialized with all the items in the given collection.
		/// </summary>
		/// <param name="collection">A collection with items to be placed into the OrderedSet.</param>
		/// <param name="comparison">A delegate to a method that will be used to compare items.</param>
		public OrderedSet(IEnumerable<T> collection, Comparison<T> comparison)
			:
				this(collection, Comparers.ComparerFromComparison(comparison))
		{
		}

		/// <summary>
		/// Creates a new OrderedSet. The Compare method of the passed comparison object
		/// will be used to compare items in this set. The set is
		/// initialized with all the items in the given collection.
		/// </summary>
		/// <remarks>
		/// The GetHashCode and Equals methods of the provided IComparer&lt;T&gt; will never
		/// be called, and need not be implemented.
		/// </remarks>
		/// <param name="collection">A collection with items to be placed into the OrderedSet.</param>
		/// <param name="comparer">An instance of IComparer&lt;T&gt; that will be used to compare items.</param>
		public OrderedSet(IEnumerable<T> collection, IComparer<T> comparer)
			:
				this(comparer)
		{
			AddMany(collection);
		}

		/// <summary>
		/// Creates a new OrderedSet given a comparer and a tree that contains the data. Used
		/// internally for Clone.
		/// </summary>
		/// <param name="comparer">Comparer for the set.</param>
		/// <param name="tree">Data for the set.</param>
		private OrderedSet(IComparer<T> comparer, RedBlackTree<T> tree)
		{
			this.comparer = comparer;
			this.tree = tree;
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
		public OrderedSet<T> Clone()
		{
			OrderedSet<T> newSet = new OrderedSet<T>(comparer, tree.Clone());
			return newSet;
		}

		/// <summary>
		/// Makes a deep clone of this set. A new set is created with a clone of
		/// each element of this set, by calling ICloneable.Clone on each element. If T is
		/// a value type, then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks><para>If T is a reference type, it must implement
		/// ICloneable. Otherwise, an InvalidOperationException is thrown.</para>
		/// <para>Cloning the set takes time O(N log N), where N is the number of items in the set.</para></remarks>
		/// <returns>The cloned set.</returns>
		/// <exception cref="InvalidOperationException">T is a reference type that does not implement ICloneable.</exception>
		public OrderedSet<T> CloneContents()
		{
			bool itemIsValueType;
			if (!Util.IsCloneableType(typeof (T), out itemIsValueType))
				throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, typeof (T).FullName));

			OrderedSet<T> clone = new OrderedSet<T>(comparer);

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
		/// Returns the IComparer&lt;T&gt; used to compare items in this set. 
		/// </summary>
		/// <value>If the set was created using a comparer, that comparer is returned. If the set was
		/// created using a comparison delegate, then a comparer equivalent to that delegate
		/// is returned. Otherwise
		/// the default comparer for T (Comparer&lt;T&gt;.Default) is returned.</value>
		public IComparer<T> Comparer
		{
			get { return this.comparer; }
		}

		/// <summary>
		/// Returns the number of items in the set.
		/// </summary>
		/// <remarks>The size of the set is returned in constant time.</remarks>
		/// <value>The number of items in the set.</value>
		public override sealed int Count
		{
			get { return tree.ElementCount; }
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
		/// <p>Enumeration all the items in the set takes time O(N log N), where N is the number
		/// of items in the set.</p>
		/// </remarks>
		/// <returns>An enumerator for enumerating all the items in the OrderedSet.</returns>		
		public override sealed IEnumerator<T> GetEnumerator()
		{
			return tree.GetEnumerator();
		}

		/// <summary>
		/// Determines if this set contains an item equal to <paramref name="item"/>. The set
		/// is not changed.
		/// </summary>
		/// <remarks>Searching the set for an item takes time O(log N), where N is the number of items in the set.</remarks>
		/// <param name="item">The item to search for.</param>
		/// <returns>True if the set contains <paramref name="item"/>. False if the set does not contain <paramref name="item"/>.</returns>
		public override sealed bool Contains(T item)
		{
			T dummy;
			return tree.Find(item, false, false, out dummy);
		}

		/// <summary>
		/// <para>Determines if this set contains an item equal to <paramref name="item"/>, according to the 
		/// comparison mechanism that was used when the set was created. The set
		/// is not changed.</para>
		/// <para>If the set does contain an item equal to <paramref name="item"/>, then the item from the set is returned.</para>
		/// </summary>
		/// <remarks>Searching the set for an item takes time O(log N), where N is the number of items in the set.</remarks>
		/// <example>
		/// In the following example, the set contains strings which are compared in a case-insensitive manner. 
		/// <code>
		/// OrderedSet&lt;string&gt; set = new OrderedSet&lt;string&gt;(StringComparer.CurrentCultureIgnoreCase);
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
			return tree.Find(item, true, false, out foundItem);
		}

		#endregion

		#region Index by sorted order

		/// <summary>
		/// Get the item by its index in the sorted order. The smallest item has index 0,
		/// the next smallest item has index 1, and the largest item has index Count-1. 
		/// </summary>
		/// <remarks>The indexer takes time O(log N), which N is the number of items in 
		/// the set.</remarks>
		/// <param name="index">The index to get the item by.</param>
		/// <returns>The item at the given index.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than or equal to Count.</exception>
		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException("index");

				return tree.GetItemByIndex(index);
			}
		}

		/// <summary>
		/// Get the index of the given item in the sorted order. The smallest item has index 0,
		/// the next smallest item has index 1, and the largest item has index Count-1. 
		/// </summary>
		/// <remarks>Finding the index takes time O(log N), which N is the number of items in 
		/// the set.</remarks>
		/// <param name="item">The item to get the index of.</param>
		/// <returns>The index of the item in the sorted set, or -1 if the item is not present
		/// in the set.</returns>
		public int IndexOf(T item)
		{
			return tree.FindIndex(item, true);
		}

		#endregion

		#region Adding elements

		/// <summary>
		/// Adds a new item to the set. If the set already contains an item equal to
		/// <paramref name="item"/>, that item is replaces with <paramref name="item"/>.
		/// </summary>
		/// <remarks>
		/// <para>Equality between items is determined by the comparison instance or delegate used
		/// to create the set.</para>
		/// <para>Adding an item takes time O(log N), where N is the number of items in the set.</para></remarks>
		/// <param name="item">The item to add to the set.</param>
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
		/// <para>Adding an item takes time O(log N), where N is the number of items in the set.</para></remarks>
		/// <param name="item">The item to add to the set.</param>
		/// <returns>True if the set already contained an item equal to <paramref name="item"/> (which was replaced), false 
		/// otherwise.</returns>
		public new bool Add(T item)
		{
			T dummy;
			return ! tree.Insert(item, DuplicatePolicy.ReplaceFirst, out dummy);
		}

		/// <summary>
		/// Adds all the items in <paramref name="collection"/> to the set. If the set already contains an item equal to
		/// one of the items in <paramref name="collection"/>, that item will be replaced.
		/// </summary>
		/// <remarks>
		/// <para>Equality between items is determined by the comparison instance or delegate used
		/// to create the set.</para>
		/// <para>Adding the collection takes time O(M log N), where N is the number of items in the set, and M is the 
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
		/// <para>Removing an item from the set takes time O(log N), where N is the number of items in the set.</para></remarks>
		/// <param name="item">The item to remove.</param>
		/// <returns>True if <paramref name="item"/> was found and removed. False if <paramref name="item"/> was not in the set.</returns>
		public override sealed bool Remove(T item)
		{
			T dummy;
			return tree.Delete(item, true, out dummy);
		}

		/// <summary>
		/// Removes all items from the set.
		/// </summary>
		/// <remarks>Clearing the sets takes a constant amount of time, regardless of the number of items in it.</remarks>
		public override sealed void Clear()
		{
			tree.StopEnumerations(); // Invalidate any enumerations.

			// The simplest and fastest way is simply to throw away the old tree and create a new one.
			tree = new RedBlackTree<T>(comparer);
		}

		/// <summary>
		/// Removes all the items in <paramref name="collection"/> from the set. Items
		/// not present in the set are ignored.
		/// </summary>
		/// <remarks>
		/// <para>Equality between items is determined by the comparison instance or delegate used
		/// to create the set.</para>
		/// <para>Removing the collection takes time O(M log N), where N is the number of items in the set, and M is the 
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

		#region First/last items

		/// <summary>
		/// Returns the first item in the set: the item
		/// that would appear first if the set was enumerated. This is also
		/// the smallest item in the set.
		/// </summary>
		/// <remarks>GetFirst() takes time O(log N), where N is the number of items in the set.</remarks>
		/// <returns>The first item in the set. </returns>
		/// <exception cref="InvalidOperationException">The set is empty.</exception>
		public T GetFirst()
		{
			T item;
			CheckEmpty();
			tree.FirstItemInRange(tree.EntireRangeTester, out item);
			return item;
		}

		/// <summary>
		/// Returns the lastl item in the set: the item
		/// that would appear last if the set was enumerated. This is also the
		/// largest item in the set.
		/// </summary>
		/// <remarks>GetLast() takes time O(log N), where N is the number of items in the set.</remarks>
		/// <returns>The lastl item in the set. </returns>
		/// <exception cref="InvalidOperationException">The set is empty.</exception>
		public T GetLast()
		{
			T item;
			CheckEmpty();
			tree.LastItemInRange(tree.EntireRangeTester, out item);
			return item;
		}

		/// <summary>
		/// Removes the first item in the set. This is also the smallest item in the set.
		/// </summary>
		/// <remarks>RemoveFirst() takes time O(log N), where N is the number of items in the set.</remarks>
		/// <returns>The item that was removed, which was the smallest item in the set. </returns>
		/// <exception cref="InvalidOperationException">The set is empty.</exception>
		public T RemoveFirst()
		{
			CheckEmpty();
			T item;
			tree.DeleteItemFromRange(tree.EntireRangeTester, true, out item);
			return item;
		}

		/// <summary>
		/// Removes the last item in the set. This is also the largest item in the set.
		/// </summary>
		/// <remarks>RemoveLast() takes time O(log N), where N is the number of items in the set.</remarks>
		/// <returns>The item that was removed, which was the largest item in the set. </returns>
		/// <exception cref="InvalidOperationException">The set is empty.</exception>
		public T RemoveLast()
		{
			CheckEmpty();
			T item;
			tree.DeleteItemFromRange(tree.EntireRangeTester, false, out item);
			return item;
		}

		/// <summary>
		/// If the collection is empty, throw an invalid operation exception.
		/// </summary>
		/// <exception cref="InvalidOperationException">The set is empty.</exception>
		private void CheckEmpty()
		{
			if (Count == 0)
				throw new InvalidOperationException(Strings.CollectionIsEmpty);
		}

		#endregion

		#region Set operations

		/// <summary>
		/// Determines if this set is a superset of another set. Neither set is modified.
		/// This set is a superset of <paramref name="otherSet"/> if every element in
		/// <paramref name="otherSet"/> is also in this set.
		/// <remarks>IsSupersetOf is computed in time O(M log N), where M is the size of the 
		/// <paramref name="otherSet"/>, and N is the size of the this set.</remarks>
		/// </summary>
		/// <param name="otherSet">OrderedSet to compare to.</param>
		/// <returns>True if this is a superset of <paramref name="otherSet"/>.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public bool IsSupersetOf(OrderedSet<T> otherSet)
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
		/// <remarks>IsProperSupersetOf is computed in time O(M log N), where M is the number of unique items in 
		/// <paramref name="otherSet"/>.</remarks>
		/// <param name="otherSet">OrderedSet to compare to.</param>
		/// <returns>True if this is a proper superset of <paramref name="otherSet"/>.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public bool IsProperSupersetOf(OrderedSet<T> otherSet)
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
		/// <remarks>IsSubsetOf is computed in time O(N log M), where M is the size of the 
		/// <paramref name="otherSet"/>, and N is the size of the this set.</remarks>
		/// <param name="otherSet">Set to compare to.</param>
		/// <returns>True if this is a subset of <paramref name="otherSet"/>.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public bool IsSubsetOf(OrderedSet<T> otherSet)
		{
			return otherSet.IsSupersetOf(this);
		}


		/// <summary>
		/// Determines if this set is a proper subset of another set. Neither set is modified.
		/// This set is a subset of <paramref name="otherSet"/> if every element in this set
		/// is also in <paramref name="otherSet"/>. Additionally, this set must have strictly 
		/// fewer items than <paramref name="otherSet"/>.
		/// </summary>
		/// <remarks>IsSubsetOf is computed in time O(N log M), where M is the size of the 
		/// <paramref name="otherSet"/>, and N is the size of the this set.</remarks>
		/// <param name="otherSet">Set to compare to.</param>
		/// <returns>True if this is a proper subset of <paramref name="otherSet"/>.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public bool IsProperSubsetOf(OrderedSet<T> otherSet)
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
		public bool IsEqualTo(OrderedSet<T> otherSet)
		{
			CheckConsistentComparison(otherSet);

			// Must be the same size.
			if (otherSet.Count != this.Count)
				return false;

			// Since both sets are ordered, we can simply compare items in order.
			using (IEnumerator<T> enum1 = this.GetEnumerator(), enum2 = otherSet.GetEnumerator())
			{
				bool continue1, continue2;

				for (;;)
				{
					continue1 = enum1.MoveNext();
					continue2 = enum2.MoveNext();
					if (!continue1 || !continue2)
						break;

					if (comparer.Compare(enum1.Current, enum2.Current) != 0)
						return false; // the two items are not equal.
				}

				// If both continue1 and continue2 are false, we reached the end of both sequences at the same
				// time and found success. If one is true and one is false, the sequences were of difference lengths -- failure.
				return (continue1 == continue2);
			}
		}

		/// <summary>
		/// Computes the union of this set with another set. The union of two sets
		/// is all items that appear in either or both of the sets. This set receives
		/// the union of the two sets, the other set is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>If equal items appear in both sets, the union will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The union of two sets is computed in time O(M + N log M), where M is the size of the 
		/// larger set, and N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to union with.</param>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public void UnionWith(OrderedSet<T> otherSet)
		{
			CheckConsistentComparison(otherSet);

			AddMany(otherSet);

			// CONSIDER: if RedBlackTree cloning is O(N), then if otherSet is much larger, better to clone it,
			// add all of the current into it, and replace.
		}

		/// <summary>
		/// Determines if this set is disjoint from another set. Two sets are disjoint
		/// if no item from one set is equal to any item in the other set.
		/// </summary>
		/// <remarks>
		/// <para>The answer is computed in time O(N log M), where M is the size of the 
		/// larger set, and N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to check disjointness with.</param>
		/// <returns>True if the two sets are disjoint, false otherwise.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public bool IsDisjointFrom(OrderedSet<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			OrderedSet<T> smaller, larger;
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
		/// is all items that appear in either or both of the sets. A new set is 
		/// created with the union of the sets and is returned. This set and the other set 
		/// are unchanged.
		/// </summary>
		/// <remarks>
		/// <para>If equal items appear in both sets, the union will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The union of two sets is computed in time O(M + N log M), where M is the size of the 
		/// larger set, and N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to union with.</param>
		/// <returns>The union of the two sets.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public OrderedSet<T> Union(OrderedSet<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			OrderedSet<T> smaller, larger, result;
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
		/// <para>The intersection of two sets is computed in time O(N log M), where M is the size of the 
		/// larger set, and N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to intersection with.</param>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public void IntersectionWith(OrderedSet<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			tree.StopEnumerations();

			OrderedSet<T> smaller, larger;
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
			RedBlackTree<T> newTree = new RedBlackTree<T>(comparer);

			foreach (T item in smaller)
			{
				if (larger.Contains(item))
					newTree.Insert(item, DuplicatePolicy.ReplaceFirst, out dummy);
			}

			tree = newTree;
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
		/// <para>The intersection of two sets is computed in time O(N log M), where M is the size of the 
		/// larger set, and N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to intersection with.</param>
		/// <returns>The intersection of the two sets.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public OrderedSet<T> Intersection(OrderedSet<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			OrderedSet<T> smaller, larger, result;
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

			result = new OrderedSet<T>(comparer);
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
		/// <para>The difference of two sets is computed in time O(M + N log M), where M is the size of the 
		/// larger set, and N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to difference with.</param>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public void DifferenceWith(OrderedSet<T> otherSet)
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
		/// <para>The difference of two sets is computed in time O(M + N log M), where M is the size of the 
		/// larger set, and N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to difference with.</param>
		/// <returns>The difference of the two sets.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public OrderedSet<T> Difference(OrderedSet<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			OrderedSet<T> result = this.Clone();
			result.DifferenceWith(otherSet);
			return result;
		}

		/// <summary>
		/// Computes the symmetric difference of this set with another set. The symmetric difference of two sets
		/// is all items that appear in either of the sets, but not both. This set receives
		/// the symmetric difference of the two sets; the other set is unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The symmetric difference of two sets is computed in time O(M + N log M), where M is the size of the 
		/// larger set, and N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to symmetric difference with.</param>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public void SymmetricDifferenceWith(OrderedSet<T> otherSet)
		{
			// Symmetric difference with myself is nothing. This check is needed because the
			// main algorithm doesn't work correctly otherwise.
			if (this == otherSet)
				Clear();

			CheckConsistentComparison(otherSet);

			// CONSIDER: if otherSet is larger, better to clone it and reverse the below?
			foreach (T item in otherSet)
			{
				if (this.Contains(item))
					this.Remove(item);
				else
					this.Add(item);
			}
		}

		/// <summary>
		/// Computes the symmetric difference of this set with another set. The symmetric difference of two sets
		/// is all items that appear in either of the sets, but not both. A new set is 
		/// created with the symmetric difference of the sets and is returned. This set and the other set 
		/// are unchanged.
		/// </summary>
		/// <remarks>
		/// <para>The symmetric difference of two sets is computed in time O(M + N log M), where M is the size of the 
		/// larger set, and N is the size of the smaller set.</para>
		/// </remarks>
		/// <param name="otherSet">Set to symmetric difference with.</param>
		/// <returns>The symmetric difference of the two sets.</returns>
		/// <exception cref="InvalidOperationException">This set and <paramref name="otherSet"/> don't use the same method for comparing items.</exception>
		public OrderedSet<T> SymmetricDifference(OrderedSet<T> otherSet)
		{
			CheckConsistentComparison(otherSet);
			OrderedSet<T> smaller, larger, result;
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
		private void CheckConsistentComparison(OrderedSet<T> otherSet)
		{
			if (otherSet == null)
				throw new ArgumentNullException("otherSet");

			if (!Equals(comparer, otherSet.comparer))
				throw new InvalidOperationException(Strings.InconsistentComparisons);
		}

		#endregion Set operations

		#region Read-only list view

		/// <summary>
		/// Get a read-only list view of the items in this ordered set. The
		/// items in the list are in sorted order, with the smallest item
		/// at index 0. This view does not copy any data, and reflects any
		/// changes to the underlying OrderedSet.
		/// </summary>
		/// <returns>A read-only IList&lt;T&gt; view onto this OrderedSet.</returns>
		public IList<T> AsList()
		{
			return new ListView(this, tree.EntireRangeTester, true, false);
		}

		/// <summary>
		/// The nested class that provides a read-only list view
		/// of all or part of the collection.
		/// </summary>
		[Serializable]
		private class ListView : ReadOnlyListBase<T>
		{
			private readonly bool entireTree; // is the view the whole tree?
			private readonly OrderedSet<T> mySet;
			private readonly RedBlackTree<T>.RangeTester rangeTester; // range tester for the range being used.
			private readonly bool reversed; // is the view reversed?

			/// <summary>
			/// Create a new list view wrapped the given set.
			/// </summary>
			/// <param name="mySet"></param>
			/// <param name="rangeTester">Range tester that defines the range being used.</param>
			/// <param name="entireTree">If true, then rangeTester defines the entire tree. Used to optimize some operations.</param>
			/// <param name="reversed">Is the view enuemerated in reverse order?</param>
			public ListView(OrderedSet<T> mySet, RedBlackTree<T>.RangeTester rangeTester, bool entireTree, bool reversed)
			{
				this.mySet = mySet;
				this.rangeTester = rangeTester;
				this.entireTree = entireTree;
				this.reversed = reversed;
			}

			public override int Count
			{
				get
				{
					if (entireTree)
						return mySet.Count;
					else
					{
						// Note: we can't cache the result of this call because the underlying
						// set can change, which would make the cached value incorrect.
						return mySet.tree.CountRange(rangeTester);
					}
				}
			}

			public override T this[int index]
			{
				get
				{
					if (entireTree)
					{
						if (reversed)
							return mySet[mySet.Count - 1 - index];
						else
							return mySet[index];
					}
					else
					{
						T dummy;
						int firstIndex = mySet.tree.FirstItemInRange(rangeTester, out dummy);
						int lastIndex = mySet.tree.LastItemInRange(rangeTester, out dummy);
						if (firstIndex < 0 || lastIndex < 0 || index < 0 || index >= (lastIndex - firstIndex + 1))
							throw new ArgumentOutOfRangeException("index");

						if (reversed)
							return mySet[lastIndex - index];
						else
							return mySet[firstIndex + index];
					}
				}
			}

			public override int IndexOf(T item)
			{
				if (entireTree)
				{
					if (reversed)
						return mySet.Count - 1 - mySet.IndexOf(item);
					else
						return mySet.IndexOf(item);
				}
				else
				{
					T dummy;

					if (rangeTester(item) != 0)
						return -1;

					if (reversed)
					{
						int indexInSet = mySet.tree.FindIndex(item, false);
						if (indexInSet < 0)
							return -1;
						int indexOfEnd = mySet.tree.LastItemInRange(rangeTester, out dummy);
						return indexOfEnd - indexInSet;
					}
					else
					{
						int indexInSet = mySet.tree.FindIndex(item, true);
						if (indexInSet < 0)
							return -1;
						int indexOfStart = mySet.tree.FirstItemInRange(rangeTester, out dummy);
						return indexInSet - indexOfStart;
					}
				}
			}
		}

		#endregion Read-only list view

		#region Sub-views

		/// <summary>
		/// Returns a View collection that can be used for enumerating the items in the set in 
		/// reversed order.
		/// </summary>
		///<remarks>
		///<p>Typically, this method is used in conjunction with a foreach statement. For example:
		///<code>
		/// foreach(T item in set.Reversed()) {
		///    // process item
		/// }
		///</code></p>
		/// <p>If an item is added to or deleted from the set while the View is being enumerated, then 
		/// the enumeration will end with an InvalidOperationException.</p>
		///<p>Calling Reverse does not copy the data in the tree, and the operation takes constant time.</p>
		///</remarks>
		/// <returns>An OrderedSet.View of items in reverse order.</returns>
		public View Reversed() // A reversed view that can be enumerated
		{
			return new View(this, tree.EntireRangeTester, true, true);
		}

		/// <summary>
		/// Returns a View collection that can be used for enumerating a range of the items in the set..
		/// Only items that are greater than <paramref name="from"/> and 
		/// less than <paramref name="to"/> are included. The items are enumerated in sorted order.
		/// Items equal to the end points of the range can be included or excluded depending on the
		/// <paramref name="fromInclusive"/> and <paramref name="toInclusive"/> parameters.
		/// </summary>
		///<remarks>
		///<p>If <paramref name="from"/> is greater than <paramref name="to"/>, the returned collection is empty. </p>
		///<p>Typically, this method is used in conjunction with a foreach statement. For example:
		///<code>
		/// foreach(T item in set.Range(from, true, to, false)) {
		///    // process item
		/// }
		///</code></p>
		/// <p>If an item is added to or deleted from the set while the View is being enumerated, then 
		/// the enumeration will end with an InvalidOperationException.</p>
		///<p>Calling Range does not copy the data in the tree, and the operation takes constant time.</p>
		///</remarks>
		/// <param name="from">The lower bound of the range.</param>
		/// <param name="fromInclusive">If true, the lower bound is inclusive--items equal to the lower bound will
		/// be included in the range. If false, the lower bound is exclusive--items equal to the lower bound will not
		/// be included in the range.</param>
		/// <param name="to">The upper bound of the range. </param>
		/// <param name="toInclusive">If true, the upper bound is inclusive--items equal to the upper bound will
		/// be included in the range. If false, the upper bound is exclusive--items equal to the upper bound will not
		/// be included in the range.</param>
		/// <returns>An OrderedSet.View of items in the given range.</returns>
		public View Range(T from, bool fromInclusive, T to, bool toInclusive) // A partial view that can be enumerated
		{
			return new View(this, tree.DoubleBoundedRangeTester(from, fromInclusive, to, toInclusive), false, false);
		}

		/// <summary>
		/// Returns a View collection that can be used for enumerating a range of the items in the set..
		/// Only items that are greater than (and optionally, equal to) <paramref name="from"/> are included. 
		/// The items are enumerated in sorted order. Items equal to <paramref name="from"/> can be included
		/// or excluded depending on the <paramref name="fromInclusive"/> parameter.
		/// </summary>
		///<remarks>
		///<p>Typically, this method is used in conjunction with a foreach statement. For example:
		///<code>
		/// foreach(T item in set.RangeFrom(from, true)) {
		///    // process item
		/// }
		///</code></p>
		/// <p>If an item is added to or deleted from the set while the View is being enumerated, then 
		/// the enumeration will end with an InvalidOperationException.</p>
		///<p>Calling RangeFrom does not copy the data in the tree, and the operation takes constant time.</p>
		///</remarks>
		/// <param name="from">The lower bound of the range.</param>
		/// <param name="fromInclusive">If true, the lower bound is inclusive--items equal to the lower bound will
		/// be included in the range. If false, the lower bound is exclusive--items equal to the lower bound will not
		/// be included in the range.</param>
		/// <returns>An OrderedSet.View of items in the given range.</returns>
		public View RangeFrom(T from, bool fromInclusive) // A partial view that can be enumerated
		{
			return new View(this, tree.LowerBoundedRangeTester(from, fromInclusive), false, false);
		}

		/// <summary>
		/// Returns a View collection that can be used for enumerating a range of the items in the set..
		/// Only items that are less than (and optionally, equal to) <paramref name="to"/> are included. 
		/// The items are enumerated in sorted order. Items equal to <paramref name="to"/> can be included
		/// or excluded depending on the <paramref name="toInclusive"/> parameter.
		/// </summary>
		///<remarks>
		///<p>Typically, this method is used in conjunction with a foreach statement. For example:
		///<code>
		/// foreach(T item in set.RangeTo(to, false)) {
		///    // process item
		/// }
		///</code></p>
		/// <p>If an item is added to or deleted from the set while the View is being enumerated, then 
		/// the enumeration will end with an InvalidOperationException.</p>
		///<p>Calling RangeTo does not copy the data in the tree, and the operation takes constant time.</p>
		///</remarks>
		/// <param name="to">The upper bound of the range. </param>
		/// <param name="toInclusive">If true, the upper bound is inclusive--items equal to the upper bound will
		/// be included in the range. If false, the upper bound is exclusive--items equal to the upper bound will not
		/// be included in the range.</param>
		/// <returns>An OrderedSet.View of items in the given range.</returns>
		public View RangeTo(T to, bool toInclusive) // A partial view that can be enumerated
		{
			return new View(this, tree.UpperBoundedRangeTester(to, toInclusive), false, false);
		}

		#endregion

		#region View nested class

		/// <summary>
		/// The OrderedSet&lt;T&gt;.View class is used to look at a subset of the Items
		/// inside an ordered set. It is returned from the Range, RangeTo, RangeFrom, and Reversed methods. 
		/// </summary>
		///<remarks>
		/// <p>Views are dynamic. If the underlying set changes, the view changes in sync. If a change is made
		/// to the view, the underlying set changes accordingly.</p>
		///<p>Typically, this class is used in conjunction with a foreach statement to enumerate the items 
		/// in a subset of the OrderedSet. For example:</p>
		///<code>
		/// foreach(T item in set.Range(from, to)) {
		///    // process item
		/// }
		///</code>
		///</remarks>
		[Serializable]
		public class View : CollectionBase<T>,
			ICollection<T>
		{
			private readonly bool entireTree; // is the view the whole tree?
			private readonly OrderedSet<T> mySet;
			private readonly RedBlackTree<T>.RangeTester rangeTester; // range tester for the range being used.
			private readonly bool reversed; // is the view reversed?

			/// <summary>
			/// Initialize the view.
			/// </summary>
			/// <param name="mySet">OrderedSet being viewed</param>
			/// <param name="rangeTester">Range tester that defines the range being used.</param>
			/// <param name="entireTree">If true, then rangeTester defines the entire tree. Used to optimize some operations.</param>
			/// <param name="reversed">Is the view enuemerated in reverse order?</param>
			internal View(OrderedSet<T> mySet, RedBlackTree<T>.RangeTester rangeTester, bool entireTree, bool reversed)
			{
				this.mySet = mySet;
				this.rangeTester = rangeTester;
				this.entireTree = entireTree;
				this.reversed = reversed;
			}

			/// <summary>
			/// Get the item by its index in the sorted order. The smallest item in the view has index 0,
			/// the next smallest item has index 1, and the largest item has index Count-1. 
			/// </summary>
			/// <remarks>The indexer takes time O(log N), which N is the number of items in 
			/// the set.</remarks>
			/// <param name="index">The index to get the item by.</param>
			/// <returns>The item at the given index.</returns>
			/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
			/// less than zero or greater than or equal to Count.</exception>
			public T this[int index]
			{
				get
				{
					if (entireTree)
					{
						if (reversed)
						{
							return mySet[mySet.Count - 1 - index];
						}
						else
						{
							return mySet[index];
						}
					}
					else
					{
						T dummy;
						int firstIndex = mySet.tree.FirstItemInRange(rangeTester, out dummy);
						int lastIndex = mySet.tree.LastItemInRange(rangeTester, out dummy);
						if (firstIndex < 0 || lastIndex < 0 || index < 0 || index >= (lastIndex - firstIndex + 1))
							throw new ArgumentOutOfRangeException("index");

						if (reversed)
							return mySet[lastIndex - index];
						else
							return mySet[firstIndex + index];
					}
				}
			}

			/// <summary>
			/// Enumerate all the items in this view.
			/// </summary>
			/// <returns>An IEnumerator&lt;T&gt; with the items in this view.</returns>
			public override sealed IEnumerator<T> GetEnumerator()
			{
				if (reversed)
					return mySet.tree.EnumerateRangeReversed(rangeTester).GetEnumerator();
				else
					return mySet.tree.EnumerateRange(rangeTester).GetEnumerator();
			}

			/// <summary>
			/// Number of items in this view.
			/// </summary>
			/// <value>Number of items that lie within the bounds the view.</value>
			public override sealed int Count
			{
				get
				{
					if (entireTree)
						return mySet.Count;
					else
					{
						// Note: we can't cache the result of this call because the underlying
						// set can change, which would make the cached value incorrect.
						return mySet.tree.CountRange(rangeTester);
					}
				}
			}

			/// <summary>
			/// Removes all the items within this view from the underlying set.
			/// </summary>
			/// <example>The following removes all the items that start with "A" from an OrderedSet.
			/// <code>
			/// set.Range("A", "B").Clear();
			/// </code>
			/// </example>
			public override sealed void Clear()
			{
				if (entireTree)
				{
					mySet.Clear(); // much faster than DeleteRange
				}
				else
				{
					mySet.tree.DeleteRange(rangeTester);
				}
			}

			/// <summary>
			/// Adds a new item to the set underlying this View. If the set already contains an item equal to
			/// <paramref name="item"/>, that item is replaces with <paramref name="item"/>. If
			/// <paramref name="item"/> is outside the range of this view, an InvalidOperationException
			/// is thrown.
			/// </summary>
			/// <remarks>
			/// <para>Equality between items is determined by the comparison instance or delegate used
			/// to create the set.</para>
			/// <para>Adding an item takes time O(log N), where N is the number of items in the set.</para></remarks>
			/// <param name="item">The item to add.</param>
			void ICollection<T>.Add(T item)
			{
				Add(item);
			}

			/// <summary>
			/// Searches the underlying set for an item equal to <paramref name="item"/>, and if found,
			/// removes it from the set. If not found, the set is unchanged. If the item is outside
			/// the range of this view, the set is unchanged.
			/// </summary>
			/// <remarks>
			/// <para>Equality between items is determined by the comparison instance or delegate used
			/// to create the set.</para>
			/// <para>Removing an item from the set takes time O(log N), where N is the number of items in the set.</para></remarks>
			/// <param name="item">The item to remove.</param>
			/// <returns>True if <paramref name="item"/> was found and removed. False if <paramref name="item"/> was not in the set, or
			/// was outside the range of this view.</returns>
			public override sealed bool Remove(T item)
			{
				if (!ItemInView(item))
					return false;
				else
					return mySet.Remove(item);
			}

			/// <summary>
			/// Determines if this view of the set contains an item equal to <paramref name="item"/>. The set
			/// is not changed. If 
			/// </summary>
			/// <remarks>Searching the set for an item takes time O(log N), where N is the number of items in the set.</remarks>
			/// <param name="item">The item to search for.</param>
			/// <returns>True if the set contains <paramref name="item"/>, and <paramref name="item"/> is within
			/// the range of this view. False otherwise.</returns>
			public override sealed bool Contains(T item)
			{
				if (!ItemInView(item))
					return false;
				else
					return mySet.Contains(item);
			}

			/// <summary>
			/// Adds a new item to the set underlying this View. If the set already contains an item equal to
			/// <paramref name="item"/>, that item is replaces with <paramref name="item"/>. If
			/// <paramref name="item"/> is outside the range of this view, an InvalidOperationException
			/// is thrown.
			/// </summary>
			/// <remarks>
			/// <para>Equality between items is determined by the comparison instance or delegate used
			/// to create the set.</para>
			/// <para>Adding an item takes time O(log N), where N is the number of items in the set.</para></remarks>
			/// <param name="item">The item to add.</param>
			/// <returns>True if the set already contained an item equal to <paramref name="item"/> (which was replaced), false 
			/// otherwise.</returns>
			public new bool Add(T item)
			{
				if (!ItemInView(item))
					throw new ArgumentException(Strings.OutOfViewRange, "item");
				else
					return mySet.Add(item);
			}

			/// <summary>
			/// Get the index of the given item in the view. The smallest item in the view has index 0,
			/// the next smallest item has index 1, and the largest item has index Count-1. 
			/// </summary>
			/// <remarks>Finding the index takes time O(log N), which N is the number of items in 
			/// the set.</remarks>
			/// <param name="item">The item to get the index of.</param>
			/// <returns>The index of the item in the view, or -1 if the item is not present
			/// in the view.</returns>
			public int IndexOf(T item)
			{
				if (entireTree)
				{
					if (reversed)
					{
						int indexInSet = mySet.tree.FindIndex(item, false);
						if (indexInSet < 0)
							return -1;

						return mySet.Count - 1 - indexInSet;
					}
					else
					{
						return mySet.tree.FindIndex(item, true);
					}
				}
				else
				{
					T dummy;

					if (!ItemInView(item))
						return -1;

					if (reversed)
					{
						int indexInSet = mySet.tree.FindIndex(item, false);
						if (indexInSet < 0)
							return -1;
						int indexOfEnd = mySet.tree.LastItemInRange(rangeTester, out dummy);
						return indexOfEnd - indexInSet;
					}
					else
					{
						int indexInSet = mySet.tree.FindIndex(item, true);
						if (indexInSet < 0)
							return -1;
						int indexOfStart = mySet.tree.FirstItemInRange(rangeTester, out dummy);
						return indexInSet - indexOfStart;
					}
				}
			}

			/// <summary>
			/// Get a read-only list view of the items in this view. The
			/// items in the list are in sorted order, with the smallest item
			/// at index 0. This view does not copy any data, and reflects any
			/// changes to the underlying OrderedSet.
			/// </summary>
			/// <returns>A read-only IList&lt;T&gt; view onto this view.</returns>
			public IList<T> AsList()
			{
				return new ListView(mySet, rangeTester, entireTree, reversed);
			}

			/// <summary>
			/// Creates a new View that has the same items as this view, in the reversed order.
			/// </summary>
			/// <returns>A new View that has the reversed order of this view, with the same upper 
			/// and lower bounds.</returns>
			public View Reversed()
			{
				return new View(mySet, rangeTester, entireTree, !reversed);
			}

			/// <summary>
			/// Returns the first item in this view: the item
			/// that would appear first if the view was enumerated. 
			/// </summary>
			/// <remarks>GetFirst() takes time O(log N), where N is the number of items in the set.</remarks>
			/// <returns>The first item in the view. </returns>
			/// <exception cref="InvalidOperationException">The view has no items in it.</exception>
			public T GetFirst()
			{
				T item;
				int found;

				if (reversed)
					found = mySet.tree.LastItemInRange(rangeTester, out item);
				else
					found = mySet.tree.FirstItemInRange(rangeTester, out item);

				if (found < 0)
					throw new InvalidOperationException(Strings.CollectionIsEmpty);

				return item;
			}

			/// <summary>
			/// Returns the last item in the view: the item
			/// that would appear last if the view was enumerated. 
			/// </summary>
			/// <remarks>GetLast() takes time O(log N), where N is the number of items in the set.</remarks>
			/// <returns>The last item in the view. </returns>
			/// <exception cref="InvalidOperationException">The view has no items in it.</exception>
			public T GetLast()
			{
				T item;
				int found;

				if (reversed)
					found = mySet.tree.FirstItemInRange(rangeTester, out item);
				else
					found = mySet.tree.LastItemInRange(rangeTester, out item);

				if (found < 0)
					throw new InvalidOperationException(Strings.CollectionIsEmpty);

				return item;
			}

			/// <summary>
			/// Determine if the given item lies within the bounds of this view.
			/// </summary>
			/// <param name="item">Item to test.</param>
			/// <returns>True if the item is within the bounds of this view.</returns>
			private bool ItemInView(T item)
			{
				return rangeTester(item) == 0;
			}
		}

		#endregion
	}
}