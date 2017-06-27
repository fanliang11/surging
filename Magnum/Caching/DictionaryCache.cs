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
    using System.Linq;


    [Serializable]
    public class DictionaryCache<TKey, TValue> :
        Cache<TKey, TValue>
    {
        readonly IDictionary<TKey, TValue> _values;

        KeySelector<TKey, TValue> _keySelector = DefaultKeyAccessor;
        MissingValueProvider<TKey, TValue> _missingValueProvider = ThrowOnMissingValue;
        CacheItemCallback<TKey, TValue> _valueAddedCallback = DefaultCacheItemCallback;
        CacheItemCallback<TKey, TValue> _valueRemovedCallback = DefaultCacheItemCallback;
        CacheItemCallback<TKey, TValue> _duplicateValueAdded;

        public DictionaryCache()
        {
            _values = new Dictionary<TKey, TValue>();
        }

        public DictionaryCache(MissingValueProvider<TKey, TValue> missingValueProvider)
            : this()
        {
            _missingValueProvider = missingValueProvider;
        }

        public DictionaryCache(IEqualityComparer<TKey> equalityComparer)
        {
            _values = new Dictionary<TKey, TValue>(equalityComparer);
        }

        public DictionaryCache(KeySelector<TKey, TValue> keySelector)
        {
            _values = new Dictionary<TKey, TValue>();
            _keySelector = keySelector;
        }

        public DictionaryCache(KeySelector<TKey, TValue> keySelector, IEnumerable<TValue> values)
            : this(keySelector)
        {
            Fill(values);
        }

        public DictionaryCache(IEqualityComparer<TKey> equalityComparer,
                               MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(equalityComparer)
        {
            _missingValueProvider = missingValueProvider;
        }

        public DictionaryCache(IDictionary<TKey, TValue> values, bool copy = true)
        {
            _values = copy ? new Dictionary<TKey, TValue>(values) : values;
        }

        public DictionaryCache(IDictionary<TKey, TValue> values,
                               MissingValueProvider<TKey, TValue> missingValueProvider,
                               bool copy = true)
            : this(values, copy)
        {
            _missingValueProvider = missingValueProvider;
        }

        public DictionaryCache(IDictionary<TKey, TValue> values, IEqualityComparer<TKey> equalityComparer)
        {
            _values = new Dictionary<TKey, TValue>(values, equalityComparer);
        }

        public DictionaryCache(IDictionary<TKey, TValue> values,
                               IEqualityComparer<TKey> equalityComparer,
                               MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(values, equalityComparer)
        {
            _missingValueProvider = missingValueProvider;
        }

        public MissingValueProvider<TKey, TValue> MissingValueProvider
        {
            set { _missingValueProvider = value ?? ThrowOnMissingValue; }
        }

        public CacheItemCallback<TKey, TValue> ValueAddedCallback
        {
            set { _valueAddedCallback = value ?? DefaultCacheItemCallback; }
        }

        public CacheItemCallback<TKey, TValue> ValueRemovedCallback
        {
            set { _valueRemovedCallback = value ?? DefaultCacheItemCallback; }
        }

        public CacheItemCallback<TKey, TValue> DuplicateValueAdded
        {
            set { _duplicateValueAdded = value ?? ThrowOnDuplicateValue; }
        }

        public KeySelector<TKey, TValue> KeySelector
        {
            set { _keySelector = value ?? DefaultKeyAccessor; }
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public TValue this[TKey key]
        {
            get { return Get(key); }
            set
            {
                TValue existingValue;
                if (_values.TryGetValue(key, out existingValue))
                {
                    _valueRemovedCallback(key, existingValue);
                    _values[key] = value;
                    _valueAddedCallback(key, value);
                }
                else
                    Add(key, value);
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        public TValue Get(TKey key)
        {
            return Get(key, _missingValueProvider);
        }

        public TValue Get(TKey key, MissingValueProvider<TKey, TValue> missingValueProvider)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return value;

            value = missingValueProvider(key);

            Add(key, value);

            return value;
        }

        public bool Has(TKey key)
        {
            return _values.ContainsKey(key);
        }

        public bool HasValue(TValue value)
        {
            TKey key = _keySelector(value);

            return Has(key);
        }

        public void Add(TKey key, TValue value)
        {
            _values.Add(key, value);
            _valueAddedCallback(key, value);
        }

        public void AddValue(TValue value)
        {
            TKey key = _keySelector(value);
            Add(key, value);
        }

        public void Fill(IEnumerable<TValue> values)
        {
            foreach (TValue value in values)
            {
                TKey key = _keySelector(value);
                Add(key, value);
            }
        }

        public void Each(Action<TValue> callback)
        {
            foreach (var value in _values)
                callback(value.Value);
        }

        public void Each(Action<TKey, TValue> callback)
        {
            foreach (var value in _values)
                callback(value.Key, value.Value);
        }

        public bool Exists(Predicate<TValue> predicate)
        {
            return _values.Any(value => predicate(value.Value));
        }

        public bool Find(Predicate<TValue> predicate, out TValue result)
        {
            foreach (var value in _values.Where(value => predicate(value.Value)))
            {
                result = value.Value;
                return true;
            }

            result = default(TValue);
            return false;
        }

        public TKey[] GetAllKeys()
        {
            return _values.Keys.ToArray();
        }

        public TValue[] GetAll()
        {
            return _values.Values.ToArray();
        }

        public void Remove(TKey key)
        {
            TValue existingValue;
            if (_values.TryGetValue(key, out existingValue))
            {
                _valueRemovedCallback(key, existingValue);
                _values.Remove(key);
            }
        }

        public void RemoveValue(TValue value)
        {
            TKey key = _keySelector(value);

            Remove(key);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool WithValue(TKey key, Action<TValue> callback)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
            {
                callback(value);
                return true;
            }

            return false;
        }

        public TResult WithValue<TResult>(TKey key,
                                          Func<TValue, TResult> callback,
                                          TResult defaultValue = default(TResult))
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return callback(value);

            return defaultValue;
        }

        static TValue ThrowOnMissingValue(TKey key)
        {
            throw new KeyNotFoundException("The specified element was not found: " + key);
        }

        static void ThrowOnDuplicateValue(TKey key, TValue value)
        {
            throw new ArgumentException(string.Format("An item with the same key already exists in the cache: {0}", key), "key");
        }

        static void DefaultCacheItemCallback(TKey key, TValue value)
        {
        }

        static TKey DefaultKeyAccessor(TValue value)
        {
            throw new InvalidOperationException("No default key accessor has been specified");
        }
    }
}