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

	public interface ExceptionAction<T>
		where T : StateMachine<T>
	{
		Type ExceptionType { get; }
		void Execute(T instance, Event @event, object parameter, Exception exception);
	}

	public class ExceptionAction<T, TException> :
		EventActionBase<T>,
		ExceptionAction<T>
		where T : StateMachine<T>
		where TException : Exception
	{
		public ExceptionAction() :
			base(new DataEvent<T, TException>(typeof (TException).Name))
		{
		}

		public void Execute(T instance, Event @event, object parameter, Exception exception)
		{
			if (ParameterMeetsCondition(parameter))
			{
				Actions.Execute(instance, @event, parameter);
			}
		}

		public Type ExceptionType
		{
			get { return typeof (TException); }
		}

		public ExceptionAction<T, TException> Then(Action<T> action, params EventAction<T>[] exceptionActions)
		{
			Actions.Add(action);
			return this;
		}

		public ExceptionAction<T, TException> Then(Action<T, TException> action, params EventAction<T>[] exceptionActions)
		{
			Actions.Add(action);
			return this;
		}

		public ExceptionAction<T, TException> Then(Action<T, DataEvent<T, TException>, TException> action, params EventAction<T>[] exceptionActions)
		{
			Actions.Add(action);
			return this;
		}

		public ExceptionAction<T, TException> TransitionTo(State state)
		{
			Actions.AddStateTransition(state);
			return this;
		}

		/// <summary>
		/// Shortcut for TransitionTo(Completed)
		/// </summary>
		/// <returns></returns>
		public ExceptionAction<T, TException> Complete()
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