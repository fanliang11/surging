// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    using System.Collections.Generic;

    public interface IPriorityQueue<T> : IQueue<T>, IEnumerable<T>
        where T : class, IPriorityQueueNode<T>
    {
        bool TryRemove(T item);

        bool Contains(T item);

        /// <summary>
        /// Notify the queue that the priority for <paramref name="item"/> has changed. The queue will adjust to ensure the priority
        /// queue properties are maintained.
        /// </summary>
        /// <param name="item">An object which is in this queue and the priority may have changed.</param>
        void PriorityChanged(T item);

        /// <summary>
        /// Removes all of the elements from this <see cref="IPriorityQueue{T}"/> without calling
        /// <see cref="IPriorityQueueNode{T}.GetPriorityQueueIndex(IPriorityQueue{T})"/> or explicitly removing references to them to
        /// allow them to be garbage collected. This should only be used when it is certain that the nodes will not be
        /// re-inserted into this or any other <see cref="IPriorityQueue{T}"/> and it is known that the <see cref="IPriorityQueue{T}"/> itself
        /// will be garbage collected after this call.
        /// </summary>
        void ClearIgnoringIndexes();
    }
}