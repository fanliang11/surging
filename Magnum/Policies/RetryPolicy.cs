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
namespace Magnum.Policies
{
	using System;
	using System.Collections.Generic;

	public static class RetryPolicy
	{
		public static ExceptionPolicy Retry(this PolicyBuilder<ExceptionHandler> builder, int retryCount)
		{
			Guard.AgainstNull(builder, "builder");
			Guard.GreaterThan(0, retryCount, "retryCount");

			return new ExceptionPolicy(action => ImplementPolicy(action, RetryImmediately(builder.Condition, retryCount)));
		}

		public static ExceptionPolicy Retry(this PolicyBuilder<ExceptionHandler> builder, int retryCount, Action<Exception, int> retryAction)
		{
			Guard.AgainstNull(builder, "builder");
			Guard.GreaterThan(0, retryCount, "retryCount");
			Guard.AgainstNull(retryAction, "retryAction");

			return new ExceptionPolicy(action => ImplementPolicy(action, RetryImmediately(builder.Condition, retryCount, retryAction)));
		}

		public static ExceptionPolicy Retry(this PolicyBuilder<ExceptionHandler> builder, Action<Exception> retryAction)
		{
			Guard.AgainstNull(builder, "builder");
			Guard.AgainstNull(retryAction, "retryAction");

			return new ExceptionPolicy(action => ImplementPolicy(action, RetryImmediately(builder.Condition, retryAction)));
		}

		public static ExceptionPolicy Retry(this PolicyBuilder<ExceptionHandler> builder, IEnumerable<TimeSpan> intervals, Action<Exception, TimeSpan> retryAction)
		{
			Guard.AgainstNull(builder, "builder");
			Guard.AgainstNull(intervals, "intervals");
			Guard.AgainstNull(retryAction, "retryAction");

			return new ExceptionPolicy(action => ImplementPolicy(action, RetryInterval(builder.Condition, intervals, retryAction)));
		}

		public static ExceptionPolicy Retry(this PolicyBuilder<ExceptionHandler> builder, IEnumerable<TimeSpan> intervals)
		{
			Guard.AgainstNull(builder, "builder");
			Guard.AgainstNull(intervals, "intervals");

			return new ExceptionPolicy(action => ImplementPolicy(action, RetryInterval(builder.Condition, intervals)));
		}

		private static void ImplementPolicy(Action action, Func<Exception, bool> isHandled)
		{
			while (true)
			{
				try
				{
					action();
					return;
				}
				catch (Exception ex)
				{
					if (!isHandled(ex))
						throw;
				}
			}
		}

		private static Func<Exception, bool> RetryImmediately(ExceptionHandler isHandled, int retryCount)
		{
			return RetryImmediately(isHandled, retryCount, (ex, c) => { });
		}

		private static Func<Exception, bool> RetryImmediately(ExceptionHandler isHandled, int retryCount, Action<Exception, int> retryAction)
		{
			int failureCount = 0;

			return x =>
				{
					failureCount++;

					if (!isHandled(x))
						return false;

					if (failureCount > retryCount)
						return false;

					retryAction(x, failureCount);

					return true;
				};
		}

		private static Func<Exception, bool> RetryImmediately(ExceptionHandler isHandled, Action<Exception> retryAction)
		{
			return x =>
				{
					if (!isHandled(x))
						return false;

					retryAction(x);

					return true;
				};
		}

		private static Func<Exception, bool> RetryInterval(ExceptionHandler isHandled, IEnumerable<TimeSpan> intervals)
		{
			return RetryInterval(isHandled, intervals, (ex, c) => { });
		}

		private static Func<Exception, bool> RetryInterval(ExceptionHandler isHandled, IEnumerable<TimeSpan> intervals, Action<Exception, TimeSpan> retryAction)
		{
			IEnumerator<TimeSpan> enumerator = intervals.GetEnumerator();

			return x =>
				{
					if (!isHandled(x))
						return false;

					if (!enumerator.MoveNext())
						return false;

					TimeSpan interval = enumerator.Current;

					retryAction(x, interval);

					ThreadUtil.Sleep(interval);

					return true;
				};
		}
	}
}