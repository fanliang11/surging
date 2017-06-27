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

	public static class ExtensionsToDisposable
	{
		/// <summary>
		/// Wraps an object that implements IDisposable in an enumeration to make it safe for use in LINQ expressions
		/// </summary>
		/// <typeparam name="T">The type of the object, which must implement IDisposable</typeparam>
		/// <param name="target">The target to wrap</param>
		/// <returns>An enumeration with a single entry equal to the target</returns>
		public static IEnumerable<T> AutoDispose<T>(this T target)
			where T : IDisposable
		{
			try
			{
				yield return target;
			}
			finally
			{
				target.Dispose();
			}
		}
	}
}