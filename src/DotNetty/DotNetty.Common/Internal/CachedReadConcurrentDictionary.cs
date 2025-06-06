using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotNetty.Common.Internal
{
    /// <summary>A thread-safe dictionary for read-heavy workloads.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public class CachedReadConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>The number of concurrent writes for which to optimize by default.</summary>
        private static Int32 DefaultConcurrencyLevel => DEFAULT_CONCURRENCY_MULTIPLIER * PlatformHelper.ProcessorCount;
        // The default concurrency level is DEFAULT_CONCURRENCY_MULTIPLIER * #CPUs. The higher the
        // DEFAULT_CONCURRENCY_MULTIPLIER, the more concurrent writes can take place without interference
        // and blocking, but also the more expensive operations that require all locks become (e.g. table
        // resizing, ToArray, Count, etc). According to brief benchmarks that we ran, 4 seems like a good
        // compromise.
        private const Int32 DEFAULT_CONCURRENCY_MULTIPLIER = 4;
        // The default capacity, i.e. the initial # of buckets. When choosing this value, we are making
        // a trade-off between the size of a very small dictionary, and the number of resizes when
        // constructing a large dictionary. Also, the capacity should not be divisible by a small prime.
        private const Int32 DEFAULT_CAPACITY = 31;

        /// <summary>The number of cache misses which are tolerated before the cache is regenerated.</summary>
        private const uint CacheMissesBeforeCaching = 10u;
        private readonly ConcurrentDictionary<TKey, TValue> _dictionary;
        private readonly IEqualityComparer<TKey> _comparer;

        /// <summary>Approximate number of reads which did not hit the cache since it was last invalidated.
        /// This is used as a heuristic that the dictionary is not being modified frequently with respect to the read volume.</summary>
        private int _cacheMissReads;

        /// <summary>Cached version of inner concurrent dictionary.</summary>
        private Dictionary<TKey, TValue> _readCache;

        /// <summary>Initializes a new instance of the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> class.</summary>
        public CachedReadConcurrentDictionary() => _dictionary = new ConcurrentDictionary<TKey, TValue>();

        /// <summary>Initializes a new instance of the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> class.</summary>
        /// <param name="capacity">The initial number of elements that the <see cref="ConcurrentDictionary{TKey,TValue}"/> can contain.</param>
        public CachedReadConcurrentDictionary(int capacity)
            => _dictionary = new ConcurrentDictionary<TKey, TValue>(DefaultConcurrencyLevel, capacity);

        /// <summary>Initializes a new instance of the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> class
        /// that contains elements copied from the specified collection and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/>.</summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}"/> implementation to use when comparing keys.</param>
        public CachedReadConcurrentDictionary(IEqualityComparer<TKey> comparer)
        {
            _dictionary = new ConcurrentDictionary<TKey, TValue>(comparer);
            _comparer = comparer;
        }

        /// <summary>Initializes a new instance of the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> class.</summary>
        /// <param name="capacity">The initial number of elements that the <see cref="ConcurrentDictionary{TKey,TValue}"/> can contain.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}"/> implementation to use when comparing keys.</param>
        public CachedReadConcurrentDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _dictionary = new ConcurrentDictionary<TKey, TValue>(DefaultConcurrencyLevel, capacity, comparer);
            _comparer = comparer;
        }

        /// <summary>Initializes a new instance of the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> class
        /// that contains elements copied from the specified collection.</summary>
        /// <param name="collection">The <see cref="T:IEnumerable{KeyValuePair{TKey,TValue}}"/> whose elements are copied to the new instance.</param>
        public CachedReadConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            => _dictionary = new ConcurrentDictionary<TKey, TValue>(collection);

        /// <summary>Initializes a new instance of the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/>
        /// class that contains elements copied from the specified collection and uses the specified
        /// <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/>.</summary>
        /// <param name="collection">The <see cref="T:IEnumerable{KeyValuePair{TKey,TValue}}"/> whose elements are copied to the new instance.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}"/> implementation to use when comparing keys.</param>
        public CachedReadConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        {
            _comparer = comparer;
            _dictionary = new ConcurrentDictionary<TKey, TValue>(collection, comparer);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => GetReadDictionary().GetEnumerator();

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((IDictionary<TKey, TValue>)_dictionary).Add(item);
            InvalidateCache();
        }

        /// <inheritdoc />
        public void Clear()
        {
            _dictionary.Clear();
            InvalidateCache();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item) => GetReadDictionary().Contains(item);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => GetReadDictionary().CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var result = ((IDictionary<TKey, TValue>)_dictionary).Remove(item);
            if (result) InvalidateCache();
            return result;
        }

        /// <inheritdoc />
        public int Count => GetReadDictionary().Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            ((IDictionary<TKey, TValue>)_dictionary).Add(key, value);
            InvalidateCache();
        }

        /// <summary>Adds a key/value pair to the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> if the key does not exist.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (GetReadDictionary().TryGetValue(key, out TValue value)) { return value; }

            value = _dictionary.GetOrAdd(key, valueFactory);
            InvalidateCache();

            return value;
        }

        /// <summary>Adds a key/value pair to the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> if the key does not already exist.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="factoryArgument">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
        {
            if (GetReadDictionary().TryGetValue(key, out TValue value)) { return value; }

            var addedValue = valueFactory(key, factoryArgument);
            if (_dictionary.TryAdd(key, addedValue))
            {
                value = addedValue;
                InvalidateCache();
            }
            else
            {
                _ = _dictionary.TryGetValue(key, out value);
            }

            return value;
        }

        /// <summary>Adds a key/value pair to the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> if the key does not already exist.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="factoryArg1">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg2">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd<TArg1, TArg2>(TKey key, Func<TKey, TArg1, TArg2, TValue> valueFactory, TArg1 factoryArg1, TArg2 factoryArg2)
        {
            if (GetReadDictionary().TryGetValue(key, out TValue value)) { return value; }

            var addedValue = valueFactory(key, factoryArg1, factoryArg2);
            if (_dictionary.TryAdd(key, addedValue))
            {
                value = addedValue;
                InvalidateCache();
            }
            else
            {
                _ = _dictionary.TryGetValue(key, out value);
            }

            return value;
        }

        /// <summary>Adds a key/value pair to the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> if the key does not already exist.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="factoryArg1">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg2">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg3">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd<TArg1, TArg2, TArg3>(TKey key, Func<TKey, TArg1, TArg2, TArg3, TValue> valueFactory,
          TArg1 factoryArg1, TArg2 factoryArg2, TArg3 factoryArg3)
        {
            if (GetReadDictionary().TryGetValue(key, out TValue value)) { return value; }

            var addedValue = valueFactory(key, factoryArg1, factoryArg2, factoryArg3);
            if (_dictionary.TryAdd(key, addedValue))
            {
                value = addedValue;
                InvalidateCache();
            }
            else
            {
                _ = _dictionary.TryGetValue(key, out value);
            }

            return value;
        }

        /// <summary>Adds a key/value pair to the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> if the key does not already exist.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="factoryArg1">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg2">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg3">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg4">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd<TArg1, TArg2, TArg3, TArg4>(TKey key, Func<TKey, TArg1, TArg2, TArg3, TArg4, TValue> valueFactory,
          TArg1 factoryArg1, TArg2 factoryArg2, TArg3 factoryArg3, TArg4 factoryArg4)
        {
            if (GetReadDictionary().TryGetValue(key, out TValue value)) { return value; }

            var addedValue = valueFactory(key, factoryArg1, factoryArg2, factoryArg3, factoryArg4);
            if (_dictionary.TryAdd(key, addedValue))
            {
                value = addedValue;
                InvalidateCache();
            }
            else
            {
                _ = _dictionary.TryGetValue(key, out value);
            }

            return value;
        }

        /// <summary>Adds a key/value pair to the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> if the key does not already exist.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="factoryArg1">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg2">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg3">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg4">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg5">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd<TArg1, TArg2, TArg3, TArg4, TArg5>(TKey key, Func<TKey, TArg1, TArg2, TArg3, TArg4, TArg5, TValue> valueFactory,
          TArg1 factoryArg1, TArg2 factoryArg2, TArg3 factoryArg3, TArg4 factoryArg4, TArg5 factoryArg5)
        {
            if (GetReadDictionary().TryGetValue(key, out TValue value)) { return value; }

            var addedValue = valueFactory(key, factoryArg1, factoryArg2, factoryArg3, factoryArg4, factoryArg5);
            if (_dictionary.TryAdd(key, addedValue))
            {
                value = addedValue;
                InvalidateCache();
            }
            else
            {
                _ = _dictionary.TryGetValue(key, out value);
            }

            return value;
        }

        /// <summary>Adds a key/value pair to the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> if the key does not already exist.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="factoryArg1">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg2">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg3">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg4">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg5">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg6">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TKey key, Func<TKey, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TValue> valueFactory,
          TArg1 factoryArg1, TArg2 factoryArg2, TArg3 factoryArg3, TArg4 factoryArg4, TArg5 factoryArg5, TArg6 factoryArg6)
        {
            if (GetReadDictionary().TryGetValue(key, out TValue value)) { return value; }

            var addedValue = valueFactory(key, factoryArg1, factoryArg2, factoryArg3, factoryArg4, factoryArg5, factoryArg6);
            if (_dictionary.TryAdd(key, addedValue))
            {
                value = addedValue;
                InvalidateCache();
            }
            else
            {
                _ = _dictionary.TryGetValue(key, out value);
            }

            return value;
        }

        /// <summary>Adds a key/value pair to the <see cref="CachedReadConcurrentDictionary{TKey,TValue}"/> if the key does not already exist.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="factoryArg1">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg2">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg3">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg4">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg5">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg6">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <param name="factoryArg7">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public TValue GetOrAdd<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(TKey key, Func<TKey, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TValue> valueFactory,
          TArg1 factoryArg1, TArg2 factoryArg2, TArg3 factoryArg3, TArg4 factoryArg4, TArg5 factoryArg5, TArg6 factoryArg6, TArg7 factoryArg7)
        {
            if (GetReadDictionary().TryGetValue(key, out TValue value)) { return value; }

            var addedValue = valueFactory(key, factoryArg1, factoryArg2, factoryArg3, factoryArg4, factoryArg5, factoryArg6, factoryArg7);
            if (_dictionary.TryAdd(key, addedValue))
            {
                value = addedValue;
                InvalidateCache();
            }
            else
            {
                _ = _dictionary.TryGetValue(key, out value);
            }

            return value;
        }

        /// <summary>Attempts to add the specified key and value.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing
        /// in Visual Basic) for reference types.</param>
        /// <returns>true if the key/value pair was added successfully; otherwise, false.</returns>
        public bool TryAdd(TKey key, TValue value)
        {
            if (_dictionary.TryAdd(key, value))
            {
                InvalidateCache();
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool ContainsKey(TKey key) => GetReadDictionary().ContainsKey(key);

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            //var result = ((IDictionary<TKey, TValue>)_dictionary).Remove(key);
            //if (result) { InvalidateCache(); }
            //return result;
            var result = _dictionary.TryRemove(key, out _);
            if (result) { InvalidateCache(); }
            return result;
        }

        /// <summary>TryRemove</summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRemove(TKey key, out TValue value)
        {
            var result = _dictionary.TryRemove(key, out value);
            if (result) { InvalidateCache(); }
            return result;
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value) => GetReadDictionary().TryGetValue(key, out value);

        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get { return GetReadDictionary()[key]; }
            set
            {
                _dictionary[key] = value;
                InvalidateCache();
            }
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys => GetReadDictionary().Keys;

        /// <inheritdoc />
        public ICollection<TValue> Values => GetReadDictionary().Values;

        [MethodImpl(InlineMethod.AggressiveOptimization)]
        private IDictionary<TKey, TValue> GetReadDictionary() => Volatile.Read(ref _readCache) ?? GetWithoutCache();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IDictionary<TKey, TValue> GetWithoutCache()
        {
            // If the dictionary was recently modified or the cache is being recomputed, return the dictionary directly.
            if ((uint)Interlocked.Increment(ref _cacheMissReads) < CacheMissesBeforeCaching) { return _dictionary; }

            // Recompute the cache if too many cache misses have occurred.
            _ = Interlocked.Exchange(ref _cacheMissReads, 0);
            _ = Interlocked.Exchange(ref _readCache, new Dictionary<TKey, TValue>(_dictionary, _comparer));
            return Volatile.Read(ref _readCache);
        }

        private void InvalidateCache()
        {
            _ = Interlocked.Exchange(ref _cacheMissReads, 0);
            _ = Interlocked.Exchange(ref _readCache, null);
        }
    }
}