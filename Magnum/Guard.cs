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
namespace Magnum
{
	using System;
	using Extensions;

	public static class Guard
	{
		public static void AgainstNull<T>(T value)
			where T : class
		{
			if (value == null)
				throw new ArgumentNullException();
		}

		public static void AgainstNull<T>(T value, string paramName)
			where T : class
		{
			if (value == null)
				throw new ArgumentNullException(paramName);
		}

		public static void AgainstNull<T>(T value, string paramName, string message)
			where T : class
		{
			if (value == null)
				throw new ArgumentNullException(paramName, message);
		}

		public static void AgainstNull<T>(T? value)
			where T : struct
		{
			if (!value.HasValue)
				throw new ArgumentNullException();
		}

		public static void AgainstNull<T>(T? value, string paramName)
			where T : struct
		{
			if (!value.HasValue)
				throw new ArgumentNullException(paramName);
		}

		public static void AgainstNull<T>(T? value, string paramName, string message)
			where T : struct
		{
			if (!value.HasValue)
				throw new ArgumentNullException(paramName, message);
		}

		public static void AgainstNull(string value)
		{
			if (value.IsNull())
				throw new ArgumentNullException();
		}

		public static void AgainstNull(string value, string paramName)
		{
			if (value.IsNull())
				throw new ArgumentNullException(paramName);
		}

		public static void AgainstNull(string value, string paramName, string message)
		{
			if (value.IsNull())
				throw new ArgumentNullException(paramName, message);
		}

		public static void AgainstEmpty(string value)
		{
			if (value.IsEmpty())
				throw new ArgumentException("string value must not be empty");
		}

		public static void AgainstEmpty(string value, string paramName)
		{
			if (value.IsEmpty())
				throw new ArgumentException("string value must not be empty", paramName);
		}

		public static void AgainstEmpty(string value, string paramName, string message)
		{
			if (value.IsEmpty())
				throw new ArgumentException(message, paramName);
		}

		public static void GreaterThan<T>(T lowerLimit, T value)
			where T : IComparable<T>
		{
			if (value.CompareTo(lowerLimit) <= 0)
				throw new ArgumentOutOfRangeException();
		}

		public static void GreaterThan<T>(T lowerLimit, T value, string paramName)
			where T : IComparable<T>
		{
			if (value.CompareTo(lowerLimit) <= 0)
				throw new ArgumentOutOfRangeException(paramName);
		}

		public static void GreaterThan<T>(T lowerLimit, T value, string paramName, string message)
			where T : IComparable<T>
		{
			if (value.CompareTo(lowerLimit) <= 0)
				throw new ArgumentOutOfRangeException(paramName, message);
		}


		public static void LessThan<T>(T upperLimit, T value)
			where T : IComparable<T>
		{
			if (value.CompareTo(upperLimit) >= 0)
				throw new ArgumentOutOfRangeException();
		}

		public static void LessThan<T>(T upperLimit, T value, string paramName)
			where T : IComparable<T>
		{
			if (value.CompareTo(upperLimit) >= 0)
				throw new ArgumentOutOfRangeException(paramName);
		}

		public static void LessThan<T>(T upperLimit, T value, string paramName, string message)
			where T : IComparable<T>
		{
			if (value.CompareTo(upperLimit) >= 0)
				throw new ArgumentOutOfRangeException(paramName, message);
		}

		public static void IsTrue<T>(Func<T, bool> condition, T target)
		{
			if (!condition(target))
				throw new ArgumentException("condition was not true");
		}

		public static void IsTrue<T>(Func<T, bool> condition, T target, string paramName)
		{
			if (!condition(target))
				throw new ArgumentException("condition was not true", paramName);
		}

		public static void IsTrue<T>(Func<T, bool> condition, T target, string paramName, string message)
		{
			if (!condition(target))
				throw new ArgumentException(message, paramName);
		}


		public static T IsTypeOf<T>(object obj)
		{
			AgainstNull(obj);

			if(obj is T)
				return (T) obj;

			throw new ArgumentException("{0} is not an instance of type {1}".FormatWith(obj.GetType().Name, typeof (T).Name));
		}

	}
}