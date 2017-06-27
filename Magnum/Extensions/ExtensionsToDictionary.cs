// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Magnum.Extensions
{
	using System;
	using System.Collections.Generic;

	public static class ExtensionsToDictionary
	{
		/// <summary>
		/// Gets the value for the specified key or adds a new value to the dictionary
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static TValue Retrieve<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
			where TValue : new()
		{
			TValue ret;
			if (!dictionary.TryGetValue(key, out ret))
			{
				ret = new TValue();
				dictionary[key] = ret;
			}
			return ret;
		}

		/// <summary>
		/// Gets the value for the specified key or adds a new value to the dictionary
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="key"></param>
		/// <param name="missingValue">The value to add if the key is not found</param>
		/// <returns></returns>
		public static TValue Retrieve<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue missingValue)
		{
			TValue ret;
			if (!dictionary.TryGetValue(key, out ret))
			{
				ret = missingValue;
				dictionary[key] = ret;
			}
			return ret;
		}

		/// <summary>
		/// Gets the value for the specified key or adds a new value to the dictionary
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="key"></param>
		/// <param name="valueProvider">The function to return the value to add if the key is not found</param>
		/// <returns></returns>
		public static TValue Retrieve<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueProvider)
		{
			TValue ret;
			if (!dictionary.TryGetValue(key, out ret))
			{
				ret = valueProvider();
				dictionary[key] = ret;
			}
			return ret;
		}

		/// <summary>
		/// Converts an object to the strongly typed version of a dictionary
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static TResult StrongGet<TResult>(this IDictionary<string, object> dictionary, string key)
		{
			object result;
			if (dictionary.TryGetValue(key, out result))
			{
				return (TResult) result;
			}

			return default(TResult);
		}
	}
}