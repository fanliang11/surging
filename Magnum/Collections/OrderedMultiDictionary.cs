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
	/// <para>The OrderedMultiDictionary class that associates values with a key. Unlike an OrderedDictionary,
	/// each key can have multiple values associated with it. When indexing an OrderedMultidictionary, instead
	/// of a single value associated with a key, you retrieve an enumeration of values.</para>
	/// <para>All of the key are stored in sorted order. Also, the values associated with a given key 
	/// are kept in sorted order as well.</para>
	/// <para>When constructed, you can chose to allow the same value to be associated with a key multiple
	/// times, or only one time. </para>
	/// </summary>
	/// <typeparam name="TKey">The type of the keys.</typeparam>
	/// <typeparam name="TValue">The of values associated with the keys.</typeparam>
	///<seealso cref="MultiDictionary{TKey,TValue}"/>
	///<seealso cref="OrderedDictionary&lt;TKey,TValue&gt;"/>
	[Serializable]
	public class OrderedMultiDictionary<TKey, TValue> : MultiDictionaryBase<TKey, TValue>,
		ICloneable
	{
		// The comparer for comparing keys
		private readonly bool allowDuplicateValues;
		private readonly IComparer<KeyValuePair<TKey, TValue>> comparer;
		private readonly IComparer<TKey> keyComparer;

		// The comparer for comparing values;
		private readonly IComparer<TValue> valueComparer;

		// The comparer for comparing key-value pairs. Ordered by keys, then by values

		// Total number of keys in the tree.
		private int keyCount;
		private RedBlackTree<KeyValuePair<TKey, TValue>> tree;

		/// <summary>
		/// The OrderedMultiDictionary&lt;TKey,TValue&gt;.View class is used to look at a subset of the keys and values
		/// inside an ordered multi-dictionary. It is returned from the Range, RangeTo, RangeFrom, and Reversed methods. 
		/// </summary>
		///<remarks>
		/// <p>Views are dynamic. If the underlying dictionary changes, the view changes in sync. If a change is made
		/// to the view, the underlying dictionary changes accordingly.</p>
		///<p>Typically, this class is used in conjunction with a foreach statement to enumerate the keys
		/// and values in a subset of the OrderedMultiDictionary. For example:</p>
		///<code>
		/// foreach(KeyValuePair&lt;TKey, TValue&gt; pair in dictionary.Range(from, to)) {
		///    // process pair
		/// }
		///</code>
		///</remarks>
		[Serializable]
		public class View : MultiDictionaryBase<TKey, TValue>
		{
			private readonly bool entireTree; // is the view the whole tree?
			private readonly OrderedMultiDictionary<TKey, TValue> myDictionary;
			private readonly RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester rangeTester; // range tester for the range being used.
			private readonly bool reversed; // is the view reversed?

			/// <summary>
			/// Initialize the View.
			/// </summary>
			/// <param name="myDictionary">Associated OrderedMultiDictionary to be viewed.</param>
			/// <param name="rangeTester">Range tester that defines the range being used.</param>
			/// <param name="entireTree">If true, then rangeTester defines the entire tree.</param>
			/// <param name="reversed">Is the view enuemerated in reverse order?</param>
			internal View(OrderedMultiDictionary<TKey, TValue> myDictionary, RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester rangeTester, bool entireTree, bool reversed)
			{
				this.myDictionary = myDictionary;
				this.rangeTester = rangeTester;
				this.entireTree = entireTree;
				this.reversed = reversed;
			}

			/// <summary>
			/// Number of keys in this view.
			/// </summary>
			/// <value>Number of keys that lie within the bounds the view.</value>
			public override sealed int Count
			{
				get
				{
					if (entireTree)
						return myDictionary.Count;
					else
					{
						int count = 0;

						using (IEnumerator<TKey> enumKeys = myDictionary.EnumerateKeys(rangeTester, reversed))
						{
							while (enumKeys.MoveNext())
							{
								++count;
							}
						}

						return count;
					}
				}
			}

			/// <summary>
			/// Tests if the key is present in the part of the dictionary being viewed.
			/// </summary>
			/// <param name="key">Key to check</param>
			/// <returns>True if the key is within this view. </returns>
			public override sealed bool ContainsKey(TKey key)
			{
				if (!KeyInView(key))
					return false;
				else
					return myDictionary.ContainsKey(key);
			}

			/// <summary>
			/// Tests if the key-value pair is present in the part of the dictionary being viewed.
			/// </summary>
			/// <param name="key">Key to check for.</param>
			/// <param name="value">Value to check for.</param>
			/// <returns>True if the key-value pair is within this view. </returns>
			public override sealed bool Contains(TKey key, TValue value)
			{
				if (!KeyInView(key))
					return false;
				else
					return myDictionary.Contains(key, value);
			}

			/// <summary>
			/// Adds the given key-value pair to the underlying dictionary of this view.
			/// If <paramref name="key"/> is not within the range of this view, an
			/// ArgumentException is thrown.
			/// </summary>
			/// <param name="key"></param>
			/// <param name="value"></param>
			/// <exception cref="ArgumentException"><paramref name="key"/> is not 
			/// within the range of this view.</exception>
			public override sealed void Add(TKey key, TValue value)
			{
				if (!KeyInView(key))
					throw new ArgumentException(Strings.OutOfViewRange, "key");
				else
					myDictionary.Add(key, value);
			}

			/// <summary>
			/// Removes the key (and associated value) from the underlying dictionary of this view. If
			/// no key in the view is equal to the passed key, the dictionary and view are unchanged.
			/// </summary>
			/// <param name="key">The key to remove.</param>
			/// <returns>True if the key was found and removed. False if the key was not found.</returns>
			public override sealed bool Remove(TKey key)
			{
				if (!KeyInView(key))
					return false;
				else
					return myDictionary.Remove(key);
			}

			/// <summary>
			/// Removes the key and value from the underlying dictionary of this view. that is equal to the passed in key. If
			/// no key in the view is equal to the passed key, or has the given value associated with it, the dictionary and view are unchanged.
			/// </summary>
			/// <param name="key">The key to remove.</param>
			/// <param name="value">The value to remove.</param>
			/// <returns>True if the key-value pair was found and removed. False if the key-value pair was not found.</returns>
			public override sealed bool Remove(TKey key, TValue value)
			{
				if (!KeyInView(key))
					return false;
				else
					return myDictionary.Remove(key, value);
			}

			/// <summary>
			/// Removes all the keys and values within this view from the underlying OrderedMultiDictionary.
			/// </summary>
			/// <example>The following removes all the keys that start with "A" from an OrderedMultiDictionary.
			/// <code>
			/// dictionary.Range("A", "B").Clear();
			/// </code>
			/// </example>
			public override sealed void Clear()
			{
				if (entireTree)
				{
					myDictionary.Clear();
				}
				else
				{
					myDictionary.keyCount -= this.Count;
					myDictionary.tree.DeleteRange(rangeTester);
				}
			}

			/// <summary>
			/// Creates a new View that has the same keys and values as this, in the reversed order.
			/// </summary>
			/// <returns>A new View that has the reversed order of this view.</returns>
			public View Reversed()
			{
				return new View(myDictionary, rangeTester, entireTree, !reversed);
			}

			/// <summary>
			/// Enumerate all the keys in the dictionary. 
			/// </summary>
			/// <returns>An IEnumerator&lt;TKey&gt; that enumerates all of the keys in the collection that
			/// have at least one value associated with them.</returns>
			protected override sealed IEnumerator<TKey> EnumerateKeys()
			{
				return myDictionary.EnumerateKeys(rangeTester, reversed);
			}

			/// <summary>
			/// Enumerate all of the values associated with a given key. If the key exists and has values associated with it, an enumerator for those
			/// values is returned throught <paramref name="values"/>. If the key does not exist, false is returned.
			/// </summary>
			/// <param name="key">The key to get values for.</param>
			/// <param name="values">If true is returned, this parameter receives an enumerators that
			/// enumerates the values associated with that key.</param>
			/// <returns>True if the key exists and has values associated with it. False otherwise.</returns>
			protected override sealed bool TryEnumerateValuesForKey(TKey key, out IEnumerator<TValue> values)
			{
				if (!KeyInView(key))
				{
					values = null;
					return false;
				}
				else
					return myDictionary.TryEnumerateValuesForKey(key, out values);
			}

			/// <summary>
			/// Gets the number of values associated with a given key.
			/// </summary>
			/// <param name="key">The key to count values of.</param>
			/// <returns>The number of values associated with <paramref name="key"/>. If <paramref name="key"/>
			/// is not present in this view, zero is returned.</returns>
			protected override sealed int CountValues(TKey key)
			{
				if (!KeyInView(key))
					return 0;
				else
					return myDictionary.CountValues(key);
			}

			/// <summary>
			/// Determine if the given key lies within the bounds of this view.
			/// </summary>
			/// <param name="key">Key to test.</param>
			/// <returns>True if the key is within the bounds of this view.</returns>
			private bool KeyInView(TKey key)
			{
				return rangeTester(NewPair(key, default(TValue))) == 0;
			}
		}

		///// <summary>
		///// Helper function to create a new KeyValuePair struct with a default value.
		///// </summary>
		///// <param name="key">The key.</param>
		///// <returns>A new KeyValuePair.</returns>
		//private static KeyValuePair<TKey, TValue> NewPair(TKey key)
		//{
		//    KeyValuePair<TKey, TValue> pair = new KeyValuePair<TKey, TValue>(key, default(TValue));
		//    return pair;
		//}

		/// <summary>
		/// Get a RangeTester that maps to the range of all items with the 
		/// given key.
		/// </summary>
		/// <param name="key">Key in the given range.</param>
		/// <returns>A RangeTester delegate that selects the range of items with that range.</returns>
		private RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester KeyRange(TKey key)
		{
			return delegate(KeyValuePair<TKey, TValue> pair)
				{
					return keyComparer.Compare(pair.Key, key);
				};
		}

		/// <summary>
		/// Gets a range tester that defines a range by first and last items.
		/// </summary>
		/// <param name="first">The lower bound.</param>
		/// <param name="firstInclusive">True if the lower bound is inclusive, false if exclusive.</param>
		/// <param name="last">The upper bound.</param>
		/// <param name="lastInclusive">True if the upper bound is inclusive, false if exclusive.</param>
		/// <returns>A RangeTester delegate that tests for a key in the given range.</returns>
		private RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester DoubleBoundedKeyRangeTester(TKey first, bool firstInclusive, TKey last, bool lastInclusive)
		{
			return delegate(KeyValuePair<TKey, TValue> pair)
				{
					if (firstInclusive)
					{
						if (keyComparer.Compare(first, pair.Key) > 0)
							return -1; // item is before first.
					}
					else
					{
						if (keyComparer.Compare(first, pair.Key) >= 0)
							return -1; // item is before or equal to first.
					}

					if (lastInclusive)
					{
						if (keyComparer.Compare(last, pair.Key) < 0)
							return 1; // item is after last.
					}
					else
					{
						if (keyComparer.Compare(last, pair.Key) <= 0)
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
		/// <returns>A RangeTester delegate that tests for a key in the given range.</returns>
		private RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester LowerBoundedKeyRangeTester(TKey first, bool inclusive)
		{
			return delegate(KeyValuePair<TKey, TValue> pair)
				{
					if (inclusive)
					{
						if (keyComparer.Compare(first, pair.Key) > 0)
							return -1; // item is before first.
						else
							return 0; // item is after or equal to first
					}
					else
					{
						if (keyComparer.Compare(first, pair.Key) >= 0)
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
		/// <returns>A RangeTester delegate that tests for a key in the given range.</returns>
		private RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester UpperBoundedKeyRangeTester(TKey last, bool inclusive)
		{
			return delegate(KeyValuePair<TKey, TValue> pair)
				{
					if (inclusive)
					{
						if (keyComparer.Compare(last, pair.Key) < 0)
							return 1; // item is after last.
						else
							return 0; // item is before or equal to last.
					}
					else
					{
						if (keyComparer.Compare(last, pair.Key) <= 0)
							return 1; // item is after or equal to last
						else
							return 0; // item is before last.
					}
				};
		}

		#region Constructors

		/// <summary>
		/// Create a new OrderedMultiDictionary. The default ordering of keys and values are used. If duplicate values
		/// are allowed, multiple copies of the same value can be associated with the same key. For example, the key "foo"
		/// could have "a", "a", and "b" associated with it. If duplicate values are not allowed, only one copies of a given value can
		/// be associated with the same key, although different keys can have the same value. For example, the key "foo" could
		/// have "a" and "b" associated with it, which key "bar" has values "b" and "c" associated with it.
		/// </summary>
		/// <remarks>The default ordering of keys and values will be used, as defined by TKey and TValue's implementation
		/// of IComparable&lt;T&gt; (or IComparable if IComparable&lt;T&gt; is not implemented). If a different ordering should be
		/// used, other constructors allow a custom Comparer or IComparer to be passed to changed the ordering.</remarks>
		/// <param name="allowDuplicateValues">Can the same value be associated with a key multiple times?</param>
		/// <exception cref="InvalidOperationException">TKey or TValue does not implement either IComparable&lt;T&gt; or IComparable.</exception>
		public OrderedMultiDictionary(bool allowDuplicateValues)
			: this(allowDuplicateValues, Comparers.DefaultComparer<TKey>(), Comparers.DefaultComparer<TValue>())
		{
		}

		/// <summary>
		/// Create a new OrderedMultiDictionary. If duplicate values
		/// are allowed, multiple copies of the same value can be associated with the same key. For example, the key "foo"
		/// could have "a", "a", and "b" associated with it. If duplicate values are not allowed, only one copies of a given value can
		/// be associated with the same key, although different keys can have the same value. For example, the key "foo" could
		/// have "a" and "b" associated with it, which key "bar" has values "b" and "c" associated with it.
		/// </summary>
		/// <param name="allowDuplicateValues">Can the same value be associated with a key multiple times?</param>
		/// <param name="keyComparison">A delegate to a method that will be used to compare keys.</param>
		/// <exception cref="InvalidOperationException">TValue does not implement either IComparable&lt;TValue&gt; or IComparable.</exception>
		public OrderedMultiDictionary(bool allowDuplicateValues, Comparison<TKey> keyComparison)
			: this(allowDuplicateValues, Comparers.ComparerFromComparison(keyComparison), Comparers.DefaultComparer<TValue>())
		{
		}

		/// <summary>
		/// Create a new OrderedMultiDictionary. If duplicate values
		/// are allowed, multiple copies of the same value can be associated with the same key. For example, the key "foo"
		/// could have "a", "a", and "b" associated with it. If duplicate values are not allowed, only one copies of a given value can
		/// be associated with the same key, although different keys can have the same value. For example, the key "foo" could
		/// have "a" and "b" associated with it, which key "bar" has values "b" and "c" associated with it.
		/// </summary>
		/// <param name="allowDuplicateValues">Can the same value be associated with a key multiple times?</param>
		/// <param name="keyComparison">A delegate to a method that will be used to compare keys.</param>
		/// <param name="valueComparison">A delegate to a method that will be used to compare values.</param>
		public OrderedMultiDictionary(bool allowDuplicateValues, Comparison<TKey> keyComparison, Comparison<TValue> valueComparison)
			: this(allowDuplicateValues, Comparers.ComparerFromComparison(keyComparison), Comparers.ComparerFromComparison(valueComparison))
		{
		}

		/// <summary>
		/// Create a new OrderedMultiDictionary. If duplicate values
		/// are allowed, multiple copies of the same value can be associated with the same key. For example, the key "foo"
		/// could have "a", "a", and "b" associated with it. If duplicate values are not allowed, only one copies of a given value can
		/// be associated with the same key, although different keys can have the same value. For example, the key "foo" could
		/// have "a" and "b" associated with it, which key "bar" has values "b" and "c" associated with it.
		/// </summary>
		/// <param name="allowDuplicateValues">Can the same value be associated with a key multiple times?</param>
		/// <param name="keyComparer">An IComparer&lt;TKey&gt; instance that will be used to compare keys.</param>
		/// <exception cref="InvalidOperationException">TValue does not implement either IComparable&lt;TValue&gt; or IComparable.</exception>
		public OrderedMultiDictionary(bool allowDuplicateValues, IComparer<TKey> keyComparer)
			: this(allowDuplicateValues, keyComparer, Comparers.DefaultComparer<TValue>())
		{
		}

		/// <summary>
		/// Create a new OrderedMultiDictionary. If duplicate values
		/// are allowed, multiple copies of the same value can be associated with the same key. For example, the key "foo"
		/// could have "a", "a", and "b" associated with it. If duplicate values are not allowed, only one copies of a given value can
		/// be associated with the same key, although different keys can have the same value. For example, the key "foo" could
		/// have "a" and "b" associated with it, which key "bar" has values "b" and "c" associated with it.
		/// </summary>
		/// <param name="allowDuplicateValues">Can the same value be associated with a key multiple times?</param>
		/// <param name="keyComparer">An IComparer&lt;TKey&gt; instance that will be used to compare keys.</param>
		/// <param name="valueComparer">An IComparer&lt;TValue&gt; instance that will be used to compare values.</param>
		public OrderedMultiDictionary(bool allowDuplicateValues, IComparer<TKey> keyComparer, IComparer<TValue> valueComparer)
		{
			if (keyComparer == null)
				throw new ArgumentNullException("keyComparer");
			if (valueComparer == null)
				throw new ArgumentNullException("valueComparer");

			this.allowDuplicateValues = allowDuplicateValues;
			this.keyComparer = keyComparer;
			this.valueComparer = valueComparer;
			this.comparer = Comparers.ComparerPairFromKeyValueComparers(keyComparer, valueComparer);
			this.tree = new RedBlackTree<KeyValuePair<TKey, TValue>>(this.comparer);
		}

		/// <summary>
		/// Create a new OrderedMultiDictionary. Used internally for cloning.
		/// </summary>
		/// <param name="allowDuplicateValues">Can the same value be associated with a key multiple times?</param>
		/// <param name="keyCount">Number of keys.</param>
		/// <param name="keyComparer">An IComparer&lt;TKey&gt; instance that will be used to compare keys.</param>
		/// <param name="valueComparer">An IComparer&lt;TValue&gt; instance that will be used to compare values.</param>
		/// <param name="comparer">Comparer of key-value pairs.</param>
		/// <param name="tree">The red-black tree used to store the data.</param>
		private OrderedMultiDictionary(bool allowDuplicateValues, int keyCount, IComparer<TKey> keyComparer, IComparer<TValue> valueComparer, IComparer<KeyValuePair<TKey, TValue>> comparer, RedBlackTree<KeyValuePair<TKey, TValue>> tree)
		{
			this.allowDuplicateValues = allowDuplicateValues;
			this.keyCount = keyCount;
			this.keyComparer = keyComparer;
			this.valueComparer = valueComparer;
			this.comparer = comparer;
			this.tree = tree;
		}

		#endregion Constructors

		#region Add or remove items

		/// <summary>
		/// <para>Adds a new value to be associated with a key. If duplicate values are permitted, this
		/// method always adds a new key-value pair to the dictionary.</para>
		/// <para>If duplicate values are not permitted, and <paramref name="key"/> already has a value
		/// equal to <paramref name="value"/> associated with it, then that value is replaced with <paramref name="value"/>,
		/// and the number of values associate with <paramref name="key"/> is unchanged.</para>
		/// </summary>
		/// <param name="key">The key to associate with.</param>
		/// <param name="value">The value to associated with <paramref name="key"/>.</param>
		public override sealed void Add(TKey key, TValue value)
		{
			KeyValuePair<TKey, TValue> pair = NewPair(key, value);
			KeyValuePair<TKey, TValue> dummy;

			if (!ContainsKey(key))
				++keyCount;

			tree.Insert(pair, allowDuplicateValues ? DuplicatePolicy.InsertLast : DuplicatePolicy.ReplaceLast, out dummy);
		}

		/// <summary>
		/// Removes a given value from the values associated with a key. If the
		/// last value is removed from a key, the key is removed also.
		/// </summary>
		/// <param name="key">A key to remove a value from.</param>
		/// <param name="value">The value to remove.</param>
		/// <returns>True if <paramref name="value"/> was associated with <paramref name="key"/> (and was
		/// therefore removed). False if <paramref name="value"/> was not associated with <paramref name="key"/>.</returns>
		public override sealed bool Remove(TKey key, TValue value)
		{
			KeyValuePair<TKey, TValue> dummy;
			bool found = tree.Delete(NewPair(key, value), false, out dummy);
			if (found && !ContainsKey(key))
				--keyCount; // Removed the last value associated with the key.
			return found;
		}

		/// <summary>
		/// Removes a key and all associated values from the dictionary. If the
		/// key is not present in the dictionary, it is unchanged and false is returned.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <returns>True if the key was present and was removed. Returns 
		/// false if the key was not present.</returns>
		public override sealed bool Remove(TKey key)
		{
			if (tree.DeleteRange(KeyRange(key)) > 0)
			{
				--keyCount;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Removes all keys and values from the dictionary.
		/// </summary>
		public override sealed void Clear()
		{
			tree.StopEnumerations(); // Invalidate any enumerations.

			// The simplest and fastest way is simply to throw away the old tree and create a new one.
			tree = new RedBlackTree<KeyValuePair<TKey, TValue>>(comparer);
			keyCount = 0;
		}

		#endregion Add or remove items

		#region Query items

		/// <summary>
		/// Returns the IComparer&lt;T&gt; used to compare keys in this dictionary. 
		/// </summary>
		/// <value>If the dictionary was created using a comparer, that comparer is returned. If the dictionary was
		/// created using a comparison delegate, then a comparer equivalent to that delegate
		/// is returned. Otherwise
		/// the default comparer for TKey (Comparer&lt;TKey&gt;.Default) is returned.</value>
		public IComparer<TKey> KeyComparer
		{
			get { return this.keyComparer; }
		}

		/// <summary>
		/// Returns the IComparer&lt;T&gt; used to compare values in this dictionary. 
		/// </summary>
		/// <value>If the dictionary was created using a comparer, that comparer is returned. If the dictionary was
		/// created using a comparison delegate, then a comparer equivalent to that delegate
		/// is returned. Otherwise
		/// the default comparer for TValue (Comparer&lt;TValue&gt;.Default) is returned.</value>
		public IComparer<TValue> ValueComparer
		{
			get { return this.valueComparer; }
		}

		/// <summary>
		/// Gets the number of key-value pairs in the dictionary. Each value associated
		/// with a given key is counted. If duplicate values are permitted, each duplicate
		/// value is included in the count.
		/// </summary>
		/// <value>The number of key-value pairs in the dictionary.</value>
		public override sealed int Count
		{
			get { return keyCount; }
		}

		/// <summary>
		/// Checks to see if <paramref name="value"/> is associated with <paramref name="key"/>
		/// in the dictionary.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <param name="value">The value to check.</param>
		/// <returns>True if <paramref name="value"/> is associated with <paramref name="key"/>.</returns>
		public override sealed bool Contains(TKey key, TValue value)
		{
			KeyValuePair<TKey, TValue> dummy;
			return tree.Find(NewPair(key, value), true, false, out dummy);
		}

		/// <summary>
		/// Checks to see if the key is present in the dictionary and has
		/// at least one value associated with it.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <returns>True if <paramref name="key"/> is present and has at least
		/// one value associated with it. Returns false otherwise.</returns>
		public override sealed bool ContainsKey(TKey key)
		{
			KeyValuePair<TKey, TValue> dummy;
			return (tree.FirstItemInRange(KeyRange(key), out dummy) >= 0);
		}

		/// <summary>
		/// Determine if two values are equal.
		/// </summary>
		/// <param name="value1">First value to compare.</param>
		/// <param name="value2">Second value to compare.</param>
		/// <returns>True if the values are equal.</returns>
		protected override sealed bool EqualValues(TValue value1, TValue value2)
		{
			return valueComparer.Compare(value1, value2) == 0;
		}

		/// <summary>
		/// Determines if this dictionary contains a key equal to <paramref name="key"/>. If so, all the values
		/// associated with that key are returned through the values parameter. 
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="values">Returns all values associated with key, if true was returned.</param>
		/// <returns>True if the dictionary contains key. False if the dictionary does not contain key.</returns>
		protected override sealed bool TryEnumerateValuesForKey(TKey key, out IEnumerator<TValue> values)
		{
			// CONSIDER: It would be nice to eliminate the double lookup here, but there doesn't seem to be an easy way.
			if (ContainsKey(key))
			{
				values = EnumerateValuesForKey(key);
				return true;
			}
			else
			{
				values = null;
				return false;
			}
		}

		/// <summary>
		/// Enumerate all of the keys in the dictionary.
		/// </summary>
		/// <returns>An IEnumerator&lt;TKey&gt; of all of the keys in the dictionary.</returns>
		protected override sealed IEnumerator<TKey> EnumerateKeys()
		{
			return EnumerateKeys(tree.EntireRangeTester, false);
		}

		/// <summary>
		/// Gets the number of values associated with a given key.
		/// </summary>
		/// <param name="key">The key to count values of.</param>
		/// <returns>The number of values associated with <paramref name="key"/>. If <paramref name="key"/>
		/// is not present in the dictionary, zero is returned.</returns>
		protected override sealed int CountValues(TKey key)
		{
			return tree.CountRange(KeyRange(key));
		}

		/// <summary>
		/// Gets a total count of values in the collection. 
		/// </summary>
		/// <returns>The total number of values associated with all keys in the dictionary.</returns>
		protected override sealed int CountAllValues()
		{
			return tree.ElementCount;
		}

		/// <summary>
		/// A private helper method that returns an enumerable that
		/// enumerates all the keys in a range.
		/// </summary>
		/// <param name="rangeTester">Defines the range to enumerate.</param>
		/// <param name="reversed">Should the keys be enumerated in reverse order?</param>
		/// <returns>An IEnumerable&lt;TKey&gt; that enumerates the keys in the given range.
		/// in the dictionary.</returns>
		private IEnumerator<TKey> EnumerateKeys(RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester rangeTester, bool reversed)
		{
			bool isFirst = true;
			TKey lastKey = default(TKey);

			IEnumerable<KeyValuePair<TKey, TValue>> pairs;

			if (reversed)
				pairs = tree.EnumerateRangeReversed(rangeTester);
			else
				pairs = tree.EnumerateRange(rangeTester);

			// Enumerate pairs; yield a new key when the key changes.
			foreach (KeyValuePair<TKey, TValue> pair in pairs)
			{
				if (isFirst || keyComparer.Compare(lastKey, pair.Key) != 0)
				{
					lastKey = pair.Key;
					yield return lastKey;
				}

				isFirst = false;
			}
		}

		/// <summary>
		/// A private helper method for the indexer to return an enumerable that
		/// enumerates all the values for a key. This is separate method because indexers
		/// can't use the yield return construct.
		/// </summary>
		/// <param name="key"></param>
		/// <returns>An IEnumerable&lt;TValue&gt; that can be used to enumerate all the
		/// values associated with <paramref name="key"/>. If <paramref name="key"/> is not present,
		/// an enumerable that enumerates no items is returned.</returns>
		private IEnumerator<TValue> EnumerateValuesForKey(TKey key)
		{
			foreach (KeyValuePair<TKey, TValue> pair in tree.EnumerateRange(KeyRange(key)))
				yield return pair.Value;
		}

		#endregion Query items

		#region Cloning

		/// <summary>
		/// Implements ICloneable.Clone. Makes a shallow clone of this dictionary; i.e., if keys or values are reference types, then they are not cloned.
		/// </summary>
		/// <returns>The cloned dictionary.</returns>
		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Makes a shallow clone of this dictionary; i.e., if keys or values of the
		/// dictionary are reference types, then they are not cloned. If TKey or TValue is a value type,
		/// then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks>Cloning the dictionary takes time O(N), where N is the number of key-value pairs in the dictionary.</remarks>
		/// <returns>The cloned dictionary.</returns>
		public OrderedMultiDictionary<TKey, TValue> Clone()
		{
			OrderedMultiDictionary<TKey, TValue> newDict = new OrderedMultiDictionary<TKey, TValue>(allowDuplicateValues, keyCount, keyComparer, valueComparer, comparer, tree.Clone());
			return newDict;
		}

		/// <summary>
		/// Makes a deep clone of this dictionary. A new dictionary is created with a clone of
		/// each entry of this dictionary, by calling ICloneable.Clone on each element. If TKey or TValue is
		/// a value type, then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks><para>If TKey or TValue is a reference type, it must implement
		/// ICloneable. Otherwise, an InvalidOperationException is thrown.</para>
		/// <para>Cloning the dictionary takes time O(N log N), where N is the number of key-value pairs in the dictionary.</para></remarks>
		/// <returns>The cloned dictionary.</returns>
		/// <exception cref="InvalidOperationException">TKey or TValue is a reference type that does not implement ICloneable.</exception>
		public OrderedMultiDictionary<TKey, TValue> CloneContents()
		{
			bool keyIsValueType, valueIsValueType;

			// Make sure that TKey and TValue can be cloned.
			if (!Util.IsCloneableType(typeof (TKey), out keyIsValueType))
				NonCloneableType(typeof (TKey));

			if (!Util.IsCloneableType(typeof (TValue), out valueIsValueType))
				NonCloneableType(typeof (TValue));

			OrderedMultiDictionary<TKey, TValue> newDict = new OrderedMultiDictionary<TKey, TValue>(allowDuplicateValues, keyComparer, valueComparer);

			foreach (KeyValuePair<TKey, TValue> pair in tree)
			{
				// Clone the key and value parts of the pair. Value types can be cloned
				// by just copying them, otherwise, ICloneable is used.
				TKey keyClone;
				TValue valueClone;

				if (keyIsValueType)
					keyClone = pair.Key;
				else
				{
					if (pair.Key == null)
						keyClone = default(TKey); // Really null, because we know TKey isn't a value type.
					else
						keyClone = (TKey) (((ICloneable) pair.Key).Clone());
				}

				if (valueIsValueType)
					valueClone = pair.Value;
				else
				{
					if (pair.Value == null)
						valueClone = default(TValue); // Really null, because we know TKey isn't a value type.
					else
						valueClone = (TValue) (((ICloneable) pair.Value).Clone());
				}

				newDict.Add(keyClone, valueClone);
			}

			return newDict;
		}

		/// <summary>
		/// Throw an InvalidOperationException indicating that this type is not cloneable.
		/// </summary>
		/// <param name="t">Type to test.</param>
		private static void NonCloneableType(Type t)
		{
			throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, t.FullName));
		}

		#endregion Cloning

		#region KeyValuePairsCollection

		/// <summary>
		/// Gets a read-only collection of all key-value pairs in the dictionary. If a key has multiple
		/// values associated with it, then a key-value pair is present for each value associated
		/// with the key.
		/// </summary>
		public override sealed ICollection<KeyValuePair<TKey, TValue>> KeyValuePairs
		{
			get { return new KeyValuePairsCollection(this); }
		}

		/// <summary>
		/// A private class that implements ICollection&lt;KeyValuePair&lt;TKey,TValue&gt;&gt; and ICollection for the
		/// KeyValuePairs collection. The collection is read-only.
		/// </summary>
		[Serializable]
		private sealed class KeyValuePairsCollection : ReadOnlyCollectionBase<KeyValuePair<TKey, TValue>>
		{
			private readonly OrderedMultiDictionary<TKey, TValue> myDictionary;

			public KeyValuePairsCollection(OrderedMultiDictionary<TKey, TValue> myDictionary)
			{
				this.myDictionary = myDictionary;
			}

			public override int Count
			{
				get { return myDictionary.CountAllValues(); }
			}

			public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			{
				return myDictionary.tree.GetEnumerator();
			}

			public override bool Contains(KeyValuePair<TKey, TValue> pair)
			{
				KeyValuePair<TKey, TValue> dummy;
				return myDictionary.tree.Find(pair, true, false, out dummy);
			}
		}

		#endregion KeyValuePairs collection

		#region Views

		/// <summary>
		/// Returns a View collection that can be used for enumerating the keys and values in the collection in 
		/// reversed order.
		/// </summary>
		///<remarks>
		///<p>Typically, this method is used in conjunction with a foreach statement. For example:
		///<code>
		/// foreach(KeyValuePair&lt;TKey, TValue&gt; pair in dictionary.Reversed()) {
		///    // process pair
		/// }
		///</code></p>
		/// <p>If an entry is added to or deleted from the dictionary while the View is being enumerated, then 
		/// the enumeration will end with an InvalidOperationException.</p>
		///<p>Calling Reverse does not copy the data in the dictionary, and the operation takes constant time.</p>
		///</remarks>
		/// <returns>An OrderedDictionary.View of key-value pairs in reverse order.</returns>
		public View Reversed()
		{
			return new View(this, tree.EntireRangeTester, true, true);
		}

		/// <summary>
		/// Returns a collection that can be used for enumerating some of the keys and values in the collection. 
		/// Only keys that are greater than <paramref name="from"/> and 
		/// less than <paramref name="to"/> are included. The keys are enumerated in sorted order.
		/// Keys equal to the end points of the range can be included or excluded depending on the
		/// <paramref name="fromInclusive"/> and <paramref name="toInclusive"/> parameters.
		/// </summary>
		///<remarks>
		///<p>If <paramref name="from"/> is greater than or equal to <paramref name="to"/>, the returned collection is empty. </p>
		///<p>The sorted order of the keys is determined by the comparison instance or delegate used
		/// to create the dictionary.</p>
		///<p>Typically, this property is used in conjunction with a foreach statement. For example:</p>
		///<code>
		/// foreach(KeyValuePair&lt;TKey, TValue&gt; pair in dictionary.Range(from, true, to, false)) {
		///    // process pair
		/// }
		///</code>
		///<p>Calling Range does not copy the data in the dictionary, and the operation takes constant time.</p></remarks>
		/// <param name="from">The lower bound of the range.</param>
		/// <param name="fromInclusive">If true, the lower bound is inclusive--keys equal to the lower bound will
		/// be included in the range. If false, the lower bound is exclusive--keys equal to the lower bound will not
		/// be included in the range.</param>
		/// <param name="to">The upper bound of the range. </param>
		/// <param name="toInclusive">If true, the upper bound is inclusive--keys equal to the upper bound will
		/// be included in the range. If false, the upper bound is exclusive--keys equal to the upper bound will not
		/// be included in the range.</param>
		/// <returns>An OrderedMultiDictionary.View of key-value pairs in the given range.</returns>
		public View Range(TKey from, bool fromInclusive, TKey to, bool toInclusive)
		{
			return new View(this, DoubleBoundedKeyRangeTester(from, fromInclusive, to, toInclusive), false, false);
		}


		/// <summary>
		/// Returns a collection that can be used for enumerating some of the keys and values in the collection. 
		/// Only keys that are greater than (and optionally, equal to) <paramref name="from"/> are included. 
		/// The keys are enumerated in sorted order. Keys equal to <paramref name="from"/> can be included
		/// or excluded depending on the <paramref name="fromInclusive"/> parameter.
		/// </summary>
		///<remarks>
		///<p>The sorted order of the keys is determined by the comparison instance or delegate used
		/// to create the dictionary.</p>
		///<p>Typically, this property is used in conjunction with a foreach statement. For example:</p>
		///<code>
		/// foreach(KeyValuePair&lt;TKey, TValue&gt; pair in dictionary.RangeFrom(from, true)) {
		///    // process pair
		/// }
		///</code>
		///<p>Calling RangeFrom does not copy of the data in the dictionary, and the operation takes constant time.</p>
		///</remarks>
		/// <param name="from">The lower bound of the range.</param>
		/// <param name="fromInclusive">If true, the lower bound is inclusive--keys equal to the lower bound will
		/// be included in the range. If false, the lower bound is exclusive--keys equal to the lower bound will not
		/// be included in the range.</param>
		/// <returns>An OrderedMultiDictionary.View of key-value pairs in the given range.</returns>
		public View RangeFrom(TKey from, bool fromInclusive)
		{
			return new View(this, LowerBoundedKeyRangeTester(from, fromInclusive), false, false);
		}

		/// <summary>
		/// Returns a collection that can be used for enumerating some of the keys and values in the collection. 
		/// Only items that are less than (and optionally, equal to) <paramref name="to"/> are included. 
		/// The items are enumerated in sorted order. Items equal to <paramref name="to"/> can be included
		/// or excluded depending on the <paramref name="toInclusive"/> parameter.
		/// </summary>
		///<remarks>
		///<p>The sorted order of the keys is determined by the comparison instance or delegate used
		/// to create the dictionary.</p>
		///<p>Typically, this property is used in conjunction with a foreach statement. For example:</p>
		///<code>
		/// foreach(KeyValuePair&lt;TKey, TValue&gt; pair in dictionary.RangeFrom(from, false)) {
		///    // process pair
		/// }
		///</code>
		///<p>Calling RangeTo does not copy the data in the dictionary, and the operation takes constant time.</p>
		///</remarks>
		/// <param name="to">The upper bound of the range. </param>
		/// <param name="toInclusive">If true, the upper bound is inclusive--keys equal to the upper bound will
		/// be included in the range. If false, the upper bound is exclusive--keys equal to the upper bound will not
		/// be included in the range.</param>
		/// <returns>An OrderedMultiDictionary.View of key-value pairs in the given range.</returns>
		public View RangeTo(TKey to, bool toInclusive)
		{
			return new View(this, UpperBoundedKeyRangeTester(to, toInclusive), false, false);
		}

		#endregion Views

		/// <summary>
		/// Helper function to create a new KeyValuePair struct.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>A new KeyValuePair.</returns>
		private static KeyValuePair<TKey, TValue> NewPair(TKey key, TValue value)
		{
			KeyValuePair<TKey, TValue> pair = new KeyValuePair<TKey, TValue>(key, value);
			return pair;
		}
	}
}