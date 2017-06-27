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
namespace Magnum.StateMachine
{
	using System;
	using System.Linq.Expressions;

	public class BasicEventAction<T> :
		EventActionBase<T>
		where T : StateMachine<T>
	{
		public BasicEventAction(Event definedEvent)
			: base(definedEvent)
		{
		}

		public BasicEventAction<T> Then(Action<T> action, params ExceptionAction<T>[] exceptionActions)
		{
			Actions.Add(action, exceptionActions);
			return this;
		}

		public BasicEventAction<T> Then(Action<T, BasicEvent<T>> action, params ExceptionAction<T>[] exceptionActions)
		{
			Actions.Add(action, exceptionActions);
			return this;
		}

		public BasicEventAction<T> Call(Expression<Action<T>> expression, params ExceptionAction<T>[] exceptionActions)
		{
			Actions.Add(expression, exceptionActions);
			return this;
		}

		public BasicEventAction<T> TransitionTo(State state)
		{
			Actions.AddStateTransition(state);
			return this;
		}

		/// <summary>
		/// Shortcut for TransitionTo(Completed)
		/// </summary>
		/// <returns></returns>
		public BasicEventAction<T> Complete()
		{
			Actions.AddStateTransition(StateMachine<T>.GetCompletedState());
			return this;
		}

		protected override bool ParameterMeetsCondition(object parameter)
		{
			return true;
		}
	}
}