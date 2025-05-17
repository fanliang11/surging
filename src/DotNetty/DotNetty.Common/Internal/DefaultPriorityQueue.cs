// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using DotNetty.Common.Utilities;

    public class DefaultPriorityQueue<T> : IPriorityQueue<T>
        where T : class, IPriorityQueueNode<T>
    {
        private static readonly T[] EmptyArray = EmptyArray<T>.Instance;

        public const int IndexNotInQueue = -1;

        private readonly IComparer<T> _comparer;
        private int _count;
        private int _capacity;
        private T[] _items;

        public DefaultPriorityQueue()
            : this(Comparer<T>.Default)
        {
        }

        public DefaultPriorityQueue(IComparer<T> comparer)
            : this(comparer, 11)
        {
        }

        public DefaultPriorityQueue(IComparer<T> comparer, int initialCapacity)
        {
            if (comparer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparer); }

            _comparer = comparer;
            _capacity = initialCapacity;
            _items = _capacity != 0 ? new T[_capacity] : EmptyArray;
        }

        public int Count => _count;

        public bool IsEmpty => 0u >= (uint)_count;

        public bool NonEmpty => (uint)_count > 0u;

        public T Peek()
        {
            if (0u >= (uint)_count) { return null; }
            return _items[0];
        }

        public bool TryPeek(out T item)
        {
            if (0u >= (uint)_count)
            {
                item = null;
                return false;
            }
            item = _items[0];
            return true;
        }

        public T Dequeue()
        {
            if (TryDequeue(out T item)) { return item; }
            return null;
        }

        public bool TryDequeue(out T item)
        {
            item = Peek();
            if (item is null) { return false; }
            item.SetPriorityQueueIndex(this, IndexNotInQueue);

            T lastItem = _items[--_count];
            _items[_count] = null;
            if (_count > 0) // Make sure we don't add the last element back.
            {
                BubbleDown(0, lastItem);
            }

            return true;
        }

        public void Enqueue(T item)
        {
            _ = TryEnqueue(item);
        }

        public bool TryEnqueue(T item)
        {
            if (item is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.item); }

            int index = item.GetPriorityQueueIndex(this);
            if (index != IndexNotInQueue)
            {
                ThrowHelper.ThrowArgumentException_PriorityQueueIndex(index, item);
            }

            int oldCount = _count;
            // Check that the array capacity is enough to hold values by doubling capacity.
            if (0u >= (uint)(oldCount - _capacity))
            {
                GrowHeap();
            }
            _count = oldCount + 1;
            BubbleUp(oldCount, item);

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowHeap()
        {
            int oldCapacity = _capacity;
            // Use a policy which allows for a 0 initial capacity. double when
            // "small", then grow by 50% when "large".
            _capacity = oldCapacity + (oldCapacity <= 64 ? oldCapacity + 2 : (oldCapacity.RightUShift(1)));
            var newHeap = new T[_capacity];
            Array.Copy(_items, 0, newHeap, 0, _count);
            _items = newHeap;
        }

        public bool Contains(T item)
            => Contains(item, item.GetPriorityQueueIndex(this));

        public bool TryRemove(T item)
        {
            int index = item.GetPriorityQueueIndex(this);
            if (!Contains(item, index)) { return false; }

            item.SetPriorityQueueIndex(this, IndexNotInQueue);

            _count--;
            if (0u >= (uint)_count || 0u >= (uint)(index - _count))
            {
                // If there are no node left, or this is the last node in the array just remove and return.
                _items[index] = default;
                return true;
            }

            // Move the last element where node currently lives in the array.
            T last = _items[index] = _items[_count];
            _items[_count] = default;
            // priorityQueueIndex will be updated below in bubbleUp or bubbleDown

            // Make sure the moved node still preserves the min-heap properties.
            if ((uint)_comparer.Compare(item, last) > SharedConstants.TooBigOrNegative) // < 0
            {
                BubbleDown(index, last);
            }
            else
            {
                BubbleUp(index, last);
            }
            return true;
        }

        public void PriorityChanged(T node)
        {
            int i = node.GetPriorityQueueIndex(this);
            if (!Contains(node, i)) { return; }

            // Preserve the min-heap property by comparing the new priority with parents/children in the heap.
            if (0u >= (uint)i)
            {
                BubbleDown(i, node);
            }
            else
            {
                // Get the parent to see if min-heap properties are violated.
                int parentIndex = (i - 1).RightUShift(1);
                T parent = _items[parentIndex];
                if ((uint)_comparer.Compare(node, parent) > SharedConstants.TooBigOrNegative) // < 0
                {
                    BubbleUp(i, node);
                }
                else
                {
                    BubbleDown(i, node);
                }
            }
        }

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private bool Contains(T node, int i)
            => /*i >= 0 && */(uint)i < (uint)_count && node.Equals(_items[i]);

        private void BubbleDown(int index, T item)
        {
            int middleIndex = _count.RightUShift(1);
            while (index < middleIndex)
            {
                // Compare node to the children of index.
                int childIndex = (index << 1) + 1;
                T childItem = _items[childIndex];

                // Make sure we get the smallest child to compare against.
                int rightChildIndex = childIndex + 1;
                if (rightChildIndex < _count &&
                    _comparer.Compare(childItem, _items[rightChildIndex]) > 0)
                {
                    childIndex = rightChildIndex;
                    childItem = _items[rightChildIndex];
                }
                // If the bubbleDown node is less than or equal to the smallest child then we will preserve the min-heap
                // property by inserting the bubbleDown node here.
                var result = _comparer.Compare(item, childItem);
                if ((uint)(result - 1) > SharedConstants.TooBigOrNegative) // <= 0
                {
                    break;
                }

                // Bubble the child up.
                _items[index] = childItem;
                childItem.SetPriorityQueueIndex(this, index);

                // Move down index down the tree for the next iteration.
                index = childIndex;
            }

            // We have found where node should live and still satisfy the min-heap property, so put it in the queue.
            _items[index] = item;
            item.SetPriorityQueueIndex(this, index);
        }

        private void BubbleUp(int index, T item)
        {
            // index > 0 means there is a parent
            while (index > 0)
            {
                int parentIndex = (index - 1).RightUShift(1);
                T parentItem = _items[parentIndex];

                // If the bubbleUp node is less than the parent, then we have found a spot to insert and still maintain
                // min-heap properties.
                if (SharedConstants.TooBigOrNegative >= (uint)_comparer.Compare(item, parentItem)) // >= 0
                {
                    break;
                }

                // Bubble the parent down.
                _items[index] = parentItem;
                parentItem.SetPriorityQueueIndex(this, index);

                // Move k up the tree for the next iteration.
                index = parentIndex;
            }

            // We have found where node should live and still satisfy the min-heap property, so put it in the queue.
            _items[index] = item;
            item.SetPriorityQueueIndex(this, index);
        }


        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Clear()
        {
            for (int i = 0; i < _count; i++)
            {
                var item = _items[i];
                if (item is object)
                {
                    item.SetPriorityQueueIndex(this, IndexNotInQueue);
                    _items[i] = default;
                }
            }
            _count = 0;
        }

        public void ClearIgnoringIndexes()
        {
            _count = 0;
        }
    }
}