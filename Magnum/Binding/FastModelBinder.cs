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
	/// A fast model binder for quickly applying values to properties
	/// </summary>
	public class FastModelBinder :
		ModelBinder
	{
		public object Bind(Type type, ModelBinderContext context)
		{
			return new InstanceBinderContext(context).Bind(type);
		}

		public T Bind<T>(ModelBinderContext context)
		{
			return (T) Bind(typeof (T), context);
		}
	}
}