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


	public static class ExtensionsToAction
	{
		public static Func<TResult> Memoize<TResult>(this Func<TResult> self)
		{
			return Memoize(self, false);
		}

		public static Func<TResult> Memoize<TResult>(this Func<TResult> self, bool threadSafe)
		{
			Guard.AgainstNull(self, "self");

			TResult result = default(TResult);
			Exception exception = null;
			bool executed = false;

			Func<TResult> memoizedFunc = () =>
				{
					if (!executed)
					{
						try
						{
							result = self();
						}
						catch (Exception ex)
						{
							exception = ex;
							throw;
						}
						finally
						{
							executed = true;
						}
					}

					if (exception != null)
						throw exception;

					return result;
				};

			return threadSafe
			       	? memoizedFunc.Synchronize(() => !executed)
			       	: memoizedFunc;
		}

		public static Func<TResult> Synchronize<TResult>(this Func<TResult> self)
		{
			return self.Synchronize(() => true);
		}

		public static Func<TResult> Synchronize<TResult>(this Func<TResult> self, Func<bool> needsSynchronizationPredicate)
		{
			Guard.AgainstNull(self, "self");
			Guard.AgainstNull(needsSynchronizationPredicate, "needsSynchronizationPredicate");

			var lockObject = new object();

			return () =>
				{
					if (needsSynchronizationPredicate())
					{
						lock (lockObject)
						{
							return self();
						}
					}

					return self();
				};
		}
	}
}