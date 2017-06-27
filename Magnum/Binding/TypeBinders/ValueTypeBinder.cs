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
namespace Magnum.Binding.TypeBinders
{
	public abstract class ValueTypeBinder<T> :
		ObjectBinder<T>
	{
		public object Bind(BinderContext context)
		{
			object value = context.PropertyValue;
			if (value == null)
				return null;

			T result;
			if(ConvertType(value, out result))
				return result;

			string text = value.ToString();

			if (ParseType(text, out result))
				return result;

			return UseXmlConvert(text);
		}

		protected virtual bool ConvertType(object value, out T result)
		{
			if (value is T)
			{
				result = (T) value;
				return true;
			}

			result = default(T);
			return false;
		}

		protected abstract bool ParseType(string text, out T result);

		protected abstract T UseXmlConvert(string text);
	}
}