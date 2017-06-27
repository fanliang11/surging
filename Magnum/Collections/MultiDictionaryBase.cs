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
	using System.Text;

	/// <summary>
	/// MultiDictionaryBase is a base class that can be used to more easily implement a class
	/// that associates multiple values to a single key. The class implements the generic
	/// IDictionary&lt;TKey, ICollection&lt;TValue&gt;&gt; interface.
	/// </summary>
	/// <remarks>
	/// <para>To use MultiDictionaryBase as a base class, the derived class must override
	/// Count, Clear, Add, Remove(TKey), Remove(TKey,TValue), Contains(TKey,TValue), 
	/// EnumerateKeys, and TryEnumerateValuesForKey. </para>
	/// <para>It may wish consider overriding CountValues, CountAllValues, ContainsKey,
	/// and EqualValues, but these are not required.
	/// </para>
	/// </remarks>
	/// <typeparam name="TKey">The key type of the dictionary.</typeparam>
	/// <typeparam name="TValue">The value type of the dictionary.</typeparam>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplayString()}")]
	public abstract class MultiDictionaryBase<TKey, TValue> : CollectionBase<KeyValuePair<TKey, ICollection<TValue>>>,
		IDictionary<TKey, ICollection<TValue>>
	{
		private volatile IEqualityComparer<TValue> valueEqualityComparer;

		/// <summary>
		/// Gets a read-only collection of all the values in the dictionary. 
		/// </summary>
		/// <returns>A read-only ICollection&lt;TValue&gt; of all the values in the dictionary.</returns>
		public virtual ICollection<TValue> Values
		{
			get { return new ValuesCollection(this); }
		}

		/// <summary>
		/// Gets a read-only collection of all key-value pairs in the dictionary. If a key has multiple
		/// values associated with it, then a key-value pair is present for each value associated
		/// with the key.
		/// </summary>
		public virtual ICollection<KeyValuePair<TKey, TValue>> KeyValuePairs
		{
			get { return new KeyValuePairsCollection(this); }
		}

		/// <summary>
		/// Returns a collection of all of the values in the dictionary associated with <paramref name="key"/>,
		/// or changes the set of values associated with <paramref name="key"/>.
		/// If the key is not present in the dictionary, an ICollection enumerating no
		/// values is returned. The returned collection of values is read-write, and can be used to 
		/// modify the collection of values associated with the key.
		/// </summary>
		/// <param name="key">The key to get the values associated with.</param>
		/// <value>An ICollection&lt;TValue&gt; with all the values associated with <paramref name="key"/>.</value>
		public virtual ICollection<TValue> this[TKey key]
		{
			get { return new ValuesForKeyCollection(this, key); }
			set { ReplaceMany(key, value); }
		}

		/// <summary>
		/// Clears the dictionary. This method must be overridden in the derived class.
		/// </summary>
		public abstract override void Clear();

		/// <summary>
		/// Gets the number of keys in the dictionary. This property must be overridden
		/// in the derived class.
		/// </summary>
		public abstract override int Count { get; }

		/// <summary>
		/// Adds a key-value pair to the collection. The value part of the pair must be a collection
		/// of values to associate with the key. If values are already associated with the given
		/// key, the new values are added to the ones associated with that key.
		/// </summary>
		/// <param name="item">A KeyValuePair contains the Key and Value collection to add.</param>
		public override void Add(KeyValuePair<TKey, ICollection<TValue>> item)
		{
			this.AddMany(item.Key, item.Value);
		}

		/// <summary>
		/// Implements IDictionary&lt;TKey, IEnumerable&lt;TValue&gt;&gt;.Add. If the 
		/// key is already present, and ArgumentException is thrown. Otherwise, a
		/// new key is added, and new values are associated with that key.
		/// </summary>
		/// <param name="key">Key to add.</param>
		/// <param name="values">Values to associate with that key.</param>
		/// <exception cref="ArgumentException">The key is already present in the dictionary.</exception>
		void IDictionary<TKey, ICollection<TValue>>.Add(TKey key, ICollection<TValue> values)
		{
			if (ContainsKey(key))
			{
				throw new ArgumentException(Strings.KeyAlreadyPresent, "key");
			}
			else
			{
				AddMany(key, values);
			}
		}

		/// <summary>
		/// Removes a key from the dictionary. This method must be overridden in the derived class.
		/// </summary>
		/// <param name="key">Key to remove from the dictionary.</param>
		/// <returns>True if the key was found, false otherwise.</returns>
		public abstract bool Remove(TKey key);

		/// <summary>
		/// Removes a set of values from a given key. If all values associated with a key are
		/// removed, then the key is removed also.
		/// </summary>
		/// <param name="pair">A KeyValuePair contains a key and a set of values to remove from that key.</param>
		/// <returns>True if at least one values was found and removed.</returns>
		public override bool Remove(KeyValuePair<TKey, ICollection<TValue>> pair)
		{
			return RemoveMany(pair.Key, pair.Value) > 0;
		}


		/// <summary>
		/// Determines if this dictionary contains a key equal to <paramref name="key"/>. If so, all the values
		/// associated with that key are returned through the values parameter. This method must be
		/// overridden by the derived class.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="values">Returns all values associated with key, if true was returned.</param>
		/// <returns>True if the dictionary contains key. False if the dictionary does not contain key.</returns>
		bool IDictionary<TKey, ICollection<TValue>>.TryGetValue(TKey key, out ICollection<TValue> values)
		{
			if (ContainsKey(key))
			{
				values = this[key];
				return true;
			}
			else
			{
				values = null;
				return false;
			}
		}

		/// <summary>
		/// Determines whether a given key is found in the dictionary.
		/// </summary>
		/// <remarks>The default implementation simply calls TryEnumerateValuesForKey.
		/// It may be appropriate to override this method to 
		/// provide a more efficient implementation.</remarks>
		/// <param name="key">Key to look for in the dictionary.</param>
		/// <returns>True if the key is present in the dictionary.</returns>
		public virtual bool ContainsKey(TKey key)
		{
			IEnumerator<TValue> values;
			return TryEnumerateValuesForKey(key, out values);
		}

		/// <summary>
		/// Determines if this dictionary contains the given key and all of the values associated with that key..
		/// </summary>
		/// <param name="pair">A key and collection of values to search for.</param>
		/// <returns>True if the dictionary has associated all of the values in <paramref name="pair"/>.Value with <paramref name="pair"/>.Key.</returns>
		public override bool Contains(KeyValuePair<TKey, ICollection<TValue>> pair)
		{
			foreach (TValue val in pair.Value)
			{
				if (! Contains(pair.Key, val))
					return false;
			}

			return true;
		}

		// Cache the equality comparer after we get it the first time.

		/// <summary>
		/// Gets a read-only collection all the keys in this dictionary.
		/// </summary>
		/// <value>An readonly ICollection&lt;TKey&gt; of all the keys in this dictionary.</value>
		public virtual ICollection<TKey> Keys
		{
			get { return new KeysCollection(this); }
		}

		/// <summary>
		/// Gets a read-only collection of all the value collections in the dictionary. 
		/// </summary>
		/// <returns>A read-only ICollection&lt;IEnumerable&lt;TValue&gt;&gt; of all the values in the dictionary.</returns>
		ICollection<ICollection<TValue>> IDictionary<TKey, ICollection<TValue>>.Values
		{
			get { return new EnumerableValuesCollection(this); }
		}

		/// <summary>
		/// Gets a collection of all the values in the dictionary associated with <paramref name="key"/>,
		/// or changes the set of values associated with <paramref name="key"/>.
		/// If the key is not present in the dictionary, a KeyNotFound exception is thrown.
		/// </summary>
		/// <param name="key">The key to get the values associated with.</param>
		/// <value>An IEnumerable&lt;TValue&gt; that enumerates all the values associated with <paramref name="key"/>.</value>
		/// <exception cref="KeyNotFoundException">The given key is not present in the dictionary.</exception>
		ICollection<TValue> IDictionary<TKey, ICollection<TValue>>.this[TKey key]
		{
			get
			{
				if (ContainsKey(key))
					return new ValuesForKeyCollection(this, key);
				else
					throw new KeyNotFoundException(Strings.KeyNotFound);
			}
			set { ReplaceMany(key, value); }
		}

		/// <summary>
		/// Enumerate all the keys in the dictionary, and for each key, the collection of values for that key.
		/// </summary>
		/// <returns>An enumerator to enumerate all the key, ICollection&lt;value&gt; pairs in the dictionary.</returns>
		public override IEnumerator<KeyValuePair<TKey, ICollection<TValue>>> GetEnumerator()
		{
			using (IEnumerator<TKey> enumKeys = EnumerateKeys())
			{
				while (enumKeys.MoveNext())
				{
					TKey key = enumKeys.Current;
					yield return new KeyValuePair<TKey, ICollection<TValue>>(key, new ValuesForKeyCollection(this, key));
				}
			}
		}

		#region Keys and Values collections

		/// <summary>
		/// A private class that provides the ICollection&lt;TValue&gt; for a particular key. This is the collection
		/// that is returned from the indexer. The collections is read-write, live, and can be used to add, remove,
		/// etc. values from the multi-dictionary.
		/// </summary>
		[Serializable]
		private sealed class ValuesForKeyCollection : CollectionBase<TValue>
		{
			private readonly TKey key;
			private readonly MultiDictionaryBase<TKey, TValue> myDictionary;

			/// <summary>
			/// Constructor. Initializes this collection.
			/// </summary>
			/// <param name="myDictionary">Dictionary we're using.</param>
			/// <param name="key">The key we're looking at.</param>
			public ValuesForKeyCollection(MultiDictionaryBase<TKey, TValue> myDictionary, TKey key)
			{
				this.myDictionary = myDictionary;
				this.key = key;
			}

			/// <summary>
			/// Get the number of values associated with the key.
			/// </summary>
			public override int Count
			{
				get { return myDictionary.CountValues(key); }
			}

			/// <summary>
			/// Remove the key and all values associated with it.
			/// </summary>
			public override void Clear()
			{
				myDictionary.Remove(key);
			}

			/// <summary>
			/// Add a new values to this key.
			/// </summary>
			/// <param name="item">New values to add.</param>
			public override void Add(TValue item)
			{
				myDictionary.Add(key, item);
			}

			/// <summary>
			/// Remove a value currently associated with key.
			/// </summary>
			/// <param name="item">Value to remove.</param>
			/// <returns>True if item was assocaited with key, false otherwise.</returns>
			public override bool Remove(TValue item)
			{
				return myDictionary.Remove(key, item);
			}

			/// <summary>
			/// Enumerate all the values associated with key.
			/// </summary>
			/// <returns>An IEnumerator&lt;TValue&gt; that enumerates all the values associated with key.</returns>
			public override IEnumerator<TValue> GetEnumerator()
			{
				IEnumerator<TValue> values;
				if (myDictionary.TryEnumerateValuesForKey(key, out values))
					return values;
				else
					return NoValues();
			}

			/// <summary>
			/// Determines if the given values is associated with key.
			/// </summary>
			/// <param name="item">Value to check for.</param>
			/// <returns>True if value is associated with key, false otherwise.</returns>
			public override bool Contains(TValue item)
			{
				return myDictionary.Contains(key, item);
			}

			/// <summary>
			/// A simple function that returns an IEnumerator&lt;TValue&gt; that
			/// doesn't yield any values. A helper.
			/// </summary>
			/// <returns>An IEnumerator&lt;TValue&gt; that yields no values.</returns>
			private static IEnumerator<TValue> NoValues()
			{
				yield break;
			}
		}

		/// <summary>
		/// A private class that implements ICollection&lt;TKey&gt; and ICollection for the
		/// Keys collection. The collection is read-only.
		/// </summary>
		[Serializable]
		private sealed class KeysCollection : ReadOnlyCollectionBase<TKey>
		{
			private readonly MultiDictionaryBase<TKey, TValue> myDictionary;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="myDictionary">The dictionary this is associated with.</param>
			public KeysCollection(MultiDictionaryBase<TKey, TValue> myDictionary)
			{
				this.myDictionary = myDictionary;
			}

			public override int Count
			{
				get { return myDictionary.Count; }
			}

			public override IEnumerator<TKey> GetEnumerator()
			{
				return myDictionary.EnumerateKeys();
			}

			public override bool Contains(TKey key)
			{
				return myDictionary.ContainsKey(key);
			}
		}

		/// <summary>
		/// A private class that implements ICollection&lt;TValue&gt; and ICollection for the
		/// Values collection. The collection is read-only.
		/// </summary>
		[Serializable]
		private sealed class ValuesCollection : ReadOnlyCollectionBase<TValue>
		{
			private readonly MultiDictionaryBase<TKey, TValue> myDictionary;

			public ValuesCollection(MultiDictionaryBase<TKey, TValue> myDictionary)
			{
				this.myDictionary = myDictionary;
			}

			public override int Count
			{
				get { return myDictionary.CountAllValues(); }
			}

			public override IEnumerator<TValue> GetEnumerator()
			{
				using (IEnumerator<TKey> enumKeys = myDictionary.EnumerateKeys())
				{
					while (enumKeys.MoveNext())
					{
						TKey key = enumKeys.Current;
						IEnumerator<TValue> enumValues;
						if (myDictionary.TryEnumerateValuesForKey(key, out enumValues))
						{
							using (enumValues)
							{
								while (enumValues.MoveNext())
									yield return enumValues.Current;
							}
						}
					}
				}
			}

			public override bool Contains(TValue value)
			{
				foreach (TValue v in this)
				{
					if (myDictionary.EqualValues(v, value))
						return true;
				}
				return false;
			}
		}

		/// <summary>
		/// A private class that implements ICollection&lt;ICollection&lt;TValue&gt;&gt; and ICollection for the
		/// Values collection on IDictionary. The collection is read-only.
		/// </summary>
		[Serializable]
		private sealed class EnumerableValuesCollection : ReadOnlyCollectionBase<ICollection<TValue>>
		{
			private readonly MultiDictionaryBase<TKey, TValue> myDictionary;

			public EnumerableValuesCollection(MultiDictionaryBase<TKey, TValue> myDictionary)
			{
				this.myDictionary = myDictionary;
			}

			public override int Count
			{
				get { return myDictionary.Count; }
			}

			public override IEnumerator<ICollection<TValue>> GetEnumerator()
			{
				using (IEnumerator<TKey> enumKeys = myDictionary.EnumerateKeys())
				{
					while (enumKeys.MoveNext())
					{
						TKey key = enumKeys.Current;
						yield return new ValuesForKeyCollection(myDictionary, key);
					}
				}
			}

			public override bool Contains(ICollection<TValue> values)
			{
				if (values == null)
					return false;
				TValue[] valueArray = Algorithms.ToArray(values);

				foreach (ICollection<TValue> v in this)
				{
					if (v.Count != valueArray.Length)
						continue;

					// First check in order for efficiency.
					if (Algorithms.EqualCollections(v, values, myDictionary.EqualValues))
						return true;

					// Now check not in order. We can't use Algorithms.EqualSets, because we don't 
					// have an IEqualityComparer, just the ability to compare for equality. Unfortunately this is N squared,
					// but there isn't a good choice here. We don't really expect this method to be used much.
					bool[] found = new bool[valueArray.Length];
					foreach (TValue x in v)
					{
						for (int i = 0; i < valueArray.Length; ++i)
						{
							if (!found[i] && myDictionary.EqualValues(x, valueArray[i]))
								found[i] = true;
						}
					}

					if (Array.IndexOf(found, false) < 0)
						return true; // every item was found. The sets must be equal.
				}
				return false;
			}
		}

		/// <summary>
		/// A private class that implements ICollection&lt;KeyValuePair&lt;TKey,TValue&gt;&gt; and ICollection for the
		/// KeyValuePairs collection. The collection is read-only.
		/// </summary>
		[Serializable]
		private sealed class KeyValuePairsCollection : ReadOnlyCollectionBase<KeyValuePair<TKey, TValue>>
		{
			private readonly MultiDictionaryBase<TKey, TValue> myDictionary;

			public KeyValuePairsCollection(MultiDictionaryBase<TKey, TValue> myDictionary)
			{
				this.myDictionary = myDictionary;
			}

			public override int Count
			{
				get { return myDictionary.CountAllValues(); }
			}

			public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			{
				using (IEnumerator<TKey> enumKeys = myDictionary.EnumerateKeys())
				{
					while (enumKeys.MoveNext())
					{
						TKey key = enumKeys.Current;
						IEnumerator<TValue> enumValues;
						if (myDictionary.TryEnumerateValuesForKey(key, out enumValues))
						{
							using (enumValues)
							{
								while (enumValues.MoveNext())
									yield return new KeyValuePair<TKey, TValue>(key, enumValues.Current);
							}
						}
					}
				}
			}

			public override bool Contains(KeyValuePair<TKey, TValue> pair)
			{
				return myDictionary[pair.Key].Contains(pair.Value);
			}
		}

		#endregion

		/// <summary>
		/// <para>Adds new values to be associated with a key. If duplicate values are permitted, this
		/// method always adds new key-value pairs to the dictionary.</para>
		/// <para>If duplicate values are not permitted, and <paramref name="key"/> already has a value
		/// equal to one of <paramref name="values"/> associated with it, then that value is replaced,
		/// and the number of values associate with <paramref name="key"/> is unchanged.</para>
		/// </summary>
		/// <param name="key">The key to associate with.</param>
		/// <param name="values">A collection of values to associate with <paramref name="key"/>.</param>
		public virtual void AddMany(TKey key, IEnumerable<TValue> values)
		{
			foreach (TValue value in values)
				Add(key, value);
		}

		/// <summary>
		/// Adds a new key-value pair to the dictionary.  This method must be overridden in the derived class.
		/// </summary>
		/// <param name="key">Key to add.</param>
		/// <param name="value">Value to associated with the key.</param>
		/// <exception cref="ArgumentException">key is already present in the dictionary</exception>
		public abstract void Add(TKey key, TValue value);

		/// <summary>
		/// Removes a key-value pair from the dictionary. This method must be overridden in the derived class.
		/// </summary>
		/// <param name="key">Key to remove from the dictionary.</param>
		/// <param name="value">Associated value to remove from the dictionary.</param>
		/// <returns>True if the key-value pair was found, false otherwise.</returns>
		public abstract bool Remove(TKey key, TValue value);

		/// <summary>
		/// Removes a collection of values from the values associated with a key. If the
		/// last value is removed from a key, the key is removed also.
		/// </summary>
		/// <param name="key">A key to remove values from.</param>
		/// <param name="values">A collection of values to remove.</param>
		/// <returns>The number of values that were present and removed. </returns>
		public virtual int RemoveMany(TKey key, IEnumerable<TValue> values)
		{
			int countRemoved = 0;

			foreach (TValue val in values)
			{
				if (Remove(key, val))
					++countRemoved;
			}

			return countRemoved;
		}

		/// <summary>
		/// Remove all of the keys (and any associated values) in a collection
		/// of keys. If a key is not present in the dictionary, nothing happens.
		/// </summary>
		/// <param name="keyCollection">A collection of key values to remove.</param>
		/// <returns>The number of keys from the collection that were present and removed.</returns>
		public int RemoveMany(IEnumerable<TKey> keyCollection)
		{
			int count = 0;
			foreach (TKey key in keyCollection)
			{
				if (Remove(key))
					++count;
			}

			return count;
		}

		/// <summary>
		/// Determines if this dictionary contains a key-value pair equal to <paramref name="key"/> and 
		/// <paramref name="value"/>. The dictionary is not changed. This method must be overridden in the derived class.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="value">The value to search for.</param>
		/// <returns>True if the dictionary has associated <paramref name="value"/> with <paramref name="key"/>.</returns>
		public abstract bool Contains(TKey key, TValue value);

		/// <summary>
		/// Replaces all values associated with <paramref name="key"/> with the single value <paramref name="value"/>.
		/// </summary>
		/// <remarks>This implementation simply calls Remove, followed by Add.</remarks>
		/// <param name="key">The key to associate with.</param>
		/// <param name="value">The new values to be associated with <paramref name="key"/>.</param>
		/// <returns>Returns true if some values were removed. Returns false if <paramref name="key"/> was not
		/// present in the dictionary before Replace was called.</returns>
		public virtual bool Replace(TKey key, TValue value)
		{
			bool removed = Remove(key);
			Add(key, value);
			return removed;
		}

		/// <summary>
		/// Replaces all values associated with <paramref name="key"/> with a new collection
		/// of values. If the collection does not permit duplicate values, and <paramref name="values"/> has duplicate
		/// items, then only the last of duplicates is added.
		/// </summary>
		/// <param name="key">The key to associate with.</param>
		/// <param name="values">The new values to be associated with <paramref name="key"/>.</param>
		/// <returns>Returns true if some values were removed. Returns false if <paramref name="key"/> was not
		/// present in the dictionary before Replace was called.</returns>
		public bool ReplaceMany(TKey key, IEnumerable<TValue> values)
		{
			bool removed = Remove(key);
			AddMany(key, values);
			return removed;
		}

		/// <summary>
		/// Shows the string representation of the dictionary. The string representation contains
		/// a list of the mappings in the dictionary.
		/// </summary>
		/// <returns>The string representation of the dictionary.</returns>
		public override string ToString()
		{
			bool firstItem = true;

			StringBuilder builder = new StringBuilder();

			builder.Append("{");

			// Call ToString on each item and put it in.
			foreach (KeyValuePair<TKey, ICollection<TValue>> pair in this)
			{
				if (!firstItem)
					builder.Append(", ");

				if (pair.Key == null)
					builder.Append("null");
				else
					builder.Append(pair.Key.ToString());

				builder.Append("->");

				// Put all values in a parenthesized list.
				builder.Append('(');

				bool firstValue = true;
				foreach (TValue val in pair.Value)
				{
					if (!firstValue)
						builder.Append(",");

					if (val == null)
						builder.Append("null");
					else
						builder.Append(val.ToString());

					firstValue = false;
				}

				builder.Append(')');

				firstItem = false;
			}

			builder.Append("}");
			return builder.ToString();
		}

		/// <summary>
		/// Enumerate all the keys in the dictionary. This method must be overridden by a derived
		/// class.
		/// </summary>
		/// <returns>An IEnumerator&lt;TKey&gt; that enumerates all of the keys in the collection that
		/// have at least one value associated with them.</returns>
		protected abstract IEnumerator<TKey> EnumerateKeys();

		/// <summary>
		/// Enumerate all of the values associated with a given key. This method must be overridden
		/// by the derived class. If the key exists and has values associated with it, an enumerator for those
		/// values is returned throught <paramref name="values"/>. If the key does not exist, false is returned.
		/// </summary>
		/// <param name="key">The key to get values for.</param>
		/// <param name="values">If true is returned, this parameter receives an enumerators that
		/// enumerates the values associated with that key.</param>
		/// <returns>True if the key exists and has values associated with it. False otherwise.</returns>
		protected abstract bool TryEnumerateValuesForKey(TKey key, out IEnumerator<TValue> values);

		/// <summary>
		/// If the derived class does not use the default comparison for values, this
		/// methods should be overridden to compare two values for equality. This is
		/// used for the correct implementation of ICollection.Contains on the Values
		/// and KeyValuePairs collections.
		/// </summary>
		/// <param name="value1">First value to compare.</param>
		/// <param name="value2">Second value to compare.</param>
		/// <returns>True if the values are equal.</returns>
		protected virtual bool EqualValues(TValue value1, TValue value2)
		{
			if (valueEqualityComparer == null)
				valueEqualityComparer = EqualityComparer<TValue>.Default;
			return valueEqualityComparer.Equals(value1, value2);
		}

		/// <summary>
		/// Gets a count of the number of values associated with a key. The
		/// default implementation is slow; it enumerators all of the values
		/// (using TryEnumerateValuesForKey) to count them. A derived class
		/// may be able to supply a more efficient implementation.
		/// </summary>
		/// <param name="key">The key to count values for.</param>
		/// <returns>The number of values associated with <paramref name="key"/>.</returns>
		protected virtual int CountValues(TKey key)
		{
			int count = 0;
			IEnumerator<TValue> enumValues;

			if (TryEnumerateValuesForKey(key, out enumValues))
			{
				using (enumValues)
				{
					while (enumValues.MoveNext())
						count += 1;
				}
			}

			return count;
		}

		/// <summary>
		/// Gets a total count of values in the collection. This default implementation
		/// is slow; it enumerates all of the keys in the dictionary and calls CountValues on each.
		/// A derived class may be able to supply a more efficient implementation.
		/// </summary>
		/// <returns>The total number of values associated with all keys in the dictionary.</returns>
		protected virtual int CountAllValues()
		{
			int count = 0;

			using (IEnumerator<TKey> enumKeys = EnumerateKeys())
			{
				while (enumKeys.MoveNext())
				{
					TKey key = enumKeys.Current;
					count += CountValues(key);
				}
			}

			return count;
		}

		/// <summary>
		/// Display the contents of the dictionary in the debugger. This is intentionally private, it is called
		/// only from the debugger due to the presence of the DebuggerDisplay attribute. It is similar
		/// format to ToString(), but is limited to 250-300 characters or so, so as not to overload the debugger.
		/// </summary>
		/// <returns>The string representation of the items in the collection, similar in format to ToString().</returns>
		internal new string DebuggerDisplayString()
		{
			const int MAXLENGTH = 250;

			bool firstItem = true;

			StringBuilder builder = new StringBuilder();

			builder.Append("{");

			// Call ToString on each item and put it in.
			foreach (KeyValuePair<TKey, ICollection<TValue>> pair in this)
			{
				if (builder.Length >= MAXLENGTH)
				{
					builder.Append(", ...");
					break;
				}

				if (!firstItem)
					builder.Append(", ");

				if (pair.Key == null)
					builder.Append("null");
				else
					builder.Append(pair.Key.ToString());

				builder.Append("->");

				// Put all values in a parenthesized list.
				builder.Append('(');

				bool firstValue = true;
				foreach (TValue val in pair.Value)
				{
					if (!firstValue)
						builder.Append(",");

					if (val == null)
						builder.Append("null");
					else
						builder.Append(val.ToString());

					firstValue = false;
				}

				builder.Append(')');

				firstItem = false;
			}

			builder.Append("}");
			return builder.ToString();
		}
	}
}