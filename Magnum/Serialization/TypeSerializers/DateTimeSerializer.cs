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
namespace Magnum.Serialization.TypeSerializers
{
	using System;
	using System.Globalization;
	using System.Xml;
	using Extensions;

	public class DateTimeSerializer :
		TypeSerializer<DateTime>
	{
		public const string DateFormat = "yyyy-MM-dd";
		public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
		public const string DateTimeMillisecondsFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
		public const string DateTimeShortMillisecondsFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

		public TypeReader<DateTime> GetReader()
		{
			return ParseShortestXsdDateTime;
		}

		public TypeWriter<DateTime> GetWriter()
		{
			return (value, output) => output(GetShortestDateTimeString(value));
		}

		private static string GetShortestDateTimeString(DateTime dateTime)
		{
			TimeSpan timeOfDay = dateTime.TimeOfDay;
			if (timeOfDay.Ticks == 0)
				return FormatDate(dateTime);

			if (timeOfDay.Milliseconds == 0)
				return FormatDateTime(dateTime.ToUniversalTime());

			return FormatDateTimeMs(dateTime.ToUniversalTime());
		}

		private static string FormatDateTimeMs(DateTime dt)
		{
			var chars = new char[24];
			Write4Chars(chars, 0, dt.Year);
			chars[4] = '-';
			Write2Chars(chars, 5, dt.Month);
			chars[7] = '-';
			Write2Chars(chars, 8, dt.Day);
			chars[10] = 'T';
			Write2Chars(chars, 11, dt.Hour);
			chars[13] = ':';
			Write2Chars(chars, 14, dt.Minute);
			chars[16] = ':';
			Write2Chars(chars, 17, dt.Second);
			chars[19] = '.';
			Write3Chars(chars, 20, dt.Millisecond);
			chars[23] = 'Z';

			return new string(chars);
		}

		private static string FormatDateTime(DateTime dt)
		{
			var chars = new char[20];
			Write4Chars(chars, 0, dt.Year);
			chars[4] = '-';
			Write2Chars(chars, 5, dt.Month);
			chars[7] = '-';
			Write2Chars(chars, 8, dt.Day);
			chars[10] = 'T';
			Write2Chars(chars, 11, dt.Hour);
			chars[13] = ':';
			Write2Chars(chars, 14, dt.Minute);
			chars[16] = ':';
			Write2Chars(chars, 17, dt.Second);
			chars[19] = 'Z';

			return new string(chars);
		}

		private static string FormatDate(DateTime dt)
		{
			var chars = new char[10];
			Write4Chars(chars, 0, dt.Year);
			chars[4] = '-';
			Write2Chars(chars, 5, dt.Month);
			chars[7] = '-';
			Write2Chars(chars, 8, dt.Day);

			return new string(chars);
		}

		private static void Write2Chars(char[] chars, int offset, int value)
		{
			chars[offset++] = (char) (value/10 + '0');
			chars[offset] = (char) (value%10 + '0');
		}

		private static void Write3Chars(char[] chars, int offset, int value)
		{
			chars[offset++] = (char)('0' + (value / 100));
			chars[offset++] = (char)('0' + ((value / 10) % 10));
			chars[offset] = (char)('0' + (value % 10));
		}

		private static void Write4Chars(char[] chars, int offset, int value)
		{
			chars[offset++] = (char)('0' + (value / 1000 % 10));
			chars[offset++] = (char)('0' + (value / 100 % 10));
			chars[offset++] = (char)('0' + ((value / 10) % 10));
			chars[offset] = (char)('0' + (value % 10));
		}

		private static DateTime ParseShortestXsdDateTime(string text)
		{
			if(text == null || text.Length == 0)
				return DateTime.MinValue;

			if (text.Length <= DateTimeMillisecondsFormat.Length || text.Length >= DateTimeShortMillisecondsFormat.Length)
				return XmlConvert.ToDateTime(text, XmlDateTimeSerializationMode.Utc);

			if (text.Length == DateTimeFormat.Length)
				return DateTime.ParseExact(text, DateTimeFormat, null, DateTimeStyles.AdjustToUniversal);

			return new DateTime(
				int.Parse(text.Substring(0, 4)),
				int.Parse(text.Substring(5, 2)),
				int.Parse(text.Substring(8, 2)),
				0, 0, 0,
				DateTimeKind.Utc);
		}
	}
}