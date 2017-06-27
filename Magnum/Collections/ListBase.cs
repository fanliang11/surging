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
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// ListBase is an abstract class that can be used as a base class for a read-write collection that needs 
	/// to implement the generic IList&lt;T&gt; and non-generic IList collections. The derived class needs
	/// to override the following methods: Count, Clear, Insert, RemoveAt, and the indexer. The implementation
	/// of all the other methods in IList&lt;T&gt; and IList are handled by ListBase.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public abstract class ListBase<T> : CollectionBase<T>,
		IList<T>,
		IList
	{
		/// <summary>
		/// Adds an item to the end of the list. This method is equivalent to calling: 
		/// <code>Insert(Count, item)</code>
		/// </summary>
		/// <param name="value">The item to add to the list.</param>
		/// <exception cref="ArgumentException"><paramref name="value"/> cannot be converted to T.</exception>
		int IList.Add(object value)
		{
			int count = Count;
			Insert(count, ConvertToItemType("value", value));
			return count;
		}

		/// <summary>
		/// Removes all the items from the list, resulting in an empty list.
		/// </summary>
		void IList.Clear()
		{
			Clear();
		}

		/// <summary>
		/// Determines if the list contains any item that compares equal to <paramref name="value"/>.
		/// </summary>
		/// <remarks>Equality in the list is determined by the default sense of
		/// equality for T. If T implements IComparable&lt;T&gt;, the
		/// Equals method of that interface is used to determine equality. Otherwise, 
		/// Object.Equals is used to determine equality.</remarks>
		/// <param name="value">The item to search for.</param>
		bool IList.Contains(object value)
		{
			if (value is T || value == null)
				return Contains((T) value);
			else
				return false;
		}

		/// <summary>
		/// Find the first occurrence of an item equal to <paramref name="value"/>
		/// in the list, and returns the index of that item.
		/// </summary>
		/// <remarks>Equality in the list is determined by the default sense of
		/// equality for T. If T implements IComparable&lt;T&gt;, the
		/// Equals method of that interface is used to determine equality. Otherwise, 
		/// Object.Equals is used to determine equality.</remarks>
		/// <param name="value">The item to search for.</param>
		/// <returns>The index of <paramref name="value"/>, or -1 if no item in the 
		/// list compares equal to <paramref name="value"/>.</returns>
		int IList.IndexOf(object value)
		{
			if (value is T || value == null)
				return IndexOf((T) value);
			else
				return -1;
		}

		/// <summary>
		/// Insert a new
		/// item at the given index. 
		/// </summary>
		/// <param name="index">The index in the list to insert the item at. After the
		/// insertion, the inserted item is located at this index. The
		/// first item in the list has index 0.</param>
		/// <param name="value">The item to insert at the given index.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than Count.</exception>
		/// <exception cref="ArgumentException"><paramref name="value"/> cannot be converted to T.</exception>
		void IList.Insert(int index, object value)
		{
			Insert(index, ConvertToItemType("value", value));
		}

		/// <summary>
		/// Returns whether the list is a fixed size. This implementation always returns false.
		/// </summary>
		/// <value>Alway false, indicating that the list is not fixed size.</value>
		bool IList.IsFixedSize
		{
			get { return false; }
		}

		/// <summary>
		/// Returns whether the list is read only. This implementation returns the value
		/// from ICollection&lt;T&gt;.IsReadOnly, which is by default, false.
		/// </summary>
		/// <value>By default, false, indicating that the list is not read only.</value>
		bool IList.IsReadOnly
		{
			get { return ((ICollection<T>) this).IsReadOnly; }
		}

		/// <summary>
		/// Searches the list for the first item that compares equal to <paramref name="value"/>.
		/// If one is found, it is removed. Otherwise, the list is unchanged.
		/// </summary>
		/// <remarks>Equality in the list is determined by the default sense of
		/// equality for T. If T implements IComparable&lt;T&gt;, the
		/// Equals method of that interface is used to determine equality. Otherwise, 
		/// Object.Equals is used to determine equality.</remarks>
		/// <param name="value">The item to remove from the list.</param>
		/// <exception cref="ArgumentException"><paramref name="value"/> cannot be converted to T.</exception>
		void IList.Remove(object value)
		{
			if (value is T || value == null)
				Remove((T) value);
		}

		/// <summary>
		/// Removes the
		/// item at the given index. 
		/// </summary>
		/// <param name="index">The index in the list to remove the item at. The
		/// first item in the list has index 0.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than or equal to Count.</exception>
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		/// <summary>
		/// Gets or sets the
		/// value at a particular index in the list.
		/// </summary>
		/// <param name="index">The index in the list to get or set an item at. The
		/// first item in the list has index 0, and the last has index Count-1.</param>
		/// <value>The item at the given index.</value>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than or equal to Count.</exception>
		/// <exception cref="ArgumentException"><paramref name="value"/> cannot be converted to T.</exception>
		object IList.this[int index]
		{
			get { return this[index]; }

			set { this[index] = ConvertToItemType("value", value); }
		}

		/// <summary>
		/// The property must be overridden by the derived class to return the number of 
		/// items in the list.
		/// </summary>
		/// <value>The number of items in the list.</value>
		public abstract override int Count { get; }

		/// <summary>
		/// This method must be overridden by the derived class to empty the list
		/// of all items.
		/// </summary>
		public abstract override void Clear();

		/// <summary>
		/// The indexer must be overridden by the derived class to get and set
		/// values of the list at a particular index.
		/// </summary>
		/// <param name="index">The index in the list to get or set an item at. The
		/// first item in the list has index 0, and the last has index Count-1.</param>
		/// <returns>The item at the given index.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than or equal to Count.</exception>
		public abstract T this[int index] { get; set; }

		/// <summary>
		/// This method must be overridden by the derived class to insert a new
		/// item at the given index. 
		/// </summary>
		/// <param name="index">The index in the list to insert the item at. After the
		/// insertion, the inserted item is located at this index. The
		/// first item in the list has index 0.</param>
		/// <param name="item">The item to insert at the given index.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than Count.</exception>
		public abstract void Insert(int index, T item);

		/// <summary>
		/// This method must be overridden by the derived class to remove the
		/// item at the given index. 
		/// </summary>
		/// <param name="index">The index in the list to remove the item at. The
		/// first item in the list has index 0.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than or equal to Count.</exception>
		public abstract void RemoveAt(int index);

		/// <summary>
		/// Enumerates all of the items in the list, in order. The item at index 0
		/// is enumerated first, then the item at index 1, and so on.
		/// </summary>
		/// <remarks>The enumerator does not check for changes made
		/// to the structure of the list. Thus, changes to the list during
		/// enumeration may cause incorrect enumeration or out of range
		/// exceptions. Consider overriding this method and adding checks
		/// for structural changes.</remarks>
		/// <returns>An IEnumerator&lt;T&gt; that enumerates all the
		/// items in the list.</returns>
		public override IEnumerator<T> GetEnumerator()
		{
			int count = Count;
			for (int i = 0; i < count; ++i)
			{
				yield return this[i];
			}
		}

		/// <summary>
		/// Determines if the list contains any item that compares equal to <paramref name="item"/>.
		/// The implementation simply checks whether IndexOf(item) returns a non-negative value.
		/// </summary>
		/// <remarks>Equality in the list is determined by the default sense of
		/// equality for T. If T implements IComparable&lt;T&gt;, the
		/// Equals method of that interface is used to determine equality. Otherwise, 
		/// Object.Equals is used to determine equality.</remarks>
		/// <param name="item">The item to search for.</param>
		/// <returns>True if the list contains an item that compares equal to <paramref name="item"/>.</returns>
		public override bool Contains(T item)
		{
			return (IndexOf(item) >= 0);
		}

		/// <summary>
		/// Adds an item to the end of the list. This method is equivalent to calling: 
		/// <code>Insert(Count, item)</code>
		/// </summary>
		/// <param name="item">The item to add to the list.</param>
		public override void Add(T item)
		{
			Insert(Count, item);
		}

		/// <summary>
		/// Searches the list for the first item that compares equal to <paramref name="item"/>.
		/// If one is found, it is removed. Otherwise, the list is unchanged.
		/// </summary>
		/// <remarks>Equality in the list is determined by the default sense of
		/// equality for T. If T implements IComparable&lt;T&gt;, the
		/// Equals method of that interface is used to determine equality. Otherwise, 
		/// Object.Equals is used to determine equality.</remarks>
		/// <param name="item">The item to remove from the list.</param>
		/// <returns>True if an item was found and removed that compared equal to
		/// <paramref name="item"/>. False if no such item was in the list.</returns>
		public override bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Finds the index of the first item in the list that is equal to <paramref name="item"/>. 
		/// </summary>
		/// <remarks>The default implementation of equality for type T is used in the search. This is the
		/// equality defined by IComparable&lt;T&gt; or object.Equals.</remarks>
		/// <param name="item">The item to search fror.</param>
		/// <returns>The index of the first item in the list that that is equal to <paramref name="item"/>.  If no item is equal
		/// to <paramref name="item"/>, -1 is returned.</returns>
		public virtual int IndexOf(T item)
		{
			return Algorithms.FirstIndexOf(this, item, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Copies all the items in the list, in order, to <paramref name="array"/>,
		/// starting at index 0.
		/// </summary>
		/// <param name="array">The array to copy to. This array must have a size
		/// that is greater than or equal to Count.</param>
		public virtual void CopyTo(T[] array)
		{
			this.CopyTo(array, 0);
		}

		/// <summary>
		/// Copies a range of elements from the list to <paramref name="array"/>,
		/// starting at <paramref name="arrayIndex"/>.
		/// </summary>
		/// <param name="index">The starting index in the source list of the range to copy.</param>
		/// <param name="array">The array to copy to. This array must have a size
		/// that is greater than or equal to Count + arrayIndex.</param>
		/// <param name="arrayIndex">The starting index in <paramref name="array"/>
		/// to copy to.</param>
		/// <param name="count">The number of items to copy.</param>
		public virtual void CopyTo(int index, T[] array, int arrayIndex, int count)
		{
			Range(index, count).CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Provides a read-only view of this list. The returned IList&lt;T&gt; provides
		/// a view of the list that prevents modifications to the list. Use the method to provide
		/// access to the list without allowing changes. Since the returned object is just a view,
		/// changes to the list will be reflected in the view.
		/// </summary>
		/// <returns>An IList&lt;T&gt; that provides read-only access to the list.</returns>
		public new virtual IList<T> AsReadOnly()
		{
			return Algorithms.ReadOnly(this);
		}

		/// <summary>
		/// Finds the first item in the list that satisfies the condition
		/// defined by <paramref name="predicate"/>. If no item matches the condition, than
		/// the default value for T (null or all-zero) is returned.
		/// </summary>
		/// <remarks>If the default value for T (null or all-zero) matches the condition defined by <paramref name="predicate"/>,
		/// and the list might contain the default value, then it is impossible to distinguish the different between finding
		/// the default value and not finding any item. To distinguish these cases, use <see cref="TryFind"/>.</remarks>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <returns>The first item that satisfies the condition <paramref name="predicate"/>. If no item satisfies that
		/// condition, the default value for T is returned.</returns>
		/// <seealso cref="TryFind"/>
		public virtual T Find(Predicate<T> predicate)
		{
			return Algorithms.FindFirstWhere(this, predicate);
		}

		/// <summary>
		/// Finds the first item in the list that satisfies the condition
		/// defined by <paramref name="predicate"/>. 
		/// </summary>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <param name="foundItem">If true is returned, this parameter receives the first item in the list
		/// that satifies the condition defined by <paramref name="predicate"/>.</param>
		/// <returns>True if an item that  satisfies the condition <paramref name="predicate"/> was found. False 
		/// if no item in the list satisfies that condition.</returns>
		public virtual bool TryFind(Predicate<T> predicate, out T foundItem)
		{
			return Algorithms.TryFindFirstWhere(this, predicate, out foundItem);
		}

		/// <summary>
		/// Finds the last item in the list that satisfies the condition
		/// defined by <paramref name="predicate"/>. If no item matches the condition, than
		/// the default value for T (null or all-zero) is returned.
		/// </summary>
		/// <remarks>If the default value for T (null or all-zero) matches the condition defined by <paramref name="predicate"/>,
		/// and the list might contain the default value, then it is impossible to distinguish the different between finding
		/// the default value and not finding any item. To distinguish these cases, use <see cref="TryFindLast"/>.</remarks>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <returns>The last item that satisfies the condition <paramref name="predicate"/>. If no item satisfies that
		/// condition, the default value for T is returned.</returns>
		/// <seealso cref="TryFindLast"/>
		public virtual T FindLast(Predicate<T> predicate)
		{
			return Algorithms.FindLastWhere(this, predicate);
		}

		/// <summary>
		/// Finds the last item in the list that satisfies the condition
		/// defined by <paramref name="predicate"/>. 
		/// </summary>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <param name="foundItem">If true is returned, this parameter receives the last item in the list
		/// that satifies the condition defined by <paramref name="predicate"/>.</param>
		/// <returns>True if an item that  satisfies the condition <paramref name="predicate"/> was found. False 
		/// if no item in the list satisfies that condition.</returns>
		public virtual bool TryFindLast(Predicate<T> predicate, out T foundItem)
		{
			return Algorithms.TryFindLastWhere(this, predicate, out foundItem);
		}

		/// <summary>
		/// Finds the index of the first item in the list that satisfies the condition
		/// defined by <paramref name="predicate"/>. If no item matches the condition, -1 is returned.
		/// </summary>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <returns>The index of the first item that satisfies the condition <paramref name="predicate"/>. If no item satisfies that
		/// condition, -1 is returned.</returns>
		public virtual int FindIndex(Predicate<T> predicate)
		{
			return Algorithms.FindFirstIndexWhere(this, predicate);
		}

		/// <summary>
		/// Finds the index of the first item, in the range of items extending from <paramref name="index"/> to the end, that satisfies the condition
		/// defined by <paramref name="predicate"/>. If no item matches the condition, -1 is returned.
		/// </summary>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <param name="index">The starting index of the range to check.</param>
		/// <returns>The index of the first item in the given range that satisfies the condition <paramref name="predicate"/>. If no item satisfies that
		/// condition, -1 is returned.</returns>
		public virtual int FindIndex(int index, Predicate<T> predicate)
		{
			int foundIndex = Algorithms.FindFirstIndexWhere(Range(index, Count - index), predicate);
			if (foundIndex < 0)
				return -1;
			else
				return foundIndex + index;
		}

		/// <summary>
		/// Finds the index of the first item, in the range of <paramref name="count"/> items starting from <paramref name="index"/>, that satisfies the condition
		/// defined by <paramref name="predicate"/>. If no item matches the condition, -1 is returned.
		/// </summary>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <param name="index">The starting index of the range to check.</param>
		/// <param name="count">The number of items in range to check.</param>
		/// <returns>The index of the first item in the given range that satisfies the condition <paramref name="predicate"/>. If no item satisfies that
		/// condition, -1 is returned.</returns>
		public virtual int FindIndex(int index, int count, Predicate<T> predicate)
		{
			int foundIndex = Algorithms.FindFirstIndexWhere(Range(index, count), predicate);
			if (foundIndex < 0)
				return -1;
			else
				return foundIndex + index;
		}

		/// <summary>
		/// Finds the index of the last item in the list that satisfies the condition
		/// defined by <paramref name="predicate"/>. If no item matches the condition, -1 is returned.
		/// </summary>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <returns>The index of the last item that satisfies the condition <paramref name="predicate"/>. If no item satisfies that
		/// condition, -1 is returned.</returns>
		public virtual int FindLastIndex(Predicate<T> predicate)
		{
			return Algorithms.FindLastIndexWhere(this, predicate);
		}

		/// <summary>
		/// Finds the index of the last item, in the range of items extending from the beginning
		/// of the list to <paramref name="index"/>, that satisfies the condition
		/// defined by <paramref name="predicate"/>. If no item matches the condition, -1 is returned.
		/// </summary>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <param name="index">The ending index of the range to check.</param>
		/// <returns>The index of the last item in the given range that satisfies the condition <paramref name="predicate"/>. If no item satisfies that
		/// condition, -1 is returned.</returns>
		public virtual int FindLastIndex(int index, Predicate<T> predicate)
		{
			return Algorithms.FindLastIndexWhere(Range(0, index + 1), predicate);
		}

		/// <summary>
		/// Finds the index of the last item, in the range of <paramref name="count"/> items ending at <paramref name="index"/>, that satisfies the condition
		/// defined by <paramref name="predicate"/>. If no item matches the condition, -1 is returned.
		/// </summary>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <param name="index">The ending index of the range to check.</param>
		/// <param name="count">The number of items in range to check.</param>
		/// <returns>The index of the last item in the given range that satisfies the condition <paramref name="predicate"/>. If no item satisfies that
		/// condition, -1 is returned.</returns>
		public virtual int FindLastIndex(int index, int count, Predicate<T> predicate)
		{
			int foundIndex = Algorithms.FindLastIndexWhere(Range(index - count + 1, count), predicate);

			if (foundIndex >= 0)
				return foundIndex + index - count + 1;
			else
				return -1;
		}

		/// <summary>
		/// Finds the index of the first item, in the range of items extending from <paramref name="index"/> to the end,  
		/// that is equal to <paramref name="item"/>. 
		/// </summary>
		/// <remarks>The default implementation of equality for type T is used in the search. This is the
		/// equality defined by IComparable&lt;T&gt; or object.Equals.</remarks>
		/// <param name="item">The item to search fror.</param>
		/// <param name="index">The starting index of the range to check.</param>
		/// <returns>The index of the first item in the given range that that is equal to <paramref name="item"/>.  If no item is equal
		/// to <paramref name="item"/>, -1 is returned.</returns>
		public virtual int IndexOf(T item, int index)
		{
			int foundIndex = Algorithms.FirstIndexOf(Range(index, Count - index), item, EqualityComparer<T>.Default);

			if (foundIndex >= 0)
				return foundIndex + index;
			else
				return -1;
		}

		/// <summary>
		/// Finds the index of the first item, in the range of <paramref name="count"/> items starting from <paramref name="index"/>,  
		/// that is equal to <paramref name="item"/>. 
		/// </summary>
		/// <remarks>The default implementation of equality for type T is used in the search. This is the
		/// equality defined by IComparable&lt;T&gt; or object.Equals.</remarks>
		/// <param name="item">The item to search fror.</param>
		/// <param name="index">The starting index of the range to check.</param>
		/// <param name="count">The number of items in range to check.</param>
		/// <returns>The index of the first item in the given range that that is equal to <paramref name="item"/>.  If no item is equal
		/// to <paramref name="item"/>, -1 is returned.</returns>
		public virtual int IndexOf(T item, int index, int count)
		{
			int foundIndex = Algorithms.FirstIndexOf(Range(index, count), item, EqualityComparer<T>.Default);

			if (foundIndex >= 0)
				return foundIndex + index;
			else
				return -1;
		}

		/// <summary>
		/// Finds the index of the last item in the list that is equal to <paramref name="item"/>. 
		/// </summary>
		/// <remarks>The default implementation of equality for type T is used in the search. This is the
		/// equality defined by IComparable&lt;T&gt; or object.Equals.</remarks>
		/// <param name="item">The item to search fror.</param>
		/// <returns>The index of the last item in the list that that is equal to <paramref name="item"/>.  If no item is equal
		/// to <paramref name="item"/>, -1 is returned.</returns>
		public virtual int LastIndexOf(T item)
		{
			return Algorithms.LastIndexOf(this, item, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Finds the index of the last item, in the range of items extending from the beginning
		/// of the list to <paramref name="index"/>, that is equal to <paramref name="item"/>. 
		/// </summary>
		/// <remarks>The default implementation of equality for type T is used in the search. This is the
		/// equality defined by IComparable&lt;T&gt; or object.Equals.</remarks>
		/// <param name="item">The item to search fror.</param>
		/// <param name="index">The ending index of the range to check.</param>
		/// <returns>The index of the last item in the given range that that is equal to <paramref name="item"/>.  If no item is equal
		/// to <paramref name="item"/>, -1 is returned.</returns>
		public virtual int LastIndexOf(T item, int index)
		{
			int foundIndex = Algorithms.LastIndexOf(Range(0, index + 1), item, EqualityComparer<T>.Default);

			return foundIndex;
		}

		/// <summary>
		/// Finds the index of the last item, in the range of <paramref name="count"/> items ending at <paramref name="index"/>, 
		/// that is equal to <paramref name="item"/>. 
		/// </summary>
		/// <remarks>The default implementation of equality for type T is used in the search. This is the
		/// equality defined by IComparable&lt;T&gt; or object.Equals.</remarks>
		/// <param name="item">The item to search for.</param>
		/// <param name="index">The ending index of the range to check.</param>
		/// <param name="count">The number of items in range to check.</param>
		/// <returns>The index of the last item in the given range that that is equal to <paramref name="item"/>.  If no item is equal
		/// to <paramref name="item"/>, -1 is returned.</returns>
		public virtual int LastIndexOf(T item, int index, int count)
		{
			int foundIndex = Algorithms.LastIndexOf(Range(index - count + 1, count), item, EqualityComparer<T>.Default);

			if (foundIndex >= 0)
				return foundIndex + index - count + 1;
			else
				return -1;
		}

		/// <summary>
		/// Returns a view onto a sub-range of this list. Items are not copied; the
		/// returned IList&lt;T&gt; is simply a different view onto the same underlying items. Changes to this list
		/// are reflected in the view, and vice versa. Insertions and deletions in the view change the size of the 
		/// view, but insertions and deletions in the underlying list do not.
		/// </summary>
		/// <remarks>
		/// <para>This method can be used to apply an algorithm to a portion of a list. For example:</para>
		/// <code>Algorithms.ReverseInPlace(deque.Range(3, 6))</code>
		/// will reverse the 6 items beginning at index 3.</remarks>
		/// <param name="start">The starting index of the view.</param>
		/// <param name="count">The number of items in the view.</param>
		/// <returns>A list that is a view onto the given sub-part of this list. </returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> + <paramref name="count"/> is greater than the
		/// size of the list.</exception>
		public virtual IList<T> Range(int start, int count)
		{
			return Algorithms.Range(this, start, count);
		}

		/// <summary>
		/// Convert the given parameter to T. Throw an ArgumentException
		/// if it isn't.
		/// </summary>
		/// <param name="name">parameter name</param>
		/// <param name="value">parameter value</param>
		private static T ConvertToItemType(string name, object value)
		{
			try
			{
				return (T) value;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(string.Format(Strings.WrongType, value, typeof (T)), name);
			}
		}
	}
}