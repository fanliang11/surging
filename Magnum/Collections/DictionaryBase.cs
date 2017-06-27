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
	using System.Text;

	/// <summary>
	/// DictionaryBase is a base class that can be used to more easily implement the
	/// generic IDictionary&lt;T&gt; and non-generic IDictionary interfaces.
	/// </summary>
	/// <remarks>
	/// <para>To use DictionaryBase as a base class, the derived class must override
	/// Count, GetEnumerator, TryGetValue, Clear, Remove, and the indexer set accessor. </para>
	/// </remarks>
	/// <typeparam name="TKey">The key type of the dictionary.</typeparam>
	/// <typeparam name="TValue">The value type of the dictionary.</typeparam>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplayString()}")]
	public abstract class DictionaryBase<TKey, TValue> : CollectionBase<KeyValuePair<TKey, TValue>>,
		IDictionary<TKey, TValue>,
		IDictionary
	{
		/// <summary>
		/// Clears the dictionary. This method must be overridden in the derived class.
		/// </summary>
		public abstract override void Clear();

		/// <summary>
		/// Removes a key from the dictionary. This method must be overridden in the derived class.
		/// </summary>
		/// <param name="key">Key to remove from the dictionary.</param>
		/// <returns>True if the key was found, false otherwise.</returns>
		public abstract bool Remove(TKey key);

		/// <summary>
		/// Determines if this dictionary contains a key equal to <paramref name="key"/>. If so, the value
		/// associated with that key is returned through the value parameter. This method must be
		/// overridden by the derived class.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="value">Returns the value associated with key, if true was returned.</param>
		/// <returns>True if the dictionary contains key. False if the dictionary does not contain key.</returns>
		public abstract bool TryGetValue(TKey key, out TValue value);

		/// <summary>
		/// Adds a new key-value pair to the dictionary. 
		/// </summary>
		/// <remarks>The default implementation of this method
		/// checks to see if the key already exists using 
		/// ContainsKey, then calls the indexer setter if the key doesn't
		/// already exist. </remarks>
		/// <param name="key">Key to add.</param>
		/// <param name="value">Value to associated with the key.</param>
		/// <exception cref="ArgumentException">key is already present in the dictionary</exception>
		public virtual void Add(TKey key, TValue value)
		{
			if (ContainsKey(key))
			{
				throw new ArgumentException(Strings.KeyAlreadyPresent, "key");
			}
			else
			{
				this[key] = value;
			}
		}

		/// <summary>
		/// Determines whether a given key is found
		/// in the dictionary.
		/// </summary>
		/// <remarks>The default implementation simply calls TryGetValue and returns
		/// what it returns.</remarks>
		/// <param name="key">Key to look for in the dictionary.</param>
		/// <returns>True if the key is present in the dictionary.</returns>
		public virtual bool ContainsKey(TKey key)
		{
			TValue dummy;
			return TryGetValue(key, out dummy);
		}

		/// <summary>
		/// The indexer of the dictionary. This is used to store keys and values and
		/// retrieve values from the dictionary. The setter
		/// accessor must be overridden in the derived class.
		/// </summary>
		/// <param name="key">Key to find in the dictionary.</param>
		/// <returns>The value associated with the key.</returns>
		/// <exception cref="KeyNotFoundException">Thrown from the get accessor if the key
		/// was not found in the dictionary.</exception>
		public virtual TValue this[TKey key]
		{
			get
			{
				TValue value;
				if (TryGetValue(key, out value))
					return value;
				else
					throw new KeyNotFoundException(Strings.KeyNotFound);
			}

			set { throw new NotImplementedException(Strings.MustOverrideIndexerSet); }
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
			foreach (KeyValuePair<TKey, TValue> pair in this)
			{
				if (!firstItem)
					builder.Append(", ");

				if (pair.Key == null)
					builder.Append("null");
				else
					builder.Append(pair.Key.ToString());

				builder.Append("->");

				if (pair.Value == null)
					builder.Append("null");
				else
					builder.Append(pair.Value.ToString());

				firstItem = false;
			}

			builder.Append("}");
			return builder.ToString();
		}

		/// <summary>
		/// Provides a read-only view of this dictionary. The returned IDictionary&lt;TKey,TValue&gt; provides
		/// a view of the dictionary that prevents modifications to the dictionary. Use the method to provide
		/// access to the dictionary without allowing changes. Since the returned object is just a view,
		/// changes to the dictionary will be reflected in the view.
		/// </summary>
		/// <returns>An IIDictionary&lt;TKey,TValue&gt; that provides read-only access to the dictionary.</returns>
		public new virtual IDictionary<TKey, TValue> AsReadOnly()
		{
			return Algorithms.ReadOnly(this);
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
			foreach (KeyValuePair<TKey, TValue> pair in this)
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

				if (pair.Value == null)
					builder.Append("null");
				else
					builder.Append(pair.Value.ToString());

				firstItem = false;
			}

			builder.Append("}");
			return builder.ToString();
		}

		/// <summary>
		/// A class that wraps a IDictionaryEnumerator around an IEnumerator that
		/// enumerates KeyValuePairs. This is useful in implementing IDictionary, because
		/// IEnumerator can be implemented with an iterator, but IDictionaryEnumerator cannot.
		/// </summary>
		[Serializable]
		private class DictionaryEnumeratorWrapper : IDictionaryEnumerator
		{
			private readonly IEnumerator<KeyValuePair<TKey, TValue>> enumerator;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="enumerator">The enumerator of KeyValuePairs that is being wrapped.</param>
			public DictionaryEnumeratorWrapper(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
			{
				this.enumerator = enumerator;
			}

			public DictionaryEntry Entry
			{
				get
				{
					KeyValuePair<TKey, TValue> pair = enumerator.Current;
					DictionaryEntry entry = new DictionaryEntry();
					if (pair.Key != null)
						entry.Key = pair.Key;
					entry.Value = pair.Value;

					return entry;
				}
			}

			public object Key
			{
				get
				{
					KeyValuePair<TKey, TValue> pair = enumerator.Current;

					return pair.Key;
				}
			}

			public object Value
			{
				get
				{
					KeyValuePair<TKey, TValue> pair = enumerator.Current;
					return pair.Value;
				}
			}

			public void Reset()
			{
				throw new NotSupportedException(Strings.ResetNotSupported);
			}


			public bool MoveNext()
			{
				return enumerator.MoveNext();
			}


			public object Current
			{
				get { return Entry; }
			}
		}

		#region Keys and Values collections

		/// <summary>
		/// A private class that implements ICollection&lt;TKey&gt; and ICollection for the
		/// Keys collection. The collection is read-only.
		/// </summary>
		[Serializable]
		private sealed class KeysCollection : ReadOnlyCollectionBase<TKey>
		{
			private readonly DictionaryBase<TKey, TValue> myDictionary;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="myDictionary">The dictionary this is associated with.</param>
			public KeysCollection(DictionaryBase<TKey, TValue> myDictionary)
			{
				this.myDictionary = myDictionary;
			}

			public override int Count
			{
				get { return myDictionary.Count; }
			}

			public override IEnumerator<TKey> GetEnumerator()
			{
				foreach (KeyValuePair<TKey, TValue> pair in myDictionary)
					yield return pair.Key;
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
			private readonly DictionaryBase<TKey, TValue> myDictionary;

			public ValuesCollection(DictionaryBase<TKey, TValue> myDictionary)
			{
				this.myDictionary = myDictionary;
			}

			public override int Count
			{
				get { return myDictionary.Count; }
			}

			public override IEnumerator<TValue> GetEnumerator()
			{
				foreach (KeyValuePair<TKey, TValue> pair in myDictionary)
					yield return pair.Value;
			}
		}

		#endregion

		#region IDictionary<TKey,TValue> Members

		/// <summary>
		/// Returns a collection of the keys in this dictionary. 
		/// </summary>
		/// <value>A read-only collection of the keys in this dictionary.</value>
		public virtual ICollection<TKey> Keys
		{
			get { return new KeysCollection(this); }
		}

		/// <summary>
		/// Returns a collection of the values in this dictionary. The ordering of 
		/// values in this collection is the same as that in the Keys collection.
		/// </summary>
		/// <value>A read-only collection of the values in this dictionary.</value>
		public virtual ICollection<TValue> Values
		{
			get { return new ValuesCollection(this); }
		}

		#endregion

		#region ICollection<KeyValuePair<TKey,TValue>> Members

		/// <summary>
		/// Adds a key-value pair to the collection. This implementation calls the Add method
		/// with the Key and Value from the item.
		/// </summary>
		/// <param name="item">A KeyValuePair contains the Key and Value to add.</param>
		public override void Add(KeyValuePair<TKey, TValue> item)
		{
			this.Add(item.Key, item.Value);
		}

		/// <summary>
		/// Determines if a dictionary contains a given KeyValuePair. This implementation checks to see if the
		/// dictionary contains the given key, and if the value associated with the key is equal to (via object.Equals)
		/// the value.
		/// </summary>
		/// <param name="item">A KeyValuePair containing the Key and Value to check for.</param>
		/// <returns></returns>
		public override bool Contains(KeyValuePair<TKey, TValue> item)
		{
			if (this.ContainsKey(item.Key))
			{
				return (Equals(this[item.Key], item.Value));
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Determines if a dictionary contains a given KeyValuePair, and if so, removes it. This implementation checks to see if the
		/// dictionary contains the given key, and if the value associated with the key is equal to (via object.Equals)
		/// the value. If so, the key-value pair is removed.
		/// </summary>
		/// <param name="item">A KeyValuePair containing the Key and Value to check for.</param>
		/// <returns>True if the item was found and removed. False otherwise.</returns>
		public override bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (((ICollection<KeyValuePair<TKey, TValue>>) this).Contains(item))
				return this.Remove(item.Key);
			else
				return false;
		}

		#endregion

		#region IDictionary Members

		/// <summary>
		/// Adds a key-value pair to the collection. If key or value are not of the expected types, an
		/// ArgumentException is thrown. If both key and value are of the expected types, the (overridden)
		/// Add method is called with the key and value to add.
		/// </summary>
		/// <param name="key">Key to add to the dictionary.</param>
		/// <param name="value">Value to add to the dictionary.</param>
		/// <exception cref="ArgumentException">key or value are not of the expected type for this dictionary.</exception>
		void IDictionary.Add(object key, object value)
		{
			CheckGenericType<TKey>("key", key);
			CheckGenericType<TValue>("value", value);
			Add((TKey) key, (TValue) value);
		}

		/// <summary>
		/// Clears this dictionary. Calls the (overridden) Clear method.
		/// </summary>
		void IDictionary.Clear()
		{
			this.Clear();
		}

		/// <summary>
		/// Determines if this dictionary contains a key equal to <paramref name="key"/>. The dictionary
		/// is not changed. Calls the (overridden) ContainsKey method. If key is not of the correct
		/// TKey for the dictionary, false is returned.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <returns>True if the dictionary contains key. False if the dictionary does not contain key.</returns>
		bool IDictionary.Contains(object key)
		{
			if (key is TKey || key == null)
				return ContainsKey((TKey) key);
			else
				return false;
		}

		/// <summary>
		/// Removes the key (and associated value) from the collection that is equal to the passed in key. If
		/// no key in the dictionary is equal to the passed key, the 
		/// dictionary is unchanged. Calls the (overridden) Remove method. If key is not of the correct
		/// TKey for the dictionary, the dictionary is unchanged.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <exception cref="ArgumentException">key could not be converted to TKey.</exception>
		void IDictionary.Remove(object key)
		{
			if (key is TKey || key == null)
				Remove((TKey) key);
		}

		/// <summary>
		/// Returns an enumerator that enumerates all the entries in the dictionary. Each entry is 
		/// returned as a DictionaryEntry.
		/// The entries are enumerated in the same orders as the (overridden) GetEnumerator
		/// method.
		/// </summary>
		/// <returns>An enumerator for enumerating all the elements in the OrderedDictionary.</returns>		
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			// You can't implement this directly with an iterator, because iterators automatically implement
			// IEnumerator, not IDictionaryEnumerator. We use the helper class DictionaryEnumeratorWrapper.
			return new DictionaryEnumeratorWrapper(this.GetEnumerator());
		}

		/// <summary>
		/// Returns whether this dictionary is fixed size. This implemented always returns false.
		/// </summary>
		/// <value>Always returns false.</value>
		bool IDictionary.IsFixedSize
		{
			get { return false; }
		}

		/// <summary>
		/// Returns if this dictionary is read-only. This implementation always returns false.
		/// </summary>
		/// <value>Always returns false.</value>
		bool IDictionary.IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Returns a collection of all the keys in the dictionary. The values in this collection will
		/// be enumerated in the same order as the (overridden) GetEnumerator method.
		/// </summary>
		/// <value>The collection of keys.</value>
		ICollection IDictionary.Keys
		{
			get { return new KeysCollection(this); }
		}

		/// <summary>
		/// Returns a collection of all the values in the dictionary. The values in this collection will
		/// be enumerated in the same order as the (overridden) GetEnumerator method.
		/// </summary>
		/// <value>The collection of values.</value>
		ICollection IDictionary.Values
		{
			get { return new ValuesCollection(this); }
		}

		/// <summary>
		/// Gets or sets the value associated with a given key. When getting a value, if this
		/// key is not found in the collection, then null is returned. When setting
		/// a value, the value replaces any existing value in the dictionary. If either the key or value
		/// are not of the correct type for this dictionary, an ArgumentException is thrown.
		/// </summary>
		/// <value>The value associated with the key, or null if the key was not present.</value>
		/// <exception cref="ArgumentException">key could not be converted to TKey, or value could not be converted to TValue.</exception>
		object IDictionary.this[object key]
		{
			get
			{
				if (key is TKey || key == null)
				{
					TKey theKey = (TKey) key;
					TValue theValue;

					// The IDictionary (non-generic) indexer returns null for not found, instead of
					// throwing an exception like the generic IDictionary indexer.
					if (TryGetValue(theKey, out theValue))
						return theValue;
					else
						return null;
				}
				else
				{
					return null;
				}
			}
			set
			{
				CheckGenericType<TKey>("key", key);
				CheckGenericType<TValue>("value", value);
				this[(TKey) key] = (TValue) value;
			}
		}

		/// <summary>
		/// Returns an enumerator that enumerates all the entries in the dictionary. Each entry is 
		/// returned as a DictionaryEntry.
		/// The entries are enumerated in the same orders as the (overridden) GetEnumerator
		/// method.
		/// </summary>
		/// <returns>An enumerator for enumerating all the elements in the OrderedDictionary.</returns>		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IDictionary) this).GetEnumerator();
		}

		/// <summary>
		/// Check that the given parameter is of the expected generic type. Throw an ArgumentException
		/// if it isn't.
		/// </summary>
		/// <typeparam name="ExpectedType">Expected type of the parameter</typeparam>
		/// <param name="name">parameter name</param>
		/// <param name="value">parameter value</param>
		private static void CheckGenericType<ExpectedType>(string name, object value)
		{
			if (!(value is ExpectedType))
				throw new ArgumentException(string.Format(Strings.WrongType, value, typeof (ExpectedType)), name);
		}

		#endregion
	}
}