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
	using System.Diagnostics;

	/// <summary>
	/// Describes what to do if a key is already in the tree when doing an
	/// insertion.
	/// </summary>
	internal enum DuplicatePolicy
	{
		InsertFirst, // Insert a new node before duplicates
		InsertLast, // Insert a new node after duplicates
		ReplaceFirst, // Replace the first of the duplicate nodes
		ReplaceLast, // Replace the last of the duplicate nodes
		DoNothing // Do nothing to the tree
	} ;

	/// <summary>
	/// The base implementation for various collections classes that use Red-Black trees
	/// as part of their implementation. This class should not (and can not) be 
	/// used directly by end users; it's only for internal use by the collections package.
	/// </summary>
	/// <remarks>
	/// The Red-Black tree manages items of type T, and uses a IComparer&lt;T&gt; that
	/// compares items to sort the tree. Multiple items can compare equal and be stored
	/// in the tree. Insert, Delete, and Find operations are provided in their full generality;
	/// all operations allow dealing with either the first or last of items that compare equal. 
	///</remarks>
	[Serializable]
	internal class RedBlackTree<T> : IEnumerable<T>
	{
		private readonly IComparer<T> comparer; // interface for comparing elements, only Compare is used.

		private int changeStamp; // An integer that is changed every time the tree structurally changes.
		private int count; // The count of elements in the tree.
		private Node root; // The root of the tree. Can be null when tree is empty.
		// Used so that enumerations throw an exception if the tree is changed
		// during enumeration.

		private Node[] stack; // A stack of nodes. This is cached locally to avoid constant re-allocated it.

		/// <summary>
		/// Initialize a red-black tree, using the given interface instance to compare elements. Only
		/// Compare is used on the IComparer interface.
		/// </summary>
		/// <param name="comparer">The IComparer&lt;T&gt; used to sort keys.</param>
		public RedBlackTree(IComparer<T> comparer)
		{
			this.comparer = comparer;
			this.count = 0;
			this.root = null;
		}

		/// <summary>
		/// Returns the number of elements in the tree.
		/// </summary>
		public int ElementCount
		{
			get { return count; }
		}

		/// 
		/// <summary>
		/// Enumerate all the items in-order
		/// </summary>
		/// <returns>An enumerator for all the items, in order.</returns>
		/// <exception cref="InvalidOperationException">The tree has an item added or deleted during the enumeration.</exception>
		public IEnumerator<T> GetEnumerator()
		{
			return EnumerateRange(EntireRangeTester).GetEnumerator();
		}

		/// <summary>
		/// Enumerate all the items in-order
		/// </summary>
		/// <returns>An enumerator for all the items, in order.</returns>
		/// <exception cref="InvalidOperationException">The tree has an item added or deleted during the enumeration.</exception>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#region Ranges

		#region Delegates

		/// <summary>
		/// A delegate that tests if an item is within a custom range. The range must be a contiguous
		/// range of items with the ordering of this tree. The range test function must test
		/// if an item is before, withing, or after the range.
		/// </summary>
		/// <param name="item">Item to test against the range.</param>
		/// <returns>Returns negative if item is before the range, zero if item is withing the range,
		/// and positive if item is after the range.</returns>
		public delegate int RangeTester(T item);

		#endregion

		/// <summary>
		/// Gets a range tester that defines a range by first and last items.
		/// </summary>
		/// <param name="useFirst">If true, bound the range on the bottom by first.</param>
		/// <param name="first">If useFirst is true, the inclusive lower bound.</param>
		/// <param name="useLast">If true, bound the range on the top by last.</param>
		/// <param name="last">If useLast is true, the exclusive upper bound.</param>
		/// <returns>A RangeTester delegate that tests for an item in the given range.</returns>
		public RangeTester BoundedRangeTester(bool useFirst, T first, bool useLast, T last)
		{
			return delegate(T item)
				{
					if (useFirst && comparer.Compare(first, item) > 0)
						return -1; // item is before first.
					else if (useLast && comparer.Compare(last, item) <= 0)
						return 1; // item is after or equal to last.
					else
						return 0; // item is greater or equal to first, and less than last.
				};
		}

		/// <summary>
		/// Gets a range tester that defines a range by first and last items.
		/// </summary>
		/// <param name="first">The lower bound.</param>
		/// <param name="firstInclusive">True if the lower bound is inclusive, false if exclusive.</param>
		/// <param name="last">The upper bound.</param>
		/// <param name="lastInclusive">True if the upper bound is inclusive, false if exclusive.</param>
		/// <returns>A RangeTester delegate that tests for an item in the given range.</returns>
		public RangeTester DoubleBoundedRangeTester(T first, bool firstInclusive, T last, bool lastInclusive)
		{
			return delegate(T item)
				{
					if (firstInclusive)
					{
						if (comparer.Compare(first, item) > 0)
							return -1; // item is before first.
					}
					else
					{
						if (comparer.Compare(first, item) >= 0)
							return -1; // item is before or equal to first.
					}

					if (lastInclusive)
					{
						if (comparer.Compare(last, item) < 0)
							return 1; // item is after last.
					}
					else
					{
						if (comparer.Compare(last, item) <= 0)
							return 1; // item is after or equal to last
					}

					return 0; // item is between first and last.
				};
		}


		/// <summary>
		/// Gets a range tester that defines a range by a lower bound.
		/// </summary>
		/// <param name="first">The lower bound.</param>
		/// <param name="inclusive">True if the lower bound is inclusive, false if exclusive.</param>
		/// <returns>A RangeTester delegate that tests for an item in the given range.</returns>
		public RangeTester LowerBoundedRangeTester(T first, bool inclusive)
		{
			return delegate(T item)
				{
					if (inclusive)
					{
						if (comparer.Compare(first, item) > 0)
							return -1; // item is before first.
						else
							return 0; // item is after or equal to first
					}
					else
					{
						if (comparer.Compare(first, item) >= 0)
							return -1; // item is before or equal to first.
						else
							return 0; // item is after first
					}
				};
		}


		/// <summary>
		/// Gets a range tester that defines a range by upper bound.
		/// </summary>
		/// <param name="last">The upper bound.</param>
		/// <param name="inclusive">True if the upper bound is inclusive, false if exclusive.</param>
		/// <returns>A RangeTester delegate that tests for an item in the given range.</returns>
		public RangeTester UpperBoundedRangeTester(T last, bool inclusive)
		{
			return delegate(T item)
				{
					if (inclusive)
					{
						if (comparer.Compare(last, item) < 0)
							return 1; // item is after last.
						else
							return 0; // item is before or equal to last.
					}
					else
					{
						if (comparer.Compare(last, item) <= 0)
							return 1; // item is after or equal to last
						else
							return 0; // item is before last.
					}
				};
		}

		/// <summary>
		/// Gets a range tester that defines a range by all items equal to an item.
		/// </summary>
		/// <param name="equalTo">The item that is contained in the range.</param>
		/// <returns>A RangeTester delegate that tests for an item equal to <paramref name="equalTo"/>.</returns>
		public RangeTester EqualRangeTester(T equalTo)
		{
			return delegate(T item)
				{
					return comparer.Compare(item, equalTo);
				};
		}

		/// <summary>
		/// A range tester that defines a range that is the entire tree.
		/// </summary>
		/// <param name="item">Item to test.</param>
		/// <returns>Always returns 0.</returns>
		public int EntireRangeTester(T item)
		{
			return 0;
		}

		/// <summary>
		/// Enumerate the items in a custom range in the tree. The range is determined by 
		/// a RangeTest delegate.
		/// </summary>
		/// <param name="rangeTester">Tests an item against the custom range.</param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates the custom range in order.</returns>
		/// <exception cref="InvalidOperationException">The tree has an item added or deleted during the enumeration.</exception>
		public IEnumerable<T> EnumerateRange(RangeTester rangeTester)
		{
			return EnumerateRangeInOrder(rangeTester, root);
		}

		/// <summary>
		/// Enumerate the items in a custom range in the tree, in reversed order. The range is determined by 
		/// a RangeTest delegate.
		/// </summary>
		/// <param name="rangeTester">Tests an item against the custom range.</param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates the custom range in reversed order.</returns>
		/// <exception cref="InvalidOperationException">The tree has an item added or deleted during the enumeration.</exception>
		public IEnumerable<T> EnumerateRangeReversed(RangeTester rangeTester)
		{
			return EnumerateRangeInReversedOrder(rangeTester, root);
		}


		/// <summary>
		/// Deletes either the first or last item from a range, as identified by a RangeTester
		/// delegate. If the range is empty, returns false.
		/// </summary>
		/// <remarks>Top-down algorithm from Weiss. Basic plan is to move down in the tree, 
		/// rotating and recoloring along the way to always keep the current node red, which 
		/// ensures that the node we delete is red. The details are quite complex, however! </remarks>
		/// <param name="rangeTester">Range to delete from.</param>
		/// <param name="deleteFirst">If true, delete the first item from the range, else the last.</param>
		/// <param name="item">Returns the item that was deleted, if true returned.</param>
		/// <returns>True if an element was deleted, false if the range is empty.</returns>
		public bool DeleteItemFromRange(RangeTester rangeTester, bool deleteFirst, out T item)
		{
			Node node; // The current node.
			Node parent; // Parent of the current node.
			Node gparent; // Grandparent of the current node.
			Node sib; // Sibling of the current node.
			Node keyNode; // Node with the key that is being removed.

			// The tree may be changed.
			StopEnumerations();

			if (root == null)
			{
				// Nothing in the tree. Go home now.
				item = default(T);
				return false;
			}

			// We decrement counts on the way down the tree. If we end up not finding an item to delete
			// we need a stack to adjust the counts back. 
			Node[] nodeStack = GetNodeStack();
			int nodeStackPtr = 0; // first free item on the stack.

			// Start at the root.
			node = root;
			sib = parent = gparent = null;
			keyNode = null;

			// Proceed down the tree, making the current node red so it can be removed.
			for (;;)
			{
				Debug.Assert(parent == null || parent.IsRed);
				Debug.Assert(sib == null || !sib.IsRed);
				Debug.Assert(!node.IsRed);

				if ((node.left == null || !node.left.IsRed) && (node.right == null || !node.right.IsRed))
				{
					// node has two black children (null children are considered black).
					if (parent == null)
					{
						// Special case for the root.
						Debug.Assert(node == root);
						node.IsRed = true;
					}
					else if ((sib.left == null || !sib.left.IsRed) && (sib.right == null || !sib.right.IsRed))
					{
						// sib has two black children.
						node.IsRed = true;
						sib.IsRed = true;
						parent.IsRed = false;
					}
					else
					{
						if (parent.left == node && (sib.right == null || !sib.right.IsRed))
						{
							// sib has a black child on the opposite side as node.
							Node tleft = sib.left;
							Rotate(parent, sib, tleft);
							sib = tleft;
						}
						else if (parent.right == node && (sib.left == null || !sib.left.IsRed))
						{
							// sib has a black child on the opposite side as node.
							Node tright = sib.right;
							Rotate(parent, sib, tright);
							sib = tright;
						}

						// sib has a red child.
						Rotate(gparent, parent, sib);
						node.IsRed = true;
						sib.IsRed = true;
						sib.left.IsRed = false;
						sib.right.IsRed = false;

						sib.DecrementCount();
						nodeStack[nodeStackPtr - 1] = sib;
						parent.DecrementCount();
						nodeStack[nodeStackPtr++] = parent;
					}
				}

				// Compare the key and move down the tree to the correct child.
				do
				{
					Node nextNode, nextSib; // Node we've moving to, and it's sibling.

					node.DecrementCount();
					nodeStack[nodeStackPtr++] = node;

					// Determine which way to move in the tree by comparing the 
					// current item to what we're looking for.
					int compare = rangeTester(node.item);

					if (compare == 0)
					{
						// We've found the node to remove. Remember it, then keep traversing the
						// tree to either find the first/last of equal keys, and if needed, the predecessor
						// or successor (the actual node to be removed).
						keyNode = node;
						if (deleteFirst)
						{
							nextNode = node.left;
							nextSib = node.right;
						}
						else
						{
							nextNode = node.right;
							nextSib = node.left;
						}
					}
					else if (compare > 0)
					{
						nextNode = node.left;
						nextSib = node.right;
					}
					else
					{
						nextNode = node.right;
						nextSib = node.left;
					}

					// Have we reached the end of our tree walk?
					if (nextNode == null)
						goto FINISHED;

					// Move down the tree.
					gparent = parent;
					parent = node;
					node = nextNode;
					sib = nextSib;
				} while (!parent.IsRed && node.IsRed);

				if (!parent.IsRed)
				{
					Debug.Assert(!node.IsRed);
					// moved to a black child.
					Rotate(gparent, parent, sib);

					sib.DecrementCount();
					nodeStack[nodeStackPtr - 1] = sib;
					parent.DecrementCount();
					nodeStack[nodeStackPtr++] = parent;

					sib.IsRed = false;
					parent.IsRed = true;
					gparent = sib;
					sib = (parent.left == node) ? parent.right : parent.left;
				}
			}

			FINISHED:
			if (keyNode == null)
			{
				// We never found a node to delete.

				// Return counts back to their previous value.
				for (int i = 0; i < nodeStackPtr; ++i)
					nodeStack[i].IncrementCount();

				// Color the root black, in case it was colored red above.
				if (root != null)
					root.IsRed = false;

				item = default(T);
				return false;
			}

			// Return the item from the node we're deleting.
			item = keyNode.item;

			// At a leaf or a node with one child which is a leaf. Remove the node.
			if (keyNode != node)
			{
				// The node we want to delete is interior. Move the item from the
				// node we're actually deleting to the key node.
				keyNode.item = node.item;
			}

			// If we have one child, replace the current with the child, otherwise,
			// replace the current node with null.
			Node replacement;
			if (node.left != null)
			{
				replacement = node.left;
				Debug.Assert(!node.IsRed && replacement.IsRed);
				replacement.IsRed = false;
			}
			else if (node.right != null)
			{
				replacement = node.right;
				Debug.Assert(!node.IsRed && replacement.IsRed);
				replacement.IsRed = false;
			}
			else
				replacement = null;

			if (parent == null)
			{
				Debug.Assert(root == node);
				root = replacement;
			}
			else if (parent.left == node)
				parent.left = replacement;
			else
			{
				Debug.Assert(parent.right == node);
				parent.right = replacement;
			}

			// Color the root black, in case it was colored red above.
			if (root != null)
				root.IsRed = false;

			// Update item count.
			count -= 1;

			// And we're done.
			return true;
		}

		/// <summary>
		/// Delete all the items in a range, identified by a RangeTester delegate.
		/// </summary>
		/// <param name="rangeTester">The delegate that defines the range to delete.</param>
		/// <returns>The number of items deleted.</returns>
		public int DeleteRange(RangeTester rangeTester)
		{
			bool deleted;
			int counter = 0;
			T dummy;

			do
			{
				deleted = DeleteItemFromRange(rangeTester, true, out dummy);
				if (deleted)
					++counter;
			} while (deleted);

			return counter;
		}

		/// <summary>
		/// Count the items in a custom range in the tree. The range is determined by 
		/// a RangeTester delegate.
		/// </summary>
		/// <param name="rangeTester">The delegate that defines the range.</param>
		/// <returns>The number of items in the range.</returns>
		public int CountRange(RangeTester rangeTester)
		{
			return CountRangeUnderNode(rangeTester, root, false, false);
		}

		/// <summary>
		/// Find the first item in a custom range in the tree, and it's index. The range is determined
		/// by a RangeTester delegate.
		/// </summary>
		/// <param name="rangeTester">The delegate that defines the range.</param>
		/// <param name="item">Returns the item found, if true was returned.</param>
		/// <returns>Index of first item in range if range is non-empty, -1 otherwise.</returns>
		public int FirstItemInRange(RangeTester rangeTester, out T item)
		{
			Node node = root, found = null;
			int curCount = 0, foundIndex = -1;

			while (node != null)
			{
				int compare = rangeTester(node.item);

				if (compare == 0)
				{
					found = node;
					if (node.left != null)
						foundIndex = curCount + node.left.Count;
					else
						foundIndex = curCount;
				}

				if (compare >= 0)
					node = node.left;
				else
				{
					if (node.left != null)
						curCount += node.left.Count + 1;
					else
						curCount += 1;
					node = node.right;
				}
			}

			if (found != null)
			{
				item = found.item;
				return foundIndex;
			}
			else
			{
				item = default(T);
				return -1;
			}
		}

		/// <summary>
		/// Find the last item in a custom range in the tree, and it's index. The range is determined
		/// by a RangeTester delegate.
		/// </summary>
		/// <param name="rangeTester">The delegate that defines the range.</param>
		/// <param name="item">Returns the item found, if true was returned.</param>
		/// <returns>Index of the item if range is non-empty, -1 otherwise.</returns>
		public int LastItemInRange(RangeTester rangeTester, out T item)
		{
			Node node = root, found = null;
			int curCount = 0, foundIndex = -1;

			while (node != null)
			{
				int compare = rangeTester(node.item);

				if (compare == 0)
				{
					found = node;
					if (node.left != null)
						foundIndex = curCount + node.left.Count;
					else
						foundIndex = curCount;
				}

				if (compare <= 0)
				{
					if (node.left != null)
						curCount += node.left.Count + 1;
					else
						curCount += 1;
					node = node.right;
				}
				else
					node = node.left;
			}

			if (found != null)
			{
				item = found.item;
				return foundIndex;
			}
			else
			{
				item = default(T);
				return foundIndex;
			}
		}

		/// <summary>
		/// Enumerate all the items in a custom range, under and including node, in-order.
		/// </summary>
		/// <param name="rangeTester">Tests an item against the custom range.</param>
		/// <param name="node">Node to begin enumeration. May be null.</param>
		/// <returns>An enumerable of the items.</returns>
		/// <exception cref="InvalidOperationException">The tree has an item added or deleted during the enumeration.</exception>
		private IEnumerable<T> EnumerateRangeInOrder(RangeTester rangeTester, Node node)
		{
			int startStamp = changeStamp;

			if (node != null)
			{
				int compare = rangeTester(node.item);

				if (compare >= 0)
				{
					// At least part of the range may lie to the left.
					foreach (T item in EnumerateRangeInOrder(rangeTester, node.left))
					{
						yield return item;
						CheckEnumerationStamp(startStamp);
					}
				}

				if (compare == 0)
				{
					// The item is within the range.
					yield return node.item;
					CheckEnumerationStamp(startStamp);
				}

				if (compare <= 0)
				{
					// At least part of the range lies to the right.
					foreach (T item in EnumerateRangeInOrder(rangeTester, node.right))
					{
						yield return item;
						CheckEnumerationStamp(startStamp);
					}
				}
			}
		}

		/// <summary>
		/// Enumerate all the items in a custom range, under and including node, in reversed order.
		/// </summary>
		/// <param name="rangeTester">Tests an item against the custom range.</param>
		/// <param name="node">Node to begin enumeration. May be null.</param>
		/// <returns>An enumerable of the items, in reversed oreder.</returns>
		/// <exception cref="InvalidOperationException">The tree has an item added or deleted during the enumeration.</exception>
		private IEnumerable<T> EnumerateRangeInReversedOrder(RangeTester rangeTester, Node node)
		{
			int startStamp = changeStamp;

			if (node != null)
			{
				int compare = rangeTester(node.item);

				if (compare <= 0)
				{
					// At least part of the range lies to the right.
					foreach (T item in EnumerateRangeInReversedOrder(rangeTester, node.right))
					{
						yield return item;
						CheckEnumerationStamp(startStamp);
					}
				}

				if (compare == 0)
				{
					// The item is within the range.
					yield return node.item;
					CheckEnumerationStamp(startStamp);
				}

				if (compare >= 0)
				{
					// At least part of the range may lie to the left.
					foreach (T item in EnumerateRangeInReversedOrder(rangeTester, node.left))
					{
						yield return item;
						CheckEnumerationStamp(startStamp);
					}
				}
			}
		}

		/// <summary>
		/// Count all the items in a custom range, under and including node.
		/// </summary>
		/// <param name="rangeTester">The delegate that defines the range.</param>
		/// <param name="node">Node to begin enumeration. May be null.</param>
		/// <param name="belowRangeTop">This node and all under it are either in the range or below it.</param>
		/// <param name="aboveRangeBottom">This node and all under it are either in the range or above it.</param>
		/// <returns>The number of items in the range, under and include node.</returns>
		private int CountRangeUnderNode(RangeTester rangeTester, Node node, bool belowRangeTop, bool aboveRangeBottom)
		{
			if (node != null)
			{
				if (belowRangeTop && aboveRangeBottom)
				{
					// This node and all below it must be in the range. Use the predefined count.
					return node.Count;
				}

				int compare = rangeTester(node.item);
				int counter;

				if (compare == 0)
				{
					counter = 1; // the node itself
					counter += CountRangeUnderNode(rangeTester, node.left, true, aboveRangeBottom);
					counter += CountRangeUnderNode(rangeTester, node.right, belowRangeTop, true);
				}
				else if (compare < 0)
				{
					counter = CountRangeUnderNode(rangeTester, node.right, belowRangeTop, aboveRangeBottom);
				}
				else
				{
					// compare > 0
					counter = CountRangeUnderNode(rangeTester, node.left, belowRangeTop, aboveRangeBottom);
				}

				return counter;
			}
			else
			{
				return 0;
			}
		}

		#endregion Ranges

		/// <summary>
		/// Clone the tree, returning a new tree containing the same items. Should
		/// take O(N) take.
		/// </summary>
		/// <returns>Clone version of this tree.</returns>
		public RedBlackTree<T> Clone()
		{
			RedBlackTree<T> newTree = new RedBlackTree<T>(comparer);
			newTree.count = this.count;
			if (this.root != null)
				newTree.root = this.root.Clone();
			return newTree;
		}

		/// <summary>
		/// Finds the key in the tree. If multiple items in the tree have
		/// compare equal to the key, finds the first or last one. Optionally replaces the item
		/// with the one searched for.
		/// </summary>
		/// <param name="key">Key to search for.</param>
		/// <param name="findFirst">If true, find the first of duplicates, else finds the last of duplicates.</param>
		/// <param name="replace">If true, replaces the item with key (if function returns true)</param>
		/// <param name="item">Returns the found item, before replacing (if function returns true).</param>
		/// <returns>True if the key was found.</returns>
		public bool Find(T key, bool findFirst, bool replace, out T item)
		{
			Node current = root; // current search location in the tree
			Node found = null; // last node found with the key, or null if none.

			while (current != null)
			{
				int compare = comparer.Compare(key, current.item);

				if (compare < 0)
				{
					current = current.left;
				}
				else if (compare > 0)
				{
					current = current.right;
				}
				else
				{
					// Go left/right on equality to find first/last of elements with this key.
					Debug.Assert(compare == 0);
					found = current;
					if (findFirst)
						current = current.left;
					else
						current = current.right;
				}
			}

			if (found != null)
			{
				item = found.item;
				if (replace)
					found.item = key;
				return true;
			}
			else
			{
				item = default(T);
				return false;
			}
		}

		/// <summary>
		/// Finds the index of the key in the tree. If multiple items in the tree have
		/// compare equal to the key, finds the first or last one. 
		/// </summary>
		/// <param name="key">Key to search for.</param>
		/// <param name="findFirst">If true, find the first of duplicates, else finds the last of duplicates.</param>
		/// <returns>Index of the item found if the key was found, -1 if not found.</returns>
		public int FindIndex(T key, bool findFirst)
		{
			T dummy;
			if (findFirst)
				return FirstItemInRange(EqualRangeTester(key), out dummy);
			else
				return LastItemInRange(EqualRangeTester(key), out dummy);
		}

		/// <summary>
		/// Find the item at a particular index in the tree.
		/// </summary>
		/// <param name="index">The zero-based index of the item. Must be &gt;= 0 and &lt; Count.</param>
		/// <returns>The item at the particular index.</returns>
		public T GetItemByIndex(int index)
		{
			if (index < 0 || index >= count)
				throw new ArgumentOutOfRangeException("index");

			Node current = root; // current search location in the tree

			for (;;)
			{
				int leftCount;

				if (current.left != null)
					leftCount = current.left.Count;
				else
					leftCount = 0;

				if (leftCount > index)
					current = current.left;
				else if (leftCount == index)
					return current.item;
				else
				{
					index -= leftCount + 1;
					current = current.right;
				}
			}
		}

		/// <summary>
		/// Insert a new node into the tree, maintaining the red-black invariants.
		/// </summary>
		/// <remarks>Algorithm from Sedgewick, "Algorithms".</remarks>
		/// <param name="item">The new item to insert</param>
		/// <param name="dupPolicy">What to do if equal item is already present.</param>
		/// <param name="previous">If false, returned, the previous item.</param>
		/// <returns>false if duplicate exists, otherwise true.</returns>
		public bool Insert(T item, DuplicatePolicy dupPolicy, out T previous)
		{
			Node node = root;
			Node parent = null, gparent = null, ggparent = null; // parent, grand, a great-grantparent of node.
			bool wentLeft = false, wentRight = false; // direction from parent to node.
			bool rotated;
			Node duplicateFound = null;

			// The tree may be changed.
			StopEnumerations();

			// We increment counts on the way down the tree. If we end up not inserting an items due
			// to a duplicate, we need a stack to adjust the counts back. We don't need the stack if the duplicate
			// policy means that we will always do an insertion.
			bool needStack = !((dupPolicy == DuplicatePolicy.InsertFirst) || (dupPolicy == DuplicatePolicy.InsertLast));
			Node[] nodeStack = null;
			int nodeStackPtr = 0; // first free item on the stack.
			if (needStack)
				nodeStack = GetNodeStack();

			while (node != null)
			{
				// If we find a node with two red children, split it so it doesn't cause problems
				// when inserting a node.
				if (node.left != null && node.left.IsRed && node.right != null && node.right.IsRed)
				{
					node = InsertSplit(ggparent, gparent, parent, node, out rotated);

					if (needStack && rotated)
					{
						nodeStackPtr -= 2;
						if (nodeStackPtr < 0)
							nodeStackPtr = 0;
					}
				}

				// Keep track of parent, grandparent, great-grand parent.
				ggparent = gparent;
				gparent = parent;
				parent = node;

				// Compare the key and the node. 
				int compare = comparer.Compare(item, node.item);

				if (compare == 0)
				{
					// Found a node with the data already. Check duplicate policy.
					if (dupPolicy == DuplicatePolicy.DoNothing)
					{
						previous = node.item;

						// Didn't insert after all. Return counts back to their previous value.
						for (int i = 0; i < nodeStackPtr; ++i)
							nodeStack[i].DecrementCount();

						return false;
					}
					else if (dupPolicy == DuplicatePolicy.InsertFirst || dupPolicy == DuplicatePolicy.ReplaceFirst)
					{
						// Insert first by treating the key as less than nodes in the tree.
						duplicateFound = node;
						compare = -1;
					}
					else
					{
						Debug.Assert(dupPolicy == DuplicatePolicy.InsertLast || dupPolicy == DuplicatePolicy.ReplaceLast);
						// Insert last by treating the key as greater than nodes in the tree.
						duplicateFound = node;
						compare = 1;
					}
				}

				Debug.Assert(compare != 0);

				node.IncrementCount();
				if (needStack)
					nodeStack[nodeStackPtr++] = node;

				// Move to the left or right as needed to find the insertion point.
				if (compare < 0)
				{
					node = node.left;
					wentLeft = true;
					wentRight = false;
				}
				else
				{
					node = node.right;
					wentRight = true;
					wentLeft = false;
				}
			}

			if (duplicateFound != null)
			{
				previous = duplicateFound.item;

				// Are we replacing instread of inserting?
				if (dupPolicy == DuplicatePolicy.ReplaceFirst || dupPolicy == DuplicatePolicy.ReplaceLast)
				{
					duplicateFound.item = item;

					// Didn't insert after all. Return counts back to their previous value.
					for (int i = 0; i < nodeStackPtr; ++i)
						nodeStack[i].DecrementCount();

					return false;
				}
			}
			else
			{
				previous = default(T);
			}

			// Create a new node.
			node = new Node();
			node.item = item;
			node.Count = 1;

			// Link the node into the tree.
			if (wentLeft)
				parent.left = node;
			else if (wentRight)
				parent.right = node;
			else
			{
				Debug.Assert(root == null);
				root = node;
			}

			// Maintain the red-black policy.
			InsertSplit(ggparent, gparent, parent, node, out rotated);

			// We've added a node to the tree, so update the count.
			count += 1;

			return (duplicateFound == null);
		}

		/// <summary>
		/// Deletes a key from the tree. If multiple elements are equal to key, 
		/// deletes the first or last. If no element is equal to the key, 
		/// returns false.
		/// </summary>
		/// <remarks>Top-down algorithm from Weiss. Basic plan is to move down in the tree, 
		/// rotating and recoloring along the way to always keep the current node red, which 
		/// ensures that the node we delete is red. The details are quite complex, however! </remarks>
		/// <param name="key">Key to delete.</param>
		/// <param name="deleteFirst">Which item to delete if multiple are equal to key. True to delete the first, false to delete last.</param>
		/// <param name="item">Returns the item that was deleted, if true returned.</param>
		/// <returns>True if an element was deleted, false if no element had 
		/// specified key.</returns>
		public bool Delete(T key, bool deleteFirst, out T item)
		{
			return DeleteItemFromRange(EqualRangeTester(key), deleteFirst, out item);
		}

		/// <summary>
		/// Prints out the tree.
		/// </summary>
		public void Print()
		{
			PrintSubTree(root, "", "");
			Console.WriteLine();
		}

		/// <summary>
		/// Validates that the tree is correctly sorted, and meets the red-black tree 
		/// axioms.
		/// </summary>
		public void Validate()
		{
			Debug.Assert(comparer != null, "Comparer should not be null");

			if (root == null)
			{
				Debug.Assert(0 == count, "Count in empty tree should be 0.");
			}
			else
			{
				Debug.Assert(! root.IsRed, "Root is not black");
				int blackHeight;
				int nodeCount = ValidateSubTree(root, out blackHeight);
				Debug.Assert(nodeCount == this.count, "Node count of tree is not correct.");
			}
		}

		/// <summary>
		/// Must be called whenever there is a structural change in the tree. Causes
		/// changeStamp to be changed, which causes any in-progress enumerations
		/// to throw exceptions.
		/// </summary>
		internal void StopEnumerations()
		{
			++changeStamp;
		}

		/// <summary>
		/// Create an array of Nodes big enough for any path from top 
		/// to bottom. This is cached, and reused from call-to-call, so only one
		/// can be around at a time per tree.
		/// </summary>
		/// <returns>The node stack.</returns>
		private Node[] GetNodeStack()
		{
			// Maximum depth needed is 2 * lg count + 1.
			int maxDepth;
			if (count < 0x400)
				maxDepth = 21;
			else if (count < 0x10000)
				maxDepth = 41;
			else
				maxDepth = 65;

			if (stack == null || stack.Length < maxDepth)
				stack = new Node[maxDepth];

			return stack;
		}

		/// <summary>
		/// The class that is each node in the red-black tree.
		/// </summary>
		[Serializable]
		private class Node
		{
			private const uint REDMASK = 0x80000000;
			private uint count;
			public T item;
			public Node left, right;

			/// <summary>
			/// Is this a red node?
			/// </summary>
			public bool IsRed
			{
				get { return (count & REDMASK) != 0; }
				set
				{
					if (value)
						count |= REDMASK;
					else
						count &= ~REDMASK;
				}
			}

			/// <summary>
			/// Get or set the Count field -- a 31-bit field
			/// that holds the number of nodes at or below this
			/// level.
			/// </summary>
			public int Count
			{
				get { return (int) (count & ~REDMASK); }
				set { count = (count & REDMASK) | (uint) value; }
			}

			/// <summary>
			/// Add one to the Count.
			/// </summary>
			public void IncrementCount()
			{
				++count;
			}

			/// <summary>
			/// Subtract one from the Count. The current
			/// Count must be non-zero.
			/// </summary>
			public void DecrementCount()
			{
				Debug.Assert(Count != 0);
				--count;
			}

			/// <summary>
			/// Clones a node and all its descendants.
			/// </summary>
			/// <returns>The cloned node.</returns>
			public Node Clone()
			{
				Node newNode = new Node();
				newNode.item = item;

				newNode.count = count;

				if (left != null)
					newNode.left = left.Clone();

				if (right != null)
					newNode.right = right.Clone();

				return newNode;
			}
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
		/// Split a node with two red children (a 4-node in the 2-3-4 tree formalism), as
		/// part of an insert operation.
		/// </summary>
		/// <param name="ggparent">great grand-parent of "node", can be null near root</param>
		/// <param name="gparent">grand-parent of "node", can be null near root</param>
		/// <param name="parent">parent of "node", can be null near root</param>
		/// <param name="node">Node to split, can't be null</param>
		/// <param name="rotated">Indicates that rotation(s) occurred in the tree.</param>
		/// <returns>Node to continue searching from.</returns>
		private Node InsertSplit(Node ggparent, Node gparent, Node parent, Node node, out bool rotated)
		{
			if (node != root)
				node.IsRed = true;
			if (node.left != null)
				node.left.IsRed = false;
			if (node.right != null)
				node.right.IsRed = false;

			if (parent != null && parent.IsRed)
			{
				// Since parent is red, gparent can't be null (root is always black). ggparent
				// might be null, however.
				Debug.Assert(gparent != null);

				// if links from gparent and parent are opposite (left/right or right/left),
				// then rotate.
				if ((gparent.left == parent) != (parent.left == node))
				{
					Rotate(gparent, parent, node);
					parent = node;
				}

				gparent.IsRed = true;

				// Do a rotate to prevent two red links in a row.
				Rotate(ggparent, gparent, parent);

				parent.IsRed = false;
				rotated = true;
				return parent;
			}
			else
			{
				rotated = false;
				return node;
			}
		}

		/// <summary>
		/// Performs a rotation involving the node, it's child and grandchild. The counts of 
		/// childs and grand-child are set the correct values from their children; this is important
		/// if they have been adjusted on the way down the try as part of an insert/delete.
		/// </summary>
		/// <param name="node">Top node of the rotation. Can be null if child==root.</param>
		/// <param name="child">One child of "node". Not null.</param>
		/// <param name="gchild">One child of "child". Not null.</param>
		private void Rotate(Node node, Node child, Node gchild)
		{
			if (gchild == child.left)
			{
				child.left = gchild.right;
				gchild.right = child;
			}
			else
			{
				Debug.Assert(gchild == child.right);
				child.right = gchild.left;
				gchild.left = child;
			}

			// Restore the counts.
			child.Count = (child.left != null ? child.left.Count : 0) + (child.right != null ? child.right.Count : 0) + 1;
			gchild.Count = (gchild.left != null ? gchild.left.Count : 0) + (gchild.right != null ? gchild.right.Count : 0) + 1;

			if (node == null)
			{
				Debug.Assert(child == root);
				root = gchild;
			}
			else if (child == node.left)
			{
				node.left = gchild;
			}
			else
			{
				Debug.Assert(child == node.right);
				node.right = gchild;
			}
		}

		/// <summary>
		/// Prints a sub-tree.
		/// </summary>
		/// <param name="node">Node to print from</param>
		/// <param name="prefixNode">Prefix for the node</param>
		/// <param name="prefixChildren">Prefix for the node's children</param>
		private void PrintSubTree(Node node, string prefixNode, string prefixChildren)
		{
			if (node == null)
				return;

			// Red nodes marked as "@@", black nodes as "..".
			Console.WriteLine("{0}{1} {2,4} {3}", prefixNode, node.IsRed ? "@@" : "..", node.Count, node.item);

			PrintSubTree(node.left, prefixChildren + "|-L-", prefixChildren + "|  ");
			PrintSubTree(node.right, prefixChildren + "|-R-", prefixChildren + "   ");
		}

		/// <summary>
		/// Validates a sub-tree and returns the count and black height.
		/// </summary>
		/// <param name="node">Sub-tree to validate. May be null.</param>
		/// <param name="blackHeight">Returns the black height of the tree.</param>
		/// <returns>Returns the number of nodes in the sub-tree. 0 if node is null.</returns>
		private int ValidateSubTree(Node node, out int blackHeight)
		{
			if (node == null)
			{
				blackHeight = 0;
				return 0;
			}

			// Check that this node is sorted with respect to any children.
			if (node.left != null)
				Debug.Assert(comparer.Compare(node.left.item, node.item) <= 0, "Left child is not less than or equal to node");
			if (node.right != null)
				Debug.Assert(comparer.Compare(node.right.item, node.item) >= 0, "Right child is not greater than or equal to node");

			// Check that the two-red rule is not violated.
			if (node.IsRed)
			{
				if (node.left != null)
					Debug.Assert(! node.left.IsRed, "Node and left child both red");
				if (node.right != null)
					Debug.Assert(! node.right.IsRed, "Node and right child both red");
			}

			// Validate sub-trees and get their size and heights.
			int leftCount, leftBlackHeight;
			int rightCount, rightBlackHeight;
			int ourCount;

			leftCount = ValidateSubTree(node.left, out leftBlackHeight);
			rightCount = ValidateSubTree(node.right, out rightBlackHeight);
			ourCount = leftCount + rightCount + 1;

			Debug.Assert(ourCount == node.Count);

			// Validate the equal black-height rule.
			Debug.Assert(leftBlackHeight == rightBlackHeight, "Black heights are not equal");

			// Calculate our black height and return the count
			blackHeight = leftBlackHeight;
			if (! node.IsRed)
				blackHeight += 1;
			return ourCount;
		}
	}
}