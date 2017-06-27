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
namespace Magnum.Binding
{
	using System;

	/// <summary>
	/// A model binder is used to map an unstructured data source to an object
	/// using a series of conventions and value providers
	/// </summary>
	public interface ModelBinder
	{
		/// <summary>
		/// Create an object of the requested type, initializing the properties
		/// of that object to the value retrieved from the context
		/// </summary>
		/// <param name="type">The type of object to create</param>
		/// <param name="context">The context to use while binding the object</param>
		/// <returns>Returns an instance of the object, or an exception if the binding was unsuccessful</returns>
		object Bind(Type type, ModelBinderContext context);

		/// <summary>
		/// Create an object of the requested type, initializing the properties
		/// of that object to the value retrieved from the context
		/// </summary>
		/// <typeparam name="T">The type of object to create</typeparam>
		/// <param name="context">The context to use while binding the object</param>
		/// <returns>Returns an instance of the object, or an exception if the binding was unsuccessful</returns>
		T Bind<T>(ModelBinderContext context);
	}
}