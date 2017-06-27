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
namespace Magnum.Binding
{
	using System.Reflection;


	/// <summary>
	///   Used by the type binders to obtain information
	/// </summary>
	public interface BinderContext
	{
		/// <summary>
		///   The current property being bound
		/// </summary>
		PropertyInfo Property { get; }

		/// <summary>
		///   Resolves the value of the property based on the naming convention.
		///   This can be an expensive call, so it should only be called once and
		///   the return value should be cached to avoid performance concerns
		/// </summary>
		object PropertyValue { get; }

		/// <summary>
		///   Binds a property of an object, pushing the property context on the stack
		/// </summary>
		/// <param name = "property"></param>
		/// <returns></returns>
		object Bind(ObjectPropertyBinder property);
	}
}