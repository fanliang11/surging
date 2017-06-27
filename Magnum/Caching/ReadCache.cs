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
    using System.Collections.Generic;


    /// <summary>
    /// A read-only view of a cache. Methods that are able to modify the cache contents are not
    /// available in this reduced interface. Methods on this interface will NOT invoke a missing
    /// item provider.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface ReadCache<TKey, TValue> :
        IEnumerable<TValue>
    {
        /// <summary>
        /// The number of items in the cache
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Checks if the key exists in the cache
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key exists, otherwise false</returns>
        bool Has(TKey key);

        /// <summary>
        /// Checks if a value exists in the cache
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value exists, otherwise false</returns>
        bool HasValue(TValue value);

        /// <summary>
        /// Calls the specified callback with each value in the cache
        /// </summary>
        /// <param name="callback">A callback that accepts the value for each item in the cache</param>
        void Each(Action<TValue> callback);

        /// <summary>
        /// Calls the specified callback with each item in the cache
        /// </summary>
        /// <param name="callback">A callback that accepts the key and value for each item in the cache</param>
        void Each(Action<TKey, TValue> callback);

        /// <summary>
        /// Uses a predicate to scan the cache for a matching value
        /// </summary>
        /// <param name="predicate">The predicate to run against each value</param>
        /// <returns>True if a matching value exists, otherwise false</returns>
        bool Exists(Predicate<TValue> predicate);

        /// <summary>
        /// Uses a predicate to scan the cache for a matching value
        /// </summary>
        /// <param name="predicate">The predicate to run against each value</param>
        /// <param name="result">The matching value</param>
        /// <returns>True if a matching value was found, otherwise false</returns>
        bool Find(Predicate<TValue> predicate, out TValue result);

        /// <summary>
        /// Gets all keys that are stored in the cache
        /// </summary>
        /// <returns>An array of every key in the dictionary</returns>
        TKey[] GetAllKeys();

        /// <summary>
        /// Gets all values that are stored in the cache
        /// </summary>
        /// <returns>An array of every value in the dictionary</returns>
        TValue[] GetAll();
    }
}