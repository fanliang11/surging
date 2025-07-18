// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotNetty.Common.Internal
{
    internal class MruCache<TKey, TValue> : IDisposable
        where TValue : class
    {
        private LinkedList<TKey> _mruList;
        private Dictionary<TKey, CacheEntry> _items;
        private readonly int _lowWatermark;
        private readonly int _highWatermark;
        private CacheEntry _mruEntry;
        private bool _disposed;

        public MruCache(int watermark)
          : this(watermark * 4 / 5, watermark)
        {
        }

        //
        // The cache will grow until the high watermark. At which point, the least recently used items
        // will be purge until the cache's size is reduced to low watermark
        //
        public MruCache(int lowWatermark, int highWatermark)
          : this(lowWatermark, highWatermark, null)
        {
        }

        public MruCache(int lowWatermark, int highWatermark, IEqualityComparer<TKey> comparer)
        {
            Debug.Assert(lowWatermark < highWatermark, "");
            Debug.Assert(lowWatermark >= 0, "");

            _lowWatermark = lowWatermark;
            _highWatermark = highWatermark;
            _mruList = new LinkedList<TKey>();
            if (comparer == null)
            {
                _items = new Dictionary<TKey, CacheEntry>();
            }
            else
            {
                _items = new Dictionary<TKey, CacheEntry>(comparer);
            }
        }

        public int Count
        {
            get
            {
                ThrowIfDisposed();
                return _items.Count;
            }
        }

        public bool IsDisposed => _disposed;

        public void Add(TKey key, TValue value)
        {
            Debug.Assert(null != key, "");
            ThrowIfDisposed();

            // if anything goes wrong (duplicate entry, etc) we should 
            // clear our caches so that we don't get out of sync
            bool success = false;
            try
            {
                if (_items.Count == _highWatermark)
                {
                    // If the cache is full, purge enough LRU items to shrink the 
                    // cache down to the low watermark
                    int countToPurge = _highWatermark - _lowWatermark;
                    for (int i = 0; i < countToPurge; i++)
                    {
                        TKey keyRemove = _mruList.Last.Value;
                        _mruList.RemoveLast();
                        TValue item = _items[keyRemove].value;
                        _ = _items.Remove(keyRemove);
                        OnSingleItemRemoved(item);
                        OnItemAgedOutOfCache(item);
                    }
                }
                // Add  the new entry to the cache and make it the MRU element
                CacheEntry entry;
                entry.node = _mruList.AddFirst(key);
                entry.value = value;
                _items.Add(key, entry);
                _mruEntry = entry;
                success = true;
            }
            finally
            {
                if (!success)
                {
                    this.Clear();
                }
            }
        }

        public void Clear()
        {
            ThrowIfDisposed();
            Clear(false);
        }

        private void Clear(bool dispose)
        {
            _mruList.Clear();
            if (dispose)
            {
                foreach (CacheEntry cacheEntry in _items.Values)
                {
                    if (cacheEntry.value is IDisposable item)
                    {
                        try
                        {
                            item.Dispose();
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                        }
                    }
                }
            }

            _items.Clear();
            _mruEntry.value = null;
            _mruEntry.node = null;
        }

        public bool Remove(TKey key)
        {
            Debug.Assert(null != key, "");
            ThrowIfDisposed();

            if (_items.TryGetValue(key, out CacheEntry entry))
            {
                _ = _items.Remove(key);
                OnSingleItemRemoved(entry.value);
                _mruList.Remove(entry.node);
                if (object.ReferenceEquals(_mruEntry.node, entry.node))
                {
                    _mruEntry.value = null;
                    _mruEntry.node = null;
                }
                return true;
            }

            return false;
        }

        protected virtual void OnSingleItemRemoved(TValue item)
        {
            ThrowIfDisposed();
        }

        protected virtual void OnItemAgedOutOfCache(TValue item)
        {
            ThrowIfDisposed();
        }

        //
        // If found, make the entry most recently used
        //
        public bool TryGetValue(TKey key, out TValue value)
        {
            // first check our MRU item
            if (_mruEntry.node != null && key != null && key.Equals(_mruEntry.node.Value))
            {
                value = _mruEntry.value;
                return true;
            }

            bool found = _items.TryGetValue(key, out CacheEntry entry);
            value = entry.value;

            // Move the node to the head of the MRU list if it's not already there
            if (found && _mruList.Count > 1
                && !object.ReferenceEquals(_mruList.First, entry.node))
            {
                _mruList.Remove(entry.node);
                _mruList.AddFirst(entry.node);
                _mruEntry = entry;
            }

            return found;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsDisposed)
                {
                    _disposed = true;
                    Clear(true);
                }
            }
        }

        [MethodImpl(InlineMethod.AggressiveInlining)]
        private void ThrowIfDisposed()
        {
            if (IsDisposed) { ThrowObjectDisposedException(); }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowObjectDisposedException()
        {
            throw GetObjectDisposedException();
            ObjectDisposedException GetObjectDisposedException()
            {
                return new ObjectDisposedException(this.GetType().FullName);
            }
        }

        private struct CacheEntry
        {
            internal TValue value;
            internal LinkedListNode<TKey> node;
        }
    }
}
