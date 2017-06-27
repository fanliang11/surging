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
using System;

#pragma warning disable 419  // Ambigious cref in XML comment

namespace Magnum.Collections
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Runtime.CompilerServices;
	using System.Text;

	/// <summary>
	/// The BinaryPredicate delegate type  encapsulates a method that takes two
	/// items of the same type, and returns a boolean value representating 
	/// some relationship between them. For example, checking whether two
	/// items are equal or equivalent is one kind of binary predicate.
	/// </summary>
	/// <param name="item1">The first item.</param>
	/// <param name="item2">The second item.</param>
	/// <returns>Whether item1 and item2 satisfy the relationship that the BinaryPredicate defines.</returns>
	public delegate bool BinaryPredicate<T>(T item1, T item2);

	/// <summary>
	/// Algorithms contains a number of static methods that implement
	/// algorithms that work on collections. Most of the methods deal with
	/// the standard generic collection interfaces such as IEnumerable&lt;T&gt;,
	/// ICollection&lt;T&gt; and IList&lt;T&gt;.
	/// </summary>
	public static class Algorithms
	{
		#region Collection wrappers

		/// <summary>
		/// The class that is used to implement IList&lt;T&gt; to view a sub-range
		/// of a list. The object stores a wrapped list, and a start/count indicating
		/// a sub-range of the list. Insertion/deletions through the sub-range view
		/// cause the count to change also; insertions and deletions directly on
		/// the wrapped list do not.
		/// </summary>
		[Serializable]
		private class ListRange<T> : ListBase<T>,
			ICollection<T>
		{
			private readonly int start;
			private readonly IList<T> wrappedList;
			private int count;

			/// <summary>
			/// Create a sub-range view object on the indicate part 
			/// of the list.
			/// </summary>
			/// <param name="wrappedList">List to wrap.</param>
			/// <param name="start">The start index of the view in the wrapped list.</param>
			/// <param name="count">The number of items in the view.</param>
			public ListRange(IList<T> wrappedList, int start, int count)
			{
				this.wrappedList = wrappedList;
				this.start = start;
				this.count = count;
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

			public override int Count
			{
				get { return Math.Min(count, wrappedList.Count - start); }
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

			public override bool Remove(T item)
			{
				if (wrappedList.IsReadOnly)
					throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, "Range"));
				else
					return base.Remove(item);
			}

			bool ICollection<T>.IsReadOnly
			{
				get { return wrappedList.IsReadOnly; }
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
		}

		/// <summary>
		/// The class that is used to implement IList&lt;T&gt; to view a sub-range
		/// of an array. The object stores a wrapped array, and a start/count indicating
		/// a sub-range of the array. Insertion/deletions through the sub-range view
		/// cause the count to change up to the size of the underlying array. Elements
		/// fall off the end of the underlying array.
		/// </summary>
		[Serializable]
		private class ArrayRange<T> : ListBase<T>
		{
			private readonly int start;
			private readonly T[] wrappedArray;
			private int count;

			/// <summary>
			/// Create a sub-range view object on the indicate part 
			/// of the array.
			/// </summary>
			/// <param name="wrappedArray">Array to wrap.</param>
			/// <param name="start">The start index of the view in the wrapped list.</param>
			/// <param name="count">The number of items in the view.</param>
			public ArrayRange(T[] wrappedArray, int start, int count)
			{
				this.wrappedArray = wrappedArray;
				this.start = start;
				this.count = count;
			}

			public override int Count
			{
				get { return count; }
			}

			public override T this[int index]
			{
				get
				{
					if (index < 0 || index >= count)
						throw new ArgumentOutOfRangeException("index");

					return wrappedArray[start + index];
				}
				set
				{
					if (index < 0 || index >= count)
						throw new ArgumentOutOfRangeException("index");

					wrappedArray[start + index] = value;
				}
			}

			public override void Clear()
			{
				Array.Copy(wrappedArray, start + count, wrappedArray, start, wrappedArray.Length - (start + count));
				FillRange(wrappedArray, wrappedArray.Length - count, count, default(T));
				count = 0;
			}

			public override void Insert(int index, T item)
			{
				if (index < 0 || index > count)
					throw new ArgumentOutOfRangeException("index");

				int i = start + index;

				if (i + 1 < wrappedArray.Length)
					Array.Copy(wrappedArray, i, wrappedArray, i + 1, wrappedArray.Length - i - 1);
				if (i < wrappedArray.Length)
					wrappedArray[i] = item;

				if (start + count < wrappedArray.Length)
					++count;
			}

			public override void RemoveAt(int index)
			{
				if (index < 0 || index >= count)
					throw new ArgumentOutOfRangeException("index");

				int i = start + index;

				if (i < wrappedArray.Length - 1)
					Array.Copy(wrappedArray, i + 1, wrappedArray, i, wrappedArray.Length - i - 1);
				wrappedArray[wrappedArray.Length - 1] = default(T);

				--count;
			}
		}

		/// <summary>
		/// The read-only ICollection&lt;T&gt; implementation that is used by the ReadOnly method.
		/// Methods that modify the collection throw a NotSupportedException, methods that don't
		/// modify are fowarded through to the wrapped collection.
		/// </summary>
		[Serializable]
		private class ReadOnlyCollection<T> : ICollection<T>
		{
			private readonly ICollection<T> wrappedCollection; // The collection we are wrapping (never null).

			/// <summary>
			/// Create a ReadOnlyCollection wrapped around the given collection.
			/// </summary>
			/// <param name="wrappedCollection">Collection to wrap.</param>
			public ReadOnlyCollection(ICollection<T> wrappedCollection)
			{
				this.wrappedCollection = wrappedCollection;
			}


			public IEnumerator<T> GetEnumerator()
			{
				return wrappedCollection.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable) wrappedCollection).GetEnumerator();
			}

			public bool Contains(T item)
			{
				return wrappedCollection.Contains(item);
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				wrappedCollection.CopyTo(array, arrayIndex);
			}

			public int Count
			{
				get { return wrappedCollection.Count; }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public void Add(T item)
			{
				MethodModifiesCollection();
			}

			public void Clear()
			{
				MethodModifiesCollection();
			}

			public bool Remove(T item)
			{
				MethodModifiesCollection();
				return false;
			}

			/// <summary>
			/// Throws an NotSupportedException stating that this collection cannot be modified.
			/// </summary>
			private static void MethodModifiesCollection()
			{
				throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, "read-only collection"));
			}
		}

		/// <summary>
		/// The read-only IList&lt;T&gt; implementation that is used by the ReadOnly method.
		/// Methods that modify the list throw a NotSupportedException, methods that don't
		/// modify are fowarded through to the wrapped list.
		/// </summary>
		[Serializable]
		private class ReadOnlyList<T> : IList<T>
		{
			private readonly IList<T> wrappedList; // The list we are wrapping (never null).

			/// <summary>
			/// Create a ReadOnlyList wrapped around the given list.
			/// </summary>
			/// <param name="wrappedList">List to wrap.</param>
			public ReadOnlyList(IList<T> wrappedList)
			{
				this.wrappedList = wrappedList;
			}


			public IEnumerator<T> GetEnumerator()
			{
				return wrappedList.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable) wrappedList).GetEnumerator();
			}

			public int IndexOf(T item)
			{
				return wrappedList.IndexOf(item);
			}

			public bool Contains(T item)
			{
				return wrappedList.Contains(item);
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				wrappedList.CopyTo(array, arrayIndex);
			}

			public int Count
			{
				get { return wrappedList.Count; }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public T this[int index]
			{
				get { return wrappedList[index]; }
				set { MethodModifiesCollection(); }
			}

			public void Add(T item)
			{
				MethodModifiesCollection();
			}

			public void Clear()
			{
				MethodModifiesCollection();
			}

			public void Insert(int index, T item)
			{
				MethodModifiesCollection();
			}

			public void RemoveAt(int index)
			{
				MethodModifiesCollection();
			}

			public bool Remove(T item)
			{
				MethodModifiesCollection();
				return false;
			}

			/// <summary>
			/// Throws an NotSupportedException stating that this collection cannot be modified.
			/// </summary>
			private static void MethodModifiesCollection()
			{
				throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, "read-only list"));
			}
		}

		/// <summary>
		/// The private class that implements a read-only wrapped for 
		/// IDictionary &lt;TKey,TValue&gt;.
		/// </summary>
		[Serializable]
		private class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
		{
			// The dictionary that is wrapped
			private readonly IDictionary<TKey, TValue> wrappedDictionary;

			/// <summary>
			/// Create a read-only dictionary wrapped around the given dictionary.
			/// </summary>
			/// <param name="wrappedDictionary">The IDictionary&lt;TKey,TValue&gt; to wrap.</param>
			public ReadOnlyDictionary(IDictionary<TKey, TValue> wrappedDictionary)
			{
				this.wrappedDictionary = wrappedDictionary;
			}

			public void Add(TKey key, TValue value)
			{
				MethodModifiesCollection();
			}

			public bool ContainsKey(TKey key)
			{
				return wrappedDictionary.ContainsKey(key);
			}

			public ICollection<TKey> Keys
			{
				get { return ReadOnly(wrappedDictionary.Keys); }
			}

			public ICollection<TValue> Values
			{
				get { return ReadOnly(wrappedDictionary.Values); }
			}

			public bool Remove(TKey key)
			{
				MethodModifiesCollection();
				return false; // never reached
			}

			public bool TryGetValue(TKey key, out TValue value)
			{
				return wrappedDictionary.TryGetValue(key, out value);
			}

			public TValue this[TKey key]
			{
				get { return wrappedDictionary[key]; }
				set { MethodModifiesCollection(); }
			}

			public void Add(KeyValuePair<TKey, TValue> item)
			{
				MethodModifiesCollection();
			}

			public void Clear()
			{
				MethodModifiesCollection();
			}

			public bool Contains(KeyValuePair<TKey, TValue> item)
			{
				return wrappedDictionary.Contains(item);
			}

			public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
			{
				wrappedDictionary.CopyTo(array, arrayIndex);
			}

			public int Count
			{
				get { return wrappedDictionary.Count; }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public bool Remove(KeyValuePair<TKey, TValue> item)
			{
				MethodModifiesCollection();
				return false; // never reached
			}

			public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			{
				return wrappedDictionary.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable) wrappedDictionary).GetEnumerator();
			}

			/// <summary>
			/// Throws an NotSupportedException stating that this collection cannot be modified.
			/// </summary>
			private static void MethodModifiesCollection()
			{
				throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, "read-only dictionary"));
			}
		}

		/// <summary>
		///  The class that provides a typed IEnumerator&lt;T&gt;
		/// view onto an untyped IEnumerator interface.
		/// </summary>
		[Serializable]
		private class TypedEnumerator<T> : IEnumerator<T>
		{
			private readonly IEnumerator wrappedEnumerator;

			/// <summary>
			/// Create a typed IEnumerator&lt;T&gt;
			/// view onto an untyped IEnumerator interface 
			/// </summary>
			/// <param name="wrappedEnumerator">IEnumerator to wrap.</param>
			public TypedEnumerator(IEnumerator wrappedEnumerator)
			{
				this.wrappedEnumerator = wrappedEnumerator;
			}

			T IEnumerator<T>.Current
			{
				get { return (T) wrappedEnumerator.Current; }
			}

			void IDisposable.Dispose()
			{
				if (wrappedEnumerator is IDisposable)
					((IDisposable) wrappedEnumerator).Dispose();
			}

			object IEnumerator.Current
			{
				get { return wrappedEnumerator.Current; }
			}

			bool IEnumerator.MoveNext()
			{
				return wrappedEnumerator.MoveNext();
			}

			void IEnumerator.Reset()
			{
				wrappedEnumerator.Reset();
			}
		}

		/// <summary>
		/// The class that provides a typed IEnumerable&lt;T&gt; view
		/// onto an untyped IEnumerable interface.
		/// </summary>
		[Serializable]
		private class TypedEnumerable<T> : IEnumerable<T>
		{
			private readonly IEnumerable wrappedEnumerable;

			/// <summary>
			/// Create a typed IEnumerable&lt;T&gt; view
			/// onto an untyped IEnumerable interface.
			/// </summary>
			/// <param name="wrappedEnumerable">IEnumerable interface to wrap.</param>
			public TypedEnumerable(IEnumerable wrappedEnumerable)
			{
				this.wrappedEnumerable = wrappedEnumerable;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return new TypedEnumerator<T>(wrappedEnumerable.GetEnumerator());
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return wrappedEnumerable.GetEnumerator();
			}
		}

		/// <summary>
		/// The class that provides a typed ICollection&lt;T&gt; view
		/// onto an untyped ICollection interface. The ICollection&lt;T&gt;
		/// is read-only.
		/// </summary>
		[Serializable]
		private class TypedCollection<T> : ICollection<T>
		{
			private readonly ICollection wrappedCollection;

			/// <summary>
			/// Create a typed ICollection&lt;T&gt; view
			/// onto an untyped ICollection interface.
			/// </summary>
			/// <param name="wrappedCollection">ICollection interface to wrap.</param>
			public TypedCollection(ICollection wrappedCollection)
			{
				this.wrappedCollection = wrappedCollection;
			}

			public void Add(T item)
			{
				MethodModifiesCollection();
			}

			public void Clear()
			{
				MethodModifiesCollection();
			}

			public bool Remove(T item)
			{
				MethodModifiesCollection();
				return false;
			}

			public bool Contains(T item)
			{
				IEqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
				foreach (object obj in wrappedCollection)
				{
					if (obj is T && equalityComparer.Equals(item, (T) obj))
						return true;
				}
				return false;
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				wrappedCollection.CopyTo(array, arrayIndex);
			}

			public int Count
			{
				get { return wrappedCollection.Count; }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public IEnumerator<T> GetEnumerator()
			{
				return new TypedEnumerator<T>(wrappedCollection.GetEnumerator());
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return wrappedCollection.GetEnumerator();
			}

			/// <summary>
			/// Throws an NotSupportedException stating that this collection cannot be modified.
			/// </summary>
			private static void MethodModifiesCollection()
			{
				throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, "strongly-typed Collection"));
			}
		}

		/// <summary>
		/// The class used to create a typed IList&lt;T&gt; view onto
		/// an untype IList interface.
		/// </summary>
		[Serializable]
		private class TypedList<T> : IList<T>
		{
			private readonly IList wrappedList;

			/// <summary>
			/// Create a typed IList&lt;T&gt; view onto
			/// an untype IList interface.
			/// </summary>
			/// <param name="wrappedList">The IList to wrap.</param>
			public TypedList(IList wrappedList)
			{
				this.wrappedList = wrappedList;
			}


			public IEnumerator<T> GetEnumerator()
			{
				return new TypedEnumerator<T>(wrappedList.GetEnumerator());
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return wrappedList.GetEnumerator();
			}

			public int IndexOf(T item)
			{
				return wrappedList.IndexOf(item);
			}

			public void Insert(int index, T item)
			{
				wrappedList.Insert(index, item);
			}

			public void RemoveAt(int index)
			{
				wrappedList.RemoveAt(index);
			}

			public void Add(T item)
			{
				wrappedList.Add(item);
			}

			public void Clear()
			{
				wrappedList.Clear();
			}

			public bool Contains(T item)
			{
				return wrappedList.Contains(item);
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				wrappedList.CopyTo(array, arrayIndex);
			}

			public T this[int index]
			{
				get { return (T) wrappedList[index]; }
				set { wrappedList[index] = value; }
			}

			public int Count
			{
				get { return wrappedList.Count; }
			}

			public bool IsReadOnly
			{
				get { return wrappedList.IsReadOnly; }
			}

			public bool Remove(T item)
			{
				if (wrappedList.Contains(item))
				{
					wrappedList.Remove(item);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// The class that is used to provide an untyped ICollection
		/// view onto a typed ICollection&lt;T&gt; interface.
		/// </summary>
		[Serializable]
		private class UntypedCollection<T> : ICollection
		{
			private readonly ICollection<T> wrappedCollection;

			/// <summary>
			/// Create an untyped ICollection
			/// view onto a typed ICollection&lt;T&gt; interface.
			/// </summary>
			/// <param name="wrappedCollection">The ICollection&lt;T&gt; to wrap.</param>
			public UntypedCollection(ICollection<T> wrappedCollection)
			{
				this.wrappedCollection = wrappedCollection;
			}


			public void CopyTo(Array array, int index)
			{
				if (array == null)
					throw new ArgumentNullException("array");

				int i = 0;
				int count = wrappedCollection.Count;

				if (index < 0)
					throw new ArgumentOutOfRangeException("index", index, Strings.ArgMustNotBeNegative);
				if (index >= array.Length || count > array.Length - index)
					throw new ArgumentException("index", Strings.ArrayTooSmall);

				foreach (T item in wrappedCollection)
				{
					if (i >= count)
						break;

					array.SetValue(item, index);
					++index;
					++i;
				}
			}

			public int Count
			{
				get { return wrappedCollection.Count; }
			}

			public bool IsSynchronized
			{
				get { return false; }
			}

			public object SyncRoot
			{
				get { return this; }
			}

			public IEnumerator GetEnumerator()
			{
				return ((IEnumerable) wrappedCollection).GetEnumerator();
			}
		}

		/// <summary>
		/// The class that implements a non-generic IList wrapper
		/// around a generic IList&lt;T&gt; interface.
		/// </summary>
		[Serializable]
		private class UntypedList<T> : IList
		{
			private readonly IList<T> wrappedList;

			/// <summary>
			/// Create a non-generic IList wrapper
			/// around a generic IList&lt;T&gt; interface.
			/// </summary>
			/// <param name="wrappedList">The IList&lt;T&gt; interface to wrap.</param>
			public UntypedList(IList<T> wrappedList)
			{
				this.wrappedList = wrappedList;
			}


			public int Add(object value)
			{
				// We assume that Add always adds to the end. Is this true?
				wrappedList.Add(ConvertToItemType("value", value));
				return wrappedList.Count - 1;
			}

			public void Clear()
			{
				wrappedList.Clear();
			}

			public bool Contains(object value)
			{
				if (value is T)
					return wrappedList.Contains((T) value);
				else
					return false;
			}

			public int IndexOf(object value)
			{
				if (value is T)
					return wrappedList.IndexOf((T) value);
				else
					return -1;
			}

			public void Insert(int index, object value)
			{
				wrappedList.Insert(index, ConvertToItemType("value", value));
			}

			public bool IsFixedSize
			{
				get { return false; }
			}

			public bool IsReadOnly
			{
				get { return wrappedList.IsReadOnly; }
			}

			public void Remove(object value)
			{
				if (value is T)
					wrappedList.Remove((T) value);
			}

			public void RemoveAt(int index)
			{
				wrappedList.RemoveAt(index);
			}

			public object this[int index]
			{
				get { return wrappedList[index]; }
				set { wrappedList[index] = ConvertToItemType("value", value); }
			}

			public void CopyTo(Array array, int index)
			{
				if (array == null)
					throw new ArgumentNullException("array");

				int i = 0;
				int count = wrappedList.Count;

				if (index < 0)
					throw new ArgumentOutOfRangeException("index", index, Strings.ArgMustNotBeNegative);
				if (index >= array.Length || count > array.Length - index)
					throw new ArgumentException("index", Strings.ArrayTooSmall);

				foreach (T item in wrappedList)
				{
					if (i >= count)
						break;

					array.SetValue(item, index);
					++index;
					++i;
				}
			}

			public int Count
			{
				get { return wrappedList.Count; }
			}

			public bool IsSynchronized
			{
				get { return false; }
			}

			public object SyncRoot
			{
				get { return this; }
			}

			public IEnumerator GetEnumerator()
			{
				return ((IEnumerable) wrappedList).GetEnumerator();
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

		/// <summary>
		/// The class that is used to implement IList&lt;T&gt; to view an array
		/// in a read-write way. Insertions cause the last item in the array
		/// to fall off, deletions replace the last item with the default value.
		/// </summary>
		[Serializable]
		private class ArrayWrapper<T> : ListBase<T>,
			IList
		{
			private readonly T[] wrappedArray;

			/// <summary>
			/// Create a list wrapper object on an array.
			/// </summary>
			/// <param name="wrappedArray">Array to wrap.</param>
			public ArrayWrapper(T[] wrappedArray)
			{
				this.wrappedArray = wrappedArray;
			}

			public override T this[int index]
			{
				get
				{
					if (index < 0 || index >= wrappedArray.Length)
						throw new ArgumentOutOfRangeException("index");

					return wrappedArray[index];
				}
				set
				{
					if (index < 0 || index >= wrappedArray.Length)
						throw new ArgumentOutOfRangeException("index");

					wrappedArray[index] = value;
				}
			}

			public override int Count
			{
				get { return wrappedArray.Length; }
			}

			public override void Clear()
			{
				int count = wrappedArray.Length;
				for (int i = 0; i < count; ++i)
					wrappedArray[i] = default(T);
			}

			public override void RemoveAt(int index)
			{
				if (index < 0 || index >= wrappedArray.Length)
					throw new ArgumentOutOfRangeException("index");

				if (index < wrappedArray.Length - 1)
					Array.Copy(wrappedArray, index + 1, wrappedArray, index, wrappedArray.Length - index - 1);
				wrappedArray[wrappedArray.Length - 1] = default(T);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IList) wrappedArray).GetEnumerator();
			}

			/// <summary>
			/// Return true, to indicate that the list is fixed size.
			/// </summary>
			bool IList.IsFixedSize
			{
				get { return true; }
			}

			public override void Insert(int index, T item)
			{
				if (index < 0 || index > wrappedArray.Length)
					throw new ArgumentOutOfRangeException("index");

				if (index + 1 < wrappedArray.Length)
					Array.Copy(wrappedArray, index, wrappedArray, index + 1, wrappedArray.Length - index - 1);
				if (index < wrappedArray.Length)
					wrappedArray[index] = item;
			}

			public override void CopyTo(T[] array, int arrayIndex)
			{
				if (array == null)
					throw new ArgumentNullException("array");
				if (array.Length < wrappedArray.Length)
					throw new ArgumentException("array is too short", "array");
				if (arrayIndex < 0 || arrayIndex >= array.Length)
					throw new ArgumentOutOfRangeException("arrayIndex");
				if (array.Length + arrayIndex < wrappedArray.Length)
					throw new ArgumentOutOfRangeException("arrayIndex");

				Array.Copy(wrappedArray, 0, array, arrayIndex, wrappedArray.Length);
			}

			public override IEnumerator<T> GetEnumerator()
			{
				return ((IList<T>) wrappedArray).GetEnumerator();
			}
		}

		/// <summary>
		/// Returns a view onto a sub-range of a list. Items from <paramref name="list"/> are not copied; the
		/// returned IList&lt;T&gt; is simply a different view onto the same underlying items. Changes to <paramref name="list"/>
		/// are reflected in the view, and vice versa. Insertions and deletions in the view change the size of the 
		/// view, but insertions and deletions in the underlying list do not.
		/// </summary>
		/// <remarks>This method can be used to apply an algorithm to a portion of a list. For example:
		/// <code>Algorithms.ReverseInPlace(Algorithms.Range(list, 3, 6))</code>
		/// will reverse the 6 items beginning at index 3.</remarks>
		/// <typeparam name="T">The type of the items in the list.</typeparam>
		/// <param name="list">The list to view.</param>
		/// <param name="start">The starting index of the view.</param>
		/// <param name="count">The number of items in the view.</param>
		/// <returns>A list that is a view onto the given sub-list. </returns>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> + <paramref name="count"/> is greater than the
		/// size of <paramref name="list"/>.</exception>
		public static IList<T> Range<T>(IList<T> list, int start, int count)
		{
			if (list == null)
				throw new ArgumentOutOfRangeException("list");
			if (start < 0 || start > list.Count || (start == list.Count && count != 0))
				throw new ArgumentOutOfRangeException("start");
			if (count < 0 || count > list.Count || count + start > list.Count)
				throw new ArgumentOutOfRangeException("count");

			return new ListRange<T>(list, start, count);
		}

		/// <summary>
		/// Returns a view onto a sub-range of an array. Items from <paramref name="array"/> are not copied; the
		/// returned IList&lt;T&gt; is simply a different view onto the same underlying items. Changes to <paramref name="array"/>
		/// are reflected in the view, and vice versa. Insertions and deletions in the view change the size of the 
		/// view. After an insertion, the last item in <paramref name="array"/> "falls off the end". After a deletion, the
		/// last item in array becomes the default value (0 or null).
		/// </summary>
		/// <remarks>This method can be used to apply an algorithm to a portion of a array. For example:
		/// <code>Algorithms.ReverseInPlace(Algorithms.Range(array, 3, 6))</code>
		/// will reverse the 6 items beginning at index 3.</remarks>
		/// <param name="array">The array to view.</param>
		/// <param name="start">The starting index of the view.</param>
		/// <param name="count">The number of items in the view.</param>
		/// <returns>A list that is a view onto the given sub-array. </returns>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> + <paramref name="count"/> is greater than the
		/// size of <paramref name="array"/>.</exception>
		public static IList<T> Range<T>(T[] array, int start, int count)
		{
			if (array == null)
				throw new ArgumentOutOfRangeException("array");
			if (start < 0 || start > array.Length || (start == array.Length && count != 0))
				throw new ArgumentOutOfRangeException("start");
			if (count < 0 || count > array.Length || count + start > array.Length)
				throw new ArgumentOutOfRangeException("count");

			return new ArrayRange<T>(array, start, count);
		}

		/// <summary>
		/// Returns a read-only view onto a collection. The returned ICollection&lt;T&gt; interface
		/// only allows operations that do not change the collection: GetEnumerator, Contains, CopyTo,
		/// Count. The ReadOnly property returns false, indicating that the collection is read-only. All other
		/// methods on the interface throw a NotSupportedException.
		/// </summary>
		/// <remarks>The data in the underlying collection is not copied. If the underlying
		/// collection is changed, then the read-only view also changes accordingly.</remarks>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The collection to wrap.</param>
		/// <returns>A read-only view onto <paramref name="collection"/>. If <paramref name="collection"/> is null, then null is returned.</returns>
		public static ICollection<T> ReadOnly<T>(ICollection<T> collection)
		{
			if (collection == null)
				return null;
			else
				return new ReadOnlyCollection<T>(collection);
		}

		/// <summary>
		/// Returns a read-only view onto a list. The returned IList&lt;T&gt; interface
		/// only allows operations that do not change the list: GetEnumerator, Contains, CopyTo,
		/// Count, IndexOf, and the get accessor of the indexer. 
		/// The IsReadOnly property returns true, indicating that the list is read-only. All other
		/// methods on the interface throw a NotSupportedException.
		/// </summary>
		/// <remarks>The data in the underlying list is not copied. If the underlying
		/// list is changed, then the read-only view also changes accordingly.</remarks>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to wrap.</param>
		/// <returns>A read-only view onto <paramref name="list"/>. Returns null if <paramref name="list"/> is null. 
		/// If <paramref name="list"/> is already read-only, returns <paramref name="list"/>.</returns>
		public static IList<T> ReadOnly<T>(IList<T> list)
		{
			if (list == null)
				return null;
			else if (list.IsReadOnly)
				return list;
			else
				return new ReadOnlyList<T>(list);
		}

		/// <summary>
		/// Returns a read-only view onto a dictionary. The returned IDictionary&lt;TKey,TValue&gt; interface
		/// only allows operations that do not change the dictionary. 
		/// The IsReadOnly property returns true, indicating that the dictionary is read-only. All other
		/// methods on the interface throw a NotSupportedException.
		/// </summary>
		/// <remarks>The data in the underlying dictionary is not copied. If the underlying
		/// dictionary is changed, then the read-only view also changes accordingly.</remarks>
		/// <param name="dictionary">The dictionary to wrap.</param>
		/// <returns>A read-only view onto <paramref name="dictionary"/>. Returns null if <paramref name="dictionary"/> is null. 
		/// If <paramref name="dictionary"/> is already read-only, returns <paramref name="dictionary"/>.</returns>
		public static IDictionary<TKey, TValue> ReadOnly<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null)
				return null;
			else if (dictionary.IsReadOnly)
				return dictionary;
			else
				return new ReadOnlyDictionary<TKey, TValue>(dictionary);
		}

		/// <summary>
		/// Given a non-generic IEnumerable interface, wrap a generic IEnumerable&lt;T&gt;
		/// interface around it. The generic interface will enumerate the same objects as the 
		/// underlying non-generic collection, but can be used in places that require a generic interface.
		/// The underlying non-generic collection must contain only items that
		/// are of type <typeparamref name="T"/> or a type derived from it. This method is useful
		/// when interfacing older, non-generic collections to newer code that uses generic interfaces.
		/// </summary>
		/// <remarks>Some collections implement both generic and non-generic interfaces. For efficiency,
		/// this method will first attempt to cast <paramref name="untypedCollection"/> to IEnumerable&lt;T&gt;. 
		/// If that succeeds, it is returned; otherwise, a wrapper object is created.</remarks>
		/// <typeparam name="T">The item type of the wrapper collection.</typeparam>
		/// <param name="untypedCollection">An untyped collection. This collection should only contain
		/// items of type <typeparamref name="T"/> or a type derived from it. </param>
		/// <returns>A generic IEnumerable&lt;T&gt; wrapper around <paramref name="untypedCollection"/>. 
		/// If <paramref name="untypedCollection"/> is null, then null is returned.</returns>
		public static IEnumerable<T> TypedAs<T>(IEnumerable untypedCollection)
		{
			if (untypedCollection == null)
				return null;
			else if (untypedCollection is IEnumerable<T>)
				return (IEnumerable<T>) untypedCollection;
			else
				return new TypedEnumerable<T>(untypedCollection);
		}

		/// <summary>
		/// Given a non-generic ICollection interface, wrap a generic ICollection&lt;T&gt;
		/// interface around it. The generic interface will enumerate the same objects as the 
		/// underlying non-generic collection, but can be used in places that require a generic interface.
		/// The underlying non-generic collection must contain only items that
		/// are of type <typeparamref  name="T"/> or a type derived from it. This method is useful
		/// when interfacing older, non-generic collections to newer code that uses generic interfaces.
		/// </summary>
		/// <remarks><para>Some collections implement both generic and non-generic interfaces. For efficiency,
		/// this method will first attempt to cast <paramref name="untypedCollection"/> to ICollection&lt;T&gt;. 
		/// If that succeeds, it is returned; otherwise, a wrapper object is created.</para>
		/// <para>Unlike the generic interface, the non-generic ICollection interfaces does
		/// not contain methods for adding or removing items from the collection. For this reason,
		/// the returned ICollection&lt;T&gt; will be read-only.</para></remarks>
		/// <typeparam  name="T">The item type of the wrapper collection.</typeparam>
		/// <param name="untypedCollection">An untyped collection. This collection should only contain
		/// items of type <typeparamref  name="T"/> or a type derived from it. </param>
		/// <returns>A generic ICollection&lt;T&gt; wrapper around <paramref name="untypedCollection"/>.
		/// If <paramref name="untypedCollection"/> is null, then null is returned.</returns>
		public static ICollection<T> TypedAs<T>(ICollection untypedCollection)
		{
			if (untypedCollection == null)
				return null;
			else if (untypedCollection is ICollection<T>)
				return (ICollection<T>) untypedCollection;
			else
				return new TypedCollection<T>(untypedCollection);
		}

		/// <summary>
		/// Given a non-generic IList interface, wrap a generic IList&lt;T&gt;
		/// interface around it. The generic interface will enumerate the same objects as the 
		/// underlying non-generic list, but can be used in places that require a generic interface.
		/// The underlying non-generic list must contain only items that
		/// are of type <typeparamref name="T"/> or a type derived from it. This method is useful
		/// when interfacing older, non-generic lists to newer code that uses generic interfaces.
		/// </summary>
		/// <remarks>Some collections implement both generic and non-generic interfaces. For efficiency,
		/// this method will first attempt to cast <paramref name="untypedList"/> to IList&lt;T&gt;. 
		/// If that succeeds, it is returned; otherwise, a wrapper object is created.</remarks>
		/// <typeparam name="T">The item type of the wrapper list.</typeparam>
		/// <param name="untypedList">An untyped list. This list should only contain
		/// items of type <typeparamref name="T"/> or a type derived from it. </param>
		/// <returns>A generic IList&lt;T&gt; wrapper around <paramref name="untypedList"/>.
		/// If <paramref name="untypedList"/> is null, then null is returned.</returns>
		public static IList<T> TypedAs<T>(IList untypedList)
		{
			if (untypedList == null)
				return null;
			else if (untypedList is IList<T>)
				return (IList<T>) untypedList;
			else
				return new TypedList<T>(untypedList);
		}

		/// <summary>
		/// Given a generic ICollection&lt;T&gt; interface, wrap a non-generic (untyped)
		/// ICollection interface around it. The non-generic interface will contain the same objects as the 
		/// underlying generic collection, but can be used in places that require a non-generic interface.
		/// This method is useful when interfacing generic interfaces with older code that uses non-generic interfaces.
		/// </summary>
		/// <remarks>Many generic collections already implement the non-generic interfaces directly. This
		/// method will first attempt to simply cast <paramref name="typedCollection"/> to ICollection. If that
		/// succeeds, it is returned; if it fails, then a wrapper object is created.</remarks>
		/// <typeparam name="T">The item type of the underlying collection.</typeparam>
		/// <param name="typedCollection">A typed collection to wrap.</param>
		/// <returns>A non-generic ICollection wrapper around <paramref name="typedCollection"/>.
		/// If <paramref name="typedCollection"/> is null, then null is returned.</returns>
		public static ICollection Untyped<T>(ICollection<T> typedCollection)
		{
			if (typedCollection == null)
				return null;
			else if (typedCollection is ICollection)
				return (ICollection) typedCollection;
			else
				return new UntypedCollection<T>(typedCollection);
		}

		/// <summary>
		/// Given a generic IList&lt;T&gt; interface, wrap a non-generic (untyped)
		/// IList interface around it. The non-generic interface will contain the same objects as the 
		/// underlying generic list, but can be used in places that require a non-generic interface.
		/// This method is useful when interfacing generic interfaces with older code that uses non-generic interfaces.
		/// </summary>
		/// <remarks>Many generic collections already implement the non-generic interfaces directly. This
		/// method will first attempt to simply cast <paramref name="typedList"/> to IList. If that
		/// succeeds, it is returned; if it fails, then a wrapper object is created.</remarks>
		/// <typeparam name="T">The item type of the underlying list.</typeparam>
		/// <param name="typedList">A typed list to wrap.</param>
		/// <returns>A non-generic IList wrapper around <paramref name="typedList"/>.
		/// If <paramref name="typedList"/> is null, then null is returned.</returns>
		public static IList Untyped<T>(IList<T> typedList)
		{
			if (typedList == null)
				return null;
			else if (typedList is IList)
				return (IList) typedList;
			else
				return new UntypedList<T>(typedList);
		}

		/// <summary>
		/// <para>Creates a read-write IList&lt;T&gt; wrapper around an array. When an array is
		/// implicitely converted to an IList&lt;T&gt;, changes to the items in the array cannot
		/// be made through the interface. This method creates a read-write IList&lt;T&gt; wrapper
		/// on an array that can be used to make changes to the array. </para>
		/// <para>Use this method when you need to pass an array to an algorithms that takes an 
		/// IList&lt;T&gt; and that tries to modify items in the list. Algorithms in this class generally do not
		/// need this method, since they have been design to operate on arrays even when they
		/// are passed as an IList&lt;T&gt;.</para>
		/// </summary>
		/// <remarks>Since arrays cannot be resized, inserting an item causes the last item in the array to be automatically
		/// removed. Removing an item causes the last item in the array to be replaced with a default value (0 or null). Clearing
		/// the list causes all the items to be replaced with a default value.</remarks>
		/// <param name="array">The array to wrap.</param>
		/// <returns>An IList&lt;T&gt; wrapper onto <paramref name="array"/>.</returns>
		public static IList<T> ReadWriteList<T>(T[] array)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			return new ArrayWrapper<T>(array);
		}

		#endregion Collection wrappers

		#region Replacing

		/// <summary>
		/// Replace all items in a collection equal to a particular value with another values, yielding another collection.
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <param name="collection">The collection to process.</param>
		/// <param name="itemFind">The value to find and replace within <paramref name="collection"/>.</param>
		/// <param name="replaceWith">The new value to replace with.</param>
		/// <returns>An new collection with the items from <paramref name="collection"/>, in the same order, 
		/// with the appropriate replacements made.</returns>
		public static IEnumerable<T> Replace<T>(IEnumerable<T> collection, T itemFind, T replaceWith)
		{
			return Replace(collection, itemFind, replaceWith, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Replace all items in a collection equal to a particular value with another values, yielding another collection. A passed
		/// IEqualityComparer is used to determine equality.
		/// </summary>
		/// <param name="collection">The collection to process.</param>
		/// <param name="itemFind">The value to find and replace within <paramref name="collection"/>.</param>
		/// <param name="replaceWith">The new value to replace with.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. Only the Equals method will be called.</param>
		/// <returns>An new collection with the items from <paramref name="collection"/>, in the same order, 
		/// with the appropriate replacements made.</returns>
		public static IEnumerable<T> Replace<T>(IEnumerable<T> collection, T itemFind, T replaceWith, IEqualityComparer<T> equalityComparer)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			foreach (T item in collection)
			{
				if (equalityComparer.Equals(item, itemFind))
					yield return replaceWith;
				else
					yield return item;
			}
		}

		/// <summary>
		/// Replace all items in a collection that a predicate evalues at true with a value, yielding another collection. .
		/// </summary>
		/// <param name="collection">The collection to process.</param>
		/// <param name="predicate">The predicate used to evaluate items with the collection. If the predicate returns true for a particular
		/// item, the item is replaces with <paramref name="replaceWith"/>.</param>
		/// <param name="replaceWith">The new value to replace with.</param>
		/// <returns>An new collection with the items from <paramref name="collection"/>, in the same order, 
		/// with the appropriate replacements made.</returns>
		public static IEnumerable<T> Replace<T>(IEnumerable<T> collection, Predicate<T> predicate, T replaceWith)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			foreach (T item in collection)
			{
				if (predicate(item))
					yield return replaceWith;
				else
					yield return item;
			}
		}

		/// <summary>
		/// Replace all items in a list or array equal to a particular value with another value. The replacement is done in-place, changing
		/// the list.
		/// </summary>
		/// <remarks><para>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</para>
		/// <para>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</para></remarks>
		/// <param name="list">The list or array to process.</param>
		/// <param name="itemFind">The value to find and replace within <paramtype name="T"/>.</param>
		/// <param name="replaceWith">The new value to replace with.</param>
		public static void ReplaceInPlace<T>(IList<T> list, T itemFind, T replaceWith)
		{
			ReplaceInPlace(list, itemFind, replaceWith, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Replace all items in a list or array equal to a particular value with another values.
		/// The replacement is done in-place, changing
		/// the list. A passed IEqualityComparer is used to determine equality.
		/// </summary>
		/// <remarks>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</remarks>
		/// <param name="list">The list or array to process.</param>
		/// <param name="itemFind">The value to find and replace within <paramtype name="T"/>.</param>
		/// <param name="replaceWith">The new value to replace with.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. Only the Equals method will be called.</param>
		public static void ReplaceInPlace<T>(IList<T> list, T itemFind, T replaceWith, IEqualityComparer<T> equalityComparer)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");
			if (list is T[])
				list = new ArrayWrapper<T>((T[]) list);
			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			int listCount = list.Count;
			for (int index = 0; index < listCount; ++index)
			{
				if (equalityComparer.Equals(list[index], itemFind))
					list[index] = replaceWith;
			}
		}

		/// <summary>
		/// Replace all items in a list or array that a predicate evaluates at true with a value. The replacement is done in-place, changing
		/// the list.
		/// </summary>
		/// <remarks>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</remarks>
		/// <param name="list">The list or array to process.</param>
		/// <param name="predicate">The predicate used to evaluate items with the collection. If the predicate returns true for a particular
		/// item, the item is replaces with <paramref name="replaceWith"/>.</param>
		/// <param name="replaceWith">The new value to replace with.</param>
		public static void ReplaceInPlace<T>(IList<T> list, Predicate<T> predicate, T replaceWith)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (predicate == null)
				throw new ArgumentNullException("predicate");
			if (list is T[])
				list = new ArrayWrapper<T>((T[]) list);
			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			int listCount = list.Count;
			for (int index = 0; index < listCount; ++index)
			{
				if (predicate(list[index]))
					list[index] = replaceWith;
			}
		}

		#endregion Replacing

		#region Consecutive items

		/// <summary>
		/// Remove consecutive equal items from a collection, yielding another collection. In each run of consecutive equal items
		/// in the collection, all items after the first item in the run are removed. 
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <param name="collection">The collection to process.</param>
		/// <returns>An new collection with the items from <paramref name="collection"/>, in the same order, 
		/// with consecutive duplicates removed.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public static IEnumerable<T> RemoveDuplicates<T>(IEnumerable<T> collection)
		{
			return RemoveDuplicates(collection, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Remove consecutive equal items from a collection, yielding another collection. In each run of consecutive equal items
		/// in the collection, all items after the first item in the run are removed. A passed
		/// IEqualityComparer is used to determine equality.
		/// </summary>
		/// <param name="collection">The collection to process.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. Only the Equals method will be called.</param>
		/// <returns>An new collection with the items from <paramref name="collection"/>, in the same order, 
		/// with consecutive duplicates removed.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> or <paramref name="equalityComparer"/> is null.</exception>
		public static IEnumerable<T> RemoveDuplicates<T>(IEnumerable<T> collection, IEqualityComparer<T> equalityComparer)
		{
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			return RemoveDuplicates(collection, equalityComparer.Equals);
		}

		/// <summary>
		/// Remove consecutive "equal" items from a collection, yielding another collection. In each run of consecutive equal items
		/// in the collection, all items after the first item in the run are removed. The passed 
		/// BinaryPredicate is used to determine if two items are "equal".
		/// </summary>
		/// <remarks>Since an arbitrary BinaryPredicate is passed to this function, what is being removed need not be true equality. </remarks>
		/// <param name="collection">The collection to process.</param>
		/// <param name="predicate">The BinaryPredicate used to compare items for "equality". An item <c>current</c> is removed if <c>predicate(first, current)==true</c>, where
		/// <c>first</c> is the first item in the group of "duplicate" items.</param>
		/// <returns>An new collection with the items from <paramref name="collection"/>, in the same order, 
		/// with consecutive "duplicates" removed.</returns>
		public static IEnumerable<T> RemoveDuplicates<T>(IEnumerable<T> collection, BinaryPredicate<T> predicate)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			T current = default(T);
			bool atBeginning = true;

			foreach (T item in collection)
			{
				// Is the new item different from the current item?
				if (atBeginning || !predicate(current, item))
				{
					current = item;
					yield return item;
				}

				atBeginning = false;
			}
		}

		/// <summary>
		/// Remove consecutive equal items from a list or array. In each run of consecutive equal items
		/// in the list, all items after the first item in the run are removed. The removal is done in-place, changing
		/// the list. 
		/// </summary>
		/// <remarks><para>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</para>
		/// <para>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</para></remarks>
		/// <param name="list">The list or array to process.</param>
		public static void RemoveDuplicatesInPlace<T>(IList<T> list)
		{
			RemoveDuplicatesInPlace(list, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Remove subsequent consecutive equal items from a list or array. In each run of consecutive equal items
		/// in the list, all items after the first item in the run are removed.
		/// The replacement is done in-place, changing
		/// the list. A passed IEqualityComparer is used to determine equality.
		/// </summary>
		/// <remarks>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</remarks>
		/// <param name="list">The list or array to process.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. Only the Equals method will be called.</param>
		public static void RemoveDuplicatesInPlace<T>(IList<T> list, IEqualityComparer<T> equalityComparer)
		{
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			RemoveDuplicatesInPlace(list, equalityComparer.Equals);
		}

		/// <summary>
		/// Remove consecutive "equal" items from a list or array. In each run of consecutive equal items
		/// in the list, all items after the first item in the run are removed. The replacement is done in-place, changing
		/// the list. The passed BinaryPredicate is used to determine if two items are "equal".
		/// </summary>
		/// <remarks><para>Since an arbitrary BinaryPredicate is passed to this function, what is being tested for need not be true equality. </para>
		/// <para>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</para></remarks>
		/// <param name="list">The list or array to process.</param>
		/// <param name="predicate">The BinaryPredicate used to compare items for "equality". </param>
		public static void RemoveDuplicatesInPlace<T>(IList<T> list, BinaryPredicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			if (list is T[])
				list = new ArrayWrapper<T>((T[]) list);
			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			T current = default(T);
			T item;
			int i = -1, j = 0;
			int listCount = list.Count;

			// Remove duplicates, compressing items to lower in the list.
			while (j < listCount)
			{
				item = list[j];
				if (i < 0 || !predicate(current, item))
				{
					current = item;
					++i;
					if (i != j)
						list[i] = current;
				}
				++j;
			}

			++i;
			if (i < listCount)
			{
				// remove items from the end.
				if (list is ArrayWrapper<T> || (list is IList && ((IList) list).IsFixedSize))
				{
					// An array or similar. Null out the last elements.
					while (i < listCount)
						list[i++] = default(T);
				}
				else
				{
					// Normal list.
					while (i < listCount)
					{
						list.RemoveAt(listCount - 1);
						--listCount;
					}
				}
			}
		}

		/// <summary>
		/// Finds the first occurence of <paramref name="count"/> consecutive equal items in the
		/// list.
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <param name="list">The list to examine.</param>
		/// <param name="count">The number of consecutive equal items to look for. The count must be at least 1.</param>
		/// <returns>The index of the first item in the first run of <paramref name="count"/> consecutive equal items, or -1 if no such run exists..</returns>
		public static int FirstConsecutiveEqual<T>(IList<T> list, int count)
		{
			return FirstConsecutiveEqual(list, count, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Finds the first occurence of <paramref name="count"/> consecutive equal items in the
		/// list. A passed IEqualityComparer is used to determine equality.
		/// </summary>
		/// <param name="list">The list to examine.</param>
		/// <param name="count">The number of consecutive equal items to look for. The count must be at least 1.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. Only the Equals method will be called.</param>
		/// <returns>The index of the first item in the first run of <paramref name="count"/> consecutive equal items, or -1 if no such run exists.</returns>
		public static int FirstConsecutiveEqual<T>(IList<T> list, int count, IEqualityComparer<T> equalityComparer)
		{
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			return FirstConsecutiveEqual(list, count, equalityComparer.Equals);
		}

		/// <summary>
		/// Finds the first occurence of <paramref name="count"/> consecutive "equal" items in the
		/// list. The passed BinaryPredicate is used to determine if two items are "equal".
		/// </summary>
		/// <remarks>Since an arbitrary BinaryPredicate is passed to this function, what is being tested for need not be true equality. </remarks>
		/// <param name="list">The list to examine.</param>
		/// <param name="count">The number of consecutive equal items to look for. The count must be at least 1.</param>
		/// <param name="predicate">The BinaryPredicate used to compare items for "equality". </param>
		/// <returns>The index of the first item in the first run of <paramref name="count"/> consecutive equal items, or -1 if no such run exists.</returns>
		public static int FirstConsecutiveEqual<T>(IList<T> list, int count, BinaryPredicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (predicate == null)
				throw new ArgumentNullException("predicate");
			if (count < 1)
				throw new ArgumentOutOfRangeException("count");

			int listCount = list.Count;
			if (listCount < count)
				return -1; // Can't find run longer than the list itself.
			if (count == 1)
				return 0; // Run of 1 must be the first item in the list.

			int start = 0, index = 0;
			T current = default(T);
			int runLength = 0;

			// Go through the list, looking for a run of the given length.
			foreach (T item in list)
			{
				if (index > 0 && predicate(current, item))
				{
					++runLength;
					if (runLength >= count)
						return start;
				}
				else
				{
					current = item;
					start = index;
					runLength = 1;
				}

				++index;
			}

			return -1;
		}

		/// <summary>
		/// Finds the first occurence of <paramref name="count"/> consecutive items in the
		/// list for which a given predicate returns true.
		/// </summary>
		/// <param name="list">The list to examine.</param>
		/// <param name="count">The number of consecutive items to look for. The count must be at least 1.</param>
		/// <param name="predicate">The predicate used to test each item.</param>
		/// <returns>The index of the first item in the first run of <paramref name="count"/> items where <paramref name="predicate"/>
		/// returns true for all items in the run, or -1 if no such run exists.</returns>
		public static int FirstConsecutiveWhere<T>(IList<T> list, int count, Predicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (predicate == null)
				throw new ArgumentNullException("predicate");
			if (count < 1)
				throw new ArgumentOutOfRangeException("count");

			int listCount = list.Count;
			if (count > listCount)
				return -1; // Can't find run longer than the list itself.

			int index = 0, start = -1;
			int runLength = 0;

			// Scan the list in order, looking for the number of consecutive true items.
			foreach (T item in list)
			{
				if (predicate(item))
				{
					if (start < 0)
						start = index;
					++runLength;
					if (runLength >= count)
						return start;
				}
				else
				{
					runLength = 0;
					start = -1;
				}

				++index;
			}

			return -1;
		}

		#endregion Consecutive items

		#region Find and SearchForSubsequence

		/// <summary>
		/// Finds the first item in a collection that satisfies the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <remarks>If the default value for T could be present in the collection, and 
		/// would be matched by the predicate, then this method is inappropriate, because
		/// you cannot disguish whether the default value for T was actually present in the collection,
		/// or no items matched the predicate. In this case, use TryFindFirstWhere.</remarks>
		/// <param name="collection">The collection to search.</param>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <returns>The first item in the collection that matches the condition, or the default value for T (0 or null) if no
		/// item that matches the condition is found.</returns>
		/// <seealso cref="Algorithms.TryFindFirstWhere{T}"/>
		public static T FindFirstWhere<T>(IEnumerable<T> collection, Predicate<T> predicate)
		{
			T retval;
			if (TryFindFirstWhere(collection, predicate, out retval))
				return retval;
			else
				return default(T);
		}

		/// <summary>
		/// Finds the first item in a collection that satisfies the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="collection">The collection to search.</param>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <param name="foundItem">Outputs the first item in the collection that matches the condition, if the method returns true.</param>
		/// <returns>True if an item satisfying the condition was found. False if no such item exists in the collection.</returns>
		/// <seealso cref="FindFirstWhere{T}"/>
		public static bool TryFindFirstWhere<T>(IEnumerable<T> collection, Predicate<T> predicate, out T foundItem)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			foreach (T item in collection)
			{
				if (predicate(item))
				{
					foundItem = item;
					return true;
				}
			}

			// didn't find any item that matches.
			foundItem = default(T);
			return false;
		}

		/// <summary>
		/// Finds the last item in a collection that satisfies the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <remarks><para>If the collection implements IList&lt;T&gt;, then the list is scanned in reverse until a 
		/// matching item is found. Otherwise, the entire collection is iterated in the forward direction.</para>
		/// <para>If the default value for T could be present in the collection, and 
		/// would be matched by the predicate, then this method is inappropriate, because
		/// you cannot disguish whether the default value for T was actually present in the collection,
		/// or no items matched the predicate. In this case, use TryFindFirstWhere.</para></remarks>
		/// <param name="collection">The collection to search.</param>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <returns>The last item in the collection that matches the condition, or the default value for T (0 or null) if no
		/// item that matches the condition is found.</returns>
		/// <seealso cref="TryFindLastWhere{T}"/>
		public static T FindLastWhere<T>(IEnumerable<T> collection, Predicate<T> predicate)
		{
			T retval;
			if (TryFindLastWhere(collection, predicate, out retval))
				return retval;
			else
				return default(T);
		}

		/// <summary>
		/// Finds the last item in a collection that satisfies the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <remarks>If the collection implements IList&lt;T&gt;, then the list is scanned in reverse until a 
		/// matching item is found. Otherwise, the entire collection is iterated in the forward direction.</remarks>
		/// <param name="collection">The collection to search.</param>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <param name="foundItem">Outputs the last item in the collection that matches the condition, if the method returns true.</param>
		/// <returns>True if an item satisfying the condition was found. False if no such item exists in the collection.</returns>
		/// <seealso cref="FindLastWhere{T}"/>
		public static bool TryFindLastWhere<T>(IEnumerable<T> collection, Predicate<T> predicate, out T foundItem)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			IList<T> list = collection as IList<T>;
			if (list != null)
			{
				// If it's a list, we can iterate in reverse.
				for (int index = list.Count - 1; index >= 0; --index)
				{
					T item = list[index];
					if (predicate(item))
					{
						foundItem = item;
						return true;
					}
				}

				// didn't find any item that matches.
				foundItem = default(T);
				return false;
			}
			else
			{
				// Otherwise, iterate the whole thing and remember the last matching one.
				bool found = false;
				foundItem = default(T);

				foreach (T item in collection)
				{
					if (predicate(item))
					{
						foundItem = item;
						found = true;
					}
				}

				return found;
			}
		}

		/// <summary>
		/// Enumerates all the items in <paramref name="collection"/> that satisfy the condition defined
		/// by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="collection">The collection to check all the items in.</param>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates the items that satisfy the condition.</returns>
		public static IEnumerable<T> FindWhere<T>(IEnumerable<T> collection, Predicate<T> predicate)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			foreach (T item in collection)
			{
				if (predicate(item))
				{
					yield return item;
				}
			}
		}

		/// <summary>
		/// Finds the index of the first item in a list that satisfies the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="list">The list to search.</param>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <returns>The index of the first item satisfying the condition. -1 if no such item exists in the list.</returns>
		public static int FindFirstIndexWhere<T>(IList<T> list, Predicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			int index = 0;
			foreach (T item in list)
			{
				if (predicate(item))
				{
					return index;
				}
				++index;
			}

			// didn't find any item that matches.
			return -1;
		}

		/// <summary>
		/// Finds the index of the last item in a list that satisfies the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="list">The list to search.</param>
		/// <param name="predicate">A delegate that defined the condition to check for.</param>
		/// <returns>The index of the last item satisfying the condition. -1 if no such item exists in the list.</returns>
		public static int FindLastIndexWhere<T>(IList<T> list, Predicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			for (int index = list.Count - 1; index >= 0; --index)
			{
				if (predicate(list[index]))
				{
					return index;
				}
			}

			// didn't find any item that matches.
			return -1;
		}

		/// <summary>
		/// Enumerates the indices of all the items in <paramref name="list"/> that satisfy the condition defined
		/// by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="list">The list to check all the items in.</param>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates the indices of items that satisfy the condition.</returns>
		public static IEnumerable<int> FindIndicesWhere<T>(IList<T> list, Predicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			int index = 0;
			foreach (T item in list)
			{
				if (predicate(item))
				{
					yield return index;
				}
				++index;
			}
		}

		/// <summary>
		/// Finds the index of the first item in a list equal to a given item.
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <param name="list">The list to search.</param>
		/// <param name="item">The item to search for.</param>
		/// <returns>The index of the first item equal to <paramref name="item"/>. -1 if no such item exists in the list.</returns>
		public static int FirstIndexOf<T>(IList<T> list, T item)
		{
			return FirstIndexOf(list, item, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Finds the index of the first item in a list equal to a given item. A passed
		/// IEqualityComparer is used to determine equality.
		/// </summary>
		/// <param name="list">The list to search.</param>
		/// <param name="item">The item to search for.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. Only the Equals method will be called.</param>
		/// <returns>The index of the first item equal to <paramref name="item"/>. -1 if no such item exists in the list.</returns>
		public static int FirstIndexOf<T>(IList<T> list, T item, IEqualityComparer<T> equalityComparer)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			int index = 0;
			foreach (T x in list)
			{
				if (equalityComparer.Equals(x, item))
				{
					return index;
				}
				++index;
			}

			// didn't find any item that matches.
			return -1;
		}

		/// <summary>
		/// Finds the index of the last item in a list equal to a given item.
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <param name="list">The list to search.</param>
		/// <param name="item">The item to search for.</param>
		/// <returns>The index of the last item equal to <paramref name="item"/>. -1 if no such item exists in the list.</returns>
		public static int LastIndexOf<T>(IList<T> list, T item)
		{
			return LastIndexOf(list, item, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Finds the index of the last item in a list equal to a given item. A passed
		/// IEqualityComparer is used to determine equality.
		/// </summary>
		/// <param name="list">The list to search.</param>
		/// <param name="item">The item to search for.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. Only the Equals method will be called.</param>
		/// <returns>The index of the last item equal to <paramref name="item"/>. -1 if no such item exists in the list.</returns>
		public static int LastIndexOf<T>(IList<T> list, T item, IEqualityComparer<T> equalityComparer)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			for (int index = list.Count - 1; index >= 0; --index)
			{
				if (equalityComparer.Equals(list[index], item))
				{
					return index;
				}
			}

			// didn't find any item that matches.
			return -1;
		}

		/// <summary>
		/// Enumerates the indices of all the items in a list equal to a given item.
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <param name="list">The list to search.</param>
		/// <param name="item">The item to search for.</param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates the indices of items equal to <paramref name="item"/>. </returns>
		public static IEnumerable<int> IndicesOf<T>(IList<T> list, T item)
		{
			return IndicesOf(list, item, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Enumerates the indices of all the items in a list equal to a given item. A passed
		/// IEqualityComparer is used to determine equality.
		/// </summary>
		/// <param name="list">The list to search.</param>
		/// <param name="item">The item to search for.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. Only the Equals method will be called.</param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates the indices of items equal to <paramref name="item"/>. </returns>
		public static IEnumerable<int> IndicesOf<T>(IList<T> list, T item, IEqualityComparer<T> equalityComparer)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			int index = 0;
			foreach (T x in list)
			{
				if (equalityComparer.Equals(x, item))
				{
					yield return index;
				}
				++index;
			}
		}

		/// <summary>
		/// Finds the index of the first item in a list equal to one of several given items.
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <param name="list">The list to search.</param>
		/// <param name="itemsToLookFor">The items to search for.</param>
		/// <returns>The index of the first item equal to any of the items in the collection <paramref name="itemsToLookFor"/>. 
		/// -1 if no such item exists in the list.</returns>
		public static int FirstIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor)
		{
			return FirstIndexOfMany(list, itemsToLookFor, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Finds the index of the first item in a list equal to one of several given items. A passed
		/// IEqualityComparer is used to determine equality.
		/// </summary>
		/// <param name="list">The list to search.</param>
		/// <param name="itemsToLookFor">The items to search for.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. 
		/// Only the Equals and GetHashCode methods will be called.</param>
		/// <returns>The index of the first item equal to any of the items in the collection <paramref name="itemsToLookFor"/>. 
		/// -1 if no such item exists in the list.</returns>
		public static int FirstIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, IEqualityComparer<T> equalityComparer)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (itemsToLookFor == null)
				throw new ArgumentNullException("itemsToLookFor");
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			// Create a set of the items we are looking for, for efficient lookup.
			Set<T> setToLookFor = new Set<T>(itemsToLookFor, equalityComparer);

			// Scan the list for the items.
			int index = 0;
			foreach (T x in list)
			{
				if (setToLookFor.Contains(x))
				{
					return index;
				}
				++index;
			}

			// didn't find any item that matches.
			return -1;
		}

		/// <summary>
		/// Finds the index of the first item in a list "equal" to one of several given items. The passed 
		/// BinaryPredicate is used to determine if two items are "equal".
		/// </summary>
		/// <remarks>Since an arbitrary BinaryPredicate is passed to this function, what is being removed need not be true equality. This methods finds 
		/// first item X which satisfies BinaryPredicate(X,Y), where Y is one of the items in <paramref name="itemsToLookFor"/></remarks>
		/// <param name="list">The list to search.</param>
		/// <param name="itemsToLookFor">The items to search for.</param>
		/// <param name="predicate">The BinaryPredicate used to compare items for "equality". </param>
		/// <returns>The index of the first item "equal" to any of the items in the collection <paramref name="itemsToLookFor"/>, using 
		/// <paramtype name="BinaryPredicate{T}"/> as the test for equality. 
		/// -1 if no such item exists in the list.</returns>
		public static int FirstIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, BinaryPredicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (itemsToLookFor == null)
				throw new ArgumentNullException("itemsToLookFor");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			// Scan the list for the items.
			int index = 0;
			foreach (T x in list)
			{
				foreach (T y in itemsToLookFor)
				{
					if (predicate(x, y))
					{
						return index;
					}
				}

				++index;
			}

			// didn't find any item that matches.
			return -1;
		}

		/// <summary>
		/// Finds the index of the last item in a list equal to one of several given items.
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <param name="list">The list to search.</param>
		/// <param name="itemsToLookFor">The items to search for.</param>
		/// <returns>The index of the last item equal to any of the items in the collection <paramref name="itemsToLookFor"/>. 
		/// -1 if no such item exists in the list.</returns>
		public static int LastIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor)
		{
			return LastIndexOfMany(list, itemsToLookFor, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Finds the index of the last item in a list equal to one of several given items. A passed
		/// IEqualityComparer is used to determine equality.
		/// </summary>
		/// <param name="list">The list to search.</param>
		/// <param name="itemsToLookFor">The items to search for.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality.</param>
		/// <returns>The index of the last item equal to any of the items in the collection <paramref name="itemsToLookFor"/>. 
		/// -1 if no such item exists in the list.</returns>
		public static int LastIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, IEqualityComparer<T> equalityComparer)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (itemsToLookFor == null)
				throw new ArgumentNullException("itemsToLookFor");
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			// Create a set of the items we are looking for, for efficient lookup.
			Set<T> setToLookFor = new Set<T>(itemsToLookFor, equalityComparer);

			// Scan the list
			for (int index = list.Count - 1; index >= 0; --index)
			{
				if (setToLookFor.Contains(list[index]))
				{
					return index;
				}
			}

			// didn't find any item that matches.
			return -1;
		}

		/// <summary>
		/// Finds the index of the last item in a list "equal" to one of several given items. The passed 
		/// BinaryPredicate is used to determine if two items are "equal".
		/// </summary>
		/// <remarks>Since an arbitrary BinaryPredicate is passed to this function, what is being removed need not be true equality. This methods finds 
		/// last item X which satisfies BinaryPredicate(X,Y), where Y is one of the items in <paramref name="itemsToLookFor"/></remarks>
		/// <param name="list">The list to search.</param>
		/// <param name="itemsToLookFor">The items to search for.</param>
		/// <param name="predicate">The BinaryPredicate used to compare items for "equality". </param>
		/// <returns>The index of the last item "equal" to any of the items in the collection <paramref name="itemsToLookFor"/>, using 
		/// <paramtype name="BinaryPredicate"/> as the test for equality. 
		/// -1 if no such item exists in the list.</returns>
		public static int LastIndexOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, BinaryPredicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (itemsToLookFor == null)
				throw new ArgumentNullException("itemsToLookFor");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			// Scan the list
			for (int index = list.Count - 1; index >= 0; --index)
			{
				foreach (T y in itemsToLookFor)
				{
					if (predicate(list[index], y))
					{
						return index;
					}
				}
			}

			// didn't find any item that matches.
			return -1;
		}

		/// <summary>
		/// Enumerates the indices of all the items in a list equal to one of several given items. 
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <param name="list">The list to search.</param>
		/// <param name="itemsToLookFor">A collection of items to search for.</param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates the indices of items equal to
		/// any of the items in the collection <paramref name="itemsToLookFor"/>. </returns>
		public static IEnumerable<int> IndicesOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor)
		{
			return IndicesOfMany(list, itemsToLookFor, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Enumerates the indices of all the items in a list equal to one of several given items. A passed
		/// IEqualityComparer is used to determine equality.
		/// </summary>
		/// <param name="list">The list to search.</param>
		/// <param name="itemsToLookFor">A collection of items to search for.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. </param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates the indices of items equal to
		/// any of the items in the collection <paramref name="itemsToLookFor"/>. </returns>
		public static IEnumerable<int> IndicesOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, IEqualityComparer<T> equalityComparer)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (itemsToLookFor == null)
				throw new ArgumentNullException("itemsToLookFor");
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			// Create a set of the items we are looking for, for efficient lookup.
			Set<T> setToLookFor = new Set<T>(itemsToLookFor, equalityComparer);

			// Scan the list
			int index = 0;
			foreach (T x in list)
			{
				if (setToLookFor.Contains(x))
				{
					yield return index;
				}
				++index;
			}
		}

		/// <summary>
		/// Enumerates the indices of all the items in a list equal to one of several given items. The passed 
		/// BinaryPredicate is used to determine if two items are "equal".
		/// </summary>
		/// <remarks>Since an arbitrary BinaryPredicate is passed to this function, what is being removed need not be true equality. This methods finds 
		/// last item X which satisfies BinaryPredicate(X,Y), where Y is one of the items in <paramref name="itemsToLookFor"/></remarks>
		/// <param name="list">The list to search.</param>
		/// <param name="itemsToLookFor">A collection of items to search for.</param>
		/// <param name="predicate">The BinaryPredicate used to compare items for "equality". </param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates the indices of items "equal" to any of the items 
		/// in the collection <paramref name="itemsToLookFor"/>, using 
		/// <paramtest name="BinaryPredicate"/> as the test for equality. </returns>
		public static IEnumerable<int> IndicesOfMany<T>(IList<T> list, IEnumerable<T> itemsToLookFor, BinaryPredicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (itemsToLookFor == null)
				throw new ArgumentNullException("itemsToLookFor");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			// Scan the list for the items.
			int index = 0;
			foreach (T x in list)
			{
				foreach (T y in itemsToLookFor)
				{
					if (predicate(x, y))
					{
						yield return index;
					}
				}

				++index;
			}
		}

		/// <summary>
		/// Searchs a list for a sub-sequence of items that match a particular pattern. A subsequence 
		/// of <paramref name="list"/> matches pattern at index i if list[i] is equal to the first item
		/// in <paramref name="pattern"/>, list[i+1] is equal to the second item in <paramref name="pattern"/>,
		/// and so forth for all the items in <paramref name="pattern"/>.
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to search.</param>
		/// <param name="pattern">The sequence of items to search for.</param>
		/// <returns>The first index with <paramref name="list"/> that matches the items in <paramref name="pattern"/>.</returns>
		public static int SearchForSubsequence<T>(IList<T> list, IEnumerable<T> pattern)
		{
			return SearchForSubsequence(list, pattern, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Searchs a list for a sub-sequence of items that match a particular pattern. A subsequence 
		/// of <paramref name="list"/> matches pattern at index i if list[i] is "equal" to the first item
		/// in <paramref name="pattern"/>, list[i+1] is "equal" to the second item in <paramref name="pattern"/>,
		/// and so forth for all the items in <paramref name="pattern"/>. The passed 
		/// BinaryPredicate is used to determine if two items are "equal".
		/// </summary>
		/// <remarks>Since an arbitrary BinaryPredicate is passed to this function, what is being tested
		/// for in the pattern need not be equality. </remarks>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to search.</param>
		/// <param name="pattern">The sequence of items to search for.</param>
		/// <param name="predicate">The BinaryPredicate used to compare items for "equality". </param>
		/// <returns>The first index with <paramref name="list"/> that matches the items in <paramref name="pattern"/>.</returns>
		public static int SearchForSubsequence<T>(IList<T> list, IEnumerable<T> pattern, BinaryPredicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (pattern == null)
				throw new ArgumentNullException("pattern");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			// Put the pattern into an array for performance (don't keep allocating enumerators).
			T[] patternArray = ToArray(pattern);

			int listCount = list.Count, patternCount = patternArray.Length;
			if (patternCount == 0)
				return 0; // A zero-length pattern occurs anywhere.
			if (listCount == 0)
				return -1; // no room for a pattern;

			for (int start = 0; start <= listCount - patternCount; ++start)
			{
				for (int count = 0; count < patternCount; ++count)
				{
					if (!predicate(list[start + count], patternArray[count]))
						goto NOMATCH;
				}
				// Got through the whole pattern. We have a match.
				return start;

				NOMATCH:
				/* no match found at start. */
				;
			}

			// no match found anywhere.
			return -1;
		}

		/// <summary>
		/// Searchs a list for a sub-sequence of items that match a particular pattern. A subsequence 
		/// of <paramref name="list"/> matches pattern at index i if list[i] is equal to the first item
		/// in <paramref name="pattern"/>, list[i+1] is equal to the second item in <paramref name="pattern"/>,
		/// and so forth for all the items in <paramref name="pattern"/>. The passed 
		/// instance of IEqualityComparer&lt;T&gt; is used for determining if two items are equal.
		/// </summary>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to search.</param>
		/// <param name="pattern">The sequence of items to search for.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. Only the Equals method will be called.</param>
		/// <returns>The first index with <paramref name="list"/> that matches the items in <paramref name="pattern"/>.</returns>
		public static int SearchForSubsequence<T>(IList<T> list, IEnumerable<T> pattern, IEqualityComparer<T> equalityComparer)
		{
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			return SearchForSubsequence(list, pattern, equalityComparer.Equals);
		}

		#endregion Find and SearchForSubsequence

		#region Set operations (coded except EqualSets)

		/// <summary>
		/// Determines if one collection is a subset of another, considered as sets. The first set is a subset
		/// of the second set if every item in the first set also occurs in the second set. If an item appears X times in the first set,
		/// it must appear at least X times in the second set.
		/// </summary>
		/// <remarks>
		/// <para>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the IsSubsetOf method on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection.</param>
		/// <param name="collection2">The second collection.</param>
		/// <returns>True if <paramref name="collection1"/> is a subset of <paramref name="collection2"/>, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static bool IsSubsetOf<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
		{
			return IsSubsetOf(collection1, collection2, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Determines if one collection is a subset of another, considered as sets. The first set is a subset
		/// of the second set if every item in the first set also occurs in the second set. If an item appears X times in the first set,
		/// it must appear at least X times in the second set.
		/// </summary>
		/// <remarks>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the IsSubsetOf method on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection.</param>
		/// <param name="collection2">The second collection.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality.</param>
		/// <returns>True if <paramref name="collection1"/> is a subset of <paramref name="collection2"/>, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static bool IsSubsetOf<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
		{
			if (collection1 == null)
				throw new ArgumentNullException("collection1");
			if (collection2 == null)
				throw new ArgumentNullException("collection2");
			if (equalityComparer == null)
				throw new ArgumentException("equalityComparer");

			Bag<T> bag1 = new Bag<T>(collection1, equalityComparer);
			Bag<T> bag2 = new Bag<T>(collection2, equalityComparer);
			return bag2.IsSupersetOf(bag1);
		}

		/// <summary>
		/// Determines if one collection is a proper subset of another, considered as sets. The first set is a proper subset
		/// of the second set if every item in the first set also occurs in the second set, and the first set is strictly smaller than
		/// the second set. If an item appears X times in the first set,
		/// it must appear at least X times in the second set.
		/// </summary>
		/// <remarks>
		/// <para>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the IsSubsetOf method on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection.</param>
		/// <param name="collection2">The second collection.</param>
		/// <returns>True if <paramref name="collection1"/> is a subset of <paramref name="collection2"/>, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static bool IsProperSubsetOf<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
		{
			return IsProperSubsetOf(collection1, collection2, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Determines if one collection is a proper subset of another, considered as sets. The first set is a proper subset
		/// of the second set if every item in the first set also occurs in the second set, and the first set is strictly smaller than
		/// the second set. If an item appears X times in the first set,
		/// it must appear at least X times in the second set.
		/// </summary>
		/// <remarks>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the IsSubsetOf method on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection.</param>
		/// <param name="collection2">The second collection.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. 
		/// Only the Equals and GetHashCode member functions of this interface are called.</param>
		/// <returns>True if <paramref name="collection1"/> is a proper subset of <paramref name="collection2"/>, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static bool IsProperSubsetOf<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
		{
			if (collection1 == null)
				throw new ArgumentNullException("collection1");
			if (collection2 == null)
				throw new ArgumentNullException("collection2");
			if (equalityComparer == null)
				throw new ArgumentException("equalityComparer");

			Bag<T> bag1 = new Bag<T>(collection1, equalityComparer);
			Bag<T> bag2 = new Bag<T>(collection2, equalityComparer);
			return bag2.IsProperSupersetOf(bag1);
		}


		/// <summary>
		/// Determines if two collections are disjoint, considered as sets. Two sets are disjoint if they
		/// have no common items.
		/// </summary>
		/// <remarks>
		/// <para>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the IsDisjoint method on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection.</param>
		/// <param name="collection2">The second collection.</param>
		/// <returns>True if <paramref name="collection1"/> are <paramref name="collection2"/> are disjoint, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static bool DisjointSets<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
		{
			return DisjointSets(collection1, collection2, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Determines if two collections are disjoint, considered as sets. Two sets are disjoint if they
		/// have no common items.
		/// </summary>
		/// <remarks>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the IsDisjoint method on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection.</param>
		/// <param name="collection2">The second collection.</param>
		/// <param name="equalityComparer">The IEqualityComparerComparer&lt;T&gt; used to compare items for equality. 
		/// Only the Equals and GetHashCode member functions of this interface are called.</param>
		/// <returns>True if <paramref name="collection1"/> are <paramref name="collection2"/> are disjoint, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static bool DisjointSets<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
		{
			if (collection1 == null)
				throw new ArgumentNullException("collection1");
			if (collection2 == null)
				throw new ArgumentNullException("collection2");
			if (equalityComparer == null)
				throw new ArgumentException("equalityComparer");

			Set<T> set1 = new Set<T>(collection1, equalityComparer);

			foreach (T item2 in collection2)
			{
				if (set1.Contains(item2))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Determines if two collections are equal, considered as sets. Two sets are equal if they
		/// have have the same items, with order not being significant.
		/// </summary>
		/// <remarks>
		/// <para>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the EqualTo method on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection.</param>
		/// <param name="collection2">The second collection.</param>
		/// <returns>True if <paramref name="collection1"/> are <paramref name="collection2"/> are equal, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static bool EqualSets<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
		{
			return EqualSets(collection1, collection2, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Determines if two collections are equal, considered as sets. Two sets are equal if they
		/// have have the same items, with order not being significant.
		/// </summary>
		/// <remarks>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the EqualTo method on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection.</param>
		/// <param name="collection2">The second collection.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. 
		/// Only the Equals and GetHashCode member functions of this interface are called.</param>
		/// <returns>True if <paramref name="collection1"/> are <paramref name="collection2"/> are equal, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static bool EqualSets<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
		{
			if (collection1 == null)
				throw new ArgumentNullException("collection1");
			if (collection2 == null)
				throw new ArgumentNullException("collection2");
			if (equalityComparer == null)
				throw new ArgumentException("equalityComparer");

			Bag<T> bag1 = new Bag<T>(collection1, equalityComparer);
			Bag<T> bag2 = new Bag<T>(collection2, equalityComparer);
			return bag2.IsEqualTo(bag1);
		}

		/// <summary>
		/// Computes the set-theoretic intersection of two collections. The intersection of two sets
		/// is all items that appear in both of the sets. If an item appears X times in one set,
		/// and Y times in the other set, the intersection contains the item Minimum(X,Y) times. 
		/// The source collections are not changed.
		/// A new collection is created with the intersection of the collections; the order of the
		/// items in this collection is undefined.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both collections, the returned collection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the Intersection or IntersectionWith methods on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection to intersect.</param>
		/// <param name="collection2">The second collection to intersect.</param>
		/// <returns>The intersection of the two collections, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static IEnumerable<T> SetIntersection<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
		{
			return SetIntersection(collection1, collection2, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Computes the set-theoretic intersection of two collections. The intersection of two sets
		/// is all items that appear in both of the sets. If an item appears X times in one set,
		/// and Y times in the other set, the intersection contains the item Minimum(X,Y) times. 
		/// The source collections are not changed.
		/// A new collection is created with the intersection of the collections; the order of the
		/// items in this collection is undefined.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both collections, the returned collection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the Intersection or IntersectionWith methods on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection to intersect.</param>
		/// <param name="collection2">The second collection to intersect.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. 
		/// Only the Equals and GetHashCode member functions of this interface are called.</param>
		/// <returns>The intersection of the two collections, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static IEnumerable<T> SetIntersection<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
		{
			if (collection1 == null)
				throw new ArgumentNullException("collection1");
			if (collection2 == null)
				throw new ArgumentNullException("collection2");
			if (equalityComparer == null)
				throw new ArgumentException("equalityComparer");

			Bag<T> bag1 = new Bag<T>(collection1, equalityComparer);
			Bag<T> bag2 = new Bag<T>(collection2, equalityComparer);
			return Util.CreateEnumerableWrapper(bag1.Intersection(bag2));
		}

		/// <summary>
		/// Computes the set-theoretic union of two collections. The union of two sets
		/// is all items that appear in either of the sets. If an item appears X times in one set,
		/// and Y times in the other set, the union contains the item Maximum(X,Y) times. 
		/// The source collections are not changed.
		/// A new collection is created with the union of the collections; the order of the
		/// items in this collection is undefined.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both collections, the returned collection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the Union or UnionWith methods on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection to union.</param>
		/// <param name="collection2">The second collection to union.</param>
		/// <returns>The union of the two collections, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static IEnumerable<T> SetUnion<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
		{
			return SetUnion(collection1, collection2, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Computes the set-theoretic union of two collections. The union of two sets
		/// is all items that appear in either of the sets. If an item appears X times in one set,
		/// and Y times in the other set, the union contains the item Maximum(X,Y) times. 
		/// The source collections are not changed.
		/// A new collection is created with the union of the collections; the order of the
		/// items in this collection is undefined.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both collections, the returned collection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the union or unionWith methods on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection to union.</param>
		/// <param name="collection2">The second collection to union.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. 
		/// Only the Equals and GetHashCode member functions of this interface are called.</param>
		/// <returns>The union of the two collections, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static IEnumerable<T> SetUnion<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
		{
			if (collection1 == null)
				throw new ArgumentNullException("collection1");
			if (collection2 == null)
				throw new ArgumentNullException("collection2");
			if (equalityComparer == null)
				throw new ArgumentException("equalityComparer");

			Bag<T> bag1 = new Bag<T>(collection1, equalityComparer);
			Bag<T> bag2 = new Bag<T>(collection2, equalityComparer);
			if (bag1.Count > bag2.Count)
			{
				bag1.UnionWith(bag2);
				return Util.CreateEnumerableWrapper(bag1);
			}
			else
			{
				bag2.UnionWith(bag1);
				return Util.CreateEnumerableWrapper(bag2);
			}
		}

		/// <summary>
		/// Computes the set-theoretic difference of two collections. The difference of two sets
		/// is all items that appear in the first set, but not in the second. If an item appears X times in the first set,
		/// and Y times in the second set, the difference contains the item X - Y times (0 times if X &lt; Y). 
		/// The source collections are not changed.
		/// A new collection is created with the difference of the collections; the order of the
		/// items in this collection is undefined.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both collections, the returned collection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the Difference or DifferenceWith methods on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection to difference.</param>
		/// <param name="collection2">The second collection to difference.</param>
		/// <returns>The difference of <paramref name="collection1"/> and <paramref name="collection2"/>, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static IEnumerable<T> SetDifference<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
		{
			return SetDifference(collection1, collection2, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Computes the set-theoretic difference of two collections. The difference of two sets
		/// is all items that appear in the first set, but not in the second. If an item appears X times in the first set,
		/// and Y times in the second set, the difference contains the item X - Y times (0 times if X &lt; Y). 
		/// The source collections are not changed.
		/// A new collection is created with the difference of the collections; the order of the
		/// items in this collection is undefined.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both collections, the returned collection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the difference or differenceWith methods on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection to difference.</param>
		/// <param name="collection2">The second collection to difference.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. 
		/// Only the Equals and GetHashCode member functions of this interface are called.</param>
		/// <returns>The difference of <paramref name="collection1"/> and <paramref name="collection2"/>, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static IEnumerable<T> SetDifference<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
		{
			if (collection1 == null)
				throw new ArgumentNullException("collection1");
			if (collection2 == null)
				throw new ArgumentNullException("collection2");
			if (equalityComparer == null)
				throw new ArgumentException("equalityComparer");

			Bag<T> bag1 = new Bag<T>(collection1, equalityComparer);
			Bag<T> bag2 = new Bag<T>(collection2, equalityComparer);
			bag1.DifferenceWith(bag2);
			return Util.CreateEnumerableWrapper(bag1);
		}

		/// <summary>
		/// Computes the set-theoretic symmetric difference of two collections. The symmetric difference of two sets
		/// is all items that appear in the one of the sets, but not in the other. If an item appears X times in the one set,
		/// and Y times in the other set, the symmetric difference contains the item AbsoluteValue(X - Y) times. 
		/// The source collections are not changed.
		/// A new collection is created with the symmetric difference of the collections; the order of the
		/// items in this collection is undefined.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both collections, the returned collection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the SymmetricDifference or SymmetricDifferenceWith methods on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection to symmetric difference.</param>
		/// <param name="collection2">The second collection to symmetric difference.</param>
		/// <returns>The symmetric difference of <paramref name="collection1"/> and <paramref name="collection2"/>, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static IEnumerable<T> SetSymmetricDifference<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
		{
			return SetSymmetricDifference(collection1, collection2, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Computes the set-theoretic symmetric difference of two collections. The symmetric difference of two sets
		/// is all items that appear in the one of the sets, but not in the other. If an item appears X times in the one set,
		/// and Y times in the other set, the symmetric difference contains the item AbsoluteValue(X - Y) times. 
		/// The source collections are not changed.
		/// A new collection is created with the symmetric difference of the collections; the order of the
		/// items in this collection is undefined.
		/// </summary>
		/// <remarks>
		/// <para>When equal items appear in both collections, the returned collection will include an arbitrary choice of one of the
		/// two equal items.</para>
		/// <para>If both collections are Set, Bag, OrderedSet, or OrderedBag
		/// collections, it is more efficient to use the symmetric difference or symmetric differenceWith methods on that class.</para>
		/// </remarks>
		/// <param name="collection1">The first collection to symmetric difference.</param>
		/// <param name="collection2">The second collection to symmetric difference.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. 
		/// Only the Equals and GetHashCode member functions of this interface are called.</param>
		/// <returns>The symmetric difference of <paramref name="collection1"/> and <paramref name="collection2"/>, considered as sets.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/> or <paramref name="collection2"/> is null.</exception>
		public static IEnumerable<T> SetSymmetricDifference<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
		{
			if (collection1 == null)
				throw new ArgumentNullException("collection1");
			if (collection2 == null)
				throw new ArgumentNullException("collection2");
			if (equalityComparer == null)
				throw new ArgumentException("equalityComparer");

			Bag<T> bag1 = new Bag<T>(collection1, equalityComparer);
			Bag<T> bag2 = new Bag<T>(collection2, equalityComparer);
			if (bag1.Count > bag2.Count)
			{
				bag1.SymmetricDifferenceWith(bag2);
				return Util.CreateEnumerableWrapper(bag1);
			}
			else
			{
				bag2.SymmetricDifferenceWith(bag1);
				return Util.CreateEnumerableWrapper(bag2);
			}
		}

		/// <summary>
		/// Computes the cartestian product of two collections: all possible pairs of items, with the first item taken from the first collection and 
		/// the second item taken from the second collection. If the first collection has N items, and the second collection has M items, the cartesian
		/// product will have N * M pairs.
		/// </summary>
		/// <typeparam name="TFirst">The type of items in the first collection.</typeparam>
		/// <typeparam name="TSecond">The type of items in the second collection.</typeparam>
		/// <param name="first">The first collection.</param>
		/// <param name="second">The second collection.</param>
		/// <returns>An IEnumerable&lt;Pair&lt;TFirst, TSecond&gt;&gt; that enumerates the cartesian product of the two collections.</returns>
		public static IEnumerable<Pair<TFirst, TSecond>> CartesianProduct<TFirst, TSecond>(IEnumerable<TFirst> first, IEnumerable<TSecond> second)
		{
			if (first == null)
				throw new ArgumentNullException("first");
			if (second == null)
				throw new ArgumentNullException("second");

			foreach (TFirst itemFirst in first)
				foreach (TSecond itemSecond in second)
					yield return new Pair<TFirst, TSecond>(itemFirst, itemSecond);
		}

		#endregion Set operations 

		#region String representations (not yet coded)

		/// <summary>
		/// Gets a string representation of the elements in the collection.
		/// The string representation starts with "{", has a list of items separated
		/// by commas (","), and ends with "}". Each item in the collection is 
		/// converted to a string by calling its ToString method (null is represented by "null").
		/// Contained collections (except strings) are recursively converted to strings by this method.
		/// </summary>
		/// <param name="collection">A collection to get the string representation of.</param>
		/// <returns>The string representation of the collection. If <paramref name="collection"/> is null, then the string "null" is returned.</returns>
		public static string ToString<T>(IEnumerable<T> collection)
		{
			return ToString(collection, true, "{", ",", "}");
		}

		/// <summary>
		/// Gets a string representation of the elements in the collection.
		/// The string to used at the beginning and end, and to separate items,
		/// and supplied by parameters. Each item in the collection is 
		/// converted to a string by calling its ToString method (null is represented by "null").
		/// </summary>
		/// <param name="collection">A collection to get the string representation of.</param>
		/// <param name="recursive">If true, contained collections (except strings) are converted to strings by a recursive call to this method, instead
		/// of by calling ToString.</param>
		/// <param name="start">The string to appear at the beginning of the output string.</param>
		/// <param name="separator">The string to appear between each item in the string.</param>
		/// <param name="end">The string to appear at the end of the output string.</param>
		/// <returns>The string representation of the collection. If <paramref name="collection"/> is null, then the string "null" is returned.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="start"/>, <paramref name="separator"/>, or <paramref name="end"/>
		///  is null.</exception>
		public static string ToString<T>(IEnumerable<T> collection, bool recursive, string start, string separator, string end)
		{
			if (start == null)
				throw new ArgumentNullException("start");
			if (separator == null)
				throw new ArgumentNullException("separator");
			if (end == null)
				throw new ArgumentNullException("end");

			if (collection == null)
				return "null";

			bool firstItem = true;

			StringBuilder builder = new StringBuilder();

			builder.Append(start);

			// Call ToString on each item and put it in.
			foreach (T item in collection)
			{
				if (!firstItem)
					builder.Append(separator);

				if (item == null)
					builder.Append("null");
				else if (recursive && item is IEnumerable && !(item is string))
					builder.Append(ToString(TypedAs<object>((IEnumerable) item), recursive, start, separator, end));
				else
					builder.Append(item.ToString());

				firstItem = false;
			}

			builder.Append(end);
			return builder.ToString();
		}

		/// <summary>
		/// Gets a string representation of the mappings in a dictionary.
		/// The string representation starts with "{", has a list of mappings separated
		/// by commas (", "), and ends with "}". Each mapping is represented
		/// by "key->value". Each key and value in the dictionary is 
		/// converted to a string by calling its ToString method (null is represented by "null").
		/// Contained collections (except strings) are recursively converted to strings by this method.
		/// </summary>
		/// <param name="dictionary">A dictionary to get the string representation of.</param>
		/// <returns>The string representation of the collection, or "null" 
		/// if <paramref name="dictionary"/> is null.</returns>
		public static string ToString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
		{
			bool firstItem = true;

			if (dictionary == null)
				return "null";

			StringBuilder builder = new StringBuilder();

			builder.Append("{");

			// Call ToString on each item and put it in.
			foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			{
				if (!firstItem)
					builder.Append(", ");

				if (pair.Key == null)
					builder.Append("null");
				else if (pair.Key is IEnumerable && !(pair.Key is string))
					builder.Append(ToString(TypedAs<object>((IEnumerable) pair.Key), true, "{", ",", "}"));
				else
					builder.Append(pair.Key.ToString());

				builder.Append("->");

				if (pair.Value == null)
					builder.Append("null");
				else if (pair.Value is IEnumerable && !(pair.Value is string))
					builder.Append(ToString(TypedAs<object>((IEnumerable) pair.Value), true, "{", ",", "}"));
				else
					builder.Append(pair.Value.ToString());


				firstItem = false;
			}

			builder.Append("}");
			return builder.ToString();
		}

		#endregion String representations

		#region Shuffles and Permutations

		private static volatile Random myRandomGenerator;

		/// <summary>
		/// Randomly shuffles the items in a collection, yielding a new collection.
		/// </summary>
		/// <typeparam name="T">The type of the items in the collection.</typeparam>
		/// <param name="collection">The collection to shuffle.</param>
		/// <returns>An array with the same size and items as <paramref name="collection"/>, but the items in a randomly chosen order.</returns>
		public static T[] RandomShuffle<T>(IEnumerable<T> collection)
		{
			return RandomShuffle(collection, GetRandomGenerator());
		}

		/// <summary>
		/// Randomly shuffles the items in a collection, yielding a new collection.
		/// </summary>
		/// <typeparam name="T">The type of the items in the collection.</typeparam>
		/// <param name="collection">The collection to shuffle.</param>
		/// <param name="randomGenerator">The random number generator to use to select the random order.</param>
		/// <returns>An array with the same size and items as <paramref name="collection"/>, but the items in a randomly chosen order.</returns>
		public static T[] RandomShuffle<T>(IEnumerable<T> collection, Random randomGenerator)
		{
			// We have to copy all items anyway, and there isn't a way to produce the items
			// on the fly that is linear. So copying to an array and shuffling it is an efficient as we can get.
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (randomGenerator == null)
				throw new ArgumentNullException("randomGenerator");

			T[] array = ToArray(collection);

			int count = array.Length;
			for (int i = count - 1; i >= 1; --i)
			{
				// Pick an random number 0 through i inclusive.
				int j = randomGenerator.Next(i + 1);

				// Swap array[i] and array[j]
				T temp = array[i];
				array[i] = array[j];
				array[j] = temp;
			}

			return array;
		}

		/// <summary>
		/// Randomly shuffles the items in a list or array, in place.
		/// </summary>
		/// <remarks>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</remarks>
		/// <param name="list">The list or array to shuffle.</param>
		public static void RandomShuffleInPlace<T>(IList<T> list)
		{
			RandomShuffleInPlace(list, GetRandomGenerator());
		}

		/// <summary>
		/// Randomly shuffles the items in a list or array, in place.
		/// </summary>
		/// <remarks>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</remarks>
		/// <param name="list">The list or array to shuffle.</param>
		/// <param name="randomGenerator">The random number generator to use to select the random order.</param>
		public static void RandomShuffleInPlace<T>(IList<T> list, Random randomGenerator)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (randomGenerator == null)
				throw new ArgumentNullException("randomGenerator");
			if (list is T[])
				list = new ArrayWrapper<T>((T[]) list);
			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			int count = list.Count;
			for (int i = count - 1; i >= 1; --i)
			{
				// Pick an random number 0 through i inclusive.
				int j = randomGenerator.Next(i + 1);

				// Swap list[i] and list[j]
				T temp = list[i];
				list[i] = list[j];
				list[j] = temp;
			}
		}

		/// <summary>
		/// Picks a random subset of <paramref name="count"/> items from <paramref name="collection"/>, and places
		/// those items into a random order. No item is selected more than once.
		/// </summary>
		/// <remarks>If the collection implements IList&lt;T&gt;, then this method takes time O(<paramref name="count"/>).
		/// Otherwise, this method takes time O(N), where N is the number of items in the collection.</remarks>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The collection of items to select from. This collection is not changed.</param>
		/// <param name="count">The number of items in the subset to choose.</param>
		/// <returns>An array of <paramref name="count"/> items, selected at random from <paramref name="collection"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative or greater than <paramref name="collection"/>.Count.</exception>
		public static T[] RandomSubset<T>(IEnumerable<T> collection, int count)
		{
			return RandomSubset(collection, count, GetRandomGenerator());
		}

		/// <summary>
		/// Picks a random subset of <paramref name="count"/> items from <paramref name="collection"/>, and places
		/// those items into a random order. No item is selected more than once.
		/// </summary>
		/// <remarks>If the collection implements IList&lt;T&gt;, then this method takes time O(<paramref name="count"/>).
		/// Otherwise, this method takes time O(N), where N is the number of items in the collection.</remarks>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The collection of items to select from. This collection is not changed.</param>
		/// <param name="count">The number of items in the subset to choose.</param>
		/// <param name="randomGenerator">The random number generates used to make the selection.</param>
		/// <returns>An array of <paramref name="count"/> items, selected at random from <paramref name="collection"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative or greater than <paramref name="collection"/>.Count.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="randomGenerator"/> is null.</exception>
		public static T[] RandomSubset<T>(IEnumerable<T> collection, int count, Random randomGenerator)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (randomGenerator == null)
				throw new ArgumentNullException("randomGenerator");

			// We need random access to the items in the collection. If it's not already an 
			// IList<T>, copy to a temporary list.
			IList<T> list = collection as IList<T>;
			if (list == null)
			{
				list = new List<T>(collection);
			}

			int listCount = list.Count;
			if (count < 0 || count > listCount)
				throw new ArgumentOutOfRangeException("count");

			T[] result = new T[count]; // the result array.
			Dictionary<int, T> swappedValues = new Dictionary<int, T>(count); // holds swapped values from the list.

			for (int i = 0; i < count; ++i)
			{
				// Set j to the index of the item to swap with, and value to the value to swap with.
				T value;
				int j = randomGenerator.Next(listCount - i) + i;

				// Swap values of i and j in the list. The list isn't actually changed; instead,
				// swapped values are stored in the dictionary swappedValues.
				if (!swappedValues.TryGetValue(j, out value))
					value = list[j];

				result[i] = value;
				if (i != j)
				{
					if (swappedValues.TryGetValue(i, out value))
						swappedValues[j] = value;
					else
						swappedValues[j] = list[i];
				}
			}

			return result;
		}

		/// <summary>
		/// Generates all the possible permutations of the items in <paramref name="collection"/>. If <paramref name="collection"/>
		/// has N items, then N factorial permutations will be generated. This method does not compare the items to determine if
		/// any of them are equal. If some items are equal, the same permutation may be generated more than once. For example,
		/// if the collections contains the three items A, A, and B, then this method will generate the six permutations, AAB, AAB,
		/// ABA, ABA, BAA, BAA (not necessarily in that order). To take equal items into account, use the GenerateSortedPermutations
		/// method.
		/// </summary>
		/// <typeparam name="T">The type of items to permute.</typeparam>
		/// <param name="collection">The collection of items to permute.</param>
		/// <returns>An IEnumerable&lt;T[]&gt; that enumerations all the possible permutations of the 
		/// items in <paramref name="collection"/>. Each permutations is returned as an array. The items in the array
		/// should be copied if they need to be used after the next permutation is generated; each permutation may
		/// reuse the same array instance.</returns>
		public static IEnumerable<T[]> GeneratePermutations<T>(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			T[] array = ToArray(collection);

			if (array.Length == 0)
				yield break;

			int[] state = new int[array.Length - 1];
			int maxLength = state.Length;

			yield return array;

			if (array.Length == 1)
				yield break;

			// The following algorithm makes two swaps for each
			// permutation generated.
			// This is not optimal in terms of number of swaps, but
			// is still O(1), and shorter and clearer to understand.
			int i = 0;
			T temp;
			for (;;)
			{
				if (state[i] < i + 1)
				{
					if (state[i] > 0)
					{
						temp = array[i + 1];
						array[i + 1] = array[state[i] - 1];
						array[state[i] - 1] = temp;
					}

					temp = array[i + 1];
					array[i + 1] = array[state[i]];
					array[state[i]] = temp;

					yield return array;

					++state[i];
					i = 0;
				}
				else
				{
					temp = array[i + 1];
					array[i + 1] = array[i];
					array[i] = temp;

					state[i] = 0;
					++i;
					if (i >= maxLength)
						yield break;
				}
			}
		}

		/// <summary>
		/// Generates all the possible permutations of the items in <paramref name="collection"/>, in lexicographical order. 
		/// Even if some items are equal, the same permutation will not be generated more than once. For example,
		/// if the collections contains the three items A, A, and B, then this method will generate only the three permutations, AAB, ABA,
		/// BAA. 
		/// </summary>
		/// <typeparam name="T">The type of items to permute.</typeparam>
		/// <param name="collection">The collection of items to permute.</param>
		/// <returns>An IEnumerable&lt;T[]&gt; that enumerations all the possible permutations of the 
		/// items in <paramref name="collection"/>. Each permutations is returned as an array. The items in the array
		/// should be copied if they need to be used after the next permutation is generated; each permutation may
		/// reuse the same array instance.</returns>
		public static IEnumerable<T[]> GenerateSortedPermutations<T>(IEnumerable<T> collection)
			where T : IComparable<T>
		{
			return GenerateSortedPermutations(collection, Comparer<T>.Default);
		}

		/// <summary>
		/// Generates all the possible permutations of the items in <paramref name="collection"/>, in lexicographical order. A
		/// supplied IComparer&lt;T&gt; instance is used to compare the items.
		/// Even if some items are equal, the same permutation will not be generated more than once. For example,
		/// if the collections contains the three items A, A, and B, then this method will generate only the three permutations, AAB, ABA,
		/// BAA. 
		/// </summary>
		/// <typeparam name="T">The type of items to permute.</typeparam>
		/// <param name="collection">The collection of items to permute.</param>
		/// <param name="comparer">The IComparer&lt;T&gt; used to compare the items.</param>
		/// <returns>An IEnumerable&lt;T[]&gt; that enumerations all the possible permutations of the 
		/// items in <paramref name="collection"/>. Each permutations is returned as an array. The items in the array
		/// should be copied if they need to be used after the next permutation is generated; each permutation may
		/// reuse the same array instance.</returns>
		public static IEnumerable<T[]> GenerateSortedPermutations<T>(IEnumerable<T> collection, IComparer<T> comparer)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			T[] array = ToArray(collection);
			int length = array.Length;
			if (length == 0)
				yield break;

			Array.Sort(array, comparer);

			yield return array;
			if (length == 1)
				yield break;

			// Keep generating the next permutation until we're done. Algorithm is
			// due to Jeffrey A. Johnson ("SEPA - a Simple Efficient Permutation Algorithm")
			int key, swap, i, j;
			T temp;
			for (;;)
			{
				// Find the key point -- where array[key]<array[key+1]. Everything after the
				// key is the tail.
				key = length - 2;
				while (comparer.Compare(array[key], array[key + 1]) >= 0)
				{
					--key;
					if (key < 0)
						yield break;
				}

				// Find the last item in the tail less than key.
				swap = length - 1;
				while (comparer.Compare(array[swap], array[key]) <= 0)
					--swap;

				// Swap it with the key.
				temp = array[key];
				array[key] = array[swap];
				array[swap] = temp;

				// Reverse the tail.
				i = key + 1;
				j = length - 1;
				while (i < j)
				{
					temp = array[i];
					array[i] = array[j];
					array[j] = temp;
					++i;
					--j;
				}

				yield return array;
			}
		}

		/// <summary>
		/// Generates all the possible permutations of the items in <paramref name="collection"/>, in lexicographical order. A
		/// supplied Comparison&lt;T&gt; delegate is used to compare the items.
		/// Even if some items are equal, the same permutation will not be generated more than once. For example,
		/// if the collections contains the three items A, A, and B, then this method will generate only the three permutations, AAB, ABA,
		/// BAA. 
		/// </summary>
		/// <typeparam name="T">The type of items to permute.</typeparam>
		/// <param name="collection">The collection of items to permute.</param>
		/// <param name="comparison">The Comparison&lt;T&gt; delegate used to compare the items.</param>
		/// <returns>An IEnumerable&lt;T[]&gt; that enumerations all the possible permutations of the 
		/// items in <paramref name="collection"/>. Each permutations is returned as an array. The items in the array
		/// should be copied if they need to be used after the next permutation is generated; each permutation may
		/// reuse the same array instance.</returns>
		public static IEnumerable<T[]> GenerateSortedPermutations<T>(IEnumerable<T> collection, Comparison<T> comparison)
		{
			return GenerateSortedPermutations(collection, Comparers.ComparerFromComparison(comparison));
		}

		/// <summary>
		/// Return a private random number generator to use if the user
		/// doesn't supply one.
		/// </summary>
		/// <returns>The private random number generator. Only one is ever created
		/// and is always returned.</returns>
		private static Random GetRandomGenerator()
		{
			if (myRandomGenerator == null)
			{
				lock (typeof (Algorithms))
				{
					if (myRandomGenerator == null)
						myRandomGenerator = new Random();
				}
			}

			return myRandomGenerator;
		}

		#endregion Shuffles and Permutations

		#region Minimum and Maximum

		/// <summary>
		/// Finds the maximum value in a collection.
		/// </summary>
		/// <remarks>Values in the collection are compared by using the IComparable&lt;T&gt;
		/// interfaces implementation on the type T.</remarks>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The collection to search.</param>
		/// <returns>The largest item in the collection. </returns>
		/// <exception cref="InvalidOperationException">The collection is empty.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public static T Maximum<T>(IEnumerable<T> collection)
			where T : IComparable<T>
		{
			return Maximum(collection, Comparer<T>.Default);
		}

		/// <summary>
		/// Finds the maximum value in a collection. A supplied IComparer&lt;T&gt; is used
		/// to compare the items in the collection.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The collection to search.</param>
		/// <param name="comparer">The comparer instance used to compare items in the collection.</param>
		/// <returns>The largest item in the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection is empty.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> or <paramref name="comparer"/> is null.</exception>
		public static T Maximum<T>(IEnumerable<T> collection, IComparer<T> comparer)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			T maxSoFar = default(T);
			bool foundOne = false;

			// Go through the collection, keeping the maximum found so far.
			foreach (T item in collection)
			{
				if (!foundOne || comparer.Compare(maxSoFar, item) < 0)
				{
					maxSoFar = item;
				}

				foundOne = true;
			}

			// If the collection was empty, throw an exception.
			if (!foundOne)
				throw new InvalidOperationException(Strings.CollectionIsEmpty);
			else
				return maxSoFar;
		}

		/// <summary>
		/// Finds the maximum value in a collection. A supplied Comparison&lt;T&gt; delegate is used
		/// to compare the items in the collection.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The collection to search.</param>
		/// <param name="comparison">The comparison used to compare items in the collection.</param>
		/// <returns>The largest item in the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection is empty.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> or <paramref name="comparison"/> is null.</exception>
		public static T Maximum<T>(IEnumerable<T> collection, Comparison<T> comparison)
		{
			return Maximum(collection, Comparers.ComparerFromComparison(comparison));
		}

		/// <summary>
		/// Finds the minimum value in a collection.
		/// </summary>
		/// <remarks>Values in the collection are compared by using the IComparable&lt;T&gt;
		/// interfaces implementation on the type T.</remarks>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The collection to search.</param>
		/// <returns>The smallest item in the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection is empty.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public static T Minimum<T>(IEnumerable<T> collection)
			where T : IComparable<T>
		{
			return Minimum(collection, Comparer<T>.Default);
		}

		/// <summary>
		/// Finds the minimum value in a collection. A supplied IComparer&lt;T&gt; is used
		/// to compare the items in the collection.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The collection to search.</param>
		/// <param name="comparer">The comparer instance used to compare items in the collection.</param>
		/// <returns>The smallest item in the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection is empty.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> or <paramref name="comparer"/> is null.</exception>
		public static T Minimum<T>(IEnumerable<T> collection, IComparer<T> comparer)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			T minSoFar = default(T);
			bool foundOne = false;

			// Go through the collection, keeping the minimum found so far.
			foreach (T item in collection)
			{
				if (!foundOne || comparer.Compare(minSoFar, item) > 0)
				{
					minSoFar = item;
				}

				foundOne = true;
			}

			// If the collection was empty, throw an exception.
			if (!foundOne)
				throw new InvalidOperationException(Strings.CollectionIsEmpty);
			else
				return minSoFar;
		}

		/// <summary>
		/// Finds the minimum value in a collection. A supplied Comparison&lt;T&gt; delegate is used
		/// to compare the items in the collection.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The collection to search.</param>
		/// <param name="comparison">The comparison used to compare items in the collection.</param>
		/// <returns>The smallest item in the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection is empty.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> or <paramref name="comparison"/> is null.</exception>
		public static T Minimum<T>(IEnumerable<T> collection, Comparison<T> comparison)
		{
			return Minimum(collection, Comparers.ComparerFromComparison(comparison));
		}

		/// <summary>
		/// Finds the index of the maximum value in a list.
		/// </summary>
		/// <remarks>Values in the list are compared by using the IComparable&lt;T&gt;
		/// interfaces implementation on the type T.</remarks>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to search.</param>
		/// <returns>The index of the largest item in the list. If the maximum value appears
		/// multiple times, the index of the first appearance is used. If the list is empty, -1 is returned.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		public static int IndexOfMaximum<T>(IList<T> list)
			where T : IComparable<T>
		{
			return IndexOfMaximum(list, Comparer<T>.Default);
		}

		/// <summary>
		/// Finds the index of the maximum value in a list. A supplied IComparer&lt;T&gt; is used
		/// to compare the items in the collection. 
		/// </summary>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to search.</param>
		/// <param name="comparer">The comparer instance used to compare items in the collection.</param>
		/// <returns>The index of the largest item in the list. If the maximum value appears
		/// multiple times, the index of the first appearance is used. If the list is empty, -1 is returned.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> or <paramref name="comparer"/> is null.</exception>
		public static int IndexOfMaximum<T>(IList<T> list, IComparer<T> comparer)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			T maxSoFar = default(T);
			int indexSoFar = -1;

			// Go through the collection, keeping the maximum found so far.
			int i = 0;
			foreach (T item in list)
			{
				if (indexSoFar < 0 || comparer.Compare(maxSoFar, item) < 0)
				{
					maxSoFar = item;
					indexSoFar = i;
				}

				++i;
			}

			return indexSoFar;
		}

		/// <summary>
		/// Finds the index of the maximum value in a list. A supplied Comparison&lt;T&gt; delegate is used
		/// to compare the items in the collection.
		/// </summary>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to search.</param>
		/// <param name="comparison">The comparison used to compare items in the collection.</param>
		/// <returns>The index of the largest item in the list. If the maximum value appears
		/// multiple times, the index of the first appearance is used. If the list is empty, -1 is returned.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> or <paramref name="comparison"/> is null.</exception>
		public static int IndexOfMaximum<T>(IList<T> list, Comparison<T> comparison)
		{
			return IndexOfMaximum(list, Comparers.ComparerFromComparison(comparison));
		}

		/// <summary>
		/// Finds the index of the minimum value in a list.
		/// </summary>
		/// <remarks>Values in the list are compared by using the IComparable&lt;T&gt;
		/// interfaces implementation on the type T.</remarks>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to search.</param>
		/// <returns>The index of the smallest item in the list. If the minimum value appears
		/// multiple times, the index of the first appearance is used.</returns>
		/// <exception cref="InvalidOperationException">The collection is empty.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		public static int IndexOfMinimum<T>(IList<T> list)
			where T : IComparable<T>
		{
			return IndexOfMinimum(list, Comparer<T>.Default);
		}

		/// <summary>
		/// Finds the index of the minimum value in a list. A supplied IComparer&lt;T&gt; is used
		/// to compare the items in the collection. 
		/// </summary>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to search.</param>
		/// <param name="comparer">The comparer instance used to compare items in the collection.</param>
		/// <returns>The index of the smallest item in the list. If the minimum value appears
		/// multiple times, the index of the first appearance is used.</returns>
		/// <exception cref="InvalidOperationException">The collection is empty.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> or <paramref name="comparer"/> is null.</exception>
		public static int IndexOfMinimum<T>(IList<T> list, IComparer<T> comparer)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			T minSoFar = default(T);
			int indexSoFar = -1;

			// Go through the collection, keeping the minimum found so far.
			int i = 0;
			foreach (T item in list)
			{
				if (indexSoFar < 0 || comparer.Compare(minSoFar, item) > 0)
				{
					minSoFar = item;
					indexSoFar = i;
				}

				++i;
			}

			return indexSoFar;
		}

		/// <summary>
		/// Finds the index of the minimum value in a list. A supplied Comparison&lt;T&gt; delegate is used
		/// to compare the items in the collection.
		/// </summary>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to search.</param>
		/// <param name="comparison">The comparison delegate used to compare items in the collection.</param>
		/// <returns>The index of the smallest item in the list. If the minimum value appears
		/// multiple times, the index of the first appearance is used.</returns>
		/// <exception cref="InvalidOperationException">The collection is empty.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> or <paramref name="comparison"/> is null.</exception>
		public static int IndexOfMinimum<T>(IList<T> list, Comparison<T> comparison)
		{
			return IndexOfMinimum(list, Comparers.ComparerFromComparison(comparison));
		}

		#endregion Minimum and Maximum

		#region Sorting and operations on sorted collections

		/// <summary>
		/// Creates a sorted version of a collection.
		/// </summary>
		/// <remarks>Values are compared by using the IComparable&lt;T&gt;
		/// interfaces implementation on the type T.</remarks>
		/// <param name="collection">The collection to sort.</param>
		/// <returns>An array containing the sorted version of the collection.</returns>
		public static T[] Sort<T>(IEnumerable<T> collection)
			where T : IComparable<T>
		{
			return Sort(collection, Comparer<T>.Default);
		}

		/// <summary>
		/// Creates a sorted version of a collection. A supplied IComparer&lt;T&gt; is used
		/// to compare the items in the collection. 
		/// </summary>
		/// <param name="collection">The collection to sort.</param>
		/// <param name="comparer">The comparer instance used to compare items in the collection. Only
		/// the Compare method is used.</param>
		/// <returns>An array containing the sorted version of the collection.</returns>
		public static T[] Sort<T>(IEnumerable<T> collection, IComparer<T> comparer)
		{
			T[] array;

			if (collection == null)
				throw new ArgumentNullException("collection");
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			array = ToArray(collection);

			Array.Sort(array, comparer);
			return array;
		}

		/// <summary>
		/// Creates a sorted version of a collection. A supplied Comparison&lt;T&gt; delegate is used
		/// to compare the items in the collection.
		/// </summary>
		/// <param name="collection">The collection to sort.</param>
		/// <param name="comparison">The comparison delegate used to compare items in the collection.</param>
		/// <returns>An array containing the sorted version of the collection.</returns>
		public static T[] Sort<T>(IEnumerable<T> collection, Comparison<T> comparison)
		{
			return Sort(collection, Comparers.ComparerFromComparison(comparison));
		}

		/// <summary>
		/// Sorts a list or array in place.
		/// </summary>
		/// <remarks><para>The Quicksort algorithms is used to sort the items. In virtually all cases,
		/// this takes time O(N log N), where N is the number of items in the list.</para>
		/// <para>Values are compared by using the IComparable&lt;T&gt;
		/// interfaces implementation on the type T.</para>
		/// <para>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</para></remarks>
		/// <param name="list">The list or array to sort.</param>
		public static void SortInPlace<T>(IList<T> list)
			where T : IComparable<T>
		{
			SortInPlace(list, Comparer<T>.Default);
		}

		/// <summary>
		/// Sorts a list or array in place. A supplied IComparer&lt;T&gt; is used
		/// to compare the items in the list. 
		/// </summary>
		/// <remarks><para>The Quicksort algorithms is used to sort the items. In virtually all cases,
		/// this takes time O(N log N), where N is the number of items in the list.</para>
		/// <para>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</para></remarks>
		/// <param name="list">The list or array to sort.</param>
		/// <param name="comparer">The comparer instance used to compare items in the collection. Only
		/// the Compare method is used.</param>
		public static void SortInPlace<T>(IList<T> list, IComparer<T> comparer)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			// If we have an array, use the built-in array sort (faster than going through IList accessors
			// with virtual calls).
			if (list is T[])
			{
				Array.Sort((T[]) list, comparer);
				return;
			}

			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			// Instead of a recursive procedure, we use an explicit stack to hold
			// ranges that we still need to sort.
			int[] leftStack = new int[32], rightStack = new int[32];
			int stackPtr = 0;

			int l = 0; // the inclusive left edge of the current range we are sorting.
			int r = list.Count - 1; // the inclusive right edge of the current range we are sorting.
			T partition; // The partition value.

			// Loop until we have nothing left to sort. On each iteration, l and r contains the bounds
			// of something to sort (unless r <= l), and leftStack/rightStack have a stack of unsorted
			// pieces (unles stackPtr == 0).
			for (;;)
			{
				if (l == r - 1)
				{
					// We have exactly 2 elements to sort. Compare them and swap if needed.
					T e1, e2;
					e1 = list[l];
					e2 = list[r];
					if (comparer.Compare(e1, e2) > 0)
					{
						list[r] = e1;
						list[l] = e2;
					}
					l = r; // sort complete, find other work from the stack.
				}
				else if (l < r)
				{
					// Sort the items in the inclusive range l .. r

					// Get the left, middle, and right-most elements and sort them, yielding e1=smallest, e2=median, e3=largest
					int m = l + (r - l)/2;
					T e1 = list[l], e2 = list[m], e3 = list[r], temp;
					if (comparer.Compare(e1, e2) > 0)
					{
						temp = e1;
						e1 = e2;
						e2 = temp;
					}
					if (comparer.Compare(e1, e3) > 0)
					{
						temp = e3;
						e3 = e2;
						e2 = e1;
						e1 = temp;
					}
					else if (comparer.Compare(e2, e3) > 0)
					{
						temp = e2;
						e2 = e3;
						e3 = temp;
					}

					if (l == r - 2)
					{
						// We have exactly 3 elements to sort, and we've done that. Store back and we're done.
						list[l] = e1;
						list[m] = e2;
						list[r] = e3;
						l = r; // sort complete, find other work from the stack.
					}
					else
					{
						// Put the smallest at the left, largest in the middle, and the median at the right (which is the partitioning value)
						list[l] = e1;
						list[m] = e3;
						list[r] = partition = e2;

						// Partition into three parts, items <= partition, items == partition, and items >= partition
						int i = l, j = r;
						T item_i, item_j;
						for (;;)
						{
							do
							{
								++i;
								item_i = list[i];
							} while (comparer.Compare(item_i, partition) < 0);

							do
							{
								--j;
								item_j = list[j];
							} while (comparer.Compare(item_j, partition) > 0);

							if (j < i)
								break;

							list[i] = item_j;
							list[j] = item_i; // swap items to continue the partition.
						}

						// Move the partition value into place.
						list[r] = item_i;
						list[i] = partition;
						++i;

						// We have partitioned the list. 
						//    Items in the inclusive range l .. j are <= partition.
						//    Items in the inclusive range i .. r are >= partition.
						//    Items in the inclusive range j+1 .. i - 1 are == partition (and in the correct final position).
						// We now need to sort l .. j and i .. r.
						// To do this, we stack one of the lists for later processing, and change l and r to the other list.
						// If we always stack the larger of the two sub-parts, the stack cannot get greater
						// than log2(Count) in size; i.e., a 32-element stack is enough for the maximum list size.
						if ((j - l) > (r - i))
						{
							// The right partition is smaller. Stack the left, and get ready to sort the right.
							leftStack[stackPtr] = l;
							rightStack[stackPtr] = j;
							l = i;
						}
						else
						{
							// The left partition is smaller. Stack the right, and get ready to sort the left.
							leftStack[stackPtr] = i;
							rightStack[stackPtr] = r;
							r = j;
						}
						++stackPtr;
					}
				}
				else if (stackPtr > 0)
				{
					// We have a stacked sub-list to sort. Pop it off and sort it.
					--stackPtr;
					l = leftStack[stackPtr];
					r = rightStack[stackPtr];
				}
				else
				{
					// We have nothing left to sort.
					break;
				}
			}
		}

		/// <summary>
		/// Sorts a list or array in place. A supplied Comparison&lt;T&gt; delegate is used
		/// to compare the items in the list.
		/// </summary>
		/// <remarks><para>The Quicksort algorithms is used to sort the items. In virtually all cases,
		/// this takes time O(N log N), where N is the number of items in the list.</para>
		/// <para>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</para></remarks>
		/// <param name="list">The list or array to sort.</param>
		/// <param name="comparison">The comparison delegate used to compare items in the collection.</param>
		public static void SortInPlace<T>(IList<T> list, Comparison<T> comparison)
		{
			SortInPlace(list, Comparers.ComparerFromComparison(comparison));
		}

		/// <summary>
		/// Creates a sorted version of a collection. The sort is stable, which means that if items X and Y are equal,
		/// and X precedes Y in the unsorted collection, X will precede Y is the sorted collection.
		/// </summary>
		/// <remarks>Values are compared by using the IComparable&lt;T&gt;
		/// interfaces implementation on the type T.</remarks>
		/// <param name="collection">The collection to sort.</param>
		/// <returns>An array containing the sorted version of the collection.</returns>
		public static T[] StableSort<T>(IEnumerable<T> collection)
			where T : IComparable<T>
		{
			return StableSort(collection, Comparer<T>.Default);
		}

		/// <summary>
		/// Creates a sorted version of a collection. The sort is stable, which means that if items X and Y are equal,
		/// and X precedes Y in the unsorted collection, X will precede Y is the sorted collection. A supplied IComparer&lt;T&gt; is used
		/// to compare the items in the collection. 
		/// </summary>
		/// <param name="collection">The collection to sort.</param>
		/// <param name="comparer">The comparer instance used to compare items in the collection. Only
		/// the Compare method is used.</param>
		/// <returns>An array containing the sorted version of the collection.</returns>
		public static T[] StableSort<T>(IEnumerable<T> collection, IComparer<T> comparer)
		{
			T[] array;

			if (collection == null)
				throw new ArgumentNullException("collection");
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			array = ToArray(collection);

			StableSortInPlace(ReadWriteList(array), comparer);
			return array;
		}

		/// <summary>
		/// Creates a sorted version of a collection. The sort is stable, which means that if items X and Y are equal,
		/// and X precedes Y in the unsorted collection, X will precede Y is the sorted collection. 
		/// A supplied Comparison&lt;T&gt; delegate is used
		/// to compare the items in the collection.
		/// </summary>
		/// <remarks>Values are compared by using the IComparable&lt;T&gt;
		/// interfaces implementation on the type T.</remarks>
		/// <param name="collection">The collection to sort.</param>
		/// <param name="comparison">The comparison delegate used to compare items in the collection.</param>
		/// <returns>An array containing the sorted version of the collection.</returns>
		public static T[] StableSort<T>(IEnumerable<T> collection, Comparison<T> comparison)
		{
			return StableSort(collection, Comparers.ComparerFromComparison(comparison));
		}

		/// <summary>
		/// Sorts a list or array in place. The sort is stable, which means that if items X and Y are equal,
		/// and X precedes Y in the unsorted collection, X will precede Y is the sorted collection. 
		/// </summary>
		/// <remarks><para>Values are compared by using the IComparable&lt;T&gt;
		/// interfaces implementation on the type T.</para>
		/// <para>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</para></remarks>
		/// <param name="list">The list or array to sort.</param>
		public static void StableSortInPlace<T>(IList<T> list)
			where T : IComparable<T>
		{
			StableSortInPlace(list, Comparer<T>.Default);
		}

		/// <summary>
		/// Sorts a list or array in place. The sort is stable, which means that if items X and Y are equal,
		/// and X precedes Y in the unsorted collection, X will precede Y is the sorted collection. 
		/// A supplied IComparer&lt;T&gt; is used
		/// to compare the items in the list. 
		/// </summary>
		/// <remarks>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</remarks>
		/// <param name="list">The list or array to sort.</param>
		/// <param name="comparer">The comparer instance used to compare items in the collection. Only
		/// the Compare method is used.</param>
		public static void StableSortInPlace<T>(IList<T> list, IComparer<T> comparer)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			if (list is T[])
				list = new ArrayWrapper<T>((T[]) list);
			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			// The stable sort algorithms also uses QuickSort. An additional array of indices (order) is
			// used to maintain the original order of items in the array, and that array is used
			// as a secondary compare when the primary compare returns equal.
			int[] order = new int[list.Count];
			for (int x = 0; x < order.Length; ++x)
				order[x] = x;

			// Instead of a recursive procedure, we use an explicit stack to hold
			// ranges that we still need to sort.
			int[] leftStack = new int[32], rightStack = new int[32];
			int stackPtr = 0;

			int l = 0; // the inclusive left edge of the current range we are sorting.
			int r = list.Count - 1; // the inclusive right edge of the current range we are sorting.
			T partition; // The partition value.
			int order_partition; // The order of the partition value;
			int c; // holds the result of a comparison temporarily.

			// Loop until we have nothing left to sort. On each iteration, l and r contains the bounds
			// of something to sort (unless r <= l), and leftStack/rightStack have a stack of unsorted
			// pieces (unles stackPtr == 0).
			for (;;)
			{
				if (l == r - 1)
				{
					// We have exactly 2 elements to sort. Compare them and swap if needed.
					T e1, e2;
					int o1, o2;
					e1 = list[l];
					o1 = order[l];
					e2 = list[r];
					o2 = order[r];
					if ((c = comparer.Compare(e1, e2)) > 0 || (c == 0 && o1 > o2))
					{
						list[r] = e1;
						order[r] = o1;
						list[l] = e2;
						order[l] = o2;
					}
					l = r; // sort complete, find other work from the stack.
				}
				else if (l < r)
				{
					// Sort the items in the inclusive range l .. r

					// Get the left, middle, and right-most elements and sort them, yielding e1=smallest, e2=median, e3=largest
					int m = l + (r - l)/2;
					T e1 = list[l], e2 = list[m], e3 = list[r], temp;
					int o1 = order[l], o2 = order[m], o3 = order[r], otemp;
					if ((c = comparer.Compare(e1, e2)) > 0 || (c == 0 && o1 > o2))
					{
						temp = e1;
						e1 = e2;
						e2 = temp;
						otemp = o1;
						o1 = o2;
						o2 = otemp;
					}
					if ((c = comparer.Compare(e1, e3)) > 0 || (c == 0 && o1 > o3))
					{
						temp = e3;
						e3 = e2;
						e2 = e1;
						e1 = temp;
						otemp = o3;
						o3 = o2;
						o2 = o1;
						o1 = otemp;
					}
					else if ((c = comparer.Compare(e2, e3)) > 0 || (c == 0 && o2 > o3))
					{
						temp = e2;
						e2 = e3;
						e3 = temp;
						otemp = o2;
						o2 = o3;
						o3 = otemp;
					}

					if (l == r - 2)
					{
						// We have exactly 3 elements to sort, and we've done that. Store back and we're done.
						list[l] = e1;
						list[m] = e2;
						list[r] = e3;
						order[l] = o1;
						order[m] = o2;
						order[r] = o3;
						l = r; // sort complete, find other work from the stack.
					}
					else
					{
						// Put the smallest at the left, largest in the middle, and the median at the right (which is the partitioning value)
						list[l] = e1;
						order[l] = o1;
						list[m] = e3;
						order[m] = o3;
						list[r] = partition = e2;
						order[r] = order_partition = o2;

						// Partition into three parts, items <= partition, items == partition, and items >= partition
						int i = l, j = r;
						T item_i, item_j;
						int order_i, order_j;
						for (;;)
						{
							do
							{
								++i;
								item_i = list[i];
								order_i = order[i];
							} while ((c = comparer.Compare(item_i, partition)) < 0 || (c == 0 && order_i < order_partition));

							do
							{
								--j;
								item_j = list[j];
								order_j = order[j];
							} while ((c = comparer.Compare(item_j, partition)) > 0 || (c == 0 && order_j > order_partition));

							if (j < i)
								break;

							list[i] = item_j;
							list[j] = item_i; // swap items to continue the partition.
							order[i] = order_j;
							order[j] = order_i;
						}

						// Move the partition value into place.
						list[r] = item_i;
						order[r] = order_i;
						list[i] = partition;
						order[i] = order_partition;
						++i;

						// We have partitioned the list. 
						//    Items in the inclusive range l .. j are <= partition.
						//    Items in the inclusive range i .. r are >= partition.
						//    Items in the inclusive range j+1 .. i - 1 are == partition (and in the correct final position).
						// We now need to sort l .. j and i .. r.
						// To do this, we stack one of the lists for later processing, and change l and r to the other list.
						// If we always stack the larger of the two sub-parts, the stack cannot get greater
						// than log2(Count) in size; i.e., a 32-element stack is enough for the maximum list size.
						if ((j - l) > (r - i))
						{
							// The right partition is smaller. Stack the left, and get ready to sort the right.
							leftStack[stackPtr] = l;
							rightStack[stackPtr] = j;
							l = i;
						}
						else
						{
							// The left partition is smaller. Stack the right, and get ready to sort the left.
							leftStack[stackPtr] = i;
							rightStack[stackPtr] = r;
							r = j;
						}
						++stackPtr;
					}
				}
				else if (stackPtr > 0)
				{
					// We have a stacked sub-list to sort. Pop it off and sort it.
					--stackPtr;
					l = leftStack[stackPtr];
					r = rightStack[stackPtr];
				}
				else
				{
					// We have nothing left to sort.
					break;
				}
			}
		}

		/// <summary>
		/// Sorts a list or array in place. The sort is stable, which means that if items X and Y are equal,
		/// and X precedes Y in the unsorted collection, X will precede Y is the sorted collection. 
		/// A supplied Comparison&lt;T&gt; delegate is used
		/// to compare the items in the list.
		/// </summary>
		/// <remarks>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</remarks>
		/// <param name="list">The list or array to sort.</param>
		/// <param name="comparison">The comparison delegate used to compare items in the collection.</param>
		public static void StableSortInPlace<T>(IList<T> list, Comparison<T> comparison)
		{
			StableSortInPlace(list, Comparers.ComparerFromComparison(comparison));
		}

		/// <summary>
		/// Searches a sorted list for an item via binary search. The list must be sorted
		/// by the natural ordering of the type (it's implementation of IComparable&lt;T&gt;).
		/// </summary>
		/// <param name="list">The sorted list to search.</param>
		/// <param name="item">The item to search for.</param>
		/// <param name="index">Returns the first index at which the item can be found. If the return
		/// value is zero, indicating that <paramref name="item"/> was not present in the list, then this
		/// returns the index at which <paramref name="item"/> could be inserted to maintain the sorted
		/// order of the list.</param>
		/// <returns>The number of items equal to <paramref name="item"/> that appear in the list.</returns>
		public static int BinarySearch<T>(IList<T> list, T item, out int index)
			where T : IComparable<T>
		{
			return BinarySearch(list, item, Comparer<T>.Default, out index);
		}

		/// <summary>
		/// Searches a sorted list for an item via binary search. The list must be sorted
		/// by the ordering in the passed instance of IComparer&lt;T&gt;.
		/// </summary>
		/// <param name="list">The sorted list to search.</param>
		/// <param name="item">The item to search for.</param>
		/// <param name="comparer">The comparer instance used to sort the list. Only
		/// the Compare method is used.</param>
		/// <param name="index">Returns the first index at which the item can be found. If the return
		/// value is zero, indicating that <paramref name="item"/> was not present in the list, then this
		/// returns the index at which <paramref name="item"/> could be inserted to maintain the sorted
		/// order of the list.</param>
		/// <returns>
		/// The number of items equal to <paramref name="item"/> that appear in the list.
		/// </returns>
		public static int BinarySearch<T>(IList<T> list, T item, IComparer<T> comparer, out int index)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			int l = 0;
			int r = list.Count;

			while (r > l)
			{
				int m = l + (r - l)/2;
				T middleItem = list[m];
				int comp = comparer.Compare(middleItem, item);
				if (comp < 0)
				{
					// middleItem < item
					l = m + 1;
				}
				else if (comp > 0)
				{
					r = m;
				}
				else
				{
					// Found something equal to item at m. Now we need to find the start and end of this run of equal items.
					int lFound = l, rFound = r, found = m;

					// Find the start of the run.
					l = lFound;
					r = found;
					while (r > l)
					{
						m = l + (r - l)/2;
						middleItem = list[m];
						comp = comparer.Compare(middleItem, item);
						if (comp < 0)
						{
							// middleItem < item
							l = m + 1;
						}
						else
						{
							r = m;
						}
					}
					Debug.Assert(l == r, "Left and Right were not equal");
					index = l;

					// Find the end of the run.
					l = found;
					r = rFound;
					while (r > l)
					{
						m = l + (r - l)/2;
						middleItem = list[m];
						comp = comparer.Compare(middleItem, item);
						if (comp <= 0)
						{
							// middleItem <= item
							l = m + 1;
						}
						else
						{
							r = m;
						}
					}
					Debug.Assert(l == r, "Left and Right were not equal");
					return l - index;
				}
			}

			// We did not find the item. l and r must be equal. 
			Debug.Assert(l == r);
			index = l;
			return 0;
		}

		/// <summary>
		/// Searches a sorted list for an item via binary search. The list must be sorted
		/// by the ordering in the passed Comparison&lt;T&gt; delegate.
		/// </summary>
		/// <param name="list">The sorted list to search.</param>
		/// <param name="item">The item to search for.</param>
		/// <param name="comparison">The comparison delegate used to sort the list.</param>
		/// <param name="index">Returns the first index at which the item can be found. If the return
		/// value is zero, indicating that <paramref name="item"/> was not present in the list, then this
		/// returns the index at which <paramref name="item"/> could be inserted to maintain the sorted
		/// order of the list.</param>
		/// <returns>The number of items equal to <paramref name="item"/> that appear in the list.</returns>
		public static int BinarySearch<T>(IList<T> list, T item, Comparison<T> comparison, out int index)
		{
			return BinarySearch(list, item, Comparers.ComparerFromComparison(comparison), out index);
		}

		/// <summary>
		/// Merge several sorted collections into a single sorted collection. Each input collection must be sorted
		/// by the natural ordering of the type (it's implementation of IComparable&lt;T&gt;). The merging
		/// is stable; equal items maintain their ordering, and equal items in different collections are placed
		/// in the order of the collections.
		/// </summary>
		/// <param name="collections">The set of collections to merge. In many languages, this parameter
		/// can be specified as several individual parameters.</param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates all the items in all the collections
		/// in sorted order. </returns>
		public static IEnumerable<T> MergeSorted<T>(params IEnumerable<T>[] collections)
			where T : IComparable<T>
		{
			return MergeSorted(Comparer<T>.Default, collections);
		}

		/// <summary>
		/// Merge several sorted collections into a single sorted collection. Each input collection must be sorted
		/// by the ordering in the passed instance of IComparer&lt;T&gt;. The merging
		/// is stable; equal items maintain their ordering, and equal items in different collections are placed
		/// in the order of the collections.
		/// </summary>
		/// <param name="collections">The set of collections to merge. In many languages, this parameter
		/// can be specified as several individual parameters.</param>
		/// <param name="comparer">The comparer instance used to sort the list. Only
		/// the Compare method is used.</param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates all the items in all the collections
		/// in sorted order. </returns>
		public static IEnumerable<T> MergeSorted<T>(IComparer<T> comparer, params IEnumerable<T>[] collections)
		{
			if (collections == null)
				throw new ArgumentNullException("collections");
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			IEnumerator<T>[] enumerators = new IEnumerator<T>[collections.Length];
			bool[] more = new bool[collections.Length];
			T smallestItem = default(T);
			int smallestItemIndex;

			try
			{
				// Get enumerators from each collection, and advance to the first element.
				for (int i = 0; i < collections.Length; ++i)
				{
					if (collections[i] != null)
					{
						enumerators[i] = collections[i].GetEnumerator();
						more[i] = enumerators[i].MoveNext();
					}
				}

				for (;;)
				{
					// Find the smallest item, and which collection it is in.
					smallestItemIndex = -1; // -1 indicates no smallest yet.
					for (int i = 0; i < enumerators.Length; ++i)
					{
						if (more[i])
						{
							T item = enumerators[i].Current;
							if (smallestItemIndex < 0 || comparer.Compare(smallestItem, item) > 0)
							{
								smallestItemIndex = i;
								smallestItem = item;
							}
						}
					}

					// If no smallest item found, we're done.
					if (smallestItemIndex == -1)
						yield break;

					// Yield the smallest item.
					yield return smallestItem;

					// Advance the enumerator it came from.
					more[smallestItemIndex] = enumerators[smallestItemIndex].MoveNext();
				}
			}
			finally
			{
				// Dispose all enumerators.
				foreach (IEnumerator<T> e in enumerators)
				{
					if (e != null)
						e.Dispose();
				}
			}
		}

		/// <summary>
		/// Merge several sorted collections into a single sorted collection. Each input collection must be sorted
		/// by the ordering in the passed Comparison&lt;T&gt; delegate. The merging
		/// is stable; equal items maintain their ordering, and equal items in different collections are placed
		/// in the order of the collections.
		/// </summary>
		/// <param name="collections">The set of collections to merge. In many languages, this parameter
		/// can be specified as several individual parameters.</param>
		/// <param name="comparison">The comparison delegate used to sort the collections.</param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates all the items in all the collections
		/// in sorted order. </returns>
		public static IEnumerable<T> MergeSorted<T>(Comparison<T> comparison, params IEnumerable<T>[] collections)
		{
			return MergeSorted(Comparers.ComparerFromComparison(comparison), collections);
		}


		/// <summary>
		/// Performs a lexicographical comparison of two sequences of values. A lexicographical comparison compares corresponding
		/// pairs of elements from two sequences in order. If the first element of sequence1 is less than the first element of sequence2, 
		/// then the comparison ends and the first sequence is lexicographically less than the second. If the first elements of each sequence
		/// are equal, then the comparison proceeds to the second element of each sequence. If one sequence is shorter than the other,
		/// but corresponding elements are all equal, then the shorter sequence is considered less than the longer one.
		/// </summary>
		/// <remarks>T must implement either IComparable&lt;T&gt; and this implementation is used
		/// to compare the items. </remarks>
		/// <typeparam name="T">Types of items to compare. This type must implement IComparable&lt;T&gt; to allow 
		/// items to be compared.</typeparam>
		/// <param name="sequence1">The first sequence to compare.</param>
		/// <param name="sequence2">The second sequence to compare.</param>
		/// <returns>Less than zero if <paramref name="sequence1"/> is lexicographically less than <paramref name="sequence2"/>.
		/// Greater than zero if <paramref name="sequence1"/> is lexicographically greater than <paramref name="sequence2"/>.
		/// Zero if <paramref name="sequence1"/> is equal to <paramref name="sequence2"/>.</returns>
		/// <exception cref="NotSupportedException">T does not implement IComparable&lt;T&gt; or IComparable.</exception>
		public static int LexicographicalCompare<T>(IEnumerable<T> sequence1, IEnumerable<T> sequence2)
			where T : IComparable<T>
		{
			return LexicographicalCompare(sequence1, sequence2, Comparer<T>.Default);
		}

		/// <summary>
		/// Performs a lexicographical comparison of two sequences of values, using a supplied comparison delegate. A lexicographical comparison compares corresponding
		/// pairs of elements from two sequences in order. If the first element of sequence1 is less than the first element of sequence2, 
		/// then the comparison ends and the first sequence is lexicographically less than the second. If the first elements of each sequence
		/// are equal, then the comparison proceeds to the second element of each sequence. If one sequence is shorter than the other,
		/// but corresponding elements are all equal, then the shorter sequence is considered less than the longer one.
		/// </summary>
		/// <typeparam name="T">Types of items to compare.</typeparam>
		/// <param name="sequence1">The first sequence to compare.</param>
		/// <param name="sequence2">The second sequence to compare.</param>
		/// <param name="comparison">The IComparison&lt;T&gt; delegate to compare items. 
		/// Only the Compare member function of this interface is called.</param>
		/// <returns>Less than zero if <paramref name="sequence1"/> is lexicographically less than <paramref name="sequence2"/>.
		/// Greater than zero if <paramref name="sequence1"/> is lexicographically greater than <paramref name="sequence2"/>.
		/// Zero if <paramref name="sequence1"/> is equal to <paramref name="sequence2"/>.</returns>
		public static int LexicographicalCompare<T>(IEnumerable<T> sequence1, IEnumerable<T> sequence2, Comparison<T> comparison)
		{
			return LexicographicalCompare(sequence1, sequence2, Comparers.ComparerFromComparison(comparison));
		}

		/// <summary>
		/// Performs a lexicographical comparison of two sequences of values, using a supplied comparer interface. A lexicographical comparison compares corresponding
		/// pairs of elements from two sequences in order. If the first element of sequence1 is less than the first element of sequence2, 
		/// then the comparison ends and the first sequence is lexicographically less than the second. If the first elements of each sequence
		/// are equal, then the comparison proceeds to the second element of each sequence. If one sequence is shorter than the other,
		/// but corresponding elements are all equal, then the shorter sequence is considered less than the longer one.
		/// </summary>
		/// <typeparam name="T">Types of items to compare.</typeparam>
		/// <param name="sequence1">The first sequence to compare.</param>
		/// <param name="sequence2">The second sequence to compare.</param>
		/// <param name="comparer">The IComparer&lt;T&gt; used to compare items. 
		/// Only the Compare member function of this interface is called.</param>
		/// <returns>Less than zero if <paramref name="sequence1"/> is lexicographically less than <paramref name="sequence2"/>.
		/// Greater than zero if <paramref name="sequence1"/> is lexicographically greater than <paramref name="sequence2"/>.
		/// Zero if <paramref name="sequence1"/> is equal to <paramref name="sequence2"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="sequence1"/>, <paramref name="sequence2"/>, or 
		/// <paramref name="comparer"/> is null.</exception>
		public static int LexicographicalCompare<T>(IEnumerable<T> sequence1, IEnumerable<T> sequence2, IComparer<T> comparer)
		{
			if (sequence1 == null)
				throw new ArgumentNullException("sequence1");
			if (sequence2 == null)
				throw new ArgumentNullException("sequence2");
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			using (IEnumerator<T> enum1 = sequence1.GetEnumerator(), enum2 = sequence2.GetEnumerator())
			{
				bool continue1, continue2;

				for (;;)
				{
					continue1 = enum1.MoveNext();
					continue2 = enum2.MoveNext();
					if (!continue1 || !continue2)
						break;

					int compare = comparer.Compare(enum1.Current, enum2.Current);
					if (compare != 0)
						return compare;
				}

				// If both continue1 and continue2 are false, we reached the end of both sequences at the same
				// time and the sequences are equal. Otherwise, the shorter sequence is considered first.
				if (continue1 == continue2)
					return 0;
				else if (continue1)
					return 1;
				else
					return -1;
			}
		}

		#endregion Sorting

		#region Comparers/Comparison utilities 

		/// <summary>
		/// A private class used by the LexicographicalComparer method to compare sequences
		/// (IEnumerable) of T by there Lexicographical ordering.
		/// </summary>
		[Serializable]
		private class LexicographicalComparerClass<T> : IComparer<IEnumerable<T>>
		{
			private readonly IComparer<T> itemComparer;

			/// <summary>
			/// Creates a new instance that comparer sequences of T by their lexicographical
			/// ordered.
			/// </summary>
			/// <param name="itemComparer">The IComparer used to compare individual items of type T.</param>
			public LexicographicalComparerClass(IComparer<T> itemComparer)
			{
				this.itemComparer = itemComparer;
			}

			public int Compare(IEnumerable<T> x, IEnumerable<T> y)
			{
				return LexicographicalCompare(x, y, itemComparer);
			}


			// For comparing this comparer to others.

			public override bool Equals(object obj)
			{
				if (obj is LexicographicalComparerClass<T>)
					return this.itemComparer.Equals(((LexicographicalComparerClass<T>) obj).itemComparer);
				else
					return false;
			}

			public override int GetHashCode()
			{
				return itemComparer.GetHashCode();
			}
		}

		/// <summary>
		/// An IComparer instance that can be used to reverse the sense of 
		/// a wrapped IComparer instance.
		/// </summary>
		[Serializable]
		private class ReverseComparerClass<T> : IComparer<T>
		{
			private readonly IComparer<T> comparer;

			/// <summary>
			/// </summary>
			/// <param name="comparer">The comparer to reverse.</param>
			public ReverseComparerClass(IComparer<T> comparer)
			{
				this.comparer = comparer;
			}

			public int Compare(T x, T y)
			{
				return - comparer.Compare(x, y);
			}

			// For comparing this comparer to others.

			public override bool Equals(object obj)
			{
				if (obj is ReverseComparerClass<T>)
					return this.comparer.Equals(((ReverseComparerClass<T>) obj).comparer);
				else
					return false;
			}

			public override int GetHashCode()
			{
				return comparer.GetHashCode();
			}
		}

		/// <summary>
		/// A class, implementing IEqualityComparer&lt;T&gt;, that compares objects
		/// for object identity only. Only Equals and GetHashCode can be used;
		/// this implementation is not appropriate for ordering.
		/// </summary>
		[Serializable]
		private class IdentityComparer<T> : IEqualityComparer<T>
			where T : class
		{
			public bool Equals(T x, T y)
			{
				return (x == y);
			}

			public int GetHashCode(T obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}

			// For comparing two IComparers to see if they compare the same thing.
			public override bool Equals(object obj)
			{
				return (obj != null && obj is IdentityComparer<T>);
			}

			// For comparing two IComparers to see if they compare the same thing.
			public override int GetHashCode()
			{
				return 0x7143DDEF;
			}
		}

		/// <summary>
		/// A private class used to implement GetCollectionEqualityComparer(). This
		/// class implements IEqualityComparer&lt;IEnumerable&lt;T&gt;gt; to compare
		/// two enumerables for equality, where order is significant.
		/// </summary>
		[Serializable]
		private class CollectionEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
		{
			private readonly IEqualityComparer<T> equalityComparer;

			public CollectionEqualityComparer(IEqualityComparer<T> equalityComparer)
			{
				this.equalityComparer = equalityComparer;
			}

			public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
			{
				return EqualCollections(x, y, equalityComparer);
			}

			public int GetHashCode(IEnumerable<T> obj)
			{
				int hash = 0x374F293E;
				foreach (T t in obj)
				{
					int itemHash = Util.GetHashCode(t, equalityComparer);
					hash += itemHash;
					hash = (hash << 9) | (hash >> 23);
				}

				return hash & 0x7FFFFFFF;
			}
		}

		/// <summary>
		/// A private class used to implement GetSetEqualityComparer(). This
		/// class implements IEqualityComparer&lt;IEnumerable&lt;T&gt;gt; to compare
		/// two enumerables for equality, where order is not significant.
		/// </summary>
		[Serializable]
		private class SetEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
		{
			private readonly IEqualityComparer<T> equalityComparer;

			public SetEqualityComparer(IEqualityComparer<T> equalityComparer)
			{
				this.equalityComparer = equalityComparer;
			}

			public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
			{
				return EqualSets(x, y, equalityComparer);
			}

			public int GetHashCode(IEnumerable<T> obj)
			{
				int hash = 0x624F273C;
				foreach (T t in obj)
				{
					int itemHash = Util.GetHashCode(t, equalityComparer);
					hash += itemHash;
				}

				return hash & 0x7FFFFFFF;
			}
		}

		/// <summary>
		/// Creates an IComparer instance that can be used for comparing ordered
		/// sequences of type T; that is IEnumerable&lt;Tgt;. This comparer can be used
		/// for collections or algorithms that use sequences of T as an item type. The Lexicographical
		/// ordered of sequences is for comparison.
		/// </summary>
		/// <remarks>T must implement either IComparable&lt;T&gt; and this implementation is used
		/// to compare the items. </remarks>
		/// <returns>At IComparer&lt;IEnumerable&lt;T&gt;&gt; that compares sequences of T.</returns>
		public static IComparer<IEnumerable<T>> GetLexicographicalComparer<T>()
			where T : IComparable<T>
		{
			return GetLexicographicalComparer(Comparer<T>.Default);
		}

		/// <summary>
		/// Creates an IComparer instance that can be used for comparing ordered
		/// sequences of type T; that is IEnumerable&lt;Tgt;. This comparer can be uses
		/// for collections or algorithms that use sequences of T as an item type. The Lexicographics
		/// ordered of sequences is for comparison.
		/// </summary>
		/// <param name="comparer">A comparer instance used to compare individual items of type T.</param>
		/// <returns>At IComparer&lt;IEnumerable&lt;T&gt;&gt; that compares sequences of T.</returns>
		public static IComparer<IEnumerable<T>> GetLexicographicalComparer<T>(IComparer<T> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			return new LexicographicalComparerClass<T>(comparer);
		}

		/// <summary>
		/// Creates an IComparer instance that can be used for comparing ordered
		/// sequences of type T; that is IEnumerable&lt;Tgt;. This comparer can be uses
		/// for collections or algorithms that use sequences of T as an item type. The Lexicographics
		/// ordered of sequences is for comparison.
		/// </summary>
		/// <param name="comparison">A comparison delegate used to compare individual items of type T.</param>
		/// <returns>At IComparer&lt;IEnumerable&lt;T&gt;&gt; that compares sequences of T.</returns>
		public static IComparer<IEnumerable<T>> GetLexicographicalComparer<T>(Comparison<T> comparison)
		{
			if (comparison == null)
				throw new ArgumentNullException("comparison");

			return new LexicographicalComparerClass<T>(Comparers.ComparerFromComparison(comparison));
		}

		/// <summary>
		/// Reverses the order of comparison of an IComparer&lt;T&gt;. The resulting comparer can be used,
		/// for example, to sort a collection in descending order. Equality and hash codes are unchanged.
		/// </summary>
		/// <typeparam name="T">The type of items thta are being compared.</typeparam>
		/// <param name="comparer">The comparer to reverse.</param>
		/// <returns>An IComparer&lt;T&gt; that compares items in the reverse order of <paramref name="comparer"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
		public static IComparer<T> GetReverseComparer<T>(IComparer<T> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			return new ReverseComparerClass<T>(comparer);
		}

		/// <summary>
		/// Gets an IEqualityComparer&lt;T&gt; instance that can be used to compare objects
		/// of type T for object identity only. Two objects compare equal only if they
		/// are references to the same object. 
		/// </summary>
		/// <returns>An IEqualityComparer&lt;T&gt; instance for identity comparison.</returns>
		public static IEqualityComparer<T> GetIdentityComparer<T>()
			where T : class
		{
			return new IdentityComparer<T>();
		}

		/// <summary>
		/// Reverses the order of comparison of an Comparison&lt;T&gt;. The resulting comparison can be used,
		/// for example, to sort a collection in descending order. 
		/// </summary>
		/// <typeparam name="T">The type of items that are being compared.</typeparam>
		/// <param name="comparison">The comparison to reverse.</param>
		/// <returns>A Comparison&lt;T&gt; that compares items in the reverse order of <paramref name="comparison"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="comparison"/> is null.</exception>
		public static Comparison<T> GetReverseComparison<T>(Comparison<T> comparison)
		{
			if (comparison == null)
				throw new ArgumentNullException("comparison");

			return delegate(T x, T y)
				{
					return -comparison(x, y);
				};
		}

		/// <summary>
		/// Given a comparison delegate that compares two items of type T, gets an
		/// IComparer&lt;T&gt; instance that performs the same comparison.
		/// </summary>
		/// <param name="comparison">The comparison delegate to use.</param>
		/// <returns>An IComparer&lt;T&gt; that performs the same comparing operation
		/// as <paramref name="comparison"/>.</returns>
		public static IComparer<T> GetComparerFromComparison<T>(Comparison<T> comparison)
		{
			if (comparison == null)
				throw new ArgumentNullException("comparison");

			return Comparers.ComparerFromComparison(comparison);
		}

		/// <summary>
		/// Given in IComparer&lt;T&gt; instenace that comparers two items from type T, 
		/// gets a Comparison delegate that performs the same comparison.
		/// </summary>
		/// <param name="comparer">The IComparer&lt;T&gt; instance to use.</param>
		/// <returns>A Comparison&lt;T&gt; delegate that performans the same comparing
		/// operation as <paramref name="comparer"/>.</returns>
		public static Comparison<T> GetComparisonFromComparer<T>(IComparer<T> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			return comparer.Compare;
		}

		/// <summary>
		/// Gets an IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation 
		/// that can be used to compare collections of elements (of type T). Two collections
		/// of T's are equal if they have the same number of items, and corresponding 
		/// items are equal, considered in order. This is the same notion of equality as
		/// in Algorithms.EqualCollections, but encapsulated in an IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation.
		/// </summary>
		/// <example>
		/// The following code creates a Dictionary where the keys are a collection of strings.
		/// <code>
		///     Dictionary&lt;IEnumerable&lt;string&gt;, int&gt; = 
		///         new Dictionary&lt;IEnumerable&lt;string&gt;, int&gt;(Algorithms.GetCollectionEqualityComparer&lt;string&gt;());
		/// </code>
		/// </example>
		/// <returns>IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation suitable for 
		/// comparing collections of T for equality.</returns>
		/// <seealso cref="Algorithms.EqualCollections{T}"/>
		public static IEqualityComparer<IEnumerable<T>> GetCollectionEqualityComparer<T>()
		{
			return GetCollectionEqualityComparer(EqualityComparer<T>.Default);
		}

		/// <summary>
		/// <para>Gets an IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation 
		/// that can be used to compare collections of elements (of type T). Two collections
		/// of T's are equal if they have the same number of items, and corresponding 
		/// items are equal, considered in order. This is the same notion of equality as
		/// in Algorithms.EqualCollections, but encapsulated in an IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation.</para>
		/// <para>An IEqualityComparer&lt;T&gt; is used to determine if individual T's are equal</para>
		/// </summary>
		/// <example>
		/// The following code creates a Dictionary where the keys are a collection of strings, compared in a case-insensitive way
		/// <code>
		///     Dictionary&lt;IEnumerable&lt;string&gt;, int&gt; = 
		///         new Dictionary&lt;IEnumerable&lt;string&gt;, int&gt;(Algorithms.GetCollectionEqualityComparer&lt;string&gt;(StringComparer.CurrentCultureIgnoreCase));
		/// </code>
		/// </example>
		/// <param name="equalityComparer">An IEqualityComparer&lt;T&gt; implementation used to compare individual T's.</param>
		/// <returns>IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation suitable for 
		/// comparing collections of T for equality.</returns>
		/// <seealso cref="Algorithms.EqualCollections{T}"/>
		public static IEqualityComparer<IEnumerable<T>> GetCollectionEqualityComparer<T>(IEqualityComparer<T> equalityComparer)
		{
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			return new CollectionEqualityComparer<T>(equalityComparer);
		}

		/// <summary>
		/// <para>Gets an IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation 
		/// that can be used to compare collections of elements (of type T). Two collections
		/// of T's are equal if they have the same number of items, and corresponding 
		/// items are equal, without regard to order. This is the same notion of equality as
		/// in Algorithms.EqualSets, but encapsulated in an IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation.</para>
		/// <para>An IEqualityComparer&lt;T&gt; is used to determine if individual T's are equal</para>
		/// </summary>
		/// <example>
		/// The following code creates a Dictionary where the keys are a set of strings, without regard to order
		/// <code>
		///     Dictionary&lt;IEnumerable&lt;string&gt;, int&gt; = 
		///         new Dictionary&lt;IEnumerable&lt;string&gt;, int&gt;(Algorithms.GetSetEqualityComparer&lt;string&gt;(StringComparer.CurrentCultureIgnoreCase));
		/// </code>
		/// </example>
		/// <returns>IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation suitable for 
		/// comparing collections of T for equality, without regard to order.</returns>
		/// <seealso cref="Algorithms.EqualSets{T}"/>
		public static IEqualityComparer<IEnumerable<T>> GetSetEqualityComparer<T>()
		{
			return GetSetEqualityComparer(EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Gets an IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation 
		/// that can be used to compare collections of elements (of type T). Two collections
		/// of T's are equal if they have the same number of items, and corresponding 
		/// items are equal, without regard to order. This is the same notion of equality as
		/// in Algorithms.EqualSets, but encapsulated in an IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation.
		/// </summary>
		/// <example>
		/// The following code creates a Dictionary where the keys are a set of strings, without regard to order
		/// <code>
		///     Dictionary&lt;IEnumerable&lt;string&gt;, int&gt; = 
		///         new Dictionary&lt;IEnumerable&lt;string&gt;, int&gt;(Algorithms.GetSetEqualityComparer&lt;string&gt;());
		/// </code>
		/// </example>
		/// <param name="equalityComparer">An IEqualityComparer&lt;T&gt; implementation used to compare individual T's.</param>
		/// <returns>IEqualityComparer&lt;IEnumerable&lt;T&gt;&gt; implementation suitable for 
		/// comparing collections of T for equality, without regard to order.</returns>
		/// <seealso cref="Algorithms.EqualSets"/>
		public static IEqualityComparer<IEnumerable<T>> GetSetEqualityComparer<T>(IEqualityComparer<T> equalityComparer)
		{
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			return new SetEqualityComparer<T>(equalityComparer);
		}

		#endregion Sorting

		#region Predicate operations

		/// <summary>
		/// Determines if a collection contains any item that satisfies the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="collection">The collection to check all the items in.</param>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <returns>True if the collection contains one or more items that satisfy the condition
		/// defined by <paramref name="predicate"/>. False if the collection does not contain
		/// an item that satisfies <paramref name="predicate"/>.</returns>
		public static bool Exists<T>(IEnumerable<T> collection, Predicate<T> predicate)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			foreach (T item in collection)
			{
				if (predicate(item))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Determines if all of the items in the collection satisfy the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="collection">The collection to check all the items in.</param>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <returns>True if all of the items in the collection satisfy the condition
		/// defined by <paramref name="predicate"/>, or if the collection is empty. False if one or more items
		/// in the collection do not satisfy <paramref name="predicate"/>.</returns>
		public static bool TrueForAll<T>(IEnumerable<T> collection, Predicate<T> predicate)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			foreach (T item in collection)
			{
				if (!predicate(item))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Counts the number of items in the collection that satisfy the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="collection">The collection to count items in.</param>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <returns>The number of items in the collection that satisfy <paramref name="predicate"/>.</returns>
		public static int CountWhere<T>(IEnumerable<T> collection, Predicate<T> predicate)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			int count = 0;
			foreach (T item in collection)
			{
				if (predicate(item))
					++count;
			}

			return count;
		}

		/// <summary>
		/// Removes all the items in the collection that satisfy the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <remarks>If the collection if an array or implements IList&lt;T&gt;, an efficient algorithm that
		/// compacts items is used. If not, then ICollection&lt;T&gt;.Remove is used
		/// to remove items from the collection. If the collection is an array or fixed-size list,
		/// the non-removed elements are placed, in order, at the beginning of
		/// the list, and the remaining list items are filled with a default value (0 or null).</remarks>
		/// <param name="collection">The collection to check all the items in.</param>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <returns>Returns a collection of the items that were removed. This collection contains the
		/// items in the same order that they orginally appeared in <paramref name="collection"/>.</returns>
		public static ICollection<T> RemoveWhere<T>(ICollection<T> collection, Predicate<T> predicate)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (predicate == null)
				throw new ArgumentNullException("predicate");
			if (collection is T[])
				collection = new ArrayWrapper<T>((T[]) collection);
			if (collection.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "collection");

			IList<T> list = collection as IList<T>;
			if (list != null)
			{
				T item;
				int i = -1, j = 0;
				int listCount = list.Count;
				List<T> removed = new List<T>();

				// Remove item where predicate is true, compressing items to lower in the list. This is much more
				// efficient than the naive algorithm that uses IList<T>.Remove().
				while (j < listCount)
				{
					item = list[j];
					if (predicate(item))
					{
						removed.Add(item);
					}
					else
					{
						++i;
						if (i != j)
							list[i] = item;
					}
					++j;
				}

				++i;
				if (i < listCount)
				{
					// remove items from the end.
					if (list is IList && ((IList) list).IsFixedSize)
					{
						// An array or similar. Null out the last elements.
						while (i < listCount)
							list[i++] = default(T);
					}
					else
					{
						// Normal list.
						while (i < listCount)
						{
							list.RemoveAt(listCount - 1);
							--listCount;
						}
					}
				}

				return removed;
			}
			else
			{
				// We have to copy all the items to remove to a List, because collections can't be modifed 
				// during an enumeration.
				List<T> removed = new List<T>();

				foreach (T item in collection)
					if (predicate(item))
						removed.Add(item);

				foreach (T item in removed)
					collection.Remove(item);

				return removed;
			}
		}

		/// <summary>
		/// Convert a collection of items by applying a delegate to each item in the collection. The resulting collection
		/// contains the result of applying <paramref name="converter"/> to each item in <paramref name="sourceCollection"/>, in
		/// order.
		/// </summary>
		/// <typeparam name="TSource">The type of items in the collection to convert.</typeparam>
		/// <typeparam name="TDest">The type each item is being converted to.</typeparam>
		/// <param name="sourceCollection">The collection of item being converted.</param>
		/// <param name="converter">A delegate to the method to call, passing each item in <paramref name="sourceCollection"/>.</param>
		/// <returns>The resulting collection from applying <paramref name="converter"/> to each item in <paramref name="sourceCollection"/>, in
		/// order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="sourceCollection"/> or <paramref name="converter"/> is null.</exception>
		public static IEnumerable<TDest> Convert<TSource, TDest>(IEnumerable<TSource> sourceCollection, Converter<TSource, TDest> converter)
		{
			if (sourceCollection == null)
				throw new ArgumentNullException("sourceCollection");
			if (converter == null)
				throw new ArgumentNullException("converter");

			foreach (TSource sourceItem in sourceCollection)
				yield return converter(sourceItem);
		}

		/// <summary>
		/// Creates a delegate that converts keys to values by used a dictionary to map values. Keys
		/// that a not present in the dictionary are converted to the default value (zero or null).
		/// </summary>
		/// <remarks>This delegate can be used as a parameter in Convert or ConvertAll methods to convert
		/// entire collections.</remarks>
		/// <param name="dictionary">The dictionary used to perform the conversion.</param>
		/// <returns>A delegate to a method that converts keys to values. </returns>
		public static Converter<TKey, TValue> GetDictionaryConverter<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
		{
			return GetDictionaryConverter(dictionary, default(TValue));
		}

		/// <summary>
		/// Creates a delegate that converts keys to values by used a dictionary to map values. Keys
		/// that a not present in the dictionary are converted to a supplied default value.
		/// </summary>
		/// <remarks>This delegate can be used as a parameter in Convert or ConvertAll methods to convert
		/// entire collections.</remarks>
		/// <param name="dictionary">The dictionary used to perform the conversion.</param>
		/// <param name="defaultValue">The result of the conversion for keys that are not present in the dictionary.</param>
		/// <returns>A delegate to a method that converts keys to values. </returns>
		/// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
		public static Converter<TKey, TValue> GetDictionaryConverter<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TValue defaultValue)
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			return delegate(TKey key)
				{
					TValue value;
					if (dictionary.TryGetValue(key, out value))
						return value;
					else
						return defaultValue;
				};
		}

		/// <summary>
		/// Performs the specified action on each item in a collection.
		/// </summary>
		/// <param name="collection">The collection to process.</param>
		/// <param name="action">An Action delegate which is invoked for each item in <paramref name="collection"/>.</param>
		public static void ForEach<T>(IEnumerable<T> collection, Action<T> action)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (action == null)
				throw new ArgumentNullException("action");

			foreach (T item in collection)
				action(item);
		}

		/// <summary>
		/// Partition a list or array based on a predicate. After partitioning, all items for which
		/// the predicate returned true precede all items for which the predicate returned false.
		/// </summary>
		/// <remarks>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</remarks>
		/// <param name="list">The list or array to partition.</param>
		/// <param name="predicate">A delegate that defines the partitioning condition.</param>
		/// <returns>The index of the first item in the second half of the partition; i.e., the first item for
		/// which <paramref name="predicate"/> returned false. If the predicate was true for all items
		/// in the list, list.Count is returned.</returns>
		public static int Partition<T>(IList<T> list, Predicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (predicate == null)
				throw new ArgumentNullException("predicate");
			if (list is T[])
				list = new ArrayWrapper<T>((T[]) list);
			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			// Move from opposite ends of the list, swapping when necessary.
			int i = 0, j = list.Count - 1;
			for (;;)
			{
				while (i <= j && predicate(list[i]))
					++i;
				while (i <= j && !predicate(list[j]))
					--j;

				if (i > j)
					break;
				else
				{
					T temp = list[i];
					list[i] = list[j];
					list[j] = temp;
					++i;
					--j;
				}
			}

			return i;
		}

		/// <summary>
		/// Partition a list or array based on a predicate. After partitioning, all items for which
		/// the predicate returned true precede all items for which the predicate returned false. 
		/// The partition is stable, which means that if items X and Y have the same result from
		/// the predicate, and X precedes Y in the original list, X will precede Y in the 
		/// partitioned list.
		/// </summary>
		/// <remarks>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</remarks>
		/// <param name="list">The list or array to partition.</param>
		/// <param name="predicate">A delegate that defines the partitioning condition.</param>
		/// <returns>The index of the first item in the second half of the partition; i.e., the first item for
		/// which <paramref name="predicate"/> returned false. If the predicate was true for all items
		/// in the list, list.Count is returned.</returns>
		public static int StablePartition<T>(IList<T> list, Predicate<T> predicate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (predicate == null)
				throw new ArgumentNullException("predicate");
			if (list is T[])
				list = new ArrayWrapper<T>((T[]) list);
			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			int listCount = list.Count;
			if (listCount == 0)
				return 0;
			T[] temp = new T[listCount];

			// Copy from list to temp buffer, true items at fron, false item (in reverse order) at back.
			int i = 0, j = listCount - 1;
			foreach (T item in list)
			{
				if (predicate(item))
					temp[i++] = item;
				else
					temp[j--] = item;
			}

			// Copy back to the original list.
			int index = 0;
			while (index < i)
			{
				list[index] = temp[index];
				index++;
			}
			j = listCount - 1;
			while (index < listCount)
				list[index++] = temp[j--];

			return i;
		}

		#endregion Predicate operations

		#region Miscellaneous operations on IEnumerable

		/// <summary>
		/// Concatenates all the items from several collections. The collections need not be of the same type, but
		/// must have the same item type.
		/// </summary>
		/// <param name="collections">The set of collections to concatenate. In many languages, this parameter
		/// can be specified as several individual parameters.</param>
		/// <returns>An IEnumerable that enumerates all the items in each of the collections, in order.</returns>
		public static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] collections)
		{
			if (collections == null)
				throw new ArgumentNullException("collections");

			foreach (IEnumerable<T> coll in collections)
			{
				foreach (T item in coll)
					yield return item;
			}
		}

		/// <summary>
		/// Determines if the two collections contain equal items in the same order. The two collections do not need
		/// to be of the same type; it is permissible to compare an array and an OrderedBag, for instance.
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <typeparam name="T">The type of items in the collections.</typeparam>
		/// <param name="collection1">The first collection to compare.</param>
		/// <param name="collection2">The second collection to compare.</param>
		/// <returns>True if the collections have equal items in the same order. If both collections are empty, true is returned.</returns>
		public static bool EqualCollections<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)
		{
			return EqualCollections(collection1, collection2, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Determines if the two collections contain equal items in the same order. The passed 
		/// instance of IEqualityComparer&lt;T&gt; is used for determining if two items are equal.
		/// </summary>
		/// <typeparam name="T">The type of items in the collections.</typeparam>
		/// <param name="collection1">The first collection to compare.</param>
		/// <param name="collection2">The second collection to compare.</param>
		/// <param name="equalityComparer">The IEqualityComparer&lt;T&gt; used to compare items for equality. 
		/// Only the Equals member function of this interface is called.</param>
		/// <returns>True if the collections have equal items in the same order. If both collections are empty, true is returned.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/>, <paramref name="collection2"/>, or
		/// <paramref name="equalityComparer"/> is null.</exception>
		public static bool EqualCollections<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer)
		{
			if (collection1 == null)
				throw new ArgumentNullException("collection1");
			if (collection2 == null)
				throw new ArgumentNullException("collection2");
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			using (IEnumerator<T> enum1 = collection1.GetEnumerator(), enum2 = collection2.GetEnumerator())
			{
				bool continue1, continue2;

				for (;;)
				{
					continue1 = enum1.MoveNext();
					continue2 = enum2.MoveNext();
					if (!continue1 || !continue2)
						break;

					if (!equalityComparer.Equals(enum1.Current, enum2.Current))
						return false; // the two items are not equal.
				}

				// If both continue1 and continue2 are false, we reached the end of both sequences at the same
				// time and found success. If one is true and one is false, the sequences were of difference lengths -- failure.
				return (continue1 == continue2);
			}
		}

		/// <summary>
		/// Determines if the two collections contain "equal" items in the same order. The passed 
		/// BinaryPredicate is used to determine if two items are "equal".
		/// </summary>
		/// <remarks>Since an arbitrary BinaryPredicate is passed to this function, what is being tested
		/// for need not be equality. For example, the following code determines if each integer in
		/// list1 is less than or equal to the corresponding integer in list2.
		/// <code>
		/// List&lt;int&gt; list1, list2;
		/// if (EqualCollections(list1, list2, delegate(int x, int y) { return x &lt;= y; }) {
		///     // the check is true...
		/// }
		/// </code>
		/// </remarks>
		/// <typeparam name="T">The type of items in the collections.</typeparam>
		/// <param name="collection1">The first collection to compare.</param>
		/// <param name="collection2">The second collection to compare.</param>
		/// <param name="predicate">The BinaryPredicate used to compare items for "equality". 
		/// This predicate can compute any relation between two items; it need not represent equality or an equivalence relation.</param>
		/// <returns>True if <paramref name="predicate"/>returns true for each corresponding pair of
		/// items in the two collections. If both collections are empty, true is returned.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection1"/>, <paramref name="collection2"/>, or
		/// <paramref name="predicate"/> is null.</exception>
		public static bool EqualCollections<T>(IEnumerable<T> collection1, IEnumerable<T> collection2, BinaryPredicate<T> predicate)
		{
			if (collection1 == null)
				throw new ArgumentNullException("collection1");
			if (collection2 == null)
				throw new ArgumentNullException("collection2");
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			using (IEnumerator<T> enum1 = collection1.GetEnumerator(), enum2 = collection2.GetEnumerator())
			{
				bool continue1, continue2;

				for (;;)
				{
					continue1 = enum1.MoveNext();
					continue2 = enum2.MoveNext();
					if (!continue1 || !continue2)
						break;

					if (!predicate(enum1.Current, enum2.Current))
						return false; // the two items are not equal.
				}

				// If both continue1 and continue2 are false, we reached the end of both sequences at the same
				// time and found success. If one is true and one is false, the sequences were of difference lengths -- failure.
				return (continue1 == continue2);
			}
		}

		/// <summary>
		/// Create an array with the items in a collection.
		/// </summary>
		/// <remarks>If <paramref name="collection"/> implements ICollection&lt;T&gt;T, then 
		/// ICollection&lt;T&gt;.CopyTo() is used to fill the array. Otherwise, the IEnumerable&lt;T&gt;.GetEnumerator()
		/// is used to fill the array.</remarks>
		/// <typeparam name="T">Element type of the collection.</typeparam>
		/// <param name="collection">Collection to create array from.</param>
		/// <returns>An array with the items from the collection, in enumeration order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public static T[] ToArray<T>(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			ICollection<T> coll = collection as ICollection<T>;
			if (coll != null)
			{
				// Use ICollection methods to do it more efficiently.
				T[] array = new T[coll.Count];
				coll.CopyTo(array, 0);
				return array;
			}
			else
			{
				// We can't allocate the correct size array now, because IEnumerable doesn't
				// have a Count property. We could enumerate twice, once to count and once
				// to copy. Or we could enumerate once, copying to a List, then copy the list
				// to the correct size array. The latter algorithm seems more efficient, although
				// it allocates extra memory for the list which is then discarded.
				List<T> list = new List<T>(collection);
				return list.ToArray();
			}
		}

		/// <summary>
		/// Count the number of items in an IEnumerable&lt;T&gt; collection. If 
		/// a more specific collection type is being used, it is more efficient to use
		/// the Count property, if one is provided.
		/// </summary>
		/// <remarks>If the collection implements ICollection&lt;T&gt;, this method
		/// simply returns ICollection&lt;T&gt;.Count. Otherwise, it enumerates all items
		/// and counts them.</remarks>
		/// <param name="collection">The collection to count items in.</param>
		/// <returns>The number of items in the collection.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		public static int Count<T>(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			// If it's really an ICollection, use that Count property as it is much faster.
			if (collection is ICollection<T>)
				return ((ICollection<T>) collection).Count;

			// Traverse the collection and count the elements.
			int count = 0;

			foreach (T item in collection)
				++count;

			return count;
		}

		/// <summary>
		/// Counts the number of items in the collection that are equal to <paramref name="find"/>.
		/// </summary>
		/// <remarks>The default sense of equality for T is used, as defined by T's
		/// implementation of IComparable&lt;T&gt;.Equals or object.Equals.</remarks>
		/// <param name="collection">The collection to count items in.</param>
		/// <param name="find">The item to compare to.</param>
		/// <returns>The number of items in the collection that are equal to <paramref name="find"/>.</returns>
		public static int CountEqual<T>(IEnumerable<T> collection, T find)
		{
			return CountEqual(collection, find, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Counts the number of items in the collection that are equal to <paramref name="find"/>.
		/// </summary>
		/// <param name="collection">The collection to count items in.</param>
		/// <param name="find">The item to compare to.</param>
		/// <param name="equalityComparer">The comparer to use to determine if two items are equal. Only the Equals
		/// member function will be called.</param>
		/// <returns>The number of items in the collection that are equal to <paramref name="find"/>.</returns>
		/// <exception cref="ArgumentException"><paramref name="collection"/> or <paramref name="equalityComparer"/>
		/// is null.</exception>
		public static int CountEqual<T>(IEnumerable<T> collection, T find, IEqualityComparer<T> equalityComparer)
		{
			if (collection == null)
				throw new ArgumentException("collection");
			if (equalityComparer == null)
				throw new ArgumentNullException("equalityComparer");

			int count = 0;
			foreach (T item in collection)
			{
				if (equalityComparer.Equals(item, find))
					++count;
			}

			return count;
		}

		/// <summary>
		/// Creates an IEnumerator that enumerates a given item <paramref name="n"/> times.
		/// </summary>
		/// <example>
		/// The following creates a list consisting of 1000 copies of the double 1.0.
		/// <code>
		/// List&lt;double&gt; list = new List&lt;double&gt;(Algorithms.NCopiesOf(1000, 1.0));
		/// </code></example>
		/// <param name="n">The number of times to enumerate the item.</param>
		/// <param name="item">The item that should occur in the enumeration.</param>
		/// <returns>An IEnumerable&lt;T&gt; that yields <paramref name="n"/> copies
		/// of <paramref name="item"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">The argument <paramref name="n"/> is less than zero.</exception>
		public static IEnumerable<T> NCopiesOf<T>(int n, T item)
		{
			if (n < 0)
				throw new ArgumentOutOfRangeException("n", n, Strings.ArgMustNotBeNegative);

			while (n-- > 0)
			{
				yield return item;
			}
		}

		#endregion Miscellaneous operations on IEnumerable

		#region Miscellaneous operations on IList

		/// <summary>
		/// Replaces each item in a list with a given value. The list does not change in size.
		/// </summary>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to modify.</param>
		/// <param name="value">The value to fill with.</param>
		/// <exception cref="ArgumentException"><paramref name="list"/> is a read-only list.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		public static void Fill<T>(IList<T> list, T value)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			int count = list.Count;
			for (int i = 0; i < count; ++i)
			{
				list[i] = value;
			}
		}

		/// <summary>
		/// Replaces each item in a array with a given value. 
		/// </summary>
		/// <param name="array">The array to modify.</param>
		/// <param name="value">The value to fill with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
		public static void Fill<T>(T[] array, T value)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			for (int i = 0; i < array.Length; ++i)
			{
				array[i] = value;
			}
		}

		/// <summary>
		/// Replaces each item in a part of a list with a given value.
		/// </summary>
		/// <typeparam name="T">The type of items in the list.</typeparam>
		/// <param name="list">The list to modify.</param>
		/// <param name="start">The index at which to start filling. The first index in the list has index 0.</param>
		/// <param name="count">The number of items to fill.</param>
		/// <param name="value">The value to fill with.</param>
		/// <exception cref="ArgumentException"><paramref name="list"/> is a read-only list.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> or <paramref name="count"/> is negative, or 
		/// <paramref name="start"/> + <paramref name="count"/> is greater than <paramref name="list"/>.Count.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		public static void FillRange<T>(IList<T> list, int start, int count, T value)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			if (count == 0)
				return;
			if (start < 0 || start >= list.Count)
				throw new ArgumentOutOfRangeException("start");
			if (count < 0 || count > list.Count || start > list.Count - count)
				throw new ArgumentOutOfRangeException("count");

			for (int i = start; i < count + start; ++i)
			{
				list[i] = value;
			}
		}

		/// <summary>
		/// Replaces each item in a part of a array with a given value.
		/// </summary>
		/// <param name="array">The array to modify.</param>
		/// <param name="start">The index at which to start filling. The first index in the array has index 0.</param>
		/// <param name="count">The number of items to fill.</param>
		/// <param name="value">The value to fill with.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> or <paramref name="count"/> is negative, or 
		/// <paramref name="start"/> + <paramref name="count"/> is greater than <paramref name="array"/>.Length.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
		public static void FillRange<T>(T[] array, int start, int count, T value)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (count == 0)
				return;
			if (start < 0 || start >= array.Length)
				throw new ArgumentOutOfRangeException("start");
			if (count < 0 || count > array.Length || start > array.Length - count)
				throw new ArgumentOutOfRangeException("count");

			for (int i = start; i < count + start; ++i)
			{
				array[i] = value;
			}
		}

		/// <summary>
		/// Copies all of the items from the collection <paramref name="source"/> to the list <paramref name="dest"/>, starting
		/// at the index <paramref name="destIndex"/>. If necessary, the size of the destination list is expanded.
		/// </summary>
		/// <param name="source">The collection that provide the source items. </param>
		/// <param name="dest">The list to store the items into.</param>
		/// <param name="destIndex">The index to begin copying items to.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="destIndex"/> is negative or 
		/// greater than <paramref name="dest"/>.Count.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="dest"/> is null.</exception>
		public static void Copy<T>(IEnumerable<T> source, IList<T> dest, int destIndex)
		{
			Copy(source, dest, destIndex, int.MaxValue);
		}

		/// <summary>
		/// Copies all of the items from the collection <paramref name="source"/> to the array <paramref name="dest"/>, starting
		/// at the index <paramref name="destIndex"/>. 
		/// </summary>
		/// <param name="source">The collection that provide the source items. </param>
		/// <param name="dest">The array to store the items into.</param>
		/// <param name="destIndex">The index to begin copying items to.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="destIndex"/> is negative or 
		/// greater than <paramref name="dest"/>.Length.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="dest"/> is null.</exception>
		/// <exception cref="ArgumentException">The collection has more items than will fit into the array. In this case, the array
		/// has been filled with as many items as fit before the exception is thrown.</exception>
		public static void Copy<T>(IEnumerable<T> source, T[] dest, int destIndex)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (dest == null)
				throw new ArgumentNullException("dest");

			if (destIndex < 0 || destIndex > dest.Length)
				throw new ArgumentOutOfRangeException("destIndex");

			using (IEnumerator<T> sourceEnum = source.GetEnumerator())
			{
				// Overwrite items to the end of the destination array. If we hit the end, throw.
				while (sourceEnum.MoveNext())
				{
					if (destIndex >= dest.Length)
						throw new ArgumentException(Strings.ArrayTooSmall, "dest");
					dest[destIndex++] = sourceEnum.Current;
				}
			}
		}

		/// <summary>
		/// Copies at most <paramref name="count"/> items from the collection <paramref name="source"/> to the list <paramref name="dest"/>, starting
		/// at the index <paramref name="destIndex"/>. If necessary, the size of the destination list is expanded. The source collection must not be
		/// the destination list or part thereof.
		/// </summary>
		/// <param name="source">The collection that provide the source items. </param>
		/// <param name="dest">The list to store the items into.</param>
		/// <param name="destIndex">The index to begin copying items to.</param>
		/// <param name="count">The maximum number of items to copy.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="destIndex"/> is negative or 
		/// greater than <paramref name="dest"/>.Count</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="dest"/> is null.</exception>
		public static void Copy<T>(IEnumerable<T> source, IList<T> dest, int destIndex, int count)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (dest == null)
				throw new ArgumentNullException("dest");
			if (dest.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "dest");

			int destCount = dest.Count;

			if (destIndex < 0 || destIndex > destCount)
				throw new ArgumentOutOfRangeException("destIndex");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			using (IEnumerator<T> sourceEnum = source.GetEnumerator())
			{
				// First, overwrite items to the end of the destination list.
				while (destIndex < destCount && count > 0 && sourceEnum.MoveNext())
				{
					dest[destIndex++] = sourceEnum.Current;
					--count;
				}

				// Second, insert items until done.
				while (count > 0 && sourceEnum.MoveNext())
				{
					dest.Insert(destCount++, sourceEnum.Current);
					--count;
				}
			}
		}

		/// <summary>
		/// Copies at most <paramref name="count"/> items from the collection <paramref name="source"/> to the array <paramref name="dest"/>, starting
		/// at the index <paramref name="destIndex"/>. The source collection must not be
		/// the destination array or part thereof.
		/// </summary>
		/// <param name="source">The collection that provide the source items. </param>
		/// <param name="dest">The array to store the items into.</param>
		/// <param name="destIndex">The index to begin copying items to.</param>
		/// <param name="count">The maximum number of items to copy. The array must be large enought to fit this number of items.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="destIndex"/> is negative or 
		/// greater than <paramref name="dest"/>.Length.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative or <paramref name="destIndex"/> + <paramref name="count"/>
		/// is greater than <paramref name="dest"/>.Length.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="dest"/> is null.</exception>
		public static void Copy<T>(IEnumerable<T> source, T[] dest, int destIndex, int count)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (dest == null)
				throw new ArgumentNullException("dest");

			int destCount = dest.Length;

			if (destIndex < 0 || destIndex > destCount)
				throw new ArgumentOutOfRangeException("destIndex");
			if (count < 0 || destIndex + count > destCount)
				throw new ArgumentOutOfRangeException("count");

			using (IEnumerator<T> sourceEnum = source.GetEnumerator())
			{
				// First, overwrite items to the end of the destination array.
				while (destIndex < destCount && count > 0 && sourceEnum.MoveNext())
				{
					dest[destIndex++] = sourceEnum.Current;
					--count;
				}
			}
		}

		/// <summary>
		/// Copies <paramref name="count"/> items from the list <paramref name="source"/>, starting at the index <paramref name="sourceIndex"/>, 
		/// to the list <paramref name="dest"/>, starting at the index <paramref name="destIndex"/>. If necessary, the size of the destination list is expanded.
		/// The source and destination lists may be the same.
		/// </summary>
		/// <param name="source">The collection that provide the source items. </param>
		/// <param name="sourceIndex">The index within <paramref name="source"/>to begin copying items from.</param>
		/// <param name="dest">The list to store the items into.</param>
		/// <param name="destIndex">The index within <paramref name="dest"/>to begin copying items to.</param>
		/// <param name="count">The maximum number of items to copy.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceIndex"/> is negative or 
		/// greater than <paramref name="source"/>.Count</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="destIndex"/> is negative or 
		/// greater than <paramref name="dest"/>.Count</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative or too large.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="dest"/> is null.</exception>
		public static void Copy<T>(IList<T> source, int sourceIndex, IList<T> dest, int destIndex, int count)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (dest == null)
				throw new ArgumentNullException("dest");
			if (dest.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "dest");

			int sourceCount = source.Count;
			int destCount = dest.Count;

			if (sourceIndex < 0 || sourceIndex >= sourceCount)
				throw new ArgumentOutOfRangeException("sourceIndex");
			if (destIndex < 0 || destIndex > destCount)
				throw new ArgumentOutOfRangeException("destIndex");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count");
			if (count > sourceCount - sourceIndex)
				count = sourceCount - sourceIndex;

			if (source == dest && sourceIndex > destIndex)
			{
				while (count > 0)
				{
					dest[destIndex++] = source[sourceIndex++];
					--count;
				}
			}
			else
			{
				int si, di;

				// First, insert any items needed at the end
				if (destIndex + count > destCount)
				{
					int numberToInsert = destIndex + count - destCount;
					si = sourceIndex + (count - numberToInsert);
					di = destCount;
					count -= numberToInsert;
					while (numberToInsert > 0)
					{
						dest.Insert(di++, source[si++]);
						--numberToInsert;
					}
				}

				// Do the copy, from end to beginning in case of overlap.
				si = sourceIndex + count - 1;
				di = destIndex + count - 1;
				while (count > 0)
				{
					dest[di--] = source[si--];
					--count;
				}
			}
		}

		/// <summary>
		/// Copies <paramref name="count"/> items from the list or array <paramref name="source"/>, starting at the index <paramref name="sourceIndex"/>, 
		/// to the array <paramref name="dest"/>, starting at the index <paramref name="destIndex"/>. 
		/// The source may be the same as the destination array.
		/// </summary>
		/// <param name="source">The list or array that provide the source items. </param>
		/// <param name="sourceIndex">The index within <paramref name="source"/>to begin copying items from.</param>
		/// <param name="dest">The array to store the items into.</param>
		/// <param name="destIndex">The index within <paramref name="dest"/>to begin copying items to.</param>
		/// <param name="count">The maximum number of items to copy. The destination array must be large enough to hold this many items.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceIndex"/> is negative or 
		/// greater than <paramref name="source"/>.Count</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="destIndex"/> is negative or 
		/// greater than <paramref name="dest"/>.Length</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative or too large.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="dest"/> is null.</exception>
		public static void Copy<T>(IList<T> source, int sourceIndex, T[] dest, int destIndex, int count)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (dest == null)
				throw new ArgumentNullException("dest");

			int sourceCount = source.Count;
			int destCount = dest.Length;

			if (sourceIndex < 0 || sourceIndex >= sourceCount)
				throw new ArgumentOutOfRangeException("sourceIndex");
			if (destIndex < 0 || destIndex > destCount)
				throw new ArgumentOutOfRangeException("destIndex");
			if (count < 0 || destIndex + count > destCount)
				throw new ArgumentOutOfRangeException("count");

			if (count > sourceCount - sourceIndex)
				count = sourceCount - sourceIndex;

			if (source is T[])
			{
				// Array.Copy is probably faster, and also handles any overlapping issues.
				Array.Copy((T[]) source, sourceIndex, dest, destIndex, count);
			}
			else
			{
				int si = sourceIndex;
				int di = destIndex;
				while (count > 0)
				{
					dest[di++] = source[si++];
					--count;
				}
			}
		}

		/// <summary>
		/// Reverses a list and returns the reversed list, without changing the source list.
		/// </summary>
		/// <param name="source">The list to reverse.</param>
		/// <returns>A collection that contains the items from <paramref name="source"/> in reverse order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
		public static IEnumerable<T> Reverse<T>(IList<T> source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			for (int i = source.Count - 1; i >= 0; --i)
				yield return source[i];
		}

		/// <summary>
		/// Reverses a list or array in place.
		/// </summary>
		/// <remarks>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</remarks>
		/// <param name="list">The list or array to reverse.</param>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="list"/> is read only.</exception>
		public static void ReverseInPlace<T>(IList<T> list)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (list is T[])
				list = new ArrayWrapper<T>((T[]) list);
			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			int i, j;
			i = 0;
			j = list.Count - 1;
			while (i < j)
			{
				T temp = list[i];
				list[i] = list[j];
				list[j] = temp;
				i++;
				j--;
			}
		}

		/// <summary>
		/// Rotates a list and returns the rotated list, without changing the source list.
		/// </summary>
		/// <param name="source">The list to rotate.</param>
		/// <param name="amountToRotate">The number of elements to rotate. This value can be positive or negative. 
		/// For example, rotating by positive 3 means that source[3] is the first item in the returned collection.
		/// Rotating by negative 3 means that source[source.Count - 3] is the first item in the returned collection.</param>
		/// <returns>A collection that contains the items from <paramref name="source"/> in rotated order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
		public static IEnumerable<T> Rotate<T>(IList<T> source, int amountToRotate)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			int count = source.Count;
			if (count != 0)
			{
				amountToRotate = amountToRotate%count;
				if (amountToRotate < 0)
					amountToRotate += count;

				// Do it in two parts.
				for (int i = amountToRotate; i < count; ++i)
					yield return source[i];
				for (int i = 0; i < amountToRotate; ++i)
					yield return source[i];
			}
		}

		/// <summary>
		/// Rotates a list or array in place.
		/// </summary>
		/// <remarks>Although arrays cast to IList&lt;T&gt; are normally read-only, this method
		/// will work correctly and modify an array passed as <paramref name="list"/>.</remarks>
		/// <param name="list">The list or array to rotate.</param>
		/// <param name="amountToRotate">The number of elements to rotate. This value can be positive or negative. 
		/// For example, rotating by positive 3 means that list[3] is the first item in the resulting list.
		/// Rotating by negative 3 means that list[list.Count - 3] is the first item in the resulting list.</param>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		public static void RotateInPlace<T>(IList<T> list, int amountToRotate)
		{
			if (list == null)
				throw new ArgumentNullException("list");
			if (list is T[])
				list = new ArrayWrapper<T>((T[]) list);
			if (list.IsReadOnly)
				throw new ArgumentException(Strings.ListIsReadOnly, "list");

			int count = list.Count;
			if (count != 0)
			{
				amountToRotate = amountToRotate%count;
				if (amountToRotate < 0)
					amountToRotate += count;

				int itemsLeft = count;
				int indexStart = 0;
				while (itemsLeft > 0)
				{
					// Rotate an orbit of items through the list. If itemsLeft is relatively prime
					// to count, this will rotate everything. If not, we need to do this several times until
					// all items have been moved.
					int index = indexStart;
					T itemStart = list[indexStart];
					for (;;)
					{
						--itemsLeft;
						int nextIndex = index + amountToRotate;
						if (nextIndex >= count)
							nextIndex -= count;
						if (nextIndex == indexStart)
						{
							list[index] = itemStart;
							break;
						}
						else
						{
							list[index] = list[nextIndex];
							index = nextIndex;
						}
					}

					// Move to the next orbit.
					++indexStart;
				}
			}
		}

		#endregion Miscellaneous operations on IList
	}
}