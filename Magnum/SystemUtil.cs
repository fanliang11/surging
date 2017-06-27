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
namespace Magnum
{
	using System;

	public static class SystemUtil
	{
		private static Func<DateTime> _nowProvider;
		private static Func<DateTime> _utcNowProvider;

		static SystemUtil()
		{
			Reset();
		}

		public static DateTime Now
		{
			get { return _nowProvider(); }
		}

		public static void SetNow(Func<DateTime> provider)
		{
			_nowProvider = provider;
		}

		public static DateTime UtcNow
		{
			get { return _utcNowProvider(); }
		}

		public static void SetUtcNow(Func<DateTime> provider)
		{
			_nowProvider = provider;
		}

		public static void Reset()
		{
			_nowProvider = () => DateTime.Now;
			_utcNowProvider = () => DateTime.UtcNow;
		}
	}
}