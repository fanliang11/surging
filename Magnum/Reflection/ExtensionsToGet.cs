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
namespace Magnum.Reflection
{
	public static class ExtensionsToGet
	{
		public static TValue Get<TValue>(this GetValue<TValue> getValue)
			where TValue : class
		{
			TValue value = null;
			getValue(x => value = x);
			return value;
		}

		/// <summary>
		/// Returns the value for the property, or the default for the property type if
		/// the actual property value is unreachable (due to a null reference in the property chain)
		/// </summary>
		/// <typeparam name="T">The object type referenced</typeparam>
		/// <typeparam name="TValue">The property type</typeparam>
		/// <param name="getValue"></param>
		/// <param name="obj">The object from which the value should be retrieved</param>
		/// <returns>The value of the property, or default(TValue) if it cannot be accessed</returns>
		public static TValue Get<T, TValue>(this GetProperty<T, TValue> getValue, T obj)
			where T : class
		{
			TValue value = default(TValue);
			getValue(obj, x => value = x);
			return value;
		}
	}
}