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
namespace Magnum.Context
{
	using System.Collections;

	public class TypedKey<T>
	{
		public bool Equals(TypedKey<T> obj)
		{
			return !ReferenceEquals(null, obj);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (TypedKey<T>)) return false;
			return Equals((TypedKey<T>) obj);
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}

		public static string UniqueKey = typeof (TypedKey<T>).FullName;
	}

	public static class ExtensionsForTypedKey
	{
		public static void Store<T>(this IDictionary items, T value)
		{
			items[TypedKey<T>.UniqueKey] = value;
		}

		public static void Remove<T>(this IDictionary items)
		{
			if(items.Exists<T>())
				items.Remove(TypedKey<T>.UniqueKey);
		}

		public static bool Exists<T>(this IDictionary items)
		{
			return items.Contains(TypedKey<T>.UniqueKey);
		}

		public static T Retrieve<T>(this IDictionary items)
		{
			return (T)items[TypedKey<T>.UniqueKey];
		}
	}
}