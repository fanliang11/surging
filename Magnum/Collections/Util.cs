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
namespace Magnum.Collections
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// A holder class for various internal utility functions that need to be shared.
	/// </summary>
	internal static class Util
	{
		/// <summary>
		/// Wrap an enumerable so that clients can't get to the underlying 
		/// implementation via a down-cast.
		/// </summary>
		[Serializable]
		private class WrapEnumerable<T> : IEnumerable<T>
		{
			private IEnumerable<T> wrapped;

			/// <summary>
			/// Create the wrapper around an enumerable.
			/// </summary>
			/// <param name="wrapped">IEnumerable to wrap.</param>
			public WrapEnumerable(IEnumerable<T> wrapped)
			{
				this.wrapped = wrapped;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return wrapped.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable) wrapped).GetEnumerator();
			}
		}

		/// <summary>
		/// Determine if a type is cloneable: either a value type or implementing
		/// ICloneable.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="isValue">Returns if the type is a value type, and does not implement ICloneable.</param>
		/// <returns>True if the type is cloneable.</returns>
		public static bool IsCloneableType(Type type, out bool isValue)
		{
			isValue = false;

			if (typeof (ICloneable).IsAssignableFrom(type))
			{
				return true;
			}
			else if (type.IsValueType)
			{
				isValue = true;
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Returns the simple name of the class, for use in exception messages. 
		/// </summary>
		/// <returns>The simple name of this class.</returns>
		public static string SimpleClassName(Type type)
		{
			string name = type.Name;

			// Just use the simple name.
			int index = name.IndexOfAny(new[] {'<', '{', '`'});
			if (index >= 0)
				name = name.Substring(0, index);

			return name;
		}

		/// <summary>
		/// Wrap an enumerable so that clients can't get to the underlying
		/// implementation via a down-case
		/// </summary>
		/// <param name="wrapped">Enumerable to wrap.</param>
		/// <returns>A wrapper around the enumerable.</returns>
		public static IEnumerable<T> CreateEnumerableWrapper<T>(IEnumerable<T> wrapped)
		{
			return new WrapEnumerable<T>(wrapped);
		}

		/// <summary>
		/// Gets the hash code for an object using a comparer. Correctly handles
		/// null.
		/// </summary>
		/// <param name="item">Item to get hash code for. Can be null.</param>
		/// <param name="equalityComparer">The comparer to use.</param>
		/// <returns>The hash code for the item.</returns>
		public static int GetHashCode<T>(T item, IEqualityComparer<T> equalityComparer)
		{
			if (item == null)
				return 0x1786E23C;
			else
				return equalityComparer.GetHashCode(item);
		}
	}
}