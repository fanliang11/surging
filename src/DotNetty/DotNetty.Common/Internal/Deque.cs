// Largely based from https://github.com/StephenCleary/Deque/blob/master/src/Nito.Collections.Deque/Deque.cs
// https://github.com/cuteant/CuteAnt.Core/blob/dev/src/CuteAnt.Core.Abstractions/Collections/Deque.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotNetty.Common.Internal
{
    /// <summary>A double-ended queue (deque), which provides O(1) indexed access, O(1) removals from the front and back, 
    /// amortized O(1) insertions to the front and back, and O(N) insertions and removals anywhere else 
    /// (with the operations getting slower as the index approaches the middle).</summary>
    /// <typeparam name="T">The type of elements contained in the deque.</typeparam>
    [DebuggerDisplay("Count = {Count}, Capacity = {Capacity}")]
    [DebuggerTypeProxy(typeof(Deque<>.DebugView))]
    public class Deque<T> : IList<T>, IReadOnlyList<T>, IList
    {
        #region @@ Fields @@

        /// <summary>The default capacity.</summary>
        private const int DefaultCapacity = 8;

        /// <summary>The circular _buffer that holds the view.</summary>
        private T[] _buffer;

        /// <summary>The offset into <see cref="_buffer"/> where the view begins.</summary>
        private int _offset;

        private readonly IEqualityComparer<T> _comparer;

        private readonly bool _useReversingEnumerator;

        private int _count = 0;

        #endregion

        #region -- Constructors --

        /// <summary>Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class.</summary>
        public Deque() : this(DefaultCapacity, false, null) { }

        /// <summary>Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the specified capacity.</summary>
        /// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
        public Deque(int capacity) : this(capacity, false, null) { }

        /// <summary>Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the specified capacity.</summary>
        /// <param name="useReversingEnumerator"></param>
        public Deque(bool useReversingEnumerator) : this(DefaultCapacity, useReversingEnumerator, null) { }

        /// <summary>Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the specified capacity.</summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, 
        /// or null to use the default <see cref="EqualityComparer{T}"/> for the type of the element.</param>
        public Deque(IEqualityComparer<T> comparer) : this(DefaultCapacity, false, comparer) { }

        /// <summary>Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the specified capacity.</summary>
        /// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
        /// <param name="useReversingEnumerator"></param>
        public Deque(int capacity, bool useReversingEnumerator) : this(capacity, useReversingEnumerator, null) { }

        /// <summary>Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the specified capacity.</summary>
        /// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, 
        /// or null to use the default <see cref="EqualityComparer{T}"/> for the type of the element.</param>
        public Deque(int capacity, IEqualityComparer<T> comparer) : this(capacity, false, comparer) { }

        /// <summary>Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the specified capacity.</summary>
        /// <param name="useReversingEnumerator"></param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, 
        /// or null to use the default <see cref="EqualityComparer{T}"/> for the type of the element.</param>
        public Deque(bool useReversingEnumerator, IEqualityComparer<T> comparer) : this(DefaultCapacity, useReversingEnumerator, comparer) { }

        /// <summary>Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the specified capacity.</summary>
        /// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
        /// <param name="useReversingEnumerator"></param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, 
        /// or null to use the default <see cref="EqualityComparer{T}"/> for the type of the element.</param>
        public Deque(int capacity, bool useReversingEnumerator, IEqualityComparer<T> comparer)
        {
            if ((uint)capacity > SharedConstants.TooBigOrNegative)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.Capacity_May_Not_Be_Negative);
            }
            _buffer = new T[capacity];
            _useReversingEnumerator = useReversingEnumerator;
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        /// <summary>Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the elements from the specified collection.</summary>
        /// <param name="collection">The collection. May not be <c>null</c>.</param>
        public Deque(IEnumerable<T> collection) : this(collection, null) { }

        /// <summary>Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the elements from the specified collection.</summary>
        /// <param name="collection">The collection. May not be <c>null</c>.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, 
        /// or null to use the default <see cref="EqualityComparer{T}"/> for the type of the element.</param>
        public Deque(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            if (collection is null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);

            var source = CollectionHelpers.ReifyCollection(collection);
            var count = source.Count;
            if ((uint)count > 0u)
            {
                _buffer = new T[count];
                DoInsertRange(0, source);
            }
            else
            {
                _buffer = new T[DefaultCapacity];
            }
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _useReversingEnumerator = false;
        }

        #endregion

        #region -- GenericListImplementations --

        /// <summary>Gets a value indicating whether this list is read-only. This implementation always returns <c>false</c>.</summary>
        /// <returns>true if this list is read-only; otherwise, false.</returns>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>Gets or sets the item at the specified index.</summary>
        /// <param name="index">The index of the item to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in this list.</exception>
        /// <exception cref="T:System.NotSupportedException">This property is set and the list is read-only.</exception>
        public T this[int index]
        {
            get
            {
                if (/*index < 0 || */(uint)index >= (uint)_count) { CheckExistingIndexArgument(_count, index); }
                return DoGetItem(index);
            }

            set
            {
                if (/*index < 0 || */(uint)index >= (uint)_count) { CheckExistingIndexArgument(_count, index); }
                DoSetItem(index, value);
            }
        }

        /// <summary>Inserts an item to this list at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into this list.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in this list.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// This list is read-only.
        /// </exception>
        public void Insert(int index, T item)
        {
            if (/*index < 0 || */(uint)index > (uint)_count) { CheckNewIndexArgument(_count, index); }
            DoInsert(index, item);
        }

        /// <summary>Removes the item at the specified index.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in this list.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// This list is read-only.
        /// </exception>
        public void RemoveAt(int index)
        {
            if (/*index < 0 || */(uint)index >= (uint)_count) { CheckExistingIndexArgument(_count, index); }
            DoRemoveAt(index);
        }

        /// <summary>Determines the index of a specific item in this list.</summary>
        /// <param name="item">The object to locate in this list.</param>
        /// <returns>The index of <paramref name="item"/> if found in this list; otherwise, -1.</returns>
        public int IndexOf(T item)
        {
            var idx = 0;
            while (idx < _count)
            {
                if (_comparer.Equals(item, DoGetItem(idx))) { return idx; }
                idx++;
            }

            return -1;
        }

        /// <summary>Adds an item to the end of this list.</summary>
        /// <param name="item">The object to add to this list.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// This list is read-only.
        /// </exception>
        void ICollection<T>.Add(T item)
        {
            DoInsert(_count, item);
        }

        /// <summary>Determines whether this list contains a specific value.</summary>
        /// <param name="item">The object to locate in this list.</param>
        /// <returns>true if <paramref name="item"/> is found in this list; otherwise, false.</returns>
        public bool Contains(T item)
        {
            if (IsEmpty) { return false; }

            var idx = 0;
            while (idx < _count)
            {
                if (_comparer.Equals(item, DoGetItem(idx))) { return true; }
                idx++;
            }
            return false;
        }

        /// <summary>Copies the elements of this list to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from this slice. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
        /// -or-
        /// The number of elements in the source <see cref="ICollection{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        public void /*ICollection<T>.*/CopyTo(T[] array, int arrayIndex)
        {
            if (array is null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            if ((uint)arrayIndex > SharedConstants.TooBigOrNegative) { CheckRangeArguments_Offset(arrayIndex); }
            uint uCount = (uint)_count;
            if (uCount > SharedConstants.TooBigOrNegative) { CheckRangeArguments_Count(_count); }
            if (uCount > (uint)(array.Length - arrayIndex)) { CheckRangeArguments(array.Length, arrayIndex, _count); }

            CopyToArray(array, arrayIndex);
        }

        /// <summary>Copies the deque elemens into an array. The resulting array always has all the deque elements contiguously.</summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The optional index in the destination array at which to begin writing.</param>
        private void CopyToArray(Array array, int arrayIndex = 0)
        {
            if (array is null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);

            if (IsSplit)
            {
                // The existing buffer is split, so we have to copy it in parts
                int length = Capacity - _offset;
                Array.Copy(_buffer, _offset, array, arrayIndex, length);
                Array.Copy(_buffer, 0, array, arrayIndex + length, _count - length);
            }
            else
            {
                // The existing buffer is whole
                Array.Copy(_buffer, _offset, array, arrayIndex, _count);
            }
        }

        /// <summary>Removes the first occurrence of a specific object from this list.</summary>
        /// <param name="item">The object to remove from this list.</param>
        /// <returns>true if <paramref name="item"/> was successfully removed from this list; otherwise, false. This method also returns false if <paramref name="item"/> is not found in this list.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// This list is read-only.
        /// </exception>
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if ((uint)_count > (uint)index)
            {
                DoRemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (_useReversingEnumerator)
            {
                return new ReversingEnumerator(this);
            }
            else
            {
                return new Enumerator(this);
            }
        }

        /// <internalonly/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (_useReversingEnumerator)
            {
                return new ReversingEnumerator(this);
            }
            else
            {
                return new Enumerator(this);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_useReversingEnumerator)
            {
                return new ReversingEnumerator(this);
            }
            else
            {
                return new Enumerator(this);
            }
        }

        #endregion

        #region -- ObjectListImplementations --

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private static bool IsT(object value)
        {
            if (value is T) { return true; }
            if (value is object) { return false; }
            return default(T) is null;
        }

        int IList.Add(object value)
        {
            if (value is null && default(T) != null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value, ExceptionResource.Value_Cannot_Be_Null);
            if (!IsT(value)) ThrowHelper.ThrowArgumentException(ExceptionResource.Value_Is_Of_Incorrect_Type, ExceptionArgument.value);

            AddLast​((T)value);
            return _count - 1;
        }

        bool IList.Contains(object value)
        {
            return IsT(value) && ((ICollection<T>)this).Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return IsT(value) ? IndexOf((T)value) : -1;
        }

        void IList.Insert(int index, object value)
        {
            if (value is null && default(T) != null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value, ExceptionResource.Value_Cannot_Be_Null); }
            if (!IsT(value)) { ThrowHelper.ThrowArgumentException(ExceptionResource.Value_Is_Of_Incorrect_Type, ExceptionArgument.value); }
            Insert(index, (T)value);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            if (IsT(value)) { _ = Remove((T)value); }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set
            {
                if (value is null && default(T) != null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value, ExceptionResource.Value_Cannot_Be_Null);
                if (!IsT(value)) ThrowHelper.ThrowArgumentException(ExceptionResource.Value_Is_Of_Incorrect_Type, ExceptionArgument.value);
                this[index] = (T)value;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array is null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array, ExceptionResource.Dest_Array_Cannot_Be_Null);
            if ((uint)index > SharedConstants.TooBigOrNegative) { CheckRangeArguments_Offset(index); }
            uint uCount = (uint)_count;
            if (uCount > SharedConstants.TooBigOrNegative) { CheckRangeArguments_Count(_count); }
            if (uCount > (uint)(array.Length - index)) { CheckRangeArguments(array.Length, index, _count); }

            try
            {
                CopyToArray(array, index);
            }
            catch (ArrayTypeMismatchException ex)
            {
                throw new ArgumentException("Destination array is of incorrect type.", nameof(array), ex);
            }
            catch (RankException ex)
            {
                throw new ArgumentException("Destination array must be single dimensional.", nameof(array), ex);
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }

        #endregion

        #region ** ThrowHelper **

        /// <summary>Checks the <paramref name="index"/> argument to see if it refers to a valid insertion point in a source of a given length.</summary>
        /// <param name="sourceLength">The length of the source. This parameter is not checked for validity.</param>
        /// <param name="index">The index into the source.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index to an insertion point for the source.</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckNewIndexArgument(int sourceLength, int index)
        {
            throw GetArgumentOutOfRangeException();
            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(nameof(index), "Invalid new index " + index + " for source length " + sourceLength);
            }
        }

        /// <summary>Checks the <paramref name="index"/> argument to see if it refers to an existing element in a source of a given length.</summary>
        /// <param name="sourceLength">The length of the source. This parameter is not checked for validity.</param>
        /// <param name="index">The index into the source.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index to an existing element for the source.</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckExistingIndexArgument(int sourceLength, int index)
        {
            throw GetArgumentOutOfRangeException();
            ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException(nameof(index), "Invalid existing index " + index + " for source length " + sourceLength);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckRangeArguments(int sourceLength, int offset, int count)
        {
            throw GetArgumentException();
            ArgumentException GetArgumentException()
            {
                return new ArgumentException("Invalid offset (" + offset + ") or count + (" + count + ") for source length " + sourceLength);
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckRangeArguments_Count(int count)
        {
            throw GetArgumentOutOfRangeException1();
            ArgumentOutOfRangeException GetArgumentOutOfRangeException1()
            {
                return new ArgumentOutOfRangeException(nameof(count), "Invalid count " + count);
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckRangeArguments_Offset(int offset)
        {
            throw GetArgumentOutOfRangeException0();
            ArgumentOutOfRangeException GetArgumentOutOfRangeException0()
            {
                return new ArgumentOutOfRangeException(nameof(offset), "Invalid offset " + offset);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index()
        {
            throw ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.startIndex,
                                                    ExceptionResource.ArgumentOutOfRange_Index);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count()
        {
            throw ThrowHelper.GetArgumentOutOfRangeException(ExceptionArgument.count,
                                                    ExceptionResource.ArgumentOutOfRange_Count);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentNullException_Action()
        {
            throw ThrowHelper.GetArgumentNullException(ExceptionArgument.action);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentNullException_Match()
        {
            throw ThrowHelper.GetArgumentNullException(ExceptionArgument.match);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentNullException_Results()
        {
            throw ThrowHelper.GetArgumentNullException(ExceptionArgument.results);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationException()
        {
            throw GetInvalidOperationException();

            static InvalidOperationException GetInvalidOperationException()
            {
                return new InvalidOperationException("The deque is empty.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentOutOfRangeException_Capacity()
        {
            throw GetArgumentOutOfRangeException();

            static ArgumentOutOfRangeException GetArgumentOutOfRangeException()
            {
                return new ArgumentOutOfRangeException("value", "Capacity cannot be set to a value less than Count");
            }
        }

        #endregion

        #region @@ Properties @@

        public bool UseReversingEnumerator => _useReversingEnumerator;

        /// <summary>Gets a value indicating whether this instance is empty.</summary>
        public bool IsEmpty
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get { return 0u >= (uint)_count; }
        }

        public bool NonEmpty
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get { return (uint)_count > 0u; }
        }

        /// <summary>Gets a value indicating whether this instance is at full capacity.</summary>
        public bool IsFull
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get { return (uint)_count >= (uint)Capacity; }
        }

        /// <summary>Gets a value indicating whether the buffer is "split" (meaning the beginning of the view is at a later index in <see cref="_buffer"/> than the end).</summary>
        public bool IsSplit
        {
            // Overflow-safe version of "(offset + Count) > Capacity"
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get { return (uint)(_offset + _count) > (uint)Capacity; }
        }

        /// <summary>Gets or sets the capacity for this deque. This value must always be greater than zero, and this property cannot be set to a value less than <see cref="Count"/>.</summary>
        /// <exception cref="InvalidOperationException"><c>Capacity</c> cannot be set to a value less than <see cref="Count"/>.</exception>
        public int Capacity
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get { return _buffer.Length; }
            set
            {
                if (value < _count) { ThrowArgumentOutOfRangeException_Capacity(); }

                if (value == _buffer.Length) { return; }

                // Create the new _buffer and copy our existing range.
                T[] newBuffer = new T[value];
                CopyToArray(newBuffer);

                // Set up to use the new _buffer.
                _buffer = newBuffer;
                _offset = 0;
            }
        }

        /// <summary>Gets the number of elements contained in this deque.</summary>
        /// <returns>The number of elements contained in this deque.</returns>
        public int Count => _count;

        #endregion

        #region ** Private methods **

        /// <summary>Applies the offset to <paramref name="index"/>, resulting in a buffer index.</summary>
        /// <param name="index">The deque index.</param>
        /// <returns>The buffer index.</returns>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private int DequeIndexToBufferIndex(int index)
        {
            //return (index + _offset) % Capacity;
            var tmp = _offset + index;
            var buffer = _buffer;
            if (tmp < buffer.Length)
            {
                return tmp;
            }
            else
            {
                return tmp % buffer.Length;
            }
        }

        /// <summary>Gets an element at the specified view index.</summary>
        /// <param name="index">The zero-based view index of the element to get. This index is guaranteed to be valid.</param>
        /// <returns>The element at the specified index.</returns>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private T DoGetItem(int index)
        {
            //return _buffer[DequeIndexToBufferIndex(index)];
            var offset = _offset + index;
            var buffer = _buffer;
            if ((uint)offset < (uint)buffer.Length)
            {
                return buffer[offset];
            }
            else
            {
                return buffer[offset % buffer.Length];
            }
        }

        /// <summary>Sets an element at the specified view index.</summary>
        /// <param name="index">The zero-based view index of the element to get. This index is guaranteed to be valid.</param>
        /// <param name="item">The element to store in the list.</param>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private void DoSetItem(int index, T item)
        {
            //_buffer[DequeIndexToBufferIndex(index)] = item;
            var offset = _offset + index;
            var buffer = _buffer;
            if ((uint)offset < (uint)buffer.Length)
            {
                buffer[offset] = item;
            }
            else
            {
                buffer[offset % buffer.Length] = item;
            }
        }

        /// <summary>Inserts an element at the specified view index.</summary>
        /// <param name="index">The zero-based view index at which the element should be inserted. This index is guaranteed to be valid.</param>
        /// <param name="item">The element to store in the list.</param>
        private void DoInsert(int index, T item)
        {
            EnsureCapacityForOneElement();

            uint uIndex = (uint)index;
            if (0u >= uIndex)
            {
                DoAddToFront(item);
            }
            else if (uIndex < (uint)_count)
            {
                DoInsertRange(index, new[] { item });
            }
            else
            {
                DoAddToBack(item);
            }
        }

        /// <summary>Removes an element at the specified view index.</summary>
        /// <param name="index">The zero-based view index of the element to remove. This index is guaranteed to be valid.</param>
        private void DoRemoveAt(int index)
        {
            uint uIndex = (uint)index;
            if (0u >= uIndex)
            {
                _ = DoRemoveFromFront();
            }
            else if (uIndex < (uint)(_count - 1))
            {
                DoRemoveRange(index, 1);
            }
            else
            {
                _ = DoRemoveFromBack();
            }
        }

        /// <summary>Increments <see cref="_offset"/> by <paramref name="value"/> using modulo-<see cref="Capacity"/> arithmetic.</summary>
        /// <param name="offset">The offset into <see cref="_buffer"/> where the view begins.</param>
        /// <param name="value">The value by which to increase <see cref="_offset"/>. May not be negative.</param>
        /// <returns>The value of <see cref="_offset"/> after it was incremented.</returns>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private void PostIncrement(ref int offset, int value)
        {
            //_offset = (_offset + value) % Capacity;
            var tmp = offset + value;
            if (tmp < _buffer.Length)
            {
                offset = tmp;
            }
            else
            {
                offset = tmp % _buffer.Length;
            }
        }

        /// <summary>Decrements <see cref="_offset"/> by <paramref name="value"/> using modulo-<see cref="Capacity"/> arithmetic.</summary>
        /// <param name="offset">The offset into <see cref="_buffer"/> where the view begins.</param>
        /// <param name="value">The value by which to reduce <see cref="_offset"/>. May not be negative or greater than <see cref="Capacity"/>.</param>
        /// <returns>The value of <see cref="_offset"/> before it was decremented.</returns>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private void PreDecrement(ref int offset, int value)
        {
            //_offset -= value;
            //if (_offset < 0) { _offset += Capacity; }
            //return _offset;
            var tmp = offset - value;
            if ((uint)tmp > SharedConstants.TooBigOrNegative) { tmp += Capacity; }
            offset = tmp;
        }

        /// <summary>Inserts a single element to the back of the view. <see cref="IsFull"/> must be false when this method is called.</summary>
        /// <param name="value">The element to insert.</param>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private void DoAddToBack(T value)
        {
            //_buffer[DequeIndexToBufferIndex(_count++)] = value;
            var offset = _offset + _count;
            var buffer = _buffer;
            if ((uint)offset < (uint)buffer.Length)
            {
                buffer[offset] = value;
            }
            else
            {
                buffer[offset % buffer.Length] = value;
            }
            _count++;
        }

        /// <summary>Inserts a single element to the front of the view. <see cref="IsFull"/> must be false when this method is called.</summary>
        /// <param name="value">The element to insert.</param>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private void DoAddToFront(T value)
        {
            //_buffer[PreDecrement(1)] = value;
            var buffer = _buffer;
            var offset = _offset - 1;
            if ((uint)offset > SharedConstants.TooBigOrNegative) { offset += buffer.Length; }
            buffer[offset] = value;
            _offset = offset;
            _count++;
        }

        /// <summary>Removes and returns the last element in the view. <see cref="IsEmpty"/> must be false when this method is called.</summary>
        /// <returns>The former last element.</returns>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private T DoRemoveFromBack()
        {
            //var index = DequeIndexToBufferIndex(--_count);
            var count = _count - 1;
            var offset = _offset + count;
            _count = count;
            var buffer = _buffer;
            if ((uint)offset < (uint)buffer.Length)
            {
                T ret = buffer[offset];
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    buffer[offset] = default;
                }
#else
                buffer[offset] = default;
#endif
                return ret;
            }
            else
            {
                offset %= buffer.Length;
                T ret = buffer[offset];
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    buffer[offset] = default;
                }
#else
                buffer[offset] = default;
#endif
                return ret;
            }
        }

        /// <summary>Removes and returns the first element in the view. <see cref="IsEmpty"/> must be false when this method is called.</summary>
        /// <returns>The former first element.</returns>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private T DoRemoveFromFront()
        {
            var offset = _offset;
            var buffer = _buffer;
            var ret = buffer[offset];
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                buffer[offset] = default;
            }
#else
            buffer[offset] = default;
#endif
            PostIncrement(ref _offset, 1);
            _count--;

            return ret;
        }

        /// <summary>Inserts a range of elements into the view.</summary>
        /// <param name="index">The index into the view at which the elements are to be inserted.</param>
        /// <param name="collection">The elements to insert. The sum of <c>collection.Count</c> and <see cref="Count"/> must be less than or equal to <see cref="Capacity"/>.</param>
        private void DoInsertRange(int index, IReadOnlyCollection<T> collection)
        {
            var collectionCount = collection.Count;
            var buffer = _buffer;
            var count = _count;
            // Make room in the existing list
            if (index < count / 2)
            {
                // Inserting into the first half of the list

                // Move lower items down: [0, index) -> [Capacity - collectionCount, Capacity - collectionCount + index)
                // This clears out the low "index" number of items, moving them "collectionCount" places down;
                //   after rotation, there will be a "collectionCount"-sized hole at "index".
                int copyCount = index;
                int writeIndex = Capacity - collectionCount;
                for (int j = 0; j != copyCount; ++j)
                {
                    buffer[DequeIndexToBufferIndex(writeIndex + j)] = buffer[DequeIndexToBufferIndex(j)];
                }

                // Rotate to the new view
                PreDecrement(ref _offset, collectionCount);
            }
            else
            {
                // Inserting into the second half of the list

                // Move higher items up: [index, count) -> [index + collectionCount, collectionCount + count)
                int copyCount = count - index;
                int writeIndex = index + collectionCount;
                for (int j = copyCount - 1; j != -1; --j)
                {
                    buffer[DequeIndexToBufferIndex(writeIndex + j)] = buffer[DequeIndexToBufferIndex(index + j)];
                }
            }

            // Copy new items into place
            int i = index;
            foreach (T item in collection)
            {
                buffer[DequeIndexToBufferIndex(i)] = item;
                ++i;
            }

            // Adjust valid count
            _count = count + collectionCount;
        }

        /// <summary>Removes a range of elements from the view.</summary>
        /// <param name="index">The index into the view at which the range begins.</param>
        /// <param name="collectionCount">The number of elements in the range. This must be greater than 0 and less than or equal to <see cref="Count"/>.</param>
        private void DoRemoveRange(int index, int collectionCount)
        {
            var count = _count;
            var buffer = _buffer;
            if (0u >= (uint)index)
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
#endif
                    var maxIndex = _offset + collectionCount;
                    for (var idx = _offset; idx < maxIndex; idx++)
                    {
                        buffer[idx % Capacity] = default;
                    }
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                }
#endif
                // Removing from the beginning: rotate to the new view
                PostIncrement(ref _offset, collectionCount);
                _count = count - collectionCount;
                return;
            }
            else if (index == count - collectionCount)
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
#endif
                    var maxIndex = index + collectionCount;
                    for (var idx = index; idx < maxIndex; idx++)
                    {
                        buffer[DequeIndexToBufferIndex(index)] = default;
                    }
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                }
#endif
                // Removing from the ending: trim the existing view
                _count = count - collectionCount;
                return;
            }

            if ((index + (collectionCount / 2)) < count / 2)
            {
                // Removing from first half of list

                // Move lower items up: [0, index) -> [collectionCount, collectionCount + index)
                int copyCount = index;
                int writeIndex = collectionCount;
                for (int j = copyCount - 1; j != -1; --j)
                {
                    var idx = DequeIndexToBufferIndex(j);
                    buffer[DequeIndexToBufferIndex(writeIndex + j)] = buffer[idx];
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                    {
                        buffer[idx] = default;
                    }
#else
                    buffer[idx] = default;
#endif
                }

                // Rotate to new view
                PostIncrement(ref _offset, collectionCount);
            }
            else
            {
                // Removing from second half of list

                // Move higher items down: [index + collectionCount, count) -> [index, count - collectionCount)
                int copyCount = count - collectionCount - index;
                int readIndex = index + collectionCount;
                for (int j = 0; j != copyCount; ++j)
                {
                    var idx = DequeIndexToBufferIndex(readIndex + j);
                    buffer[DequeIndexToBufferIndex(index + j)] = buffer[idx];
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                    {
                        buffer[idx] = default;
                    }
#else
                    buffer[idx] = default;
#endif
                }
            }

            // Adjust valid count
            _count = count - collectionCount;
        }

        /// <summary>Doubles the capacity if necessary to make room for one more element. When this method returns, <see cref="IsFull"/> is false.</summary>
        [MethodImpl(InlineMethod.AggressiveInlining)]
        private void EnsureCapacityForOneElement()
        {
            var capacity = _buffer.Length; // Capacity
            if (_count == capacity) // IsFull
            {
                Capacity = (0u >= (uint)capacity) ? 1 : capacity * 2;
            }
        }

        #endregion

        #region -- AddLast​ --

        /// <summary>Inserts a single element at the back of this deque.</summary>
        /// <param name="value">The element to insert.</param>
        public void AddLast​(T value)
        {
            EnsureCapacityForOneElement();
            DoAddToBack(value);
        }

        #endregion

        #region -- AddFirst​ --

        /// <summary>Inserts a single element at the front of this deque.</summary>
        /// <param name="value">The element to insert.</param>
        public void AddFirst​(T value)
        {
            EnsureCapacityForOneElement();
            DoAddToFront(value);
        }

        #endregion

        #region -- InsertRange --

        /// <summary>Inserts a collection of elements into this deque.</summary>
        /// <param name="index">The index at which the collection is inserted.</param>
        /// <param name="collection">The collection of elements to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index to an insertion point for the source.</exception>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (/*index < 0 || */(uint)index > (uint)_count) { CheckNewIndexArgument(_count, index); }
            var source = CollectionHelpers.ReifyCollection(collection);
            int collectionCount = source.Count;

            // Overflow-safe check for "Count + collectionCount > Capacity"
            if (collectionCount > Capacity - _count)
            {
                Capacity = checked(_count + collectionCount);
            }

            if (0u >= (uint)collectionCount) { return; }

            DoInsertRange(index, source);
        }

        #endregion

        #region -- RemoveRange --

        /// <summary>Removes a range of elements from this deque.</summary>
        /// <param name="offset">The index into the deque at which the range begins.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Either <paramref name="offset"/> or <paramref name="count"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">The range [<paramref name="offset"/>, <paramref name="offset"/> + <paramref name="count"/>) is not within the range [0, <see cref="Count"/>).</exception>
        public void RemoveRange(int offset, int count)
        {
            uint uCount = (uint)count;
            if (0u >= uCount) { return; }
            if ((uint)offset > SharedConstants.TooBigOrNegative) { CheckRangeArguments_Offset(offset); }
            if (uCount > SharedConstants.TooBigOrNegative) { CheckRangeArguments_Count(count); }
            if (uCount > (uint)(_count - offset)) { CheckRangeArguments(_count, offset, count); }

            DoRemoveRange(offset, count);
        }

        #endregion

        #region -- RemoveAll --

        public int RemoveAll(Predicate<T> match)
        {
            if (match is null) { ThrowArgumentNullException_Match(); }

            if (IsEmpty) { return 0; }

            int freeIndex = 0;   // the first free slot in items array
            var buffer = _buffer;
            var count = _count;

            // Find the first item which needs to be removed.
            while (freeIndex < count && !match(buffer[DequeIndexToBufferIndex(freeIndex)])) freeIndex++;
            if (freeIndex >= count) { return 0; }

            int current = freeIndex + 1;
            while (current < count)
            {
                // Find the first item which needs to be kept.
                while (current < count && match(buffer[DequeIndexToBufferIndex(current)])) current++;

                if (current < count)
                {
                    var idx = DequeIndexToBufferIndex(current++);
                    // copy item to the free slot.
                    buffer[DequeIndexToBufferIndex(freeIndex++)] = buffer[idx];
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                    {
                        buffer[idx] = default;
                    }
#else
                    buffer[idx] = default;
#endif
                }
            }

            int result = count - freeIndex;
            _count = freeIndex;
            return result;
        }

        #endregion

        #region -- UpdateAll --

        public int UpdateAll(Predicate<T> match, Func<T, T> updateValueFactory)
        {
            if (match is null) { ThrowArgumentNullException_Match(); }
            if (updateValueFactory is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.updateValueFactory); }
            if (IsEmpty) { return 0; }

            var buffer = _buffer;
            var size = 0;
            var idx = 0;
            while (idx < _count)
            {
                //var index = DequeIndexToBufferIndex(idx);
                var index = _offset + idx;
                if ((uint)index < (uint)buffer.Length)
                {
                    var item = buffer[index];
                    if (match(item))
                    {
                        buffer[index] = updateValueFactory(item);
                        size++;
                    }
                }
                else
                {
                    var offset = index % buffer.Length;
                    var item = buffer[offset];
                    if (match(item))
                    {
                        buffer[offset] = updateValueFactory(item);
                        size++;
                    }
                }

                idx++;
            }
            return size;
        }

        #endregion

        #region -- ConvertAll --

        public Deque<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            if (converter is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.converter); }

            var deque = new Deque<TOutput>(_count);

            var idx = 0;
            while (idx < _count)
            {
                var item = DoGetItem(idx);
                deque._buffer[idx] = converter(item);
                idx++;
            }
            deque._count = _count;

            return deque;
        }

        #endregion

        #region -- Exists --

        public bool Exists(Predicate<T> match) => (uint)_count > (uint)FindIndex(match);

        #endregion

        #region -- Find --

        public T Find(Predicate<T> match)
        {
            if (match is null) { ThrowArgumentNullException_Match(); }

            var idx = 0;
            uint uCount = (uint)_count;
            while ((uint)idx < uCount)
            {
                var item = DoGetItem(idx);
                if (match(item)) { return item; }
                idx++;
            }
            return default;
        }

        #endregion

        #region -- FindAll --

        public Deque<T> FindAll(Predicate<T> match)
        {
            if (match is null) { ThrowArgumentNullException_Match(); }

            var list = new Deque<T>();
            var idx = 0;
            uint uCount = (uint)_count;
            while ((uint)idx < uCount)
            {
                var item = DoGetItem(idx);
                if (match(item)) { list.AddLast​(item); }
                idx++;
            }
            return list;
        }

        #endregion

        #region -- FindIndex --

        public int FindIndex(Predicate<T> match) => FindIndex(0, _count, match);

        public int FindIndex(int startIndex, Predicate<T> match) => FindIndex(startIndex, _count - startIndex, match);

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            if ((uint)startIndex > (uint)_count) { ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index(); }
            uint uEndIndex = (uint)(startIndex + count);
            if ((uint)count > SharedConstants.TooBigOrNegative || uEndIndex > (uint)_count) { ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count(); }
            if (match is null) { ThrowArgumentNullException_Match(); }

            var idx = startIndex;
            while ((uint)idx < uEndIndex)
            {
                var item = DoGetItem(idx);
                if (match(item)) { return idx; }
                idx++;
            }

            return -1;
        }

        #endregion

        #region -- FindLast --

        public T FindLast(Predicate<T> match)
        {
            if (match is null) { ThrowArgumentNullException_Match(); }

            var idx = _count - 1;
            while (idx >= 0)
            {
                var item = DoGetItem(idx);
                if (match(item)) { return item; }
                idx--;
            }

            return default;
        }

        #endregion

        #region -- FindLastIndex --

        public int FindLastIndex(Predicate<T> match) => FindLastIndex(_count - 1, _count, match);

        public int FindLastIndex(int startIndex, Predicate<T> match) => FindLastIndex(startIndex, startIndex + 1, match);

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            if (match is null) { ThrowArgumentNullException_Match(); }
            uint uCount = (uint)_count;
            if (0u >= uCount)
            {
                // Special case for 0 length List
                if (startIndex != -1)
                {
                    ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
                }
            }
            else
            {
                // Make sure we're not out of range
                if ((uint)startIndex >= uCount)
                {
                    ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
                }
            }

            int endIndex = startIndex - count;
            // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
            if ((uint)count > SharedConstants.TooBigOrNegative || (uint)(endIndex + 1) > SharedConstants.TooBigOrNegative)
            {
                ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
            }

            var idx = startIndex;
            while (idx > endIndex)
            {
                if (match(DoGetItem(idx))) { return idx; }
                idx--;
            }

            return -1;
        }

        #endregion

        #region -- ForEach --

        public void ForEach(Action<T> action)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = 0;
            while (idx < _count)
            {
                action(DoGetItem(idx));
                idx++;
            }
        }

        public void ForEach<TArg>(Action<T, TArg> action, TArg arg)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = 0;
            while (idx < _count)
            {
                action(DoGetItem(idx), arg);
                idx++;
            }
        }

        public void ForEach<TArg1, TArg2>(Action<T, TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = 0;
            while (idx < _count)
            {
                action(DoGetItem(idx), arg1, arg2);
                idx++;
            }
        }


        public void ForEach(Action<T, int> action)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = 0;
            while (idx < _count)
            {
                action(DoGetItem(idx), idx);
                idx++;
            }
        }

        public void ForEach<TArg>(Action<T, int, TArg> action, TArg arg)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = 0;
            while (idx < _count)
            {
                action(DoGetItem(idx), idx, arg);
                idx++;
            }
        }

        public void ForEach<TArg1, TArg2>(Action<T, int, TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = 0;
            while (idx < _count)
            {
                action(DoGetItem(idx), idx, arg1, arg2);
                idx++;
            }
        }

        #endregion

        #region -- Reverse --

        public void Reverse(Action<T> action)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = _count - 1;
            while (idx >= 0)
            {
                action(DoGetItem(idx));
                idx--;
            }
        }

        public void Reverse<TArg>(Action<T, TArg> action, TArg arg)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = _count - 1;
            while (idx >= 0)
            {
                action(DoGetItem(idx), arg);
                idx--;
            }
        }

        public void Reverse<TArg1, TArg2>(Action<T, TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = _count - 1;
            while (idx >= 0)
            {
                action(DoGetItem(idx), arg1, arg2);
                idx--;
            }
        }

        public void Reverse(Action<T, int> action)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = _count - 1;
            while (idx >= 0)
            {
                action(DoGetItem(idx), idx);
                idx--;
            }
        }

        public void Reverse<TArg>(Action<T, int, TArg> action, TArg arg)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = _count - 1;
            while (idx >= 0)
            {
                action(DoGetItem(idx), idx, arg);
                idx--;
            }
        }

        public void Reverse<TArg1, TArg2>(Action<T, int, TArg1, TArg2> action, TArg1 arg1, TArg2 arg2)
        {
            if (action is null) { ThrowArgumentNullException_Action(); }
            if (IsEmpty) { return; }

            var idx = _count - 1;
            while (idx >= 0)
            {
                action(DoGetItem(idx), idx, arg1, arg2);
                idx--;
            }
        }

        #endregion

        #region -- TrueForAll --

        public bool TrueForAll(Predicate<T> match)
        {
            if (match is null) { ThrowArgumentNullException_Match(); }

            if (IsEmpty) { return false; }

            var idx = 0;
            while (idx < _count)
            {
                if (!match(DoGetItem(idx))) { return false; }
                idx++;
            }
            return true;
        }

        #endregion

        #region -- GetLast / Last --

        public T Last
        {
            get
            {
                if (IsEmpty) { ThrowInvalidOperationException(); }

                return DoGetItem(_count - 1);
            }
        }

        public T LastOrDefault
        {
            get
            {
                if (IsEmpty) { return default; }

                return DoGetItem(_count - 1);
            }
        }

        public T GetLast()
        {
            if (IsEmpty) { ThrowInvalidOperationException(); }

            return DoGetItem(_count - 1);
        }

        public bool TryGetLast(out T result)
        {
            if (IsEmpty) { result = default; return false; }

            result = DoGetItem(_count - 1);
            return true;
        }

        #endregion

        #region -- RemoveLast --

        /// <summary>Removes and returns the last element of this deque.</summary>
        /// <returns>The former last element.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        public T RemoveLast()
        {
            if (IsEmpty) { ThrowInvalidOperationException(); }

            return DoRemoveFromBack();
        }

        /// <summary>Removes and returns the last element of this deque.</summary>
        /// <param name="result">The former last element.</param>
        /// <returns>true if an item could be dequeued; otherwise, false.</returns>
        public bool TryRemoveLast(out T result)
        {
            if (IsEmpty) { result = default; return false; }

            result = DoRemoveFromBack();
            return true;
        }

        public bool TryRemoveLast(List<T> results, int count)
        {
            if (results is null) { ThrowArgumentNullException_Results(); }
            if ((uint)count > SharedConstants.TooBigOrNegative) { CheckRangeArguments_Count(count); }

            if (IsEmpty) { return false; }

            var buffer = _buffer;
            var startIndex = _count - 1;
            var maxCount = Math.Min(_count, count);
            var endIndex = startIndex - maxCount;
            var idx = startIndex;
            while (idx > endIndex)
            {
                //var index = DequeIndexToBufferIndex(idx);
                var index = _offset + idx;
                if ((uint)index < (uint)buffer.Length)
                {
                    results.Add(buffer[index]);
                    buffer[index] = default;
                }
                else
                {
                    var offset = index % buffer.Length;
                    results.Add(buffer[offset]);
                    buffer[offset] = default;
                }

                idx--;
            }

            _count -= maxCount;

            return true;
        }

        /// <summary>Removes and returns the last element of this deque.</summary>
        /// <param name="match">The predicate that must return true for the item to be dequeued.  If null, all items implicitly return true.</param>
        /// <param name="result">The former last element.</param>
        /// <returns>true if an item could be dequeued; otherwise, false.</returns>
        public bool TryRemoveLastIf(Predicate<T> match, out T result)
        {
            if (match is null) { ThrowArgumentNullException_Match(); }

            if (IsEmpty) { result = default; return false; }

            var size = _count - 1;
            var buffer = _buffer;

            //var index = DequeIndexToBufferIndex(size);
            var index = _offset + size;
            if ((uint)index < (uint)buffer.Length)
            {
                var item = buffer[index];
                if (match(item))
                {
                    result = item;
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                    {
                        buffer[index] = default;
                    }
#else
                    buffer[index] = default;
#endif
                    _count = size;
                    return true;
                }
            }
            else
            {
                index %= buffer.Length;
                var item = buffer[index];
                if (match(item))
                {
                    result = item;
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                    {
                        buffer[index] = default;
                    }
#else
                    buffer[index] = default;
#endif
                    _count = size;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public bool TryRemoveLastUntil(Predicate<T> match, out T result)
        {
            if (match is null) { ThrowArgumentNullException_Match(); }

            if (IsEmpty) { result = default; return false; }

            var found = false;
            T item = default;
            var idx = _count - 1;
            while (idx >= 0)
            {
                item = DoGetItem(idx);
                if (match(item))
                {
                    found = true;
                    break;
                }
                idx--;
            }

            if (found) { DoRemoveAt(idx); }

            result = item;
            return found;
        }

        #endregion

        #region -- GetFirst / First --

        public T First
        {
            get
            {
                if (IsEmpty) { ThrowInvalidOperationException(); }
                return _buffer[_offset];
            }
        }

        public T FirstOrDefault
        {
            get
            {
                if (IsEmpty) { return default; }
                return _buffer[_offset];
            }
        }

        public T GetFirst()
        {
            if (IsEmpty) { ThrowInvalidOperationException(); }
            return _buffer[_offset];
        }

        public bool TryGetFirst(out T result)
        {
            if (IsEmpty) { result = default; return false; }

            result = _buffer[_offset];
            return true;
        }

        #endregion

        #region -- RemoveFirst --

        /// <summary>Removes and returns the first element of this deque.</summary>
        /// <returns>The former first element.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        public T RemoveFirst()
        {
            if (IsEmpty) { ThrowInvalidOperationException(); }

            return DoRemoveFromFront();
        }

        /// <summary>Removes and returns the first element of this deque.</summary>
        /// <param name="result">The former first element.</param>
        /// <returns>true if an item could be dequeued; otherwise, false.</returns>
        public bool TryRemoveFirst(out T result)
        {
            if (IsEmpty) { result = default; return false; }

            result = DoRemoveFromFront();
            return true;
        }

        public bool TryRemoveFirst(List<T> results, int count)
        {
            if (results is null) { ThrowArgumentNullException_Results(); }
            if ((uint)count > SharedConstants.TooBigOrNegative) { CheckRangeArguments_Count(count); }

            if (IsEmpty) { return false; }

            var buffer = _buffer;
            var idx = 0;
            var maxCount = Math.Min(_count, count);
            while (idx < maxCount)
            {
                //var index = DequeIndexToBufferIndex(idx);
                var index = _offset + idx;
                if ((uint)index < (uint)buffer.Length)
                {
                    results.Add(buffer[index]);
                    buffer[index] = default;
                }
                else
                {
                    var offset = index % buffer.Length;
                    results.Add(buffer[offset]);
                    buffer[offset] = default;
                }

                idx++;
            }

            _count -= idx;
            PostIncrement(ref _offset, idx);

            return true;
        }

        /// <summary>Removes and returns the first element of this deque.</summary>
        /// <param name="match">The predicate that must return true for the item to be dequeued.  If null, all items implicitly return true.</param>
        /// <param name="result">The former first element.</param>
        /// <returns>true if an item could be dequeued; otherwise, false.</returns>
        public bool TryRemoveFirstIf(Predicate<T> match, out T result)
        {
            if (match is null) { ThrowArgumentNullException_Match(); }

            if (IsEmpty) { result = default; return false; }

            var offset = _offset;
            var buffer = _buffer;
            var item = buffer[offset];
            if (match(item))
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    buffer[offset] = default;
                }
#else
                buffer[offset] = default;
#endif
                --_count;
                result = item;
                PostIncrement(ref _offset, 1);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public bool TryRemoveFirstUntil(Predicate<T> match, out T result)
        {
            if (match is null) { ThrowArgumentNullException_Match(); }

            if (IsEmpty) { result = default; return false; }

            var found = false;
            T item = default;
            var idx = 0;
            while (idx < _count)
            {
                item = DoGetItem(idx);
                if (match(item))
                {
                    found = true;
                    break;
                }
                idx++;
            }

            if (found) { DoRemoveAt(idx); }

            result = item;
            return found;
        }

        #endregion

        #region -- Clear --

        /// <summary>Removes all items from this deque.</summary>
        public void Clear()
        {
            if (NonEmpty)
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
#endif
                    if (IsSplit)
                    {
                        // The existing buffer is split, so we have to copy it in parts
                        int length = Capacity - _offset;
                        Array.Clear(_buffer, _offset, length);
                        Array.Clear(_buffer, 0, _count - length);
                    }
                    else
                    {
                        // The existing buffer is whole
                        Array.Clear(_buffer, _offset, _count);
                    }
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                }
#endif
            }
            _offset = 0;
            _count = 0;
        }

        #endregion

        #region -- ToArray --

        /// <summary>Creates and returns a new array containing the elements in this deque.</summary>
        public T[] ToArray()
        {
            var result = new T[_count];
            CopyTo(result, 0);
            return result;
        }

        #endregion

        #region -- struct Enumerator --

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly Deque<T> _deque;
            //private readonly int _version;
            private int _index;   // -1 = not started, -2 = ended/disposed
            private T _currentElement;

            internal Enumerator(Deque<T> deque)
            {
                _deque = deque;
                //_version = deque._version;
                _index = -1;
                _currentElement = default;
            }

            public void Dispose()
            {
                _index = -2;
                _currentElement = default;
            }

            public bool MoveNext()
            {
                //if (_version != _deque._version) throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);

                if (_index == -2) { return false; }

                _index++;

                if (_index == _deque._count)
                {
                    // We've run past the last element
                    _index = -2;
                    _currentElement = default;
                    return false;
                }

                _currentElement = _deque.DoGetItem(_index);
                return true;
            }

            public T Current
            {
                get
                {
                    if ((uint)_index > SharedConstants.TooBigOrNegative) { ThrowEnumerationNotStartedOrEnded(_index); }
                    return _currentElement;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowEnumerationNotStartedOrEnded(int index)
            {
                Debug.Assert(index == -1 || index == -2);
                throw GetInvalidOperationException();
                InvalidOperationException GetInvalidOperationException()
                {
                    throw new InvalidOperationException(index == -1
                        ? "Enumeration has not started. Call MoveNext."
                        : "Enumeration already finished.");
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            void IEnumerator.Reset()
            {
                //if (_version != _deque._version) throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                _index = -1;
                _currentElement = default;
            }
        }

        #endregion

        #region -- struct ReversingEnumerator --

        public struct ReversingEnumerator : IEnumerator<T>, IEnumerator
        {
            private readonly Deque<T> _deque;
            //private readonly int _version;
            private int _index;   // -1 = not started, -2 = ended/disposed
            private T _currentElement;

            internal ReversingEnumerator(Deque<T> q)
            {
                _deque = q;
                //_version = _deque._version;
                _index = -2;
                _currentElement = default;
            }

            public void Dispose()
            {
                _index = -1;
                _currentElement = default;
            }

            public bool MoveNext()
            {
                bool retval;
                //if (_version != _deque._version) throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                if (_index == -2)
                {  // First call to enumerator.
                    _index = _deque.Count - 1;
                    retval = (_index >= 0);
                    if (retval)
                    {
                        _currentElement = _deque.DoGetItem(_index);
                    }
                    return retval;
                }
                if (_index == -1)
                {  // End of enumeration.
                    return false;
                }

                retval = (--_index >= 0);
                if (retval)
                    _currentElement = _deque.DoGetItem(_index);
                else
                    _currentElement = default;
                return retval;
            }

            public T Current
            {
                get
                {
                    if ((uint)_index > SharedConstants.TooBigOrNegative) { ThrowEnumerationNotStartedOrEnded(_index); }
                    return _currentElement;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowEnumerationNotStartedOrEnded(int index)
            {
                Debug.Assert(index == -1 || index == -2);
                throw GetInvalidOperationException();
                InvalidOperationException GetInvalidOperationException()
                {
                    throw new InvalidOperationException(index == -2
                        ? "Enumeration has not started. Call MoveNext."
                        : "Enumeration already finished.");
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            void IEnumerator.Reset()
            {
                //if (_version != _deque._version) throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                _index = -2;
                _currentElement = default;
            }
        }

        #endregion

        #region ** class DebugView **

        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly Deque<T> deque;

            public DebugView(Deque<T> deque)
            {
                this.deque = deque;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Items
            {
                get { return deque.ToArray(); }
            }
        }

        #endregion
    }
}
