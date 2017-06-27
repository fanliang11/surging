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
namespace Magnum.Collections
{
	using System;
	using System.Collections;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;


	[Serializable]
	public class Cache<TKey, TValue> :
		IEnumerable<TValue>
	{
		readonly Action<TKey, TValue> _duplicateValueAddedCallback = DefaultDuplicateValueAddedCallback;
		readonly ConcurrentDictionary<TKey, TValue> _values;
		Func<TValue, TKey> _keyConverter = DefaultKeyConverter;
		Func<TKey, TValue> _missingValueProvider = ThrowOnMissingValue;
		Action<TKey, TValue> _valueAddedCallback = DefaultValueAddedAction;

		public Cache()
		{
			_values = new ConcurrentDictionary<TKey, TValue>();
		}

		public Cache(IEqualityComparer<TKey> comparer)
		{
			_values = new ConcurrentDictionary<TKey, TValue>(comparer);
		}

		public Cache(IDictionary<TKey, TValue> dictionary)
		{
			_values = new ConcurrentDictionary<TKey, TValue>(dictionary);
		}

		public Cache(Func<TKey, TValue> missingValueProvider)
		{
			_values = new ConcurrentDictionary<TKey, TValue>();
			_missingValueProvider = missingValueProvider;
		}

		public Cache(Func<TKey, TValue> missingValueProvider, IEqualityComparer<TKey> comparer)
		{
			_values = new ConcurrentDictionary<TKey, TValue>(comparer);
			_missingValueProvider = missingValueProvider;
		}

		public Cache(IDictionary<TKey, TValue> dictionary, Func<TKey, TValue> missingValueProvider)
		{
			_values = new ConcurrentDictionary<TKey, TValue>(dictionary);
			_missingValueProvider = missingValueProvider;
		}

		public Func<TKey, TValue> MissingValueProvider
		{
			set { _missingValueProvider = value; }
		}

		public Action<TKey, TValue> ValueAddedCallback
		{
			set { _valueAddedCallback = value; }
		}

		public Func<TValue, TKey> KeyConverter
		{
			get { return _keyConverter; }
			set { _keyConverter = value; }
		}

		public TValue this[TKey key]
		{
			get
			{
				return Retrieve(key);
			}
			set
			{
				bool added = false;
				_values.AddOrUpdate(key, (add) =>
					{
						added = true;
						return value;
					}, (update, current) => value);

				if(added)
					_valueAddedCallback(key, value);
			}
		}

		public TValue First
		{
			get
			{
				return _values.Values.FirstOrDefault();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<TValue>)this).GetEnumerator();
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			return _values.Values.GetEnumerator();
		}

		public void Add(TKey key, TValue value)
		{
			bool added = false;
			_values.AddOrUpdate(key, (add) =>
			{
				added = true;
				return value;
			}, (update, current) =>
				{
					_duplicateValueAddedCallback(update, current);
					return value;
				});

			if (added)
				_valueAddedCallback(key, value);
		}

		public void Fill(IEnumerable<TValue> values)
		{
			values.Each(value =>
				{
					TKey key = _keyConverter(value);
					Add(key, value);
				});
		}

		public TValue Retrieve(TKey key)
		{
			return Retrieve(key, _missingValueProvider);
		}

		public TValue Retrieve(TKey key, Func<TKey, TValue> missingValueProvider)
		{
			bool added = false;
			TValue value = _values.GetOrAdd(key, x =>
			{
				added = true;
				return missingValueProvider(x);
			});

			if (added)
				_valueAddedCallback(key, value);

			return value;
		}

		public void Each(Action<TValue> action)
		{
			foreach (var pair in _values)
				action(pair.Value);
		}

		public void Each(Action<TKey, TValue> action)
		{
			foreach (var pair in _values)
				action(pair.Key, pair.Value);
		}

		public bool Has(TKey key)
		{
			return _values.ContainsKey(key);
		}

		public bool Exists(Predicate<TValue> predicate)
		{
			return _values.Values.Any(x => predicate(x));
		}

		public TValue Find(Predicate<TValue> predicate)
		{
			return _values.Values.FirstOrDefault(x => predicate(x));
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
			TValue existing;
			_values.TryRemove(key, out existing);
		}

		public void ClearAll()
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

		static TValue ThrowOnMissingValue(TKey key)
		{
			throw new KeyNotFoundException("The specified element was not found: " + key);
		}

		static void DefaultValueAddedAction(TKey key, TValue value)
		{
		}

		static void DefaultDuplicateValueAddedCallback(TKey key, TValue value)
		{
			throw new InvalidOperationException("Duplicate value added for key: " + key);
		}

		static TKey DefaultKeyConverter(TValue value)
		{
			throw new InvalidOperationException("No default key converter has been specified");
		}
	}
}