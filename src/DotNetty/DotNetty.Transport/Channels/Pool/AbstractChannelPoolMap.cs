/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Transport.Channels.Pool
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DotNetty.Common.Utilities;

    public abstract class AbstractChannelPoolMap<TKey, TPool> : IChannelPoolMap<TKey, TPool>
        where TPool : IChannelPool
    {
        private readonly ConcurrentDictionary<TKey, TPool> _map;

        public AbstractChannelPoolMap()
        {
            _map = new ConcurrentDictionary<TKey, TPool>();
        }

        public AbstractChannelPoolMap(IEqualityComparer<TKey> comparer)
        {
            _map = new ConcurrentDictionary<TKey, TPool>(comparer);
        }

        public TPool Get(TKey key)
        {
            if (key is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key); }

            if (!_map.TryGetValue(key, out TPool pool))
            {
                pool = NewPool(key);
                TPool old = _map.GetOrAdd(key, pool);
                if (!ReferenceEquals(old, pool))
                {
                    // We need to destroy the newly created pool as we not use it.
                    _ = PoolCloseAsyncIfSupported(pool);
                    pool = old;
                }
            }

            return pool;
        }

        /// <summary>
        /// Removes the <see cref="IChannelPool"/> from this <see cref="AbstractChannelPoolMap{TKey, TPool}"/>.
        /// 
        /// <para>If the removed pool extends <see cref="SimpleChannelPool"/> it will be closed asynchronously to avoid blocking in
        /// this method.</para>
        /// </summary>
        /// <param name="key">The key to remove. Must not be null.</param>
        /// <returns><c>true</c> if removed, otherwise <c>false</c>.</returns>
        public bool Remove(TKey key)
        {
            if (key is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key); }
            if (_map.TryRemove(key, out TPool pool))
            {
                _ = PoolCloseAsyncIfSupported(pool);
                return true;
            }
            return false;
        }

        private Task<bool> RemoveAsyncIfSupported(TKey key)
        {
            if (key is null) { return ThrowHelper.FromArgumentNullException<bool>(ExceptionArgument.key); }

            if (_map.TryRemove(key, out TPool pool))
            {
                var removePromise = new TaskCompletionSource<bool>();
                PoolCloseAsyncIfSupported(pool).ContinueWith((t, s) =>
                    {
                        if (t.IsSuccess())
                        {
                            ((TaskCompletionSource<bool>)s).TrySetResult(true);
                        }
                        else
                        {
                            ((TaskCompletionSource<bool>)s).TrySetException(TaskUtil.Unwrap(t.Exception));
                        }
                    },
                    removePromise
#if !NET451
                    , TaskContinuationOptions.RunContinuationsAsynchronously
#endif
                );
                return removePromise.Task;
            }
            return TaskUtil.False;
        }

        /// <summary>
        /// If the pool implementation supports asynchronous close, then use it to avoid a blocking close call in case
        /// the ChannelPoolMap operations are called from an EventLoop.
        /// </summary>
        /// <param name="pool">the ChannelPool to be closed</param>
        /// <returns></returns>
        private static Task PoolCloseAsyncIfSupported(IChannelPool pool)
        {
            if (pool is SimpleChannelPool simpleChannelPool)
            {
                return simpleChannelPool.CloseAsync();
            }
            else
            {
                try
                {
                    pool.Close();
                    return TaskUtil.Completed;
                }
                catch (Exception exc)
                {
                    return TaskUtil.FromException(exc);
                }
            }
        }

        /// <summary>
        /// Returns the number of <see cref="IChannelPool"/>s currently in this <see cref="AbstractChannelPoolMap{TKey, TPool}"/>.
        /// </summary>
        public int Count => _map.Count;

        /// <summary>
        /// Returns <c>true</c> if the <see cref="AbstractChannelPoolMap{TKey, TPool}"/> is empty, otherwise <c>false</c>.
        /// </summary>
        public bool IsEmpty => 0u >= (uint)_map.Count;

        public bool Contains(TKey key)
        {
            if (key is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key); }
            return _map.ContainsKey(key);
        }

        /// <summary>
        /// Called once a new <see cref="IChannelPool"/> needs to be created as none exists yet for the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <typeparamref name="TKey"/> to create a new <typeparamref name="TPool"/> for.</param>
        /// <returns>The new <typeparamref name="TPool"/> corresponding to the given <typeparamref name="TKey"/>.</returns>
        protected abstract TPool NewPool(TKey key);

        public virtual void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (TKey key in _map.Keys)
                {
                    // Wait for remove to finish to ensure that resources are released before returning from close
                    try
                    {
                        _ = RemoveAsyncIfSupported(key).GetAwaiter().GetResult();
                    }
                    catch { }
                }
            }
        }
    }
}
