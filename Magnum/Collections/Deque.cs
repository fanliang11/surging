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
	/// <para>The Deque class implements a type of list known as a Double Ended Queue. A Deque
	/// is quite similar to a List, in that items have indices (starting at 0), and the item at any
	/// index can be efficiently retrieved. The difference between a List and a Deque lies in the
	/// efficiency of inserting elements at the beginning. In a List, items can be efficiently added
	/// to the end, but inserting an item at the beginning of the List is slow, taking time 
	/// proportional to the size of the List. In a Deque, items can be added to the beginning 
	/// or end equally efficiently, regardless of the number of items in the Deque. As a trade-off
	/// for this increased flexibility, Deque is somewhat slower than List (but still constant time) when
	/// being indexed to get or retrieve elements. </para>
	/// </summary>
	/// <remarks>
	/// <para>The Deque class can also be used as a more flexible alternative to the Queue 
	/// and Stack classes. Deque is as efficient as Queue and Stack for adding or removing items, 
	/// but is more flexible: it allows access
	/// to all items in the queue, and allows adding or removing from either end.</para>
	/// <para>Deque is implemented as a ring buffer, which is grown as necessary. The size
	/// of the buffer is doubled whenever the existing capacity is too small to hold all the
	/// elements.</para>
	/// </remarks>
	/// <typeparam name="T">The type of items stored in the Deque.</typeparam>
	[Serializable]
	public class Deque<T> : ListBase<T>,
		ICloneable
	{
		// The initial size of the buffer.
		private const int INITIAL_SIZE = 8;

		// A ring buffer containing all the items in the deque. Shrinks or grows as needed.
		// Except temporarily during an add, there is always at least one empty item.
		private T[] buffer;

		// Index of the first item (index 0) in the deque.
		// Always in the range 0 through buffer.Length - 1.

		// Holds the change stamp for the collection.
		private int changeStamp;
		private int end;
		private int start;

		/// <summary>
		/// Create a new Deque that is initially empty.
		/// </summary>
		public Deque()
		{
		}

		/// <summary>
		/// Create a new Deque initialized with the items from the passed collection,
		/// in order.
		/// </summary>
		/// <param name="collection">A collection of items to initialize the Deque with.</param>
		public Deque(IEnumerable<T> collection)
		{
			AddManyToBack(collection);
		}

		/// <summary>
		/// Gets the number of items currently stored in the Deque. The last item
		/// in the Deque has index Count-1.
		/// </summary>
		/// <remarks>Getting the count of items in the Deque takes a small constant
		/// amount of time.</remarks>
		/// <value>The number of items stored in this Deque.</value>
		public override sealed int Count
		{
			get
			{
				if (end >= start)
					return end - start;
				else
					return end + buffer.Length - start;
			}
		}

		/// <summary>
		/// Gets or sets the capacity of the Deque. The Capacity is the number of
		/// items that this Deque can hold without expanding its internal buffer. Since
		/// Deque will automatically expand its buffer when necessary, in almost all cases
		/// it is unnecessary to worry about the capacity. However, if it is known that a
		/// Deque will contain exactly 1000 items eventually, it can slightly improve 
		/// efficiency to set the capacity to 1000 up front, so that the Deque does not
		/// have to expand automatically.
		/// </summary>
		/// <value>The number of items that this Deque can hold without expanding its
		/// internal buffer.</value>
		/// <exception cref="ArgumentOutOfRangeException">The capacity is being set
		/// to less than Count, or to too large a value.</exception>
		public int Capacity
		{
			get
			{
				if (buffer == null)
					return 0;
				else
					return buffer.Length - 1;
			}
			set
			{
				if (value < Count)
					throw new ArgumentOutOfRangeException("value", Strings.CapacityLessThanCount);
				if (value > int.MaxValue - 1)
					throw new ArgumentOutOfRangeException("value");
				if (value == Capacity)
					return;

				T[] newBuffer = new T[value + 1];
				CopyTo(newBuffer, 0);
				end = Count;
				start = 0;
				buffer = newBuffer;
			}
		}

		/// <summary>
		/// Gets or sets an item at a particular index in the Deque. 
		/// </summary>
		/// <remarks>Getting or setting the item at a particular index takes a small constant amount
		/// of time, no matter what index is used.</remarks>
		/// <param name="index">The index of the item to retrieve or change. The front item has index 0, and
		/// the back item has index Count-1.</param>
		/// <returns>The value at the indicated index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">The index is less than zero or greater than or equal
		/// to Count.</exception>
		public override sealed T this[int index]
		{
			get
			{
				int i = index + start;
				if (i < start) // handles both the case where index < 0, or the above addition overflow to a negative number.
					throw new ArgumentOutOfRangeException("index");

				if (end >= start)
				{
					if (i >= end)
						throw new ArgumentOutOfRangeException("index");
					return buffer[i];
				}
				else
				{
					int length = buffer.Length;
					if (i >= length)
					{
						i -= length;
						if (i >= end)
							throw new ArgumentOutOfRangeException("index");
					}
					return buffer[i];
				}
			}

			set
			{
				// Like List<T>, we stop enumerations after a set operation. There is no
				// technical reason to do this, however.
				StopEnumerations();

				int i = index + start;
				if (i < start) // handles both the case where index < 0, or the above addition overflow to a negative number.
					throw new ArgumentOutOfRangeException("index");

				if (end >= start)
				{
					if (i >= end)
						throw new ArgumentOutOfRangeException("index");
					buffer[i] = value;
				}
				else
				{
					int length = buffer.Length;
					if (i >= length)
					{
						i -= length;
						if (i >= end)
							throw new ArgumentOutOfRangeException("index");
					}
					buffer[i] = value;
				}
			}
		}

		/// <summary>
		/// Creates a new Deque that is a copy of this one.
		/// </summary>
		/// <remarks>Copying a Deque takes O(N) time, where N is the number of items in this Deque..</remarks>
		/// <returns>A copy of the current deque.</returns>
		object ICloneable.Clone()
		{
			return this.Clone();
		}

		/// <summary>
		/// Copies all the items in the Deque into an array.
		/// </summary>
		/// <param name="array">Array to copy to.</param>
		/// <param name="arrayIndex">Starting index in <paramref name="array"/> to copy to.</param>
		public override sealed void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			// This override is provided to give a more efficient implementation to CopyTo than
			// the default one provided by CollectionBase.

			int length = (buffer == null) ? 0 : buffer.Length;

			if (start > end)
			{
				Array.Copy(buffer, start, array, arrayIndex, length - start);
				Array.Copy(buffer, 0, array, arrayIndex + length - start, end);
			}
			else
			{
				if (end > start)
					Array.Copy(buffer, start, array, arrayIndex, end - start);
			}
		}

		/// <summary>
		/// Trims the amount of memory used by the Deque by changing
		/// the Capacity to be equal to Count. If no more items will be added
		/// to the Deque, calling TrimToSize will reduce the amount of memory
		/// used by the Deque.
		/// </summary>
		public void TrimToSize()
		{
			Capacity = Count;
		}

		/// <summary>
		/// Removes all items from the Deque.
		/// </summary>
		/// <remarks>Clearing the Deque takes a small constant amount of time, regardless of
		/// how many items are currently in the Deque.</remarks>
		public override sealed void Clear()
		{
			StopEnumerations();
			buffer = null;
			start = end = 0;
		}

		/// <summary>
		/// Enumerates all of the items in the list, in order. The item at index 0
		/// is enumerated first, then the item at index 1, and so on. If the items
		/// are added to or removed from the Deque during enumeration, the 
		/// enumeration ends with an InvalidOperationException.
		/// </summary>
		/// <returns>An IEnumerator&lt;T&gt; that enumerates all the
		/// items in the list.</returns>
		/// <exception cref="InvalidOperationException">The Deque has an item added or deleted during the enumeration.</exception>
		public override sealed IEnumerator<T> GetEnumerator()
		{
			int startStamp = changeStamp;
			int count = Count;

			for (int i = 0; i < count; ++i)
			{
				yield return this[i];
				CheckEnumerationStamp(startStamp);
			}
		}

		/// <summary>
		/// Inserts a new item at the given index in the Deque. All items at indexes 
		/// equal to or greater than <paramref name="index"/> move up one index
		/// in the Deque.
		/// </summary>
		/// <remarks>The amount of time to insert an item in the Deque is proportional
		/// to the distance of index from the closest end of the Deque: 
		/// O(Min(<paramref name="index"/>, Count - <paramref name="index"/>)).
		/// Thus, inserting an item at the front or end of the Deque is always fast; the middle of
		/// of the Deque is the slowest place to insert.
		/// </remarks>
		/// <param name="index">The index in the Deque to insert the item at. After the
		/// insertion, the inserted item is located at this index. The
		/// front item in the Deque has index 0.</param>
		/// <param name="item">The item to insert at the given index.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than Count.</exception>
		public override sealed void Insert(int index, T item)
		{
			StopEnumerations();

			int count = Count;
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException("index");

			if (buffer == null)
			{
				// The buffer hasn't been created yet.
				CreateInitialBuffer(item);
				return;
			}

			int length = buffer.Length;
			int i; // The location the new item was placed at.

			if (index < count/2)
			{
				// Inserting into the first half of the list. Move items with
				// lower index down in the buffer.
				start -= 1;
				if (start < 0)
					start += length;
				i = index + start;
				if (i >= length)
				{
					i -= length;
					if (length - 1 > start)
						Array.Copy(buffer, start + 1, buffer, start, length - 1 - start);
					buffer[length - 1] = buffer[0]; // unneeded if end == 0, but doesn't hurt
					if (i > 0)
						Array.Copy(buffer, 1, buffer, 0, i);
				}
				else
				{
					if (i > start)
						Array.Copy(buffer, start + 1, buffer, start, i - start);
				}
			}
			else
			{
				// Inserting into the last half of the list. Move items with higher
				// index up in the buffer.
				i = index + start;
				if (i >= length)
					i -= length;
				if (i <= end)
				{
					if (end > i)
						Array.Copy(buffer, i, buffer, i + 1, end - i);
					end += 1;
					if (end >= length)
						end -= length;
				}
				else
				{
					if (end > 0)
						Array.Copy(buffer, 0, buffer, 1, end);
					buffer[0] = buffer[length - 1];
					if (length - 1 > i)
						Array.Copy(buffer, i, buffer, i + 1, length - 1 - i);
					end += 1;
				}
			}

			buffer[i] = item;
			if (start == end)
				IncreaseBuffer();
		}

		/// <summary>
		/// Inserts a collection of items at the given index in the Deque. All items at indexes 
		/// equal to or greater than <paramref name="index"/> increase their indices in the Deque
		/// by the number of items inserted.
		/// </summary>
		/// <remarks>The amount of time to insert a collection in the Deque is proportional
		/// to the distance of index from the closest end of the Deque, plus the number of items
		/// inserted (M): 
		/// O(M + Min(<paramref name="index"/>, Count - <paramref name="index"/>)).
		/// </remarks>
		/// <param name="index">The index in the Deque to insert the collection at. After the
		/// insertion, the first item of the inserted collection is located at this index. The
		/// front item in the Deque has index 0.</param>
		/// <param name="collection">The collection of items to insert at the given index.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than Count.</exception>
		public void InsertRange(int index, IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			StopEnumerations();

			int count = Count;
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException("index");

			// We need an ICollection, because we need the count of the collection.
			// If needed, copy the items to a temporary list.
			ICollection<T> coll;
			if (collection is ICollection<T>)
				coll = (ICollection<T>) collection;
			else
			{
				coll = new List<T>(collection);
			}
			if (coll.Count == 0)
				return; // nothing to do.

			// Create enough capacity in the list for the new items.
			if (Capacity < Count + coll.Count)
				Capacity = Count + coll.Count;

			int length = buffer.Length;
			int s, d;

			if (index < count/2)
			{
				// Inserting into the first half of the list. Move items with
				// lower index down in the buffer.
				s = start;
				d = s - coll.Count;
				if (d < 0)
					d += length;
				start = d;
				int c = index;

				while (c > 0)
				{
					int chunk = c;
					if (length - d < chunk)
						chunk = length - d;
					if (length - s < chunk)
						chunk = length - s;
					Array.Copy(buffer, s, buffer, d, chunk);
					c -= chunk;
					if ((d += chunk) >= length)
						d -= length;
					if ((s += chunk) >= length)
						s -= length;
				}
			}
			else
			{
				// Inserting into the last half of the list. Move items with higher
				// index up in the buffer.
				s = end;
				d = s + coll.Count;
				if (d >= length)
					d -= length;
				end = d;
				int move = count - index; // number of items at end to move

				int c = move;
				while (c > 0)
				{
					int chunk = c;
					if (d > 0 && d < chunk)
						chunk = d;
					if (s > 0 && s < chunk)
						chunk = s;
					if ((d -= chunk) < 0)
						d += length;
					if ((s -= chunk) < 0)
						s += length;
					Array.Copy(buffer, s, buffer, d, chunk);
					c -= chunk;
				}

				d -= coll.Count;
				if (d < 0)
					d += length;
			}

			// Copy the items into the space vacated, which starts at d.
			foreach (T item in coll)
			{
				buffer[d] = item;
				if (++d >= length)
					d -= length;
			}
		}

		/// <summary>
		/// Removes the item at the given index in the Deque. All items at indexes 
		/// greater than <paramref name="index"/> move down one index
		/// in the Deque.
		/// </summary>
		/// <remarks>The amount of time to delete an item in the Deque is proportional
		/// to the distance of index from the closest end of the Deque: 
		/// O(Min(<paramref name="index"/>, Count - 1 - <paramref name="index"/>)).
		/// Thus, deleting an item at the front or end of the Deque is always fast; the middle of
		/// of the Deque is the slowest place to delete.
		/// </remarks>
		/// <param name="index">The index in the list to remove the item at. The
		/// first item in the list has index 0.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than or equal to Count.</exception>
		public override sealed void RemoveAt(int index)
		{
			StopEnumerations();

			int count = Count;

			if (index < 0 || index >= count)
				throw new ArgumentOutOfRangeException("index");

			int length = buffer.Length;
			int i; // index of removed item
			if (index < count/2)
			{
				// Removing in the first half of the list. Move items with
				// lower index up in the buffer.
				i = index + start;

				if (i >= length)
				{
					i -= length;

					if (i > 0)
						Array.Copy(buffer, 0, buffer, 1, i);
					buffer[0] = buffer[length - 1];
					if (length - 1 > start)
						Array.Copy(buffer, start, buffer, start + 1, length - 1 - start);
				}
				else
				{
					if (i > start)
						Array.Copy(buffer, start, buffer, start + 1, i - start);
				}

				buffer[start] = default(T);
				start += 1;
				if (start >= length)
					start -= length;
			}
			else
			{
				// Removing in the second half of the list. Move items with
				// higher indexes down in the buffer.
				i = index + start;
				if (i >= length)
					i -= length;
				end -= 1;
				if (end < 0)
					end = length - 1;

				if (i <= end)
				{
					if (end > i)
						Array.Copy(buffer, i + 1, buffer, i, end - i);
				}
				else
				{
					if (length - 1 > i)
						Array.Copy(buffer, i + 1, buffer, i, length - 1 - i);
					buffer[length - 1] = buffer[0];
					if (end > 0)
						Array.Copy(buffer, 1, buffer, 0, end);
				}

				buffer[end] = default(T);
			}
		}

		/// <summary>
		/// Removes a range of items at the given index in the Deque. All items at indexes 
		/// greater than <paramref name="index"/> move down <paramref name="count"/> indices
		/// in the Deque.
		/// </summary>
		/// <remarks>The amount of time to delete <paramref name="count"/> items in the Deque is proportional
		/// to the distance to the closest end of the Deque: 
		/// O(Min(<paramref name="index"/>, Count - <paramref name="index"/> - <paramref name="count"/>)).
		/// </remarks>
		/// <param name="index">The index in the list to remove the range at. The
		/// first item in the list has index 0.</param>
		/// <param name="count">The number of items to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than or equal to Count, or <paramref name="count"/> is less than zero
		/// or too large.</exception>
		public void RemoveRange(int index, int count)
		{
			StopEnumerations();

			int dequeCount = Count;

			if (index < 0 || index >= dequeCount)
				throw new ArgumentOutOfRangeException("index");
			if (count < 0 || count > dequeCount - index)
				throw new ArgumentOutOfRangeException("count");
			if (count == 0)
				return;

			int length = buffer.Length;
			int s, d;
			if (index < dequeCount/2)
			{
				// Removing in the first half of the list. Move items with
				// lower index up in the buffer.
				s = start + index;
				if (s >= length)
					s -= length;
				d = s + count;
				if (d >= length)
					d -= length;

				int c = index;
				while (c > 0)
				{
					int chunk = c;
					if (d > 0 && d < chunk)
						chunk = d;
					if (s > 0 && s < chunk)
						chunk = s;
					if ((d -= chunk) < 0)
						d += length;
					if ((s -= chunk) < 0)
						s += length;
					Array.Copy(buffer, s, buffer, d, chunk);
					c -= chunk;
				}

				// At this point, s == start
				for (c = 0; c < count; ++c)
				{
					buffer[s] = default(T);
					if (++s >= length)
						s -= length;
				}
				start = s;
			}
			else
			{
				// Removing in the second half of the list. Move items with
				// higher indexes down in the buffer.
				int move = dequeCount - index - count;
				s = end - move;
				if (s < 0)
					s += length;
				d = s - count;
				if (d < 0)
					d += length;

				int c = move;
				while (c > 0)
				{
					int chunk = c;
					if (length - d < chunk)
						chunk = length - d;
					if (length - s < chunk)
						chunk = length - s;
					Array.Copy(buffer, s, buffer, d, chunk);
					c -= chunk;
					if ((d += chunk) >= length)
						d -= length;
					if ((s += chunk) >= length)
						s -= length;
				}

				// At this point, s == end.
				for (c = 0; c < count; ++c)
				{
					if (--s < 0)
						s += length;
					buffer[s] = default(T);
				}
				end = s;
			}
		}

		/// <summary>
		/// Adds an item to the front of the Deque. The indices of all existing items
		/// in the Deque are increased by 1. This method is 
		/// equivalent to <c>Insert(0, item)</c> but is a little more
		/// efficient.
		/// </summary>
		/// <remarks>Adding an item to the front of the Deque takes
		/// a small constant amount of time, regardless of how many items are in the Deque.</remarks>
		/// <param name="item">The item to add.</param>
		public void AddToFront(T item)
		{
			StopEnumerations();

			if (buffer == null)
			{
				// The buffer hasn't been created yet.
				CreateInitialBuffer(item);
				return;
			}

			if (--start < 0)
				start += buffer.Length;
			buffer[start] = item;
			if (start == end)
				IncreaseBuffer();
		}

		/// <summary>
		/// Adds a collection of items to the front of the Deque. The indices of all existing items
		/// in the Deque are increased by the number of items inserted. The first item in the added collection becomes the
		/// first item in the Deque. 
		/// </summary>
		/// <remarks>This method takes time O(M), where M is the number of items in the 
		/// <paramref name="collection"/>.</remarks>
		/// <param name="collection">The collection of items to add.</param>
		public void AddManyToFront(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			InsertRange(0, collection);
		}

		/// <summary>
		/// Adds an item to the back of the Deque. The indices of all existing items
		/// in the Deque are unchanged. This method is 
		/// equivalent to <c>Insert(Count, item)</c> but is a little more
		/// efficient.
		/// </summary>
		/// <remarks>Adding an item to the back of the Deque takes
		/// a small constant amount of time, regardless of how many items are in the Deque.</remarks>
		/// <param name="item">The item to add.</param>
		public void AddToBack(T item)
		{
			StopEnumerations();

			if (buffer == null)
			{
				// The buffer hasn't been created yet.
				CreateInitialBuffer(item);
				return;
			}

			buffer[end] = item;
			if (++end >= buffer.Length)
				end -= buffer.Length;
			if (start == end)
				IncreaseBuffer();
		}

		/// <summary>
		/// Adds an item to the back of the Deque. The indices of all existing items
		/// in the Deque are unchanged. This method is 
		/// equivalent to <c>AddToBack(item)</c>.
		/// </summary>
		/// <remarks>Adding an item to the back of the Deque takes
		/// a small constant amount of time, regardless of how many items are in the Deque.</remarks>
		/// <param name="item">The item to add.</param>
		public override sealed void Add(T item)
		{
			AddToBack(item);
		}


		/// <summary>
		/// Adds a collection of items to the back of the Deque. The indices of all existing items
		/// in the Deque are unchanged. The last item in the added collection becomes the
		/// last item in the Deque.
		/// </summary>
		/// <remarks>This method takes time O(M), where M is the number of items in the 
		/// <paramref name="collection"/>.</remarks>
		/// <param name="collection">The collection of item to add.</param>
		public void AddManyToBack(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			foreach (T item in collection)
				AddToBack(item);
		}

		/// <summary>
		/// Removes an item from the front of the Deque. The indices of all existing items
		/// in the Deque are decreased by 1. This method is 
		/// equivalent to <c>RemoveAt(0)</c> but is a little more
		/// efficient.
		/// </summary>
		/// <remarks>Removing an item from the front of the Deque takes
		/// a small constant amount of time, regardless of how many items are in the Deque.</remarks>
		/// <returns>The item that was removed.</returns>
		/// <exception cref="InvalidOperationException">The Deque is empty.</exception>
		public T RemoveFromFront()
		{
			if (start == end)
				throw new InvalidOperationException(Strings.CollectionIsEmpty);

			StopEnumerations();

			T item = buffer[start];
			buffer[start] = default(T);
			if (++start >= buffer.Length)
				start -= buffer.Length;
			return item;
		}

		/// <summary>
		/// Removes an item from the back of the Deque. The indices of all existing items
		/// in the Deque are unchanged. This method is 
		/// equivalent to <c>RemoveAt(Count-1)</c> but is a little more
		/// efficient.
		/// </summary>
		/// <remarks>Removing an item from the back of the Deque takes
		/// a small constant amount of time, regardless of how many items are in the Deque.</remarks>
		/// <exception cref="InvalidOperationException">The Deque is empty.</exception>
		public T RemoveFromBack()
		{
			if (start == end)
				throw new InvalidOperationException(Strings.CollectionIsEmpty);

			StopEnumerations();

			if (--end < 0)
				end += buffer.Length;
			T item = buffer[end];
			buffer[end] = default(T);
			return item;
		}

		/// <summary>
		/// Retreives the item currently at the front of the Deque. The Deque is 
		/// unchanged. This method is 
		/// equivalent to <c>deque[0]</c> (except that a different exception is thrown).
		/// </summary>
		/// <remarks>Retreiving the item at the front of the Deque takes
		/// a small constant amount of time, regardless of how many items are in the Deque.</remarks>
		/// <returns>The item at the front of the Deque.</returns>
		/// <exception cref="InvalidOperationException">The Deque is empty.</exception>
		public T GetAtFront()
		{
			if (start == end)
				throw new InvalidOperationException(Strings.CollectionIsEmpty);

			return buffer[start];
		}

		/// <summary>
		/// Retreives the item currently at the back of the Deque. The Deque is 
		/// unchanged. This method is 
		/// equivalent to <c>deque[deque.Count - 1]</c> (except that a different exception is thrown).
		/// </summary>
		/// <remarks>Retreiving the item at the back of the Deque takes
		/// a small constant amount of time, regardless of how many items are in the Deque.</remarks>
		/// <returns>The item at the back of the Deque.</returns>
		/// <exception cref="InvalidOperationException">The Deque is empty.</exception>
		public T GetAtBack()
		{
			if (start == end)
				throw new InvalidOperationException(Strings.CollectionIsEmpty);

			if (end == 0)
				return buffer[buffer.Length - 1];
			else
				return buffer[end - 1];
		}

		/// <summary>
		/// Creates a new Deque that is a copy of this one.
		/// </summary>
		/// <remarks>Copying a Deque takes O(N) time, where N is the number of items in this Deque..</remarks>
		/// <returns>A copy of the current deque.</returns>
		public Deque<T> Clone()
		{
			return new Deque<T>(this);
		}

		/// <summary>
		/// Makes a deep clone of this Deque. A new Deque is created with a clone of
		/// each element of this set, by calling ICloneable.Clone on each element. If T is
		/// a value type, then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks><para>If T is a reference type, it must implement
		/// ICloneable. Otherwise, an InvalidOperationException is thrown.</para>
		/// <para>Cloning the Deque takes time O(N), where N is the number of items in the Deque.</para></remarks>
		/// <returns>The cloned Deque.</returns>
		/// <exception cref="InvalidOperationException">T is a reference type that does not implement ICloneable.</exception>
		public Deque<T> CloneContents()
		{
			bool itemIsValueType;
			if (!Util.IsCloneableType(typeof (T), out itemIsValueType))
				throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, typeof (T).FullName));

			Deque<T> clone = new Deque<T>();

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

				clone.AddToBack(itemClone);
			}

			return clone;
		}

		/// <summary>
		/// Print out the internal state of the Deque for debugging.
		/// </summary>
		internal void Print()
		{
			Console.WriteLine("length={0}  start={1}  end={2}", buffer.Length, start, end);
			for (int i = 0; i < buffer.Length; ++i)
			{
				if (i == start)
					Console.Write("start-> ");
				else
					Console.Write("        ");
				if (i == end)
					Console.Write("end-> ");
				else
					Console.Write("      ");
				Console.WriteLine("{0,4} {1}", i, buffer[i]);
			}
			Console.WriteLine();
		}

		/// <summary>
		/// Must be called whenever there is a structural change in the tree. Causes
		/// changeStamp to be changed, which causes any in-progress enumerations
		/// to throw exceptions.
		/// </summary>
		private void StopEnumerations()
		{
			++changeStamp;
		}

		/// <summary>
		/// Checks the given stamp against the current change stamp. If different, the
		/// collection has changed during enumeration and an InvalidOperationException
		/// must be thrown
		/// </summary>
		/// <param name="startStamp">changeStamp at the start of the enumeration.</param>
		private void CheckEnumerationStamp(int startStamp)
		{
			if (startStamp != changeStamp)
			{
				throw new InvalidOperationException(Strings.ChangeDuringEnumeration);
			}
		}

		/// <summary>
		/// Creates the initial buffer and initialized the Deque to contain one initial
		/// item.
		/// </summary>
		/// <param name="firstItem">First and only item for the Deque.</param>
		private void CreateInitialBuffer(T firstItem)
		{
			// The buffer hasn't been created yet.
			buffer = new T[INITIAL_SIZE];
			start = 0;
			end = 1;
			buffer[0] = firstItem;
			return;
		}

		/// <summary>
		/// Increase the amount of buffer space. When calling this method, the Deque
		/// must not be empty. If start and end are equal, that indicates a completely
		/// full Deque.
		/// </summary>
		private void IncreaseBuffer()
		{
			int length = buffer.Length;

			T[] newBuffer = new T[length*2];
			if (start >= end)
			{
				Array.Copy(buffer, start, newBuffer, 0, length - start);
				Array.Copy(buffer, 0, newBuffer, length - start, end);
				end = end + length - start;
				start = 0;
			}
			else
			{
				Array.Copy(buffer, start, newBuffer, 0, end - start);
				end = end - start;
				start = 0;
			}

			buffer = newBuffer;
		}
	}
}