// Copyright 2007-2010 The Apache Software Foundation.
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
namespace Magnum.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;


    public abstract class AbstractCacheDecorator<TKey, TValue> :
        Cache<TKey, TValue>
    {
        readonly Cache<TKey, TValue> _cache;

        public AbstractCacheDecorator(Cache<TKey, TValue> cache)
        {
            _cache = cache;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _cache.GetEnumerator();
        }

        public int Count
        {
            get { return _cache.Count; }
        }

        public bool Has(TKey key)
        {
            return _cache.Has(key);
        }

        public bool HasValue(TValue value)
        {
            return _cache.HasValue(value);
        }

        public void Each(Action<TValue> callback)
        {
            _cache.Each(callback);
        }

        public void Each(Action<TKey, TValue> callback)
        {
            _cache.Each(callback);
        }

        public bool Exists(Predicate<TValue> predicate)
        {
            return _cache.Exists(predicate);
        }

        public bool Find(Predicate<TValue> predicate, out TValue result)
        {
            return _cache.Find(predicate, out result);
        }

        public TKey[] GetAllKeys()
        {
            return _cache.GetAllKeys();
        }

        public TValue[] GetAll()
        {
            return _cache.GetAll();
        }

        public MissingValueProvider<TKey, TValue> MissingValueProvider
        {
            set { _cache.MissingValueProvider = value; }
        }

        public CacheItemCallback<TKey, TValue> ValueAddedCallback
        {
            set { _cache.ValueAddedCallback = value; }
        }

        public CacheItemCallback<TKey, TValue> ValueRemovedCallback
        {
            set { _cache.ValueRemovedCallback = value; }
        }

        public CacheItemCallback<TKey, TValue> DuplicateValueAdded
        {
            set { _cache.DuplicateValueAdded = value; }
        }

        public KeySelector<TKey, TValue> KeySelector
        {
            set { _cache.KeySelector = value; }
        }

        public TValue this[TKey key]
        {
            get { return _cache[key]; }
            set { _cache[key] = value; }
        }

        public TValue Get(TKey key)
        {
            return _cache.Get(key);
        }

        public TValue Get(TKey key, MissingValueProvider<TKey, TValue> missingValueProvider)
        {
            return _cache.Get(key, missingValueProvider);
        }

        public void Add(TKey key, TValue value)
        {
            _cache.Add(key, value);
        }

        public void AddValue(TValue value)
        {
            _cache.AddValue(value);
        }

        public void Remove(TKey key)
        {
            _cache.Remove(key);
        }

        public void RemoveValue(TValue value)
        {
            _cache.RemoveValue(value);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void Fill(IEnumerable<TValue> values)
        {
            _cache.Fill(values);
        }

        public bool WithValue(TKey key, Action<TValue> callback)
        {
            return _cache.WithValue(key, callback);
        }

        public TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback,
                                          TResult defaultValue = default(TResult))
        {
            return _cache.WithValue(key, callback, defaultValue);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}