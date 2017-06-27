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
	/// ReadOnlyDictionaryBase is a base class that can be used to more easily implement the
	/// generic IDictionary&lt;T&gt; and non-generic IDictionary interfaces.
	/// </summary>
	/// <remarks>
	/// <para>To use ReadOnlyDictionaryBase as a base class, the derived class must override
	/// Count, TryGetValue, GetEnumerator. </para>
	/// </remarks>
	/// <typeparam name="TKey">The key type of the dictionary.</typeparam>
	/// <typeparam name="TValue">The value type of the dictionary.</typeparam>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplayString()}")]
	public abstract class ReadOnlyDictionaryBase<TKey, TValue> : ReadOnlyCollectionBase<KeyValuePair<TKey, TValue>>,
		IDictionary<TKey, TValue>,
		IDictionary
	{
		/// <summary>
		/// Adds a new key-value pair to the dictionary. Always throws an exception
		/// indicating that this method is not supported in a read-only dictionary.
		/// </summary>
		/// <param name="key">Key to add.</param>
		/// <param name="value">Value to associated with the key.</param>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			MethodModifiesCollection();
		}

		/// <summary>
		/// Removes a key from the dictionary. Always throws an exception
		/// indicating that this method is not supported in a read-only dictionary.
		/// </summary>
		/// <param name="key">Key to remove from the dictionary.</param>
		/// <returns>True if the key was found, false otherwise.</returns>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		public virtual bool Remove(TKey key)
		{
			MethodModifiesCollection();
			return false;
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
		/// Determines if this dictionary contains a key equal to <paramref name="key"/>. If so, the value
		/// associated with that key is returned through the value parameter. This method must be overridden 
		/// in the derived class.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="value">Returns the value associated with key, if true was returned.</param>
		/// <returns>True if the dictionary contains key. False if the dictionary does not contain key.</returns>
		public abstract bool TryGetValue(TKey key, out TValue value);

		/// <summary>
		/// The indexer of the dictionary. The set accessor throws an NotSupportedException
		/// stating the dictionary is read-only.
		/// </summary>
		/// <remarks>The get accessor is implemented by calling TryGetValue.</remarks>
		/// <param name="key">Key to find in the dictionary.</param>
		/// <returns>The value associated with the key.</returns>
		/// <exception cref="NotSupportedException">Always thrown from the set accessor, indicating
		/// that the dictionary is read only.</exception>
		/// <exception cref="KeyNotFoundException">Thrown from the get accessor if the key
		/// was not found.</exception>
		public virtual TValue this[TKey key]
		{
			get
			{
				TValue value;
				bool found = TryGetValue(key, out value);
				if (found)
					return value;
				else
					throw new KeyNotFoundException(Strings.KeyNotFound);
			}

			set { MethodModifiesCollection(); }
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
		/// Throws an NotSupportedException stating that this collection cannot be modified.
		/// </summary>
		private void MethodModifiesCollection()
		{
			throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, Util.SimpleClassName(this.GetType())));
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
			private readonly ReadOnlyDictionaryBase<TKey, TValue> myDictionary;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="myDictionary">The dictionary this is associated with.</param>
			public KeysCollection(ReadOnlyDictionaryBase<TKey, TValue> myDictionary)
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
		/// A private class that implements ICollection&lt;TKey&gt; and ICollection for the
		/// Values collection. The collection is read-only.
		/// </summary>
		[Serializable]
		private sealed class ValuesCollection : ReadOnlyCollectionBase<TValue>
		{
			private readonly ReadOnlyDictionaryBase<TKey, TValue> myDictionary;

			public ValuesCollection(ReadOnlyDictionaryBase<TKey, TValue> myDictionary)
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

		#endregion

		#region IDictionary Members

		/// <summary>
		/// Adds a key-value pair to the collection. Always throws an exception
		/// indicating that this method is not supported in a read-only dictionary.
		/// </summary>
		/// <param name="key">Key to add to the dictionary.</param>
		/// <param name="value">Value to add to the dictionary.</param>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		void IDictionary.Add(object key, object value)
		{
			MethodModifiesCollection();
		}

		/// <summary>
		/// Clears this dictionary. Always throws an exception
		/// indicating that this method is not supported in a read-only dictionary.
		/// </summary>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		void IDictionary.Clear()
		{
			MethodModifiesCollection();
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
		/// Removes the key (and associated value) from the collection that is equal to the passed in key. Always throws an exception
		/// indicating that this method is not supported in a read-only dictionary.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		void IDictionary.Remove(object key)
		{
			MethodModifiesCollection();
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
		/// Returns whether this dictionary is fixed size. 
		/// </summary>
		/// <value>Always returns true.</value>
		bool IDictionary.IsFixedSize
		{
			get { return true; }
		}

		/// <summary>
		/// Returns if this dictionary is read-only. 
		/// </summary>
		/// <value>Always returns true.</value>
		bool IDictionary.IsReadOnly
		{
			get { return true; }
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
		/// Gets the value associated with a given key. When getting a value, if this
		/// key is not found in the collection, then null is returned. If the key is not of the correct type 
		/// for this dictionary, null is returned.
		/// </summary>
		/// <value>The value associated with the key, or null if the key was not present.</value>
		/// <exception cref="NotSupportedException">Always thrown from the set accessor, indicating
		/// that the dictionary is read only.</exception>
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
			set { MethodModifiesCollection(); }
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

		#endregion
	}
}