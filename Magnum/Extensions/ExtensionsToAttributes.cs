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
	using System.Linq;
	using System.Reflection;

	public static class ExtensionsToAttributes
	{
		/// <summary>
		/// Returns the first attribute of the specified type for the object specified
		/// </summary>
		/// <typeparam name="T">The type of attribute</typeparam>
		/// <param name="provider">An attribute provider, which can be a MethodInfo, PropertyInfo, Type, etc.</param>
		/// <returns>The attribute instance if found, or null</returns>
		public static T GetAttribute<T>(this ICustomAttributeProvider provider)
			where T : Attribute
		{
			return provider.GetCustomAttributes(typeof (T), true)
				.Cast<T>()
				.FirstOrDefault();
		}

		/// <summary>
		/// Determines if the target has the specified attribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="provider"></param>
		/// <returns></returns>
		public static bool HasAttribute<T>(this ICustomAttributeProvider provider)
			where T : Attribute
		{
			return provider.GetAttribute<T>() != null;
		}

		/// <summary>
		/// Calls the provided action for each instance of the specified attribute type for the object specified
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="provider"></param>
		/// <param name="action"></param>
		public static void ForAttributesOf<T>(this ICustomAttributeProvider provider, Action<T> action)
			where T : Attribute
		{
			provider.GetCustomAttributes(typeof (T), true)
				.Cast<T>()
				.Each(action);
		}
	}
}