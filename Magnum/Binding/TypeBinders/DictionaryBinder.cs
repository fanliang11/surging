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
	using System;
	using System.Collections.Generic;
	using System.Xml;

//	public class DictionaryBinder<TKey, TValue> :
//		ObjectBinder<IDictionary<TKey, TValue>>
//	{
//		public object Bind(BinderContext context)
//		{
//			Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
//
//			if (DictionaryIsEmpty(context))
//				return dictionary;
//
//			MoveToFirstElement(context);
//
//			while (context.NodeType != XmlNodeType.EndElement)
//			{
//				ReadItem(context, dictionary);
//			}
//
//			MovePastEndElement(context);
//
//			return dictionary;
//		}
//
//		private void MovePastEndElement(BinderContext context)
//		{
//			context.Read();
//		}
//
//		private bool DictionaryIsEmpty(BinderContext context)
//		{
//			if (context.IsEmptyElement)
//			{
//				context.Read();
//				return true;
//			}
//
//			return false;
//		}
//
//		private void MoveToFirstElement(BinderContext context)
//		{
//			context.Read();
//		}
//
//		private void ReadItem(BinderContext context, IDictionary<TKey, TValue> dictionary)
//		{
//			if (context.NodeType != XmlNodeType.Element || context.LocalName != "item")
//				throw new InvalidOperationException("Dictionary is not at an item element");
//
//			context.Read();
//
//			object key = context.Bind(context.Namespace);
//
//			object element = context.Bind(context.Namespace);
//
//			if (context.NodeType != XmlNodeType.EndElement || context.LocalName != "item")
//				throw new InvalidOperationException("Dictionary is not at the end of an item");
//
//			context.Read();
//
//			if (key != null)
//			{
//				dictionary.Add((TKey) key, (TValue) element);
//			}
//		}
//	}
}