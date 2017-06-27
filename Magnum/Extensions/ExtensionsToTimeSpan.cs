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
namespace Magnum.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Text;


	public static class ExtensionsToTimeSpan
	{
		static TimeSpan _day = TimeSpan.FromDays(1);
		static TimeSpan _hour = TimeSpan.FromHours(1);
		static TimeSpan _month = TimeSpan.FromDays(30);
		static TimeSpan _year = TimeSpan.FromDays(365);

		/// <summary>
		/// Creates a TimeSpan for the specified number of weeks
		/// </summary>
		/// <param name="value">The number of weeks</param>
		/// <returns></returns>
		public static TimeSpan Weeks(this int value)
		{
			return TimeSpan.FromDays(value*7);
		}

		/// <summary>
		/// Creates a TimeSpan for the specified number of days
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan Days(this int value)
		{
			return TimeSpan.FromDays(value);
		}

		/// <summary>
		/// Creates a TimeSpan for the specified number of hours
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan Hours(this int value)
		{
			return TimeSpan.FromHours(value);
		}

		/// <summary>
		/// Creates a TimeSpan for the specified number of minutes
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan Minutes(this int value)
		{
			return TimeSpan.FromMinutes(value);
		}

		/// <summary>
		/// Creates a TimeSpan for the specified number of seconds
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan Seconds(this int value)
		{
			return TimeSpan.FromSeconds(value);
		}

		public static TimeSpan Seconds(this double value)
		{
			return TimeSpan.FromSeconds(value);
		}

		/// <summary>
		/// Creates a TimeSpan for the specified number of milliseconds
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan Milliseconds(this int value)
		{
			return TimeSpan.FromMilliseconds(value);
		}

		/// <summary>
		/// Returns an enumeration of the specified TimeSpan with the specified number of elements
		/// </summary>
		/// <param name="value">The TimeSpan to repeat</param>
		/// <param name="times">The number of times to repeat the TimeSpan</param>
		/// <returns>An enumeration of TimeSpan</returns>
		public static IEnumerable<TimeSpan> Repeat(this TimeSpan value, int times)
		{
			for (int i = 0; i < times; i++)
				yield return value;
		}

		public static string ToFriendlyString(this TimeSpan ts)
		{
			if (ts.Equals(_month))
				return "1M";
			if (ts.Equals(_year))
				return "1y";
			if (ts.Equals(_day))
				return "1d";
			if (ts.Equals(_hour))
				return "1h";

			var sb = new StringBuilder();

			int years = ts.Days/365;
			int months = (ts.Days%365)/30;
			int weeks = ((ts.Days%365)%30)/7;
			int days = (((ts.Days%365)%30)%7);

			var parts = new List<string>();

			if (years > 0)
				sb.Append(years).Append("y");

			if (months > 0)
				sb.Append(months).Append("M");

			if (weeks > 0)
				sb.Append(weeks).Append("w");

			if (days > 0)
				sb.Append(days).Append("d");

			if (ts.Hours > 0)
				sb.Append(ts.Hours).Append("h");
			if (ts.Minutes > 0)
				sb.Append(ts.Minutes).Append("m");
			if (ts.Seconds > 0)
				sb.Append(ts.Seconds).Append("s");
			if (ts.Milliseconds > 0)
				sb.Append(ts.Milliseconds).Append("ms");

			return sb.ToString();
		}
	}
}