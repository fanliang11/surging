// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class EmptyPriorityQueue<T> : IPriorityQueue<T>
        where T : class, IPriorityQueueNode<T>
    {
        public static readonly EmptyPriorityQueue<T> Instance = new EmptyPriorityQueue<T>();

        EmptyPriorityQueue()
        {
        }
        
        public bool TryEnqueue(T item) => false;

        public bool TryDequeue(out T item)
        {
            item = default;
            return false;
        }

        public bool TryPeek(out T item)
        {
            item = default;
            return false;
        }

        public int Count => 0;

        public bool IsEmpty => true;

        public bool NonEmpty => false;

        public void Clear()
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryRemove(T item) => false;

        public bool Contains(T item) => false;

        public void PriorityChanged(T item)
        {
        }

        public void ClearIgnoringIndexes()
        {
        }
    }
}