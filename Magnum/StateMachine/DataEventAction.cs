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

	public class DataEventAction<T, TData> :
		EventActionBase<T>
		where T : StateMachine<T>
	{
		private Func<TData, bool> _checkCondition = x => true;

		public DataEventAction(Event definedEvent)
			: base(definedEvent)
		{
		}

		public Expression<Func<TData, bool>> Condition { get; private set; }

		public DataEventAction<T, TData> Where(Expression<Func<TData, bool>> condition)
		{
			Condition = condition;

			_checkCondition = condition.Compile();

			return this;
		}

		public DataEventAction<T, TData> Then(Action<T> action, params ExceptionAction<T>[] exceptionActions)
		{
			Actions.Add(action, exceptionActions);
			return this;
		}

		public DataEventAction<T, TData> Then(Action<T, TData> action, params ExceptionAction<T>[] exceptionActions)
		{
			Actions.Add(action, exceptionActions);
			return this;
		}

		public DataEventAction<T, TData> Call(Expression<Action<T>> expression, params ExceptionAction<T>[] exceptionActions)
		{
			Actions.Add(expression, exceptionActions);
			return this;
		}

		public DataEventAction<T, TData> Call(Expression<Action<T, TData>> expression, params ExceptionAction<T>[] exceptionActions)
		{
			Actions.Add(expression, exceptionActions);
			return this;
		}

		public DataEventAction<T, TData> TransitionTo(State state)
		{
			Actions.AddStateTransition(state);
			return this;
		}

		/// <summary>
		/// Shortcut for TransitionTo(Completed)
		/// </summary>
		/// <returns></returns>
		public DataEventAction<T, TData> Complete()
		{
			Actions.AddStateTransition(StateMachine<T>.GetCompletedState());
			return this;
		}

		protected override bool ParameterMeetsCondition(object parameter)
		{
            if (!(parameter is TData))
                return false;
			var eventData = (TData) parameter;
			if (eventData == null)
				return false;

			return _checkCondition(eventData);
		}
	}
}