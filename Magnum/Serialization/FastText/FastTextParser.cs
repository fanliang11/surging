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
	using Extensions;

	public class FastTextParser
	{
		public const string DoubleQuoteString = "\"\"";
		public const string EmptyMap = "{}";
		public const char ItemSeparator = ',';
		public const string ItemSeparatorString = ",";
		public const char ListEnd = ']';
		public const string ListEndString = "]";
		public const char ListStart = '[';
		public const string ListStartString = "[";
		public const char MapEnd = '}';
		public const string MapEndString = "}";
		public const string MapNullValue = "\"\"";
		public const char MapSeparator = ':';
		public const string MapSeparatorString = ":";
		public const char MapStart = '{';
		public const string MapStartString = "{";
		public const char Quote = '"';
		public const string QuoteString = "\"";
		public static readonly char[] EscapeChars = new[] {Quote, ItemSeparator, MapStart, MapSeparator, MapEnd, ListStart, ListEnd};

		protected static string ReadToChar(string value, ref int index, char separator)
		{
			int start = index;
			int length = value.Length;

			if (value[start] != Quote)
			{
				index = value.IndexOf(separator, start);
				if (index == -1)
					index = length;

				return value.Substring(start, index - start);
			}

			while (++index < length)
			{
				if (value[index] == Quote
				    && (index + 1 >= length || value[index + 1] == separator))
				{
					index++;
					return value.Substring(start, index - start);
				}
			}

			throw new IndexOutOfRangeException("The ending quote character was not found.");
		}

		protected static string RemoveListChars(string value)
		{
			if (value == null || value.Length == 0)
				return null;

			return value[0] == ListStart ? value.Substring(1, value.Length - 2) : value;
		}

		protected static void ReadMap(string text, Action<string, string> keyValueCallback)
		{
			if (text == EmptyMap)
				return;

			int length = text.Length;
			for (int index = 1; index < length; index++)
			{
				string key = ReadMapKey(text, ref index);
				index++;

				string value = ReadMapValue(text, ref index);

				keyValueCallback(key, value);
			}
		}

		protected static string ReadMapKey(string value, ref int index)
		{
			return ReadToChar(value, ref index, MapSeparator);
//			int start = index;
//			while (value[++index] != MapSeparator)
//			{
//			}
//			return value.Substring(start, index - start);
		}

		protected static string ReadMapValue(string value, ref int index)
		{
			int start = index;
			int length = value.Length;
			if (index == length)
				return null;

			string result;

			char ch = value[index];
			if (ch == ItemSeparator || ch == MapEnd)
				return null;

			if (TryReadListValue(value, ref index, out result))
				return result;

			if (TryReadMapValue(value, ref index, out result))
				return result;

			if (TryReadQuotedValue(value, ref index, out result))
				return result;

			while (++index < length)
			{
				ch = value[index];

				if (ch == ItemSeparator || ch == MapEnd)
					break;
			}

			return value.Substring(start, index - start);
		}

		private static bool TryReadListValue(string value, ref int index, out string result)
		{
			result = null;

			char ch = value[index];
			if (ch != ListStart)
				return false;

			bool inQuote = false;
			int depth = 1;

			int start = index;
			int length = value.Length;
			while (++index < length && depth > 0)
			{
				ch = value[index];

				if (ch == Quote)
					inQuote = !inQuote;

				if (inQuote)
					continue;

				if (ch == ListStart)
					depth++;

				if (ch == ListEnd)
					depth--;
			}

			result = value.Substring(start, index - start);
			return true;
		}

		private static bool TryReadMapValue(string value, ref int index, out string result)
		{
			result = null;

			char ch = value[index];
			if (ch != MapStart)
				return false;

			bool inQuote = false;
			int depth = 1;

			int start = index;
			int length = value.Length;
			while (++index < length && depth > 0)
			{
				ch = value[index];

				if (ch == Quote)
					inQuote = !inQuote;

				if (inQuote)
					continue;

				if (ch == MapStart)
					depth++;

				if (ch == MapEnd)
					depth--;
			}

			result = value.Substring(start, index - start);
			return true;
		}

		private static bool TryReadQuotedValue(string value, ref int index, out string result)
		{
			result = null;

			if (value[index] != Quote)
				return false;

			int start = index;
			int length = value.Length;
			while (++index < length)
			{
				if (value[index] != Quote)
					continue;

				bool isDoubleQuote = index + 1 < length && value[index + 1] == Quote;

				++index; // skip quote/escaped quote
				if (!isDoubleQuote)
					break;
			}

			result = value.Substring(start, index - start);
			return true;
		}
	}
}