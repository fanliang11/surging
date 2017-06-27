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
	using System.Collections;
	using System.Linq;
	using System.Reflection;


	public static class ExtensionsToObject
	{
		const int RecursionLimit = 10;

		public static string Stringify(this object value)
		{
			try
			{
				return StringifyInternal(value, RecursionLimit);
			}
			catch (InvalidOperationException ex)
			{
				return value.ToString();
			}
		}

		static string StringifyInternal(object value, int recursionLevel)
		{
			if (value == null)
				return "null";

			if (recursionLevel < 0)
				throw new InvalidOperationException();

			if (value is string || value is char)
				return "\"" + value + "\"";

			var collection = value as IEnumerable;
			if (collection != null)
				return StringifyCollection(collection, recursionLevel);

			if (value.GetType().IsValueType)
				return value.ToString();

			return StringifyObject(value, recursionLevel);
		}


		static string StringifyCollection(IEnumerable collection, int recursionLevel)
		{
			string[] elements = collection.Cast<object>()
				.Select(x => StringifyInternal(x, recursionLevel - 1))
				.ToArray();

			return "[" + String.Join(", ", elements) + "]";
		}

		static string StringifyObject(object value, int recursionLevel)
		{
			string[] elements = value
				.GetType()
				.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Select(x => "{0} = {1}".FormatWith(x.Name, StringifyInternal(x.GetValue(value, null), recursionLevel - 1)))
				.ToArray();

			return "{" + String.Join(", ", elements) + "}";
		}

		public static T CastAs<T>(this object input)
			where T : class
		{
			if (input == null)
				throw new ArgumentNullException("input");

			var result = input as T;
			if (result == null)
				throw new InvalidOperationException("Unable to convert from " + input.GetType().FullName + " to "
				                                    + typeof(T).FullName);

			return result;
		}

		/// <summary>
		/// Returns the value of the instance member, or the default value if the instance is null
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="instance"></param>
		/// <param name="accessor"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static TValue ValueOrDefault<T, TValue>(this T instance, Func<T, TValue> accessor, TValue defaultValue)
			where T : class
		{
			if(null == instance)
				return defaultValue;

			return accessor(instance);
		}
	}
}