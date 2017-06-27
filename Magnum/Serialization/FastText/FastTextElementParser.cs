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
namespace Magnum.Serialization.FastText
{
	using System;
	using System.Collections.Generic;

	public class FastTextElementParser<TElement> :
		FastTextParser
	{
		public FastTextElementParser(TypeSerializer<TElement> elementTypeSerializer)
		{
			ElementTypeSerializer = elementTypeSerializer;
			ElementWriter = ElementTypeSerializer.GetWriter();
			ElementReader = ElementTypeSerializer.GetReader();

			StringReader = new FastTextStringSerializer().GetReader();
		}

		public TypeReader<string> StringReader { get; private set; }
		protected TypeSerializer<TElement> ElementTypeSerializer { get; private set; }
		protected TypeWriter<TElement> ElementWriter { get; private set; }
		protected TypeReader<TElement> ElementReader { get; private set; }

		protected List<TElement> ListReader(string value)
		{
			var elements = new List<TElement>();

			if (string.IsNullOrEmpty(value))
				return null;

			value = RemoveListChars(value);

			if (value[0] == MapStart)
			{
				int index = 0;
				int length = value.Length;
				do
				{
					string elementText = ReadMapValue(value, ref index);
					TElement element = (string.IsNullOrEmpty(elementText)) ? default(TElement) : ElementReader(elementText);

					elements.Add(element);
				} while (++index < length);
			}
			else
			{
				int length = value.Length;
				for (int index = 0; index < length; index++)
				{
					string elementText = ReadToChar(value, ref index, ItemSeparator);
					TElement element = (string.IsNullOrEmpty(elementText)) ? default(TElement) : ElementReader(elementText);

					elements.Add(element);
				}
			}

			return elements;
		}

		protected void ListWriter(IEnumerable<TElement> value, Action<string> output)
		{
			if (value == null)
				return;

			output(ListStartString);

			bool addSeparator = false;

			foreach (TElement obj in value)
			{
				if (addSeparator)
					output(ItemSeparatorString);
				else
					addSeparator = true;

				ElementWriter(obj, output);
			}

			output(ListEndString);
		}
	}
}