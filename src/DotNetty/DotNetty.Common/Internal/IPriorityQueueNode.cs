// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Internal
{
    /// <summary>
    /// Provides methods for <see cref="DefaultPriorityQueue{T}"/> to maintain internal state. These methods should generally not be
    /// used outside the scope of <see cref="DefaultPriorityQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPriorityQueueNode<T>
        where T : class, IPriorityQueueNode<T>
    {
        /// <summary>
        /// Get the last value set by <see cref="SetPriorityQueueIndex(IPriorityQueue{T}, int)"/> for the value corresponding to
        /// <paramref name="queue"/>.
        /// 
        /// Throwing exceptions from this method will result in undefined behavior.
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        int GetPriorityQueueIndex(IPriorityQueue<T> queue);

        /// <summary>
        /// Used by <see cref="DefaultPriorityQueue{T}"/> to maintain state for an element in the queue.
        /// 
        /// Throwing exceptions from this method will result in undefined behavior.
        /// </summary>
        /// <param name="queue">The queue for which the index is being set.</param>
        /// <param name="i">The index as used by <see cref="DefaultPriorityQueue{T}"/>.</param>
        void SetPriorityQueueIndex(IPriorityQueue<T> queue, int i);
    }
}