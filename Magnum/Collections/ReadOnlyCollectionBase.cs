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
	/// ReadOnlyCollectionBase is a base class that can be used to more easily implement the
	/// generic ICollection&lt;T&gt; and non-generic ICollection interfaces for a read-only collection:
	/// a collection that does not allow adding or removing elements.
	/// </summary>
	/// <remarks>
	/// <para>To use ReadOnlyCollectionBase as a base class, the derived class must override
	/// the Count and GetEnumerator methods. </para>
	/// <para>ICollection&lt;T&gt;.Contains need not be implemented by the
	/// derived class, but it should be strongly considered, because the ReadOnlyCollectionBase implementation
	/// may not be very efficient.</para>
	/// </remarks>
	/// <typeparam name="T">The item type of the collection.</typeparam>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplayString()}")]
	public abstract class ReadOnlyCollectionBase<T> : ICollection<T>,
		ICollection
	{
		/// <summary>
		/// Display the contents of the collection in the debugger. This is intentionally private, it is called
		/// only from the debugger due to the presence of the DebuggerDisplay attribute. It is similar
		/// format to ToString(), but is limited to 250-300 characters or so, so as not to overload the debugger.
		/// </summary>
		/// <returns>The string representation of the items in the collection, similar in format to ToString().</returns>
		internal string DebuggerDisplayString()
		{
			const int MAXLENGTH = 250;

			StringBuilder builder = new StringBuilder();

			builder.Append('{');

			// Call ToString on each item and put it in.
			bool firstItem = true;
			foreach (T item in this)
			{
				if (builder.Length >= MAXLENGTH)
				{
					builder.Append(",...");
					break;
				}

				if (!firstItem)
					builder.Append(',');

				if (item == null)
					builder.Append("null");
				else
					builder.Append(item.ToString());

				firstItem = false;
			}

			builder.Append('}');
			return builder.ToString();
		}

		/// <summary>
		/// Throws an NotSupportedException stating that this collection cannot be modified.
		/// </summary>
		private void MethodModifiesCollection()
		{
			throw new NotSupportedException(string.Format(Strings.CannotModifyCollection, Util.SimpleClassName(this.GetType())));
		}

		#region Delegate operations

		/// <summary>
		/// Shows the string representation of the collection. The string representation contains
		/// a list of the items in the collection.
		/// </summary>
		/// <returns>The string representation of the collection.</returns>
		public override string ToString()
		{
			return Algorithms.ToString(this);
		}

		/// <summary>
		/// Determines if the collection contains any item that satisfies the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <returns>True if the collection contains one or more items that satisfy the condition
		/// defined by <paramref name="predicate"/>. False if the collection does not contain
		/// an item that satisfies <paramref name="predicate"/>.</returns>
		public virtual bool Exists(Predicate<T> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			return Algorithms.Exists(this, predicate);
		}

		/// <summary>
		/// Determines if all of the items in the collection satisfy the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <returns>True if all of the items in the collection satisfy the condition
		/// defined by <paramref name="predicate"/>, or if the collection is empty. False if one or more items
		/// in the collection do not satisfy <paramref name="predicate"/>.</returns>
		public virtual bool TrueForAll(Predicate<T> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			return Algorithms.TrueForAll(this, predicate);
		}

		/// <summary>
		/// Counts the number of items in the collection that satisfy the condition
		/// defined by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <returns>The number of items in the collection that satisfy <paramref name="predicate"/>.</returns>
		public virtual int CountWhere(Predicate<T> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			return Algorithms.CountWhere(this, predicate);
		}

		/// <summary>
		/// Enumerates the items in the collection that satisfy the condition defined
		/// by <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">A delegate that defines the condition to check for.</param>
		/// <returns>An IEnumerable&lt;T&gt; that enumerates the items that satisfy the condition.</returns>
		public IEnumerable<T> FindAll(Predicate<T> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			return Algorithms.FindWhere(this, predicate);
		}

		/// <summary>
		/// Performs the specified action on each item in this collection.
		/// </summary>
		/// <param name="action">An Action delegate which is invoked for each item in this collection.</param>
		public virtual void ForEach(Action<T> action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			Algorithms.ForEach(this, action);
		}

		/// <summary>
		/// Convert this collection of items by applying a delegate to each item in the collection. The resulting enumeration
		/// contains the result of applying <paramref name="converter"/> to each item in this collection, in
		/// order.
		/// </summary>
		/// <typeparam name="TOutput">The type each item is being converted to.</typeparam>
		/// <param name="converter">A delegate to the method to call, passing each item in this collection.</param>
		/// <returns>An IEnumerable&lt;TOutput^gt; that enumerates the resulting collection from applying <paramref name="converter"/> to each item in this collection in
		/// order.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="converter"/> is null.</exception>
		public virtual IEnumerable<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
		{
			if (converter == null)
				throw new ArgumentNullException("converter");

			return Algorithms.Convert(this, converter);
		}

		#endregion Delegate operations

		#region ICollection<T> Members

		/// <summary>
		/// This method throws an NotSupportedException
		/// stating the collection is read-only.
		/// </summary>
		/// <param name="item">Item to be added to the collection.</param>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		void ICollection<T>.Add(T item)
		{
			MethodModifiesCollection();
		}

		/// <summary>
		/// This method throws an NotSupportedException
		/// stating the collection is read-only.
		/// </summary>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		void ICollection<T>.Clear()
		{
			MethodModifiesCollection();
		}

		/// <summary>
		/// This method throws an NotSupportedException
		/// stating the collection is read-only.
		/// </summary>
		/// <param name="item">Item to be removed from the collection.</param>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		bool ICollection<T>.Remove(T item)
		{
			MethodModifiesCollection();
			return false;
		}

		/// <summary>
		/// Determines if the collection contains a particular item. This default implementation
		/// iterates all of the items in the collection via GetEnumerator, testing each item
		/// against <paramref name="item"/> using IComparable&lt;T&gt;.Equals or
		/// Object.Equals.
		/// </summary>
		/// <remarks>You should strongly consider overriding this method to provide
		/// a more efficient implementation.</remarks>
		/// <param name="item">The item to check for in the collection.</param>
		/// <returns>True if the collection contains <paramref name="item"/>, false otherwise.</returns>
		public virtual bool Contains(T item)
		{
			IEqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
			foreach (T i in this)
			{
				if (equalityComparer.Equals(i, item))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Copies all the items in the collection into an array. Implemented by
		/// using the enumerator returned from GetEnumerator to get all the items
		/// and copy them to the provided array.
		/// </summary>
		/// <param name="array">Array to copy to.</param>
		/// <param name="arrayIndex">Starting index in <paramref name="array"/> to copy to.</param>
		public virtual void CopyTo(T[] array, int arrayIndex)
		{
			int count = this.Count;

			if (count == 0)
				return;

			if (array == null)
				throw new ArgumentNullException("array");
			if (count < 0)
				throw new IndexOutOfRangeException(Strings.ArgMustNotBeNegative);
			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, Strings.ArgMustNotBeNegative);
			if (arrayIndex >= array.Length || count > array.Length - arrayIndex)
				throw new ArgumentException("arrayIndex", Strings.ArrayTooSmall);

			int index = arrayIndex, i = 0;
			//TODO: Look into this
			foreach (T item in (ICollection<T>) this)
			{
				if (i >= count)
					break;

				array[index] = item;
				++index;
				++i;
			}
		}

		/// <summary>
		/// Must be overridden to provide the number of items in the collection.
		/// </summary>
		/// <value>The number of items in the collection.</value>
		public abstract int Count { get; }

		/// <summary>
		/// Indicates whether the collection is read-only. Returns the value
		/// of readOnly that was provided to the constructor.
		/// </summary>
		/// <value>Always true.</value>
		bool ICollection<T>.IsReadOnly
		{
			get { return true; }
		}

		/// <summary>
		/// Creates an array of the correct size, and copies all the items in the 
		/// collection into the array, by calling CopyTo.
		/// </summary>
		/// <returns>An array containing all the elements in the collection, in order.</returns>
		public virtual T[] ToArray()
		{
			int count = this.Count;

			T[] array = new T[count];
			CopyTo(array, 0);
			return array;
		}

		#endregion

		#region IEnumerable<T> Members

		/// <summary>
		/// Must be overridden to enumerate all the members of the collection.
		/// </summary>
		/// <returns>A generic IEnumerator&lt;T&gt; that can be used
		/// to enumerate all the items in the collection.</returns>
		public abstract IEnumerator<T> GetEnumerator();

		#endregion

		#region ICollection Members

		/// <summary>
		/// Copies all the items in the collection into an array. Implemented by
		/// using the enumerator returned from GetEnumerator to get all the items
		/// and copy them to the provided array.
		/// </summary>
		/// <param name="array">Array to copy to.</param>
		/// <param name="index">Starting index in <paramref name="array"/> to copy to.</param>
		void ICollection.CopyTo(Array array, int index)
		{
			int count = this.Count;

			if (count == 0)
				return;

			if (array == null)
				throw new ArgumentNullException("array");
			if (count < 0)
				throw new IndexOutOfRangeException(Strings.ArgMustNotBeNegative);
			if (index < 0)
				throw new ArgumentOutOfRangeException("index", index, Strings.ArgMustNotBeNegative);
			if (index >= array.Length || count > array.Length - index)
				throw new ArgumentException("index", Strings.ArrayTooSmall);

			int i = 0;
			//TODO: Look into this
			foreach (object o in (ICollection) this)
			{
				if (i >= count)
					break;

				array.SetValue(o, index);
				++index;
				++i;
			}
		}

		/// <summary>
		/// Indicates whether the collection is synchronized.
		/// </summary>
		/// <value>Always returns false, indicating that the collection is not synchronized.</value>
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		/// <summary>
		/// Indicates the synchronization object for this collection.
		/// </summary>
		/// <value>Always returns this.</value>
		object ICollection.SyncRoot
		{
			get { return this; }
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Provides an IEnumerator that can be used to iterate all the members of the
		/// collection. This implementation uses the IEnumerator&lt;T&gt; that was overridden
		/// by the derived classes to enumerate the members of the collection.
		/// </summary>
		/// <returns>An IEnumerator that can be used to iterate the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (T item in this)
			{
				yield return item;
			}
		}

		#endregion
	}
}