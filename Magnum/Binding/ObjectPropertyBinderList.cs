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
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Reflection;

	public class ObjectPropertyBinderList<T> :
		IEnumerable<ObjectPropertyBinder<T>>
	{
		private readonly IList<ObjectPropertyBinder<T>> _properties;

		public ObjectPropertyBinderList()
		{
			IEnumerable<ObjectPropertyBinder<T>> properties = typeof (T).GetAllProperties()
				.Where(x => x.GetGetMethod() != null)
				.Where(x => x.GetSetMethod(true) != null)
				.Select(x => new ObjectPropertyBinder<T>(x));

			_properties = new List<ObjectPropertyBinder<T>>(properties);
		}

		public IEnumerator<ObjectPropertyBinder<T>> GetEnumerator()
		{
			return _properties.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}