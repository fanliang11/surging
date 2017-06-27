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
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using Collections;

	public class EventActionList<T> :
		IEnumerable<EventAction<T>>
		where T : StateMachine<T>
	{
		private readonly List<ActionItem> _actions = new List<ActionItem>();

		public IEnumerator<EventAction<T>> GetEnumerator()
		{
			foreach (ActionItem item in _actions)
			{
				yield return item.Action;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(Action<T> action, params ExceptionAction<T>[] exceptionActions)
		{
			_actions.Add(new ActionItem
				{
					Action = new LambdaAction<T>(action),
					ExceptionHandler = CreateExceptionHandlerForAction(exceptionActions),
				});
		}

		public void Add<TData>(Action<T, TData> action, params ExceptionAction<T>[] exceptionActions)
		{
			_actions.Add(new ActionItem
				{
					Action = new LambdaAction<T, TData>((x, e, d) => action(x, d)),
					ExceptionHandler = CreateExceptionHandlerForAction(exceptionActions),
				});
		}

		public void Add<TData>(Action<T, DataEvent<T, TData>, TData> action, params ExceptionAction<T>[] exceptionActions)
		{
			_actions.Add(new ActionItem
				{
					Action = new LambdaAction<T, TData>(action),
					ExceptionHandler = CreateExceptionHandlerForAction(exceptionActions),
				});
		}

		public void Add(Expression<Action<T>> expression, params ExceptionAction<T>[] exceptionActions)
		{
			_actions.Add(new ActionItem
				{
					Action = new ExpressionAction<T>(expression),
					ExceptionHandler = CreateExceptionHandlerForAction(exceptionActions),
				});
		}

		public void Add<TData>(Expression<Action<T, TData>> expression, params ExceptionAction<T>[] exceptionActions)
		{
			_actions.Add(new ActionItem
				{
					Action = new ExpressionAction<T, TData>(expression),
					ExceptionHandler = CreateExceptionHandlerForAction(exceptionActions),
				});
		}

		public void AddStateTransition(State state)
		{
			_actions.Add(new ActionItem
				{
					Action = new TransitionToAction<T>(state),
					ExceptionHandler = CreateExceptionHandlerForAction(),
				});
		}

		public void Execute(T stateMachine, Event @event, object parameter)
		{
			foreach (ActionItem actionItem in _actions)
			{
				try
				{
					actionItem.Action.Execute(stateMachine, @event, parameter);
				}
				catch (Exception ex)
				{
					actionItem.ExceptionHandler.HandleException(stateMachine, @event, parameter, ex);
					break;
				}
			}
		}

		public IEventActionExceptionHandler<T> CreateExceptionHandlerForAction(params ExceptionAction<T>[] exceptionActions)
		{
			return new ExceptionActionDictionary<T> {exceptionActions};
		}

		private class ActionItem
		{
			public EventAction<T> Action { get; set; }
			public IEventActionExceptionHandler<T> ExceptionHandler { get; set; }
		}
	}
}