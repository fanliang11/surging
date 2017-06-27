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

	public static class ExtensionsToInterfaces
	{
		/// <summary>
		///   Checks if an object implements the specified interface
		/// </summary>
		/// <typeparam name = "T">The interface type</typeparam>
		/// <param name = "obj">The object to check</param>
		/// <returns>True if the interface is implemented by the object, otherwise false</returns>
		public static bool Implements<T>(this object obj)
		{
			return obj.Implements(typeof (T));
		}

		/// <summary>
		///   Checks if an object implements the specified interface
		/// </summary>
		/// <param name = "obj">The object to check</param>
		/// <param name = "interfaceType">The interface type (can be generic, either specific or open)</param>
		/// <returns>True if the interface is implemented by the object, otherwise false</returns>
		public static bool Implements(this object obj, Type interfaceType)
		{
			Guard.AgainstNull(obj, "obj");

			Type objectType = obj.GetType();

			return objectType.Implements(interfaceType);
		}

		/// <summary>
		///   Checks if a type implements the specified interface
		/// </summary>
		/// <typeparam name = "T">The interface type (can be generic, either specific or open)</typeparam>
		/// <param name = "objectType">The type to check</param>
		/// <returns>True if the interface is implemented by the type, otherwise false</returns>
		public static bool Implements<T>(this Type objectType)
		{
			return objectType.Implements(typeof (T));
		}

		/// <summary>
		///   Checks if a type implements the specified interface
		/// </summary>
		/// <param name = "objectType">The type to check</param>
		/// <param name = "interfaceType">The interface type (can be generic, either specific or open)</param>
		/// <returns>True if the interface is implemented by the type, otherwise false</returns>
		public static bool Implements(this Type objectType, Type interfaceType)
		{
			Guard.AgainstNull(objectType, "objectType");
			Guard.AgainstNull(interfaceType, "interfaceType");
//			Guard.IsTrue(x => x.IsInterface, interfaceType, "interfaceType", "Must be an interface");

			if (interfaceType.IsGenericTypeDefinition)
				return objectType.ImplementsGeneric(interfaceType);

			return interfaceType.IsAssignableFrom(objectType);
		}

		/// <summary>
		///   Checks if a type implements an open generic at any level of the inheritance chain, including all
		///   base classes
		/// </summary>
		/// <param name = "objectType">The type to check</param>
		/// <param name = "interfaceType">The interface type (must be a generic type definition)</param>
		/// <returns>True if the interface is implemented by the type, otherwise false</returns>
		public static bool ImplementsGeneric(this Type objectType, Type interfaceType)
		{
			Type matchedType;
			return objectType.ImplementsGeneric(interfaceType, out matchedType);
		}

		/// <summary>
		///   Checks if a type implements an open generic at any level of the inheritance chain, including all
		///   base classes
		/// </summary>
		/// <param name = "objectType">The type to check</param>
		/// <param name = "interfaceType">The interface type (must be a generic type definition)</param>
		/// <param name = "matchedType">The matching type that was found for the interface type</param>
		/// <returns>True if the interface is implemented by the type, otherwise false</returns>
		public static bool ImplementsGeneric(this Type objectType, Type interfaceType, out Type matchedType)
		{
			Guard.AgainstNull(objectType);
			Guard.AgainstNull(interfaceType);
			Guard.IsTrue(x => x.IsGenericType, interfaceType, "interfaceType", "Must be a generic type");
			Guard.IsTrue(x => x.IsGenericTypeDefinition, interfaceType, "interfaceType", "Must be a generic type definition");

			matchedType = null;

			if (interfaceType.IsInterface)
			{
				matchedType = objectType.GetInterfaces()
					.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType)
					.FirstOrDefault();
				if (matchedType != null)
					return true;
			}

			if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == interfaceType)
			{
				matchedType = objectType;
				return true;
			}

			Type baseType = objectType.BaseType;
			if (baseType == null)
				return false;

			return baseType.ImplementsGeneric(interfaceType, out matchedType);
		}
	}
}