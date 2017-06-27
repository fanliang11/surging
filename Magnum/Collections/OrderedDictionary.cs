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
	/// OrderedDictionary&lt;TKey, TValue&gt; is a collection that maps keys of type TKey
	/// to values of type TValue. The keys are maintained in a sorted order, and at most one value
	/// is permitted for each key.
	/// </summary>
	/// <remarks>
	/// <p>The keys are compared in one of three ways. If TKey implements IComparable&lt;TKey&gt; or IComparable,
	/// then the CompareTo method of that interface will be used to compare elements. Alternatively, a comparison
	/// function can be passed in either as a delegate, or as an instance of IComparer&lt;TKey&gt;.</p>
	/// <p>OrderedDictionary is implemented as a balanced binary tree. Inserting, deleting, and looking up an
	/// an element all are done in log(N) type, where N is the number of keys in the tree.</p>
	/// <p><see cref="Dictionary{TKey,TValue}"/> is similar, but uses hashing instead of comparison, and does not maintain
	/// the keys in sorted order.</p>
	///</remarks>
	///<seealso cref="Dictionary&lt;TKey,TValue&gt;"/>
	[Serializable]
	public class OrderedDictionary<TKey, TValue> : DictionaryBase<TKey, TValue>,
		ICloneable
	{
		// The comparer for comparing keys. This is saved to return from the Comparer property,
		// but is otherwise not used.
		private readonly IComparer<TKey> keyComparer;

		// The comparer for comparing key-value pairs.
		private IComparer<KeyValuePair<TKey, TValue>> pairComparer;

		private RedBlackTree<KeyValuePair<TKey, TValue>> tree;

		/// <summary>
		/// Creates a new OrderedDictionary. The TKey must implemented IComparable&lt;TKey&gt;
		/// or IComparable. 
		/// The CompareTo method of this interface will be used to compare keys in this dictionary.
		/// </summary>
		/// <exception cref="InvalidOperationException">TKey does not implement IComparable&lt;TKey&gt;.</exception>
		public OrderedDictionary()
			:
				this(Comparers.DefaultComparer<TKey>())
		{
		}

		/// <summary>
		/// Creates a new OrderedDictionary. The Compare method of the passed comparison object
		/// will be used to compare keys in this dictionary.
		/// </summary>
		/// <remarks>
		/// The GetHashCode and Equals methods of the provided IComparer&lt;TKey&gt; will never
		/// be called, and need not be implemented.</remarks>
		/// <param name="comparer">An instance of IComparer&lt;TKey&gt; that will be used to compare keys.</param>
		public OrderedDictionary(IComparer<TKey> comparer)
			:
				this(null, comparer, Comparers.ComparerKeyValueFromComparerKey<TKey, TValue>(comparer))
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
		}

		/// <summary>
		/// Creates a new OrderedDictionary. The passed delegate will be used to compare keys in this dictionary.
		/// </summary>
		/// <param name="comparison">A delegate to a method that will be used to compare keys.</param>
		public OrderedDictionary(Comparison<TKey> comparison)
			:
				this(null, Comparers.ComparerFromComparison(comparison), Comparers.ComparerKeyValueFromComparisonKey<TKey, TValue>(comparison))
		{
		}

		/// <summary>
		/// <para>Creates a new OrderedDictionary. The TKey must implemented IComparable&lt;TKey&gt;
		/// or IComparable. 
		/// The CompareTo method of this interface will be used to compare keys in this dictionary.</para>
		/// <para>A collection and keys and values (typically another dictionary) is used to initialized the 
		/// contents of the dictionary.</para>
		/// </summary>
		/// <param name="keysAndValues">A collection of keys and values whose contents are used to initialized the dictionary.</param>
		/// <exception cref="InvalidOperationException">TKey does not implement IComparable&lt;TKey&gt;.</exception>
		public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keysAndValues)
			: this(keysAndValues, Comparers.DefaultComparer<TKey>())
		{
		}

		/// <summary>
		/// <para>Creates a new OrderedDictionary. The Compare method of the passed comparison object
		/// will be used to compare keys in this dictionary.</para>
		/// <para>A collection and keys and values (typically another dictionary) is used to initialized the 
		/// contents of the dictionary.</para>
		/// </summary>
		/// <remarks>
		/// The GetHashCode and Equals methods of the provided IComparer&lt;TKey&gt; will never
		/// be called, and need not be implemented.</remarks>
		/// <param name="keysAndValues">A collection of keys and values whose contents are used to initialized the dictionary.</param>
		/// <param name="comparer">An instance of IComparer&lt;TKey&gt; that will be used to compare keys.</param>
		public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keysAndValues, IComparer<TKey> comparer)
			: this(keysAndValues, comparer, Comparers.ComparerKeyValueFromComparerKey<TKey, TValue>(comparer))
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
		}

		/// <summary>
		/// <para>Creates a new OrderedDictionary. The passed delegate will be used to compare keys in this dictionary.</para>
		/// <para>A collection and keys and values (typically another dictionary) is used to initialized the 
		/// contents of the dictionary.</para>
		/// </summary>
		/// <param name="keysAndValues">A collection of keys and values whose contents are used to initialized the dictionary.</param>
		/// <param name="comparison">A delegate to a method that will be used to compare keys.</param>
		public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keysAndValues, Comparison<TKey> comparison)
			: this(keysAndValues, Comparers.ComparerFromComparison(comparison), Comparers.ComparerKeyValueFromComparisonKey<TKey, TValue>(comparison))
		{
		}

		/// <summary>
		/// Creates a new OrderedDictionary. The passed comparer 
		/// will be used to compare key-value pairs in this dictionary. Used internally  
		/// from other constructors.
		/// </summary>
		/// <param name="keysAndValues">A collection of keys and values whose contents are used to initialized the dictionary.</param>
		/// <param name="keyComparer">An IComparer that will be used to compare keys.</param>
		/// <param name="pairComparer">An IComparer that will be used to compare key-value pairs.</param>
		private OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keysAndValues, IComparer<TKey> keyComparer, IComparer<KeyValuePair<TKey, TValue>> pairComparer)
		{
			this.keyComparer = keyComparer;
			this.pairComparer = pairComparer;
			tree = new RedBlackTree<KeyValuePair<TKey, TValue>>(this.pairComparer);

			if (keysAndValues != null)
				AddMany(keysAndValues);
		}

		/// <summary>
		/// Creates a new OrderedDictionary. The passed comparison delegate 
		/// will be used to compare keys in this dictionary, and the given tree is used. Used internally for Clone().
		/// </summary>
		/// <param name="keyComparer">An IComparer that will be used to compare keys.</param>
		/// <param name="pairComparer">A delegate to a method that will be used to compare key-value pairs.</param>
		/// <param name="tree">RedBlackTree that contains the data for the dictionary.</param>
		private OrderedDictionary(IComparer<TKey> keyComparer, IComparer<KeyValuePair<TKey, TValue>> pairComparer, RedBlackTree<KeyValuePair<TKey, TValue>> tree)
		{
			this.keyComparer = keyComparer;
			this.pairComparer = pairComparer;
			this.tree = tree;
		}

		/// <summary>
		/// Returns the IComparer&lt;T&gt; used to compare keys in this dictionary. 
		/// </summary>
		/// <value>If the dictionary was created using a comparer, that comparer is returned. If the dictionary was
		/// created using a comparison delegate, then a comparer equivalent to that delegate
		/// is returned. Otherwise
		/// the default comparer for TKey (Comparer&lt;TKey&gt;.Default) is returned.</value>
		public IComparer<TKey> Comparer
		{
			get { return this.keyComparer; }
		}

		/// <summary>
		/// Makes a shallow clone of this dictionary; i.e., if keys or values of the
		/// dictionary are reference types, then they are not cloned. If TKey or TValue is a value type,
		/// then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks>Cloning the dictionary takes time O(N), where N is the number of keys in the dictionary.</remarks>
		/// <returns>The cloned dictionary.</returns>
		public OrderedDictionary<TKey, TValue> Clone()
		{
			OrderedDictionary<TKey, TValue> newDict = new OrderedDictionary<TKey, TValue>(keyComparer, pairComparer, tree.Clone());
			return newDict;
		}

		/// <summary>
		/// Makes a deep clone of this dictionary. A new dictionary is created with a clone of
		/// each entry of this dictionary, by calling ICloneable.Clone on each element. If TKey or TValue is
		/// a value type, then each element is copied as if by simple assignment.
		/// </summary>
		/// <remarks><para>If TKey or TValue is a reference type, it must implement
		/// ICloneable. Otherwise, an InvalidOperationException is thrown.</para>
		/// <para>Cloning the dictionary takes time O(N log N), where N is the number of keys in the dictionary.</para></remarks>
		/// <returns>The cloned dictionary.</returns>
		/// <exception cref="InvalidOperationException">TKey or TValue is a reference type that does not implement ICloneable.</exception>
		public OrderedDictionary<TKey, TValue> CloneContents()
		{
			bool keyIsValueType, valueIsValueType;

			// Make sure that TKey and TValue can be cloned.
			if (!Util.IsCloneableType(typeof (TKey), out keyIsValueType))
				NonCloneableType(typeof (TKey));

			if (!Util.IsCloneableType(typeof (TValue), out valueIsValueType))
				NonCloneableType(typeof (TValue));

			OrderedDictionary<TKey, TValue> newDict = new OrderedDictionary<TKey, TValue>(null, keyComparer, pairComparer);

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
		/// <returns>An OrderedDictionary.View of key-value pairs in the given range.</returns>
		public View Range(TKey from, bool fromInclusive, TKey to, bool toInclusive)
		{
			return new View(this, tree.DoubleBoundedRangeTester(NewPair(from), fromInclusive, NewPair(to), toInclusive), false, false);
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
		/// <returns>An OrderedDictionary.View of key-value pairs in the given range.</returns>
		public View RangeFrom(TKey from, bool fromInclusive)
		{
			return new View(this, tree.LowerBoundedRangeTester(NewPair(from), fromInclusive), false, false);
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
		/// <returns>An OrderedDictionary.View of key-value pairs in the given range.</returns>
		public View RangeTo(TKey to, bool toInclusive)
		{
			return new View(this, tree.UpperBoundedRangeTester(NewPair(to), toInclusive), false, false);
		}

		/// <summary>
		/// The OrderedDictionary&lt;TKey,TValue&gt;.View class is used to look at a subset of the keys and values
		/// inside an ordered dictionary. It is returned from the Range, RangeTo, RangeFrom, and Reversed methods. 
		/// </summary>
		///<remarks>
		/// <p>Views are dynamic. If the underlying dictionary changes, the view changes in sync. If a change is made
		/// to the view, the underlying dictionary changes accordingly.</p>
		///<p>Typically, this class is used in conjunction with a foreach statement to enumerate the keys
		/// and values in a subset of the OrderedDictionary. For example:</p>
		///<code>
		/// foreach(KeyValuePair&lt;TKey, TValue&gt; pair in dictionary.Range(from, to)) {
		///    // process pair
		/// }
		///</code>
		///</remarks>
		[Serializable]
		public class View : DictionaryBase<TKey, TValue>
		{
			private readonly bool entireTree; // is the view the whole tree?
			private readonly OrderedDictionary<TKey, TValue> myDictionary;
			private readonly RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester rangeTester; // range tester for the range being used.
			private readonly bool reversed; // is the view reversed?

			/// <summary>
			/// Initialize the View.
			/// </summary>
			/// <param name="myDictionary">Associated OrderedDictionary to be viewed.</param>
			/// <param name="rangeTester">Range tester that defines the range being used.</param>
			/// <param name="entireTree">If true, then rangeTester defines the entire tree.</param>
			/// <param name="reversed">Is the view enuemerated in reverse order?</param>
			internal View(OrderedDictionary<TKey, TValue> myDictionary, RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester rangeTester, bool entireTree, bool reversed)
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
						return myDictionary.tree.CountRange(rangeTester);
				}
			}

			/// <summary>
			/// Gets or sets the value associated with a given key. When getting a value, if this
			/// key is not found in the collection, then an ArgumentException is thrown. When setting
			/// a value, the value replaces any existing value in the dictionary. When setting a value, the 
			/// key must be within the range of keys being viewed.
			/// </summary>
			/// <value>The value associated with the key.</value>
			/// <exception cref="ArgumentException">A value is being retrieved, and the key is not present in the dictionary, 
			/// or a value is being set, and the key is outside the range of keys being viewed by this View.</exception>
			public override sealed TValue this[TKey key]
			{
				get // technically we don't need to override this, but fixes a bug in NDOC.
				{ return base[key]; }
				set
				{
					if (!KeyInView(key))
						throw new ArgumentException(Strings.OutOfViewRange, "key");
					else
						myDictionary[key] = value;
				}
			}

			/// <summary>
			/// Enumerate all the keys and values in this view.
			/// </summary>
			/// <returns>An IEnumerator of KeyValuePairs with the keys and views in this view.</returns>
			public override sealed IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			{
				if (reversed)
					return myDictionary.tree.EnumerateRangeReversed(rangeTester).GetEnumerator();
				else
					return myDictionary.tree.EnumerateRange(rangeTester).GetEnumerator();
			}

			/// <summary>
			/// Tests if the key is present in the part of the dictionary being viewed.
			/// </summary>
			/// <param name="key">Key to check for.</param>
			/// <returns>True if the key is within this view. </returns>
			public override sealed bool ContainsKey(TKey key)
			{
				if (!KeyInView(key))
					return false;
				else
					return myDictionary.ContainsKey(key);
			}

			/// <summary>
			/// Determines if this view contains a key equal to <paramref name="key"/>. If so, the value
			/// associated with that key is returned through the value parameter. 
			/// </summary>
			/// <param name="key">The key to search for.</param>
			/// <param name="value">Returns the value associated with key, if true was returned.</param>
			/// <returns>True if the key is within this view. </returns>
			public override sealed bool TryGetValue(TKey key, out TValue value)
			{
				if (!KeyInView(key))
				{
					value = default(TValue);
					return false;
				}
				else
				{
					return myDictionary.TryGetValue(key, out value);
				}
			}

			/// <summary>
			/// Removes the key (and associated value) from the underlying dictionary of this view. that is equal to the passed in key. If
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
			/// Removes all the keys and values within this view from the underlying OrderedDictionary.
			/// </summary>
			/// <example>The following removes all the keys that start with "A" from an OrderedDictionary.
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
			/// Determine if the given key lies within the bounds of this view.
			/// </summary>
			/// <param name="key">Key to test.</param>
			/// <returns>True if the key is within the bounds of this view.</returns>
			private bool KeyInView(TKey key)
			{
				return rangeTester(NewPair(key, default(TValue))) == 0;
			}
		}

		#region IDictionary<TKey,TValue> Members

		/// <summary>
		/// Gets or sets the value associated with a given key. When getting a value, if this
		/// key is not found in the collection, then an ArgumentException is thrown. When setting
		/// a value, the value replaces any existing value in the dictionary.
		/// </summary>
		/// <remarks>The indexer takes time O(log N), where N is the number of entries in the dictionary.</remarks>
		/// <value>The value associated with the key</value>
		/// <exception cref="ArgumentException">A value is being retrieved, and the key is not present in the dictionary.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
		public override sealed TValue this[TKey key]
		{
			get
			{
				KeyValuePair<TKey, TValue> pairFound;
				bool found;

				found = tree.Find(NewPair(key), false, false, out pairFound);
				if (found)
					return pairFound.Value;
				else
					throw new KeyNotFoundException(Strings.KeyNotFound);
			}
			set
			{
				KeyValuePair<TKey, TValue> dummy;
				tree.Insert(NewPair(key, value),
					DuplicatePolicy.ReplaceLast, out dummy);
			}
		}

		/// <summary>
		/// Removes the key (and associated value) from the collection that is equal to the passed in key. If
		/// no key in the dictionary is equal to the passed key, false is returned and the 
		/// dictionary is unchanged.
		/// </summary>
		/// <remarks>Equality between keys is determined by the comparison instance or delegate used
		/// to create the dictionary.</remarks>
		/// <param name="key">The key to remove.</param>
		/// <returns>True if the key was found and removed. False if the key was not found.</returns>
		public override sealed bool Remove(TKey key)
		{
			KeyValuePair<TKey, TValue> keyPair = NewPair(key);
			KeyValuePair<TKey, TValue> item;
			return tree.Delete(keyPair, true, out item);
		}

		/// <summary>
		/// Removes all keys and values from the dictionary.
		/// </summary>
		/// <remarks>Clearing the dictionary takes a constant amount of time, regardless of the number of keys in it.</remarks>
		public override sealed void Clear()
		{
			tree.StopEnumerations(); // Invalidate any enumerations.

			// The simplest and fastest way is simply to throw away the old tree and create a new one.
			tree = new RedBlackTree<KeyValuePair<TKey, TValue>>(pairComparer);
		}

		/// <summary>
		/// Finds a key in the dictionary. If the dictionary already contains
		/// a key equal to the passed key, then the existing value is returned via value. If the dictionary
		/// doesn't contain that key, then value is associated with that key.
		/// </summary>
		/// <remarks><para> between keys is determined by the comparison instance or delegate used
		/// to create the dictionary.</para>
		/// <para>This method takes time O(log N), where N is the number of keys in the dictionary. If a value is added, It is more efficient than
		/// calling TryGetValue followed by Add, because the dictionary is not searched twice.</para></remarks>
		/// <param name="key">The new key. </param>
		/// <param name="value">The new value to associated with that key, if the key isn't present. If the key was present, 
		/// returns the exist value associated with that key.</param>
		/// <returns>True if key was already present, false if key wasn't present (and a new value was added).</returns>
		public bool GetValueElseAdd(TKey key, ref TValue value)
		{
			KeyValuePair<TKey, TValue> pair = NewPair(key, value);
			KeyValuePair<TKey, TValue> old;

			bool added = tree.Insert(pair, DuplicatePolicy.DoNothing, out old);
			if (!added)
				value = old.Value;
			return !added;
		}

		/// <summary>
		/// Adds a new key and value to the dictionary. If the dictionary already contains
		/// a key equal to the passed key, then an ArgumentException is thrown
		/// </summary>
		/// <remarks>
		/// <para>Equality between keys is determined by the comparison instance or delegate used
		/// to create the dictionary.</para>
		/// <para>Adding an key and value takes time O(log N), where N is the number of keys in the dictionary.</para></remarks>
		/// <param name="key">The new key. "null" is a valid key value.</param>
		/// <param name="value">The new value to associated with that key.</param>
		/// <exception cref="ArgumentException">key is already present in the dictionary</exception>
		public override sealed void Add(TKey key, TValue value)
		{
			KeyValuePair<TKey, TValue> pair = NewPair(key, value);
			KeyValuePair<TKey, TValue> dummy;

			bool added = tree.Insert(pair, DuplicatePolicy.DoNothing, out dummy);
			if (! added)
				throw new ArgumentException(Strings.KeyAlreadyPresent, "key");
		}

		/// <summary>
		/// Changes the value associated with a given key. If the dictionary does not contain
		/// a key equal to the passed key, then an ArgumentException is thrown.
		/// </summary>
		/// <remarks>
		/// <p>Unlike adding or removing an element, changing the value associated with a key
		/// can be performed while an enumeration (foreach) on the the dictionary is in progress.</p>
		/// <p>Equality between keys is determined by the comparison instance or delegate used
		/// to create the dictionary.</p>
		/// <p>Replace takes time O(log N), where N is the number of entries in the dictionary.</p></remarks>
		/// <param name="key">The new key. </param>
		/// <param name="value">The new value to associated with that key.</param>
		/// <exception cref="KeyNotFoundException">key is not present in the dictionary</exception>
		public void Replace(TKey key, TValue value)
		{
			KeyValuePair<TKey, TValue> pair = NewPair(key, value);
			KeyValuePair<TKey, TValue> dummy;

			bool found = tree.Find(pair, true, true, out dummy);
			if (!found)
				throw new KeyNotFoundException(Strings.KeyNotFound);
		}

		/// <summary>
		/// Adds multiple key-value pairs to a dictionary. If a key exists in both the current instance and dictionaryToAdd,
		/// then the value is updated with the value from <paramref name="keysAndValues>"/> (no exception is thrown).
		/// Since IDictionary&lt;TKey,TValue&gt; inherits from IEnumerable&lt;KeyValuePair&lt;TKey,TValue&gt;&gt;, this
		/// method can be used to merge one dictionary into another.
		/// </summary>
		/// <remarks>AddMany takes time O(M log (N+M)), where M is the size of <paramref name="keysAndValues>"/>, and N is the size of
		/// this dictionary.</remarks>
		/// <param name="keysAndValues">A collection of keys and values whose contents are added to the current dictionary.</param>
		public void AddMany(IEnumerable<KeyValuePair<TKey, TValue>> keysAndValues)
		{
			if (keysAndValues == null)
				throw new ArgumentNullException("keysAndValues");

			foreach (KeyValuePair<TKey, TValue> pair in keysAndValues)
			{
				this[pair.Key] = pair.Value;
			}
		}

		/// <summary>
		/// Removes all the keys found in another collection (such as an array or List&lt;TKey&gt;). Each key in keyCollectionToRemove
		/// is removed from the dictionary. Keys that are not present are ignored.
		/// </summary>
		/// <remarks>RemoveMany takes time O(M log N), where M is the size of keyCollectionToRemove, and N is this
		/// size of this collection.</remarks>
		/// <returns>The number of keys removed from the dictionary.</returns>
		/// <param name="keyCollectionToRemove">A collection of keys to remove from the dictionary.</param>
		public int RemoveMany(IEnumerable<TKey> keyCollectionToRemove)
		{
			if (keyCollectionToRemove == null)
				throw new ArgumentNullException("keyCollectionToRemove");

			int count = 0;

			foreach (TKey key in keyCollectionToRemove)
			{
				if (this.Remove(key))
					++count;
			}

			return count;
		}

		/// <summary>
		/// Determines if this dictionary contains a key equal to <paramref name="key"/>. The dictionary
		/// is not changed.
		/// </summary>
		/// <remarks>Searching the dictionary for a key takes time O(log N), where N is the number of keys in the dictionary.</remarks>
		/// <param name="key">The key to search for.</param>
		/// <returns>True if the dictionary contains key. False if the dictionary does not contain key.</returns>
		public override sealed bool ContainsKey(TKey key)
		{
			KeyValuePair<TKey, TValue> pairFound;

			return tree.Find(NewPair(key), false, false, out pairFound);
		}

		/// <summary>
		/// Determines if this dictionary contains a key equal to <paramref name="key"/>. If so, the value
		/// associated with that key is returned through the value parameter.
		/// </summary>
		/// <remarks>TryGetValue takes time O(log N), where N is the number of entries in the dictionary.</remarks>
		/// <param name="key">The key to search for.</param>
		/// <param name="value">Returns the value associated with key, if true was returned.</param>
		/// <returns>True if the dictionary contains key. False if the dictionary does not contain key.</returns>
		public override sealed bool TryGetValue(TKey key, out TValue value)
		{
			KeyValuePair<TKey, TValue> pairFound;

			bool found = tree.Find(NewPair(key), false, false, out pairFound);
			value = pairFound.Value;
			return found;
		}

		#endregion

		#region ICollection<KeyValuePair<TKey,TValue>> Members

		/// <summary>
		/// Returns the number of keys in the dictionary.
		/// </summary>
		/// <remarks>The size of the dictionary is returned in constant time..</remarks>
		/// <value>The number of keys in the dictionary.</value>
		public override sealed int Count
		{
			get { return tree.ElementCount; }
		}

		#endregion

		#region IEnumerable<KeyValuePair<TKey,TValue>> Members

		/// <summary>
		/// Returns an enumerator that enumerates all the entries in the dictionary. Each entry is 
		/// returned as a KeyValuePair&lt;TKey,TValue&gt;.
		/// The entries are enumerated in the sorted order of the keys.
		/// </summary>
		/// <remarks>
		/// <p>Typically, this method is not called directly. Instead the "foreach" statement is used
		/// to enumerate the elements of the dictionary, which uses this method implicitly.</p>
		/// <p>If an element is added to or deleted from the dictionary while it is being enumerated, then 
		/// the enumeration will end with an InvalidOperationException.</p>
		/// <p>Enumeration all the entries in the dictionary takes time O(N log N), where N is the number
		/// of entries in the dictionary.</p>
		/// </remarks>
		/// <returns>An enumerator for enumerating all the elements in the OrderedDictionary.</returns>		
		public override sealed IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return tree.GetEnumerator();
		}

		#endregion

		#region ICloneable Members

		/// <summary>
		/// Implements ICloneable.Clone. Makes a shallow clone of this dictionary; i.e., if keys or values are reference types, then they are not cloned.
		/// </summary>
		/// <returns>The cloned dictionary.</returns>
		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

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

		/// <summary>
		/// Helper function to create a new KeyValuePair struct with a default value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>A new KeyValuePair.</returns>
		private static KeyValuePair<TKey, TValue> NewPair(TKey key)
		{
			KeyValuePair<TKey, TValue> pair = new KeyValuePair<TKey, TValue>(key, default(TValue));
			return pair;
		}

		/// <summary>
		/// Throw an InvalidOperationException indicating that this type is not cloneable.
		/// </summary>
		/// <param name="t">Type to test.</param>
		private static void NonCloneableType(Type t)
		{
			throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, t.FullName));
		}
	}
}