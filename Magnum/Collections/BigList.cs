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
	using System.Diagnostics;

	/// <summary>
	/// BigList&lt;T&gt; provides a list of items, in order, with indices of the items ranging from 0 to one less
	/// than the count of items in the collection. BigList&lt;T&gt; is optimized for efficient operations on large (&gt;100 items)
	/// lists, especially for insertions, deletions, copies, and concatinations.
	/// </summary>
	/// <remarks>
	/// <para>BigList&lt;T&gt; class is similar in functionality to the standard List&lt;T&gt; class. Both classes
	/// provide a collection that stores an set of items in order, with indices of the items ranging from 0 to one less
	/// than the count of items in the collection. Both classes provide the ability to add and remove items from any index,
	/// and the get or set the item at any index.</para> 
	/// <para>BigList&lt;T&gt; differs significantly from List&lt;T&gt; in the performance of various operations, 
	/// especially when the lists become large (several hundred items or more). With List&lt;T&gt;, inserting or removing
	/// elements from anywhere in a large list except the end is very inefficient -- every item after the point of inserting
	/// or deletion has to be moved in the list. The BigList&lt;T&gt; class, however, allows for fast insertions
	/// and deletions anywhere in the list. Furthermore, BigList&lt;T&gt; allows copies of a list, sub-parts
	/// of a list, and concatinations of two lists to be very fast. When a copy is made of part or all of a BigList,
	/// two lists shared storage for the parts of the lists that are the same. Only when one of the lists is changed is additional
	/// memory allocated to store the distinct parts of the lists.</para>
	/// <para>Of course, there is a small price to pay for this extra flexibility. Although still quite efficient, using an 
	/// index to get or change one element of a BigList, while still reasonably efficient, is significantly slower than using
	/// a plain List. Because of this, if you want to process every element of a BigList, using a foreach loop is a lot
	/// more efficient than using a for loop and indexing the list.</para>
	/// <para>In general, use a List when the only operations you are using are Add (to the end), foreach,
	/// or indexing, or you are very sure the list will always remain small (less than 100 items). For large (&gt;100 items) lists
	/// that do insertions, removals, copies, concatinations, or sub-ranges, BigList will be more efficient than List. 
	/// In almost all cases, BigList is more efficient and easier to use than LinkedList.</para>
	/// </remarks>
	/// <typeparam name="T">The type of items to store in the BigList.</typeparam>
	[Serializable]
	public class BigList<T> : ListBase<T>,
		ICloneable
	{
		private const int BALANCEFACTOR = 6; // how far the root must be in depth from fully balanced to invoke the rebalance operation (min 3).
		private const int MAXFIB = 44; // maximum index in the above, not counting the final MaxValue.
		private const uint MAXITEMS = int.MaxValue - 1; // maximum number of items in a BigList.

#if DEBUG
		private const int MAXLEAF = 8; // Maximum number of elements in a leaf node -- small for debugging purposes.
#else
        const int MAXLEAF = 120; // Maximum number of elements in a leaf node. 
#endif

		// The fibonacci numbers. Used in the rebalancing algorithm. Final MaxValue makes sure we don't go off the end.
		private static readonly int[] FIBONACCI = {
			1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584,
			4181, 6765, 10946, 17711, 28657, 46368, 75025, 121393, 196418, 317811, 514229, 832040,
			1346269, 2178309, 3524578, 5702887, 9227465, 14930352, 24157817, 39088169, 63245986,
			102334155, 165580141, 267914296, 433494437, 701408733, 1134903170, 1836311903, int.MaxValue
		};

		// Holds the change stamp for the collection.
		private int changeStamp;
		private Node root;

		/// <summary>
		/// Creates a new BigList. The BigList is initially empty.
		/// </summary>
		/// <remarks>Creating a empty BigList takes constant time and consumes a very small amount of memory.</remarks>
		public BigList()
		{
			root = null;
		}

		/// <summary>
		/// Creates a new BigList initialized with the items from <paramref name="collection"/>, in order.
		/// </summary>
		/// <remarks>Initializing the tree list with the elements of collection takes time O(N), where N is the number of
		/// items in <paramref name="collection"/>.</remarks>
		/// <param name="collection">The collection used to initialize the BigList. </param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public BigList(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			root = NodeFromEnumerable(collection);
			CheckBalance();
		}

		/// <summary>
		/// Creates a new BigList initialized with a given number of copies of the items from <paramref name="collection"/>, in order. 
		/// </summary>
		/// <remarks>Initializing the tree list with the elements of collection takes time O(N + log K), where N is the number of
		/// items in <paramref name="collection"/>, and K is the number of copies.</remarks>
		/// <param name="copies">Number of copies of the collection to use.</param>
		/// <param name="collection">The collection used to initialize the BigList. </param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="copies"/> is negative.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public BigList(IEnumerable<T> collection, int copies)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			root = NCopiesOfNode(copies, NodeFromEnumerable(collection));
			CheckBalance();
		}

		/// <summary>
		/// Creates a new BigList that is a copy of <paramref name="list"/>.
		/// </summary>
		/// <remarks>Copying a BigList takes constant time, and little 
		/// additional memory, since the storage for the items of the two lists is shared. However, changing
		/// either list will take additional time and memory. Portions of the list are copied when they are changed.</remarks>
		/// <param name="list">The BigList to copy. </param>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		public BigList(BigList<T> list)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (list.root == null)
				root = null;
			else
			{
				list.root.MarkShared();
				root = list.root;
			}
		}


		/// <summary>
		/// Creates a new BigList that is several copies of <paramref name="list"/>.
		/// </summary>
		/// <remarks>Creating K copies of a BigList takes time O(log K), and O(log K) 
		/// additional memory, since the storage for the items of the two lists is shared. However, changing
		/// either list will take additional time and memory. Portions of the list are copied when they are changed.</remarks>
		/// <param name="copies">Number of copies of the collection to use.</param>
		/// <param name="list">The BigList to copy. </param>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		public BigList(BigList<T> list, int copies)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (list.root == null)
				root = null;
			else
			{
				list.root.MarkShared();
				root = NCopiesOfNode(copies, list.root);
			}
		}

		/// <summary>
		/// Creates a new BigList from the indicated Node.
		/// </summary>
		/// <param name="node">Node that becomes the new root. If null, the new BigList is empty.</param>
		private BigList(Node node)
		{
			this.root = node;
			CheckBalance();
		}

		/// <summary>
		/// Gets the number of items stored in the BigList. The indices of the items
		/// range from 0 to Count-1.
		/// </summary>
		/// <remarks>Getting the number of items in the BigList takes constant time.</remarks>
		/// <value>The number of items in the BigList.</value>
		public override sealed int Count
		{
			get
			{
				if (root == null)
					return 0;
				else
					return root.Count;
			}
		}

		/// <summary>
		/// Gets or sets an item in the list, by index.
		/// </summary>
		/// <remarks><para> Gettingor setting an item takes time O(log N), where N is the number of items
		/// in the list.</para>
		/// <para>To process each of the items in the list, using GetEnumerator() or a foreach loop is more efficient
		/// that accessing each of the elements by index.</para></remarks>
		/// <param name="index">The index of the item to get or set. The first item in the list
		/// has index 0, the last item has index Count-1.</param>
		/// <returns>The value of the item at the given index.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or 
		/// greater than or equal to Count.</exception>
		public override sealed T this[int index]
		{
			get
			{
				// This could just be a simple call to GetAt on the root.
				// It is recoded as an interative algorithm for performance.

				if (root == null || index < 0 || index >= root.Count)
					throw new ArgumentOutOfRangeException("index");

				Node current = root;
				ConcatNode curConcat = current as ConcatNode;

				while (curConcat != null)
				{
					int leftCount = curConcat.left.Count;
					if (index < leftCount)
						current = curConcat.left;
					else
					{
						current = curConcat.right;
						index -= leftCount;
					}

					curConcat = current as ConcatNode;
				}

				LeafNode curLeaf = (LeafNode) current;
				return curLeaf.items[index];
			}

			set
			{
				// This could just be a simple call to SetAtInPlace on the root.
				// It is recoded as an interative algorithm for performance.

				if (root == null || index < 0 || index >= root.Count)
					throw new ArgumentOutOfRangeException("index");

				// Like List<T>, we stop enumerations after a set operation. This could be made
				// to not happen, but it would be complex, because set operations on a shared node
				// could change the node.
				StopEnumerations();

				if (root.Shared)
					root = root.SetAt(index, value);

				Node current = root;
				ConcatNode curConcat = current as ConcatNode;

				while (curConcat != null)
				{
					int leftCount = curConcat.left.Count;
					if (index < leftCount)
					{
						current = curConcat.left;
						if (current.Shared)
						{
							curConcat.left = current.SetAt(index, value);
							return;
						}
					}
					else
					{
						current = curConcat.right;
						index -= leftCount;
						if (current.Shared)
						{
							curConcat.right = current.SetAt(index, value);
							return;
						}
					}

					curConcat = current as ConcatNode;
				}

				LeafNode curLeaf = (LeafNode) current;
				curLeaf.items[index] = value;
			}
		}

		/// <summary>
		/// Creates a new BigList that is a copy of this list.
		/// </summary>
		/// <remarks>Copying a BigList takes constant time, and little 
		/// additional memory, since the storage for the items of the two lists is shared. However, changing
		/// either list will take additional time and memory. Portions of the list are copied when they are changed.</remarks>
		/// <returns>A copy of the current list</returns>
		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Removes all of the items from the BigList.
		/// </summary>
		/// <remarks>Clearing a BigList takes constant time.</remarks>
		public override sealed void Clear()
		{
			StopEnumerations();
			root = null;
		}

		/// <summary>
		/// Inserts a new item at the given index in the BigList. All items at indexes 
		/// equal to or greater than <paramref name="index"/> move up one index.
		/// </summary>
		/// <remarks>The amount of time to insert an item is O(log N), no matter where
		/// in the list the insertion occurs. Inserting an item at the beginning or end of the 
		/// list is O(N). 
		/// </remarks>
		/// <param name="index">The index to insert the item at. After the
		/// insertion, the inserted item is located at this index. The
		/// first item has index 0.</param>
		/// <param name="item">The item to insert at the given index.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than Count.</exception>
		public override sealed void Insert(int index, T item)
		{
			StopEnumerations();

			if ((uint) Count + 1 > MAXITEMS)
				throw new InvalidOperationException(Strings.CollectionTooLarge);

			if (index <= 0 || index >= Count)
			{
				if (index == 0)
					AddToFront(item);
				else if (index == Count)
					Add(item);
				else
					throw new ArgumentOutOfRangeException("index");
			}
			else
			{
				if (root == null)
					root = new LeafNode(item);
				else
				{
					Node newRoot = root.InsertInPlace(index, item);
					if (newRoot != root)
					{
						root = newRoot;
						CheckBalance();
					}
				}
			}
		}

		/// <summary>
		/// Inserts a collection of items at the given index in the BigList. All items at indexes 
		/// equal to or greater than <paramref name="index"/> increase their indices 
		/// by the number of items inserted.
		/// </summary>
		/// <remarks>The amount of time to insert an arbitrary collection in the BigList is O(M + log N), 
		/// where M is the number of items inserted, and N is the number of items in the list.
		/// </remarks>
		/// <param name="index">The index to insert the collection at. After the
		/// insertion, the first item of the inserted collection is located at this index. The
		/// first item has index 0.</param>
		/// <param name="collection">The collection of items to insert at the given index.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than Count.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public void InsertRange(int index, IEnumerable<T> collection)
		{
			StopEnumerations();

			if (collection == null)
				throw new ArgumentNullException("collection");

			if (index <= 0 || index >= Count)
			{
				if (index == 0)
					AddRangeToFront(collection);
				else if (index == Count)
					AddRange(collection);
				else
					throw new ArgumentOutOfRangeException("index");
			}
			else
			{
				Node node = NodeFromEnumerable(collection);
				if (node == null)
					return;
				else if (root == null)
					root = node;
				else
				{
					if ((uint) Count + (uint) node.Count > MAXITEMS)
						throw new InvalidOperationException(Strings.CollectionTooLarge);

					Node newRoot = root.InsertInPlace(index, node, true);
					if (newRoot != root)
					{
						root = newRoot;
						CheckBalance();
					}
				}
			}
		}

		/// <summary>
		/// Inserts a BigList of items at the given index in the BigList. All items at indexes 
		/// equal to or greater than <paramref name="index"/> increase their indices 
		/// by the number of items inserted.
		/// </summary>
		/// <remarks>The amount of time to insert another BigList is O(log N), 
		/// where N is the number of items in the list, regardless of the number of items in the 
		/// inserted list. Storage is shared between the two lists until one of them is changed.
		/// </remarks>
		/// <param name="index">The index to insert the collection at. After the
		/// insertion, the first item of the inserted collection is located at this index. The
		/// first item has index 0.</param>
		/// <param name="list">The BigList of items to insert at the given index.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than Count.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		public void InsertRange(int index, BigList<T> list)
		{
			StopEnumerations();

			if (list == null)
				throw new ArgumentNullException("list");
			if ((uint) Count + (uint) list.Count > MAXITEMS)
				throw new InvalidOperationException(Strings.CollectionTooLarge);

			if (index <= 0 || index >= Count)
			{
				if (index == 0)
					AddRangeToFront(list);
				else if (index == Count)
					AddRange(list);
				else
					throw new ArgumentOutOfRangeException("index");
			}
			else
			{
				if (list.Count == 0)
					return;

				if (root == null)
				{
					list.root.MarkShared();
					root = list.root;
				}
				else
				{
					if (list.root == root)
						root.MarkShared(); // make sure inserting into itself works.

					Node newRoot = root.InsertInPlace(index, list.root, false);
					if (newRoot != root)
					{
						root = newRoot;
						CheckBalance();
					}
				}
			}
		}

		/// <summary>
		/// Removes the item at the given index in the BigList. All items at indexes 
		/// greater than <paramref name="index"/> move down one index.
		/// </summary>
		/// <remarks>The amount of time to delete an item in the BigList is O(log N),
		/// where N is the number of items in the list. 
		/// </remarks>
		/// <param name="index">The index in the list to remove the item at. The
		/// first item in the list has index 0.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than or equal to Count.</exception>
		public override sealed void RemoveAt(int index)
		{
			RemoveRange(index, 1);
		}

		/// <summary>
		/// Removes a range of items at the given index in the Deque. All items at indexes 
		/// greater than <paramref name="index"/> move down <paramref name="count"/> indices
		/// in the Deque.
		/// </summary>
		/// <remarks>The amount of time to delete <paramref name="count"/> items in the Deque is proportional
		/// to the distance of index from the closest end of the Deque, plus <paramref name="count"/>: 
		/// O(count + Min(<paramref name="index"/>, Count - 1 - <paramref name="index"/>)).
		/// </remarks>
		/// <param name="index">The index in the list to remove the range at. The
		/// first item in the list has index 0.</param>
		/// <param name="count">The number of items to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
		/// less than zero or greater than or equal to Count, or <paramref name="count"/> is less than zero
		/// or too large.</exception>
		public void RemoveRange(int index, int count)
		{
			if (count == 0)
				return; // nothing to do.
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException("index");
			if (count < 0 || count > Count - index)
				throw new ArgumentOutOfRangeException("count");

			StopEnumerations();

			Node newRoot = root.RemoveRangeInPlace(index, index + count - 1);
			if (newRoot != root)
			{
				root = newRoot;
				CheckBalance();
			}
		}

		/// <summary>
		/// Adds an item to the end of the BigList. The indices of all existing items
		/// in the Deque are unchanged. 
		/// </summary>
		/// <remarks>Adding an item takes, on average, constant time.</remarks>
		/// <param name="item">The item to add.</param>
		public override sealed void Add(T item)
		{
			if ((uint) Count + 1 > MAXITEMS)
				throw new InvalidOperationException(Strings.CollectionTooLarge);

			StopEnumerations();

			if (root == null)
				root = new LeafNode(item);
			else
			{
				Node newRoot = root.AppendInPlace(item);
				if (newRoot != root)
				{
					root = newRoot;
					CheckBalance();
				}
			}
		}

		/// <summary>
		/// Adds an item to the beginning of the BigList. The indices of all existing items
		/// in the Deque are increased by one, and the new item has index zero. 
		/// </summary>
		/// <remarks>Adding an item takes, on average, constant time.</remarks>
		/// <param name="item">The item to add.</param>
		public void AddToFront(T item)
		{
			if ((uint) Count + 1 > MAXITEMS)
				throw new InvalidOperationException(Strings.CollectionTooLarge);

			StopEnumerations();

			if (root == null)
				root = new LeafNode(item);
			else
			{
				Node newRoot = root.PrependInPlace(item);
				if (newRoot != root)
				{
					root = newRoot;
					CheckBalance();
				}
			}
		}

		/// <summary>
		/// Adds a collection of items to the end of BigList. The indices of all existing items
		/// are unchanged. The last item in the added collection becomes the
		/// last item in the BigList.
		/// </summary>
		/// <remarks>This method takes time O(M + log N), where M is the number of items in the 
		/// <paramref name="collection"/>, and N is the size of the BigList.</remarks>
		/// <param name="collection">The collection of items to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public void AddRange(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			StopEnumerations();

			Node node = NodeFromEnumerable(collection);
			if (node == null)
				return;
			else if (root == null)
			{
				root = node;
				CheckBalance();
			}
			else
			{
				if ((uint) Count + (uint) node.count > MAXITEMS)
					throw new InvalidOperationException(Strings.CollectionTooLarge);

				Node newRoot = root.AppendInPlace(node, true);
				if (newRoot != root)
				{
					root = newRoot;
					CheckBalance();
				}
			}
		}

		/// <summary>
		/// Adds a collection of items to the front of BigList. The indices of all existing items
		/// in the are increased by the number of items in <paramref name="collection"/>. 
		/// The first item in the added collection becomes the first item in the BigList.
		/// </summary>
		/// <remarks>This method takes time O(M + log N), where M is the number of items in the 
		/// <paramref name="collection"/>, and N is the size of the BigList.</remarks>
		/// <param name="collection">The collection of items to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public void AddRangeToFront(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			StopEnumerations();

			Node node = NodeFromEnumerable(collection);
			if (node == null)
				return;
			else if (root == null)
			{
				root = node;
				CheckBalance();
			}
			else
			{
				if ((uint) Count + (uint) node.Count > MAXITEMS)
					throw new InvalidOperationException(Strings.CollectionTooLarge);

				Node newRoot = root.PrependInPlace(node, true);
				if (newRoot != root)
				{
					root = newRoot;
					CheckBalance();
				}
			}
		}

		/// <summary>
		/// Creates a new BigList that is a copy of this list.
		/// </summary>
		/// <remarks>Copying a BigList takes constant time, and little 
		/// additional memory, since the storage for the items of the two lists is shared. However, changing
		/// either list will take additional time and memory. Portions of the list are copied when they are changed.</remarks>
		/// <returns>A copy of the current list</returns>
		public BigList<T> Clone()
		{
			if (root == null)
				return new BigList<T>();
			else
			{
				root.MarkShared();
				return new BigList<T>(root);
			}
		}

		/// <summary>
		/// Makes a deep clone of this BigList. A new BigList is created with a clone of
		/// each element of this set, by calling ICloneable.Clone on each element. If T is
		/// a value type, then this method is the same as Clone.
		/// </summary>
		/// <remarks><para>If T is a reference type, it must implement
		/// ICloneable. Otherwise, an InvalidOperationException is thrown.</para>
		/// <para>If T is a reference type, cloning the list takes time approximate O(N), where N is the number of items in the list.</para></remarks>
		/// <returns>The cloned set.</returns>
		/// <exception cref="InvalidOperationException">T is a reference type that does not implement ICloneable.</exception>
		public BigList<T> CloneContents()
		{
			if (root == null)
				return new BigList<T>();
			else
			{
				bool itemIsValueType;
				if (!Util.IsCloneableType(typeof (T), out itemIsValueType))
					throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, typeof (T).FullName));

				if (itemIsValueType)
					return Clone();

				// Create a new list by converting each item in this list via cloning.
				return new BigList<T>(Algorithms.Convert(this, delegate(T item)
					{
						if (item == null)
							return default(T); // Really null, because we know T is a reference type
						else
							return (T) (((ICloneable) item).Clone());
					}));
			}
		}

		/// <summary>
		/// Adds a BigList of items to the end of BigList. The indices of all existing items
		/// are unchanged. The last item in <paramref name="list"/> becomes the
		/// last item in this list. The added list <paramref name="list"/> is unchanged.
		/// </summary>
		/// <remarks>This method takes, on average, constant time, regardless of the size
		/// of either list. Although conceptually all of the items in <paramref name="list"/> are
		/// copied, storage is shared between the two lists until changes are made to the 
		/// shared sections.</remarks>
		/// <param name="list">The list of items to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		public void AddRange(BigList<T> list)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if ((uint) Count + (uint) list.Count > MAXITEMS)
				throw new InvalidOperationException(Strings.CollectionTooLarge);

			if (list.Count == 0)
				return;

			StopEnumerations();

			if (root == null)
			{
				list.root.MarkShared();
				root = list.root;
			}
			else
			{
				Node newRoot = root.AppendInPlace(list.root, false);
				if (newRoot != root)
				{
					root = newRoot;
					CheckBalance();
				}
			}
		}

		/// <summary>
		/// Adds a BigList of items to the front of BigList. The indices of all existing items
		/// are increased by the number of items in <paramref name="list"/>. The first item in <paramref name="list"/> 
		/// becomes the first item in this list. The added list <paramref name="list"/> is unchanged.
		/// </summary>
		/// <remarks>This method takes, on average, constant time, regardless of the size
		/// of either list. Although conceptually all of the items in <paramref name="list"/> are
		/// copied, storage is shared between the two lists until changes are made to the 
		/// shared sections.</remarks>
		/// <param name="list">The list of items to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		public void AddRangeToFront(BigList<T> list)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if ((uint) Count + (uint) list.Count > MAXITEMS)
				throw new InvalidOperationException(Strings.CollectionTooLarge);

			if (list.Count == 0)
				return;

			StopEnumerations();

			if (root == null)
			{
				list.root.MarkShared();
				root = list.root;
			}
			else
			{
				Node newRoot = root.PrependInPlace(list.root, false);
				if (newRoot != root)
				{
					root = newRoot;
					CheckBalance();
				}
			}
		}

		/// <summary>
		/// Creates a new list that contains a subrange of elements from this list. The
		/// current list is unchanged.
		/// </summary>
		/// <remarks>This method takes take O(log N), where N is the size of the current list. Although
		/// the sub-range is conceptually copied, storage is shared between the two lists until a change
		/// is made to the shared items.</remarks>
		/// <remarks>If a view of a sub-range is desired, instead of a copy, use the
		/// more efficient <see cref="Range"/> method, which provides a view onto a sub-range of items.</remarks>
		/// <param name="index">The starting index of the sub-range.</param>
		/// <param name="count">The number of items in the sub-range. If this is zero,
		/// the returned list is empty.</param>
		/// <returns>A new list with the <paramref name="count"/> items that start at <paramref name="index"/>.</returns>
		public BigList<T> GetRange(int index, int count)
		{
			if (count == 0)
				return new BigList<T>();

			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException("index");
			if (count < 0 || count > Count - index)
				throw new ArgumentOutOfRangeException("count");

			return new BigList<T>(root.Subrange(index, index + count - 1));
		}

		/// <summary>
		/// Returns a view onto a sub-range of this list. Items are not copied; the
		/// returned IList&lt;T&gt; is simply a different view onto the same underlying items. Changes to this list
		/// are reflected in the view, and vice versa. Insertions and deletions in the view change the size of the 
		/// view, but insertions and deletions in the underlying list do not.
		/// </summary>
		/// <remarks>
		/// <para>If a copy of the sub-range is desired, use the <see cref="GetRange"/> method instead.</para>
		/// <para>This method can be used to apply an algorithm to a portion of a list. For example:</para>
		/// <code>Algorithms.ReverseInPlace(list.Range(3, 6))</code>
		/// will reverse the 6 items beginning at index 3.</remarks>
		/// <param name="index">The starting index of the view.</param>
		/// <param name="count">The number of items in the view.</param>
		/// <returns>A list that is a view onto the given sub-list. </returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> + <paramref name="count"/> is greater than the
		/// size of this list.</exception>
		public override sealed IList<T> Range(int index, int count)
		{
			if (index < 0 || index > this.Count || (index == this.Count && count != 0))
				throw new ArgumentOutOfRangeException("index");
			if (count < 0 || count > this.Count || count + index > this.Count)
				throw new ArgumentOutOfRangeException("count");

			return new BigListRange(this, index, count);
		}

		/// <summary>
		/// Enumerates all of the items in the list, in order. The item at index 0
		/// is enumerated first, then the item at index 1, and so on. Usually, the
		/// foreach statement is used to call this method implicitly.
		/// </summary>
		/// <remarks>Enumerating all of the items in the list take time O(N), where
		/// N is the number of items in the list. Using GetEnumerator() or foreach
		/// is much more efficient than accessing all items by index.</remarks>
		/// <returns>An IEnumerator&lt;T&gt; that enumerates all the
		/// items in the list.</returns>
		public override sealed IEnumerator<T> GetEnumerator()
		{
			return GetEnumerator(0, int.MaxValue);
		}

		/// <summary>
		/// Convert the list to a new list by applying a delegate to each item in the collection. The resulting list
		/// contains the result of applying <paramref name="converter"/> to each item in the list, in
		/// order. The current list is unchanged.
		/// </summary>
		/// <typeparam name="TDest">The type each item is being converted to.</typeparam>
		/// <param name="converter">A delegate to the method to call, passing each item in <type name="BigList&lt;T&gt;"/>.</param>
		/// <returns>The resulting BigList from applying <paramref name="converter"/> to each item in this list.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="converter"/> is null.</exception>
		public new BigList<TDest> ConvertAll<TDest>(Converter<T, TDest> converter)
		{
			return new BigList<TDest>(Algorithms.Convert(this, converter));
		}

		/// <summary>
		/// Reverses the current list in place.
		/// </summary>
		public void Reverse()
		{
			Algorithms.ReverseInPlace(this);
		}

		/// <summary>
		/// Reverses the items in the range of <paramref name="count"/> items starting from <paramref name="start"/>, in place.
		/// </summary>
		/// <param name="start">The starting index of the range to reverse.</param>
		/// <param name="count">The number of items in range to reverse.</param>
		public void Reverse(int start, int count)
		{
			Algorithms.ReverseInPlace(Range(start, count));
		}

		/// <summary>
		/// Sorts the list in place.
		/// </summary>
		/// <remarks><para>The Quicksort algorithm is used to sort the items. In virtually all cases,
		/// this takes time O(N log N), where N is the number of items in the list.</para>
		/// <para>Values are compared by using the IComparable or IComparable&lt;T&gt;
		/// interface implementation on the type T.</para></remarks>
		/// <exception cref="InvalidOperationException">The type T does not implement either the IComparable or
		/// IComparable&lt;T&gt; interfaces.</exception>
		public void Sort()
		{
			Sort(Comparers.DefaultComparer<T>());
		}

		/// <summary>
		/// Sorts the list in place. A supplied IComparer&lt;T&gt; is used
		/// to compare the items in the list. 
		/// </summary>
		/// <remarks>The Quicksort algorithms is used to sort the items. In virtually all cases,
		/// this takes time O(N log N), where N is the number of items in the list.</remarks>
		/// <param name="comparer">The comparer instance used to compare items in the collection. Only
		/// the Compare method is used.</param>
		public void Sort(IComparer<T> comparer)
		{
			Algorithms.SortInPlace(this, comparer);
		}

		/// <summary>
		/// Sorts the list in place. A supplied Comparison&lt;T&gt; delegate is used
		/// to compare the items in the list.
		/// </summary>
		/// <remarks>The Quicksort algorithms is used to sort the items. In virtually all cases,
		/// this takes time O(N log N), where N is the number of items in the list.</remarks>
		/// <param name="comparison">The comparison delegate used to compare items in the collection.</param>
		public void Sort(Comparison<T> comparison)
		{
			Sort(Comparers.ComparerFromComparison(comparison));
		}


		/// <summary>
		/// Searches a sorted list for an item via binary search. The list must be sorted
		/// in the order defined by the default ordering of the item type; otherwise, 
		/// incorrect results will be returned.
		/// </summary>
		/// <param name="item">The item to search for.</param>
		/// <returns>Returns the index of the first occurence of <paramref name="item"/> in the list. If the item does not occur
		/// in the list, the bitwise complement of the first item larger than <paramref name="item"/> in the list is returned. If no item is 
		/// larger than <paramref name="item"/>, the bitwise complement of Count is returned.</returns>
		/// <exception cref="InvalidOperationException">The type T does not implement either the IComparable or
		/// IComparable&lt;T&gt; interfaces.</exception>
		public int BinarySearch(T item)
		{
			return BinarySearch(item, Comparers.DefaultComparer<T>());
		}

		/// <summary>
		/// Searches a sorted list for an item via binary search. The list must be sorted
		/// by the ordering defined by the passed IComparer&lt;T&gt; interface; otherwise, 
		/// incorrect results will be returned.
		/// </summary>
		/// <param name="item">The item to search for.</param>
		/// <param name="comparer">The IComparer&lt;T&gt; interface used to sort the list.</param>
		/// <returns>Returns the index of the first occurence of <paramref name="item"/> in the list. If the item does not occur
		/// in the list, the bitwise complement of the first item larger than <paramref name="item"/> in the list is returned. If no item is 
		/// larger than <paramref name="item"/>, the bitwise complement of Count is returned.</returns>
		public int BinarySearch(T item, IComparer<T> comparer)
		{
			int count, index;

			count = Algorithms.BinarySearch(this, item, comparer, out index);
			if (count == 0)
				return (~index);
			else
				return index;
		}

		/// <summary>
		/// Searches a sorted list for an item via binary search. The list must be sorted
		/// by the ordering defined by the passed Comparison&lt;T&gt; delegate; otherwise, 
		/// incorrect results will be returned.
		/// </summary>
		/// <param name="item">The item to search for.</param>
		/// <param name="comparison">The comparison delegate used to sort the list.</param>
		/// <returns>Returns the index of the first occurence of <paramref name="item"/> in the list. If the item does not occur
		/// in the list, the bitwise complement of the first item larger than <paramref name="item"/> in the list is returned. If no item is 
		/// larger than <paramref name="item"/>, the bitwise complement of Count is returned.</returns>
		public int BinarySearch(T item, Comparison<T> comparison)
		{
			return BinarySearch(item, Comparers.ComparerFromComparison(comparison));
		}


		/// <summary>
		/// Attempts to validate the internal consistency of the tree.
		/// </summary>
		public void Validate()
		{
			if (root != null)
			{
				root.Validate();
				Debug.Assert(Count != 0);
			}
			else
				Debug.Assert(Count == 0);
		}

		/// <summary>
		/// Prints out the internal structure of the tree, for debugging purposes.
		/// </summary>
		public void Print()
		{
			Console.WriteLine("SERIES: Count={0}", Count);
			if (Count > 0)
			{
				Console.Write("ITEMS: ");
				foreach (T item in this)
				{
					Console.Write("{0} ", item);
				}
				Console.WriteLine();
				Console.WriteLine("TREE:");
				root.Print("      ", "      ");
			}
			Console.WriteLine();
		}

		/// <summary>
		/// Rebalance the current tree. Once rebalanced, the depth of the current tree is no more than
		/// two levels from fully balanced, where fully balanced is defined as having Fibonacci(N+2) or more items
		/// in a tree of depth N.
		/// </summary>
		/// <remarks>The rebalancing algorithm is from "Ropes: an Alternative to Strings", by 
		/// Boehm, Atkinson, and Plass, in SOFTWARE--PRACTICE AND EXPERIENCE, VOL. 25(12), 1315–1330 (DECEMBER 1995).
		/// </remarks>
		internal void Rebalance()
		{
			Node[] rebalanceArray;
			int slots;

			// The basic rebalancing algorithm is add nodes to a rabalance array, where a node at index K in the 
			// rebalance array has Fibonacci(K+1) to Fibonacci(K+2) items, and the entire list has the nodes
			// from largest to smallest concatenated.

			if (root == null)
				return;
			if (root.Depth <= 1 || (root.Depth - 2 <= MAXFIB && Count >= FIBONACCI[root.Depth - 2]))
				return; // already sufficiently balanced.

			// How many slots does the rebalance array need?
			for (slots = 0; slots <= MAXFIB; ++slots)
				if (root.Count < FIBONACCI[slots])
					break;
			rebalanceArray = new Node[slots];

			// Add all the nodes to the rebalance array.
			AddNodeToRebalanceArray(rebalanceArray, root, false);

			// Concatinate all the node in the rebalance array.
			Node result = null;
			for (int slot = 0; slot < slots; ++slot)
			{
				Node n = rebalanceArray[slot];
				if (n != null)
				{
					if (result == null)
						result = n;
					else
						result = result.PrependInPlace(n, !n.Shared);
				}
			}

			// And we're done. Check that it worked!
			root = result;
			Debug.Assert(root.Depth <= 1 || (root.Depth - 2 <= MAXFIB && Count >= FIBONACCI[root.Depth - 2]));
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
		/// Enumerates a range of the items in the list, in order. The item at <paramref name="start"/>
		/// is enumerated first, then the next item at index 1, and so on. At most <paramref name="maxItems"/>
		/// items are enumerated. 
		/// </summary>
		/// <remarks>Enumerating all of the items in the list take time O(N), where
		/// N is the number of items being enumerated. Using GetEnumerator() or foreach
		/// is much more efficient than accessing all items by index.</remarks>
		/// <param name="start">Index to start enumerating at.</param>
		/// <param name="maxItems">Max number of items to enumerate.</param>
		/// <returns>An IEnumerator&lt;T&gt; that enumerates all the
		/// items in the given range.</returns>
		private IEnumerator<T> GetEnumerator(int start, int maxItems)
		{
			// We could use a recursive enumerator here, but an explicit stack
			// is a lot more efficient, and efficiency matters here.

			int startStamp = changeStamp; // to detect changes during enumeration.

			if (root != null && maxItems > 0)
			{
				ConcatNode[] stack = new ConcatNode[root.Depth];
				bool[] leftStack = new bool[root.Depth];
				int stackPtr = 0, startIndex = 0;
				Node current = root;
				LeafNode currentLeaf;
				ConcatNode currentConcat;

				if (start != 0)
				{
					// Set current to the node containing start, and set startIndex to
					// the index within that node.
					if (start < 0 || start >= root.Count)
						throw new ArgumentOutOfRangeException("start");

					currentConcat = current as ConcatNode;
					startIndex = start;
					while (currentConcat != null)
					{
						stack[stackPtr] = currentConcat;

						int leftCount = currentConcat.left.Count;
						if (startIndex < leftCount)
						{
							leftStack[stackPtr] = true;
							current = currentConcat.left;
						}
						else
						{
							leftStack[stackPtr] = false;
							current = currentConcat.right;
							startIndex -= leftCount;
						}

						++stackPtr;
						currentConcat = current as ConcatNode;
					}
				}

				for (;;)
				{
					// If not already at a leaf, walk to the left to find a leaf node.
					while ((currentConcat = current as ConcatNode) != null)
					{
						stack[stackPtr] = currentConcat;
						leftStack[stackPtr] = true;
						++stackPtr;
						current = currentConcat.left;
					}

					// Iterate the leaf.
					currentLeaf = (LeafNode) current;

					int limit = currentLeaf.Count;
					if (limit > startIndex + maxItems)
						limit = startIndex + maxItems;

					for (int i = startIndex; i < limit; ++i)
					{
						yield return currentLeaf.items[i];
						CheckEnumerationStamp(startStamp);
					}

					// Update the number of items to interate.
					maxItems -= limit - startIndex;
					if (maxItems <= 0)
						yield break; // Done!

					// From now on, start enumerating at 0.
					startIndex = 0;

					// Go back up the stack until we find a place to the right
					// we didn't just come from.
					for (;;)
					{
						ConcatNode parent;
						if (stackPtr == 0)
							yield break; // iteration is complete.

						parent = stack[--stackPtr];
						if (leftStack[stackPtr])
						{
							leftStack[stackPtr] = false;
							++stackPtr;
							current = parent.right;
							break;
						}

						current = parent;
						// And keep going up...
					}

					// current is now a new node we need to visit. Loop around to get it.
				}
			}
		}

		/// <summary>
		/// Check the balance of the current tree and rebalance it if it is more than BALANCEFACTOR
		/// levels away from fully balanced. Note that rebalancing a tree may leave it two levels away from 
		/// fully balanced.
		/// </summary>
		private void CheckBalance()
		{
			if (root != null &&
			    (root.Depth > BALANCEFACTOR && !(root.Depth - BALANCEFACTOR <= MAXFIB && Count >= FIBONACCI[root.Depth - BALANCEFACTOR])))
			{
				Rebalance();
			}
		}

		/// <summary>
		/// Part of the rebalancing algorithm. Adds a node to the rebalance array. If it is already balanced, add it directly, otherwise
		/// add its children.
		/// </summary>
		/// <param name="rebalanceArray">Rebalance array to insert into.</param>
		/// <param name="node">Node to add.</param>
		/// <param name="shared">If true, mark the node as shared before adding, because one
		/// of its parents was shared.</param>
		private void AddNodeToRebalanceArray(Node[] rebalanceArray, Node node, bool shared)
		{
			if (node.Shared)
				shared = true;

			if (node.IsBalanced())
			{
				if (shared)
					node.MarkShared();
				AddBalancedNodeToRebalanceArray(rebalanceArray, node);
			}
			else
			{
				ConcatNode n = (ConcatNode) node; // leaf nodes are always balanced.
				AddNodeToRebalanceArray(rebalanceArray, n.left, shared);
				AddNodeToRebalanceArray(rebalanceArray, n.right, shared);
			}
		}

		/// <summary>
		/// The base class for the two kinds of nodes in the tree: Concat nodes
		/// and Leaf nodes.
		/// </summary>
		[Serializable]
		private abstract class Node
		{
			// Number of items in this node.
			public int count;

			// If true, indicates that this node is referenced by multiple 
			// concat nodes or multiple BigList. Neither this node nor 
			// nodes below it may be modifed ever again. Never becomes
			// false after being set to true. It's volatile so that accesses
			// from another thread work appropriately -- if shared is set
			// to true, no other thread will attempt to change the node.
			protected volatile bool shared;

			/// <summary>
			/// The number of items stored in the node (or below it).
			/// </summary>
			/// <value>The number of items in the node or below.</value>
			public int Count
			{
				get { return count; }
			}

			/// <summary>
			/// Is this node shared by more that one list (or within a single)
			/// lists. If true, indicates that this node, and any nodes below it,
			/// may never be modified. Never becomes false after being set to 
			/// true.
			/// </summary>
			/// <value></value>
			public bool Shared
			{
				get { return shared; }
			}

			/// <summary>
			/// Gets the depth of this node. A leaf node has depth 0, 
			/// a concat node with two leaf children has depth 1, etc.
			/// </summary>
			/// <value>The depth of this node.</value>
			public abstract int Depth { get; }

			/// <summary>
			/// Marks this node as shared by setting the shared variable.
			/// </summary>
			public void MarkShared()
			{
				shared = true;
			}

			/// <summary>
			/// Returns the items at the given index in this node.
			/// </summary>
			/// <param name="index">0-based index, relative to this node.</param>
			/// <returns>Item at that index.</returns>
			public abstract T GetAt(int index);

			/// <summary>
			/// Returns a node that has a sub-range of items from this node. The
			/// sub-range may not be empty, but may extend outside the node. 
			/// In other words, first might be less than zero or last might be greater
			/// than count. But, last can't be less than zero and first can't be
			/// greater than count. Also, last must be greater than or equal to last.
			/// </summary>
			/// <param name="first">Inclusive first element, relative to this node.</param>
			/// <param name="last">Inclusize last element, relative to this node.</param>
			/// <returns>Node with the given sub-range.</returns>
			public abstract Node Subrange(int first, int last);

			// Any operation that could potentially modify a node exists
			// in two forms -- the "in place" form that possibly modifies the
			// node, and the non-"in place" that returns a new node with 
			// the modification. However, even "in-place" operations may return
			// a new node, because a shared node can never be modified, even
			// by an in-place operation.


			/// <summary>
			/// Changes the item at the given index. Never changes this node,
			/// but always returns a new node with the given item changed.
			/// </summary>
			/// <param name="index">Index, relative to this node, to change.</param>
			/// <param name="item">New item to place at the given index.</param>
			/// <returns>A new node with the given item changed.</returns>
			public abstract Node SetAt(int index, T item);

			/// <summary>
			/// Changes the item at the given index. May change this node,
			/// or return a new node with the given item changed.
			/// </summary>
			/// <param name="index">Index, relative to this node, to change.</param>
			/// <param name="item">New item to place at the given index.</param>
			/// <returns>A node with the give item changed. If it can be done in place
			/// then "this" is returned.</returns>
			public abstract Node SetAtInPlace(int index, T item);

			/// <summary>
			/// Append a node after this node. Never changes this node, but returns
			/// a new node with the given appending done.
			/// </summary>
			/// <param name="node">Node to append.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A new node with the give node appended to this node.</returns>
			public abstract Node Append(Node node, bool nodeIsUnused);

			/// <summary>
			/// Append a node after this node. May change this node, or return 
			/// a new node.
			/// </summary>
			/// <param name="node">Node to append.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A node with the give node appended to this node. May be a new
			/// node or the current node.</returns>
			public abstract Node AppendInPlace(Node node, bool nodeIsUnused);

			/// <summary>
			/// Append a item after this node. May change this node, or return 
			/// a new node. Equivalent to AppendInPlace(new LeafNode(item), true), but
			/// may be more efficient because a new LeafNode might not be allocated.
			/// </summary>
			/// <param name="item">Item to append.</param>
			/// <returns>A node with the given item appended to this node. May be a new
			/// node or the current node.</returns>
			public abstract Node AppendInPlace(T item);

			/// <summary>
			/// Remove a range of items from this node. Never changes this node, but returns
			/// a new node with the removing done. The
			/// sub-range may not be empty, but may extend outside the node. 
			/// In other words, first might be less than zero or last might be greater
			/// than count. But, last can't be less than zero and first can't be
			/// greater than count. Also, last must be greater than or equal to last.
			/// </summary>
			/// <param name="first">Inclusive index of first item in sub-range, relative
			/// to this node.</param>
			/// <param name="last">Inclusize index of last item in sub-range, relative
			/// to this node.</param>
			/// <returns>A new node with the sub-range removed.</returns>
			public abstract Node RemoveRange(int first, int last);

			/// <summary>
			/// Remove a range of items from this node. May change this node, or returns
			/// a new node with the given appending done. The
			/// sub-range may not be empty, but may extend outside the node. 
			/// In other words, first might be less than zero or last might be greater
			/// than count. But, last can't be less than zero and first can't be
			/// greater than count. Also, last must be greater than or equal to last.
			/// </summary>
			/// <param name="first">Inclusive index of first item in sub-range, relative
			/// to this node.</param>
			/// <param name="last">Inclusize index of last item in sub-range, relative
			/// to this node.</param>
			/// <returns>A node with the sub-range removed. If done in-place, returns
			/// "this".</returns>
			public abstract Node RemoveRangeInPlace(int first, int last);

			/// <summary>
			/// Inserts a node inside this node. Never changes this node, but returns
			/// a new node with the given appending done.
			/// </summary>
			/// <param name="index">Index, relative to this node, to insert at. Must 
			/// be in bounds.</param>
			/// <param name="node">Node to insert.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A new node with the give node inserted.</returns>
			public abstract Node Insert(int index, Node node, bool nodeIsUnused);

			/// <summary>
			/// Inserts an item inside this node. May change this node, or return
			/// a new node with the given appending done. Equivalent to 
			/// InsertInPlace(new LeafNode(item), true), but may be more efficient.
			/// </summary>
			/// <param name="index">Index, relative to this node, to insert at. Must 
			/// be in bounds.</param>
			/// <param name="item">Item to insert.</param>
			/// <returns>A node with the give item inserted. If done in-place, returns
			/// "this".</returns>
			public abstract Node InsertInPlace(int index, T item);

			/// <summary>
			/// Inserts a node inside this node. May change this node, or return
			/// a new node with the given appending done.
			/// </summary>
			/// <param name="index">Index, relative to this node, to insert at. Must 
			/// be in bounds.</param>
			/// <param name="node">Node to insert.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A node with the given item inserted. If done in-place, returns
			/// "this".</returns>
			public abstract Node InsertInPlace(int index, Node node, bool nodeIsUnused);

			/// <summary>
			/// Validates the node for consistency, as much as possible. Also validates
			/// child nodes, if any.
			/// </summary>
			public abstract void Validate();

			/// <summary>
			/// Print out the contents of this node.
			/// </summary>
			/// <param name="prefixNode">Prefix to use in front of this node.</param>
			/// <param name="prefixChildren">Prefixed to use in front of children of this node.</param>
			public abstract void Print(string prefixNode, string prefixChildren);

			/// <summary>
			/// Prefpend a node before this node. Never changes this node, but returns
			/// a new node with the given prepending done.
			/// </summary>
			/// <param name="node">Node to prepend.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A new node with the give node prepended to this node.</returns>
			public Node Prepend(Node node, bool nodeIsUnused)
			{
				if (nodeIsUnused)
					return node.AppendInPlace(this, false);
				else
					return node.Append(this, false);
			}

			/// <summary>
			/// Prepend a node before this node. May change this node, or return 
			/// a new node.
			/// </summary>
			/// <param name="node">Node to prepend.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A node with the give node prepended to this node. May be a new
			/// node or the current node.</returns>
			public Node PrependInPlace(Node node, bool nodeIsUnused)
			{
				if (nodeIsUnused)
					return node.AppendInPlace(this, !this.shared);
				else
					return node.Append(this, !this.shared);
			}

			/// <summary>
			/// Prepend a item before this node. May change this node, or return 
			/// a new node. Equivalent to PrependInPlace(new LeafNode(item), true), but
			/// may be more efficient because a new LeafNode might not be allocated.
			/// </summary>
			/// <param name="item">Item to prepend.</param>
			/// <returns>A node with the given item prepended to this node. May be a new
			/// node or the current node.</returns>
			public abstract Node PrependInPlace(T item);

			/// <summary>
			/// Determine if this node is balanced. A node is balanced if the number
			/// of items is greater than
			/// Fibonacci(Depth+2). Balanced nodes are never rebalanced unless
			/// they go out of balance again.
			/// </summary>
			/// <returns>True if the node is balanced by this definition.</returns>
			public bool IsBalanced()
			{
				return (Depth <= MAXFIB && Count >= FIBONACCI[Depth]);
			}

			/// <summary>
			/// Determine if this node is almost balanced. A node is almost balanced if t
			/// its depth is at most one greater than a fully balanced node with the same count.
			/// </summary>
			/// <returns>True if the node is almost balanced by this definition.</returns>
			public bool IsAlmostBalanced()
			{
				return (Depth == 0 || (Depth - 1 <= MAXFIB && Count >= FIBONACCI[Depth - 1]));
			}
		}

		/// <summary>
		/// The LeafNode class is the type of node that lives at the leaf of a tree and holds
		/// the actual items stored in the list. Each leaf holds at least 1, and at most MAXLEAF
		/// items in the items array. The number of items stored is found in "count", which may
		/// be less than "items.Length".
		/// </summary>
		[Serializable]
		private sealed class LeafNode : Node
		{
			/// <summary>
			/// Array that stores the items in the nodes. Always has a least "count" elements,
			/// but may have more as padding.
			/// </summary>
			public T[] items;

			/// <summary>
			/// Creates a LeafNode that holds a single item.
			/// </summary>
			/// <param name="item">Item to place into the leaf node.</param>
			public LeafNode(T item)
			{
				// CONSIDER: is MAXLEAF always the right thing to do? It seems to work well in most cases.
				count = 1;
				items = new T[MAXLEAF];
				items[0] = item;
			}

			/// <summary>
			/// Creates a new leaf node with the indicates count of item and the
			/// </summary>
			/// <param name="count">Number of items. Can't be zero.</param>
			/// <param name="newItems">The array of items. The LeafNode takes
			/// possession of this array.</param>
			public LeafNode(int count, T[] newItems)
			{
				Debug.Assert(count <= newItems.Length && count > 0);
				Debug.Assert(newItems.Length <= MAXLEAF);

				this.count = count;
				items = newItems;
			}

			public override int Depth
			{
				get { return 0; }
			}

			/// <summary>
			/// Returns the items at the given index in this node.
			/// </summary>
			/// <param name="index">0-based index, relative to this node.</param>
			/// <returns>Item at that index.</returns>
			public override T GetAt(int index)
			{
				return items[index];
			}

			/// <summary>
			/// Changes the item at the given index. May change this node,
			/// or return a new node with the given item changed.
			/// </summary>
			/// <param name="index">Index, relative to this node, to change.</param>
			/// <param name="item">New item to place at the given index.</param>
			/// <returns>A node with the give item changed. If it can be done in place
			/// then "this" is returned.</returns>
			public override Node SetAtInPlace(int index, T item)
			{
				if (shared)
					return SetAt(index, item); // Can't update a shared node in place.

				items[index] = item;
				return this;
			}

			/// <summary>
			/// Changes the item at the given index. Never changes this node,
			/// but always returns a new node with the given item changed.
			/// </summary>
			/// <param name="index">Index, relative to this node, to change.</param>
			/// <param name="item">New item to place at the given index.</param>
			/// <returns>A new node with the given item changed.</returns>
			public override Node SetAt(int index, T item)
			{
				T[] newItems = (T[]) items.Clone();
				newItems[index] = item;
				return new LeafNode(count, newItems);
			}

			/// <summary>
			/// Prepend a item before this node. May change this node, or return 
			/// a new node. Equivalent to PrependInPlace(new LeafNode(item), true), but
			/// may be more efficient because a new LeafNode might not be allocated.
			/// </summary>
			/// <param name="item">Item to prepend.</param>
			/// <returns>A node with the given item prepended to this node. May be a new
			/// node or the current node.</returns>
			public override Node PrependInPlace(T item)
			{
				if (shared)
					return Prepend(new LeafNode(item), true); // Can't update a shared node in place.

				// Add into the current leaf, if possible.
				if (count < MAXLEAF)
				{
					if (count == items.Length)
					{
						T[] newItems = new T[MAXLEAF];
						Array.Copy(items, 0, newItems, 1, count);
						items = newItems;
					}
					else
					{
						Array.Copy(items, 0, items, 1, count);
					}

					items[0] = item;
					count += 1;

					return this;
				}
				else
				{
					return new ConcatNode(new LeafNode(item), this);
				}
			}

			/// <summary>
			/// Append a item after this node. May change this node, or return 
			/// a new node. Equivalent to AppendInPlace(new LeafNode(item), true), but
			/// may be more efficient because a new LeafNode might not be allocated.
			/// </summary>
			/// <param name="item">Item to append.</param>
			/// <returns>A node with the given item appended to this node. May be a new
			/// node or the current node.</returns>
			public override Node AppendInPlace(T item)
			{
				if (shared)
					return Append(new LeafNode(item), true); // Can't update a shared node in place.

				// Add into the current leaf, if possible.
				if (count < MAXLEAF)
				{
					if (count == items.Length)
					{
						T[] newItems = new T[MAXLEAF];
						Array.Copy(items, 0, newItems, 0, count);
						items = newItems;
					}

					items[count] = item;
					count += 1;
					return this;
				}
				else
				{
					return new ConcatNode(this, new LeafNode(item));
				}
			}

			/// <summary>
			/// Append a node after this node. May change this node, or return 
			/// a new node.
			/// </summary>
			/// <param name="node">Node to append.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A node with the give node appended to this node. May be a new
			/// node or the current node.</returns>
			public override Node AppendInPlace(Node node, bool nodeIsUnused)
			{
				if (shared)
					return Append(node, nodeIsUnused); // Can't update a shared node in place.

				// If we're appending a leaf, try to merge them if possible.
				if (MergeLeafInPlace(node))
				{
					return this;
				}

				// If we're appending a tree with a left leaf node, try to merge them if possible.
				ConcatNode otherConcat = (node as ConcatNode);
				if (otherConcat != null && MergeLeafInPlace(otherConcat.left))
				{
					if (! nodeIsUnused)
						otherConcat.right.MarkShared();
					return new ConcatNode(this, otherConcat.right);
				}

				// Otherwise, create a Concat node.
				if (! nodeIsUnused)
					node.MarkShared();
				return new ConcatNode(this, node);
			}

			public override Node Append(Node node, bool nodeIsUnused)
			{
				Node result;

				// If we're appending a leaf, try to merge them if possible.
				if ((result = MergeLeaf(node)) != null)
					return result;

				// If we're appending a concat with a left leaf, try to merge them if possible.
				ConcatNode otherConcat = (node as ConcatNode);
				if (otherConcat != null && (result = MergeLeaf(otherConcat.left)) != null)
				{
					if (! nodeIsUnused)
						otherConcat.right.MarkShared();
					return new ConcatNode(result, otherConcat.right);
				}

				// Otherwise, create a Concat node.
				if (!nodeIsUnused)
					node.MarkShared();
				MarkShared();
				return new ConcatNode(this, node);
			}

			/// <summary>
			/// Inserts an item inside this node. May change this node, or return
			/// a new node with the given appending done. Equivalent to 
			/// InsertInPlace(new LeafNode(item), true), but may be more efficient.
			/// </summary>
			/// <param name="index">Index, relative to this node, to insert at. Must 
			/// be in bounds.</param>
			/// <param name="item">Item to insert.</param>
			/// <returns>A node with the give item inserted. If done in-place, returns
			/// "this".</returns>
			public override Node InsertInPlace(int index, T item)
			{
				if (shared)
					return Insert(index, new LeafNode(item), true); // Can't update a shared node in place.

				// Insert into the current leaf, if possible.
				if (count < MAXLEAF)
				{
					if (count == items.Length)
					{
						T[] newItems = new T[MAXLEAF];
						if (index > 0)
							Array.Copy(items, 0, newItems, 0, index);
						if (count > index)
							Array.Copy(items, index, newItems, index + 1, count - index);
						items = newItems;
					}
					else
					{
						if (count > index)
							Array.Copy(items, index, items, index + 1, count - index);
					}

					items[index] = item;
					count += 1;
					return this;
				}
				else
				{
					if (index == count)
					{
						// Inserting at count is just an appending operation.
						return new ConcatNode(this, new LeafNode(item));
					}
					else if (index == 0)
					{
						// Inserting at 0 is just a prepending operation.
						return new ConcatNode(new LeafNode(item), this);
					}
					else
					{
						// Split into two nodes, and put the new item at the end of the first.
						T[] leftItems = new T[MAXLEAF];
						Array.Copy(items, 0, leftItems, 0, index);
						leftItems[index] = item;
						Node leftNode = new LeafNode(index + 1, leftItems);

						T[] rightItems = new T[count - index];
						Array.Copy(items, index, rightItems, 0, count - index);
						Node rightNode = new LeafNode(count - index, rightItems);

						return new ConcatNode(leftNode, rightNode);
					}
				}
			}

			/// <summary>
			/// Inserts a node inside this node. May change this node, or return
			/// a new node with the given appending done.
			/// </summary>
			/// <param name="index">Index, relative to this node, to insert at. Must 
			/// be in bounds.</param>
			/// <param name="node">Node to insert.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A node with the given item inserted. If done in-place, returns
			/// "this".</returns>
			public override Node InsertInPlace(int index, Node node, bool nodeIsUnused)
			{
				if (shared)
					return Insert(index, node, nodeIsUnused); // Can't update a shared node in place.

				LeafNode otherLeaf = (node as LeafNode);
				int newCount;

				if (otherLeaf != null && (newCount = otherLeaf.Count + this.count) <= MAXLEAF)
				{
					// Combine the two leaf nodes into one.
					if (newCount > items.Length)
					{
						T[] newItems = new T[MAXLEAF];
						Array.Copy(items, 0, newItems, 0, index);
						Array.Copy(otherLeaf.items, 0, newItems, index, otherLeaf.Count);
						Array.Copy(items, index, newItems, index + otherLeaf.Count, count - index);
						items = newItems;
					}
					else
					{
						Array.Copy(items, index, items, index + otherLeaf.Count, count - index);
						Array.Copy(otherLeaf.items, 0, items, index, otherLeaf.count);
					}
					count = newCount;
					return this;
				}
				else if (index == 0)
				{
					// Inserting at 0 is a prepend.
					return PrependInPlace(node, nodeIsUnused);
				}
				else if (index == count)
				{
					// Inserting at count is an append.
					return AppendInPlace(node, nodeIsUnused);
				}
				else
				{
					// Split existing node into two nodes at the insertion point, then concat all three nodes together.

					T[] leftItems = new T[index];
					Array.Copy(items, 0, leftItems, 0, index);
					Node leftNode = new LeafNode(index, leftItems);

					T[] rightItems = new T[count - index];
					Array.Copy(items, index, rightItems, 0, count - index);
					Node rightNode = new LeafNode(count - index, rightItems);

					leftNode = leftNode.AppendInPlace(node, nodeIsUnused);
					leftNode = leftNode.AppendInPlace(rightNode, true);
					return leftNode;
				}
			}

			/// <summary>
			/// Inserts a node inside this node. Never changes this node, but returns
			/// a new node with the given appending done.
			/// </summary>
			/// <param name="index">Index, relative to this node, to insert at. Must 
			/// be in bounds.</param>
			/// <param name="node">Node to insert.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A new node with the give node inserted.</returns>
			public override Node Insert(int index, Node node, bool nodeIsUnused)
			{
				LeafNode otherLeaf = (node as LeafNode);
				int newCount;

				if (otherLeaf != null && (newCount = otherLeaf.Count + this.count) <= MAXLEAF)
				{
					// Combine the two leaf nodes into one.
					T[] newItems = new T[MAXLEAF];
					Array.Copy(items, 0, newItems, 0, index);
					Array.Copy(otherLeaf.items, 0, newItems, index, otherLeaf.Count);
					Array.Copy(items, index, newItems, index + otherLeaf.Count, count - index);
					return new LeafNode(newCount, newItems);
				}
				else if (index == 0)
				{
					// Inserting at 0 is a prepend.
					return Prepend(node, nodeIsUnused);
				}
				else if (index == count)
				{
					// Inserting at count is an append.
					return Append(node, nodeIsUnused);
				}
				else
				{
					// Split existing node into two nodes at the insertion point, then concat all three nodes together.

					T[] leftItems = new T[index];
					Array.Copy(items, 0, leftItems, 0, index);
					Node leftNode = new LeafNode(index, leftItems);

					T[] rightItems = new T[count - index];
					Array.Copy(items, index, rightItems, 0, count - index);
					Node rightNode = new LeafNode(count - index, rightItems);

					leftNode = leftNode.AppendInPlace(node, nodeIsUnused);
					leftNode = leftNode.AppendInPlace(rightNode, true);
					return leftNode;
				}
			}

			/// <summary>
			/// Remove a range of items from this node. May change this node, or returns
			/// a new node with the given appending done. The
			/// sub-range may not be empty, but may extend outside the node. 
			/// In other words, first might be less than zero or last might be greater
			/// than count. But, last can't be less than zero and first can't be
			/// greater than count. Also, last must be greater than or equal to last.
			/// </summary>
			/// <param name="first">Inclusive index of first item in sub-range, relative
			/// to this node.</param>
			/// <param name="last">Inclusize index of last item in sub-range, relative
			/// to this node.</param>
			/// <returns>A node with the sub-range removed. If done in-place, returns
			/// "this".</returns>
			public override Node RemoveRangeInPlace(int first, int last)
			{
				if (shared)
					return RemoveRange(first, last);

				Debug.Assert(first <= last);
				Debug.Assert(last >= 0);

				if (first <= 0 && last >= count - 1)
				{
					return null; // removing entire node.
				}

				if (first < 0)
					first = 0;
				if (last >= count)
					last = count - 1;
				int newCount = first + (count - last - 1); // number of items remaining.
				if (count > last + 1)
					Array.Copy(items, last + 1, items, first, count - last - 1);
				for (int i = newCount; i < count; ++i)
					items[i] = default(T);
				count = newCount;
				return this;
			}

			/// <summary>
			/// Remove a range of items from this node. Never changes this node, but returns
			/// a new node with the removing done. The
			/// sub-range may not be empty, but may extend outside the node. 
			/// In other words, first might be less than zero or last might be greater
			/// than count. But, last can't be less than zero and first can't be
			/// greater than count. Also, last must be greater than or equal to last.
			/// </summary>
			/// <param name="first">Inclusive index of first item in sub-range, relative
			/// to this node.</param>
			/// <param name="last">Inclusize index of last item in sub-range, relative
			/// to this node.</param>
			/// <returns>A new node with the sub-range removed.</returns>
			public override Node RemoveRange(int first, int last)
			{
				Debug.Assert(first <= last);
				Debug.Assert(last >= 0);

				if (first <= 0 && last >= count - 1)
				{
					return null; // removing entire node.
				}

				if (first < 0)
					first = 0;
				if (last >= count)
					last = count - 1;
				int newCount = first + (count - last - 1); // number of items remaining.
				T[] newItems = new T[newCount];
				if (first > 0)
					Array.Copy(items, 0, newItems, 0, first);
				if (count > last + 1)
					Array.Copy(items, last + 1, newItems, first, count - last - 1);
				return new LeafNode(newCount, newItems);
			}

			/// <summary>
			/// Returns a node that has a sub-range of items from this node. The
			/// sub-range may not be empty, but may extend outside the node. 
			/// In other words, first might be less than zero or last might be greater
			/// than count. But, last can't be less than zero and first can't be
			/// greater than count. Also, last must be greater than or equal to last.
			/// </summary>
			/// <param name="first">Inclusive first element, relative to this node.</param>
			/// <param name="last">Inclusize last element, relative to this node.</param>
			/// <returns>Node with the given sub-range.</returns>
			public override Node Subrange(int first, int last)
			{
				Debug.Assert(first <= last);
				Debug.Assert(last >= 0);
				if (first <= 0 && last >= count - 1)
				{
					MarkShared();
					return this;
				}
				else
				{
					if (first < 0)
						first = 0;
					if (last >= count)
						last = count - 1;
					int n = last - first + 1;
					T[] newItems = new T[n];
					Array.Copy(items, first, newItems, 0, n);
					return new LeafNode(n, newItems);
				}
			}

			/// <summary>
			/// Validates the node for consistency, as much as possible. Also validates
			/// child nodes, if any.
			/// </summary>
			public override void Validate()
			{
				// Check count and length of buffer.
				Debug.Assert(count > 0);
				Debug.Assert(items != null);
				Debug.Assert(items.Length > 0);
				Debug.Assert(count <= MAXLEAF);
				Debug.Assert(items.Length <= MAXLEAF);
				Debug.Assert(count <= items.Length);
			}

			/// <summary>
			/// Print out the contents of this node.
			/// </summary>
			/// <param name="prefixNode">Prefix to use in front of this node.</param>
			/// <param name="prefixChildren">Prefixed to use in front of children of this node.</param>
			public override void Print(string prefixNode, string prefixChildren)
			{
				Console.Write("{0}LEAF {1} count={2}/{3} ", prefixNode, shared ? "S" : " ", count, items.Length);
				for (int i = 0; i < count; ++i)
					Console.Write("{0} ", items[i]);
				Console.WriteLine();
			}

			/// <summary>
			/// If other is a leaf node, and the resulting size would be less than MAXLEAF, merge
			/// the other leaf node into this one (after this one) and return true.
			/// </summary>
			/// <param name="other">Other node to possible merge.</param>
			/// <returns>If <paramref name="other"/> could be merged into this node, returns
			/// true. Otherwise returns false and the current node is unchanged.</returns>
			private bool MergeLeafInPlace(Node other)
			{
				Debug.Assert(!shared);
				LeafNode otherLeaf = (other as LeafNode);
				int newCount;
				if (otherLeaf != null && (newCount = otherLeaf.Count + this.count) <= MAXLEAF)
				{
					// Combine the two leaf nodes into one.
					if (newCount > items.Length)
					{
						T[] newItems = new T[MAXLEAF];
						Array.Copy(items, 0, newItems, 0, count);
						items = newItems;
					}
					Array.Copy(otherLeaf.items, 0, items, count, otherLeaf.count);
					count = newCount;
					return true;
				}
				return false;
			}

			/// <summary>
			/// If other is a leaf node, and the resulting size would be less than MAXLEAF, merge
			/// the other leaf node with this one (after this one) and return a new node with 
			/// the merged items. Does not modify this.
			/// If no merging, return null.
			/// </summary>
			/// <param name="other">Other node to possible merge.</param>
			/// <returns>If the nodes could be merged, returns the new node. Otherwise
			/// returns null.</returns>
			private Node MergeLeaf(Node other)
			{
				LeafNode otherLeaf = (other as LeafNode);
				int newCount;
				if (otherLeaf != null && (newCount = otherLeaf.Count + this.count) <= MAXLEAF)
				{
					// Combine the two leaf nodes into one.
					T[] newItems = new T[MAXLEAF];
					Array.Copy(items, 0, newItems, 0, count);
					Array.Copy(otherLeaf.items, 0, newItems, count, otherLeaf.count);
					return new LeafNode(newCount, newItems);
				}
				return null;
			}
		}

		/// <summary>
		/// A ConcatNode is an interior (non-leaf) node that represents the concatination of
		/// the left and right child nodes. Both children must always be non-null.
		/// </summary>
		[Serializable]
		private sealed class ConcatNode : Node
		{
			/// <summary>
			/// The depth of this node -- the maximum length path to 
			/// a leaf. If this node has two children that are leaves, the
			/// depth in 1.
			/// </summary>
			private short depth;

			/// <summary>
			/// The left and right child nodes. They are never null.
			/// </summary>
			public Node left, right;

			/// <summary>
			/// Create a new ConcatNode with the given children.
			/// </summary>
			/// <param name="left">The left child. May not be null.</param>
			/// <param name="right">The right child. May not be null.</param>
			public ConcatNode(Node left, Node right)
			{
				Debug.Assert(left != null && right != null);
				this.left = left;
				this.right = right;
				this.count = left.Count + right.Count;
				if (left.Depth > right.Depth)
					this.depth = (short) (left.Depth + 1);
				else
					this.depth = (short) (right.Depth + 1);
			}

			/// <summary>
			/// The depth of this node -- the maximum length path to 
			/// a leaf. If this node has two children that are leaves, the
			/// depth in 1.
			/// </summary>
			/// <value>The depth of this node.</value>
			public override int Depth
			{
				get { return depth; }
			}


			/// <summary>
			/// Returns the items at the given index in this node.
			/// </summary>
			/// <param name="index">0-based index, relative to this node.</param>
			/// <returns>Item at that index.</returns>
			public override T GetAt(int index)
			{
				int leftCount = left.Count;
				if (index < leftCount)
					return left.GetAt(index);
				else
					return right.GetAt(index - leftCount);
			}

			/// <summary>
			/// Changes the item at the given index. May change this node,
			/// or return a new node with the given item changed.
			/// </summary>
			/// <param name="index">Index, relative to this node, to change.</param>
			/// <param name="item">New item to place at the given index.</param>
			/// <returns>A node with the give item changed. If it can be done in place
			/// then "this" is returned.</returns>
			public override Node SetAtInPlace(int index, T item)
			{
				if (shared)
					return SetAt(index, item); // Can't update a shared node in place.

				int leftCount = left.Count;

				if (index < leftCount)
				{
					Node newLeft = left.SetAtInPlace(index, item);
					if (newLeft != left)
						return NewNodeInPlace(newLeft, right);
					else
						return this;
				}
				else
				{
					Node newRight = right.SetAtInPlace(index - leftCount, item);
					if (newRight != right)
						return NewNodeInPlace(left, newRight);
					else
						return this;
				}
			}

			/// <summary>
			/// Changes the item at the given index. Never changes this node,
			/// but always returns a new node with the given item changed.
			/// </summary>
			/// <param name="index">Index, relative to this node, to change.</param>
			/// <param name="item">New item to place at the given index.</param>
			/// <returns>A new node with the given item changed.</returns>
			public override Node SetAt(int index, T item)
			{
				int leftCount = left.Count;

				if (index < leftCount)
				{
					return NewNode(left.SetAt(index, item), right);
				}
				else
				{
					return NewNode(left, right.SetAt(index - leftCount, item));
				}
			}

			/// <summary>
			/// Prepend a item before this node. May change this node, or return 
			/// a new node. Equivalent to PrependInPlace(new LeafNode(item), true), but
			/// may be more efficient because a new LeafNode might not be allocated.
			/// </summary>
			/// <param name="item">Item to prepend.</param>
			/// <returns>A node with the given item prepended to this node. May be a new
			/// node or the current node.</returns>
			public override Node PrependInPlace(T item)
			{
				if (shared)
					return Prepend(new LeafNode(item), true); // Can't update a shared node in place.

				LeafNode leftLeaf;
				if (left.Count < MAXLEAF && !left.Shared && (leftLeaf = left as LeafNode) != null)
				{
					// Prepend the item to the left leaf. This keeps repeated prepends from creating
					// single item nodes.
					int c = leftLeaf.Count;
					if (c == leftLeaf.items.Length)
					{
						T[] newItems = new T[MAXLEAF];
						Array.Copy(leftLeaf.items, 0, newItems, 1, c);
						leftLeaf.items = newItems;
					}
					else
					{
						Array.Copy(leftLeaf.items, 0, leftLeaf.items, 1, c);
					}

					leftLeaf.items[0] = item;
					leftLeaf.count += 1;
					this.count += 1;
					return this;
				}
				else
					return new ConcatNode(new LeafNode(item), this);
			}

			/// <summary>
			/// Append a item after this node. May change this node, or return 
			/// a new node. Equivalent to AppendInPlace(new LeafNode(item), true), but
			/// may be more efficient because a new LeafNode might not be allocated.
			/// </summary>
			/// <param name="item">Item to append.</param>
			/// <returns>A node with the given item appended to this node. May be a new
			/// node or the current node.</returns>
			public override Node AppendInPlace(T item)
			{
				if (shared)
					return Append(new LeafNode(item), true); // Can't update a shared node in place.

				LeafNode rightLeaf;
				if (right.Count < MAXLEAF && !right.Shared && (rightLeaf = right as LeafNode) != null)
				{
					int c = rightLeaf.Count;
					if (c == rightLeaf.items.Length)
					{
						T[] newItems = new T[MAXLEAF]; // use MAXLEAF when appending, because we'll probably append again.
						Array.Copy(rightLeaf.items, 0, newItems, 0, c);
						rightLeaf.items = newItems;
					}

					rightLeaf.items[c] = item;
					rightLeaf.count += 1;
					this.count += 1;
					return this;
				}
				else
					return new ConcatNode(this, new LeafNode(item));
			}

			/// <summary>
			/// Append a node after this node. May change this node, or return 
			/// a new node.
			/// </summary>
			/// <param name="node">Node to append.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A node with the give node appended to this node. May be a new
			/// node or the current node.</returns>
			public override Node AppendInPlace(Node node, bool nodeIsUnused)
			{
				if (shared)
					return Append(node, nodeIsUnused); // Can't update a shared node in place.

				if (right.Count + node.Count <= MAXLEAF && right is LeafNode && node is LeafNode)
					return NewNodeInPlace(left, right.AppendInPlace(node, nodeIsUnused));

				if (!nodeIsUnused)
					node.MarkShared();
				return new ConcatNode(this, node);
			}

			public override Node Append(Node node, bool nodeIsUnused)
			{
				// If possible combine with a child leaf node on the right.
				if (right.Count + node.Count <= MAXLEAF && right is LeafNode && node is LeafNode)
					return NewNode(left, right.Append(node, nodeIsUnused));

				// Concatinate with this node. 
				this.MarkShared();
				if (!nodeIsUnused)
					node.MarkShared();
				return new ConcatNode(this, node);
			}

			/// <summary>
			/// Inserts an item inside this node. May change this node, or return
			/// a new node with the given appending done. Equivalent to 
			/// InsertInPlace(new LeafNode(item), true), but may be more efficient.
			/// </summary>
			/// <param name="index">Index, relative to this node, to insert at. Must 
			/// be in bounds.</param>
			/// <param name="item">Item to insert.</param>
			/// <returns>A node with the give item inserted. If done in-place, returns
			/// "this".</returns>
			public override Node InsertInPlace(int index, T item)
			{
				if (shared)
					return Insert(index, new LeafNode(item), true);

				int leftCount = left.Count;
				if (index <= leftCount)
					return NewNodeInPlace(left.InsertInPlace(index, item), right);
				else
					return NewNodeInPlace(left, right.InsertInPlace(index - leftCount, item));
			}

			/// <summary>
			/// Inserts a node inside this node. May change this node, or return
			/// a new node with the given appending done.
			/// </summary>
			/// <param name="index">Index, relative to this node, to insert at. Must 
			/// be in bounds.</param>
			/// <param name="node">Node to insert.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A node with the given item inserted. If done in-place, returns
			/// "this".</returns>
			public override Node InsertInPlace(int index, Node node, bool nodeIsUnused)
			{
				if (shared)
					return Insert(index, node, nodeIsUnused);

				int leftCount = left.Count;
				if (index < leftCount)
					return NewNodeInPlace(left.InsertInPlace(index, node, nodeIsUnused), right);
				else
					return NewNodeInPlace(left, right.InsertInPlace(index - leftCount, node, nodeIsUnused));
			}

			/// <summary>
			/// Inserts a node inside this node. Never changes this node, but returns
			/// a new node with the given appending done.
			/// </summary>
			/// <param name="index">Index, relative to this node, to insert at. Must 
			/// be in bounds.</param>
			/// <param name="node">Node to insert.</param>
			/// <param name="nodeIsUnused">If true, the given node is not used
			/// in any current list, so it may be change, overwritten, or destroyed
			/// if convenient. If false, the given node is in use. It should be marked
			/// as shared if is is used within the return value.</param>
			/// <returns>A new node with the give node inserted.</returns>
			public override Node Insert(int index, Node node, bool nodeIsUnused)
			{
				int leftCount = left.Count;
				if (index < leftCount)
					return NewNode(left.Insert(index, node, nodeIsUnused), right);
				else
					return NewNode(left, right.Insert(index - leftCount, node, nodeIsUnused));
			}

			/// <summary>
			/// Remove a range of items from this node. May change this node, or returns
			/// a new node with the given appending done. The
			/// sub-range may not be empty, but may extend outside the node. 
			/// In other words, first might be less than zero or last might be greater
			/// than count. But, last can't be less than zero and first can't be
			/// greater than count. Also, last must be greater than or equal to last.
			/// </summary>
			/// <param name="first">Inclusive index of first item in sub-range, relative
			/// to this node.</param>
			/// <param name="last">Inclusize index of last item in sub-range, relative
			/// to this node.</param>
			/// <returns>A node with the sub-range removed. If done in-place, returns
			/// "this".</returns>
			public override Node RemoveRangeInPlace(int first, int last)
			{
				if (shared)
					return RemoveRange(first, last);

				Debug.Assert(first < count);
				Debug.Assert(last >= 0);

				if (first <= 0 && last >= count - 1)
				{
					return null;
				}

				int leftCount = left.Count;
				Node newLeft = left, newRight = right;

				// Is part of the left being removed?
				if (first < leftCount)
					newLeft = left.RemoveRangeInPlace(first, last);
				// Is part of the right being remove?
				if (last >= leftCount)
					newRight = right.RemoveRangeInPlace(first - leftCount, last - leftCount);

				return NewNodeInPlace(newLeft, newRight);
			}

			/// <summary>
			/// Remove a range of items from this node. Never changes this node, but returns
			/// a new node with the removing done. The
			/// sub-range may not be empty, but may extend outside the node. 
			/// In other words, first might be less than zero or last might be greater
			/// than count. But, last can't be less than zero and first can't be
			/// greater than count. Also, last must be greater than or equal to last.
			/// </summary>
			/// <param name="first">Inclusive index of first item in sub-range, relative
			/// to this node.</param>
			/// <param name="last">Inclusize index of last item in sub-range, relative
			/// to this node.</param>
			/// <returns>A new node with the sub-range removed.</returns>
			public override Node RemoveRange(int first, int last)
			{
				Debug.Assert(first < count);
				Debug.Assert(last >= 0);

				if (first <= 0 && last >= count - 1)
				{
					return null;
				}

				int leftCount = left.Count;
				Node newLeft = left, newRight = right;

				// Is part of the left being removed?
				if (first < leftCount)
					newLeft = left.RemoveRange(first, last);
				// Is part of the right being remove?
				if (last >= leftCount)
					newRight = right.RemoveRange(first - leftCount, last - leftCount);

				return NewNode(newLeft, newRight);
			}

			/// <summary>
			/// Returns a node that has a sub-range of items from this node. The
			/// sub-range may not be empty, but may extend outside the node. 
			/// In other words, first might be less than zero or last might be greater
			/// than count. But, last can't be less than zero and first can't be
			/// greater than count. Also, last must be greater than or equal to last.
			/// </summary>
			/// <param name="first">Inclusive first element, relative to this node.</param>
			/// <param name="last">Inclusize last element, relative to this node.</param>
			/// <returns>Node with the given sub-range.</returns>
			public override Node Subrange(int first, int last)
			{
				Debug.Assert(first < count);
				Debug.Assert(last >= 0);

				if (first <= 0 && last >= count - 1)
				{
					// range encapsulate the whole node, so just return it.
					MarkShared();
					return this;
				}

				int leftCount = left.Count;
				Node leftPart = null, rightPart = null;

				// Is part of the left included?
				if (first < leftCount)
					leftPart = left.Subrange(first, last);
				// Is part of the right included?
				if (last >= leftCount)
					rightPart = right.Subrange(first - leftCount, last - leftCount);

				Debug.Assert(leftPart != null || rightPart != null);

				// Combine the left parts and the right parts.
				if (leftPart == null)
					return rightPart;
				else if (rightPart == null)
					return leftPart;
				else
					return new ConcatNode(leftPart, rightPart);
			}

			/// <summary>
			/// Validates the node for consistency, as much as possible. Also validates
			/// child nodes, if any.
			/// </summary>
			public override void Validate()
			{
				Debug.Assert(left != null);
				Debug.Assert(right != null);
				Debug.Assert(Depth > 0);
				Debug.Assert(Count > 0);
				Debug.Assert(Math.Max(left.Depth, right.Depth) + 1 == Depth);
				Debug.Assert(left.Count + right.Count == Count);
				left.Validate();
				right.Validate();
			}

			/// <summary>
			/// Print out the contents of this node.
			/// </summary>
			/// <param name="prefixNode">Prefix to use in front of this node.</param>
			/// <param name="prefixChildren">Prefixed to use in front of children of this node.</param>
			public override void Print(string prefixNode, string prefixChildren)
			{
				Console.WriteLine("{0}CONCAT {1} {2} count={3} depth={4}", prefixNode, shared ? "S" : " ", IsBalanced() ? "B" : (IsAlmostBalanced() ? "A" : " "), count, depth);
				left.Print(prefixChildren + "|-L-", prefixChildren + "|  ");
				right.Print(prefixChildren + "|-R-", prefixChildren + "   ");
			}

			/// <summary>
			/// Create a new node with the given children. Mark unchanged
			/// children as shared. There are four
			/// possible cases:
			/// 1. If one of the new children is null, the other new child is returned.
			/// 2. If neither child has changed, then this is marked as shared as returned.
			/// 3. If one child has changed, the other child is marked shared an a new node is returned.
			/// 4. If both children have changed, a new node is returned.
			/// </summary>
			/// <param name="newLeft">New left child.</param>
			/// <param name="newRight">New right child.</param>
			/// <returns>New node with the given children. Returns null if and only if both
			/// new children are null.</returns>
			private Node NewNode(Node newLeft, Node newRight)
			{
				if (left == newLeft)
				{
					if (right == newRight)
					{
						MarkShared();
						return this; // Nothing changed. In this case we can return the same node.
					}
					else
						left.MarkShared();
				}
				else
				{
					if (right == newRight)
						right.MarkShared();
				}

				if (newLeft == null)
					return newRight;
				else if (newRight == null)
					return newLeft;
				else
					return new ConcatNode(newLeft, newRight);
			}

			/// <summary>
			/// Updates a node with the given new children. If one of the new children is
			/// null, the other is returned. If both are null, null is returned.
			/// </summary>
			/// <param name="newLeft">New left child.</param>
			/// <param name="newRight">New right child.</param>
			/// <returns>Node with the given children. Usually, but not always, this. Returns
			/// null if and only if both new children are null.</returns>
			private Node NewNodeInPlace(Node newLeft, Node newRight)
			{
				Debug.Assert(!shared);

				if (newLeft == null)
					return newRight;
				else if (newRight == null)
					return newLeft;

				left = newLeft;
				right = newRight;
				count = left.Count + right.Count;
				if (left.Depth > right.Depth)
					depth = (short) (left.Depth + 1);
				else
					depth = (short) (right.Depth + 1);
				return this;
			}
		}

		/// <summary>
		/// The class that is used to implement IList&lt;T&gt; to view a sub-range
		/// of a BigList. The object stores a wrapped list, and a start/count indicating
		/// a sub-range of the list. Insertion/deletions through the sub-range view
		/// cause the count to change also; insertions and deletions directly on
		/// the wrapped list do not.
		/// </summary>
		/// <remarks>This is different from Algorithms.Range in a very few respects:
		/// it is specialized to only wrap BigList, and it is a lot more efficient in enumeration.</remarks>
		[Serializable]
		private class BigListRange : ListBase<T>
		{
			private readonly int start;
			private readonly BigList<T> wrappedList;
			private int count;

			/// <summary>
			/// Create a sub-range view object on the indicate part 
			/// of the list. 
			/// </summary>
			/// <param name="wrappedList">List to wrap.</param>
			/// <param name="start">The start index of the view in the wrapped list.</param>
			/// <param name="count">The number of items in the view.</param>
			public BigListRange(BigList<T> wrappedList, int start, int count)
			{
				this.wrappedList = wrappedList;
				this.start = start;
				this.count = count;
			}

			public override int Count
			{
				get { return Math.Min(count, wrappedList.Count - start); }
			}

			public override T this[int index]
			{
				get
				{
					if (index < 0 || index >= count)
						throw new ArgumentOutOfRangeException("index");

					return wrappedList[start + index];
				}
				set
				{
					if (index < 0 || index >= count)
						throw new ArgumentOutOfRangeException("index");

					wrappedList[start + index] = value;
				}
			}

			public override void Clear()
			{
				if (wrappedList.Count - start < count)
					count = wrappedList.Count - start;

				while (count > 0)
				{
					wrappedList.RemoveAt(start + count - 1);
					--count;
				}
			}

			public override void Insert(int index, T item)
			{
				if (index < 0 || index > count)
					throw new ArgumentOutOfRangeException("index");

				wrappedList.Insert(start + index, item);
				++count;
			}

			public override void RemoveAt(int index)
			{
				if (index < 0 || index >= count)
					throw new ArgumentOutOfRangeException("index");

				wrappedList.RemoveAt(start + index);
				--count;
			}

			public override IEnumerator<T> GetEnumerator()
			{
				return wrappedList.GetEnumerator(start, count);
			}
		}

		/// <summary>
		/// Concatenates two lists together to create a new list. Both lists being concatenated
		/// are unchanged. The resulting list contains all the items in <paramref name="first"/>, followed
		/// by all the items in <paramref name="second"/>.
		/// </summary>
		/// <remarks>This method takes, on average, constant time, regardless of the size
		/// of either list. Although conceptually all of the items in both lists are
		/// copied, storage is shared until changes are made to the 
		/// shared sections.</remarks>
		/// <param name="first">The first list to concatenate.</param>
		/// <param name="second">The second list to concatenate.</param>
		/// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is null.</exception>
		public static BigList<T> operator +(BigList<T> first, BigList<T> second)
		{
			if (first == null)
				throw new ArgumentNullException("first");
			if (second == null)
				throw new ArgumentNullException("second");
			if ((uint) first.Count + (uint) second.Count > MAXITEMS)
				throw new InvalidOperationException(Strings.CollectionTooLarge);

			if (first.Count == 0)
				return second.Clone();
			else if (second.Count == 0)
				return first.Clone();
			else
			{
				BigList<T> result = new BigList<T>(first.root.Append(second.root, false));
				result.CheckBalance();
				return result;
			}
		}

		/// <summary>
		/// Given an IEnumerable&lt;T&gt;, create a new Node with all of the 
		/// items in the enumerable. Returns null if the enumerable has no items.
		/// </summary>
		/// <param name="collection">The collection to copy.</param>
		/// <returns>Returns a Node, not shared or with any shared children, 
		/// with the items from the collection. If the collection was empty,
		/// null is returned.</returns>
		private static Node NodeFromEnumerable(IEnumerable<T> collection)
		{
			Node node = null;
			LeafNode leaf;
			IEnumerator<T> enumerator = collection.GetEnumerator();

			while ((leaf = LeafFromEnumerator(enumerator)) != null)
			{
				if (node == null)
					node = leaf;
				else
				{
					if ((uint) (node.count) + (uint) (leaf.count) > MAXITEMS)
						throw new InvalidOperationException(Strings.CollectionTooLarge);

					node = node.AppendInPlace(leaf, true);
				}
			}

			return node;
		}

		/// <summary>
		/// Consumes up to MAXLEAF items from an Enumerator and places them in a leaf
		/// node. If the enumerator is at the end, null is returned.
		/// </summary>
		/// <param name="enumerator">The enumerator to take items from.</param>
		/// <returns>A LeafNode with items taken from the enumerator. </returns>
		private static LeafNode LeafFromEnumerator(IEnumerator<T> enumerator)
		{
			int i = 0;
			T[] items = null;

			while (i < MAXLEAF && enumerator.MoveNext())
			{
				if (i == 0)
					items = new T[MAXLEAF];

				if (items != null)
					items[i++] = enumerator.Current;
			}

			if (items != null)
				return new LeafNode(i, items);
			else
				return null;
		}

		/// <summary>
		/// Create a node that has N copies of the given node. 
		/// </summary>
		/// <param name="copies">Number of copies. Must be non-negative.</param>
		/// <param name="node">Node to make copies of.</param>
		/// <returns>null if node is null or copies is 0. Otherwise, a node consisting of <paramref name="copies"/> copies
		/// of node.</returns>
		/// <exception cref="ArgumentOutOfRangeException">copies is negative.</exception>
		private static Node NCopiesOfNode(int copies, Node node)
		{
			if (copies < 0)
				throw new ArgumentOutOfRangeException("copies", Strings.ArgMustNotBeNegative);

			// Do the simple cases.
			if (copies == 0 || node == null)
				return null;
			if (copies == 1)
				return node;

			if (copies*(long) (node.count) > MAXITEMS)
				throw new InvalidOperationException(Strings.CollectionTooLarge);

			// Build up the copies by powers of two.
			int n = 1;
			Node power = node, builder = null;
			while (copies > 0)
			{
				power.MarkShared();

				if ((copies & n) != 0)
				{
					// This power of two is used in the final result.
					copies -= n;
					if (builder == null)
						builder = power;
					else
						builder = builder.Append(power, false);
				}

				n *= 2;
				power = power.Append(power, false);
			}

			return builder;
		}

		/// <summary>
		/// Part of the rebalancing algorithm. Adds a balanced node to the rebalance array. 
		/// </summary>
		/// <param name="rebalanceArray">Rebalance array to insert into.</param>
		/// <param name="balancedNode">Node to add.</param>
		private static void AddBalancedNodeToRebalanceArray(Node[] rebalanceArray, Node balancedNode)
		{
			int slot;
			int count;
			Node accum = null;
			Debug.Assert(balancedNode.IsBalanced());

			count = balancedNode.Count;
			slot = 0;
			while (count >= FIBONACCI[slot + 1])
			{
				Node n = rebalanceArray[slot];
				if (n != null)
				{
					rebalanceArray[slot] = null;
					if (accum == null)
						accum = n;
					else
						accum = accum.PrependInPlace(n, !n.Shared);
				}
				++slot;
			}

			// slot is the location where balancedNode originally ended up, but possibly
			// not the final resting place.
			if (accum != null)
				balancedNode = balancedNode.PrependInPlace(accum, !accum.Shared);
			for (;;)
			{
				Node n = rebalanceArray[slot];
				if (n != null)
				{
					rebalanceArray[slot] = null;
					balancedNode = balancedNode.PrependInPlace(n, !n.Shared);
				}

				if (balancedNode.Count < FIBONACCI[slot + 1])
				{
					rebalanceArray[slot] = balancedNode;
					break;
				}
				++slot;
			}

			// The above operations should ensure that everything in the rebalance array is now almost balanced.
			for (int i = 0; i < rebalanceArray.Length; ++i)
			{
				if (rebalanceArray[i] != null)
					Debug.Assert(rebalanceArray[i].IsAlmostBalanced());
			}
		}
	}
}