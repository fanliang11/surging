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
	using System.Diagnostics;

	public class ExceptionPolicy
	{
		private readonly Action<Action> _policy;

		public ExceptionPolicy(Action<Action> policy)
		{
			Guard.AgainstNull(policy, "policy");

			_policy = policy;
		}

		[DebuggerNonUserCode]
		public void Do(Action action)
		{
			_policy(action);
		}

		[DebuggerNonUserCode]
		public TResult Do<TResult>(Func<TResult> action)
		{
			TResult result = default(TResult);

			_policy(() => { result = action(); });

			return result;
		}

		public static PolicyBuilder<ExceptionHandler> InCaseOf<TException>()
			where TException : Exception
		{
			return PolicyBuilder.For<EventHandler>(ex => ex is TException);
		}

		public static PolicyBuilder<ExceptionHandler> InCaseOf<TException1,TException2>()
			where TException1 : Exception
			where TException2 : Exception
		{
			return PolicyBuilder.For<EventHandler>(ex => (ex is TException1) || (ex is TException2));
		}

		public static PolicyBuilder<ExceptionHandler> InCaseOf<TException1,TException2,TException3>()
			where TException1 : Exception
			where TException2 : Exception
			where TException3 : Exception
		{
			return PolicyBuilder.For<EventHandler>(ex => (ex is TException1) || (ex is TException2) || (ex is TException3));
		}
	}
}